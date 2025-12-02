using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Repositories;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
    public class PrescriptionViewModel : ViewModelBase
    {
        private readonly MedicationRepository _medicationRepository = new MedicationRepository();
        private readonly InventoryService _inventoryService = new InventoryService();
        private readonly InventoryRepository _inventoryRepository = new InventoryRepository();

        public ObservableCollection<Medication> Medications { get; } = new ObservableCollection<Medication>();

        private Medication? _selectedMedication;
        public Medication? SelectedMedication
        {
            get => _selectedMedication;
            set
            {
                if (_selectedMedication == value) return;
                _selectedMedication = value;
                OnPropertyChanged(nameof(SelectedMedication));
                RefreshCurrentStock();
                OnPropertyChanged(nameof(ProjectedStock));
                AlertMessage = string.Empty;
            }
        }

        private int _currentStock;
        public int CurrentStock
        {
            get => _currentStock;
            private set
            {
                if (_currentStock == value) return;
                _currentStock = value;
                OnPropertyChanged(nameof(CurrentStock));
                OnPropertyChanged(nameof(ProjectedStock));
            }
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(ProjectedStock));
                UpdateThresholdPreview();
            }
        }

        public int ProjectedStock => SelectedMedication == null ? 0 : Math.Max(0, CurrentStock - Math.Max(0, Quantity));

        private string _alertMessage = string.Empty;
        public string AlertMessage
        {
            get => _alertMessage;
            set
            {
                if (_alertMessage == value) return;
                _alertMessage = value;
                OnPropertyChanged(nameof(AlertMessage));
            }
        }

        public ICommand SavePrescriptionCommand => new RelayCommand(SavePrescription);
        public ICommand InitSeedCommand => new RelayCommand(SeedSampleData);

        public PrescriptionViewModel()
        {
            // Ensure tables exist
            _medicationRepository.EnsureTables();
            _inventoryRepository.EnsureTables();
            LoadMedications();
            Quantity = 1;
        }

        private void LoadMedications()
        {
            Medications.Clear();
            foreach (var m in _medicationRepository.GetAll())
            {
                Medications.Add(m);
            }
            SelectedMedication = Medications.FirstOrDefault();
        }

        private void RefreshCurrentStock()
        {
            if (SelectedMedication == null)
            {
                CurrentStock = 0;
                return;
            }
            CurrentStock = _inventoryService.GetCurrentStock(SelectedMedication.Id);
            UpdateThresholdPreview();
        }

        private void UpdateThresholdPreview()
        {
            if (SelectedMedication == null) return;
            var preview = CurrentStock - Math.Max(0, Quantity);
            var check = _inventoryService.CheckThresholds(SelectedMedication.Id);
            if (preview <= check.MinimumStock)
            {
                AlertMessage = "최소재고량 이하로 내려갑니다. 자동 발주가 필요합니다.";
            }
            else if (preview < check.ReorderPoint)
            {
                AlertMessage = "발주점 미만입니다. 자동 발주가 예정됩니다.";
            }
            else
            {
                AlertMessage = string.Empty;
            }
        }

        private void SavePrescription()
        {
            if (SelectedMedication == null) return;
            if (Quantity <= 0) return;

            var result = _inventoryService.ConsumeOnPrescription(SelectedMedication.Id, Quantity);
            if (!result.Success)
            {
                AlertMessage = result.ErrorMessage ?? "처방 처리에 실패했습니다.";
                RefreshCurrentStock();
                return;
            }

            RefreshCurrentStock();

            if (result.PurchaseOrderId.HasValue)
            {
                AlertMessage = $"자동 발주가 생성되었습니다. PO #{result.PurchaseOrderId.Value}";
            }
            else
            {
                var check = _inventoryService.CheckThresholds(SelectedMedication.Id);
                if (check.AtOrBelowMinimum)
                {
                    AlertMessage = "최소재고량 도달했습니다.";
                }
                else
                {
                    AlertMessage = "처방이 저장되었습니다.";
                }
            }
        }

        private void SeedSampleData()
        {
            // Add a few meds if none exist
            if (Medications.Any()) return;

            var id1 = _medicationRepository.Insert(new Medication
            {
                Name = "Acetaminophen 500mg",
                Sku = "ACETA-500",
                MinimumStock = 20,
                ReorderPoint = 50,
                ReorderQuantity = 200
            });
            var id2 = _medicationRepository.Insert(new Medication
            {
                Name = "Amoxicillin 250mg",
                Sku = "AMOXI-250",
                MinimumStock = 10,
                ReorderPoint = 30,
                ReorderQuantity = 100
            });
            _inventoryRepository.UpsertQuantity(id1, 120);
            _inventoryRepository.UpsertQuantity(id2, 25);

            LoadMedications();
        }
    }
}



