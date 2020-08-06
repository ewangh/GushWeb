using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using GushLibrary.Models;
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
            List<string> rises = new List<string>();

            var List = proc.ProcServer.ExecChangeProc();
            string plateKey = "PlateType";
            string plateValue = "plateType";
            string CodeKey = "Codes";

            var plateArray = INIhelp.GetValue(plateKey, plateValue).Split(',');
            foreach (var pkey in plateArray)
            {
                var codeArray = INIhelp.GetValue(CodeKey, pkey).Split(',');
                var sum = List.Where(d => codeArray.Contains(d.Code)).Sum(d=>d.Change_x-d.Change_9);

                if (sum > 1)
                {
                    rises.Add(pkey);
                }
            }

            ViewData["Rises"] = String.Join(",", rises);
            return View(changesList);
        }

        public ActionResult Catapult()
        {
            string date = DateTime.Now.ToYYYYMMDD();
            ViewData["date"] = date;
            var catapultList = proc.ProcServer.ExecCatapultProc(date);
            var pd = catapultList.ToPagedList(1, 100);

            return View(pd);
        }

        [HttpPost]
        public ActionResult CatapultAsyn(string date, int index = 1)
        {
            ViewData["date"] = date;
            IEnumerable<t_catapult> t_catapults = new List<t_catapult>();

            if (date.IsDateTime())
            {
                t_catapults = proc.ProcServer.ExecCatapultProc(date);
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
                Isst = d.Isst,
                CoordDate = CoordDate,
                ZeroDate = ZeroDate,
            }));
        }

        [HttpPost]
        public JsonResult GetChanges(string ptype)
        {
            IEnumerable<t_change> changesList = new List<t_change>();

            if (String.IsNullOrEmpty(ptype))
            {
                return Json(changesList);
            }

            var List = proc.ProcServer.ExecChangeProc();

            if (int.TryParse(ptype, out int _page))
            {
                int _size = 10;
                int _skip = _page * _size;
                changesList = List.Take(_skip + _size).Skip(_skip);
            }
            else
            {
                string key = "Codes";
                var codeArray = INIhelp.GetValue(key, ptype).Split(',');

                changesList = List.Where(d => codeArray.Contains(d.Code));
            }


            var pd = changesList.Select(d => new
            {
                name = d.Name,
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
    }
}
