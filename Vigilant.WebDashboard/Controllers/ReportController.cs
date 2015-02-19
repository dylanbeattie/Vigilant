using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vigilant.Entities;

namespace Vigilant.WebDashboard.Controllers {
	public class ReportController : Controller {

		private VigilantDatabase db = new VigilantDatabase();

		[HttpPost]
		public ActionResult CompareUrlChecks(int[] urlCheckIds, int interval) {
			var urlChecks = db.UrlChecks.Where(c => urlCheckIds.Contains(c.UrlCheckId));
			var threshold = DateTime.UtcNow.AddMinutes(-1 * interval);
			var responses = urlChecks.SelectMany(r => r.Receptors).SelectMany(r => r.Responses).Where(r => r.RequestSentAtUtc > threshold);
			var data = new Dictionary<string, Dictionary<string, int>>();
			foreach (var receptor in responses.Select(r => r.Receptor).Distinct()) {
				var endpoint = receptor.Endpoint.Nickname;
				if (!data.ContainsKey(endpoint)) data.Add(endpoint, new Dictionary<string, int>());
				if (data[endpoint].ContainsKey(receptor.UrlCheck.Name)) continue;
				data[endpoint].Add(receptor.UrlCheck.Name, Convert.ToInt32(receptor.Responses.Average(r => r.ResponseTimeInMilliseconds)));
			}
			return (View(data));
		}

	}
}
