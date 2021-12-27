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

        public QRBeeController(IQRBeeAPI service)
        {
            _service = service;
        }

        [HttpPost]
        public Task<RegistrationResponse>  Register([FromBody] RegistrationRequest value)
        {
            return _service.Register(value);
        }

    }
}
