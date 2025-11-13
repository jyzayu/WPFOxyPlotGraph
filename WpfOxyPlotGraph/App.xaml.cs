using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.ViewModels;
using WpfOxyPlotGraph.Views;

namespace WpfOxyPlotGraph
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<INavigationService>(provider =>
            {
                var mainWindow = provider.GetService<MainWindow>();
                NavigationService navigationService = new NavigationService();
                navigationService.MainFrame = mainWindow.MainFrame;
                return navigationService;
            });

            services.AddTransient<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }


}


