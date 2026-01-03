using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models.Settings;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
  public class SettingsViewModel : ViewModelBase
  {
    private readonly SettingsService _settingsService;

    public HospitalInfo Hospital { get; set; } = new HospitalInfo();
    public string DocumentTemplatesPath { get; set; } = string.Empty;
    public ObservableCollection<string> Printers { get; } = new ObservableCollection<string>();
    public string SelectedPrinter { get; set; } = string.Empty;

    public ICommand BrowseTemplatesPathCommand => new RelayCommand(BrowseTemplatesPath);
    public ICommand SaveCommand => new RelayCommand(Save);
    public ICommand ReloadCommand => new RelayCommand(Reload);

    public SettingsViewModel() : this(GetSettingsService()) { }

    public SettingsViewModel(SettingsService settingsService)
    {
      _settingsService = settingsService;
      Reload();
      LoadPrinters();
    }

    private void Reload()
    {
      var s = _settingsService.Get();
      Hospital = s.Hospital ?? new HospitalInfo();
      DocumentTemplatesPath = s.DocumentTemplatesPath ?? string.Empty;
      SelectedPrinter = s.DefaultPrinter ?? string.Empty;
      OnPropertyChanged(nameof(Hospital));
      OnPropertyChanged(nameof(DocumentTemplatesPath));
      OnPropertyChanged(nameof(SelectedPrinter));
    }

    private void Save()
    {
      var s = new AppSettings
      {
        Hospital = Hospital,
        DocumentTemplatesPath = DocumentTemplatesPath,
        DefaultPrinter = SelectedPrinter
      };
      _settingsService.Save(s);
      System.Windows.MessageBox.Show("설정을 저장했습니다.", "저장 완료");
    }

    private void BrowseTemplatesPath()
    {
      var dlg = new Microsoft.Win32.OpenFileDialog
      {
        Title = "템플릿 폴더 선택 (아무 파일 선택)",
        CheckFileExists = false,
        FileName = "폴더를 선택하려면 아무 파일명이나 입력 후 열기"
      };
      var res = dlg.ShowDialog();
      if (res == true)
      {
        var dir = System.IO.Path.GetDirectoryName(dlg.FileName) ?? string.Empty;
        DocumentTemplatesPath = dir;
        OnPropertyChanged(nameof(DocumentTemplatesPath));
      }
    }

    private void LoadPrinters()
    {
      Printers.Clear();
      foreach (var p in _settingsService.GetInstalledPrinters())
      {
        Printers.Add(p);
      }
      if (!string.IsNullOrWhiteSpace(SelectedPrinter) && !Printers.Contains(SelectedPrinter))
      {
        Printers.Insert(0, SelectedPrinter);
      }
    }

    private static SettingsService GetSettingsService()
    {
      if (System.Windows.Application.Current is WpfOxyPlotGraph.App && WpfOxyPlotGraph.App.Services != null)
      {
        var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(SettingsService)) as SettingsService;
        if (svc != null) return svc;
      }
      return new SettingsService();
    }
  }
}


