using cloudpatternsapi.interfaces.services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cloudpatternsapi.controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthcheckController : ControllerBase
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthcheckController(IHealthCheckService healthCheckService)
        {
            _healthCheckService = healthCheckService;
        }
        [HttpGet("/db")]
        public async Task<ActionResult> DatabaseHealthCheck()
        {
            var response = await _healthCheckService.DatabaseHealthCheck();
            return Ok(response);
        }
        [HttpGet]
        public async Task<ActionResult> ApiHealthCheck()
        {
            var response = await _healthCheckService.ApiHealthCheck();
            return Ok(response);
        }
    }
}
