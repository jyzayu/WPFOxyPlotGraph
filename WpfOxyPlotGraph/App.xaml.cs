using System.Configuration;
using System.Data;
using System.Windows;
using WpfOxyPlotGraph.ViewModels;
using WpfOxyPlotGraph.Views;

namespace WpfOxyPlotGraph
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var mainView = new MainView
            {
                DataContext = new MainViewModel()
            };

            mainView.Show();
        }
    }
}


