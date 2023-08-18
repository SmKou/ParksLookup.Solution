using ParksApi.Models;

namespace ParksApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ParksController : ControllerBase
{
    private readonly ParksContext _db;

    public ParksController(ParksContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Park>>> Get([FromQuery] string name, string state)
    {
        return NoContent();
    }

    [HttpGet("id")]
    public async Task<ActionResult<Park>> GetPark(int id)
    {
        return NoContent();
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Park>> Post([FromBody] Park park)
    {
        return NoContent();
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] Park park)
    {
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return NoContent();
    }
}