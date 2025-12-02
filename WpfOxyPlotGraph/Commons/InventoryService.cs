using System;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Repositories;

namespace WpfOxyPlotGraph.Commons
{
    public class InventoryService
    {
        private readonly MedicationRepository _medicationRepository = new MedicationRepository();
        private readonly InventoryRepository _inventoryRepository = new InventoryRepository();
        private readonly PurchaseOrderRepository _purchaseOrderRepository = new PurchaseOrderRepository();
		private readonly EmailNotificationService _emailNotificationService = new EmailNotificationService();

        public InventoryService()
        {
            // Ensure required tables exist
            _medicationRepository.EnsureTables();
            _inventoryRepository.EnsureTables();
            _purchaseOrderRepository.EnsureTables();
        }

        public int GetCurrentStock(int medicationId)
        {
            return _inventoryRepository.GetQuantity(medicationId);
        }

        public CheckResult CheckThresholds(int medicationId)
        {
            var med = _medicationRepository.GetById(medicationId);
            if (med == null) return new CheckResult(false, false, 0, 0, 0);
            var current = _inventoryRepository.GetQuantity(medicationId);
            bool atOrBelowMin = current <= med.MinimumStock;
            bool belowReorder = current < med.ReorderPoint;
            return new CheckResult(atOrBelowMin, belowReorder, current, med.ReorderPoint, med.MinimumStock);
        }

        public ConsumeResult ConsumeOnPrescription(int medicationId, int quantity)
        {
            if (quantity <= 0) return new ConsumeResult(false, "처방 수량은 1 이상이어야 합니다.", 0, null);
            var current = _inventoryRepository.GetQuantity(medicationId);
            if (current < quantity)
            {
                return new ConsumeResult(false, "재고가 부족합니다.", current, null);
            }

            _inventoryRepository.Decrease(medicationId, quantity);
            var after = _inventoryRepository.GetQuantity(medicationId);

            // Threshold checks and auto order
            var med = _medicationRepository.GetById(medicationId);
            if (med == null) return new ConsumeResult(true, null, after, null);

            int? poId = null;
            if (after < med.ReorderPoint && med.ReorderQuantity > 0)
            {
                poId = _purchaseOrderRepository.InsertOrIncreasePending(medicationId, med.ReorderQuantity);
				if (poId.HasValue)
				{
					// Fire-and-forget email notification (ignore result)
					_emailNotificationService.TrySendAutoOrderNotification(med, poId.Value, after);
				}
            }

            return new ConsumeResult(true, null, after, poId);
        }

        public record CheckResult(bool AtOrBelowMinimum, bool BelowReorderPoint, int CurrentStock, int ReorderPoint, int MinimumStock);
        public record ConsumeResult(bool Success, string? ErrorMessage, int AfterStock, int? PurchaseOrderId);
    }
}



