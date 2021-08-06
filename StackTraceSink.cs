using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;

using System;
using System.Collections.Generic;
using System.Text;

namespace Sharpbrake.Serilog
{
    internal class StackTraceSink: ILogEventSink
    {
        private readonly ILogEventSink _wrappedSink;
        public StackTraceSink(ILogEventSink wrappedSink)
        {
            _wrappedSink = wrappedSink ?? throw new ArgumentNullException(nameof(wrappedSink));
        }

        public void Emit(LogEvent logEvent)
        {
            StackTraceLogEvent stackTraceLogEvent = null;
            try
            {
                stackTraceLogEvent = new StackTraceLogEvent(logEvent);
            }
            catch(Exception ex)
            {
                SelfLog.WriteLine($"Failed to wrapp LogEvent. {ex}");
            }
            _wrappedSink.Emit(stackTraceLogEvent ?? logEvent);
        }
    }
}
