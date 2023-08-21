using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ParksApi.Models;

namespace ParksApi.Controllers;

[ApiController]
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

    /// <summary>
    /// Gets list of parks user saved and can be filtered with params
    /// </summary>
    /// <param name="model"></param>
    /// <param name="name"></param>
    /// <param name="state"></param>
    /// <param name="type"></param>
    /// <param name="sortOrder"></param>
    /// <param name="pageSize"></param>
    /// <param name="pageIndex"></param>
    /// <returns>List of parks</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ParkViewModel>>> Get([FromBody] LoginInputModel model, [FromQuery] string name, string state, string type, string sortOrder, int pageSize, int pageIndex)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        ApplicationUser user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            if (user == null)
                return NotFound();
        }

        IQueryable<ParkViewModel> query = _db.UserParks
            .AsQueryable()
            .Where(entry => entry.User == user)
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

    /// <summary>
    /// Add parks to user's list
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Status code</returns>
    [HttpPost]
    public async Task<ActionResult> Post([FromBody] ParksInputModel model)
    {
        List<string> invalidCodes = new List<string>();
        foreach (string park in model.Parks)
            if (!_db.Parks.Any(entry => entry.ParkCode == park))
                invalidCodes.Add(park);
        if (invalidCodes.Count != 0)
            return NotFound("Invalid parkcodes: " + string.Join(", ", invalidCodes));
        
        ApplicationUser user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            if (user == null)
                return NotFound("User does not exist");
        }

        List<string> validCodes = new List<string>();
        foreach (string park in model.Parks)
            if (!_db.UserParks.Any(entry => entry.ParkCode == park && entry.User == user))
                validCodes.Add(park);
        if (validCodes.Count == 0)
            return BadRequest("Park(s) already in user's list");

        UserPark[] joins = new UserPark[validCodes.Count];
        for (int i = 0; i < joins.Length; i++)
            joins[i] = new UserPark { ParkCode = validCodes[i], User = user };
        _db.UserParks.AddRange(joins);
        _db.SaveChanges();
        return Ok("Park(s) added to user list");
    }

    /// <summary>
    /// Remove parks from user's list
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Status code</returns>
    [HttpDelete]
    public async Task<ActionResult> Delete([FromBody] ParksInputModel model)
    {
        List<string> invalidCodes = new List<string>();
        foreach (string park in model.Parks)
            if (!_db.Parks.Any(entry => entry.ParkCode == park))
                invalidCodes.Add(park);
        if (invalidCodes.Count != 0)
            return NotFound("Invalid parkcodes: " + string.Join(", ", invalidCodes));

        ApplicationUser user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
        if (user == null)
        {
            user = await _userManager.FindByNameAsync(model.UserNameOrEmail);
            if (user == null)
                return NotFound("User does not exist");
        }

        List<string> validCodes = new List<string>();
        foreach (string park in model.Parks)
            if (_db.UserParks.Any(entry => entry.ParkCode == park && entry.User == user))
                validCodes.Add(park);
        if (validCodes.Count == 0)
            return BadRequest("User does not have park(s) in their list");

        UserPark[] joins = new UserPark[validCodes.Count];
        for (int i = 0; i < joins.Length; i++)
            joins[i] = _db.UserParks.SingleOrDefault(entry => entry.User == user && entry.ParkCode == validCodes[i]);
        _db.UserParks.RemoveRange(joins);
        await _db.SaveChangesAsync();
        return Ok("Park(s) removed from user list");
    }
}