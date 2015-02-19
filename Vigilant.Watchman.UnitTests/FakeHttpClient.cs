using System.Threading;
using Vigilant.Watchman.Http;

namespace Vigilant.Watchman.UnitTests {

    public class FakeHttpClient : IHttpClient {
        private readonly string response;
        private readonly int delayInMilliseconds;

        public FakeHttpClient(string response, int delayInMilliseconds) {
            this.response = response;
            this.delayInMilliseconds = delayInMilliseconds;
        }

        public string Retrieve(string ipAddress, int port, string httpRequest, bool useSsl) {
            Thread.Sleep(delayInMilliseconds);
            return (response);
        }

        public void Dispose() {
        }
    }
}
