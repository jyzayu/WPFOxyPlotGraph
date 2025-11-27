using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Repositories;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
    public class RegisterPatientViewModel : ViewModelBase
    {
        private readonly PatientRepository _patientRepository = new PatientRepository();

        public string Name { get; set; } = string.Empty;
        public string ResidentRegistrationNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;

        public ICommand RegisterCommand => new RelayCommand(Register);
        public ICommand ClearCommand => new RelayCommand(Clear);

        public RegisterPatientViewModel()
        {
            _patientRepository.EnsureTables();
        }

        private void Register()
        {
            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(ResidentRegistrationNumber) ||
                string.IsNullOrWhiteSpace(Address) ||
                string.IsNullOrWhiteSpace(Contact))
            {
                return;
            }

            var patient = new Patient
            {
                Name = Name.Trim(),
                ResidentRegistrationNumber = ResidentRegistrationNumber.Trim(),
                Address = Address.Trim(),
                Contact = Contact.Trim()
            };

            _patientRepository.Insert(patient);
            Clear();
        }

        private void Clear()
        {
            Name = string.Empty;
            ResidentRegistrationNumber = string.Empty;
            Address = string.Empty;
            Contact = string.Empty;
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(ResidentRegistrationNumber));
            OnPropertyChanged(nameof(Address));
            OnPropertyChanged(nameof(Contact));
        }
    }
}


