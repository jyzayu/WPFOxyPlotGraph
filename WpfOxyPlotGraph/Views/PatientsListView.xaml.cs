using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
  public partial class PatientsListView : Page
  {
    public PatientsListView()
    {
      InitializeComponent();
      DataContext = new PatientsListViewModel();
    }

    private void OnEditRowClick(object sender, System.Windows.RoutedEventArgs e)
    {
      if (sender is System.Windows.FrameworkElement fe && fe.DataContext is WpfOxyPlotGraph.Models.Patient rowPatient)
      {
        var ownerWindow = System.Windows.Window.GetWindow(this);
        var dlg = new EditPatientWindow(rowPatient) { Owner = ownerWindow };
        var result = dlg.ShowDialog();
        if (result == true && dlg.EditedPatient != null)
        {
          if (DataContext is PatientsListViewModel vm)
          {
            vm.UpdatePatient(dlg.EditedPatient);
          }
        }
      }
    }
  }
}

