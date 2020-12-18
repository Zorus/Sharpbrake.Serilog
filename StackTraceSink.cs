using Serilog.Core;
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
            _wrappedSink.Emit(new StackTraceLogEvent(logEvent));
        }
    }
}
