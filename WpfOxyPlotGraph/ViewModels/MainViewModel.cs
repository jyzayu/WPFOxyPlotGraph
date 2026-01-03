using OxyPlot;
using OxyPlot.Axes;
using System.Windows.Input;
using WpfBase;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Commons.Auth;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.Views;
using WpfOxyPlotGraph.WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
    public class MainViewModel : ViewModelBase
  {
        private readonly INavigationService _navigationService;
        private readonly AuthService _authService = GetAuthService();
        public ICommand LoadOxyPlotCommand => new RelayCommand<string>(LoadOxyPlot);
        public ICommand NavigateToDashboardCommand { get; }
        public ICommand InputScoreCommand { get; }
        public ICommand ViewStudentsCommand { get; }
        public ICommand ManageAppointmentsCommand { get; }
        public ICommand ViewDoctorScheduleCommand { get; }
        public ICommand ManagePrescriptionsCommand { get; }
        public ICommand ManageEncountersCommand { get; }
        public ICommand ViewAuditLogCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand OpenBackupCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }
        public string CurrentUserDisplay => _authService?.CurrentUser?.DisplayName ?? "로그인 필요";
        public bool IsAuthenticated => _authService?.CurrentUser != null;
        public bool CanViewAudit => _authService?.IsInRole(Role.Admin) == true || _authService?.IsInRole(Role.Auditor) == true;
        public bool CanOpenSettings => _authService?.IsInRole(Role.Admin) == true;
        public bool CanOpenBackup => _authService?.IsInRole(Role.Admin) == true;
        public bool CanManagePrescriptions => _authService?.IsInRole(Role.Doctor) == true;
        public bool CanViewDoctorSchedule => _authService?.IsInRole(Role.Doctor) == true;
        public bool CanManageAppointments => (_authService?.IsInRole(Role.Doctor) == true) || (_authService?.IsInRole(Role.FrontDesk) == true);
        public bool CanRegisterPatient => (_authService?.IsInRole(Role.Doctor) == true) || (_authService?.IsInRole(Role.Nurse) == true) || (_authService?.IsInRole(Role.FrontDesk) == true);
        public bool CanViewPatients => CanRegisterPatient;
        
        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            InputScoreCommand = new RelayCommand(ExecuteInputScore);    
            NavigateToDashboardCommand = new RelayCommand(NavigateToInitialPage);
            ViewStudentsCommand = new RelayCommand(ExecuteViewStudents);
            ManageAppointmentsCommand = new RelayCommand(ExecuteManageAppointments);
            ViewDoctorScheduleCommand = new RelayCommand(ExecuteViewDoctorSchedule);
            ManagePrescriptionsCommand = new RelayCommand(ExecuteManagePrescriptions);
            ManageEncountersCommand = new RelayCommand(ExecuteManageEncounters);
            ViewAuditLogCommand = new RelayCommand(ExecuteViewAuditLog);
            OpenSettingsCommand = new RelayCommand(ExecuteOpenSettings);
            OpenBackupCommand = new RelayCommand(ExecuteOpenBackup);
            LoginCommand = new RelayCommand(ExecuteLogin);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            if (_authService != null)
            {
                _authService.AuthStateChanged += (_, __) =>
                {
                    OnPropertyChanged(nameof(CurrentUserDisplay));
                    OnPropertyChanged(nameof(IsAuthenticated));
                    OnPropertyChanged(nameof(CanViewAudit));
                    OnPropertyChanged(nameof(CanOpenSettings));
                    OnPropertyChanged(nameof(CanOpenBackup));
                    OnPropertyChanged(nameof(CanManagePrescriptions));
                    OnPropertyChanged(nameof(CanViewDoctorSchedule));
                    OnPropertyChanged(nameof(CanManageAppointments));
                    OnPropertyChanged(nameof(CanRegisterPatient));
                    OnPropertyChanged(nameof(CanViewPatients));
                };
            }
        }


    private void ExecuteInputScore()
    {
           if (!CanRegisterPatient) return;
           _navigationService.NavigateTo<RegisterPatientView>();
    }
    private void ExecuteViewStudents()
    {
           if (!CanViewPatients) return;
           _navigationService.NavigateTo<PatientsListView>();
    }
    private void ExecuteManageAppointments()
    {
           if (!CanManageAppointments) return;
           _navigationService.NavigateTo<AppointmentsView>();
    }
    private void ExecuteViewDoctorSchedule()
    {
           if (!CanViewDoctorSchedule) return;
           _navigationService.NavigateTo<DoctorScheduleView>();
    }
    private void ExecuteManagePrescriptions()
    {
           if (!CanManagePrescriptions) return;
           _navigationService.NavigateTo<PrescriptionView>();
    }
    private void ExecuteManageEncounters()
    {
           if (!CanManagePrescriptions) return;
           _navigationService.NavigateTo<EncounterEditorView>();
    }
    private void ExecuteViewAuditLog()
    {
           if (!CanViewAudit) return;
           _navigationService.NavigateTo<AuditLogView>();
    }
    private void ExecuteOpenSettings()
    {
           if (!CanOpenSettings) return;
           _navigationService.NavigateTo<SettingsView>();
    }
    private void ExecuteOpenBackup()
    {
           if (!CanOpenBackup) return;
           _navigationService.NavigateTo<BackupView>();
    }
        public void NavigateToInitialPage()
        {
            if (IsAuthenticated)
                _navigationService.NavigateTo<PatientsListView>();
            else
                _navigationService.NavigateTo<LoginView>();
        }

        private void ExecuteLogin()
        {
            _navigationService.NavigateTo<LoginView>();
        }
        private void ExecuteLogout()
        {
            _authService?.Logout();
            _navigationService.NavigateTo<LoginView>();
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

    private static AuthService GetAuthService()
    {
        var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(AuthService)) as AuthService;
        return svc ?? new AuthService(new Commons.Audit.AuditLogService());
    }
  }
}
