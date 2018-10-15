
namespace SerilogPlay
{

	using Microsoft.AspNetCore;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.Logging;
	using Serilog;
	using System;
	using System.IO;


	public class Program
	{
		public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
			.AddEnvironmentVariables()
			.Build();

		private static string _environmentName;


		public static void Main(string[] args)
		{
			Serilog.Debugging.SelfLog.Enable(Console.WriteLine);
			Log.Logger = new LoggerConfiguration()
							.ReadFrom.Configuration(Configuration)
							.CreateLogger();
			try
			{
				Log.Information("Getting the motors running...");
				Log.Information("Starting web host");
				CreateWebHostBuilder(args).Build().Run();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
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
				 }).UseStartup<Startup>()
				 .UseConfiguration(Configuration)
				 .UseSerilog();
			;
		}
	}
}
