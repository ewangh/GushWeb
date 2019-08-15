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
using Antlr.Runtime.Tree;
using GushLibrary.Models;
using GushWeb.ActionFilters;
using GushWeb.Models;
using GushWeb.Utility;
using PagedList;

namespace GushWeb.Controllers
{
    [AlarmnotesActionFilters]
    public class AlarmnotesController : BaseController
    {
        private GushDBContext db = new GushDBContext();
        const int pageSize = 30;
        // GET: Alarmnotes

        private Expression<Func<t_alarmnotes, bool>> getExpression(string date, string[] array = null)
        {
            Expression<Func<t_alarmnotes, bool>> expression = t => true;
            //expression = expression.And(d => d.Date.CompareTo(date) == 0 && !d.Name.ToLower().Contains("st") && d.Price < d.Closed * 1.097m && d.Time.CompareTo("09:32:03") < 0);
            expression = expression.And(d => d.Date.CompareTo(date) == 0);
            expression = expression.And(d => !d.Name.ToLower().Contains("st"));
            expression = expression.And(d => d.Price < d.Closed * 1.097m);
            expression = expression.And(d => d.Time.CompareTo("09:32:03") < 0);
            if (!array.IsNullOrEmpty())
            {
                expression=expression.And(p => !array.Contains(p.Code));
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

            var expression = getExpression(date);
            var pageData = db.AlarmNotesList.Where(expression).OrderBy(d => d.Time);
            return View(pageData);
        }

        [HttpPost]
        public async Task<ActionResult> IndexAsyn(FormCollection collection)
        {
            string codes = collection["codes"];
            //var pageData = db.AlarmNotesList.Where(d => d.Date == dt && !d.Name.ToLower().Contains("st") && d.Price < d.Closed * 1.097m && d.Time.CompareTo("09:32:03") < 0).OrderBy(d => d.Time).AsEnumerable();
            if (!Request.IsAjaxRequest() || codes.IsNullOrEmpty())
            {
                return PartialView("pviewIndex", new List<t_alarmnotes>());
            }
            string[] codeArray = codes.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
            var expression = getExpression(Today, codeArray);
            var pageData = await db.AlarmNotesList.Where(expression).OrderBy(d => d.Time).ToListAsync();

            return PartialView("pviewIndex", pageData);
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
    }
}
