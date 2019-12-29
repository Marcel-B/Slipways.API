using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
        private readonly IRepositoryWrapper _rep;
        private readonly ILogger<SlipwaysController> _logger;

        public SlipwaysController(
            IRepositoryWrapper rep,
            ILogger<SlipwaysController> logger)
        {
            _options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                IgnoreNullValues = true
            };
            _rep = rep;
            _logger = logger;
        }

        // GET: api/slipway
        [HttpGet]
        public async Task<IEnumerable<Slipway>> GetAsync()
        {
            using (Metrics.CreateHistogram($"slipwaysapi_duration_GET_api_slipway_seconds", "Histogram").NewTimer())
            {
                var result = await _rep.Slipway.SelectAllAsync();
                return result.OrderBy(_ => _.Name);
            }
        }

        // GET api/slipway/8177a148-5674-4b8f-8ded-050907f640f3
        [HttpGet("{id}")]
        public async Task<ActionResult<Slipway>> GetAsync(
            Guid id)
        {
            using (Metrics.CreateHistogram($"slipwaysapi_duration_GET_api_slipway_id_seconds", "Histogram").NewTimer())
            {
                return await _rep.Slipway.SelectByIdAsync(id);
            }
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(
            SlipwayDto slipwayDto)
        {
            using (Metrics.CreateHistogram($"slipwaysapi_duration_POST_api_slipway_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var slipway = slipwayDto.ToClass();
                    slipway.Id = Guid.NewGuid();

                    var result = await _rep.Slipway.InsertAsync(slipway);
                    if (result != null && slipwayDto.Extras != null)
                    {
                        var extras = new HashSet<SlipwayExtra>();
                        foreach (var extra in slipwayDto.Extras)
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
                        _ = await _rep.SlipwayExtra.InsertRangeAsync(extras);
                    }
                    slipwayDto.Id = slipway.Id;
                    slipwayDto.Created = result.Created;
                    return new JsonResult(slipway, _options);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Error occurred while insert Slipway", e);
                    return new StatusCodeResult(500);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> PutAsync(
            Guid id,
            SlipwayDto slipwayDto)
        {
            using (Metrics.CreateHistogram($"slipwaysapi_duration_PUT_api_slipway_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var slipway = slipwayDto.ToClass();
                    slipway.Id = Guid.NewGuid();

                    var result = await _rep.Slipway.UpdateAsync(slipway);
                    if (result != null)
                    {
                        var extras = new HashSet<SlipwayExtra>();
                        foreach (var extra in slipwayDto.Extras)
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
                        _ = await _rep.SlipwayExtra.UpdateRangeAsync(extras);
                    }
                    slipwayDto.Id = slipway.Id;
                    slipwayDto.Created = result.Created;
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
        public async Task<string> DeleteSlipwayAsync(
            Guid id)
        {
            using (Metrics.CreateHistogram($"slipwaysapi_duration_DELETE_api_slipway_seconds", "Histogram").NewTimer())
            {
                try
                {
                    _logger.LogInformation(5555, $"Try to delete Slipway '{id}'");
                    var result = await _rep.Slipway.DeleteAsync(id);
                    var json = JsonSerializer.Serialize(result, _options);
                    _logger.LogInformation($"Delete Result is:\n{json}");
                    Response.StatusCode = 200;
                    return json;
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, $"Error occurred while deleting Slipway with id '{id}'", e);
                    Response.StatusCode = 500;
                    return string.Empty;
                }
            }
        }
    }
}
