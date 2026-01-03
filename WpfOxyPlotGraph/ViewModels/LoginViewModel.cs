using System.Windows.Input;
using WpfOxyPlotGraph.Commons.Auth;
using WpfOxyPlotGraph.WpfBase;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Views;
using WpfBase;

namespace WpfOxyPlotGraph.ViewModels
{
  public class LoginViewModel : ViewModelBase
  {
    private readonly AuthService _auth;
    private readonly INavigationService _nav;
    private readonly NotificationService _notify;

    public string Username { get; set; } = string.Empty;

    public ICommand LoginCommand => new RelayCommand<string>(DoLogin);

    public LoginViewModel() : this(GetAuth(), GetNav(), GetNotify()) { }

    public LoginViewModel(AuthService auth, INavigationService nav, NotificationService notify)
    {
      _auth = auth;
      _nav = nav;
      _notify = notify;
    }

    private void DoLogin(string password)
    {
      if (_auth.TryLogin(Username?.Trim() ?? string.Empty, password ?? string.Empty, out var err))
      {
        _nav.NavigateTo<PatientsListView>();
      }
      else
      {
        _notify.ShowError(err);
      }
    }

    private static AuthService GetAuth()
    {
      var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(AuthService)) as AuthService;
      return svc ?? new AuthService(new Commons.Audit.AuditLogService());
    }
    private static INavigationService GetNav()
    {
      var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(INavigationService)) as INavigationService;
      return svc ?? new Commons.NavigationService();
    }
    private static NotificationService GetNotify()
    {
      var svc = WpfOxyPlotGraph.App.Services.GetService(typeof(NotificationService)) as NotificationService;
      return svc ?? new NotificationService();
    }
  }
}


