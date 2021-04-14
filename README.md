# Sharpbrake.Serilog
[![NuGet](https://img.shields.io/nuget/v/Sharpbrake.Serilog)](https://www.nuget.org/packages/Sharpbrake.Serilog/)

A Serilog sink that send logs to airbrake. 

In the example shown, the sink will send logs to airbrake with minimum log level `LogEventLevel.Fatal`. The Sharpbrake.Serilog Sink also supports more flexible Airbrake customization using the AirbrakeConfig class.
```csharp
Log.Logger = new LoggerConfiguration()
	.WriteTo.Airbrake(projectId: "<projectId>", projectKey: "<projectKey>", LogEventLevel.Fatal)
	.CreateLogger();
```

[Airbrake repo is located](https://github.com/airbrake/sharpbrake)