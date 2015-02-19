namespace Vigilant.Watchman {
	public interface IHttpClient {
		string Retrieve(string ipAddress, string httpRequest);
	}
}