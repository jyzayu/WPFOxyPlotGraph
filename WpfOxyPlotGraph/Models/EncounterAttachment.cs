using System;

namespace WpfOxyPlotGraph.Models
{
	public class EncounterAttachment
	{
		public string Id { get; set; } = string.Empty; // hash-based id
		public int PatientId { get; set; }
		public int EncounterId { get; set; }
		public string OriginalFileName { get; set; } = string.Empty;
		public string StoredFileName { get; set; } = string.Empty; // <hash>.<ext>
		public string ContentType { get; set; } = string.Empty;
		public long FileSizeBytes { get; set; }
		public DateTime CreatedAt { get; set; }
	}
}


