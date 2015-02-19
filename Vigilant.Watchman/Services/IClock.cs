using System;

namespace Vigilant.Watchman.Services {
	public interface IClock {
		DateTime UtcNow { get; }
	}
}