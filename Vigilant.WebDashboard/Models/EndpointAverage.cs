using Vigilant.Entities;

namespace Vigilant.WebDashboard.Models {
	public class EndpointAverage {
		public Endpoint Endpoint { get; set; }
		public int AverageResponseTime { get; set; }
	}
}