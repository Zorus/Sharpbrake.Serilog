using Serilog;
using Serilog.Configuration;
using Serilog.Events;

using Sharpbrake.Client;
using Sharpbrake.Serilog;

using System;
using System.Collections.Generic;
using System.Text;

namespace Serilog
{
    public static class AirbrakeLoggerConfigurationExtensions
    {
        public static LoggerConfiguration Airbrake(
            this LoggerSinkConfiguration sinkConfiguration,
            string projectId, 
            string projectKey,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            return sinkConfiguration.Sink(new AirbrakeSink(projectId, projectKey), restrictedToMinimumLevel);
        }

        public static LoggerConfiguration Airbrake(
            this LoggerSinkConfiguration sinkConfiguration,
            AirbrakeConfig config,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            return sinkConfiguration.Sink(new AirbrakeSink(config), restrictedToMinimumLevel);
        }

        /// <summary>
        /// Workaround to fix Airbrake call stack for Serilog.Sinks.Async
        /// </summary>
        /// <returns></returns>
        public static LoggerConfiguration StackTraceWrapper(
            this LoggerSinkConfiguration sinkConfiguration,
            Action<LoggerSinkConfiguration> configureWrappedSink)
        {
            return LoggerSinkConfiguration.Wrap(
                sinkConfiguration,
                sink => new StackTraceSink(sink),
                configureWrappedSink);
        }
    }
}
