using Microsoft.AspNetCore.Mvc;
using SharedModels;
using ApiService.Services;

namespace ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly IRabbitMQService _rabbitMQService;

        public DevicesController(IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        [HttpPost]
        public IActionResult CreateDevice([FromBody] Device device)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _rabbitMQService.SendMessage(device);
                return Ok(new { Message = "Device creation request has been sent" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error processing request", Error = ex.Message });
            }
        }
    }
} 