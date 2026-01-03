using System;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace WpfOxyPlotGraph.Commons
{
	public class NotificationService
	{
		public void ShowInfo(string message, string? title = null)
		{
			MessageBox.Show(message, title ?? "알림", MessageBoxButton.OK, MessageBoxImage.Information);
		}

		public void ShowError(string userMessage, Exception? ex = null, Action? retry = null)
		{
			var text = userMessage;
			if (ex != null)
			{
				text += "\r\n\r\n자세한 오류:\r\n" + ex.Message;
			}
			if (retry == null)
			{
				MessageBox.Show(text, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}
			var result = MessageBox.Show(text + "\r\n\r\n다시 시도하시겠습니까?", "오류", MessageBoxButton.YesNo, MessageBoxImage.Error);
			if (result == MessageBoxResult.Yes)
			{
				try { retry(); }
				catch (Exception rex)
				{
					MessageBox.Show("재시도 중 오류: " + rex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}
	}
}


