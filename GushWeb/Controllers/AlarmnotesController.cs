using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Antlr.Runtime.Tree;
using GushLibrary.Models;
using GushWeb.ActionFilters;
using GushWeb.Models;
using GushWeb.Utility;
using Newtonsoft.Json.Linq;
using PagedList;

namespace GushWeb.Controllers
{
    [AlarmnotesActionFilters]
    public class AlarmnotesController : BaseController
    {
        private GushDBContext db = new GushDBContext();
        const int pageSize = 30;
        // GET: Alarmnotes
        private Expression<Func<t_alarmnotes, bool>> getExpression(string date, Notestate state, string[] array = null)
        {
            Expression<Func<t_alarmnotes, bool>> expression = t => true;
            //expression = expression.And(d => d.Date.CompareTo(date) == 0 && !d.Name.ToLower().Contains("st") && d.Price < d.Closed * 1.097m && d.Time.CompareTo("09:32:03") < 0);
            expression = expression.And(d => d.Date.CompareTo(date) == 0);
            expression = expression.And(d => d.Price < d.Closed * 1.097m);
            expression = expression.And(d => d.State == state);
            //expression = expression.And(d => d.Time.CompareTo("09:32:03") < 0);
            if (!array.IsNullOrEmpty())
            {
                expression = expression.And(p => !array.Contains(p.Code));
            }

            return expression;
        }

        public ActionResult Index(string date)
        {
            if (!String.IsNullOrEmpty(date))
            {
                var datePage = AlarmnotesSingleton.GetObj().GetDatePage(ref date);
                ViewBag.Prev = datePage.PrevDate;
                ViewBag.Current = datePage.CurrentDate;
                ViewBag.Next = datePage.NextDate;
            }
            else
            {
                date = Today;
                ViewBag.Current = Today;
            }

            var expression = getExpression(date, Notestate.Opn);
            var queue = db.AlarmNotesList.Where(expression).OrderBy(d => d.Time).ToList();
            var codes = queue.ConvertAll(d => d.Code);
            var stateDic = db.FoamList.Where(d => d.Date.CompareTo(date) < 0 && codes.Contains(d.Code)).GroupBy(d => d.Code)
                .Select(g => new { code = g.Key, queue = g }).ToDictionary(d => d.code, d => d.queue.OrderByDescending(p => p.Date).FirstOrDefault()?.State);
            var samples = GetSamples(date, date == Today);// date == Today ? OnsSamples(date) : SetSamplesByDate(date);

            foreach (var obj in queue)
            {
                if (samples.ContainsKey(obj.Code))
                {
                    obj.Num = samples[obj.Code];
                }
                if (stateDic.ContainsKey(obj.Code))
                {
                    obj.ForceState = stateDic[obj.Code];
                }
            }

            var pageData = from a in queue
                           orderby a.Num, a.Time
                           join f in db.FoamList.Where(d => d.Date.CompareTo(date) == 0) on new { a.Code, a.Date } equals new
                           { f.Code, f.Date } into temp
                           from t in temp.DefaultIfEmpty()
                           select a.ToAlarmnotes(t);

            if (User.Identity.IsAuthenticated)
            {
                return View("Index", pageData);
                //TODO:vue request
                //return View("IndexNew", pageData);
            }

            return View(pageData);
        }

        [HttpPost]
        public async Task<ActionResult> IndexAsyn(FormCollection collection)
        {
            string codes = collection["codes"] ?? string.Empty;
            //var pageData = db.AlarmNotesList.Where(d => d.Date == dt && !d.Name.ToLower().Contains("st") && d.Price < d.Closed * 1.097m && d.Time.CompareTo("09:32:03") < 0).OrderBy(d => d.Time).AsEnumerable();
            if (!Request.IsAjaxRequest())
            {
                return PartialView("pviewIndex", new List<t_alarmnotes>());
            }
            string[] codeArray = codes.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            var expression = getExpression(Today, Notestate.Opn, codeArray);
            var queue = await db.AlarmNotesList.Where(expression).OrderBy(d => d.Time).ToListAsync();
            var stateDic = await db.FoamList.Where(d => d.Date.CompareTo(Today) < 0 && codeArray.Contains(d.Code)).GroupBy(d => d.Code)
                .Select(g => new { code = g.Key, queue = g }).ToDictionaryAsync(d => d.code, d => d.queue.OrderByDescending(p => p.Date).FirstOrDefault()?.State);
            var samples = GetSamples(Today, true);

            foreach (var obj in queue)
            {
                if (samples.ContainsKey(obj.Code))
                {
                    obj.Num = samples[obj.Code];
                }
                if (stateDic.ContainsKey(obj.Code))
                {
                    obj.ForceState = stateDic[obj.Code];
                }
            }

            var pageData = from a in queue
                           orderby a.Num, a.Time
                           join f in db.FoamList.Where(d => d.Date.CompareTo(Today) == 0) on new { a.Code, a.Date } equals new
                           { f.Code, f.Date } into temp
                           from t in temp.DefaultIfEmpty()
                           select a.ToAlarmnotes(t);
            return PartialView("pviewIndex", pageData);
        }

