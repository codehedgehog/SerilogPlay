﻿namespace SerilogPlay.SimpleMvcClient
{
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Mvc.Razor;
	using Microsoft.AspNetCore.Routing;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.Extensions.Logging;
	using System;
	using System.Collections.Generic;
	using System.IdentityModel.Tokens.Jwt;
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
			//services.AddHttpContextAccessor();
			//services.AddLogging(builder => builder.ClearProviders().AddSerilog(dispose: true));

			services.AddRouting(options => { options.LowercaseUrls = true; });
			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => false;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});

			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
										.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.Use(async (context, next) =>
			{
				if (context.Request.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)) context.Request.Scheme = "https";
				await next.Invoke();
			});
			app.UseDeveloperExceptionPage();
			app.UseDatabaseErrorPage();

			// Display friendly error pages for any non-success case.  This is only for client side, and not for API,
			// This will handle any situation where a status code is >= 400 and < 600, and no response body has already been generated.
			// appBuilder.UseStatusCodePagesWithReExecute(pathFormat: "/Home/Error", queryFormat: "?statusCode={0}");
			app.UseStatusCodePagesWithReExecute(pathFormat:"/home/error", queryFormat: "?statusCode={0}");
			// Handle unhandled errors
			app.UseExceptionHandler("/Home/Error");
			//app.UseExceptionHandler("/error/500");

			app.UseHsts();
			app.UseHttpsRedirection();

			app.UseAuthentication();
			app.UseStaticFiles();
			app.UseCookiePolicy(new CookiePolicyOptions() { MinimumSameSitePolicy = SameSiteMode.None });



			app.UseMvc(routes =>
			{
				routes.MapRoute(name: "default", template: "{controller=Home}/{action=Index}/{id?}");
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