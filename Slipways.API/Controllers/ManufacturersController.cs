using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManufacturersController : Controller
    {
        private readonly IRepositoryWrapper _repository;
        private readonly ILogger<ManufacturersController> _logger;

        public ManufacturersController(
            IRepositoryWrapper repository,
            ILogger<ManufacturersController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // GET: api/manufacturers
        [HttpGet]
        public async Task<IActionResult> GetAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var manufacturers = await _repository.Manufacturer.SelectAllAsync(cancellationToken);
                var result = manufacturers.Select(_ => _.ToDto());
                return new OkObjectResult(result);
            }
            catch (Exception e)
            {
                _logger.LogError(6666, e, $"Unexpected error occurred while GET Manufaturers");
                return new StatusCodeResult(500);
            }
        }

        // POST api/manufacturers
        [HttpPost]
        public async Task<IActionResult> PostAsync(
            ManufacturerDto manufacturerDto)
        {
            if (manufacturerDto == null || string.IsNullOrWhiteSpace(manufacturerDto.Name))
            {
                _logger.LogWarning(5000, $"Error occurred while POST Manufacturer - Value null or incorrect format");
                return BadRequest("Value null or incorrect format");
            }

            try
            {
                var manufacturer = manufacturerDto.ToClass();
                var result = await _repository.Manufacturer.InsertAsync(manufacturer);

                if (result == null)
                {
                    _logger.LogError(5005, $"Error occurred while POST manufacturer '{manufacturerDto.Name}'. InsertAsync results null");
                    return new StatusCodeResult(500);
                }

                manufacturerDto.Id = result.Id;
                return new JsonResult(manufacturerDto, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
            catch (Exception e)
            {
                _logger.LogError(6666, e, $"Unexpected error occurred while POST manufacturer '{manufacturerDto.Name}'");
            }
            return new StatusCodeResult(500);
        }
    }
}

