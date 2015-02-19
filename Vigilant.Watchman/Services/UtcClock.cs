using System;

namespace Vigilant.Watchman.Services {
	public class UtcClock : IClock {
		public DateTime UtcNow {
			get { return (DateTime.UtcNow); }
		}
	}
}