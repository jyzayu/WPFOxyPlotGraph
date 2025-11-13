using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
  public class InputScoreViewModel : ViewModelBase
  {
    public ObservableCollection<Student> Students { get; }

    private Student _selectedStudent;
    public Student SelectedStudent
    {
      get => _selectedStudent;
      set
      {
        if (_selectedStudent == value) return;
        _selectedStudent = value;
        OnPropertyChanged(nameof(SelectedStudent));
        InvalidateCommands();
      }
    }

    private DateTime _date = DateTime.Today;
    public DateTime Date
    {
      get => _date;
      set
      {
        if (_date == value) return;
        _date = value;
        OnPropertyChanged(nameof(Date));
        InvalidateCommands();
      }
    }

    private int _korScore;
    public int KorScore
    {
      get => _korScore;
      set
      {
        if (_korScore == value) return;
        _korScore = value;
        OnPropertyChanged(nameof(KorScore));
        InvalidateCommands();
      }
    }

    private int _mathScore;
    public int MathScore
    {
      get => _mathScore;
      set
      {
        if (_mathScore == value) return;
        _mathScore = value;
        OnPropertyChanged(nameof(MathScore));
        InvalidateCommands();
      }
    }

    private int _engScore;
    public int EngScore
    {
      get => _engScore;
      set
      {
        if (_engScore == value) return;
        _engScore = value;
        OnPropertyChanged(nameof(EngScore));
        InvalidateCommands();
      }
    }

    public ICommand SaveCommand { get; }
    public ICommand ClearCommand { get; }

    public InputScoreViewModel()
    {
      var students = StudentWithScore
        .GetSeedDatas()
        .Select(x => x.Student)
        .GroupBy(s => s.Id)
        .Select(g => g.First())
        .OrderBy(s => s.Id)
        .ToList();

      Students = new ObservableCollection<Student>(students);

      SaveCommand = new RelayCommand(ExecuteSave, CanSave);
      ClearCommand = new RelayCommand(ExecuteClear);
    }

    private bool CanSave()
    {
      return SelectedStudent != null
        && IsValidScore(KorScore)
        && IsValidScore(MathScore)
        && IsValidScore(EngScore);
    }

    private bool IsValidScore(int score) => score >= 0 && score <= 100;

    private void ExecuteSave()
    {
      // 현재는 입력 가능하도록 UI/VM 구성. 실제 저장은 DB 연동 시 구현 권장.
      // 사용성 확인용으로 알림만 제공.
      MessageBox.Show(
        $"저장됨:\n학생: {SelectedStudent?.Name}\n일자: {Date:yyyy-MM-dd}\n국어: {KorScore}, 수학: {MathScore}, 영어: {EngScore}",
        "입력 완료",
        MessageBoxButton.OK,
        MessageBoxImage.Information
      );
      ExecuteClear();
    }

    private void ExecuteClear()
    {
      Date = DateTime.Today;
      KorScore = 0;
      MathScore = 0;
      EngScore = 0;
    }

    private void InvalidateCommands()
    {
      CommandManager.InvalidateRequerySuggested();
    }
  }
}


