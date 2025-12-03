using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Repositories;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
    public class DoctorScheduleViewModel : ViewModelBase
    {
        private readonly AppointmentRepository _appointmentRepository = new AppointmentRepository();
        private readonly PatientRepository _patientRepository = new PatientRepository();

        public ObservableCollection<string> Doctors { get; } = new ObservableCollection<string>();
        public ObservableCollection<Patient> Patients { get; } = new ObservableCollection<Patient>();

        private string _selectedDoctor = string.Empty;
        public string SelectedDoctor
        {
            get => _selectedDoctor;
            set
            {
                if (_selectedDoctor == value) return;
                _selectedDoctor = value;
                OnPropertyChanged(nameof(SelectedDoctor));
                RefreshSchedule();
            }
        }

        private DateTime? _selectedDate = DateTime.Today;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate == value) return;
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                RefreshSchedule();
            }
        }

        public ObservableCollection<string> BookedTimeSlots { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableTimeSlots { get; } = new ObservableCollection<string>();

        private string? _selectedAvailableSlot;
        public string? SelectedAvailableSlot
        {
            get => _selectedAvailableSlot;
            set
            {
                if (_selectedAvailableSlot == value) return;
                _selectedAvailableSlot = value;
                OnPropertyChanged(nameof(SelectedAvailableSlot));
            }
        }

        private Patient _selectedPatient;
        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (_selectedPatient == value) return;
                _selectedPatient = value;
                OnPropertyChanged(nameof(SelectedPatient));
            }
        }

        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public ICommand BookAppointmentCommand => new RelayCommand(BookAppointment);

        public DoctorScheduleViewModel()
        {
            _appointmentRepository.EnsureTables();
            _patientRepository.EnsureTables();
            LoadDoctors();
            LoadPatients();
            RefreshSchedule();
        }

        private void LoadDoctors()
        {
            Doctors.Clear();
            foreach (var name in _appointmentRepository.GetDistinctDoctorNames())
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    Doctors.Add(name);
                }
            }
        }

        private void LoadPatients()
        {
            Patients.Clear();
            foreach (var p in _patientRepository.GetAll().OrderBy(p => p.Id))
            {
                Patients.Add(p);
            }
        }

        private void RefreshSchedule()
        {
            BookedTimeSlots.Clear();
            AvailableTimeSlots.Clear();

            if (string.IsNullOrWhiteSpace(SelectedDoctor)) return;
            if (SelectedDate == null) return;

            var workStart = new TimeSpan(9, 0, 0);
            var workEnd = new TimeSpan(17, 0, 0);
            var slotMinutes = 30;

            var appointments = _appointmentRepository
                .GetByDoctorAndDate(SelectedDoctor.Trim(), SelectedDate.Value.Date)
                .Where(a => !string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            var booked = appointments
                .Select(a => a.AppointmentDate.ToString("HH:mm"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var t in booked.OrderBy(x => x))
            {
                BookedTimeSlots.Add(t);
            }

            var cursor = workStart;
            while (cursor < workEnd)
            {
                var label = $"{cursor.Hours:D2}:{cursor.Minutes:D2}";
                if (!booked.Contains(label))
                {
                    AvailableTimeSlots.Add(label);
                }
                cursor = cursor.Add(TimeSpan.FromMinutes(slotMinutes));
            }
        }

        private void BookAppointment()
        {
            if (SelectedPatient == null) return;
            if (string.IsNullOrWhiteSpace(SelectedDoctor)) return;
            if (SelectedDate == null) return;
            if (string.IsNullOrWhiteSpace(SelectedAvailableSlot)) return;
            if (string.IsNullOrWhiteSpace(Reason)) return;

            if (!TimeSpan.TryParse(SelectedAvailableSlot, out var timeOfDay)) return;
            var scheduledDateTime = SelectedDate.Value.Date + timeOfDay;

            // Double-check availability
            var alreadyBooked = _appointmentRepository
                .GetByDoctorAndDate(SelectedDoctor.Trim(), scheduledDateTime.Date)
                .Any(a => !string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase)
                       && a.AppointmentDate == scheduledDateTime);
            if (alreadyBooked) return;

            var appt = new Models.Appointment
            {
                PatientId = SelectedPatient.Id,
                AppointmentDate = scheduledDateTime,
                DoctorName = SelectedDoctor.Trim(),
                Reason = Reason.Trim(),
                Status = "Scheduled",
                Notes = Notes?.Trim() ?? string.Empty
            };
            var newId = _appointmentRepository.Insert(appt);
            if (newId > 0)
            {
                // Refresh lists to reflect the new booking
                RefreshSchedule();
                // Clear only slot/notes/reason to speed repeated bookings
                SelectedAvailableSlot = null;
                Reason = string.Empty;
                Notes = string.Empty;
                OnPropertyChanged(nameof(Reason));
                OnPropertyChanged(nameof(Notes));
            }
        }
    }
}


