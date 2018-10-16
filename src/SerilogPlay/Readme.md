# SerilogPlay

## Sample code

`Email Sink` is relying on the `PeriodicBatching Sink` which following its configuration buffers the logs and periodically (or when you hit the `batchPostingLimit` threshold) writes them to the sink.  

That means that before your application is closing you have to explicitely flush your sinks (overall its a good practice) to trigger a write : just call the `Log.CloseAndFlush()`

```
try{
  //Boring application code
}catch(Exception e){
  Log.Fatal(e, "Woops, something went wrong");
}finally{
  Log.CloseAndFlush();
}
```



## NuGet Packages

```
+ install-package Serilog.AspNetCore
+ install-package Serilog.Settings.Configuration
+ install-package Serilog.Sinks.RollingFile
+ install-package Serilog.Sinks.Async
+ install-package Serilog.Sinks.Console
+ install-package Serilog.Sinks.Email
+ install-package Serilog.Sinks.MSSqlServer

+ install-package Serilog.Enrichers.Environment
+ install-package Serilog.Enrichers.Process
+ install-package Serilog.Enrichers.Thread

+ install-package Serilog.Exceptions
+ Install-Package Serilog.Enrichers.AspNetCore.HttpContext


```


## References

* `Serilog.Email.Sinks` and `appsettings.json` configuration
  * [ASP.NET Core and Serilog Email Sink - JSON config](https://stackoverflow.com/questions/50387469/net-core-and-serilog-email-sink-json-config)
    * [Configure Serilog Email sink with appsettings.json to work with Gmail](https://stackoverflow.com/questions/46942106/trying-to-configure-serilog-email-sink-with-appsettings-json-to-work-with-gmail) 
 
* General Theory
  * [Serilog Tutorial for .NET Logging: 16 Best Practices and Tips](https://stackify.com/serilog-tutorial-net-logging/)

* For exploration:
  * [Logging with Serilog.Exceptions](https://rehansaeed.com/logging-with-serilog-exceptions/)
  * [Add Serilog to MS SQL Server with custom column and "Verification Code" column](https://blog.bitscry.com/2018/02/01/adding-serilog-to-an-asp-net-core-2-web-application/)


* GitHub References:
  * [GitHub: Serilog.Sinks.MSSqlServer](https://github.com/serilog/serilog-sinks-mssqlserver)
  * [GitHub: Serilog.Sinks.Email](https://github.com/serilog/serilog-sinks-email/tree/dev)
