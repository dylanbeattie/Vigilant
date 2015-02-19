using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vigilant.Entities;

namespace Vigilant.WebDashboard.Controllers
{
    public class ReceptorController : Controller
    {
        private VigilantDatabase db = new VigilantDatabase();

        //
        // GET: /Receptor/

        public ActionResult Index()
        {
            var receptors = db.Receptors.Include("Endpoint").Include("UrlCheck");
            return View(receptors.ToList());
        }

        //
        // GET: /Receptor/Details/5

        public ActionResult Details(int id = 0)
        {
            Receptor receptor = db.Receptors.Single(r => r.ReceptorId == id);
            if (receptor == null)
            {
                return HttpNotFound();
            }
            return View(receptor);
        }

        //
        // GET: /Receptor/Create

        public ActionResult Create()
        {
            ViewBag.EndpointId = new SelectList(db.Endpoints, "EndpointId", "Nickname");
            ViewBag.UrlCheckId = new SelectList(db.UrlChecks, "UrlCheckId", "Name");
            return View();
        }

        //
        // POST: /Receptor/Create

        [HttpPost]
        public ActionResult Create(Receptor receptor)
        {
            if (ModelState.IsValid)
            {
                db.Receptors.AddObject(receptor);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.EndpointId = new SelectList(db.Endpoints, "EndpointId", "Nickname", receptor.EndpointId);
            ViewBag.UrlCheckId = new SelectList(db.UrlChecks, "UrlCheckId", "Name", receptor.UrlCheckId);
            return View(receptor);
        }

        //
        // GET: /Receptor/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Receptor receptor = db.Receptors.Single(r => r.ReceptorId == id);
            if (receptor == null)
            {
                return HttpNotFound();
            }
            ViewBag.EndpointId = new SelectList(db.Endpoints, "EndpointId", "Nickname", receptor.EndpointId);
            ViewBag.UrlCheckId = new SelectList(db.UrlChecks, "UrlCheckId", "Name", receptor.UrlCheckId);
            return View(receptor);
        }

        //
        // POST: /Receptor/Edit/5

        [HttpPost]
        public ActionResult Edit(Receptor receptor)
        {
            if (ModelState.IsValid)
            {
                db.Receptors.Attach(receptor);
                db.ObjectStateManager.ChangeObjectState(receptor, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.EndpointId = new SelectList(db.Endpoints, "EndpointId", "Nickname", receptor.EndpointId);
            ViewBag.UrlCheckId = new SelectList(db.UrlChecks, "UrlCheckId", "Name", receptor.UrlCheckId);
            return View(receptor);
        }

        //
        // GET: /Receptor/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Receptor receptor = db.Receptors.Single(r => r.ReceptorId == id);
            if (receptor == null)
            {
                return HttpNotFound();
            }
            return View(receptor);
        }

        //
        // POST: /Receptor/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Receptor receptor = db.Receptors.Single(r => r.ReceptorId == id);
            db.Receptors.DeleteObject(receptor);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}