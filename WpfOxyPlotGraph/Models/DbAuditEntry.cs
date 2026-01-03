namespace WpfOxyPlotGraph.Models
{
    public class DbAuditEntry
    {
        public int AuditId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public int? PrimaryKeyId { get; set; }
        public System.DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
    }
}


