using System;
using WpfOxyPlotGraph.Commons.Auth;

namespace WpfOxyPlotGraph.Commons.Auth
{
	public class UserRecord
	{
		public string Username { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public Role Roles { get; set; } = Role.None;
		public string PasswordHashHex { get; set; } = string.Empty;
		public string Salt { get; set; } = string.Empty;
		public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
	}

	public class AuthUser
	{
		public string Username { get; set; } = string.Empty;
		public string DisplayName { get; set; } = string.Empty;
		public Role Roles { get; set; } = Role.None;

		public bool IsInRole(Role role) => (Roles & role) == role;
	}
}


