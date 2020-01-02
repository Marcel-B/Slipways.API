using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using com.b_velop.Slipways.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaterController : ControllerBase
    {
        private readonly JsonSerializerOptions _options;
        private readonly IRepositoryWrapper _repository;
        private readonly ILogger<WaterController> _logger;

        public WaterController(
            IRepositoryWrapper repository,
            ILogger<WaterController> logger)
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                IgnoreNullValues = true
            };
            _repository = repository;
            _logger = logger;
        }

        // GET: api/water
        [HttpGet]
        public async Task<IEnumerable<Water>> GetAsync(
            CancellationToken cancellationToken)
        {
            using (Metrics.CreateHistogram($"slipways_api_duration_GET_api_water_seconds", "Histogram").NewTimer())
            {
                var result = await _repository.Water.SelectAllAsync(cancellationToken);
                return result.OrderBy(_ => _.Longname);
            }
        }

        // GET api/water/8177a148-5674-4b8f-8ded-050907f640f3
        [HttpGet("{id}")]
        public async Task<ActionResult<Water>> GetAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            using (Metrics.CreateHistogram($"slipways_api_duration_GET_api_water_id_seconds", "Histogram").NewTimer())
            {
                return await _repository.Water.SelectByIdAsync(id, cancellationToken);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(
            WaterDto waterDto,
            CancellationToken cancellationToken)
        {
            if (waterDto == null || string.IsNullOrWhiteSpace(waterDto.Longname))
                return BadRequest("WaterDto is null or incorrect format");

            using (Metrics.CreateHistogram($"slipways_api_duration_POST_api_water_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var water = waterDto.ToClass();
                    var result = await _repository.Water.InsertAsync(water, cancellationToken);
                    waterDto.Id = result.Id;
                    return new JsonResult(waterDto, _options);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Unexpected error occured while post water '{waterDto.Longname}'", e);
                    return new StatusCodeResult(500);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(
            Guid id,
            WaterDto waterDto,
            CancellationToken cancellationToken)
        {
            if (id != waterDto.Id)
            {
                _logger.LogWarning(5555, $"Unable to update Water, IDs are not the same '{id} : {waterDto.Id}'");
                return BadRequest("IDs are not the same");
            }

            try
            {
                using (Metrics.CreateHistogram($"slipways_api_duration_PUT_api_water_seconds", "Histogram").NewTimer())
                {
                    var water = await _repository.Water.SelectByIdAsync(id, cancellationToken);
                    water.Longname = waterDto.Longname;
                    water.Shortname = waterDto.Shortname;
                    var result = _repository.Water.UpdateAsync(water);
                    return new JsonResult(waterDto, _options);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(6666, $"Unexpected error occured while Put Water with '{id}'", e);
                return new StatusCodeResult(500);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning(5000, $"Unable to delete Water - no ID");
                return BadRequest("Id is incorrect");
            }

            using (Metrics.CreateHistogram($"slipways_api_duration_DELETE_api_water_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var result = await _repository.Water.DeleteAsync(id, cancellationToken);
                    return new JsonResult(result, _options);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Unexpected error occurred while deleting Water with id '{id}'", e);
                    return new StatusCodeResult(500);
                }
            }
        }
    }
}