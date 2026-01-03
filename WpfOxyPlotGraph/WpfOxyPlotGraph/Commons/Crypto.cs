using System;
using System.Security.Cryptography;
using System.Text;

namespace WpfOxyPlotGraph.Commons
{
	public static class Crypto
	{
		private static readonly byte[] AppScopeEntropy = Encoding.UTF8.GetBytes("WpfOxyPlotGraph:v1");

		public static string EncryptString(string plaintext, string purpose)
		{
			if (plaintext == null)
			{
				return string.Empty;
			}
			var plainBytes = Encoding.UTF8.GetBytes(plaintext);
			var entropy = BuildEntropy(purpose);
			var cipherBytes = ProtectedData.Protect(plainBytes, entropy, DataProtectionScope.LocalMachine);
			return Convert.ToBase64String(cipherBytes);
		}

		public static string DecryptString(string ciphertextBase64, string purpose)
		{
			if (string.IsNullOrWhiteSpace(ciphertextBase64))
			{
				return string.Empty;
			}
			var cipherBytes = Convert.FromBase64String(ciphertextBase64);
			var entropy = BuildEntropy(purpose);
			var plainBytes = ProtectedData.Unprotect(cipherBytes, entropy, DataProtectionScope.LocalMachine);
			return Encoding.UTF8.GetString(plainBytes);
		}

		// Lenient decrypt to ease migration: if value isn't valid base64/encrypted, return as-is
		public static string DecryptStringLenient(string value, string purpose)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			try
			{
				return DecryptString(value, purpose);
			}
			catch
			{
				return value;
			}
		}

		public static string ComputeSha256Hex(string input)
		{
			if (input == null)
			{
				return string.Empty;
			}
			using var sha = SHA256.Create();
			var bytes = Encoding.UTF8.GetBytes(input);
			var hash = sha.ComputeHash(bytes);
			var sb = new StringBuilder(hash.Length * 2);
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("x2"));
			}
			return sb.ToString();
		}

		public static string ComputeRrnHmacHex(string rrnPlain)
		{
			if (rrnPlain == null)
			{
				return string.Empty;
			}
			var keyB64 = Environment.GetEnvironmentVariable("WPF_OXY_HMAC_KEY");
			if (string.IsNullOrWhiteSpace(keyB64))
			{
				throw new InvalidOperationException("HMAC key not configured. Set environment variable 'WPF_OXY_HMAC_KEY' to a Base64-encoded 32-byte key.");
			}
			byte[] key;
			try
			{
				key = Convert.FromBase64String(keyB64);
			}
			catch (FormatException ex)
			{
				throw new InvalidOperationException("Invalid Base64 for 'WPF_OXY_HMAC_KEY'.", ex);
			}
			using var hmac = new HMACSHA256(key);
			var data = Encoding.UTF8.GetBytes(rrnPlain);
			var mac = hmac.ComputeHash(data);
			var sb = new StringBuilder(mac.Length * 2);
			for (int i = 0; i < mac.Length; i++)
			{
				sb.Append(mac[i].ToString("x2"));
			}
			return sb.ToString();
		}

		private static byte[] BuildEntropy(string purpose)
		{
			if (string.IsNullOrEmpty(purpose))
			{
				return AppScopeEntropy;
			}
			var purposeBytes = Encoding.UTF8.GetBytes(purpose);
			var combined = new byte[AppScopeEntropy.Length + purposeBytes.Length];
			Buffer.BlockCopy(AppScopeEntropy, 0, combined, 0, AppScopeEntropy.Length);
			Buffer.BlockCopy(purposeBytes, 0, combined, AppScopeEntropy.Length, purposeBytes.Length);
			return combined;
		}
	}
}


