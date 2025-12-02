using System;
using System.Net;
using System.Net.Mail;
using WpfOxyPlotGraph.Models;

namespace WpfOxyPlotGraph.Commons
{
	public class EmailNotificationService
	{
		private static string GetEnv(string key, string? defaultValue = null)
		{
			var value = Environment.GetEnvironmentVariable(key);
			return string.IsNullOrWhiteSpace(value) ? (defaultValue ?? string.Empty) : value.Trim();
		}

		public bool TrySendAutoOrderNotification(Medication medication, int purchaseOrderId, int afterStock)
		{
			// Configuration via environment variables for safety
			// Required:
			//   GMAIL_USER           -> your Gmail address (sender)
			//   GMAIL_APP_PASSWORD   -> Gmail App Password (with 2FA enabled)
			// Optional:
			//   GMAIL_TO             -> recipient (defaults to GMAIL_USER)
			//   GMAIL_SMTP_HOST      -> default: smtp.gmail.com
			//   GMAIL_SMTP_PORT      -> default: 587
			var gmailUser = GetEnv("GMAIL_USER");
			var gmailAppPassword = GetEnv("GMAIL_APP_PASSWORD");
			if (string.IsNullOrWhiteSpace(gmailUser) || string.IsNullOrWhiteSpace(gmailAppPassword))
			{
				// Not configured; skip silently
				return false;
			}

			var recipient = GetEnv("GMAIL_TO", gmailUser);
			var smtpHost = GetEnv("GMAIL_SMTP_HOST", "smtp.gmail.com");
			var smtpPortRaw = GetEnv("GMAIL_SMTP_PORT", "587");
			int.TryParse(smtpPortRaw, out var smtpPort);
			if (smtpPort <= 0) smtpPort = 587;

			var subject = $"자동 발주 생성 알림: {medication.Name} (PO #{purchaseOrderId})";
			var body =
				$"자동 발주가 생성되었습니다.\r\n\r\n" +
				$"- 발주 번호: #{purchaseOrderId}\r\n" +
				$"- 품목: {medication.Name}\r\n" +
				$"- SKU: {medication.Sku}\r\n" +
				$"- 처방 후 재고: {afterStock}\r\n" +
				$"- 발주 수량(설정): {medication.ReorderQuantity}\r\n" +
				$"- 발주점: {medication.ReorderPoint}, 최소재고: {medication.MinimumStock}\r\n" +
				$"- 발생 시각: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";

			try
			{
				using var message = new MailMessage(gmailUser, recipient, subject, body);
				using var client = new SmtpClient(smtpHost, smtpPort)
				{
					EnableSsl = true,
					Credentials = new NetworkCredential(gmailUser, gmailAppPassword),
					DeliveryMethod = SmtpDeliveryMethod.Network
				};
				client.Send(message);
				return true;
			}
			catch
			{
				// Ignore send failures to avoid breaking business flow
				return false;
			}
		}
	}
}


