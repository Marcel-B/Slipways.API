using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using com.b_velop.Slipways.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : Controller
    {
        private IRepositoryWrapper _rep;
        private JsonSerializerOptions _options;
        private ILogger<ServiceController> _logger;

        public ServiceController(
            IRepositoryWrapper rep,
            ILogger<ServiceController> logger)
        {
            _rep = rep;
            _logger = logger;

            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IgnoreNullValues = true
            };
        }

        // GET: api/values
        [HttpGet]
        [Authorize("reader")]
        public async Task<IEnumerable<Service>> Get()
        {
            var values = await _rep.Service.GetAllIncludeAsync();
            return values;
        }

        [HttpPost]
        [Authorize("allin")]
        public async Task<ActionResult> PostAsync(
           ServiceDto serviceDto)
        {
            using (Metrics.CreateHistogram($"slipwaysql_duration_POST_api_service_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var service = serviceDto.ToClass();
                    service.Id = Guid.NewGuid();

                    var result = await _rep.Service.InsertAsync(service);
                    if (result != null)
                    {
                        if (serviceDto.Manufacturers != null)
                        {
                            var manufacturers = new HashSet<ManufacturerService>();
                            foreach (var manufacturer in serviceDto.Manufacturers)
                            {
                                var manufacturerService = new ManufacturerService
                                {
                                    Id = Guid.NewGuid(),
                                    Created = DateTime.Now,
                                    ServiceFk = service.Id,
                                    ManufacturerFk = manufacturer.Id,
                                };
                                manufacturers.Add(manufacturerService);
                            }
                            _ = await _rep.ManufacturerServices.InsertRangeAsync(manufacturers);
                        }
                        serviceDto.Id = service.Id;
                        return new JsonResult(serviceDto, _options);
                    }
                    return new StatusCodeResult(500);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Error occurred while insert Service", e);
                    return new StatusCodeResult(500);
                }
            }
        }
    }
}
