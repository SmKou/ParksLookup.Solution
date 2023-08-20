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
using ParksApi.ViewModels;

namespace ParksApi.Controllers;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
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
    public async Task<ActionResult> Get([FromQuery]
    string name, string username, int parkid, int pageSize, int pageIndex)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsConfirmedEmployee)
            return Unauthorized();

        if (string.IsNullOrEmpty(name))
            name = "";
        if (string.IsNullOrEmpty(username))
            username = "";

        IQueryable<UserViewModel> query = _db.Users
            .AsQueryable()
            .OrderBy(entry => entry.GivenName)
            .Where(entry => entry.NormalizedUserName.Contains(username) && entry.GivenName.Contains(name))
            .Select(user => new UserViewModel
            {
                FullName = user.GivenName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ParkId = user.ParkId
            });

        if (parkid != 0 && _db.Parks.Any(park => park.ParkId == parkid))
            query = query.Where(entry => entry.ParkId == parkid);

        PaginatedList<UserViewModel> model = await PaginatedList<UserViewModel>.CreateAsync(query, pageIndex, pageSize);
        return Ok(model);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<UserViewModel>> GetAccount(string username)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser account = await _userManager.FindByIdAsync(userId);
        if (account == null)
            return Unauthorized();

        ApplicationUser user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return NotFound();

        UserViewModel model = new UserViewModel
        {
            FullName = user.GivenName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ParkId = user.ParkId
        };
        return Ok(model);
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] UserInputModel model)
    {
        model.IsConfirmedEmployee = true;
        return RedirectToRoute(new { action = "Register", controller = "Register", model = model });
    }

    [HttpPut("{username}")]
    public async Task<IActionResult> Put(string username, [FromBody] AccountInputModel model)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        if (model.UserName != username)
            return BadRequest();
        if (!_db.Parks.Any(park => park.ParkId == model.ParkId))
            return NotFound("Data Invalid: Park does not exist.");
            
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        string normalize = username.ToUpper();
        if (user.NormalizedUserName != normalize)
            return Unauthorized("Cannot modify another user's account.");
        if (user.NormalizedUserName != normalize && user.NormalizedEmail != model.Email.ToUpper())
            return BadRequest("Update Invalid: Cannot change both username and email.");

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
            return BadRequest("Update Failed: Could not update user.");
        
        if (!string.IsNullOrEmpty(model.Password) && !string.IsNullOrEmpty(model.NewPassword))
        {
            IdentityResult password = await _userManager.ChangePasswordAsync(user, model.Password, model.NewPassword);
            if (!password.Succeeded)
                return BadRequest("Update Failed: Could not update user.");
        }

        return Ok("User updated");
    }

    [HttpDelete("{username}")]
    public async Task<ActionResult> Delete(string username)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user.NormalizedUserName != username.ToUpper())
            return Unauthorized("Cannot delete another user's account.");
        IdentityResult result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
            return Ok("User deleted");
        else
            return BadRequest("User could not be deleted.");
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
public class RegisterController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public RegisterController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpPost]
    public async Task<ActionResult> Register([FromBody] UserInputModel model)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        if (!_db.Parks.Any(park => park.ParkId == model.ParkId))
            return NotFound();

        ApplicationUser user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            GivenName = model.GivenName,
            ParkId = model.ParkId,
            IsConfirmedEmployee = model.IsConfirmedEmployee != null ? model.IsConfirmedEmployee : false
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

[Route("api/v{version:apiVersion}/account/[controller]")]
[ApiVersion("1.0")]
public class LoginController : ControllerBase
{
    private readonly ParksContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signinManager;

    public LoginController(ParksContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signinManager)
    {
        _db = db;
        _userManager = userManager;
        _signinManager = signinManager;
    }

    [HttpPost]
    public async Task<ActionResult> Post([FromBody] LoginInputModel login)
    {
        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);
        string handle = login.UserNameOrEmail;
        if (!_db.Users.Any(user => user.NormalizedUserName == handle.ToLower()
        && !_db.Users.Any(user => user.NormalizedEmail == handle.ToLower())))
            return NotFound();
        ApplicationUser user = _db.Users.SingleOrDefault(user => user.NormalizedEmail == handle.ToLower());
        if (user != null)
            handle = user.UserName;

        Microsoft.AspNetCore.Identity.SignInResult result = await _signinManager.PasswordSignInAsync(handle, login.Password, isPersistent: true, lockoutOnFailure: false);
        if (result.Succeeded)
            return Ok("Token: " + AccountController.GenerateJSONWebToken());
        else
        {
            ModelState.AddModelError("Login Failed", "There is something wrong with your login or password.");
            return UnprocessableEntity(ModelState);
        }
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        ApplicationUser user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound();
        await _signinManager.SignOutAsync();
        return Ok("User logged out.");
    }
}