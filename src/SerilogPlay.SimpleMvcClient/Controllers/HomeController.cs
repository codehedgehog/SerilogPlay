namespace SerilogPlay.SimpleMvcClient.Controllers
{
	using Microsoft.AspNetCore.Authentication;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Diagnostics;
	using Microsoft.AspNetCore.Mvc;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Serilog;
	using SerilogPlay.SimpleMvcClient.Models;
	using System;
	using System.Diagnostics;

	//using System.Diagnostics;
	using System.Net.Http;
	using System.Threading.Tasks;

	[Authorize]
	public class HomeController : Controller
	{
		[AllowAnonymous]
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult About()
		{
			Log.Information("We got here at the About page....");
			ViewData["Message"] = "SerilogPlay is to explore everything about Serilog and Exception Handlers.";
			return View();
		}

		public IActionResult Contact()
		{
			Log.Information("We got here at the Contact page....");
			ViewData["Message"] = "Your contact page.";
			return View();
		}

		public IActionResult BadPage(int id)
		{
			ViewData["Message"] = "Your exception page.";
			throw new System.Exception("Craziness!!!");
			//return View();
		}

		public IActionResult BadPageWithQuery(int id, string code)
		{
			throw new Exception("Something bad happened.");
		}

		public async Task<IActionResult> GoodApi()
		{
			var client = new HttpClient();
			var token = await HttpContext.GetTokenAsync("access_token");
			client.SetBearerToken(token);
			var response = await GetWithHandlingAsync(client, "https://localhost:44369/api/Values");
			ViewBag.JsonApiResult = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();
			return View();
		}

		public async Task<IActionResult> UnauthApi()
		{
			var client = new HttpClient();
			var token = await HttpContext.GetTokenAsync("id_token");  // consciously getting wrong token here
			client.SetBearerToken(token);
			var response = await GetWithHandlingAsync(client, "https://localhost:44369/api/Values");
			ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();
			return View("BadApi");  // should never really get here....
		}

		public async Task<IActionResult> BadApi()
		{
			var client = new HttpClient();
			var token = await HttpContext.GetTokenAsync("access_token");  // correctly used the Access Token
			client.SetBearerToken(token);
			var response = await GetWithHandlingAsync(client, "https://localhost:44369/api/Values/123");  // calls a route that throws an exception
			ViewBag.Json = JArray.Parse(await response.Content.ReadAsStringAsync()).ToString();
			return View(); // should never really get here....
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error(int? statusCode = null)
		{
			ErrorViewModel modelResult = new ErrorViewModel() { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
			if (HttpContext.Features.Get<IStatusCodeReExecuteFeature>() is StatusCodeReExecuteFeature reExecuteFeature)
			{
				modelResult.OriginalPath = reExecuteFeature?.OriginalPath;
				modelResult.OriginalPathBase = reExecuteFeature?.OriginalPathBase;
				modelResult.OriginalQueryString = reExecuteFeature?.OriginalQueryString;
				Log.Warning($"Status Code: {statusCode} from request {modelResult.OriginalPathBase}{modelResult.OriginalPath}{modelResult.OriginalQueryString} Request ID: {modelResult.RequestId}");
			}
			if (HttpContext.Features.Get<IExceptionHandlerPathFeature>() is ExceptionHandlerFeature exceptionFeature)
			{
				if (exceptionFeature != null)
				{
					modelResult.RouteOfException = exceptionFeature.Path;
					if (exceptionFeature.Error != null)
					{
						modelResult.ErrorTargetSiteName = exceptionFeature.Error.TargetSite.Name;
						modelResult.ErrorMessage = $"{exceptionFeature.Error.InnerException?.Message} | {exceptionFeature.Error?.Message}";
						modelResult.ErrorSource = exceptionFeature.Error.Source;
						modelResult.ErrorStackTrace = exceptionFeature.Error.StackTrace;
						modelResult.ErrorData = JsonConvert.SerializeObject(exceptionFeature.Error.Data);
					}
				}
			}
			return View(viewName: "Error", model: modelResult);
		}

		private static async Task<HttpResponseMessage> GetWithHandlingAsync(HttpClient client, string apiRoute)
		{
			var response = await client.GetAsync(apiRoute);
			if (!response.IsSuccessStatusCode)
			{
				string error = string.Empty;
				string id = string.Empty;

				if (response.Content.Headers.ContentLength > 0)
				{
					var j = JObject.Parse(await response.Content.ReadAsStringAsync());
					error = (string)j["error"];
					id = (string)j["id"];
				}
				//below logs warning with these details and THEN throws exception, which will also get logged
				//    but without the details from the API call and response.
				//    An alternative would be to use Serilog.Enrichers.Exceptions and include the API details
				//    in the ex.Data fields -- e.g. ex.Data.Add("ApiStatus", (int) response.StatusCode);
				//    Then you would throw the exception and only get ONE log entry with all of the details
				var ex = new Exception("API Failure");

				ex.Data.Add("API Route", $"GET {apiRoute}");
				ex.Data.Add("API Status", (int)response.StatusCode);
				if (!string.IsNullOrEmpty(error))
				{
					ex.Data.Add("API Error", error);
					ex.Data.Add("API ErrorId", id);
				}
				//Log.Warning(ex,
				//    "Got non-success response from API {ApiStatus}--{ApiError}--{ApiErrorId}--{ApiUrl}",
				//    (int) response.StatusCode,
				//    error,
				//    id,
				//    $"GET {apiRoute}");

				throw ex;
			}

			return response;
		}
	}
}