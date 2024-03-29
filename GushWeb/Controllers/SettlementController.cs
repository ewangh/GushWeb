﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using GushLibrary.Models;
using GushWeb.ActionFilters;
using GushWeb.Helpers;
using GushWeb.Models;
using GushWeb.Utility;
using PagedList;

namespace GushWeb.Controllers
{
    //[Authorize]
    public class SettlementController : BaseController
    {
        private GushDBContext db = new GushDBContext();
        private GushProcContext proc = new GushProcContext();
        const string SZprefix = "sz";
        const string SHprefix = "sh";
        private const int pageSize = 30;

        // GET: Settlement
        public ActionResult Index(string codes)
        {
            List<t_settlement> t_settlement = new List<t_settlement>();

            if (!String.IsNullOrWhiteSpace(codes))
            {
                string[] codeArray = codes.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
                if (codeArray.Length > 0)
                {
                    for (int i = 0; i < codeArray.Length; i++)
                    {
                        if (codeArray[i].StartsWith("00") || codeArray[i].StartsWith("300"))
                            codeArray[i] = SZprefix + codeArray[i];
                        else if (codeArray[i].StartsWith("600"))
                            codeArray[i] = SHprefix + codeArray[i];
                    }
                    var group = from p in db.SettlementList
                                where codeArray.Contains(p.Code)
                                group p by p.Code into g
                                select new { g.Key, notes = g.OrderByDescending(d => d.Date).FirstOrDefault() };
                    foreach (var obj in group)
                    {
                        t_settlement.Add(obj.notes);
                    }
                };
            }

            return View(t_settlement);
        }

        public ActionResult Index2(string date)
        {
            List<t_opennotes> t_opennotes = new List<t_opennotes>();

            if (date.IsDateTime())
            {
                t_opennotes = proc.ProcServer.ExecOpenPro(date).ToList();
            }

            return View(t_opennotes);
        }

