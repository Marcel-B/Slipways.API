using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> PostPortAsync(
            PortDto portDto,
            CancellationToken cancellationToken)
        {
            if (portDto == null || string.IsNullOrWhiteSpace(portDto.Name))
            {
                _logger.LogWarning($"Error occurred while POST new Port. No name provided");
                return BadRequest("Value null or incorrect format");
            }
            try
            {
                var port = portDto.ToClass();
                port = await _repository.Port.InsertAsync(port, cancellationToken);
                if (port == null)
                {
                    _logger.LogWarning(5555, $"Error occurred while inserting Port '{portDto.Name}'");
                    return new StatusCodeResult(500);
                }
                portDto.Id = port.Id;
                if (port.Slipways != null)
                {
                    foreach (var slipway in port.Slipways)
                    {
                        var tmp = await _repository.Slipway.SelectByIdAsync(slipway.Id, cancellationToken);
                        tmp.PortFk = port.Id;
                        _ = await _repository.Slipway.UpdateAsync(tmp.Copy());
                    }
                }
                return new JsonResult(portDto);
            }
            catch (Exception e)
            {
                _logger.LogError(6666, $"Unexpected error occurred while inserting Port '{portDto.Name}'\n{e.StackTrace}", e);
            }
            return new StatusCodeResult(500);
        }
    }
}