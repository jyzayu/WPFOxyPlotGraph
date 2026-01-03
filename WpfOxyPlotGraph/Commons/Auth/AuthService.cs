using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WpfOxyPlotGraph.Commons.Audit;

namespace WpfOxyPlotGraph.Commons.Auth
{
	public class AuthService
	{
		private readonly string _usersFilePath;
		private readonly AuditLogService _audit;
		private List<UserRecord> _users = new List<UserRecord>();

		public AuthUser? CurrentUser { get; private set; }
		public event EventHandler? AuthStateChanged;

		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
		{
			WriteIndented = true
		};

		public AuthService(AuditLogService audit)
		{
			_audit = audit;
			var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WpfOxyPlotGraph");
			Directory.CreateDirectory(appDir);
			_usersFilePath = Path.Combine(appDir, "users.json");
			LoadOrSeed();
		}

		public bool TryLogin(string username, string password, out string errorMessage)
		{
			errorMessage = string.Empty;
			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				errorMessage = "아이디와 비밀번호를 입력하세요.";
				return false;
			}
			var user = _users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
			if (user == null)
			{
				errorMessage = "존재하지 않는 사용자입니다.";
				_ = _audit.LogAsync(new AuditEvent
				{
					Action = "Login",
					User = username,
					Details = "Unknown user",
					Success = false,
					ErrorMessage = errorMessage
				});
				return false;
			}
			var expected = user.PasswordHashHex;
			var actual = ComputePasswordHashHex(username, password, user.Salt);
			if (!string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
			{
				errorMessage = "비밀번호가 올바르지 않습니다.";
				_ = _audit.LogAsync(new AuditEvent
				{
					Action = "Login",
					User = username,
					Details = "Invalid password",
					Success = false,
					ErrorMessage = errorMessage
				});
				return false;
			}
			CurrentUser = new AuthUser
			{
				Username = user.Username,
				DisplayName = user.DisplayName,
				Roles = user.Roles
			};
			_ = _audit.LogAsync(new AuditEvent
			{
				Action = "Login",
				User = username,
				Details = "User logged in",
				Success = true
			});
			AuthStateChanged?.Invoke(this, EventArgs.Empty);
			return true;
		}

		public void Logout()
		{
			var prev = CurrentUser?.Username ?? string.Empty;
			CurrentUser = null;
			_ = _audit.LogAsync(new AuditEvent
			{
				Action = "Logout",
				User = prev,
				Details = "User logged out",
				Success = true
			});
			AuthStateChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool IsInRole(Role role)
		{
			return CurrentUser != null && CurrentUser.IsInRole(role);
		}

		private void LoadOrSeed()
		{
			try
			{
				if (File.Exists(_usersFilePath))
				{
					var json = File.ReadAllText(_usersFilePath);
					var loaded = JsonSerializer.Deserialize<List<UserRecord>>(json, JsonOptions);
					if (loaded != null && loaded.Count > 0)
					{
						_users = loaded;
						return;
					}
				}
			}
			catch
			{
				// ignore and seed
			}
			// Seed: admin / admin123!
			var salt = GenerateSalt();
			var admin = new UserRecord
			{
				Username = "admin",
				DisplayName = "관리자",
				Roles = Role.Admin | Role.Auditor | Role.Doctor | Role.Nurse | Role.FrontDesk,
				Salt = salt,
				PasswordHashHex = ComputePasswordHashHex("admin", "admin123!", salt),
				CreatedAt = DateTimeOffset.Now
			};
			_users = new List<UserRecord> { admin };
			Persist();
		}

		private void Persist()
		{
			try
			{
				var json = JsonSerializer.Serialize(_users, JsonOptions);
				File.WriteAllText(_usersFilePath, json);
			}
			catch
			{
				// ignore
			}
		}

		private static string GenerateSalt()
		{
			var bytes = RandomNumberGenerator.GetBytes(16);
			return Convert.ToBase64String(bytes);
		}

		private static string ComputePasswordHashHex(string username, string password, string salt)
		{
			var material = $"{username}:{password}:{salt}";
			using var sha = SHA256.Create();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(material));
			var sb = new StringBuilder(hash.Length * 2);
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("x2"));
			}
			return sb.ToString();
		}
	}
}


