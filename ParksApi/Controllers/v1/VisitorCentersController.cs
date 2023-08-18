using ParksApi.Models;

namespace ParksApi.Contorllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class VisitorCentersController : ControllerBase
{
    private readonly ParksContext _db;

    public VisitorCentersController(ParksContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VisitorCenter>>> Get([FromQuery] string name, string park)
    {
        return NoContent();
    }

    [HttpGet("id")]
    public async Task<ActionResult<VisitorCenter>> GetCenter(int id)
    {
        return NoContent();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<VisitorCenter>> Post([FromBody] VisitorCenter center)
    {
        return NoContent();
    }

    [Authorize]
    [HttpPut("id")]
    public async Task<IActionResult> Put(int id, [FromBody] VisitorCenter center)
    {
        return NoContent();
    }

    [Authorize]
    [HttpDelete("id")]
    public async Task<IActionResult> Delete(int id)
    {
        return NoContent();
    }
}