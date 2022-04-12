using Microsoft.AspNetCore.Mvc;
using QRBee.Api.Services;
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

        [HttpGet]
        public Task<RedirectResult> Get()
        {
            _logger.LogInformation($"Redirecting to Swagger...");
            return Task.FromResult(RedirectPermanent("/swagger"));
        }

        [HttpPost("Register")]
        public Task<RegistrationResponse>  Register([FromBody] RegistrationRequest value)
        {
            _logger.LogInformation($"Trying to register user {value.Name}");
            return _service.Register(value);
        }

        [HttpPatch("Update/{clientId}")]
        public Task Update([FromRoute] string clientId, [FromBody] RegistrationRequest value)
        {
            _logger.LogInformation($"Trying to update user {value.Name}");
            return _service.Update(clientId,value);
        }

        [HttpPost("Pay")]
        public Task<PaymentResponse> Pay([FromBody] PaymentRequest value)
        {
            _logger.LogInformation($"Trying to insert new transaction {value.ClientResponse.MerchantRequest.MerchantTransactionId}");
            return _service.Pay(value);
        }

        [HttpPost("ConfirmPay")]
        public Task ConfirmPay([FromBody] PaymentConfirmation value)
        {
            _logger.LogInformation($"Trying to confirm transaction with gatewayTransactionId: {value.GatewayTransactionId}");
            return _service.ConfirmPay(value);
        }
    }
}
