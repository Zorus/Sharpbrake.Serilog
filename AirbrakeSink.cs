using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Sharpbrake.Client;
using Sharpbrake.Client.Model;
using System;
using System.Collections.Generic;
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
        private readonly INoticeData _noticeData;

        public AirbrakeSink(string projectId, string projectKey, ILoggerFilter filter, INoticeData noticeData)
        : this(new AirbrakeConfig()
        {
            ProjectId = projectId,
            ProjectKey = projectKey
        }, filter, noticeData)
        {
        }

        public AirbrakeSink(AirbrakeConfig config, ILoggerFilter filter, INoticeData noticeData)
        {
            _airbrake = new AirbrakeNotifier(config);
            _noticeData = noticeData;

            filter?.SetNoticeSender(EmitImpl);
            _filter = filter;
        }

        private Notice BuildNotice(LogEvent logEvent)
        {
            Notice notice = null;
            if (logEvent.Exception != null)
            {
                notice = _airbrake.BuildNotice(logEvent.Exception, $"{logEvent.RenderMessage()}. {logEvent.Exception.Message}");
            }
            else
            {
                notice = _airbrake.BuildNotice(logEvent.RenderMessage());
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
            }
            notice.EnvironmentVars = _noticeData?.GetEnvironmentVariables();
            notice.Session = _noticeData?.GetSessionVariables();
            return notice;
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

