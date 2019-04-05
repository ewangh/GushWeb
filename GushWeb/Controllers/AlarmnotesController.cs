using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GushLibrary.Models;
using GushWeb.Models;
using GushWeb.Utility;
using PagedList;

namespace GushWeb.Controllers
{
    public class AlarmnotesController : BaseController
    {
        private GushDBContext db = new GushDBContext();
        readonly int pageSize = 30;
        readonly string dt = DateTime.Now.AddDays(-92).ToString("yyyy-MM-dd");
        // GET: Alarmnotes
        public ActionResult Index(string codes)
        {
            var pageData = db.AlarmNotesList.Where(d => d.Date == dt && d.Price<d.Closed*1.097m && d.Time.CompareTo("09:32:03") < 0).OrderBy(d => d.Time);
            ViewBag.Codes = string.Join(",", pageData.ToList().ConvertAll(d => d.Code).ToArray());
            if (Request.IsAjaxRequest() && !codes.IsNullOrEmpty())
            {
                string[] codeArray = codes.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
                return PartialView("pviewIndex", pageData.Where(p => !codeArray.Contains(p.Code)));
            }
            return View(pageData);
        }

        //[HttpPost]
        //public ActionResult IndexAsyn(string codes)
        //{
        //    var pageData = db.AlarmNotesList.Where(d => d.Date == dt && d.Price < d.Closed * 1.097m && d.Time.CompareTo("09:32:03") < 0).OrderBy(d => d.Time);
        //    ViewBag.Codes = string.Join(",",pageData.ToList().ConvertAll(d => d.Code).ToArray());
        //    if (Request.IsAjaxRequest() && !codes.IsNullOrEmpty())
        //    {
        //        string[] codeArray = codes.Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries);
        //        return PartialView("pviewIndex", pageData.Where(p => !codeArray.Contains(p.Code)));
        //    }
        //    return View(pageData);
        //}

        // GET: Alarmnotes/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            t_settlement obj = (from v in db.SettlementList
                                where v.Code == id
                                select v).Where(v => v.Date.CompareTo(dt) < 0).OrderByDescending(v => v.Date).FirstOrDefault();
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
            var pageData = db.AlarmNotesList.Where(d => d.Date == dt).OrderBy(d => d.Time);
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
