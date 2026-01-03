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
using WpfOxyPlotGraph.Commons.Auth;

namespace WpfOxyPlotGraph.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // MainWindow.xaml.cs - 호스트 Window
    public partial class MainWindow : Window
    {
        private readonly INavigationService navigationService;
        public MainWindow()
        {
            InitializeComponent();

            var nav = WpfOxyPlotGraph.App.Services.GetService(typeof(INavigationService)) as INavigationService;
            if (nav is NavigationService navImpl)
            {
                navImpl.MainFrame = MainFrame;
            }
            navigationService = nav ?? new NavigationService { MainFrame = MainFrame };

            DataContext = new MainViewModel(navigationService);

            // MainWindow 생성 직후 MainPage로 네비게이션
            var auth = WpfOxyPlotGraph.App.Services.GetService(typeof(AuthService)) as AuthService;
            if (auth != null && auth.CurrentUser != null)
            {
                navigationService.NavigateTo<PatientsListView>();
            }
            else
            {
                navigationService.NavigateTo<LoginView>();
            }
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
