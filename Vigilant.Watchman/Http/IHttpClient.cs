using System;

namespace Vigilant.Watchman.Http {
	public interface IHttpClient : IDisposable {
		string Retrieve(string ipAddress, int port, string httpRequest, bool useSsl);
	}
}