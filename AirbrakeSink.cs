using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Sharpbrake.Client;
using Sharpbrake.Client.Model;
using System;
using System.Linq;

namespace Sharpbrake.Serilog
{
    public interface ILoggerFilter
    {
        void SetNoticeSender(Action<LogEvent> sendAction);
        LogEvent Filter(LogEvent logEvent);
    }

    public class AirbrakeSink : ILogEventSink
    {
        private readonly AirbrakeNotifier _airbrake;
        private readonly ILoggerFilter _filter;

        public AirbrakeSink(string projectId, string projectKey, ILoggerFilter filter)
        : this(new AirbrakeConfig()
        {
            ProjectId = projectId,
            ProjectKey = projectKey
        }, filter)
        {
        }

        public AirbrakeSink(AirbrakeConfig config, ILoggerFilter filter)
        {
            _airbrake = new AirbrakeNotifier(config);
            
            filter?.SetNoticeSender(EmitImpl);
            _filter = filter;
        }

        private Notice BuildNotice(LogEvent logEvent)
        {
            if (logEvent.Exception != null)
            {
                return _airbrake.BuildNotice(logEvent.Exception, $"{logEvent.RenderMessage()}. {logEvent.Exception.Message}");
            }
            else
            {
                var notice = _airbrake.BuildNotice(logEvent.RenderMessage());
                if(logEvent is StackTraceLogEvent stackTraceEvent && stackTraceEvent.StackTrace != null)
                {
                    if(notice.Errors.FirstOrDefault() is ErrorEntry error)
                    {
                        error.Backtrace = Utils.GetBacktrace(stackTraceEvent.StackTrace);
                    }
                    else
                    {
                        notice.Errors.Add(new ErrorEntry()
                        {
                            Backtrace = Utils.GetBacktrace(stackTraceEvent.StackTrace),
                            Message = logEvent.RenderMessage()
                        });
                    }
                }
                return notice;
            }
        }

        private void EmitImpl(LogEvent logEvent)
        {
            try
            {
                var notice = BuildNotice(logEvent);
                _airbrake.NotifyAsync(notice)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"Failed to send logs. {ex}");
            }
        }

        public void Emit(LogEvent logEvent)
        {
            try
            {
                if (_filter != null)
                {
                    logEvent = _filter.Filter(logEvent);
                }

                if (logEvent == null)
                {
                    return;
                }

                EmitImpl(logEvent);
            }
            catch(Exception ex)
            {
                SelfLog.WriteLine($"Failed to send logs. {ex}");
            }
        }
    }
}

