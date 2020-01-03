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
        public async Task<IEnumerable<ManufacturerDto>> GetAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var manufacturers = await _repository.Manufacturer.SelectAllAsync(cancellationToken);
                var result = manufacturers.Select(_ => _.ToDto());
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(6666, $"Unexpected error occurred while GET Manufaturers", e);
            }
            return null;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(
            Guid id)
        {
            return "value";
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
                manufacturerDto.Id = result.Id;
                return new JsonResult(manufacturerDto);
            }
            catch (Exception e)
            {
                _logger.LogError(6666, $"Unexpected error occurred while POST manufacturer '{manufacturerDto.Name}'", e);
            }
            return new StatusCodeResult(500);
        }

        // PUT api/manufacturers/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/manufacturers/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
