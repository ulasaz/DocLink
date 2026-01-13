using DocLink.Services.DTO_s;
using DocLink.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DocLink.WebAPI.Controllers;

[ApiController]
[Route("api/specialists")]
public class SpecialistController : ControllerBase
{ 
    private ISpecialistService _specialistService;

    public SpecialistController(ISpecialistService specialistService)
    {
        _specialistService = specialistService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<SpecialistDto>>> GetAllSpecialistsDtoAsync()
    {
        return Ok(await _specialistService.GetAllSpecialistsDtoAsync());
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<SpecialistDetailsDto>>> GetSpecialistProfile(Guid id)
    {
        try 
        {
            var result = await _specialistService.GetAllDataAboutSpecialist(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}