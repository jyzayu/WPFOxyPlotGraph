using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Commons.Printing;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Repositories;
using WpfOxyPlotGraph.WpfBase;
using System.Diagnostics;

namespace WpfOxyPlotGraph.ViewModels
{
	public class EncounterEditorViewModel : ViewModelBase
	{
		private readonly PatientRepository _patientRepo = new PatientRepository();
		private readonly EncounterRepository _encounterRepo = new EncounterRepository();
		private readonly AttachmentService _attachments = new AttachmentService();
		private readonly DocumentTemplateService _templates = new DocumentTemplateService();
		private readonly PrintService _print = new PrintService();
		private readonly AuditRepository _auditRepo = new AuditRepository();

		public ObservableCollection<Patient> Patients { get; } = new ObservableCollection<Patient>();
		public ObservableCollection<Encounter> Encounters { get; } = new ObservableCollection<Encounter>();
		public ObservableCollection<EncounterAttachment> CurrentAttachments { get; } = new ObservableCollection<EncounterAttachment>();
		public ObservableCollection<DbAuditEntry> EncounterHistory { get; } = new ObservableCollection<DbAuditEntry>();

		public EncounterEditorViewModel()
		{
			LoadPatients();
		}

		private void LoadPatients()
		{
			Patients.Clear();
			var swRepo = Stopwatch.StartNew();
            var list =	_patientRepo.GetPageLite(0, 10).ToList();
			swRepo.Stop();
			Trace.WriteLine($"[UI] Patients(repo) enumerate {swRepo.ElapsedMilliseconds} ms (rows={list.Count})");

			var swUi = Stopwatch.StartNew();
			foreach (var p in list)
			{
				Patients.Add(p);
			}
			swUi.Stop();
			Trace.WriteLine($"[UI] Patients(collection) populate {swUi.ElapsedMilliseconds} ms");
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
				LoadEncounters();
				SelectedEncounter = null;
			}
		}

		private Encounter _selectedEncounter;
		public Encounter SelectedEncounter
		{
			get => _selectedEncounter;
			set
			{
				if (_selectedEncounter == value) return;
				_selectedEncounter = value;
				OnPropertyChanged(nameof(SelectedEncounter));
				LoadAttachments();
				LoadHistory();
			}
		}

		public ICommand NewEncounterCommand => new RelayCommand(NewEncounter, () => SelectedPatient != null);
		public ICommand SaveEncounterCommand => new RelayCommand(SaveEncounter, () => SelectedPatient != null && SelectedEncounter != null);
		public ICommand ApplySummaryTemplateCommand => new RelayCommand(ApplySummaryTemplate, () => SelectedEncounter != null && SelectedPatient != null);
		public ICommand AddAttachmentCommand => new RelayCommand<string>(AddAttachment, _ => SelectedEncounter != null && SelectedPatient != null);
		public ICommand PreviewAttachmentCommand => new RelayCommand<string>(PreviewAttachment, _ => SelectedEncounter != null && SelectedPatient != null);
		public ICommand DeleteAttachmentCommand => new RelayCommand<string>(DeleteAttachment, _ => SelectedEncounter != null && SelectedPatient != null);
		public ICommand PrintSummaryCommand => new RelayCommand(PrintSummary, () => SelectedEncounter != null && SelectedPatient != null);
		public ICommand PrintPrescriptionCommand => new RelayCommand(PrintPrescription, () => SelectedEncounter != null && SelectedPatient != null);
		public ICommand PrintLabOrderCommand => new RelayCommand(PrintLabOrder, () => SelectedEncounter != null && SelectedPatient != null);

		private void LoadEncounters()
		{
			Encounters.Clear();
			if (SelectedPatient == null) return;
			var swRepo = Stopwatch.StartNew();
			var list = _encounterRepo.GetByPatientIdPage(SelectedPatient.Id, 0, 100).ToList();
			swRepo.Stop();
			Trace.WriteLine($"[UI] Encounters(repo) enumerate {swRepo.ElapsedMilliseconds} ms (rows={list.Count})");

			var swUi = Stopwatch.StartNew();
			foreach (var e in list) Encounters.Add(e);
			swUi.Stop();
			Trace.WriteLine($"[UI] Encounters(collection) populate {swUi.ElapsedMilliseconds} ms");
		}

		private void NewEncounter()
		{
			if (SelectedPatient == null) return;
			var now = DateTime.Now;
			SelectedEncounter = new Encounter
			{
				Id = 0,
				PatientId = SelectedPatient.Id,
				VisitAt = now,
				Diagnosis = string.Empty,
				Notes = string.Empty
			};
		}

		private void SaveEncounter()
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			if (SelectedEncounter.Id == 0)
			{
				var id = _encounterRepo.Insert(SelectedEncounter);
				SelectedEncounter.Id = id;
				Encounters.Insert(0, SelectedEncounter);
			}
			else
			{
				_encounterRepo.Update(SelectedEncounter);
				// refresh list item ordering by VisitAt desc
				var ordered = Encounters.OrderByDescending(x => x.VisitAt).ThenByDescending(x => x.Id).ToList();
				Encounters.Clear();
				foreach (var e in ordered) Encounters.Add(e);
			}
			LoadHistory();
		}

		private void ApplySummaryTemplate()
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			var content = _templates.ApplyEncounterTemplate(DocumentTemplateType.EncounterSummary, SelectedPatient, SelectedEncounter);
			SelectedEncounter.Notes = content;
			OnPropertyChanged(nameof(SelectedEncounter));
		}

		private void AddAttachment(string filePath)
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			var item = _attachments.Add(SelectedPatient.Id, SelectedEncounter.Id, filePath);
			CurrentAttachments.Insert(0, item);
		}

		private void PreviewAttachment(string attachmentId)
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			_attachments.OpenWithDefaultApp(SelectedPatient.Id, SelectedEncounter.Id, attachmentId);
		}

		private void DeleteAttachment(string attachmentId)
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			if (_attachments.Delete(SelectedPatient.Id, SelectedEncounter.Id, attachmentId))
			{
				var idx = CurrentAttachments.ToList().FindIndex(a => a.Id == attachmentId);
				if (idx >= 0) CurrentAttachments.RemoveAt(idx);
			}
		}

		private void LoadAttachments()
		{
			CurrentAttachments.Clear();
			if (SelectedPatient == null || SelectedEncounter == null || SelectedEncounter.Id == 0) return;
			foreach (var a in _attachments.List(SelectedPatient.Id, SelectedEncounter.Id)) CurrentAttachments.Add(a);
		}

		private void LoadHistory()
		{
			EncounterHistory.Clear();
			if (SelectedEncounter == null || SelectedEncounter.Id == 0) return;
			foreach (var h in _auditRepo.GetEncounterHistory(SelectedEncounter.Id).OrderByDescending(h => h.AuditId))
			{
				EncounterHistory.Add(h);
			}
		}

		private void PrintSummary()
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			var doc = _print.BuildEncounterSummaryDocument(SelectedPatient, SelectedEncounter);
			_print.ShowPrintPreview(doc);
		}

		private void PrintPrescription()
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			var doc = _print.BuildPrescriptionDocument(SelectedPatient, SelectedEncounter);
			_print.ShowPrintPreview(doc);
		}

		private void PrintLabOrder()
		{
			if (SelectedPatient == null || SelectedEncounter == null) return;
			var doc = _print.BuildLabOrderDocument(SelectedPatient, SelectedEncounter);
			_print.ShowPrintPreview(doc);
		}
	}
}


