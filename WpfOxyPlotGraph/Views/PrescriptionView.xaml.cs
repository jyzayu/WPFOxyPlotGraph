using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
    public partial class PrescriptionView : Page
    {
        public PrescriptionView()
        {
            InitializeComponent();
            DataContext = new PrescriptionViewModel();
        }
    }
}



