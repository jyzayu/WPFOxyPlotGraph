using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class RegisterPatientView : Page
  {
    public RegisterPatientView()
    {
      InitializeComponent();
      DataContext = new RegisterPatientViewModel();
    }
  }
}


