using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Vigilant.Watchman {
	public class HttpClient : HttpClientBase, IHttpClient {
		public string Retrieve(string ipAddress, string httpRequest) {
			var endpoint = CreateEndpoint(ipAddress, 80);
			using (var socket = ConnectSocket(endpoint)) {
				using (var stream = new NetworkStream(socket)) {
					return (ReadStream(httpRequest, stream));
				}
			}
		}
	}

	public class HttpsClient : HttpClientBase, IHttpClient {
		public string Retrieve(string ipAddress, string httpRequest) {
			var endpoint = CreateEndpoint(ipAddress, 443);
			using (var socket = ConnectSocket(endpoint)) {
				using (var stream = new SslStream(new NetworkStream(socket), false, ValidateCertificate)) {
					stream.AuthenticateAsClient(ipAddress);
					return (ReadStream(httpRequest, stream));
				}
			}
		}
		
		private static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
			return (true);
		}
	}
}