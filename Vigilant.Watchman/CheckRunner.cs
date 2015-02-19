using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Castle.Windsor;
using log4net;
using Vigilant.Entities;
using Vigilant.Watchman.Http;
using Vigilant.Watchman.Services;

namespace Vigilant.Watchman {
	public class CheckRunner {
		private readonly IWindsorContainer container;
		private readonly ILog log;
		private readonly IClock clock;

		public CheckRunner(IWindsorContainer container, ILog log, IClock clock) {
			this.container = container;
			this.log = log;
			this.clock = clock;
		}

		private static List<int> runningReceptorIds = new List<int>();

		public IEnumerable<Response> RunChecks(IEnumerable<Receptor> receptors) {

			foreach (var receptor in receptors) {
				if (runningReceptorIds.Contains(receptor.ReceptorId)) {
					log.Debug(receptor + " - SKIPPING (already running)");
				} else if (!receptor.ShouldRunAtUtc(clock.UtcNow)) {
					log.Debug(receptor + " - SKIPPING (not due yet)");
				} else {
					yield return (RunCheck(receptor));
				}
			}
		}

		private static string RetrieveResponse(IHttpClient httpClient, Receptor receptor) {
			var ipAddress = receptor.Endpoint.IpAddress;
			var port = receptor.UrlCheck.Port;
			var httpRequest = receptor.UrlCheck.RawHttpRequest;
			var useSsl = receptor.UrlCheck.UseSsl;
			return (httpClient.Retrieve(ipAddress, port, httpRequest, useSsl));
		}

		public Response RunCheck(Receptor receptor) {
			var clock = container.Resolve<IClock>();
			using (var httpClient = container.Resolve<IHttpClient>()) {
				var sw = new Stopwatch();
				var response = new Response { Receptor = receptor, RequestSentAtUtc = clock.UtcNow };
				var responseOk = false;
				sw.Start();
				try {
					var httpResponse = RetrieveResponse(httpClient, receptor);
					responseOk = ((receptor.UrlCheck.ExpectedResponse != null) && httpResponse.Contains(receptor.UrlCheck.ExpectedResponse));
				} catch (Exception ex) {
					response.IsCritical = true;
					response.Failure = new Failure { ExceptionMessage = ex.Message, StackTrace = ex.StackTrace };
				}
				var responseTime = Convert.ToInt32(sw.ElapsedMilliseconds);
				response.ResponseTimeInMilliseconds = responseTime;
				response.IsCritical = ((!responseOk) || (responseTime > receptor.UrlCheck.CriticalThresholdInMilliseconds));
				response.IsUnstable = (responseTime > receptor.UrlCheck.UnstableThresholdInMilliseconds);
				receptor.RegisterResponse(response);
				log.Debug(response);
				return (response);
			}
		}
	}
}