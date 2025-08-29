using BlazorWebPoc.ApiService.Model;
using BlazorWebPoc.ApiService.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static BlazorWebPoc.ApiService.Service.impl.CvrServiceImpl;

namespace BlazorWebPoc.ApiService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class CvrController : ControllerBase
    {
        private readonly ICvrService _cvrService;

        public CvrController(ICvrService cvrService)
        {
            _cvrService = cvrService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<CvrSearchResponse>> SearchCvr([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                return BadRequest("Query must be at least 3 characters long");
            }

            var results = await _cvrService.SearchCvrAsync(query).ConfigureAwait(false);    
            return Ok(results);
        }

        [HttpGet("details/{cvrNumber}")]
        public async Task<ActionResult<CvrSearchResult>> GetCvrDetails(string cvrNumber)
        {
            var result = await _cvrService.GetCvrDetailsAsync(cvrNumber).ConfigureAwait(false);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
