using FluentAssertions;
using NUnit.Framework;
using Vigilant.Watchman.Http;

namespace Vigilant.Watchman.StoryTests {
    [TestFixture]
    public class HttpsClientTests {
        [Test]
        public void Can_Get_Ssl_Spotlight_Homepage() {
            const string httpRequest = "GET / HTTP/1.1\r\nHost:www.spotlight.com\r\nAccept-Encoding:gzip";
            IHttpClient client = new HttpClient();
            var result = client.Retrieve("193.164.100.151", 443, httpRequest, true);
            result.Should().StartWith("HTTP/1.1 200 OK");
            result.Should().Contain("Spotlight");
        }

        [Test]
        public void Can_Get_Ssl_Google_Homepage() {
            const string httpRequest = "GET / HTTP/1.1\r\nHost:www.google.co.uk";
            IHttpClient client = new HttpClient();
            var result = client.Retrieve("173.194.67.94", 443, httpRequest, true);
            result.Should().StartWith("HTTP/1.1 200 OK");
            result.Should().Contain("Google Search");
        }
    }
}