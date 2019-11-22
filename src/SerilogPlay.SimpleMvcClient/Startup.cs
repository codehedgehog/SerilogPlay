namespace SerilogPlay.SimpleMvcClient
{
	using Microsoft.AspNetCore.Builder;
		using Microsoft.AspNetCore.CookiePolicy;
		using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Razor;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.Security.Claims;
	using System.Threading.Tasks;

	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.HttpOnly = HttpOnlyPolicy.Always;
				options.Secure = CookieSecurePolicy.Always;
				options.CheckConsentNeeded = context => false;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			//services.AddHttpContextAccessor();
			//services.AddLogging(builder => builder.ClearProviders().AddSerilog(dispose: true));

			services.AddRouting(options => { options.LowercaseUrls = true; });

			//JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

			services.AddAuthentication(options =>
				{
					options.DefaultScheme = "Cookies";
					options.DefaultChallengeScheme = "oidc";
				})
				.AddCookie("Cookies")
				.AddOpenIdConnect("oidc", options =>
				{
					options.SignInScheme = "Cookies";
					options.Authority = "https://demo.identityserver.io";
					options.ClientId = "server.hybrid";
					options.ClientSecret = "secret";
					options.ResponseType = "code id_token";
					options.Scope.Add("email");
					options.Scope.Add("api");
					options.Scope.Add("offline_access");
					options.GetClaimsFromUserInfoEndpoint = true;
					options.SaveTokens = true;
					options.Events.OnTicketReceived = e =>
					{
						e.Principal = TransformClaims(e.Principal);
						return Task.CompletedTask;
					};
				});
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			services.AddMvc(options =>
											{
												options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
											})
										.AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
										.AddDataAnnotationsLocalization().AddRazorOptions(options => { })
										.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
		{
			// Middleware that run before routing
			app.Use(async (context, next) =>
			{
				if (context.Request.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)) context.Request.Scheme = "https";
				await next.Invoke();
			});

			// Display friendly error pages for any non-success case.  This is only for client side, and not for API,
			// This will handle any situation where a status code is >= 400 and < 600, and no response body has already been generated.
			// appBuilder.UseStatusCodePagesWithReExecute(pathFormat: "/Home/Error", queryFormat: "?statusCode={0}");
			app.UseStatusCodePagesWithReExecute(pathFormat: "/home/error", queryFormat: "?statusCode={0}");
			// Handle unhandled errors
			app.UseExceptionHandler("/home/error");

			app.UseHsts();
			app.UseHttpsRedirection();
			app.UseXContentTypeOptions();
			app.UseReferrerPolicy(opts => opts.NoReferrer());
			app.UseXXssProtection(options => options.EnabledWithBlockMode());
			app.UseXfo(options => options.Deny());
			app.UseStaticFiles();
			app.UseNoCacheHttpHeaders();
			app.UseXRobotsTag(options => options.NoIndex().NoFollow());

			// Runs matching. An endpoint is selected and set on the HttpContext if a match is found.
			app.UseRouting();

			// Middlewares that run after routing occurs. These middleware can take different actions based on the endpoint.
			app.UseCookiePolicy(new CookiePolicyOptions() { MinimumSameSitePolicy = SameSiteMode.None });
			app.UseAuthentication();
			app.UseAuthorization();

			// Executes the endpoint that was selected by routing.
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapDefaultControllerRoute();
				endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}

		private ClaimsPrincipal TransformClaims(ClaimsPrincipal principal)
		{
			var claims = new List<Claim>();
			claims.AddRange(principal.Claims);  // retain any claims from originally authenticated user
			claims.Add(new Claim("junk", "garbage"));
			var newIdentity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType, "name", "role");
			return new ClaimsPrincipal(newIdentity);
		}
	}
}