using System;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
  public class BackupViewModel : ViewModelBase
  {
    private readonly BackupService _backup;
    public string DestinationDirectory { get; set; } = string.Empty;
    public bool Encrypt { get; set; } = true;
    public string? LastResultPath { get; set; }
    public string SimulationReport { get; set; } = string.Empty;

    public ICommand BrowseDestinationCommand => new RelayCommand(BrowseDestination);
    public ICommand RunBackupCommand => new RelayCommand(RunBackup);
    public ICommand SimulateRestoreCommand => new RelayCommand(SimulateRestore);
    public ICommand RunRestoreCommand => new RelayCommand(RunRestore);

    public BackupViewModel() : this(GetBackupService()) { }

    public BackupViewModel(BackupService backup)
    {
      _backup = backup;
      DestinationDirectory = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
        "WpfOxy_Backups");
    }

    private void BrowseDestination()
    {
      var dlg = new System.Windows.Forms.FolderBrowserDialog();
      var res = dlg.ShowDialog();
      if (res == System.Windows.Forms.DialogResult.OK)
      {
        DestinationDirectory = dlg.SelectedPath;
        OnPropertyChanged(nameof(DestinationDirectory));
      }
    }

    private void RunBackup()
    {
      try
      {
        var path = _backup.CreateBackup(DestinationDirectory, Encrypt, "manual");
        LastResultPath = path;
        OnPropertyChanged(nameof(LastResultPath));
        System.Windows.MessageBox.Show($"백업 완료:\r\n{path}", "백업");
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show($"백업 실패: {ex.Message}", "오류");
      }
    }

    private void SimulateRestore()
    {
      var ofd = new Microsoft.Win32.OpenFileDialog
      {
        Title = "복원 원본 선택",
        Filter = "Backup Files|*.zip;*.dpapi|All Files|*.*"
      };
      var ok = ofd.ShowDialog();
      if (ok != true) return;
      var report = _backup.SimulateRestore(ofd.FileName, "manual");
      SimulationReport = $"생성: {report.WillCreate.Count}개, 덮어쓰기: {report.WillOverwrite.Count}개";
      OnPropertyChanged(nameof(SimulationReport));
      System.Windows.MessageBox.Show($"{SimulationReport}", "시뮬레이션 결과");
    }

    private void RunRestore()
    {
      var ofd = new Microsoft.Win32.OpenFileDialog
      {
        Title = "복원 원본 선택",
        Filter = "Backup Files|*.zip;*.dpapi|All Files|*.*"
      };
      var ok = ofd.ShowDialog();
      if (ok != true) return;
      var confirm = System.Windows.MessageBox.Show("복원을 진행하시겠습니까? 현재 설정/로그가 덮어써집니다.", "복원 확인", System.Windows.MessageBoxButton.YesNo);
      if (confirm != System.Windows.MessageBoxResult.Yes) return;
      try
      {
        _backup.Restore(ofd.FileName, "manual");
        System.Windows.MessageBox.Show("복원 완료. 일부 설정 적용을 위해 앱 재시작이 필요할 수 있습니다.", "복원");
      }
      catch (Exception ex)
      {
        System.Windows.MessageBox.Show($"복원 실패: {ex.Message}", "오류");
      }
    }

    private static BackupService GetBackupService()
    {
      if (System.Windows.Application.Current is WpfOxyPlotGraph.App && WpfOxyPlotGraph.App.Services != null)
      {
        var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(BackupService)) as BackupService;
        if (svc != null) return svc;
      }
      var settings = new SettingsService();
      return new BackupService(settings);
    }
  }
}


