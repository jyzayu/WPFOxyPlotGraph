using OxyPlot;
using OxyPlot.Axes;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Views;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
    public class MainViewModel : ViewModelBase
  {
        private readonly INavigationService _navigationService;
        public ICommand LoadOxyPlotCommand => new RelayCommand<string>(LoadOxyPlot);
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand InputScoreCommand { get; }
        public ICommand ViewStudentsCommand { get; }
        public ICommand ManageAppointmentsCommand { get; }
        public ICommand ViewDoctorScheduleCommand { get; }
        public ICommand ManagePrescriptionsCommand { get; }
        
        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InputScoreCommand = new RelayCommand(ExecuteInputScore);    
            NavigateToDashboardCommand = new RelayCommand(NavigateToInitialPage);
            ViewStudentsCommand = new RelayCommand(ExecuteViewStudents);
            ManageAppointmentsCommand = new RelayCommand(ExecuteManageAppointments);
            ViewDoctorScheduleCommand = new RelayCommand(ExecuteViewDoctorSchedule);
            ManagePrescriptionsCommand = new RelayCommand(ExecuteManagePrescriptions);
        }


    private void ExecuteInputScore()
    {
           _navigationService.NavigateTo<RegisterPatientView>();
    }
    private void ExecuteViewStudents()
    {
           _navigationService.NavigateTo<PatientsListView>();
    }
    private void ExecuteManageAppointments()
    {
           _navigationService.NavigateTo<AppointmentsView>();
    }
    private void ExecuteViewDoctorSchedule()
    {
           _navigationService.NavigateTo<DoctorScheduleView>();
    }
    private void ExecuteManagePrescriptions()
    {
           _navigationService.NavigateTo<PrescriptionView>();
    }
        public void NavigateToInitialPage()
        {
            _navigationService.NavigateTo<PatientsListView>();
        }


        // 또는 ICommand를 사용하여 네비게이션


        Func<TestScore, int> GetScoreFunc(string subject)
    {
      switch (subject)
      {
        case "국어":
          return x => x.KorScore;
        case "수학":
          return x => x.MathScore;
        case "영어":
          return x => x.EngScore;
      }
      return default!;
    }

    private void LoadOxyPlot(string subject)
    {
      Func<TestScore, int> scoreFunc = GetScoreFunc(subject);
      SetPlotModel(subject, scoreFunc);
    }

    private void SetPlotModel(string subject, Func<TestScore, int> testScoreFunc)
    {
      IEnumerable<StudentWithScore> data = StudentWithScore.GetSeedDatas(); // 데이터 생성

      // PlotModel 생성
      OxyPlotManager plotManager = new OxyPlotManager($"{subject} 점수");

      // X축 생성
      plotManager.SetDateTiemAxisX("일자", "yyyy-MM-dd");

      // Y축 생성
      plotManager.SetAxisY("점수");

      // Legend 추가
      plotManager.SetRegend();

      // 데이터 추가
      IEnumerable<IGrouping<Student, StudentWithScore>> studentGroup = data.GroupBy(x => x.Student);

      // 학생별 색상 추가
      plotManager.SetOxyColors(studentGroup.Count());

      foreach (IGrouping<Student, StudentWithScore> studentData in studentGroup)
      {
        IEnumerable<DataPoint> dataPoints = studentData.Select(x => new DataPoint(DateTimeAxis.ToDouble(x.Score.Date), testScoreFunc(x.Score)));
        plotManager.AddLineSeriesDataPoints(studentData.Key.Name, dataPoints);
        plotManager.SetNextColor();
      }

      this.PlotModel = plotManager.PlotModel;

      OnPropertyChanged(nameof(PlotModel));
    }

    public MainViewModel()
    {
    }

    public PlotModel PlotModel { get; set; } = default!;
  }
}