        private ForceState? GetForceState(string code, string date)
        {
            return db.FoamList.Where(d => d.Code == code && d.Date.CompareTo(date) < 0).OrderByDescending(d => d.Date)
                .FirstOrDefault()?.State;
        }

        private decimal? GetCprice(string code, string date)
        {
            return db.SettlementList.Where(d => d.Code == code && d.Date.CompareTo(date) < 0).OrderByDescending(d => d.Date).FirstOrDefault()?.cPrice;
        }

        //public async Task<JsonResult> Get(string codes)
        //{
        //    string[] codeArray = codes?.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
        //    var expression = getExpression(Today, codeArray);
        //    var pageData = await db.AlarmNotesList.Where(expression).OrderBy(d => d.Time).ToListAsync();
        //    return Json(pageData, JsonRequestBehavior.AllowGet);
        //}

        //TODO:vue request
        public JsonResult GetAlarmnotes(string codes)
        {
            var list = new List<t_alarmnotes>();
            list.Add(new t_alarmnotes() { Code = "123", Name = "abc" });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        // GET: Alarmnotes/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_settlement obj = (from v in db.SettlementList
                                where v.Code == id
                                select v).Where(v => v.Date.CompareTo(Today) < 0).OrderByDescending(v => v.Date).FirstOrDefault();
            if (obj == null)
            {
                return HttpNotFound();
            }
            return View(obj);
        }

        // GET: Alarmnotes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Alarmnotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [NonAction, HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Code,Name,Opening,Closed,Price,Highest,Lower,Date,Time,Closing,Volume")] t_alarmnotes t_alarmnotes)
        {
            if (ModelState.IsValid)
            {
                t_alarmnotes.Id = Guid.NewGuid();
                db.AlarmNotesList.Add(t_alarmnotes);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(t_alarmnotes);
        }

        // GET: Alarmnotes/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_alarmnotes t_alarmnotes = db.AlarmNotesList.Find(id);
            if (t_alarmnotes == null)
            {
                return HttpNotFound();
            }
            return View(t_alarmnotes);
        }

        // POST: Alarmnotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [NonAction, HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Code,Name,Opening,Closed,Price,Highest,Lower,Date,Time,Closing,Volume")] t_alarmnotes t_alarmnotes)
        {
            if (ModelState.IsValid)
            {
                db.Entry(t_alarmnotes).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(t_alarmnotes);
        }

        // GET: Alarmnotes/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_alarmnotes t_alarmnotes = db.AlarmNotesList.Find(id);
            if (t_alarmnotes == null)
            {
                return HttpNotFound();
            }
            return View(t_alarmnotes);
        }

        // POST: Alarmnotes/Delete/5
        [NonAction]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            t_alarmnotes t_alarmnotes = db.AlarmNotesList.Find(id);
            db.AlarmNotesList.Remove(t_alarmnotes);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ProductList(int p = 1)
        {
            var pageData = db.AlarmNotesList.Where(d => d.Date == Today).OrderBy(d => d.Time);
            if (pageData.Count() < pageSize)
            {
                return View(pageData.ToList());
            }
            else
            {
                return View(pageData.ToPagedList(pageNumber: p, pageSize: pageSize));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        public Dictionary<string, int?> GetSamples(string date, bool isOns)
        {
            return isOns ? OnsSamples(date) : SetSamplesByDate(date);
        }

        private Dictionary<string, int?> OnsSamples(string date)
        {
            Dictionary<string, int?> samples = new Dictionary<string, int?>();

            var list = db.ChangesList.Where(d => (d.Date_x ?? d.Date_9).CompareTo(date) == 0)
                .OrderByDescending(d => d.Change_x);
            int? num = 0;
            decimal? prevChange = decimal.MaxValue;

            foreach (var obj in list)
            {
                if (samples.ContainsKey(obj.Code))
                {
                    continue;
                }

                if (obj.Change_x.HasValue && obj.Change_x < prevChange)
                {
                    prevChange = obj.Change_x;
                    num++;
                }
                samples.Add(obj.Code, num); //-1
            }

            return samples;
        }

        private Dictionary<string, int?> SetSamplesByDate(string date)
        {
            Dictionary<string, int?> samples;

            samples = db.ChangesList.GroupBy(d => d.Date_9)
                .Select(g => new { date = g.Key, queue = g }).Where(d => d.date.CompareTo(date) < 0)
                .OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_9);

            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_8).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_8);


            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_7).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_7);

            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_6).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_6);

            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_5).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_5);

            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_4).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_4);

            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_3).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_3);

            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_2).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_2);

            if (samples != null)
            {
                return samples;
            }

            samples = db.ChangesList.GroupBy(d => d.Date_1).Select(g => new { date = g.Key, queue = g })
                .Where(d => d.date.CompareTo(date) < 0).OrderByDescending(d => d.date).FirstOrDefault()?.queue
                .ToDictionary(pair => pair.Code, pair => pair.Num_1);

            if (samples != null)
            {
                return samples;
            }

            samples = new Dictionary<string, int?>();
            return samples;
        }
    }
}
