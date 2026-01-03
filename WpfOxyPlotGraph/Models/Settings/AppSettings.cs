using System.Collections.Generic;

namespace WpfOxyPlotGraph.Models.Settings
{
	public class HospitalInfo
	{
		public string Name { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string Director { get; set; } = string.Empty;
	}

	public class AppSettings
	{
		public HospitalInfo Hospital { get; set; } = new HospitalInfo();
		public string DocumentTemplatesPath { get; set; } = string.Empty;
		public Dictionary<string, List<string>> CodeCategories { get; set; } = new Dictionary<string, List<string>>();
		public string DefaultPrinter { get; set; } = string.Empty;
	}
}


