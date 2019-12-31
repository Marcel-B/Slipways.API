using System;
using com.b_velop.Slipways.API.Infrastructure;
using com.b_velop.Slipways.Data.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;

namespace Slipways.API
{
    public class Startup
    {
        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment WebHostEnvironment { get; }

        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddControllers();

            var secretProvider = new SecretProvider();
            var port = Environment.GetEnvironmentVariable("PORT");
            var server = Environment.GetEnvironmentVariable("SERVER");
            var user = Environment.GetEnvironmentVariable("USER");
            var database = Environment.GetEnvironmentVariable("DATABASE");

            var pw = string.Empty;

            if (WebHostEnvironment.IsStaging())
            {
                pw = secretProvider.GetSecret("dev_slipway_db");
            }
            else if (WebHostEnvironment.IsProduction())
            {
                pw = secretProvider.GetSecret("sqlserver");
            }
            else
            {
                pw = "foo123bar!";
            }
            var str = $"Server={server},{port};Database={database};User Id={user};Password={pw}";
#if DEBUG
            str = $"Server=db,1433;Database=Slipways;User Id=sa;Password=foo123bar!";
#endif
            services.AddSlipwaysData(str);
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpMetrics();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}
