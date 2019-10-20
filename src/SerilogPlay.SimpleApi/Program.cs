namespace SerilogPlay.SimpleApi
{
	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Serilog;
	using Serilog.Context;
	using Serilog.Enrichers.AspnetcoreHttpcontext;
	using Serilog.Exceptions;
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;

	public class Program
	{
		public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		private static string _environmentName;

		public static int Main(string[] args)
		{
			Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
			try
			{
				CreateWebHostBuilder(args).Build().Run();
				Log.Debug("Getting the motors running...");
				Log.Debug("Starting API web host");
				return 0;
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
				Console.WriteLine("Host terminated unexpectedly");
				Console.Write(ex.ToString());
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				.ConfigureLogging((hostingContext, config) =>
				{
					config.ClearProviders();
					_environmentName = hostingContext.HostingEnvironment.EnvironmentName;
				})
				.ConfigureKestrel(c => c.AddServerHeader = false)
				.UseStartup<Startup>()
				.UseConfiguration(Configuration)
				.UseSerilog((provider, context, loggerConfig) =>
				{
					var name = Assembly.GetExecutingAssembly().GetName();
					loggerConfig
						 .Enrich.FromLogContext()
						 .Enrich.WithExceptionDetails()
						 .Enrich.WithAspnetcoreHttpcontext(serviceProvider: provider, customMethod: CustomEnricherLogic)
						 .Enrich.WithProperty("Assembly", $"{name.Name}")
						 .Enrich.WithProperty("Version", $"{name.Version}")
						 .Enrich.WithMachineName()
						 .Enrich.WithThreadId()
						 .ReadFrom.Configuration(Configuration)
						 .WriteTo.Console();
				});
		}

		private static CustomEnricherHttpContextInfo CustomEnricherLogic(IHttpContextAccessor ctx) // LogEvent logEvent, ILogEventPropertyFactory pf
		{
			HttpContext context = ctx.HttpContext;
			if (context == null) return null;

			CustomEnricherHttpContextInfo theInfo = new CustomEnricherHttpContextInfo()
			{
				RemoteIpAddress = context.Connection.RemoteIpAddress.MapToIPv4().ToString(),
				FullRequestPath = $"{context.Request.Method} {context.Request.Scheme}://{context.Request.Host.ToString().Trim('/')}{context.Request.Path.ToString()}{((context.Request.QueryString.HasValue) ? context.Request.QueryString.Value : null)}",
			};
			if (!string.IsNullOrWhiteSpace(theInfo.RemoteIpAddress)) LogContext.PushProperty(name: "RemoteIPAddress", value: theInfo.RemoteIpAddress);
			if (!string.IsNullOrWhiteSpace(theInfo.FullRequestPath)) LogContext.PushProperty(name: "FullRequestPath", value: theInfo.FullRequestPath);
			if (context.Request.Query != null && context.Request.Query.Count > 0) theInfo.Query = context.Request.Query.Select(q => new KeyValuePair<string, string>(q.Key, q.Value)).ToList();
			if (context.Request.Headers.ContainsKey("Authorization"))
			{
				theInfo.Authorization = context.Request.Headers["Authorization"];
				LogContext.PushProperty(name: "Authorization", value: theInfo.Authorization);
			}
			if (context.Request.Headers.ContainsKey("X-Forwarded-For")) theInfo.RemoteIpAddress = context.Request.Headers["X-Forwarded-For"];
			return theInfo;
		}

		private class CustomEnricherHttpContextInfo
		{
			public string FullRequestPath { get; set; }
			public string RemoteIpAddress { get; set; }
			public string Authorization { get; set; }
			public List<KeyValuePair<string, string>> Query { get; set; }
		}
	}
}