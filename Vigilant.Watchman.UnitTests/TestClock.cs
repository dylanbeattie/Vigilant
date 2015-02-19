using System;
using Vigilant.Watchman.Services;

namespace Vigilant.Watchman.UnitTests {
    public class TestClock : IClock {
        private readonly DateTime fixedUtcDateTime;

        public TestClock(DateTime fixedUtcDateTime) {
            this.fixedUtcDateTime = fixedUtcDateTime;
        }

        public DateTime UtcNow {
            get { return (fixedUtcDateTime); }
        }
    }
}