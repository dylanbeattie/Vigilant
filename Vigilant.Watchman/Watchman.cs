using System;
using System.Linq;
using System.Timers;
using Castle.Windsor;
using log4net;
using Vigilant.Entities;

namespace Vigilant.Watchman {
	public class Watchman {
		private readonly IWindsorContainer container;
		private readonly ILog log;
		private Timer timer;

		public Watchman(IWindsorContainer container, ILog log, TimeSpan interval) {
			this.container = container;
			this.log = log;
			timer = new Timer(interval.TotalMilliseconds);
			timer.Elapsed += RunChecks;
			log.InfoFormat("Created Vigilant Watchman service - running checks every {0} milliseconds", interval.TotalMilliseconds);
		}

		void RunChecks(object sender, ElapsedEventArgs e) {
			log.Debug("RunChecks()");
			try {
				var checkRunner = container.Resolve<CheckRunner>();
				using (var db = container.Resolve<VigilantDatabase>()) {
					foreach (var response in checkRunner.RunChecks(db.Receptors)) db.AddToResponses(response);
					db.SaveChanges();
				}
			} catch (Exception ex) {
				log.Warn("Error in VigilantWatchman.RunChecks()", ex);
			}
		}

		public void Start() {
			timer.Start();
			log.Info("VigilantWatchman service started");
		}

		public void Stop() {
			timer.Stop();
			log.Info("VigilantWatchman service stopped");
		}
	}
}