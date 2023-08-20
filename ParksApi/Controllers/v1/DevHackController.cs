using ParksApi.Models;

namespace ParksApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class DevHackController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public DevHackController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult> Get()
    {
        Park parkA = new Park
        {
            ParkName = "Alagnak",
            Description = "The headwaters of Alagnak Wild River lie within the rugged Aleutian Range of neighboring Katmai National Park and Preserve. Meandering west towards Bristol Bay and the Bering Sea, the Alagnak traverses the beautiful Alaska Peninsula, providing an unparalleled opportunity to experience the unique wilderness, wildlife, and cultural heritage of southwest Alaska.",
            State = "alaska",
            Directions = "Alagnak Wild River is located in a remote part of the Alaska Peninsula, about 290 miles southwest of Anchorage. Access is by boat or small floatplane."
        };
        Park parkB = new Park
        {
            ParkName = "Cape Krusenstern",
            Description = "North of the Arctic Circle, the monument forms 70 miles of shoreline on the Chukchi Sea.  More than 114 beach ridges provide evidence of human use for 5,000 years.  The Inupiat continue to use the area today.  Vast wetlands provide habitat for shorebirds from as far away as South America.  Hikers and boaters can see carpets of wildflowers among shrubs containing wisps of qiviut from muskoxen.",
            State = "alaska",
            Directions = "Cape Krusenstern National Monument lies within a remote area of northwest Alaska and is bordered by the Arctic Ocean and Chukchi Sea. Visitors generally access the monument via the regional hub in Kotzebue. Commercial airlines provide daily service from Anchorage to Kotzebue. Chartered flights with licensed air taxi services, booked in advance, can take backcountry travelers to remote destinations within the monument."
        };
        Park parkC = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkD = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkE = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkF = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkG = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkH = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkI = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkJ = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        Park parkK = new Park
        {
            ParkName = "",
            Description = "",
            State = "",
            Directions = ""
        };
        _db.Parks.AddRange(new Park[]
        {
            parkA,
            parkB,
            parkC,
            parkD,
            parkE,
            parkF,
            parkG,
            parkH,
            parkI,
            parkJ,
            parkK
        });
    }

    [Authorize]
    [HttpGet("{username}")]
    public async Task<ActionResult> GetUser(string username)
    {
        ApplicationUser user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        user.IsConfirmedEmployee = true;
        IdentityResult result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
            return Ok("Confirmed");
        else
            return StatusCode(418);
    }


    [HttpPost]
    public async Task<ActionResult> Post([FromBody] SeedInputModel model)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        if (_db.Parks.Any(park => park.ParkName == model.ParkName))
            return BadRequest("Data Invalid: Park already exists.");

        Park park = new Park
        {
            ParkName = model.ParkName,
            Description = model.Description,
            State = model.State.ToLower(),
            Directions = model.Directions
        };
        _db.Parks.Add(park);
        _db.SaveChanges();

        ApplicationUser user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            GivenName = model.FullName,
            ParkId = park.ParkId,
            IsConfirmedEmployee = false
        };
        IdentityResult result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            Microsoft.AspNetCore.Identity.SignInResult signin = await _signinManager.PasswordSignInAsync(model.UserName, model.Password, isPersistent: true, lockoutOnFailure: false);
            if (signin.Succeeded)
                return Ok("Token: " + AccountController.GenerateJSONWebToken());
            else
                return BadRequest("Auth Failed: Could not produce token.");
        }
        else
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("Registration Failed", error.Description);
            return UnprocessableEntity(ModelState);
        }
    }
}