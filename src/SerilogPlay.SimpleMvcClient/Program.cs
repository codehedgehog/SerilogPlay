namespace SerilogPlay.SimpleMvcClient
{
	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Serilog;
	using Serilog.Context;
	using Serilog.Enrichers.AspnetcoreHttpcontext;
	using Serilog.Events;
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
				Log.Debug("Starting web host");
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
				Path = context.Request.Path.ToString(),
				Host = context.Request.Host.ToString(),
				Method = context.Request.Method,
				RemoteIpAddress = context.Connection.RemoteIpAddress.MapToIPv4().ToString(),
				Scheme = context.Request.Scheme,
				QueryString = (context.Request.QueryString.HasValue) ? context.Request.QueryString.Value : null
			};
			LogContext.PushProperty("FullRequest", $"{theInfo.Method} {theInfo.Scheme}://{theInfo.Host.Trim('/')}{theInfo.Path}{theInfo.QueryString}");
			if (!string.IsNullOrWhiteSpace(theInfo.RemoteIpAddress)) LogContext.PushProperty("RemoteIPAddress", theInfo.RemoteIpAddress);
			var currentUser = context.User;
			if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated)
			{
				theInfo.CurrentUserName = currentUser.Identity.Name;
				LogContext.PushProperty("CurrentUserName", theInfo.CurrentUserName);
				int i = 0;
				theInfo.UserClaims = currentUser.Claims.ToDictionary(x => $"{x.Type} ({i++})", y => y.Value);
				//myInfo.UserClaims = currentUser.Claims.Select(a => new KeyValuePair<string, string>(a.Type, a.Value)).ToList();
			}
			if (context.Request.Query != null && context.Request.Query.Count > 0)
			{
				int i = 0;
				theInfo.Query = context.Request.Query.Select(q => new KeyValuePair<string, string>(q.Key, q.Value)).ToList();
			}
			if (context.Request.Headers.ContainsKey("X-Forwarded-For")) theInfo.RemoteIpAddress = context.Request.Headers["X-Forwarded-For"];
			if (context.Request.Headers.ContainsKey("User-Agent"))
			{
				theInfo.UserAgent = context.Request.Headers["User-Agent"];
				LogContext.PushProperty("UserAgent", theInfo.UserAgent);
			}
			if (context.Request.Headers.ContainsKey("Referer"))
			{
				theInfo.Referer = context.Request.Headers["Referer"];
				LogContext.PushProperty("Referer", theInfo.Referer);
			}
			if (context.Request.Headers.ContainsKey("X-Original-For")) theInfo.XOriginalFor = context.Request.Headers["X-Original-For"];
			if (context.Request.Headers.ContainsKey("X-Original-Proto")) theInfo.XOriginalProto = context.Request.Headers["X-Original-Proto"];
			//logEvent.AddPropertyIfAbsent(pf.CreateProperty("UserInfo", myInfo, true));
			return theInfo;
		}

		private class CustomEnricherHttpContextInfo
		{
			public string Path { get; set; }
			public string Host { get; set; }
			public string Method { get; set; }
			public string RemoteIpAddress { get; set; }
			public string CurrentUserName { get; set; }
			public Dictionary<string, string> UserClaims { get; set; } //public List<KeyValuePair<string, string>> UserClaims { get; set; }
			public string QueryString { get; set; }
			public List<KeyValuePair<string, string>> Query { get; set; }
			public string Referer { get; set; }
			public string UserAgent { get; set; }
			public string Scheme { get; set; }
			public string XOriginalFor { get; set; }
			public string XOriginalProto { get; set; }
		}

	}
}