using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Commons.Audit;
using WpfOxyPlotGraph.Commons.Auth;
using WpfOxyPlotGraph.ViewModels;
using WpfOxyPlotGraph.Views;

namespace WpfOxyPlotGraph
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private ServiceProvider _serviceProvider;
		public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
			Services = _serviceProvider;

			// Start background services
			var backup = _serviceProvider.GetService<BackupService>();
			if (backup != null)
			{
				var dest = System.IO.Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"WpfOxyPlotGraph",
					"backups");
				backup.StartAutoBackupDailyAt(3, dest, encrypt: true, purpose: "daily");
			}

			var audit = _serviceProvider.GetService<AuditLogService>();
			if (audit != null)
			{
				_ = audit.LogAsync(new Commons.Audit.AuditEvent
				{
					Action = "AppStart",
					Details = "Application started",
					User = Commons.Audit.AuditLogService.GetDefaultUser(),
					Success = true
				});
			}

            var mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow?.Show();
        }

		protected override void OnExit(ExitEventArgs e)
		{
			try
			{
				var audit = _serviceProvider?.GetService<AuditLogService>();
				if (audit != null)
				{
					_ = audit.LogAsync(new Commons.Audit.AuditEvent
					{
						Action = "AppExit",
						Details = "Application exiting",
						User = Commons.Audit.AuditLogService.GetDefaultUser(),
						Success = true
					});
				}
				var backup = _serviceProvider?.GetService<BackupService>();
				backup?.Dispose();
			}
			catch { }
			base.OnExit(e);
		}

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<AuditLogService>();
            services.AddSingleton<SettingsService>();
			services.AddSingleton<BackupService>(provider =>
			{
				var settings = provider.GetRequiredService<SettingsService>();
				var notifier = provider.GetService<NotificationService>();
				var audit = provider.GetService<AuditLogService>();
				return new BackupService(settings, notifier, audit);
			});
			services.AddSingleton<NotificationService>();
			services.AddSingleton<AuthService>();
            services.AddSingleton<INavigationService, NavigationService>();

            services.AddTransient<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }


}


