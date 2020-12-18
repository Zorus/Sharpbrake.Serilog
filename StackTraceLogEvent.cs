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
            StackTrace = new StackTrace(skipFrames: 2);
        }
    }
}
