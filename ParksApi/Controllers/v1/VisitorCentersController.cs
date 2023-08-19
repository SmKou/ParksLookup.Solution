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
    public async Task<ActionResult<IEnumerable<VisitorCenter>>> Get([FromQuery] string name, int parkId, string sortOrder, int pageSize, int pageIndex)
    {
        IQueryable<VisitorCenter> query = _db.Centers.AsQueryable();
        if (!string.IsNullOrEmpty(name))
            query = query.Where(entry => entry.CenterName.Contains(name));
        if (parkId > 0)
            query = query.Where(entry => entry.ParkId == parkId);
        if (!string.IsNullOrEmpty(sortOrder))
        {
            if (sortOrder == "desc")
                query = query.OrderByDescending(entry => entry.CenterName);
            else
                query = query.OrderBy(entry => entry.CenterName);
        }
        return await PaginatedList<VisitorCenter>.CreateAsync(query, pageIndex, pageSize);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VisitorCenter>> GetCenter(int id)
    {
        VisitorCenter center = await _db.Centers.FindAsync(id);
        if (center == null)
            return NotFound();
        else
            return Ok(center);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<VisitorCenter>> Post([FromBody] VisitorCenter center)
    {
        return NoContent();
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] VisitorCenter center)
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