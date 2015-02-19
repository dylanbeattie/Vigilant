using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FluentAssertions;
using log4net;
using Moq;
using NUnit.Framework;
using Vigilant.Entities;
using Vigilant.Watchman.Http;
using Vigilant.Watchman.Services;

namespace Vigilant.Watchman.UnitTests {
	[TestFixture]
	public class UrlCheckTests {
		private IWindsorContainer container;
		private IClock clock;
		private Mock<ILog> mockLog;

		[SetUp]
		public void SetUp() {
			container = new WindsorContainer();
			clock = new TestClock(new DateTime(2012, 7, 19, 12, 56, 32));
			mockLog = new Mock<ILog>();
			container.Register(
				Component.For<IClock>().Instance(clock),
				Component.For<IWindsorContainer>().Instance(container),
				Component.For<ILog>().Instance(mockLog.Object),
				Component.For<CheckRunner>().ImplementedBy<CheckRunner>().LifeStyle.Transient
			);
		}

		[Test]
		[TestCase(0)]
		[TestCase(100)]
		[TestCase(200)]
		public void UrlCheckResponseTimes(int latency) {
			var http = new FakeHttpClient("HTTP/1.1 200 OK", latency);
			container.Register(Component.For<IHttpClient>().Instance(http));
			var checkRunner = container.Resolve<CheckRunner>();
			var receptor = new Receptor { Endpoint = new Endpoint(), UrlCheck = new UrlCheck() };
			var response = checkRunner.RunCheck(receptor);
			response.ResponseTimeInMilliseconds.Should().BeInRange(latency - 10, latency + 10);
		}

		[Test]
		[TestCase(0)]
		[TestCase(100)]
		[TestCase(200)]
		public void UrlCheckCriticalTimeResponses(int latency) {
			var http = new FakeHttpClient("HTTP/1.1 200 OK", latency + 100);
			container.Register(Component.For<IHttpClient>().Instance(http));
			var checkRunner = container.Resolve<CheckRunner>();
			var urlCheck = new UrlCheck() { CriticalThresholdInMilliseconds = latency };
			var receptor = new Receptor { Endpoint = new Endpoint(), UrlCheck = urlCheck };
			var response = checkRunner.RunCheck(receptor);
			response.IsCritical.Should().Be(true);
		}

		[Test]
		[TestCase(0)]
		[TestCase(100)]
		[TestCase(200)]
		public void UrlCheckUnstableTimeResponses(int latency) {
			var http = new FakeHttpClient("HTTP/1.1 200 OK", latency + 100);
			container.Register(Component.For<IHttpClient>().Instance(http));
			var checkRunner = container.Resolve<CheckRunner>();
			var urlCheck = new UrlCheck() { UnstableThresholdInMilliseconds = latency };
			var receptor = new Receptor { Endpoint = new Endpoint(), UrlCheck = urlCheck };
			var response = checkRunner.RunCheck(receptor);
			response.IsUnstable.Should().Be(true);
		}

		[Test]
		[TestCase(0)]
		[TestCase(100)]
		[TestCase(200)]
		public void UrlCheck_Registers_Instability_With_Receptor(int latency) {
			var http = new FakeHttpClient("HTTP/1.1 200 OK", latency + 100);
			container.Register(Component.For<IHttpClient>().Instance(http));
			var checkRunner = container.Resolve<CheckRunner>();
			var urlCheck = new UrlCheck() { UnstableThresholdInMilliseconds = latency };
			var receptor = new Receptor { Endpoint = new Endpoint(), UrlCheck = urlCheck };
			checkRunner.RunCheck(receptor);
			receptor.LastCheckWasUnstable.Should().BeTrue();
		}

		[Test]
		[TestCase(0)]
		[TestCase(100)]
		[TestCase(200)]
		public void UrlCheckUnstableContentResponses(int latency) {
			var http = new FakeHttpClient("HTTP/1.1 500 Internal Server Error", latency + 100);
			container.Register(Component.For<IHttpClient>().Instance(http));
			var checkRunner = container.Resolve<CheckRunner>();
			var urlCheck = new UrlCheck() { UnstableThresholdInMilliseconds = latency, ExpectedResponse = "HTTP/1.1 200 OK" };
			var endpoint = new Endpoint() { };
			var receptor = new Receptor { Endpoint = endpoint, UrlCheck = urlCheck };
			var response = checkRunner.RunCheck(receptor);
			response.IsCritical.Should().Be(true);
		}

