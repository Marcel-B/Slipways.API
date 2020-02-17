using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtraController : ControllerBase
    {
        private readonly IRepositoryWrapper _repository;
        private readonly JsonSerializerOptions _options;
        private readonly ILogger<ExtraController> _logger;

        public ExtraController(
            IRepositoryWrapper repository,
            ILogger<ExtraController> logger)
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

        [HttpPost]
        public async Task<IActionResult> PostAsync(
            ExtraDto extraDto,
            CancellationToken cancellationToken)
        {
            if (extraDto == null || string.IsNullOrWhiteSpace(extraDto.Name))
                return BadRequest("Extra has not the correct format or is null");

            using (Metrics.CreateHistogram($"slipways_api_duration_POST_api_extra_seconds", "Histogram").NewTimer())
            {
                try
                {
                    var extra = extraDto.ToClass();
                    var result = await _repository.Extra.InsertAsync(extra, cancellationToken);
                    if (result == null)
                    {
                        _logger.LogError(6600, $"Error occurred while inserting Extra '{extraDto.Name}'");
                        return new StatusCodeResult(500);
                    }
                    extraDto.Id = result.Id;
                    return new JsonResult(extraDto, _options);
                }
                catch (Exception e)
                {
                    _logger.LogError(6666, e, $"Unexpected error occurred while insert '{extraDto.Name}'");
                    return new StatusCodeResult(500);
                }
            }
        }
    }
}
