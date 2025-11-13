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
using System.Windows.Shapes;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.ViewModels;

namespace WpfOxyPlotGraph.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // MainWindow.xaml.cs - 호스트 Window
    public partial class MainWindow : Window
    {
        NavigationService navigationService = new NavigationService();
        public MainWindow()
        {
            InitializeComponent();

            navigationService.MainFrame = MainFrame;

            DataContext = new MainViewModel(navigationService);

            // MainWindow 생성 직후 MainPage로 네비게이션
            navigationService.NavigateTo<MainPage>();
        }

        public void Home_Click(object sender, RoutedEventArgs e)
        {
            navigationService.NavigateTo<MainPage>();
        }

        public void InputScore_Click(object sender, RoutedEventArgs e)
        {
            navigationService.NavigateTo<InputScoreView>(); 
        }
    }
}
