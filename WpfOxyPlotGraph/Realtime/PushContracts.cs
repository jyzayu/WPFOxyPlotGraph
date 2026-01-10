using System.Text.Json;

namespace WpfOxyPlotGraph.Realtime
{
    public class PushEnvelope
    {
        public string Type { get; set; } = string.Empty; // e.g., "MRI_COMPLETED"
        public string Doctor { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public long TimestampUnixMs { get; set; }

        public string ToJson() => JsonSerializer.Serialize(this);
        public static PushEnvelope? FromJson(string json)
            => JsonSerializer.Deserialize<PushEnvelope>(json);
    }
}


