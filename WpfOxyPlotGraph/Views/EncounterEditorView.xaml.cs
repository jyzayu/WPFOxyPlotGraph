using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfOxyPlotGraph.Models;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
	public partial class EncounterEditorView : Page
	{
		public EncounterEditorView()
		{
			InitializeComponent();
			DataContext = new EncounterEditorViewModel();
		}

		private EncounterEditorViewModel? Vm => DataContext as EncounterEditorViewModel;

		private void OnAddAttachmentClick(object sender, RoutedEventArgs e)
		{
			if (Vm == null) return;
			var dlg = new OpenFileDialog
			{
				Filter = "이미지/PDF|*.png;*.jpg;*.jpeg;*.gif;*.pdf|모든 파일|*.*",
				Multiselect = false
			};
			if (dlg.ShowDialog() == true)
			{
				if (Vm.AddAttachmentCommand.CanExecute(dlg.FileName))
				{
					Vm.AddAttachmentCommand.Execute(dlg.FileName);
				}
			}
		}

		private void OnOpenAttachmentClick(object sender, RoutedEventArgs e)
		{
			if (Vm == null) return;
			if (AttachmentsGrid.SelectedItem is EncounterAttachment a)
			{
				if (Vm.PreviewAttachmentCommand.CanExecute(a.Id))
				{
					Vm.PreviewAttachmentCommand.Execute(a.Id);
				}
			}
		}

		private void OnDeleteAttachmentClick(object sender, RoutedEventArgs e)
		{
			if (Vm == null) return;
			if (AttachmentsGrid.SelectedItem is EncounterAttachment a)
			{
				if (MessageBox.Show($"삭제하시겠습니까?\r\n{a.OriginalFileName}", "확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					if (Vm.DeleteAttachmentCommand.CanExecute(a.Id))
					{
						Vm.DeleteAttachmentCommand.Execute(a.Id);
					}
				}
			}
		}
	}
}


