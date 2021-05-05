using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CleanArchitecture.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebHookController : ControllerBase
    {
        [HttpPost, Route("/stj")]
        public IActionResult ParseWithDefault()
        {
            return Ok();
        }

        [HttpPost, Route("/manual")]
        public async Task<IActionResult> ParseManual()
        {
            var foo = await Request.BodyReader.ReadAsync();
            return Ok();
        }
    }
}
