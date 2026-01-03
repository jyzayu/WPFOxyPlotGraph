using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfOxyPlotGraph.Models.Settings;
using Timer = System.Threading.Timer;

namespace WpfOxyPlotGraph.Commons
{
	public class BackupService : IDisposable
	{
		private readonly SettingsService _settingsService;
		private readonly NotificationService? _notificationService;
		private readonly Commons.Audit.AuditLogService? _auditLogService;
		private readonly string _appDataDir;
		private Timer _timer;
		private bool _isRunning;

		public BackupService(SettingsService settingsService, NotificationService? notificationService = null, Commons.Audit.AuditLogService? auditLogService = null)
		{
			_settingsService = settingsService;
			_notificationService = notificationService;
			_auditLogService = auditLogService;
			_appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WpfOxyPlotGraph");
			Directory.CreateDirectory(_appDataDir);
		}

		public string CreateBackup(string destinationDirectory, bool encrypt, string? purpose = null)
		{
			Directory.CreateDirectory(destinationDirectory);
			var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			var zipPath = Path.Combine(destinationDirectory, $"backup_{timestamp}.zip");

			var toInclude = GetPathsToInclude();
			using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
			{
				foreach (var p in toInclude)
				{
					if (File.Exists(p))
					{
						zip.CreateEntryFromFile(p, Path.GetFileName(p));
					}
					else if (Directory.Exists(p))
					{
						AddDirectoryToZip(zip, p, Path.GetFileName(p));
					}
				}
			}

			if (encrypt)
			{
				var bytes = File.ReadAllBytes(zipPath);
				var entropy = BuildEntropy(purpose ?? "backup");
				var protectedBytes = ProtectedData.Protect(bytes, entropy, DataProtectionScope.LocalMachine);
				var encPath = Path.ChangeExtension(zipPath, ".zip.dpapi");
				File.WriteAllBytes(encPath, protectedBytes);
				File.Delete(zipPath);
				return encPath;
			}
			return zipPath;
		}

		public RestoreSimulationResult SimulateRestore(string sourcePath, string? purpose = null)
		{
			var zipPath = PrepareZipForRestore(sourcePath, purpose);
			using var zip = ZipFile.OpenRead(zipPath);
			var result = new RestoreSimulationResult();
			foreach (var entry in zip.Entries)
			{
				var targetPath = Path.Combine(_appDataDir, entry.FullName);
				if (File.Exists(targetPath))
				{
					result.WillOverwrite.Add(targetPath);
				}
				else
				{
					result.WillCreate.Add(targetPath);
				}
			}
			return result;
		}

		public void Restore(string sourcePath, string? purpose = null)
		{
			var zipPath = PrepareZipForRestore(sourcePath, purpose);
			using var zip = ZipFile.OpenRead(zipPath);
			foreach (var entry in zip.Entries)
			{
				var targetPath = Path.Combine(_appDataDir, entry.FullName);
				Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? _appDataDir);
				entry.ExtractToFile(targetPath, overwrite: true);
			}
			if (zipPath.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
			{
				try { File.Delete(zipPath); } catch { }
			}
		}

		public void StartAutoBackupDailyAt(int hour24, string destinationDirectory, bool encrypt, string? purpose = null)
		{
			StopAutoBackup();
			_isRunning = true;
			_timer = new Timer(_ =>
			{
				if (!_isRunning) return;
				try
				{
					var now = DateTime.Now;
					if (now.Hour == hour24)
					{
						CreateBackup(destinationDirectory, encrypt, purpose);
						if (_auditLogService != null)
						{
							_ = _auditLogService.LogAsync(new Commons.Audit.AuditEvent
							{
								Action = "AutoBackup",
								Details = $"Auto backup completed to {destinationDirectory}",
								User = Commons.Audit.AuditLogService.GetDefaultUser(),
								Success = true
							});
						}
					}
				}
				catch (Exception ex)
				{
					if (_auditLogService != null)
					{
						_ = _auditLogService.LogAsync(new Commons.Audit.AuditEvent
						{
							Action = "AutoBackup",
							Details = "Auto backup failed",
							User = Commons.Audit.AuditLogService.GetDefaultUser(),
							Success = false,
							ErrorMessage = ex.Message
						});
					}
					// avoid blocking UI with message boxes on background timer
				}
			}, null, TimeSpan.Zero, TimeSpan.FromHours(1));
		}

		public void StopAutoBackup()
		{
			_isRunning = false;
			_timer?.Dispose();
			_timer = null;
		}

		private string PrepareZipForRestore(string sourcePath, string? purpose)
		{
			if (sourcePath.EndsWith(".dpapi", StringComparison.OrdinalIgnoreCase))
			{
				var protectedBytes = File.ReadAllBytes(sourcePath);
				var entropy = BuildEntropy(purpose ?? "backup");
				var bytes = ProtectedData.Unprotect(protectedBytes, entropy, DataProtectionScope.LocalMachine);
				var tempZip = Path.Combine(Path.GetTempPath(), $"restore_{Guid.NewGuid():N}.zip");
				File.WriteAllBytes(tempZip, bytes);
				return tempZip;
			}
			return sourcePath;
		}

		private List<string> GetPathsToInclude()
		{
			var list = new List<string>
			{
				Path.Combine(_appDataDir, "appsettings.json"),
				Path.Combine(_appDataDir, "logs")
			};
			var settings = _settingsService.Get();
			if (!string.IsNullOrWhiteSpace(settings.DocumentTemplatesPath) && Directory.Exists(settings.DocumentTemplatesPath))
			{
				list.Add(settings.DocumentTemplatesPath);
			}
			return list;
		}

		private static void AddDirectoryToZip(ZipArchive zip, string directoryPath, string baseInZip)
		{
			foreach (var file in Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories))
			{
				var rel = Path.GetRelativePath(directoryPath, file);
				var entryPath = Path.Combine(baseInZip, rel).Replace('\\', '/');
				zip.CreateEntryFromFile(file, entryPath);
			}
		}

		private static byte[] BuildEntropy(string purpose)
		{
			var prefix = Encoding.UTF8.GetBytes("WpfOxyPlotGraph:backup:");
			var p = Encoding.UTF8.GetBytes(purpose ?? string.Empty);
			var combined = new byte[prefix.Length + p.Length];
			Buffer.BlockCopy(prefix, 0, combined, 0, prefix.Length);
			Buffer.BlockCopy(p, 0, combined, prefix.Length, p.Length);
			return combined;
		}

		public void Dispose()
		{
			StopAutoBackup();
		}
	}

	public class RestoreSimulationResult
	{
		public System.Collections.Generic.List<string> WillOverwrite { get; } = new System.Collections.Generic.List<string>();
		public System.Collections.Generic.List<string> WillCreate { get; } = new System.Collections.Generic.List<string>();
	}
}


