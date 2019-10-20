namespace SerilogPlay.SimpleApi
{
	using IdentityServer4.AccessTokenValidation;
	using Microsoft.AspNetCore.Authentication.JwtBearer;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.Extensions.DependencyInjection;
	using System.IdentityModel.Tokens.Jwt;

	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			JwtSecurityTokenHandler.DefaultInboundClaimFilter.Clear();
			JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
			JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

			services.AddMvcCore()
							.AddAuthorization();

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

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseStatusCodePagesWithReExecute(pathFormat: "/api/error", queryFormat: "?statusCode={0}");
			app.UseMiddleware<CustomErrorMiddleware>();
			app.UseHsts();
			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}