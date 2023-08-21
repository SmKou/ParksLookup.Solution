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

    /// <summary>
    /// Gets list of parks based on params
    /// </summary>
    /// <param name="name"></param>
    /// <param name="state"></param>
    /// <param name="type"></param>
    /// <param name="sortOrder"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageIndex"></param>
    /// <returns>List of parks</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkViewModel>>> Get([FromQuery] string name, string state, string type, string sortOrder, int pageSize, int pageIndex)
    {
        
        IQueryable<Park> query = _db.Parks.AsQueryable();
        if (!string.IsNullOrEmpty(name))
            query = query.Where(entry => entry.FullName.Contains(name));
        if (!string.IsNullOrEmpty(state))
        {
            state = state.ToUpper();
            query = query.Where(entry => entry.StateCode.Contains(state));
        }
        if (!string.IsNullOrEmpty(type))
        {
            if (type == "state")
                query = query.Where(entry => entry.IsStatePark);
            else if (type == "national")
                query = query.Where(entry => !entry.IsStatePark);
        }
        if (!string.IsNullOrEmpty(sortOrder))
        {
            switch (sortOrder)
            {
                case "desc":
                    query = query.OrderByDescending(entry => entry.FullName);
                    break;
                default:
                    query = query.OrderBy(entry => entry.FullName);
                    break;
            }
        }
        IQueryable<ParkViewModel> model = query.Select(entry => new ParkViewModel
        {
            ParkCode = entry.ParkCode,
            Type = entry.IsStatePark ? "state" : "national",
            StateCode = entry.StateCode,
            FullName = entry.FullName,
            Description = entry.Description
        });
        return await PaginatedList<ParkViewModel>.CreateAsync(model, pageIndex, pageSize);
    }

    /// <summary>
    /// Get park information of specified parkcode
    /// </summary>
    /// <param name="code"></param>
    /// <returns>Park information</returns>
    [HttpGet("{code}")]
    public async Task<ActionResult<ParkViewModel>> GetPark(string code)
    {
        Park park = await _db.Parks.SingleOrDefaultAsync(entry => entry.ParkCode == code);
        if (park == null)
            return NotFound();
        ParkViewModel model = new ParkViewModel
        {
            ParkCode = park.ParkCode,
            Type = park.IsStatePark ? "state" : "national",
            StateCode = park.StateCode,
            FullName = park.FullName,
            Description = park.Description
        };
        return Ok(model);
    }
}