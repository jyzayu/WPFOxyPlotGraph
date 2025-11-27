using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class AppointmentsView : Page
  {
    public AppointmentsView()
    {
      InitializeComponent();
      DataContext = new AppointmentsViewModel();
    }
  }
}