        [AllowAnonymous]
        public ActionResult BreakThrough()
        {
            IEnumerable<t_delta> t_delta = new List<t_delta>();
            var pd = t_delta.ToPagedList(1, pageSize * 10);
            return View(pd);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult BreakThroughAsync(string ptype, string begin, string end, int index = 1)
        {
            IEnumerable<t_delta> t_delta = new List<t_delta>();

            if (!String.IsNullOrWhiteSpace(begin))
            {
                if (String.IsNullOrWhiteSpace(end))
                {
                    end = db.SettlementList.Max(d => d.Date);
                }

                ViewData["begin"] = begin;
                ViewData["end"] = end;
                ViewData["ptype"] = ptype;

                var q1 = db.SettlementList.Where(d => d.Date.CompareTo(begin) == 0 && d.Code.StartsWith(ptype));
                var q2 = db.SettlementList.Where(d => d.Date.CompareTo(end) == 0 && d.Code.StartsWith(ptype));

                t_delta = from s in q1
                          join f in q2 on s.Code equals f.Code into temp
                          from t in temp.Where(d => d.Price >= s.Highest && d.Volume > s.Volume)
                          select new t_delta() { Code = t.Code, Name = t.Name, Delta = s.Highest.HasValue ? t.Price / s.Highest : null, Change = t.Closed.HasValue ? (t.Price / t.Closed - 1) * 100 : null, BeginDate = s.Date, EndDate = t.Date };
            }
            return PartialView("pview_through", t_delta.OrderBy(d => d.Delta).ThenBy(d => d.Change).ToPagedList(index, pageSize * 10));
        }

        public ActionResult Finace()
        {
            IEnumerable<t_finace> t_finaces = new List<t_finace>();
            var pd = t_finaces.ToPagedList(1, pageSize);
            return View(pd);
        }

        [HttpPost]
        public ActionResult IndexAsyn(string codes)
        {
            List<t_settlement> t_settlement = new List<t_settlement>();
            if (!String.IsNullOrWhiteSpace(codes))
            {
                string[] codeArray = codes.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
                if (codeArray.Length > 0)
                {
                    for (int i = 0; i < codeArray.Length; i++)
                    {
                        if (codeArray[i].StartsWith("00") || codeArray[i].StartsWith("300"))
                            codeArray[i] = SZprefix + codeArray[i];
                        else if (codeArray[i].StartsWith("600"))
                            codeArray[i] = SHprefix + codeArray[i];
                    }
                    var group = from p in db.SettlementList
                                where codeArray.Contains(p.Code)
                                group p by p.Code into g
                                select new { g.Key, notes = g.OrderByDescending(d => d.Date).FirstOrDefault() };
                    foreach (var obj in group)
                    {
                        t_settlement.Add(obj.notes);
                    }
                };
            }
            return PartialView("pviewIndex", t_settlement);
        }

        [HttpPost]
        public ActionResult Index2Asyn(string date)
        {
            IEnumerable<t_opennotes> t_opennotes = new List<t_opennotes>();

            if (date.IsDateTime())
            {
                t_opennotes = proc.ProcServer.ExecOpenPro(date);
            }

            return PartialView("pviewIndexBydate", t_opennotes);
        }

        [HttpPost]
        public ActionResult FinaceAsyn(float rate, int porder, string date1, string date2, string ptype, int index = 1)
        {
            ViewData["rate"] = rate;
            ViewData["porder"] = porder;
            ViewData["date1"] = date1;
            ViewData["date2"] = date2;
            ViewData["ptype"] = ptype;

            IEnumerable<t_finace> t_finaces = new List<t_finace>();

            if (date1 != null && date1.IsDateTime())
            {
                if (!String.IsNullOrEmpty(ptype))
                {
                    t_finaces = GetFinacesWithPlateType(porder, date1, date2, ptype, index);
                }
                else
                {
                    t_finaces = GetFinaces(rate, porder, date1, date2, index);
                }
            }
            return PartialView("pview_finace", t_finaces);
        }

        private IEnumerable<t_finace> GetFinaces(float rate, int order, string date1, string date2, int index)
        {
            var t_finaces = proc.ProcServer.ExecFinanceProc(rate, order, date1, date2.IsDateTime() ? date2 : date1);
            var pd = t_finaces.ToList().ToPagedList(index, pageSize);
            return pd;
        }

        private IEnumerable<t_finace> GetFinacesWithPlateType(int order, string date1, string date2, string ptype, int index)
        {
            string key = "Codes";
            float rate = -100;
            var codeArray = INIhelp.GetValue(key, ptype).Split(',');
            var t_finaces = proc.ProcServer.ExecFinanceProc(rate, order, date1, date2.IsDateTime() ? date2 : date1);
            var pd = t_finaces.Where(d => codeArray.Contains(d.Code)).ToList().ToPagedList(index, pageSize);
            return pd;
        }

        // GET: Settlement/Details/5
        public ActionResult Details(string id, string date)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewData["stockCode"] = id;
            ViewData["stockDate"] = date;
            return View();
        }

        public ActionResult Changes()
        {
            var changesList = new List<t_change>();
            return View(changesList);
        }

        public async Task<ActionResult> Board()
        {
            var changesList = db.ChangesList.Where(d => !String.IsNullOrEmpty(d.Date_x));

            if (changesList.Any())
            {
                changesList = changesList.Where(d => d.Limit_9.HasValue && d.Limit_9 != 0);
            }
            else
            {
                changesList = db.ChangesList.Where(d => d.Limit_8.HasValue && d.Limit_8 != 0);
            }

            return View(changesList);
        }

        [AlarmnotesActionFilters]
        public async Task<ActionResult> Catapult()
        {
            string date = Today;
            ViewData["date"] = date;
            ViewData["daytype"] = -1;
            IEnumerable<t_catapult> catapultList = await proc.ProcServer.ExecCatapultProc(date, (int)ViewData["daytype"]);
            var pd = catapultList.ToPagedList(1, 100);
            return View(pd);
        }

