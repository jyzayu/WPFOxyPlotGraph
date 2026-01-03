using System;

namespace WpfOxyPlotGraph.Models
{
	public class Encounter
	{
		public int Id { get; set; }
		public int PatientId { get; set; }
		public DateTime VisitAt { get; set; }
		public string Diagnosis { get; set; } = string.Empty;
		public string Notes { get; set; } = string.Empty;
	}
}


