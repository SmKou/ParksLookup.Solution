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
    public async Task<ActionResult<IEnumerable<Park>>> Get([FromQuery] string name, string state, string sortOrder, int pageSize, int pageIndex)
    {
        
        IQueryable<Park> query = _db.Parks.AsQueryable();
        if (!string.IsNullOrEmpty(name))
            query = query.Where(entry => entry.ParkName.Contains(name));
        if (!string.IsNullOrEmpty(state))
        {
            state = state.ToLower();
            query = query.Where(entry => entry.State == state);
        }
        if (!string.IsNullOrEmpty(sortOrder))
        {
            if (sortOrder == "desc")
                query = query.OrderByDescending(entry => entry.ParkName);
            else
                query = query.OrderBy(entry => entry.ParkName);
        }
        return await PaginatedList<Park>.CreateAsync(query, pageIndex, pageSize);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Park>> GetPark(int id)
    {
        Park park = await _db.Parks.FindAsync(id);
        if (park == null)
            return NotFound();
        else
            return Ok(park);
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