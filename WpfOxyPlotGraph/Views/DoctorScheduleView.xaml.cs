using System.Windows.Controls;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
    public partial class DoctorScheduleView : Page
    {
        public DoctorScheduleView()
        {
            InitializeComponent();
            DataContext = new DoctorScheduleViewModel();
        }
    }
}


