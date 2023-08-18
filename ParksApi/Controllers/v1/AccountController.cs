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
    public async Task<ActionResult> Seed([FromBody] SeedViewModel seed)
    {
        if (!ModelState.IsValid && !_db.Parks.Any() && !_db.Centers.Any())
        {
            SeedViewModel.Seed(_db);
            return NoContent("Database seeded.");
        }

        if (!ModelState.IsValid)
        {

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
}