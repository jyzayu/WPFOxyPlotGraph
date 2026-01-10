using System;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfOxyPlotGraph.Models;
using FontFamily = System.Windows.Media.FontFamily;
using Orientation = System.Windows.Controls.Orientation;

namespace WpfOxyPlotGraph.Commons.Printing
{
	public class PrintService
	{
		private readonly DocumentTemplateService _templates = new DocumentTemplateService();

		public FlowDocument BuildEncounterSummaryDocument(Patient patient, Encounter encounter, string? logoImagePath = null, string? signatureImagePath = null)
		{
			var doc = CreateBaseDocument();
			var header = BuildHeader(logoImagePath, "진료 요약");
			doc.Blocks.Add(header);

			var body = new Paragraph();
			body.Inlines.Add(new Bold(new Run("환자: ")));
			body.Inlines.Add(new Run(patient?.Name ?? string.Empty));
			body.Inlines.Add(new LineBreak());
			body.Inlines.Add(new Bold(new Run("내원일시: ")));
			body.Inlines.Add(new Run(encounter?.VisitAt.ToString("yyyy-MM-dd HH:mm") ?? string.Empty));
			body.Inlines.Add(new LineBreak());
			body.Inlines.Add(new Bold(new Run("진단: ")));
			body.Inlines.Add(new Run(encounter?.Diagnosis ?? string.Empty));
			body.Inlines.Add(new LineBreak());
			body.Inlines.Add(new Bold(new Run("메모: ")));
			body.Inlines.Add(new Run(encounter?.Notes ?? string.Empty));
			doc.Blocks.Add(body);

			if (!string.IsNullOrWhiteSpace(signatureImagePath) && File.Exists(signatureImagePath))
			{
				doc.Blocks.Add(BuildSignature(signatureImagePath));
			}

			return doc;
		}

		public FlowDocument BuildPrescriptionDocument(Patient patient, Encounter encounter, string? logoImagePath = null, string? signatureImagePath = null)
		{
			var doc = CreateBaseDocument();
			doc.Blocks.Add(BuildHeader(logoImagePath, "처방전"));
			var content = _templates.ApplyEncounterTemplate(DocumentTemplateType.Prescription, patient, encounter);
			doc.Blocks.Add(new Paragraph(new Run(content)));
			if (!string.IsNullOrWhiteSpace(signatureImagePath) && File.Exists(signatureImagePath))
			{
				doc.Blocks.Add(BuildSignature(signatureImagePath));
			}
			return doc;
		}

		public FlowDocument BuildLabOrderDocument(Patient patient, Encounter encounter, string? logoImagePath = null, string? signatureImagePath = null)
		{
			var doc = CreateBaseDocument();
			doc.Blocks.Add(BuildHeader(logoImagePath, "검사의뢰서"));
			var content = _templates.ApplyEncounterTemplate(DocumentTemplateType.LabOrder, patient, encounter);
			doc.Blocks.Add(new Paragraph(new Run(content)));
			if (!string.IsNullOrWhiteSpace(signatureImagePath) && File.Exists(signatureImagePath))
			{
				doc.Blocks.Add(BuildSignature(signatureImagePath));
			}
			return doc;
		}

		public void ShowPrintPreview(FlowDocument document)
		{
			var window = new WpfOxyPlotGraph.Views.PrintPreviewWindow
			{
				Owner = System.Windows.Application.Current?.MainWindow,
			};
			window.SetDocument(document);
			window.ShowDialog();
		}

		public void Print(FlowDocument document, string? printerName = null)
		{
			var pd = new System.Windows.Controls.PrintDialog();
			if (!string.IsNullOrWhiteSpace(printerName))
			{
				try
				{
					var server = new LocalPrintServer();
					var queue = server.GetPrintQueue(printerName);
					pd.PrintQueue = queue;
				}
				catch
				{
					// fallback to default
				}
			}

			// Let user confirm printer and settings
			var ok = pd.ShowDialog();
			if (ok == true)
			{
				document.PageHeight = pd.PrintableAreaHeight;
				document.PageWidth = pd.PrintableAreaWidth;
				document.ColumnWidth = pd.PrintableAreaWidth;
				pd.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, "문서 출력");
			}
		}

		private static FlowDocument CreateBaseDocument()
		{
			return new FlowDocument
			{
				PagePadding = new Thickness(50),
				FontFamily = new FontFamily("Malgun Gothic"),
				FontSize = 14,
				TextAlignment = TextAlignment.Left
			};
		}

		private static BlockUIContainer BuildHeader(string? logoPath, string title)
		{
			var stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 20) };
			if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
			{
				try
				{
					var img = new System.Windows.Controls.Image
					{
						Source = new BitmapImage(new Uri(logoPath, UriKind.Absolute)),
						Height = 48,
						Margin = new Thickness(0, 0, 12, 0)
					};
					stack.Children.Add(img);
				}
				catch
				{
					// ignore bad logo
				}
			}
			var tb = new TextBlock
			{
				Text = title,
				FontSize = 24,
				FontWeight = FontWeights.Bold,
				VerticalAlignment = VerticalAlignment.Center
			};
			stack.Children.Add(tb);
			return new BlockUIContainer(stack);
		}

		private static BlockUIContainer BuildSignature(string signaturePath)
		{
			var label = new TextBlock
			{
				Text = "서명",
				FontWeight = FontWeights.Bold,
				Margin = new Thickness(0, 30, 0, 8)
			};
			var img = new System.Windows.Controls.Image
			{
				Height = 60,
				Stretch = Stretch.Uniform
			};
			try
			{
				img.Source = new BitmapImage(new Uri(signaturePath, UriKind.Absolute));
			}
			catch
			{
				// ignore
			}
			var panel = new StackPanel { Orientation = Orientation.Vertical };
			panel.Children.Add(label);
			panel.Children.Add(img);
			return new BlockUIContainer(panel);
		}
	}
}


