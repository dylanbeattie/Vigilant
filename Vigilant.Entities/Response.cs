using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vigilant.Entities {
	public partial class Response {
		public override string ToString() {
			if (Failure != null) return (Receptor + " is CRITICAL (Failure: " + Failure.ExceptionMessage.Truncate(32) + " after " + ResponseTimeInMilliseconds + "ms)");
			if (IsCritical) return (Receptor + " is CRITICAL (" + ResponseTimeInMilliseconds + "ms)");
			if (IsUnstable) return (Receptor + " is UNSTABLE (" + ResponseTimeInMilliseconds + "ms)");
			return (Receptor + " is OK (" + ResponseTimeInMilliseconds + "ms)");
		}
	}


	public partial class Receptor {
		public bool ShouldRunAtUtc(DateTime utcNow) {
			if (this.LastTestedAtUtc.HasValue) {
				return (this.LastTestedAtUtc.Value + this.Frequency < utcNow);
			}
			return (true);
		}

		public override string ToString() {
			return (UrlCheck.Name + " on " + Endpoint.Nickname);
		}

		public void RegisterResponse(Response response) {
			LastCheckWasCritical = response.IsCritical;
			LastCheckWasUnstable = response.IsUnstable;
			LastResponseTimeInMilliseconds = response.ResponseTimeInMilliseconds;
			LastTestedAtUtc = response.RequestSentAtUtc;
		}
	}

	public static class StringExtensions {
		public static string Truncate(this string s, int maximumLength) {
			if (String.IsNullOrEmpty(s)) return (s);
			return s.Length <= maximumLength ? (s) : (s.Substring(0, maximumLength));
		}
	}
}
