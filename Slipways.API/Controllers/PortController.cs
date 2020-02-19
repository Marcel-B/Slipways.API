using System;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortController : ControllerBase
    {
        private IRepositoryWrapper _repository;
        private ILogger<PortController> _logger;

        public PortController(
            IRepositoryWrapper repository,
            ILogger<PortController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync(
            PortDto portDto,
            CancellationToken cancellationToken)
        {
            if (portDto == null || string.IsNullOrWhiteSpace(portDto.Name))
            {
                //_logger.LogWarning(5000, $"Error occurred while POST new Port. Dto is null or no name provided");
                return BadRequest("Value null or incorrect format");
            }

            try
            {
                var port = portDto.ToClass();
                var slipways = port.Slipways;
                port.Slipways = null;
                port = await _repository.Port.InsertAsync(port, cancellationToken, false);
                if (port == null)
                {
                    _logger.LogError(5005, $"Error occurred while inserting Port '{portDto.Name}'");
                    return new StatusCodeResult(500);
                }
                _repository.SaveChanges();
                portDto.Id = port.Id;
                if (slipways != null)
                {
                    foreach (var slipway in slipways)
                    {
                        var tmp = await _repository.Slipway.AddPortToSlipwayAsync(slipway.Id, port.Id);
                        _logger.LogInformation($"Add Port '{port.Name} - {port.Id}' to Slipway '{slipway?.Name} - {slipway?.Id}'");
                    }
                }
                _repository.SaveChanges();
                return new JsonResult(portDto);
            }
            catch (Exception e)
            {
                _logger.LogError(6666, e, $"Unexpected error occurred while inserting Port '{portDto.Name}'");
            }
            return new StatusCodeResult(500);
        }
    }
}