		[Test]
		public void UrlCheckExceptionResponses() {
			var httpMock = new Mock<IHttpClient>();
			httpMock
				.Setup(http => http.Retrieve(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Callback<string, int, string, bool>((ipAddress, port, httpRequest, useSsl) => {
					throw (new Exception("Dummy Exception"));
				});
			container.Register(Component.For<IHttpClient>().Instance(httpMock.Object));
			var checkRunner = container.Resolve<CheckRunner>();
			var urlCheck = new UrlCheck() { };
			var endpoint = new Endpoint() { };
			var receptor = new Receptor { Endpoint = endpoint, UrlCheck = urlCheck };
			var response = checkRunner.RunCheck(receptor);
			response.IsCritical.Should().Be(true);
		}

		[Test]
		public void UrlCheck_With_Exception_Includes_Message_In_Response() {
			var httpMock = new Mock<IHttpClient>();
			httpMock
				.Setup(http => http.Retrieve(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Callback<string, int, string, bool>((ipAddress, port, httpRequest, useSsl) => {
					throw (new Exception("Dummy Exception"));
				});
			container.Register(Component.For<IHttpClient>().Instance(httpMock.Object));
			var checkRunner = container.Resolve<CheckRunner>();
			var urlCheck = new UrlCheck() { };
			var endpoint = new Endpoint() { };
			var receptor = new Receptor { Endpoint = endpoint, UrlCheck = urlCheck };
			var response = checkRunner.RunCheck(receptor);
			response.Failure.Should().NotBeNull();
			response.Failure.ExceptionMessage.Should().Be("Dummy Exception");
		}

		[Test]
		public void UrlCheck_With_Exception_Includes_StackTrace_In_Response() {
			var httpMock = new Mock<IHttpClient>();
			httpMock
				.Setup(http => http.Retrieve(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Callback<string, int, string, bool>((ipAddress, port, httpRequest, useSsl) => {
					throw (new Exception("Dummy Exception"));
				});
			container.Register(Component.For<IHttpClient>().Instance(httpMock.Object));
			var checkRunner = container.Resolve<CheckRunner>();
			var receptor = new Receptor { Endpoint = new Endpoint(), UrlCheck = new UrlCheck() };
			var response = checkRunner.RunCheck(receptor);
			response.Failure.Should().NotBeNull();
		}

		[Test]
		public void UrlCheck_With_Exception_Sets_REceptor_To_Critical() {
			var httpMock = new Mock<IHttpClient>();
			httpMock
				.Setup(http => http.Retrieve(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
				.Callback<string, int, string, bool>((ipAddress, port, httpRequest, useSsl) => {
					throw (new Exception("Dummy Exception"));
				});
			container.Register(Component.For<IHttpClient>().Instance(httpMock.Object));
			var checkRunner = container.Resolve<CheckRunner>();
			var receptor = new Receptor { Endpoint = new Endpoint(), UrlCheck = new UrlCheck() };
			checkRunner.RunCheck(receptor);
			receptor.LastCheckWasCritical.Should().BeTrue();
		}

		[Test]
		public void UrlCheck_Logs_RequestedAt_DateTime_Correctly() {
			var http = new FakeHttpClient("HTTP/1.1 200 OK", 0);
			container.Register(Component.For<IHttpClient>().Instance(http));
			var checkRunner = container.Resolve<CheckRunner>();
			var urlCheck = new UrlCheck() { };
			var endpoint = new Endpoint() { };
			var receptor = new Receptor { Endpoint = endpoint, UrlCheck = urlCheck };
			var response = checkRunner.RunCheck(receptor);
			response.RequestSentAtUtc.Should().Be(clock.UtcNow);
		}

		[Test]
		public void UrlCheck_Updates_Receptor_LastRequestedAtUtc_DateTime_Correctly() {
			var http = new FakeHttpClient("HTTP/1.1 200 OK", 0);
			container.Register(Component.For<IHttpClient>().Instance(http));
			var checkRunner = container.Resolve<CheckRunner>();
			var receptor = new Receptor { Endpoint = new Endpoint(), UrlCheck = new UrlCheck() };
			checkRunner.RunCheck(receptor);
			receptor.LastTestedAtUtc.Should().Be(clock.UtcNow);
		}
	}
}
