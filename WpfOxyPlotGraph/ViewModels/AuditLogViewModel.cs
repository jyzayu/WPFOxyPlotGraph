using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Commons.Audit;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Repositories;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
  public class AuditLogViewModel : ViewModelBase
  {
    private readonly AuditRepository _repo = new AuditRepository();
    private readonly AuditLogService _fileAudit; // kept for optional CSV export compatibility
    public ObservableCollection<DbAuditEntry> Entries { get; } = new ObservableCollection<DbAuditEntry>();

    public ICommand RefreshCommand => new RelayCommand(Load);
    public ICommand ExportCsvCommand => new RelayCommand(ExportCsv);

    private string _tableFilter = string.Empty;
    public string TableFilter
    {
      get => _tableFilter;
      set
      {
        if (_tableFilter == value) return;
        _tableFilter = value;
        OnPropertyChanged(nameof(TableFilter));
      }
    }

    public AuditLogViewModel() : this(GetAudit()) { }

    public AuditLogViewModel(AuditLogService audit)
    {
      _fileAudit = audit;
      Load();
    }

    private void Load()
    {
      Entries.Clear();
      var rows = string.IsNullOrWhiteSpace(TableFilter)
        ? _repo.GetRecent(500)
        : _repo.GetByTable(TableFilter.Trim().ToUpperInvariant(), 500);
      foreach (var r in rows) Entries.Add(r);
    }

    private void ExportCsv()
    {
      var dest = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
        $"db_audit_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
      System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(dest)!);
      using var sw = new System.IO.StreamWriter(dest, false, System.Text.Encoding.UTF8);
      sw.WriteLine("AuditId,TableName,Operation,PrimaryKeyId,ChangedAt,ChangedBy,OldValues,NewValues");
      foreach (var r in Entries)
      {
        string Esc(string v)
        {
          if (v == null) return string.Empty;
          var needs = v.Contains(',') || v.Contains('"') || v.Contains('\n') || v.Contains('\r');
          if (!needs) return v;
          return $"\"{v.Replace("\"", "\"\"")}\"";
        }
        sw.WriteLine(string.Join(",", new string[] {
          r.AuditId.ToString(),
          Esc(r.TableName),
          Esc(r.Operation),
          r.PrimaryKeyId?.ToString() ?? string.Empty,
          r.ChangedAt.ToString("yyyy-MM-dd HH:mm:ss"),
          Esc(r.ChangedBy),
          Esc(r.OldValues ?? string.Empty),
          Esc(r.NewValues ?? string.Empty)
        }));
      }
      sw.Flush();
      System.Windows.MessageBox.Show($"감사 로그를 내보냈습니다:\r\n{dest}", "내보내기 완료");
    }

    private static AuditLogService GetAudit()
    {
      if (System.Windows.Application.Current is WpfOxyPlotGraph.App && WpfOxyPlotGraph.App.Services != null)
      {
        var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(AuditLogService)) as AuditLogService;
        if (svc != null) return svc;
      }
      return new AuditLogService();
    }
  }
}


