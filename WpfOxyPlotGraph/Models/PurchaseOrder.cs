using System;

namespace WpfOxyPlotGraph.Models
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Ordered, Received, Cancelled
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


