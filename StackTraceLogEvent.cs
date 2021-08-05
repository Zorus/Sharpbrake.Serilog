using Serilog.Events;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Sharpbrake.Serilog
{
    internal class StackTraceLogEvent : LogEvent
    {
        public StackTrace StackTrace { get; set; }

        public StackTraceLogEvent(LogEvent logEvent)
            : base(
                logEvent.Timestamp,
                logEvent.Level,
                logEvent.Exception,
                logEvent.MessageTemplate,
                logEvent.Properties.Select(x => new LogEventProperty(x.Key, x.Value))
                )
        {
            StackTrace = GetStackTrace();
        }

        private StackTrace GetStackTrace()
        {
#if !NETSTANDARD1_4
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var stackFrame = stackTrace.GetFrame(i);
                var namesp = stackFrame.GetMethod().DeclaringType?.Namespace;
                if (string.IsNullOrEmpty(namesp)
                    || namesp.StartsWith("Sharpbrake", StringComparison.Ordinal)
                    || namesp.StartsWith("Serilog", StringComparison.Ordinal))
                {
                    continue;
                }
                return new StackTrace(skipFrames: i, fNeedFileInfo: true);
            }
#endif
            return null;
        }
    }
}
