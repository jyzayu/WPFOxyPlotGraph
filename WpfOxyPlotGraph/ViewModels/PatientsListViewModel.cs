using System.Collections.ObjectModel;
using System.Linq;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.WpfBase;
using System.Collections.Generic;
using WpfOxyPlotGraph.Repositories;
using WpfOxyPlotGraph.Commons.Audit;
using System.Threading.Tasks;

namespace WpfOxyPlotGraph.ViewModels
{
  public class PatientsListViewModel : ViewModelBase
  {
    public ObservableCollection<Patient> Patients { get; }
    public ObservableCollection<PatientVisit> Visits { get; } = new ObservableCollection<PatientVisit>();

    private readonly PatientRepository _patientRepository = new PatientRepository();
    private readonly VisitRepository _visitRepository = new VisitRepository();
    private readonly AuditLogService _auditLogService;

    private Patient _selectedPatient;
    public Patient SelectedPatient
    {
      get => _selectedPatient;
      set
      {
        if (_selectedPatient == value) return;
        _selectedPatient = value;
        OnPropertyChanged(nameof(SelectedPatient));
        LoadVisitsForSelectedPatient();
        _ = LogViewPatientAsync(_selectedPatient);
      }
    }

    public PatientsListViewModel() : this(GetAuditFromServiceProvider())
    {
    }

    public PatientsListViewModel(AuditLogService auditLogService)
    {
      _auditLogService = auditLogService;

      IEnumerable<Patient> allPatients = _patientRepository.GetAll().OrderBy(p => p.Id);
      Patients = new ObservableCollection<Patient>(allPatients);
    }

    private void LoadVisitsForSelectedPatient()
    {
      Visits.Clear();
      if (SelectedPatient == null) return;

      IEnumerable<PatientVisit> visits = _visitRepository.GetByPatientId(SelectedPatient.Id);
      foreach (var v in visits)
      {
        Visits.Add(v);
      }
    }

    public void UpdatePatient(Patient updated)
    {
      if (updated == null) return;

      try
      {
        _patientRepository.Update(updated);
      }
      catch (System.Exception ex)
      {
        _ = _auditLogService.LogAsync(new AuditEvent
        {
          Action = "UpdatePatient",
          PatientId = updated.Id,
          PatientName = updated.Name,
          Details = "Patient update failed",
          User = AuditLogService.GetDefaultUser(),
          Success = false,
          ErrorMessage = ex.Message
        });
        var notifier = GetNotificationService();
        notifier.ShowError("환자 정보 저장 중 오류가 발생했습니다.", ex, retry: () =>
        {
          _patientRepository.Update(updated);
        });
      }

      // Replace the item in the collection to ensure UI refresh
      Patient existing = Patients.FirstOrDefault(p => p.Id == updated.Id);
      if (existing != null)
      {
        int index = Patients.IndexOf(existing);
        var newItem = new Patient
        {
          Id = updated.Id,
          Name = updated.Name,
          ResidentRegistrationNumber = updated.ResidentRegistrationNumber,
          Address = updated.Address,
          Contact = updated.Contact
        };
        Patients[index] = newItem;
        SelectedPatient = newItem;
      }

      _ = _auditLogService.LogAsync(new AuditEvent
      {
        Action = "UpdatePatient",
        PatientId = updated.Id,
        PatientName = updated.Name,
        Details = "Patient record updated",
        User = AuditLogService.GetDefaultUser(),
        Success = true
      });
    }

    private async Task LogViewPatientAsync(Patient patient)
    {
      if (patient == null) return;
      await _auditLogService.LogAsync(new AuditEvent
      {
        Action = "ViewPatient",
        PatientId = patient.Id,
        PatientName = patient.Name,
        Details = "Patient selected in list",
        User = AuditLogService.GetDefaultUser(),
        Success = true
      });
    }

    private static AuditLogService GetAuditFromServiceProvider()
    {
      if (System.Windows.Application.Current is WpfOxyPlotGraph.App app && WpfOxyPlotGraph.App.Services != null)
      {
        var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(AuditLogService)) as AuditLogService;
        if (svc != null) return svc;
      }
      return new AuditLogService();
    }

    private static WpfOxyPlotGraph.Commons.NotificationService GetNotificationService()
    {
      if (System.Windows.Application.Current is WpfOxyPlotGraph.App && WpfOxyPlotGraph.App.Services != null)
      {
        var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(WpfOxyPlotGraph.Commons.NotificationService)) as WpfOxyPlotGraph.Commons.NotificationService;
        if (svc != null) return svc;
      }
      return new WpfOxyPlotGraph.Commons.NotificationService();
    }
  }
}


