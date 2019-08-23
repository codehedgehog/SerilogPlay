namespace SerilogPlay.SimpleApi
{
	using IdentityServer4.AccessTokenValidation;
	using Microsoft.AspNetCore.Authentication.JwtBearer;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.DependencyInjection;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;
	using System.IdentityModel.Tokens.Jwt;

	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			JwtSecurityTokenHandler.DefaultInboundClaimFilter.Clear();
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
			JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

			services.AddMvcCore()
							.AddAuthorization()
							.AddJsonFormatters();

			services.AddMvc()
							.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
							.AddJsonOptions(options =>
							 {
								 options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
								 options.SerializerSettings.Formatting = Formatting.Indented;
								 options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
								 options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
							 });


			services.AddAuthentication(options =>
								{
									options.DefaultAuthenticateScheme = IdentityServerAuthenticationDefaults.AuthenticationScheme;
									options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
								})
							.AddIdentityServerAuthentication(options =>
								{
									options.Authority = "https://demo.identityserver.io";
									options.RequireHttpsMetadata = false;
									options.LegacyAudienceValidation = true;
									options.ApiName = "api";  // defines required scope in bearer token
								});
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			//if (env.IsDevelopment())
			//{
			//	app.UseDeveloperExceptionPage();
			//}
			app.UseHsts();
			app.UseHttpsRedirection();
			app.UseAuthentication();
			app.UseStatusCodePagesWithReExecute(pathFormat: "/api/error", queryFormat: "?statusCode={0}");
			//appBuilder.UseExceptionHandler("/apierror/500");

			app.UseMiddleware<CustomErrorMiddleware>();
			app.UseMvc();
		}
	}
}