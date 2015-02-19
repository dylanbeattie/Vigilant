using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vigilant.Entities;
using Vigilant.WebDashboard.Models;

namespace Vigilant.WebDashboard.Controllers
{
    public class UrlCheckController : Controller
    {
        private VigilantDatabase db = new VigilantDatabase();

        //
        // GET: /UrlCheck/

        public ActionResult Index()
        {
            return View(db.UrlChecks.ToList());
        }

		public ActionResult Recent(int id) {
			var urlcheck = db.UrlChecks.Single(u => u.UrlCheckId == id);
			var responses = urlcheck.Receptors.SelectMany(r => r.Responses.Where(response => response.RequestSentAtUtc > DateTime.UtcNow.AddMinutes(-15)));
			var groups = responses.GroupBy(r => r.Receptor.Endpoint);
			var results = groups.Select(g => new EndpointAverage { Endpoint = g.Key, AverageResponseTime = Convert.ToInt32(g.Average(r => r.ResponseTimeInMilliseconds)) });
			return(View(results));
		}


    	//
        // GET: /UrlCheck/Details/5

        public ActionResult Details(int id = 0)
        {
            UrlCheck urlcheck = db.UrlChecks.Single(u => u.UrlCheckId == id);
            if (urlcheck == null)
            {
                return HttpNotFound();
            }
            return View(urlcheck);
        }

        //
        // GET: /UrlCheck/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /UrlCheck/Create

        [HttpPost]
        public ActionResult Create(UrlCheck urlcheck)
        {
            if (ModelState.IsValid)
            {
                db.UrlChecks.AddObject(urlcheck);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(urlcheck);
        }

        //
        // GET: /UrlCheck/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UrlCheck urlcheck = db.UrlChecks.Single(u => u.UrlCheckId == id);
            if (urlcheck == null)
            {
                return HttpNotFound();
            }
            return View(urlcheck);
        }

        //
        // POST: /UrlCheck/Edit/5

        [HttpPost]
        public ActionResult Edit(UrlCheck urlcheck)
        {
            if (ModelState.IsValid)
            {
                db.UrlChecks.Attach(urlcheck);
                db.ObjectStateManager.ChangeObjectState(urlcheck, EntityState.Modified);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(urlcheck);
        }

        //
        // GET: /UrlCheck/Delete/5

        public ActionResult Delete(int id = 0)
        {
            UrlCheck urlcheck = db.UrlChecks.Single(u => u.UrlCheckId == id);
            if (urlcheck == null)
            {
                return HttpNotFound();
            }
            return View(urlcheck);
        }

        //
        // POST: /UrlCheck/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            UrlCheck urlcheck = db.UrlChecks.Single(u => u.UrlCheckId == id);
            db.UrlChecks.DeleteObject(urlcheck);
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