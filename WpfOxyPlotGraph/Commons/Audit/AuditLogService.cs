using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace WpfOxyPlotGraph.Commons.Audit
{
	public class AuditLogService
	{
		private readonly string _logDirectory;
		private readonly string _logFilePath;
		private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			WriteIndented = false
		};

		public AuditLogService()
		{
			_logDirectory = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"WpfOxyPlotGraph",
				"logs");
			Directory.CreateDirectory(_logDirectory);
			_logFilePath = Path.Combine(_logDirectory, "audit.log.jsonl");
		}

		public async Task LogAsync(AuditEvent evt, CancellationToken cancellationToken = default)
		{
			if (evt == null) return;
			if (string.IsNullOrWhiteSpace(evt.User))
			{
				evt.User = GetDefaultUser();
			}
			var line = JsonSerializer.Serialize(evt, JsonOptions) + Environment.NewLine;
			await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
			try
			{
				await File.AppendAllTextAsync(_logFilePath, line, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
			}
			finally
			{
				_writeLock.Release();
			}
		}

		public IEnumerable<AuditEvent> ReadAll(Func<AuditEvent, bool>? predicate = null)
		{
			if (!File.Exists(_logFilePath))
			{
				yield break;
			}
			foreach (var line in File.ReadLines(_logFilePath, Encoding.UTF8))
			{
				if (string.IsNullOrWhiteSpace(line)) continue;
				AuditEvent? evt = null;
				try
				{
					evt = JsonSerializer.Deserialize<AuditEvent>(line, JsonOptions);
				}
				catch
				{
					// skip malformed lines
				}
				if (evt == null) continue;
				if (predicate == null || predicate(evt))
				{
					yield return evt;
				}
			}
		}

		public int ExportCsv(string destinationFilePath, Func<AuditEvent, bool>? predicate = null)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath) ?? _logDirectory);
			var rows = ReadAll(predicate);
			var sb = new StringBuilder();
			sb.AppendLine("Timestamp,User,Action,PatientId,PatientName,Success,ErrorMessage,Details");
			foreach (var e in rows)
			{
				sb.Append(EscapeCsv(e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")));
				sb.Append(',').Append(EscapeCsv(e.User));
				sb.Append(',').Append(EscapeCsv(e.Action));
				sb.Append(',').Append(e.PatientId?.ToString() ?? string.Empty);
				sb.Append(',').Append(EscapeCsv(e.PatientName ?? string.Empty));
				sb.Append(',').Append(e.Success ? "true" : "false");
				sb.Append(',').Append(EscapeCsv(e.ErrorMessage ?? string.Empty));
				sb.Append(',').Append(EscapeCsv(e.Details ?? string.Empty));
				sb.AppendLine();
			}
			File.WriteAllText(destinationFilePath, sb.ToString(), Encoding.UTF8);
			// return exported row count (excluding header)
			return ReadAll(predicate).Count();
		}

		public static string GetDefaultUser()
		{
			try
			{
				return Environment.UserName ?? "unknown";
			}
			catch
			{
				return "unknown";
			}
		}

		private static string EscapeCsv(string value)
		{
			if (value == null) return string.Empty;
			var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
			if (!needsQuotes)
			{
				return value;
			}
			var escaped = value.Replace("\"", "\"\"");
			return $"\"{escaped}\"";
		}
	}
}


