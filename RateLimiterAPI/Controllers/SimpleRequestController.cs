using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RateLimiter.Domain;
using RateLimiter.Domain.Interfaces;

namespace RateLimiterAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SimpleRequestControllerController : ControllerBase
    {
        private readonly ILogger<SimpleRequestControllerController> _logger;
        private readonly IRuleRunner _ruleRunner;
        public readonly IConfiguration _configuration;

        public SimpleRequestControllerController(ILogger<SimpleRequestControllerController> logger, IConfiguration configuration, IRuleRunner ruleRunner)
        {
            _logger = logger;
            _ruleRunner = ruleRunner;
            _configuration = configuration;
        }

        [HttpGet()]
        [Route("/Token/{token}")]
        public async Task<ObjectResult> Get(string token)
        {
            try
            {
                 var results = await _ruleRunner.RunRules(ParseToken(token), PopulateConfigurations());

                if (results.Status == false) 
                {
                    return new TooManyRequestsObjectResult(results.Message, results.RetryAfter);
                }
                return new ObjectResult(results.Message);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                return new BadRequestObjectResult(ex.Message);
            }
        }

        private RateLimiter.Domain.Models.RateLimiterRequest ParseToken(string token)
        {
            RateLimiter.Domain.Models.RateLimiterRequest request = new RateLimiter.Domain.Models.RateLimiterRequest();

            var bytes = Convert.FromBase64String(token);
            var decodedString = System.Text.Encoding.ASCII.GetString(bytes);

            var properties = decodedString.Split(',',StringSplitOptions.None);
            string country = properties[1];
            var countryCode = (RateLimiter.Domain.Enumerations.Contries)Enum.Parse(typeof(RateLimiter.Domain.Enumerations.Contries), country);

            return new RateLimiter.Domain.Models.RateLimiterRequest
            {
                Id = Guid.Parse(properties[0]),
                Country = countryCode,
                RequestDate = DateTime.UtcNow
            };
        }

        private RateLimiter.Domain.Models.Configurations PopulateConfigurations()
        {
            return new RateLimiter.Domain.Models.Configurations
            {
              RequestsPerTimespan = _configuration.GetValue<int>("RequestsPerTimespan"),  
              RequestTimespan = _configuration.GetValue<int>("RequestTimespan"),
              TimespanSinceLastCall = _configuration.GetValue<int>("TimespanSinceLastCall")
            };
        }
    }
}
