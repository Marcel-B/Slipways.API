using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Slipways.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SlipwaysController : ControllerBase
    {

        private readonly ILogger<SlipwaysController> _logger;

        public SlipwaysController(ILogger<SlipwaysController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<object> Get()
        {
            return null;
        }
    }
}
