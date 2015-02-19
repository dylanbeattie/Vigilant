using System;
using System.IO;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using log4net;
using log4net.Config;
using Topshelf;
using Topshelf.Dashboard;
using Topshelf.HostConfigurators;
using Topshelf.ServiceConfigurators;
using Vigilant.Entities;
using Vigilant.Watchman.Http;
using Vigilant.Watchman.Services;

namespace Vigilant.Watchman {
	public class Program {
		private static IWindsorContainer container;
		private static ILog log;
		private static Watchman CreateWatchman(IWindsorContainer container, ILog log) {
			var interval = TimeSpan.FromMilliseconds(2000);
			return (new Watchman(container, log, interval));
		}

		private static void Main(string[] args) {
			XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));
			log = LogManager.GetLogger("Vigilant");
			container = new WindsorContainer();
			container.Register(
				Component.For<VigilantDatabase>().ImplementedBy<VigilantDatabase>().LifeStyle.Transient,
				Component.For<IHttpClient>().ImplementedBy<HttpClient>().LifeStyle.Transient,
				Component.For<IClock>().ImplementedBy<UtcClock>().LifeStyle.Singleton,
				Component.For<CheckRunner>().ImplementedBy<CheckRunner>().LifeStyle.Transient,
				Component.For<ILog>().Instance(log),
				Component.For<IWindsorContainer>().Instance(container)
			);
			log.Info("Running Main() method from Vigilant Watchman");
			var host = HostFactory.New(ConfigureServiceHost);
			host.Run();
			Console.WriteLine("OK, press a key...");
			Console.ReadKey(false);
		}

		private static void ConfigureServiceHost(HostConfigurator config) {
			config.EnableDashboard();
			config.RunAsNetworkService();
			config.SetDisplayName("Vigilant Watchman");
			config.SetServiceName("VigilantWatchman");
			config.Service<Watchman>(ConfigureWatchmanService);
		}

		private static void ConfigureWatchmanService(ServiceConfigurator<Watchman> service) {
			service.SetServiceName("VigilantWatchman");
			service.ConstructUsing(() => CreateWatchman(container, log));
			service.WhenStarted(watchman => watchman.Start());
			service.WhenStopped(watchman => watchman.Stop());
		}
	}
}