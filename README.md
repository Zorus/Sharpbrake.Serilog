# Sharpbrake.Serilog
[![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Serilog)](https://www.nuget.org/packages/Sharpbrake.Serilog/)

A Serilog sink that sends log records to Airbrake server.

## Airbrake sink
In the example shown, the sink will send logs to [Airbrake](https://airbrake.io) with minimum log level `LogEventLevel.Fatal`. The Sharpbrake.Serilog Sink also supports more flexible Airbrake customization using the AirbrakeConfig class.
```csharp
Log.Logger = new LoggerConfiguration()
	.WriteTo.Airbrake(projectId: "<projectId>", projectKey: "<projectKey>", LogEventLevel.Fatal)
	.CreateLogger();
```

## StackTraceCapturer sink
This sink allows you to use a stack capturing when it's needed, for example when you want to use **Async** sink while keeping the original stack trace. This is how you use it:
```csharp
var config = new Sharpbrake.Client.AirbrakeConfig
{...};
 
var config = new LoggerConfiguration()
    .WriteTo
    .StackTraceCapturer(w => w.Async(x => x.Airbrake(config, LogEventLevel.Fatal)));

Log.Logger = config.CreateLogger();
```
In the code above the `StackTraceCapturer` is added and it wraps other sink - `Async`  which in turn wraps our `Airbrake` sink described above - this way you can notify Airbrake in async manner while keep original stack traces.

## Additional filter
You can specify additional filter for the Airbrake sink. This might be needed when you want to avoid some messages to go Airbrake server or deduplicate similar messages.
```csharp
var config = new Sharpbrake.Client.AirbrakeConfig
{...};
 
var config = new LoggerConfiguration()
    .WriteTo.Airbrake(config, LogEventLevel.Fatal, new AibrakeThrottlingFilter())
	.CreateLogger();
```
where `AibrakeThrottlingFilter` can look like this:
```csharp
class AibrakeThrottlingFilter : Sharpbrake.Serilog.ILoggerFilter
    {
        // The main filtering routine
        // return null means 'don't send'
        public LogEvent Filter(LogEvent logEvent)
        {
            return null; // don't send to Airbrake
        }

        // Airbrake sink will give a 'sender' to the object
        // to allow it send notifications later
        public void SetNoticeSender(Action<LogEvent> sendAction)
        {
            // this is the sender we can use to send message later
        }
    }
```
You can build any logic that you want to on top of this interface and use `sendAction` to send custom message at any point of time
