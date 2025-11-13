using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
    /// <summary>
    /// Interaction logic for InputScoreView.xaml
    /// </summary>
    public partial class InputScoreView : Page
    {
        public InputScoreView()
        {
            InitializeComponent();
            DataContext = new InputScoreViewModel();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
