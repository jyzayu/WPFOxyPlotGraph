using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class StudentsListView : Page
  {
    public StudentsListView()
    {
      InitializeComponent();
      DataContext = new StudentsListViewModel();
    }
  }
}


