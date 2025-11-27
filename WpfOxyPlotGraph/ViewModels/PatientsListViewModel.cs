using System.Collections.ObjectModel;
using System.Linq;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.WpfBase;
using System.Collections.Generic;
using WpfOxyPlotGraph.Repositories;

namespace WpfOxyPlotGraph.ViewModels
{
  public class PatientsListViewModel : ViewModelBase
  {
    public ObservableCollection<Patient> Patients { get; }
    public ObservableCollection<PatientVisit> Visits { get; } = new ObservableCollection<PatientVisit>();

    private readonly PatientRepository _patientRepository = new PatientRepository();
    private readonly VisitRepository _visitRepository = new VisitRepository();

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
      }
    }

    public PatientsListViewModel()
    {
      _patientRepository.EnsureTables();
      _visitRepository.EnsureTables();

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
  }
}

