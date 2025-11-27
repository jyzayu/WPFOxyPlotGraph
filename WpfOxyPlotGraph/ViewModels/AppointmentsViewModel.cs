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
    public class AppointmentsViewModel : ViewModelBase
    {
        private readonly PatientRepository _patientRepository = new PatientRepository();
        private readonly AppointmentRepository _appointmentRepository = new AppointmentRepository();

        public ObservableCollection<Patient> Patients { get; }
        public ObservableCollection<Appointment> Appointments { get; } = new ObservableCollection<Appointment>();
        public string[] AvailableStatuses { get; } = new[] { "Scheduled", "Completed", "Cancelled" };

        private Patient _selectedPatient;
        public Patient SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (_selectedPatient == value) return;
                _selectedPatient = value;
                OnPropertyChanged(nameof(SelectedPatient));
                LoadAppointmentsForSelectedPatient();
                ClearForm();
            }
        }

        private Appointment _selectedAppointment;
        public Appointment SelectedAppointment
        {
            get => _selectedAppointment;
            set
            {
                if (_selectedAppointment == value) return;
                _selectedAppointment = value;
                OnPropertyChanged(nameof(SelectedAppointment));
                FillFormFromSelected();
            }
        }

        public string AppointmentDateTimeText { get; set; } = string.Empty; // yyyy-MM-dd HH:mm

        private string _doctorName = string.Empty;
        public string DoctorName
        {
            get => _doctorName;
            set
            {
                if (_doctorName == value) return;
                _doctorName = value;
                OnPropertyChanged(nameof(DoctorName));
                RefreshAvailableTimeSlots();
            }
        }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Scheduled";
        public string Notes { get; set; } = string.Empty;

        public ICommand CreateCommand => new RelayCommand(CreateAppointment);
        public ICommand UpdateCommand => new RelayCommand(UpdateAppointment);
        public ICommand CancelCommand => new RelayCommand(CancelAppointment);
        public ICommand ClearCommand => new RelayCommand(ClearForm);

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate == value) return;
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                RefreshAvailableTimeSlots();
            }
        }
        private DateTime? _selectedDate;

        public ObservableCollection<string> AvailableTimeSlots { get; } = new ObservableCollection<string>();

        public string? SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set
            {
                if (_selectedTimeSlot == value) return;
                _selectedTimeSlot = value;
                OnPropertyChanged(nameof(SelectedTimeSlot));
            }
        }
        private string? _selectedTimeSlot;

        public AppointmentsViewModel()
        {
            _patientRepository.EnsureTables();
            _appointmentRepository.EnsureTables();

            var allPatients = _patientRepository.GetAll().OrderBy(p => p.Id);
            Patients = new ObservableCollection<Patient>(allPatients);
        }

        private void LoadAppointmentsForSelectedPatient()
        {
            Appointments.Clear();
            if (SelectedPatient == null) return;
            foreach (var a in _appointmentRepository.GetByPatientId(SelectedPatient.Id))
            {
                Appointments.Add(a);
            }
        }

        private void FillFormFromSelected()
        {
            if (SelectedAppointment == null)
            {
                ClearForm();
                return;
            }
            AppointmentDateTimeText = SelectedAppointment.AppointmentDate.ToString("yyyy-MM-dd HH:mm");
            DoctorName = SelectedAppointment.DoctorName;
            Reason = SelectedAppointment.Reason;
            Status = SelectedAppointment.Status;
            Notes = SelectedAppointment.Notes;
            // Sync new date/time selectors
            SelectedDate = SelectedAppointment.AppointmentDate.Date;
            SelectedTimeSlot = SelectedAppointment.AppointmentDate.ToString("HH:mm");
            OnPropertyChanged(nameof(AppointmentDateTimeText));
            OnPropertyChanged(nameof(DoctorName));
            OnPropertyChanged(nameof(Reason));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Notes));
        }

        private void RefreshAvailableTimeSlots()
        {
            AvailableTimeSlots.Clear();
            if (string.IsNullOrWhiteSpace(DoctorName)) return;
            if (SelectedDate == null) return;

            var workStart = new TimeSpan(9, 0, 0);   // 09:00
            var workEnd = new TimeSpan(17, 0, 0);    // 17:00
            var slotMinutes = 30;

            var existing = _appointmentRepository
                .GetByDoctorAndDate(DoctorName.Trim(), SelectedDate.Value.Date)
                .Where(a => !string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                .Select(a => a.AppointmentDate.ToString("HH:mm"))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var cursor = workStart;
            while (cursor < workEnd)
            {
                var label = $"{cursor.Hours:D2}:{cursor.Minutes:D2}";
                if (!existing.Contains(label))
                {
                    AvailableTimeSlots.Add(label);
                }
                cursor = cursor.Add(TimeSpan.FromMinutes(slotMinutes));
            }

            // Preserve current selection if still available
            if (SelectedTimeSlot != null && !AvailableTimeSlots.Contains(SelectedTimeSlot))
            {
                SelectedTimeSlot = null;
            }
        }

        private DateTime? TryParseAppointmentDateTime()
        {
            if (DateTime.TryParse(AppointmentDateTimeText, out var dt))
            {
                return dt;
            }
            return null;
        }

        private DateTime? GetSelectedDateTime()
        {
            if (SelectedDate != null && !string.IsNullOrWhiteSpace(SelectedTimeSlot))
            {
                if (TimeSpan.TryParse(SelectedTimeSlot, out var ts))
                {
                    return SelectedDate.Value.Date + ts;
                }
            }
            return null;
        }

        private bool IsSlotAvailable(string doctorName, DateTime dateTime, int? excludeAppointmentId = null)
        {
            var sameDay = _appointmentRepository
                .GetByDoctorAndDate(doctorName, dateTime.Date)
                .Where(a => !string.Equals(a.Status, "Cancelled", StringComparison.OrdinalIgnoreCase));

            foreach (var a in sameDay)
            {
                if (excludeAppointmentId.HasValue && a.Id == excludeAppointmentId.Value) continue;
                if (a.AppointmentDate == dateTime) return false;
            }
            return true;
        }

        private bool ValidateForCreateOrUpdate()
        {
            if (SelectedPatient == null) return false;
            if (string.IsNullOrWhiteSpace(DoctorName)) return false;
            if (string.IsNullOrWhiteSpace(Reason)) return false;
            // Allow either new schedule selection or legacy free text
            var scheduleDt = GetSelectedDateTime();
            if (scheduleDt == null && TryParseAppointmentDateTime() == null) return false;
            return true;
        }

        private void CreateAppointment()
        {
            if (!ValidateForCreateOrUpdate()) return;
            var dt = GetSelectedDateTime() ?? TryParseAppointmentDateTime()!.Value;
            // Enforce availability
            if (!IsSlotAvailable(DoctorName.Trim(), dt))
            {
                return;
            }
            var appt = new Appointment
            {
                PatientId = SelectedPatient.Id,
                AppointmentDate = dt,
                DoctorName = DoctorName.Trim(),
                Reason = Reason.Trim(),
                Status = string.IsNullOrWhiteSpace(Status) ? "Scheduled" : Status.Trim(),
                Notes = Notes?.Trim() ?? string.Empty
            };
            var newId = _appointmentRepository.Insert(appt);
            appt.Id = newId;
            Appointments.Insert(0, appt);
            SelectedAppointment = appt;
        }

        private void UpdateAppointment()
        {
            if (SelectedAppointment == null) return;
            if (!ValidateForCreateOrUpdate()) return;

            var dt = GetSelectedDateTime() ?? TryParseAppointmentDateTime()!.Value;
            if (!IsSlotAvailable(DoctorName.Trim(), dt, SelectedAppointment.Id))
            {
                return;
            }
            SelectedAppointment.AppointmentDate = dt;
            SelectedAppointment.DoctorName = DoctorName.Trim();
            SelectedAppointment.Reason = Reason.Trim();
            SelectedAppointment.Status = string.IsNullOrWhiteSpace(Status) ? "Scheduled" : Status.Trim();
            SelectedAppointment.Notes = Notes?.Trim() ?? string.Empty;

            _appointmentRepository.Update(SelectedAppointment);

            // Refresh grid
            var idx = Appointments.IndexOf(SelectedAppointment);
            if (idx >= 0)
            {
                Appointments.RemoveAt(idx);
                // Keep sorted by date desc
                var insertIndex = 0;

                //System.NullReferenceException: 'Object reference not set to an instance of an object.'
                //WpfOxyPlotGraph.ViewModels.AppointmentsViewModel.SelectedAppointment.get returned null.
                if (SelectedAppointment == null) return;
                
                while (insertIndex < Appointments.Count &&
                        Appointments[insertIndex].AppointmentDate >= SelectedAppointment.AppointmentDate)
                {
                    insertIndex++;
                }


                Appointments.Insert(insertIndex, SelectedAppointment);
            }
        }

        private void CancelAppointment()
        {
            if (SelectedAppointment == null) return;
            _appointmentRepository.Cancel(SelectedAppointment.Id);
            SelectedAppointment.Status = "Cancelled";
            OnPropertyChanged(nameof(SelectedAppointment));

            // Reflect status immediately in collection
            var idx = Appointments.IndexOf(SelectedAppointment);
            if (idx >= 0)
            {
                Appointments[idx] = SelectedAppointment;
            }
        }

        private void ClearForm()
        {
            AppointmentDateTimeText = string.Empty;
            DoctorName = string.Empty;
            Reason = string.Empty;
            Status = "Scheduled";
            Notes = string.Empty;
            OnPropertyChanged(nameof(AppointmentDateTimeText));
            OnPropertyChanged(nameof(DoctorName));
            OnPropertyChanged(nameof(Reason));
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(Notes));
        }
    }
}


