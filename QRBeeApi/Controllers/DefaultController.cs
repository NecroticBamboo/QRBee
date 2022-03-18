using Microsoft.AspNetCore.Mvc;

namespace QRBee.Api.Controllers
{
    [Route("/")]
    [ApiController]
    public class DefaultController : Controller
    {
        private readonly ILogger<QRBeeController> _logger;

        public DefaultController(ILogger<QRBeeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public Task<RedirectResult> Get()
        {
            _logger.LogInformation($"Redirecting to Swagger...");
            return Task.FromResult(RedirectPermanent("/swagger"));
        }
    }
}
