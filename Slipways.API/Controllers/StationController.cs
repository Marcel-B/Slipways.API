using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.b_velop.Slipways.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StationController : Controller
    {
        private readonly IRepositoryWrapper _rep;
        private readonly ILogger<StationController> _logger;

        public StationController(
            IRepositoryWrapper rep,
            ILogger<StationController> logger)
        {
            _rep = rep;
            _logger = logger;
        }

        // GET: api/values
        [HttpGet]
        public async Task<IEnumerable<Station>> GetAsync()
        {
            using (Metrics.CreateHistogram($"slipwaysapi_duration_GET_api_station_seconds", "Histogram").NewTimer())
            {
                var result = await _rep.Station.SelectAllAsync();
                return result.OrderBy(_ => _.Longname);
            }
        }

        // GET api/values/8177a148-5674-4b8f-8ded-050907f640f3
        [HttpGet("{id}")]
        public async Task<ActionResult<Station>> GetAsync(
            Guid id)
        {
            using (Metrics.CreateHistogram($"slipwaysapi_duration_GET_api_station_id_seconds", "Histogram").NewTimer())
            {
                return await _rep.Station.SelectByIdAsync(id);
            }
        }
    }
}