        [AlarmnotesActionFilters]
        [HttpPost]
        public async Task<ActionResult> CatapultAsyn(string date, int daytype, int col = 0, int index = 1)
        {
            ViewData["date"] = date;
            ViewData["daytype"] = daytype;
            ViewData["col"] = col;
            IEnumerable<t_catapult> t_catapults = new List<t_catapult>();

            if (date.IsDateTime())
            {
                t_catapults = await proc.ProcServer.ExecCatapultProc(date, daytype);

                switch (col)
                {
                    case 1:
                        t_catapults = t_catapults.OrderBy(d => d.Rank);
                        break;
                    case 2:
                        t_catapults = t_catapults.OrderByDescending(d => d.Rank);
                        break;
                    case 3:
                        t_catapults = t_catapults.OrderBy(d => d.Ltotal);
                        break;
                    case 4:
                        t_catapults = t_catapults.OrderByDescending(d => d.Ltotal);
                        break;
                    case 5:
                        t_catapults = t_catapults.OrderBy(d => d.NextPrice / d.Price).ThenBy(d => d.Price / d.Closed);
                        break;
                    case 6:
                        t_catapults = t_catapults.OrderByDescending(d => d.NextPrice / d.Price).ThenBy(d => d.Price / d.Closed);
                        break;
                    case 7:
                        t_catapults = t_catapults.OrderBy(d => d.Price / d.Closed);
                        break;
                    case 8:
                        t_catapults = t_catapults.OrderByDescending(d => d.Price / d.Closed);
                        break;
                    default:
                        t_catapults = t_catapults.OrderByDescending(d => d.NextOpen / d.Price).ThenBy(d => d.Price / d.Closed);
                        break;
                }
            }

            var pd = t_catapults.ToPagedList(index, 100);
            return PartialView("pview_catapult", pd);
        }

        // GET: Settlement/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Settlement/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Opening,Closed,Highest,Lower,Price,Volume,bPrice,cPrice,bVolume,Date,Time")] t_settlement t_settlement)
        {
            if (ModelState.IsValid)
            {
                t_settlement.Id = Guid.NewGuid();
                db.SettlementList.Add(t_settlement);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(t_settlement);
        }

        // GET: Settlement/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            t_settlement t_settlement = db.SettlementList.Find(id);

            if (t_settlement == null)
            {
                return HttpNotFound();
            }

            return View(t_settlement);
        }

        // POST: Settlement/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Opening,Closed,Highest,Lower,Price,Volume,bPrice,cPrice,bVolume,Date,Time")] t_settlement t_settlement)
        {
            if (ModelState.IsValid)
            {
                db.Entry(t_settlement).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(t_settlement);
        }

        // GET: Settlement/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_settlement t_settlement = db.SettlementList.Find(id);
            if (t_settlement == null)
            {
                return HttpNotFound();
            }
            return View(t_settlement);
        }

        // POST: Settlement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            t_settlement t_settlement = db.SettlementList.Find(id);
            db.SettlementList.Remove(t_settlement);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [OutputCache(NoStore = true, Duration = 0)]
        [HttpPost]
        public JsonResult GetData(string codes)
        {
            List<t_settlement> t_settlement = new List<t_settlement>();
            if (!String.IsNullOrWhiteSpace(codes))
            {
                string[] codeArray = codes.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
                if (codeArray.Length > 0)
                {
                    for (int i = 0; i < codeArray.Length; i++)
                    {
                        if (codeArray[i].StartsWith("00") || codeArray[i].StartsWith("300"))
                            codeArray[i] = SZprefix + codeArray[i];
                        else if (codeArray[i].StartsWith("600"))
                            codeArray[i] = SHprefix + codeArray[i];
                    }
                    var group = from p in db.SettlementList
                                where codeArray.Contains(p.Code)
                                group p by p.Code into g
                                select new { g.Key, notes = g.OrderByDescending(d => d.Date).FirstOrDefault() };
                    foreach (var obj in group)
                    {
                        t_settlement.Add(obj.notes);
                    }
                    //var obj = db.SettlementList.Where(v => codeArray.Contains(v.Code)).OrderByDescending(v=>v.Date).FirstOrDefault();
                    //if (obj != null)
                    //    t_settlement.Add(obj);
                };
            }
            return Json(t_settlement);
        }
        [HttpPost]
        public JsonResult GetKLine(string code, string date)
        {
            List<t_settlement> settlementList = new List<t_settlement>();
            if (!String.IsNullOrWhiteSpace(code))
            {
                code = code.Trim();
                if (code.StartsWith("00") || code.StartsWith("300"))
                    code = SZprefix + code;
                else if (code.StartsWith("600"))
                    code = SHprefix + code;
                settlementList = db.SettlementList.Where(d => d.Code == code).OrderBy(d => d.Date).ToList();
            }

            var CoordDate = date.IsNullOrEmpty() ? Today : date;
            var ZeroDate = DateTime.Parse(CoordDate).AddMonths(-1).ToYYYYMMDD();

            return Json(settlementList.Select(d => new
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Date = d.Date,
                Opening = d.Opening,
                Closed = d.Closed,
                Highest = d.Highest,
                Lower = d.Lower,
                Price = d.Price,
                Volume = d.Volume,
                bPrice = d.bPrice,
                cPrice = d.cPrice,
                dPrice = d.dPrice,
                ePrice = d.ePrice,
                fPrice = d.fPrice,
                gPrice = d.gPrice,
                Time = d.Time,
                Isst = d.IsLimit,
                CoordDate = CoordDate,
                ZeroDate = ZeroDate,
            }));
        }

