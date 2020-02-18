using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using com.b_velop.Slipways.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : Controller
    {
        private readonly IRepositoryWrapper _repository;
        private readonly JsonSerializerOptions _options;
        private readonly ILogger<ServiceController> _logger;

        public ServiceController(
            IRepositoryWrapper repository,
            ILogger<ServiceController> logger)
        {
            _repository = repository;
            _logger = logger;

            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                IgnoreNullValues = true
            };
        }

        // GET: api/values
        [HttpGet]
        public async Task<IEnumerable<Service>> GetAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var values = await _repository.Service.SelectAllAsync(cancellationToken);
                return values;
            }
            catch (Exception e)
            {
                _logger.LogError(6666, e, $"Unexpected Error occurred while Select all Services");
                return null;
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(
           ServiceDto serviceDto,
           CancellationToken cancellationToken)
        {
            if (serviceDto == null || string.IsNullOrEmpty(serviceDto.Name))
                return BadRequest("ServiceDto is null or format error");

            using (Metrics.CreateHistogram($"slipways_api_duration_POST_api_service_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var service = serviceDto.ToClass();
                    var result = await _repository.Service.InsertAsync(service, cancellationToken);
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
                            _ = await _repository.ManufacturerServices.InsertRangeAsync(manufacturers, cancellationToken);
                        }
                        serviceDto.Id = service.Id;
                        return new JsonResult(serviceDto, _options);
                    }
                    return new StatusCodeResult(500);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Unexpected error occurred while insert Service '{serviceDto?.Name}'", e);
                    return new StatusCodeResult(500);
                }
            }
        }
    }
}
