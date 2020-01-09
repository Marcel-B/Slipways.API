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

            var password = string.Empty;

            if (WebHostEnvironment.IsStaging())
            {
                password = secretProvider.GetSecret("dev_slipway_db");
            }
            else if (WebHostEnvironment.IsProduction())
            {
                password = secretProvider.GetSecret("sqlserver");
            }
            else
            {
                password = "foo123bar!";
            }
            var connectionString = $"Server={server},{port};Database={database};User Id={user};Password={password}";
#if DEBUG
            connectionString = $"Server=localhost,1433;Database=Slipways;User Id=sa;Password=foo123bar!";
#endif
            services.AddSlipwaysData(connectionString);
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
