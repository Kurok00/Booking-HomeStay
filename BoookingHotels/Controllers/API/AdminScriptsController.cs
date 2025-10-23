using BoookingHotels.Data;
using BoookingHotels.Scripts;
using Microsoft.AspNetCore.Mvc;

namespace BoookingHotels.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminScriptsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminScriptsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// API endpoint để fix duplicate amenities
        /// GET: api/AdminScripts/fix-duplicate-amenities
        /// </summary>
        [HttpGet("fix-duplicate-amenities")]
        public async Task<IActionResult> FixDuplicateAmenitiesEndpoint()
        {
            try
            {
                await Scripts.FixDuplicateAmenities.FixAsync(_context);
                return Ok(new
                {
                    success = true,
                    message = "✅ Fixed duplicate amenities successfully!"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"❌ Error: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
