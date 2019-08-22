namespace SerilogPlay.SimpleMvcClient
{
	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Serilog;
	using Serilog.Core;
	using Serilog.Events;
	using Serilog.Exceptions;
	using SerilogPlay.SimpleMvcClient.Models;
	using System;
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
			var name = Assembly.GetExecutingAssembly().GetName();

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProperty("Assembly", $"{name.Name}")
				.Enrich.WithProperty("Version", $"{name.Version}")
				.Enrich.WithThreadId()
				.Enrich.WithExceptionDetails()
				.ReadFrom.Configuration(Configuration)
				.WriteTo.Console()
				.CreateLogger();
			try
			{
				Log.Information("Getting the motors running...");
				Log.Information("Starting web host");
				CreateWebHostBuilder(args).Build().Run();
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
				 .UseKestrel(c => c.AddServerHeader = false)
				 .UseStartup<Startup>()
				 .UseConfiguration(Configuration)
				 .UseSerilog();
		}

		public static void AddCustomContextInfo(IHttpContextAccessor ctx, LogEvent logEvent, ILogEventPropertyFactory pf)
		{
			var context = ctx.HttpContext;
			if (context == null) return;

			var userInfo = context.Items["my-custom-info"] as UserInfo;
			if (userInfo == null)
			{
				var user = context.User.Identity;
				if (user == null || !user.IsAuthenticated) return;
				var i = 0;
				userInfo = new UserInfo
				{
					Name = user.Name,
					Claims = context.User.Claims.ToDictionary(x => $"{x.Type} ({i++})", y => y.Value)
				};
				context.Items["my-custom-info"] = userInfo;
			}

			logEvent.AddPropertyIfAbsent(pf.CreateProperty("UserInfo", userInfo, true));
		}
	}
}