using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using backend.Models;
using backend.Repositories;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpmController : ControllerBase
    {
        private readonly ISpmRepository _repository;
        private readonly ILogger<SpmController> _logger;

        public SpmController(ISpmRepository repository, ILogger<SpmController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("sections")]
        public async Task<IActionResult> GetSections()
        {
            try
            {
                var sections = await _repository.GetSectionsAsync();
                return Ok(sections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sections");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("equip-l1")]
        public async Task<IActionResult> GetEquipL1([FromQuery] string section)
        {
            if (string.IsNullOrWhiteSpace(section))
            {
                return BadRequest("Section parameter is required.");
            }

            try
            {
                var equipL1 = await _repository.GetEquipL1Async(section);
                return Ok(equipL1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Equipment Level 1 for section {Section}", section);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("equip-l2")]
        public async Task<IActionResult> GetEquipL2([FromQuery] string section, [FromQuery] string equipL1)
        {
            if (string.IsNullOrWhiteSpace(section) || string.IsNullOrWhiteSpace(equipL1))
            {
                return BadRequest("Section and EquipL1 parameters are required.");
            }

            try
            {
                var equipL2 = await _repository.GetEquipL2Async(section, equipL1);
                return Ok(equipL2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Equipment Level 2 for section {Section}, L1 {L1}", section, equipL1);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("grey-parts")]
        public async Task<IActionResult> GetGreyParts([FromQuery] string equipL2Id)
        {
            if (string.IsNullOrWhiteSpace(equipL2Id))
            {
                return BadRequest("Equipment Level 2 ID is required.");
            }

            try
            {
                var greyParts = await _repository.GetGreyPartsAsync(equipL2Id);
                if (greyParts == null)
                {
                    return NotFound($"Equipment details not found for ID: {equipL2Id}");
                }
                return Ok(greyParts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching details for L2 equipment {L2Id}", equipL2Id);
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("observation-types")]
        public async Task<IActionResult> GetObservationTypes()
        {
            try
            {
                var types = await _repository.GetObservationTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching observation types");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("affected-portions")]
        public async Task<IActionResult> GetAffectedPortions()
        {
            try
            {
                var portions = await _repository.GetAffectedPortionsAsync();
                return Ok(portions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching affected portions");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("save-observation")]
        public async Task<IActionResult> SaveObservation([FromForm] SpmObservationFormModel form)
        {
            if (form == null || string.IsNullOrWhiteSpace(form.EquipIdL2))
            {
                return BadRequest("Invalid observation data: Equipment Level 2 ID is required.");
            }

            try
            {
                string? attachmentName = null;
                string? fileExtension = null;

                if (form.Attachment != null && form.Attachment.Length > 0)
                {
                    // Handle file upload
                    fileExtension = Path.GetExtension(form.Attachment.FileName);
                    attachmentName = $"{Guid.NewGuid()}{fileExtension}";

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var filePath = Path.Combine(uploadsFolder, attachmentName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await form.Attachment.CopyToAsync(stream);
                    }
                    _logger.LogInformation("File saved successfully to {Path}", filePath);
                }

                var input = new SpmObservationInput
                {
                    EquipIdL2 = form.EquipIdL2,
                    ObsType = form.ObsType,
                    AffectedP = form.AffectedP,
                    DefDetails = form.DefDetails,
                    DiameterNew = form.DiameterNew,
                    HardnessNew = form.HardnessNew,
                    LiningCondNew = form.LiningCondNew,
                    BearingCondNew = form.BearingCondNew,
                    BakelitePlateCondNew = form.BakelitePlateCondNew,
                    SeverityStatus = form.SeverityStatus,
                    SpAuditDate = form.SpAuditDate,
                    LastRollChangeDate = form.LastRollChangeDate,
                    LastBearGreaseDate = form.LastBearGreaseDate
                };

                bool saved = await _repository.SaveObservationAsync(input, attachmentName, fileExtension);
                if (saved)
                {
                    return Ok(new { success = true, message = "Observation saved successfully." });
                }
                else
                {
                    return StatusCode(500, "Failed to save the observation to the database.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving observation");
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }

    public class SpmObservationFormModel
    {
        public string EquipIdL2 { get; set; } = string.Empty;
        public string ObsType { get; set; } = string.Empty;
        public string AffectedP { get; set; } = string.Empty;
        public string DefDetails { get; set; } = string.Empty;
        public string DiameterNew { get; set; } = string.Empty;
        public string HardnessNew { get; set; } = string.Empty;
        public string LiningCondNew { get; set; } = string.Empty;
        public string BearingCondNew { get; set; } = string.Empty;
        public string BakelitePlateCondNew { get; set; } = string.Empty;
        public string SeverityStatus { get; set; } = string.Empty;
        public DateTime? SpAuditDate { get; set; }
        public DateTime? LastRollChangeDate { get; set; }
        public DateTime? LastBearGreaseDate { get; set; }
        public IFormFile? Attachment { get; set; }
    }
}