        public IEnumerable<string> GetPlateTypes()
        {
            return INIhelp.GetValue("PlateType", "plateType").Split(',').AsEnumerable();
        }

        public IEnumerable<string> GetPlateCodes(string pkey)
        {
            return INIhelp.GetValue("Codes", pkey).Split(',').AsEnumerable();
        }

        [HttpPost]
        public async Task<JsonResult> GetChanges(string ptype)
        {
            IEnumerable<t_change> changesList = new List<t_change>();
            Dictionary<string, int> dic = new Dictionary<string, int>();

            if (String.IsNullOrEmpty(ptype))
            {
                return Json(changesList);
            }

            if (int.TryParse(ptype, out int _page))
            {
                int _size = 10;
                int _skip = _page * _size;

                changesList = await (from v in db.ChangesList
                                     where v.Date_x == db.ChangesList.Max(dx => dx.Date_x)
                                     select v)
                    .OrderByDescending(v => v.Change_x - v.Change_9)
                    .ThenByDescending(v => v.Change_9 - v.Change_8)
                    .ThenByDescending(v => v.Change_8 - v.Change_7)
                    .ThenByDescending(v => v.Change_7 - v.Change_6)
                    .ThenByDescending(v => v.Change_6 - v.Change_5)
                    .ThenByDescending(v => v.Change_5 - v.Change_4)
                    .ThenByDescending(v => v.Change_4 - v.Change_3)
                    .ThenByDescending(v => v.Change_3 - v.Change_2)
                    .ThenByDescending(v => v.Change_2 - v.Change_1)
                    .ThenByDescending(v => v.Change_1)
                    .Take(_skip + _size).Skip(_skip).ToArrayAsync();
            }
            else
            {
                string key = "Codes";
                var codeArray = INIhelp.GetValue(key, ptype).Split(',');

                changesList = await (from v in db.ChangesList
                                     where v.Date_x == db.ChangesList.Max(dx => dx.Date_x)
                                     select v).Where(v => codeArray.Contains(v.Code)).OrderByDescending(v => v.Change_x - v.Change_9)
                    .OrderByDescending(v => v.Change_x - v.Change_9)
                    .ThenByDescending(v => v.Change_9 - v.Change_8)
                    .ThenByDescending(v => v.Change_8 - v.Change_7)
                    .ThenByDescending(v => v.Change_7 - v.Change_6)
                    .ThenByDescending(v => v.Change_6 - v.Change_5)
                    .ThenByDescending(v => v.Change_5 - v.Change_4)
                    .ThenByDescending(v => v.Change_4 - v.Change_3)
                    .ThenByDescending(v => v.Change_3 - v.Change_2)
                    .ThenByDescending(v => v.Change_2 - v.Change_1)
                    .ThenByDescending(v => v.Change_1).ToArrayAsync();

                dic = TotalSamples(codeArray);
            }

            var pd = changesList.Select(d => new
            {
                name = String.Format("{0}({1})", d.Name, (dic.ContainsKey(d.Code) ? dic[d.Code].ToString() : "")),
                type = "line",
                axis = new string[]
                {
                    d.Date_1, d.Date_2, d.Date_3, d.Date_4, d.Date_5, d.Date_6, d.Date_7, d.Date_8, d.Date_9, d.Date_x
                },
                data = new decimal?[]
                {
                    d.Change_1, d.Change_2, d.Change_3, d.Change_4, d.Change_5, d.Change_6, d.Change_7, d.Change_8,
                    d.Change_9, d.Change_x
                }
            });

            return Json(pd);
        }

        private Dictionary<string, int> TotalSamples(string[] codes)
        {
            return db.FoamList.Where(d => codes.Contains(d.Code)).GroupBy(d => d.Date)
                .Select(g => new { date = g.Key, queue = g }).OrderByDescending(d => d.date).FirstOrDefault()?.queue.OrderByDescending(d => d.Total)
                .Select((item, index) => new { Key = item.Code, Num = index }).ToDictionary(d => d.Key, d => d.Num);
        }

