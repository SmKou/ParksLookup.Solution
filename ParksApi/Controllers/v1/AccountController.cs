using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using ParksApi.Models;
using ParksApi.InputModels;

namespace ParksApi.Controllers;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AccountController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public AccountController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        if (!_db.Parks.Any() && !_db.Centers.Any())
        {
            SeedInputModel.Seed(_db);
            return Ok("Database modeled.");
        }
        return NoContent();
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] AccountInputModel model)
    {
        if (!ModelState.IsValid)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("DataInvalid", error.Description);
            return UnprocessableEntity(ModelState);
        }
        if (model.UserId != id)
            return BadRequest();
        if (!_db.Parks.Any(park => park.ParkId == model.ParkId))
            return NotFound(new { "DataInvalid" = "Park does not exist." });
        string username = model.UserName.ToLower();
        string email = model.Email.ToLower();
        if (!_db.Users.Any(user => user.NormalizedUserName == username) 
        && !_db.Users.Any(user => user.NormalizedEmail == email))
            return NotFound(new { "DataInvalid" = "Account does not exist." });
        string userId = userId.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { "DataInvalid" = "Account does not exist." });
        if (user.UserName != model.UserName && user.Email != model.Email)
            return BadRequest(new { "UpdateFailed" = "Cannot change both username and email." });

        if (user.UserName != model.UserName)
            user.UserName = model.UserName;
        else if (user.Email != model.Email)
            user.Email = model.Email;
        if (user.PhoneNumber != model.PhoneNumber)
            user.PhoneNumber = model.PhoneNumber;
        if (user.GivenName != model.GivenName)
            user.GivenName = model.GivenName;

        IdentityResult result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { "UpdateFailed" = "Could not update user." });
        
        if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.NewPassword))
        {
            IdentityResult password = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (!password.Succeeded)
                return BadRequest(new { "UpdateFailed" = "Could not update user." });
        }

        return NoContent();
    }

    public static string GenerateJSONWebToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Kou.dBlueParksLookupApi"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "http://localhost:5006",
            audience: "http://localhost:5006",
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

[Route("api/v{version:apiVersion}/account/[controller]")]
[ApiVersion("1.0")]
public class SeedController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public SeedController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] SeedInputModel model)
    {
        if (!ModelState.IsValid)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("DataInvalid", error.Description);
            return UnprocessableEntity(ModelState);
        }
        if (_db.Parks.Any(park => park.ParkName == model.ParkName))
            return BadRequest(new { "SeedInvalid" = "Park already exists." });

        Park park = new Park
        {
            ParkName = model.ParkName,
            Description = model.Description,
            State = model.State.ToLower(),
            Directions = model.Directions
        };
        _db.Parks.Add(park);
        _db.SaveChanges();

        Application user = new ApplicationUser
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
                return Ok(new { "Token" = GenerateJSONWebToken() });
            else
                return BadRequest(new { "AuthFailed" = "Could not produce token." });
        }
        else
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("RegistrationFailed", error.Description);
            return UnprocessableEntity(ModelState);
        }
    }
}

[Route("api/v{version:apiVersion}/account/[controller]")]
[ApiVersion("1.0")]
public class RegisterController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public SeedController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpPost]
    public async Task<ActionResult> Register([FromBody] UserInputModel model)
    {
        if (!ModelState.IsValid)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("DataInvalid", error.Description);
            return UnprocessableEntity(ModelState);
        }
        if (!_db.Parks.Any(park => park.ParkId == model.ParkId))
            return NotFound;

        ApplicationUser user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            GivenName = model.GivenName,
            ParkId = model.ParkId,
            IsConfirmedEmployee = false
        };
        IdentityResult result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            Microsoft.AspNetCore.Identity.SignInResult _signin = await signinManager.PasswordSignInAsync(model.UserName, model.Password, isPersistent: true, lockoutOnFailure: false);
            if (signin.Succeeded)
                return Ok(new { "Token" = AccountController.GenerateJSONWebToken() });
            else
                return BadRequest(new { "AuthFailed" = "Could not produce token." });
        }
        else
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("RegistrationFailed", error.Description);
            return UnprocessableEntity(ModelState);
        }
    }
}

[Route("api/v{version:apiVersion}/account/[controller]")]
[ApiVersion("1.0")]
public class LoginController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public SeedController(ParksContext db, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _signinManager = signinManager;
    }

    [HttpPost]
    public async Task<ActionResult> Login([FromBody] LoginInputModel login)
    {
        if (!ModelState.IsValid)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("DataInvalid", error.Description);
            return UnprocessableEntity(ModelState);
        }
        string handle = login.UserNameOrEmail;
        if (!_db.Users.Any(user => user.NormalizedUserName == handle.ToLower()
        && !_db.Users.Any(user => user.NormalizedEmail == handle.ToLower())))
            return NotFound();
        ApplicationUser user = _db.Users.SingleOrDefault(user => user.NormalizedEmail == handle.ToLower());
        if (user != null)
            handle = user.UserName;

        Microsoft.AspNetCore.Identity.SignInResult result = await _signinManager.PasswordSignInAsync(handle, login.Password, isPersistent: true, lockoutOnFailure: false);
        if (result.Succeeded)
            return Ok(new { "Token" = AccountController.GenerateJSONWebToken() });
        else
        {
            ModelState.AddModelError("LoginFailed", "There is something wrong with your login or password.");
            return UnprocessableEntity(ModelState);
        }
    }
}