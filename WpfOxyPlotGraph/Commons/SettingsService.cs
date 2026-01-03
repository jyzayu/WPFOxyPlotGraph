using System;
using System.IO;
using System.Printing;
using System.Text.Json;
using WpfOxyPlotGraph.Models.Settings;

namespace WpfOxyPlotGraph.Commons
{
	public class SettingsService
	{
		private readonly string _settingsDir;
		private readonly string _settingsFile;
		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true
		};

		public SettingsService()
		{
			_settingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WpfOxyPlotGraph");
			Directory.CreateDirectory(_settingsDir);
			_settingsFile = Path.Combine(_settingsDir, "appsettings.json");
		}

		public AppSettings Get()
		{
			try
			{
				if (File.Exists(_settingsFile))
				{
					var json = File.ReadAllText(_settingsFile);
					var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
					if (loaded != null) return loaded;
				}
			}
			catch
			{
				// ignore and fallback to defaults
			}
			return new AppSettings();
		}

		public void Save(AppSettings settings)
		{
			if (settings == null) return;
			var json = JsonSerializer.Serialize(settings, JsonOptions);
			File.WriteAllText(_settingsFile, json);
		}

		public string[] GetInstalledPrinters()
		{
			try
			{
				using var server = new LocalPrintServer();
				var queues = server.GetPrintQueues();
				var names = new System.Collections.Generic.List<string>();
				foreach (var q in queues)
				{
					names.Add(q.FullName);
				}
				return names.ToArray();
			}
			catch
			{
				return Array.Empty<string>();
			}
		}
	}
}


