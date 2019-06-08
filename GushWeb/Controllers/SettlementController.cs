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

namespace GushWeb.Controllers
{
    //[Authorize]
    public class SettlementController : BaseController
    {
        private GushDBContext db = new GushDBContext();
        readonly string SZprefix = "sz";
        readonly string SHprefix = "sh";

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

        // GET: Settlement/Details/5
        public ActionResult Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewData["stockCode"] = id;
            return View();
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
                                select new { g.Key, notes = g.OrderByDescending(d=>d.Date).FirstOrDefault() };
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
        public JsonResult GetKLine(string code)
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
            
            return Json(settlementList);
        }
    }
}
