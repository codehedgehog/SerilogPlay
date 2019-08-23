
namespace SerilogPlay.SimpleApi
{
	using Microsoft.AspNetCore.Diagnostics;
	using Microsoft.AspNetCore.Http;
	using Newtonsoft.Json;
	using Serilog;
	using System;
	using System.Threading.Tasks;


	public class CustomErrorMiddleware
	{
		private readonly RequestDelegate next;

		public CustomErrorMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context /* other dependencies */)
		{
			try
			{
				await next(context);
			}
			catch (Exception ex)
			{
				await HandleExceptionAsync(context, ex);
			}
		}

		private static Task HandleExceptionAsync(HttpContext context, Exception exception)
		{
			//var logger = loggerFactory.CreateLogger("Serilog Global exception logger");
			IExceptionHandlerFeature exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
			Log.Fatal(exception: exception, exception.Message);
			if (exceptionHandlerFeature != null && exceptionHandlerFeature.Error != null)
			{
				//logger.LogError(eventId: StatusCodes.Status500InternalServerError, exception: exceptionHandlerFeature.Error, message: exceptionHandlerFeature.Error.Message);
				Log.Fatal(exception: exceptionHandlerFeature.Error, messageTemplate: exceptionHandlerFeature.Error.Message);
			}
			var result = JsonConvert.SerializeObject(new
			{
				error = "An error occurred in our API.  Please refer to the error id below with our support team.",
				id = context.TraceIdentifier
			});
			context.Response.ContentType = "application/json";
			context.Response.StatusCode = StatusCodes.Status500InternalServerError;
			return context.Response.WriteAsync(result);
		}
	}
}
