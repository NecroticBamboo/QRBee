using Microsoft.AspNetCore.Mvc;
using QRBee.Api.Services;
using QRBee.Core;
using QRBee.Core.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace QRBee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRBeeController : ControllerBase
    {
        private readonly IQRBeeAPI _service;
        private readonly ILogger<QRBeeController> _logger;

        public QRBeeController(IQRBeeAPI service, ILogger<QRBeeController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("Register")]
        public Task<RegistrationResponse>  Register([FromBody] RegistrationRequest value)
        {
            _logger.LogInformation($"Trying to register user {value.Name}");
            return _service.Register(value);
        }

    }
}
