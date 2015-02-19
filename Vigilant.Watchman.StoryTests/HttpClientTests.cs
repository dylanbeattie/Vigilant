using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Vigilant.Watchman.Http;

namespace Vigilant.Watchman.StoryTests {
    [TestFixture]
    public class HttpClientTests {
        [Test]
        public void Can_Get_Google_Homepage() {
            const string httpRequest = "GET / HTTP/1.1\r\nHost:www.google.co.uk\r\n";
            IHttpClient client = new HttpClient();
            var result = client.Retrieve("173.194.67.94", 80, httpRequest, false);
            result.Should().StartWith("HTTP/1.1 200 OK");
            result.Should().Contain("Google Search");
        }

        [Test]
        public void Can_Get_Spotlight_Homepage() {
            const string httpRequest = "GET / HTTP/1.1\r\nHost:www.spotlight.com\r\n";
            IHttpClient client = new HttpClient();
            var result = client.Retrieve("193.164.100.151", 80, httpRequest, false);
            result.Should().StartWith("HTTP/1.1 200 OK");
            result.Should().Contain("Spotlight");
        }

        [Test]
        public void Can_Get_Gzip_Encoded_Spotlight_Homepage() {
            const string httpRequest = "GET / HTTP/1.1\r\nHost:www.spotlight.com\r\nAccept-Encoding:gzip";
            IHttpClient client = new HttpClient();
            var result = client.Retrieve("193.164.100.151", 80, httpRequest, false);
            result.Should().StartWith("HTTP/1.1 200 OK");
            result.Should().Contain("Spotlight");
        }

        [Test]
        public void Can_Get_Bbc_Homepage() {
            const string httpRequest = "GET / HTTP/1.1\r\nHost:www.bbc.co.uk\r\n";
            IHttpClient client = new HttpClient();
            var result = client.Retrieve("212.58.244.67", 80, httpRequest, false);
            result.Should().StartWith("HTTP/1.1 200 OK");
            result.Should().Contain("The BBC is not responsible for the content of external sites.");
        }
    }
}