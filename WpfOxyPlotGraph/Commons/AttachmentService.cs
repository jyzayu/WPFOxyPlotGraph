using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Windows.Media.Imaging;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Commons
{
	public class AttachmentService
	{
		private const long DefaultMaxFileSizeBytes = 25L * 1024 * 1024; // 25 MB
		private static readonly string[] AllowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".pdf" };

		private readonly string _root;
		private readonly long _maxFileSizeBytes;

		public AttachmentService(string? rootFolder = null, long? maxFileSizeBytes = null)
		{
			_root = rootFolder ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WpfOxyPlotGraph", "attachments");
			_maxFileSizeBytes = maxFileSizeBytes ?? DefaultMaxFileSizeBytes;
			Directory.CreateDirectory(_root);
		}

		public IReadOnlyList<EncounterAttachment> List(int patientId, int encounterId)
		{
			var metaPath = GetMetadataPath(patientId, encounterId);
			if (!File.Exists(metaPath)) return Array.Empty<EncounterAttachment>();
			try
			{
				var json = File.ReadAllText(metaPath);
				var list = JsonSerializer.Deserialize<List<EncounterAttachment>>(json) ?? new List<EncounterAttachment>();
				return list.OrderByDescending(a => a.CreatedAt).ToList();
			}
			catch
			{
				return Array.Empty<EncounterAttachment>();
			}
		}

		public EncounterAttachment Add(int patientId, int encounterId, string filePath)
		{
			if (!File.Exists(filePath)) throw new FileNotFoundException("File not found.", filePath);
			var fileInfo = new FileInfo(filePath);
			if (fileInfo.Length > _maxFileSizeBytes) throw new InvalidOperationException($"파일이 너무 큽니다. 최대 {_maxFileSizeBytes / (1024 * 1024)}MB");
			var ext = Path.GetExtension(fileInfo.Name).ToLowerInvariant();
			if (!AllowedExtensions.Contains(ext)) throw new InvalidOperationException($"허용되지 않는 확장자입니다: {ext}");

			byte[] bytes = File.ReadAllBytes(filePath);
			var hash = ComputeSha256Hex(bytes);
			var safeOriginal = SanitizeFileName(Path.GetFileNameWithoutExtension(fileInfo.Name));
			var storedName = $"{safeOriginal}_{hash}{ext}";

			var folder = GetEncounterFolder(patientId, encounterId);
			Directory.CreateDirectory(folder);
			var destPath = Path.Combine(folder, storedName);
			if (!File.Exists(destPath)) File.Copy(filePath, destPath);

			var attachment = new EncounterAttachment
			{
				Id = hash,
				PatientId = patientId,
				EncounterId = encounterId,
				OriginalFileName = fileInfo.Name,
				StoredFileName = storedName,
				ContentType = GuessContentType(ext),
				FileSizeBytes = fileInfo.Length,
				CreatedAt = DateTime.Now
			};

			UpsertMetadata(patientId, encounterId, attachment);
			return attachment;
		}

		public bool Delete(int patientId, int encounterId, string attachmentId)
		{
			var list = List(patientId, encounterId).ToList();
			var target = list.FirstOrDefault(a => string.Equals(a.Id, attachmentId, StringComparison.OrdinalIgnoreCase));
			if (target == null) return false;

			// Remove file
			var filePath = Path.Combine(GetEncounterFolder(patientId, encounterId), target.StoredFileName);
			try { if (File.Exists(filePath)) File.Delete(filePath); } catch { /* ignore */ }

			// Update metadata
			list.RemoveAll(a => string.Equals(a.Id, attachmentId, StringComparison.OrdinalIgnoreCase));
			WriteMetadata(patientId, encounterId, list);
			return true;
		}

		public void OpenWithDefaultApp(int patientId, int encounterId, string attachmentId)
		{
			var list = List(patientId, encounterId);
			var target = list.FirstOrDefault(a => string.Equals(a.Id, attachmentId, StringComparison.OrdinalIgnoreCase));
			if (target == null) return;
			var path = Path.Combine(GetEncounterFolder(patientId, encounterId), target.StoredFileName);
			if (!File.Exists(path)) return;
			try
			{
				var psi = new ProcessStartInfo
				{
					FileName = path,
					UseShellExecute = true
				};
				Process.Start(psi);
			}
			catch
			{
				// ignore
			}
		}

		public BitmapSource? TryLoadImagePreview(int patientId, int encounterId, string attachmentId, int decodePixelWidth = 640)
		{
			var list = List(patientId, encounterId);
			var target = list.FirstOrDefault(a => string.Equals(a.Id, attachmentId, StringComparison.OrdinalIgnoreCase));
			if (target == null) return null;
			var path = Path.Combine(GetEncounterFolder(patientId, encounterId), target.StoredFileName);
			if (!File.Exists(path)) return null;
			var ext = Path.GetExtension(path).ToLowerInvariant();
			if (!(ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif")) return null;
			try
			{
				var bmp = new BitmapImage();
				using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
				bmp.BeginInit();
				bmp.CacheOption = BitmapCacheOption.OnLoad;
				bmp.DecodePixelWidth = decodePixelWidth;
				bmp.StreamSource = fs;
				bmp.EndInit();
				bmp.Freeze();
				return bmp;
			}
			catch
			{
				return null;
			}
		}

		private void UpsertMetadata(int patientId, int encounterId, EncounterAttachment attachment)
		{
			var list = List(patientId, encounterId).ToList();
			var idx = list.FindIndex(a => string.Equals(a.Id, attachment.Id, StringComparison.OrdinalIgnoreCase));
			if (idx >= 0) list[idx] = attachment;
			else list.Add(attachment);
			WriteMetadata(patientId, encounterId, list);
		}

		private void WriteMetadata(int patientId, int encounterId, List<EncounterAttachment> list)
		{
			var metaPath = GetMetadataPath(patientId, encounterId);
			Directory.CreateDirectory(Path.GetDirectoryName(metaPath)!);
			var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText(metaPath, json);
		}

		private string GetEncounterFolder(int patientId, int encounterId)
		{
			return Path.Combine(_root, patientId.ToString(), encounterId.ToString());
		}

		private string GetMetadataPath(int patientId, int encounterId)
		{
			return Path.Combine(GetEncounterFolder(patientId, encounterId), "attachments.json");
		}

		private static string GuessContentType(string ext)
		{
			return ext switch
			{
				".png" => "image/png",
				".jpg" => "image/jpeg",
				".jpeg" => "image/jpeg",
				".gif" => "image/gif",
				".pdf" => "application/pdf",
				_ => "application/octet-stream"
			};
		}

		private static string ComputeSha256Hex(byte[] content)
		{
			using var sha = SHA256.Create();
			var hash = sha.ComputeHash(content);
			return string.Concat(hash.Select(b => b.ToString("x2")));
		}

		private static string SanitizeFileName(string name)
		{
			var invalid = Path.GetInvalidFileNameChars();
			var cleaned = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
			if (string.IsNullOrWhiteSpace(cleaned)) cleaned = "file";
			// trim to reasonable length
			if (cleaned.Length > 80) cleaned = cleaned.Substring(0, 80);
			return cleaned;
		}
	}
}


