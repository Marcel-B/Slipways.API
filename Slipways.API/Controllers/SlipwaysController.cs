using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using com.b_velop.Slipways.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlipwaysController : ControllerBase
    {
        private readonly JsonSerializerOptions _options;
        private readonly IRepositoryWrapper _repository;
        private readonly ILogger<SlipwaysController> _logger;

        public SlipwaysController(
            IRepositoryWrapper repository,
            ILogger<SlipwaysController> logger)
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

        // GET: api/slipway
        [HttpGet]
        public async Task<IEnumerable<Slipway>> GetAsync(
            CancellationToken cancellationToken)
        {
            using (Metrics.CreateHistogram($"slipways_api_duration_GET_api_slipway_seconds", "Histogram").NewTimer())
            {
                var result = await _repository.Slipway.SelectAllAsync(cancellationToken);
                return result.OrderBy(_ => _.Name);
            }
        }

        // GET api/slipway/8177a148-5674-4b8f-8ded-050907f640f3
        [HttpGet("{id}")]
        public async Task<ActionResult<Slipway>> GetAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            using (Metrics.CreateHistogram($"slipways_api_duration_GET_api_slipway_id_seconds", "Histogram").NewTimer())
            {
                return await _repository.Slipway.SelectByIdAsync(id, cancellationToken);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(
            SlipwayDto slipwayDto,
            CancellationToken cancellationToken)
        {
            if (slipwayDto == null || string.IsNullOrWhiteSpace(slipwayDto.Name))
                return BadRequest("SlipwayDto is null or incorrect format");

            using (Metrics.CreateHistogram($"slipways_api_duration_POST_api_slipway_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var slipway = slipwayDto.ToClass();

                    var result = await _repository.Slipway.InsertAsync(slipway, cancellationToken);
                    if (result != null && slipwayDto.Extras != null)
                    {
                        var extras = new HashSet<SlipwayExtra>();
                        foreach (var extra in slipwayDto?.Extras)
                        {
                            var slipwayExtra = new SlipwayExtra
                            {
                                Id = Guid.NewGuid(),
                                Created = DateTime.Now,
                                ExtraFk = extra.Id,
                                SlipwayFk = result.Id,
                            };
                            extras.Add(slipwayExtra);
                        }
                        _ = await _repository.SlipwayExtra.InsertRangeAsync(extras, cancellationToken);
                    }
                    slipwayDto.Id = slipway.Id;
                    slipwayDto.Created = result.Created;
                    return new JsonResult(slipway, _options);
                }
                catch (NullReferenceException e)
                {
                    _logger.LogError(6664, $"Error occurred while insert Slipway\n'{slipwayDto?.Name} / {slipwayDto?.City}'", e);
                    return new StatusCodeResult(500);
                }
                catch (ArgumentNullException e)
                {
                    _logger.LogError(6665, $"Error occurred while insert Slipway\n'{slipwayDto?.Name} / {slipwayDto?.City}'", e);
                    return new StatusCodeResult(500);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Unexpected error occurred while insert Slipway\n'{slipwayDto?.Name} / {slipwayDto?.City}'", e);
                    return new StatusCodeResult(500);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(
            Guid id,
            SlipwayDto slipwayDto,
            CancellationToken cancellationToken)
        {
            using (Metrics.CreateHistogram($"slipways_api_duration_PUT_api_slipway_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var slipway = slipwayDto.ToClass();

                    if (slipway.Id != id)
                        return BadRequest("IDs are not the same");

                    var result = await _repository.Slipway.UpdateAsync(slipway, cancellationToken);
                    if (result == null)
                    {
                        _logger.LogError(6660, $"Error occurred while updating Slipway '{slipwayDto.Name} : {id}'");
                        return new StatusCodeResult(500);
                    }

                    // TODO - update not really works atm
                    var extras = new HashSet<SlipwayExtra>();
                    foreach (var extra in slipwayDto.Extras)
                    {
                        var slipwayExtra = new SlipwayExtra
                        {
                            Created = DateTime.Now,
                            ExtraFk = extra.Id,
                            SlipwayFk = result.Id,
                        };
                        extras.Add(slipwayExtra);
                    }
                    _ = await _repository.SlipwayExtra.UpdateRangeAsync(extras, cancellationToken);
                    // ************

                    slipwayDto.Id = slipway.Id;
                    slipwayDto.Created = result.Created;
                    slipwayDto.Updated = result.Updated;

                    return new JsonResult(slipway, _options);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Error occurred while update Slipway", e);
                    return new StatusCodeResult(500);
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlipwayAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest("Slipway ID is not valid");

            using (Metrics.CreateHistogram($"slipways_api_duration_DELETE_api_slipway_seconds", "Histogram").NewTimer())
            {
                try
                {
                    _logger.LogInformation(5555, $"Try to delete Slipway '{id}'");
                    var result = await _repository.Slipway.DeleteAsync(id, cancellationToken);
                    var json = JsonSerializer.Serialize(result, _options);
                    _logger.LogInformation($"Delete Result is:\n{json}");
                    return new OkObjectResult(json);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Error occurred while deleting Slipway with id '{id}'", e);
                    return new StatusCodeResult(500);
                }
            }
        }
    }
}
