using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class SettingsView : Page
  {
    public SettingsView()
    {
      InitializeComponent();
      DataContext = new SettingsViewModel();
    }
  }
}


