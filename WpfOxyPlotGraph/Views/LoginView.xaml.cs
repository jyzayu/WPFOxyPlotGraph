using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class LoginView : Page
  {
    public LoginView()
    {
      InitializeComponent();
      DataContext = new LoginViewModel();
    }

    private void OnLoginClick(object sender, System.Windows.RoutedEventArgs e)
    {
      if (DataContext is LoginViewModel vm)
      {
        vm.LoginCommand.Execute(this.Pwd.Password);
      }
    }
  }
}


