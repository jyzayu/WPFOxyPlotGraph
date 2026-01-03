using System.Windows;
using WpfOxyPlotGraph.Models;
using MessageBox = System.Windows.MessageBox;

namespace WpfOxyPlotGraph.Views
{
  public partial class EditPatientWindow : Window
  {
    private Patient _editablePatient;

    public Patient EditedPatient { get; private set; }

    public EditPatientWindow(Patient patientToEdit)
    {
      InitializeComponent();
      _editablePatient = new Patient
      {
        Id = patientToEdit.Id,
        Name = patientToEdit.Name,
        ResidentRegistrationNumber = patientToEdit.ResidentRegistrationNumber,
        Address = patientToEdit.Address,
        Contact = patientToEdit.Contact
      };
      DataContext = _editablePatient;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrWhiteSpace(_editablePatient.Name) ||
          string.IsNullOrWhiteSpace(_editablePatient.ResidentRegistrationNumber) ||
          string.IsNullOrWhiteSpace(_editablePatient.Address) ||
          string.IsNullOrWhiteSpace(_editablePatient.Contact))
      {
        MessageBox.Show(this, "모든 필드를 입력하세요.", "검증 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
        return;
      }

      EditedPatient = _editablePatient;
      DialogResult = true;
      Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      DialogResult = false;
      Close();
    }
  }
}


