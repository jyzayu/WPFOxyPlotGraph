using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfOxyPlotGraph.Commons;
using WpfOxyPlotGraph.Realtime;

namespace WpfOxyPlotGraph.Realtime
{
    public sealed class PushClient : IDisposable
    {
        private readonly NotificationService _notifier;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;

        public PushClient(NotificationService notifier)
        {
            _notifier = notifier;
        }

        public async Task ConnectAsync(string host, int port, string doctorName, CancellationToken cancellationToken = default)
        {
            await DisconnectAsync();

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();

            // Simple handshake: identify doctor
            var authLine = $"AUTH|{doctorName}\n";
            var authBytes = Encoding.UTF8.GetBytes(authLine);
            await _stream.WriteAsync(authBytes, 0, authBytes.Length, _cts.Token);
            await _stream.FlushAsync(_cts.Token);

            _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            if (_stream == null) return;
            using var reader = new StreamReader(_stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);
            while (!cancellationToken.IsCancellationRequested)
            {
                string? line = null;
                try
                {
                    line = await reader.ReadLineAsync();
                }
                catch
                {
                    break;
                }
                if (string.IsNullOrEmpty(line)) continue;
                try
                {
                    var env = PushEnvelope.FromJson(line);
                    if (env != null && string.Equals(env.Type, "MRI_COMPLETED", StringComparison.OrdinalIgnoreCase))
                    {
                        var text = $"[{env.PatientId}] {env.PatientName} 환자의 MRI 촬영이 완료되었습니다.";
                        System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            _notifier.ShowInfo(text, "MRI 완료");
                        });
                    }
                }
                catch
                {
                    // ignore malformed
                }
            }
        }

        public async Task DisconnectAsync()
        {
            try { _cts?.Cancel(); } catch { }
            try { if (_stream != null) await _stream.DisposeAsync(); } catch { }
            try { _client?.Close(); } catch { }
            _stream = null;
            _client = null;
            _cts = null;
        }

        public void Dispose()
        {
            try { _cts?.Cancel(); } catch { }
            try { _stream?.Dispose(); } catch { }
            try { _client?.Dispose(); } catch { }
        }
    }
}


