using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class BackupView : Page
  {
    public BackupView()
    {
      InitializeComponent();
      DataContext = new BackupViewModel();
    }
  }
}


