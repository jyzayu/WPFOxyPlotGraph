using System.Collections.ObjectModel;
using System.Linq;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.WpfBase;
using System.Collections.Generic;
using WpfOxyPlotGraph.Repositories;
using WpfOxyPlotGraph.Commons.Audit;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WpfOxyPlotGraph.ViewModels
{
  public class PatientsListViewModel : ViewModelBase
  {
    public ObservableCollection<Patient> Patients { get; }
    public ObservableCollection<PatientVisit> Visits { get; } = new ObservableCollection<PatientVisit>();

    private const int PageSize = 10;
    private string _searchText = string.Empty;
    public string SearchText
    {
      get => _searchText;
      set
      {
        if (_searchText == value) return;
        _searchText = value ?? string.Empty;
        OnPropertyChanged(nameof(SearchText));
        _ = LoadPatientsAsync();
      }
    }

    private readonly PatientRepository _patientRepository = new PatientRepository();
    private readonly VisitRepository _visitRepository = new VisitRepository();
    private readonly AuditLogService _auditLogService;

    private Patient _selectedPatient;
    private bool _loadingDetail = false;
    public Patient SelectedPatient
    {
      get => _selectedPatient;
      set
      {
        if (_selectedPatient == value) return;
        _selectedPatient = value;
        OnPropertyChanged(nameof(SelectedPatient));

        if (_selectedPatient != null && !_loadingDetail)
        {
          try
          {
            _loadingDetail = true;
            var sw = Stopwatch.StartNew();
            var full = _patientRepository.GetById(_selectedPatient.Id);
            sw.Stop();
            Trace.WriteLine($"[UI] Patient detail fetch {sw.ElapsedMilliseconds} ms (id={_selectedPatient.Id})");
            if (full != null)
            {
              // update selected instance fields without replacing reference to minimize UI churn
              _selectedPatient.Name = full.Name;
              _selectedPatient.ResidentRegistrationNumber = full.ResidentRegistrationNumber;
              _selectedPatient.Address = full.Address;
              _selectedPatient.Contact = full.Contact;
              OnPropertyChanged(nameof(SelectedPatient));
            }
          }
          finally
          {
            _loadingDetail = false;
          }
        }

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

      Patients = new ObservableCollection<Patient>();
      _ = LoadPatientsAsync();
    }

    private bool _loadingPatients = false;
    private async Task LoadPatientsAsync()
    {
      if (_loadingPatients) return;
      _loadingPatients = true;
      try
      {
        var swRepo = Stopwatch.StartNew();
        var list = await Task.Run(() =>
        {
          if (!string.IsNullOrWhiteSpace(SearchText))
          {
            return _patientRepository.GetPageLiteSearch(SearchText.Trim(), 0, PageSize).ToList();
          }
          return _patientRepository.GetPageLite(0, PageSize).ToList();
        });
        swRepo.Stop();
        Trace.WriteLine($"[UI] Patients repo enumerate {swRepo.ElapsedMilliseconds} ms (rows={list.Count}) search='{SearchText}'");

        var swUi = Stopwatch.StartNew();
        Patients.Clear();
        foreach (var p in list) Patients.Add(p);
        swUi.Stop();
        Trace.WriteLine($"[UI] Patients collection populate {swUi.ElapsedMilliseconds} ms");
      }
      finally
      {
        _loadingPatients = false;
      }
    }

    private void LoadVisitsForSelectedPatient()
    {
      Visits.Clear();
      if (SelectedPatient == null) return;

      var swRepo = Stopwatch.StartNew();
      var list = _visitRepository.GetByPatientIdPage(SelectedPatient.Id, 0, 100).ToList();
      swRepo.Stop();
      Trace.WriteLine($"[UI] Visits repo enumerate {swRepo.ElapsedMilliseconds} ms (rows={list.Count})");

      var swUi = Stopwatch.StartNew();
      foreach (var v in list)
      {
        Visits.Add(v);
      }
      swUi.Stop();
      Trace.WriteLine($"[UI] Visits collection populate {swUi.ElapsedMilliseconds} ms");
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