        [HttpPost]
        public JsonResult GetRises(string ptype)
        {
            List<Rise> rises = new List<Rise>();
            var plateArray = GetPlateTypes();
            int top = 6;

            foreach (var pkey in plateArray)
            {
                var codeArray = GetPlateCodes(pkey);
                {
                    var sum = (from v in db.ChangesList.GroupBy(d => d.Date_9).Select(g => (new { date = g.Key, queue = g })).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                               where codeArray.Contains(v.Code)
                               select v).OrderByDescending(d => String.IsNullOrEmpty(d.Date_x) ? d.Change_9 - d.Change_8 : d.Change_x - d.Change_9).Take(top).Sum(d => String.IsNullOrEmpty(d.Date_x) ? d.Change_9 - d.Change_8 : d.Change_x - d.Change_9);

                    var riseObj = new Rise(pkey, pkey, sum) { IsCheck = pkey == ptype };
                    rises.Add(riseObj);
                }
            }

            return Json(rises.OrderByDescending(d => d.Change));
        }

        [Route("hello")]
        [HttpGet]
        public JsonResult JsRises()
        {
            List<Rise> rises = new List<Rise>();

            var plateArray = GetPlateTypes();
            int top = 6;

            foreach (var pkey in plateArray)
            {
                var codeArray = GetPlateCodes(pkey);
                var list = (from v in db.ChangesList.GroupBy(d => d.Date_9)
                        .Select(g => (new { date = g.Key, queue = g })).OrderByDescending(d => d.date)
                        .FirstOrDefault()?.queue
                            where codeArray.Contains(v.Code)
                            select v).OrderByDescending(d => String.IsNullOrEmpty(d.Date_x) ? d.Change_9 - d.Change_8 : d.Change_x - d.Change_9).Take(top);
                var sum = list.Sum(d => String.IsNullOrEmpty(d.Date_x) ? d.Change_9 - d.Change_8 : d.Change_x - d.Change_9);
                var dic = TotalSamples(codeArray.ToArray());
                var children = new List<Stock>();
                foreach (var obj in list)
                {
                    children.Add(new Stock()
                    {
                        Code = obj.Code,
                        Name = obj.Name,
                        NumTotal = dic.ContainsKey(obj.Code) ? dic[obj.Code] : new Nullable<int>(),
                        Change = String.IsNullOrEmpty(obj.Date_x) ? Math.Round((obj.Change_9 / obj.Change_8 - 1).Value * 100, 2) : Math.Round((obj.Change_x / obj.Change_9 - 1).Value * 100, 2),
                        Date = obj.Date_x ?? obj.Date_9
                    });
                }

                if (children.Any(d =>
                    d.NumTotal == 0 || d.NumTotal == 1 || d.NumTotal == 2) && children.Max(d => d.Change > 5) && children.OrderBy(d => d.NumTotal).FirstOrDefault()?.Change > 1)
                {
                    var riseObj = new Rise(pkey, pkey, sum) { Length = codeArray.Count(), Stocks = children };
                    rises.Add(riseObj);
                }
            }

            return Json(rises.OrderByDescending(d => d.Change), JsonRequestBehavior.AllowGet);
        }

        [Route("rises")]
        [HttpGet]
        public String RiseCodes(string id)
        {
            var plateArray = GetPlateTypes();
            StringBuilder codes = new StringBuilder();

            foreach (var pkey in plateArray)
            {
                if (String.IsNullOrEmpty(id))
                {
                    codes.AppendLine(pkey + "=");
                    var codeArray = GetPlateCodes(pkey);
                    var dic = TotalSamples(codeArray.ToArray());
                    codes.AppendLine(String.Join(",", dic.OrderBy(d => d.Value).ToDictionary(d => d.Key).Keys));
                }
                else if (pkey == id)
                {
                    codes.AppendLine(pkey + "=");
                    var codeArray = GetPlateCodes(pkey);
                    var dic = TotalSamples(codeArray.ToArray());
                    codes.AppendLine(String.Join(",", dic.OrderBy(d => d.Value).ToDictionary(d => d.Key).Keys));

                    break;
                }
            }

            return codes.ToString();
        }
    }
}
