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
    public async Task<ActionResult> Register([FromBody] RegisterViewModel register)
    {
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> Login([FromBody] LoginViewModel login)
    {
        return NoContent();
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Put(int id, [FromBody] LoginViewModel login)
    {
        return NoContent();
    }
}