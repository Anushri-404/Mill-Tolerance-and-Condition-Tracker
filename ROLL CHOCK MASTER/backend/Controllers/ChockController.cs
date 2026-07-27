using Microsoft.AspNetCore.Mvc;
using RollChockBackend.Models;
using RollChockBackend.Repositories;

namespace RollChockBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChockController : ControllerBase
    {
        private readonly IChockRepository _repository;
        private readonly ILogger<ChockController> _logger;

        public ChockController(IChockRepository repository, ILogger<ChockController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("lookups")]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var lookups = await _repository.GetLookupsAsync();
                return Ok(lookups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chock lookups");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("query")]
        public async Task<IActionResult> Query([FromQuery] string chockId, [FromQuery] string chockType)
        {
            if (string.IsNullOrWhiteSpace(chockId) || string.IsNullOrWhiteSpace(chockType))
            {
                return BadRequest("Enter Chock Type and Chock ID");
            }

            try
            {
                var result = await _repository.QueryChockAsync(chockId, chockType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying chock {ChockId}/{ChockType}", chockId, chockType);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("type-config")]
        public async Task<IActionResult> GetTypeConfig([FromQuery] string chockType, [FromQuery] string? chockId)
        {
            if (string.IsNullOrWhiteSpace(chockType))
            {
                return BadRequest("Chock Type is required.");
            }

            try
            {
                var config = await _repository.GetTypeConfigAsync(chockType, chockId);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching type config for {ChockType}", chockType);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] ChockSaveRequest input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.CHM_CHK_TYP) || string.IsNullOrWhiteSpace(input.CHM_ID_CHOCK))
            {
                return BadRequest("Chock Type and Chock ID are required.");
            }

            try
            {
                var (success, wasUpdate) = await _repository.SaveChockAsync(input);
                return Ok(new { success, wasUpdate });
            }
            catch (InvalidOperationException ex)
            {
                // Validation-style failures (missing maker, wrong status) —
                // mirrors the legacy trigger's MESSAGE()/FORM_TRIGGER_FAILURE.
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving chock");
                return StatusCode(500, new { success = false, message = $"Record Failed: {ex.Message}" });
            }
        }
    }
}
