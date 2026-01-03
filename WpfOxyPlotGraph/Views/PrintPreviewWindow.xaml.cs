using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;

namespace WpfOxyPlotGraph.Views
{
	public partial class PrintPreviewWindow : Window
	{
		private FlowDocument? _document;

		public PrintPreviewWindow()
		{
			InitializeComponent();
		}

		public void SetDocument(FlowDocument document)
		{
			_document = document;
			Viewer.Document = _document;
			StatusText.Text = "문서 페이지: " + ((IDocumentPaginatorSource)_document).DocumentPaginator.PageCount;
		}

		private void BtnPrint_Click(object sender, RoutedEventArgs e)
		{
			if (_document == null) return;
			var dlg = new System.Windows.Controls.PrintDialog();
			if (dlg.ShowDialog() == true)
			{
				_document.PageHeight = dlg.PrintableAreaHeight;
				_document.PageWidth = dlg.PrintableAreaWidth;
				_document.ColumnWidth = dlg.PrintableAreaWidth;
				dlg.PrintDocument(((IDocumentPaginatorSource)_document).DocumentPaginator, "문서 출력");
			}
		}

		private void BtnClose_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}


