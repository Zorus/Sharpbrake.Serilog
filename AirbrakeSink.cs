using Serilog.Core;
using Serilog.Debugging;
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
        private readonly AirbrakeNotifier _airbrake;

        public AirbrakeSink(string projectId, string projectKey)
        : this(new AirbrakeConfig()
            {
                ProjectId = projectId,
                ProjectKey = projectKey
            })
        { 
        }

        public AirbrakeSink(AirbrakeConfig config)
        {
            _airbrake = new AirbrakeNotifier(config);
        }

        private Notice BuildNotice(LogEvent logEvent)
        {
            if (logEvent.Exception != null)
            {
                return _airbrake.BuildNotice(logEvent.Exception, $"{logEvent.RenderMessage()}. {logEvent.Exception.Message}");
            }
            else
            {
                return _airbrake.BuildNotice(logEvent.RenderMessage());
            }
        }

        public void Emit(LogEvent logEvent)
        {
            try
            {
                var notice = BuildNotice(logEvent);
                _airbrake.NotifyAsync(notice)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch(Exception ex)
            {
                SelfLog.WriteLine($"Failed to send logs. {ex}");
            }
        }
    }
}

