using System.Collections.ObjectModel;
using System.Linq;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.WpfBase;
using System.Windows.Input;
using WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
  public class StudentsListViewModel : ViewModelBase
  {
    public ObservableCollection<Student> Students { get; }
    public ObservableCollection<TestScore> Scores { get; } = new ObservableCollection<TestScore>();
    public ICommand DeleteScoreCommand { get; }
    private Student _selectedStudent;
    public Student SelectedStudent
    {
      get => _selectedStudent;
      set
      {
        if (_selectedStudent == value) return;
        _selectedStudent = value;
        OnPropertyChanged(nameof(SelectedStudent));
        LoadScoresForSelectedStudent();
      }
    }

    public StudentsListViewModel()
    {
      DeleteScoreCommand = new RelayCommand<TestScore>(DeleteScore);
      var students = StudentWithScore
        .GetSeedDatas()
        .Select(x => x.Student)
        .GroupBy(s => s.Id)
        .Select(g => g.First())
        .OrderBy(s => s.Id)
        .ToList();

      Students = new ObservableCollection<Student>(students);
    }

    private void DeleteScore(TestScore score)
    {
      if (score == null) return;
      var item = Scores.FirstOrDefault(x => x.Id == score.Id && x.Date == score.Date && x.StudentId == score.StudentId);
      if (item != null)
      {
        Scores.Remove(item);
      }
    }

    private void LoadScoresForSelectedStudent()
    {
      Scores.Clear();
      if (SelectedStudent == null) return;

      var scores = StudentWithScore
        .GetSeedDatas()
        .Where(x => x.Student.Id == SelectedStudent.Id)
        .Select(x => x.Score)
        .OrderBy(x => x.Date)
        .ToList();

      foreach (var score in scores)
      {
        Scores.Add(score);
      }
    }
  }
}


