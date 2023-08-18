using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using ParksApi.Models;
using ParksApi.ViewModels;

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

    [HttpPost]
    public async Task<ActionResult> Seed([FromBody] SeedInputModel seed)
    {
        if (!ModelState.IsValid && !_db.Parks.Any() && !_db.Centers.Any())
        {
            SeedInputModel.Seed(_db);
            return NoContent("Database seeded.");
        }

        if (!ModelState.IsValid)
            return UnprocessableEntity(ModelState);

        Park park = new Park
        {
            Name = seed.ParkName,
            Description = seed.Description,
            State = seed.State.ToLower(),
            Directions = seed.Directions
        };
        _db.Parks.Add(park);
        _db.SaveChanges();
        ApplicationUser user = new ApplicationUser
        {
            UserName = seed.UserName,
            Email = seed.Email,
            Name = seed.GivenName,
            ParkId = park.ParkId,
            IsConfirmedEmployee = true
        };
        IdentityResult result = await _userManager.CreateAsync(user, seed.Password);
        if (result.Succeeded)
        {
            SignInResult signin = await _signinManager.PasswordSignInAsync(user.UserName, user.Password, isPersistent: true, lockoutOnFailure: false);
            if (signin.Succeeded)
                return Ok(GenerateJSONWebToken())
            else
                return StatusCode(StatusCodes.Status500InternalServerError, signin.Failed)
        }
        else
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return UnprocessableEntity(ModelState);
        }
    }

    [HttpPost]
    public async Task<ActionResult> Register([FromBody] UserInputModel register)
    {
        
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Login([FromBody] LoginInputModel login)
    {
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Put(int id, [FromBody] LoginInputModel login)
    {
        return NoContent();
    }

    private string GenerateJSONWebToken()
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Kou.dBlueParks"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: "http://localhost:5006",
            audience: "http://localhost:5006",
            expires: DateTime.Now.AddHours(1);
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}