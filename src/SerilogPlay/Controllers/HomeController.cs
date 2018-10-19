namespace SerilogPlay.Controllers
{
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Diagnostics;
	using Microsoft.AspNetCore.Mvc;
	using SerilogPlay.Models;
	using System.Diagnostics;

	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult About()
		{
			ViewData["Message"] = "Your application description page.";

			return View();
		}

		[Authorize]
		public IActionResult Contact()
		{
			ViewData["Message"] = "Your contact page.";

			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			ErrorViewModel modelResult = new ErrorViewModel() { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
			if (HttpContext.Features.Get<IStatusCodeReExecuteFeature>() is StatusCodeReExecuteFeature reExecuteFeature)
			{
				modelResult.OriginalPath = reExecuteFeature?.OriginalPath;
				modelResult.OriginalPathBase = reExecuteFeature?.OriginalPathBase;
				modelResult.OriginalQueryString = reExecuteFeature?.OriginalQueryString;
			}
			if (HttpContext.Features.Get<IExceptionHandlerPathFeature>() is ExceptionHandlerFeature exceptionFeature)
			{
				modelResult.RouteOfException = exceptionFeature?.Path;
				modelResult.ErrorSource = exceptionFeature?.Error?.Source;
				modelResult.ErrorTargetSiteName = exceptionFeature?.Error?.TargetSite.Name;
				modelResult.ErrorStackTrace = exceptionFeature?.Error?.StackTrace;
				modelResult.ErrorMessage = $"{exceptionFeature?.Error?.InnerException?.Message} | {exceptionFeature.Error?.Message}";
			}
			return View(viewName: "Error", model: modelResult);
		}
	}
}