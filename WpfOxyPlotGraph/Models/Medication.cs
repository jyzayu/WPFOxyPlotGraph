using System;

namespace WpfOxyPlotGraph.Models
{
    public class Medication
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int MinimumStock { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}


