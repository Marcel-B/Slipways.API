using System;
using com.b_velop.Slipways.API.Infrastructure;
using com.b_velop.Slipways.Data;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Repositories;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
            var authority = Environment.GetEnvironmentVariable("AUTHORITY");
            var apiResource = Environment.GetEnvironmentVariable("API_RESOURCE");

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "cache";
                options.InstanceName = "Slipways";
            });

            services.AddDbContext<SlipwaysContext>(options =>
            {
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
                options.UseSqlServer(str);
            });

            services.AddScoped<IWaterRepository, WaterRepository>();
            services.AddScoped<IStationRepository, StationRepository>();
            services.AddScoped<ISlipwayRepository, SlipwayRepository>();
            services.AddScoped<IExtraRepository, ExtraRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
            services.AddScoped<IManufacturerServicesRepository, ManufacturerServicesRepository>();
            services.AddScoped<IPortRepository, PortRepository>();
            services.AddScoped<ISlipwayExtraRepository, SlipwayExtraRepository>();
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("reader", builder =>
                {
                    builder.RequireScope("slipways.api.reader");
                });
                options.AddPolicy("allin", builder => builder.RequireScope("slipways.api.allaccess"));
            });

            services
                .AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            .AddIdentityServerAuthentication(options =>
            {
                options.Authority = authority;
                options.RequireHttpsMetadata = false;
                options.ApiName = apiResource;
            });
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();
            });
        }
    }
}
