using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GushWeb.Models;

namespace GushWeb.Controllers
{
    public class OwnController : BaseController
    {
        private WebDBContext db = new WebDBContext();

        // GET: Own
        public ActionResult Index()
        {
            db.Owns.Add(new OwnModel()
            {
                Id = Guid.NewGuid(),
                name = "111",
                sex = 1,
            });
            return View(db.Owns.ToList());
        }

        // GET: Own/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OwnModel ownModel = db.Owns.Find(id);
            if (ownModel == null)
            {
                return HttpNotFound();
            }
            return View(ownModel);
        }

        // GET: Own/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Own/Create
        [HttpPost]

        [ValidateAntiForgeryToken]
        public ActionResult Create(OwnModel ownModel)
        {
            if (ModelState.IsValid)
            {
                ownModel.Id = Guid.NewGuid();
                db.Owns.Add(ownModel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(ownModel);
        }

        // GET: Own/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OwnModel ownModel = db.Owns.Find(id);
            if (ownModel == null)
            {
                return HttpNotFound();
            }
            return View(ownModel);
        }

        // POST: Own/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(OwnModel ownModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(ownModel).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(ownModel);
        //}

        [HttpPost]
        public ActionResult Edit(FormCollection form)
        {
            OwnModel ownModel = new OwnModel();
            if (!TryUpdateModel<OwnModel>(ownModel))
            {
                return View();
            }
            if (ModelState.IsValid)
            {
                db.Entry(ownModel).State = EntityState.Modified;
                //db.SaveChanges();
                //return RedirectToAction("Index");
            }
            if (form["btn1"] != null)
            {
                return View();
            }
            else if (form["btn2"] != null)
            {
                return View();
            }
            return View();
        }

        // GET: Own/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            OwnModel ownModel = db.Owns.Find(id);
            if (ownModel == null)
            {
                return HttpNotFound();
            }
            return View(ownModel);
        }

        // POST: Own/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            OwnModel ownModel = db.Owns.Find(id);
            db.Owns.Remove(ownModel);
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

        public ActionResult Test01()
        {
            return View();
        }

        public ActionResult Test02()
        {
            return View();
        }

        private static string _code = "";
        public ActionResult Test03(string code)
        {
            ViewData.Model = _code;
            _code = code;
            ViewBag.LastError = "alert('ok')";
            return View();
        }
    }
}
