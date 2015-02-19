using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vigilant.Entities;

namespace Vigilant.WebDashboard.Controllers {
	public class EndpointController : Controller {
		private VigilantDatabase db = new VigilantDatabase();

		//
		// GET: /Endpoint/

		public ActionResult Index() {
			return View(db.Endpoints.ToList());
		}

		//
		// GET: /Endpoint/Details/5

		public ActionResult Details(int id = 0) {
			Endpoint endpoint = db.Endpoints.Single(e => e.EndpointId == id);
			if (endpoint == null) {
				return HttpNotFound();
			}
			return View(endpoint);
		}

		//
		// GET: /Endpoint/Create

		public ActionResult Create() {
			return View();
		}

		//
		// POST: /Endpoint/Create

		[HttpPost]
		public ActionResult Create(Endpoint endpoint) {
			if (ModelState.IsValid) {
				db.Endpoints.AddObject(endpoint);
				db.SaveChanges();
				return RedirectToAction("Index");
			}

			return View(endpoint);
		}

		//
		// GET: /Endpoint/Edit/5

		public ActionResult Edit(int id = 0) {
			Endpoint endpoint = db.Endpoints.Single(e => e.EndpointId == id);
			if (endpoint == null) {
				return HttpNotFound();
			}
			return View(endpoint);
		}

		//
		// POST: /Endpoint/Edit/5

		[HttpPost]
		public ActionResult Edit(Endpoint endpoint) {
			if (ModelState.IsValid) {
				db.Endpoints.Attach(endpoint);
				db.ObjectStateManager.ChangeObjectState(endpoint, EntityState.Modified);
				db.SaveChanges();
				return RedirectToAction("Index");
			}
			return View(endpoint);
		}

		//
		// GET: /Endpoint/Delete/5

		public ActionResult Delete(int id = 0) {
			Endpoint endpoint = db.Endpoints.Single(e => e.EndpointId == id);
			if (endpoint == null) {
				return HttpNotFound();
			}
			return View(endpoint);
		}

		//
		// POST: /Endpoint/Delete/5

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id) {
			Endpoint endpoint = db.Endpoints.Single(e => e.EndpointId == id);
			db.Endpoints.DeleteObject(endpoint);
			db.SaveChanges();
			return RedirectToAction("Index");
		}

		protected override void Dispose(bool disposing) {
			db.Dispose();
			base.Dispose(disposing);
		}
	}
}