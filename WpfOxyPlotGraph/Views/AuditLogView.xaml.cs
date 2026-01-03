using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class AuditLogView : Page
  {
    public AuditLogView()
    {
      InitializeComponent();
      DataContext = new AuditLogViewModel();
    }
  }
}


