using Serilog.Core;
using Serilog.Events;
using Sharpbrake.Client;
using Sharpbrake.Client.Model;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sharpbrake.Serilog
{
    public class AirbrakeSink : ILogEventSink
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Channel<LogEvent> _logChannel;
        private readonly AirbrakeNotifier _airbrake;

        public AirbrakeSink(string projectId, string projectKey, CancellationToken cancellationToken = default)
        : this(new AirbrakeConfig()
            {
                ProjectId = projectId,
                ProjectKey = projectKey
            }
            , cancellationToken)
        { 
        }

        public AirbrakeSink(AirbrakeConfig config, CancellationToken cancellationToken = default)
        {
            _logChannel = Channel.CreateUnbounded<LogEvent>();
            _airbrake = new AirbrakeNotifier(config);
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        private async void ProcessLogsAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    LogEvent item = await _logChannel.Reader.ReadAsync(_cancellationTokenSource.Token).ConfigureAwait(false);
                    await SendEventAsync(item).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async Task SendEventAsync(LogEvent logEvent)
        {
            var notice = BuildNotice(logEvent);
            var responce = await _airbrake.NotifyAsync(notice).ConfigureAwait(false);
        }

        private Notice BuildNotice(LogEvent logEvent)
        {
            if (logEvent.Exception != null)
            {
                return _airbrake.BuildNotice(logEvent.Exception, logEvent.RenderMessage());
            }
            else
            {
                return _airbrake.BuildNotice(logEvent.RenderMessage());
            }
        }

        public void Emit(LogEvent logEvent)
        {
            _logChannel.Writer.TryWrite(logEvent);
        }
    }
}

