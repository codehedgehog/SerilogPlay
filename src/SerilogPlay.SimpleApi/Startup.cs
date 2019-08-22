namespace SerilogPlay.SimpleApi
{
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.DependencyInjection;

	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddAuthorization();

			services.AddAuthentication("Bearer")
					.AddIdentityServerAuthentication(options =>
					{
						options.Authority = "https://demo.identityserver.io";
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
			app.UseMiddleware<CustomErrorMiddleware>();
			app.UseMvc();
		}
	}
}