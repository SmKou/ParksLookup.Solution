using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ParksApi.Models;

namespace ParksApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(ParksContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<IEnumerable<ParkViewModel>>> Get(string username, [FromQuery] string name, string state, string type, string sortOrder, int pageSize, int pageIndex)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.NormalizedUserName != username.ToUpper())
            return Unauthorized();

        IQueryable<ParkViewModel> query = _db.UserParks
            .AsQueryable()
            .Where(entry => entry.UserName == user.NormalizedUserName)
            .Join(_db.Parks,
                userpark => userpark.ParkCode,
                park => park.ParkCode,
                (userpark, park) => new ParkViewModel
                {
                    ParkCode = park.ParkCode,
                    Type = park.IsStatePark ? "state" : "national",
                    StateCode = park.StateCode,
                    FullName = park.FullName,
                    Description = park.Description
                }
            );

        if (!string.IsNullOrEmpty(name))
            query = query.Where(entry => entry.FullName.Contains(name));
        if (!string.IsNullOrEmpty(state))
            query = query.Where(entry => entry.StateCode.Contains(state));
        if (!string.IsNullOrEmpty(type))
        {
            if (type == "state")
                query = query.Where(entry => entry.Type == "state");
            else
                query = query.Where(entry => entry.Type != "state");
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
        return await PaginatedList<ParkViewModel>.CreateAsync(query, pageIndex, pageSize);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] string parkcode, string username)
    {
        if (!_db.Parks.Any(entry => entry.ParkCode == parkcode))
            return NotFound();
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.NormalizedUserName != username.ToUpper())
            return Unauthorized();

        UserPark join = new UserPark
        {
            ParkCode = parkcode,
            UserName = username.ToUpper()
        };
        _db.UserParks.Add(join);
        _db.SaveChanges();
        return Ok("Park added to user list");
    }

    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] string parkcode, string username)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.NormalizedUserName != username.ToUpper())
            return Unauthorized();

        UserPark model = _db.UserParks.SingleOrDefault(entry => entry.ParkCode == parkcode && entry.UserName == user.NormalizedUserName);
        if (model == null)
            return NotFound();
        _db.UserParks.Remove(model);
        await _db.SaveChangesAsync();
        return Ok("Park removed from user list");
    }
}