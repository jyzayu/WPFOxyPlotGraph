using System;
using System.Text.Json.Serialization;

namespace WpfOxyPlotGraph.Commons.Audit
{
	public sealed class AuditEvent
	{
		public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
		public string User { get; set; } = string.Empty;
		public string Action { get; set; } = string.Empty;
		public int? PatientId { get; set; }
		public string? PatientName { get; set; }
		public string? Details { get; set; }
		public bool Success { get; set; } = true;
		public string? ErrorMessage { get; set; }

		[JsonIgnore]
		public bool IsPatientRelated => PatientId.HasValue;
	}
}


