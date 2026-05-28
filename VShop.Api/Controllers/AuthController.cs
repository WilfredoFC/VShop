using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using VShop.Application.Dtos.User;
using VShop.Application.Interfaces;
using VShop.Identity.Entities;

namespace VShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<AppUser> userManager,
    IAccountService accountService,
    IConfiguration config) : ControllerBase
{
    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginApiRequest req)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user == null || !user.Status)
            return Unauthorized(new { error = "Credenciales inválidas." });

        if (!await userManager.CheckPasswordAsync(user, req.Password))
            return Unauthorized(new { error = "Credenciales inválidas." });

        if (!user.EmailConfirmed)
            return Unauthorized(new { error = "Debes confirmar tu correo antes de iniciar sesión." });

        var roles = await userManager.GetRolesAsync(user);
        var token = GenerateToken(user, roles);

        return Ok(new AuthApiResponse(
            Token: token,
            UserId: user.Id,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Roles: [.. roles]
        ));
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterApiRequest req)
    {
        var origin = config["WebBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var saveDto = new SaveUserDto
        {
            UserName = req.Email,
            Email = req.Email,
            Password = req.Password,
            Name = req.FirstName,
            LastName = req.LastName,
            Cedula = req.Cedula ?? "",
            Role = "Cliente"
        };

        var result = await accountService.RegisterUser(saveDto, origin);
        if (result.HasError)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Registro exitoso. Revisa tu correo para confirmar tu cuenta." });
    }

    // GET api/auth/confirm-email?userId=&token=
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
    {
        var result = await accountService.ConfirmAccountAsync(userId, token);
        return Ok(new { message = result });
    }

    // POST api/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordApiRequest req)
    {
        var origin = config["WebBaseUrl"] ?? $"{Request.Scheme}://{Request.Host}";
        var result = await accountService.ForgotPasswordAsync(
            new VShop.Application.Dtos.User.ForgotPasswordRequestDto { UserName = req.Email, Origin = origin });

        if (result.HasError)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Se envió un enlace de recuperación a tu correo." });
    }

    // POST api/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordApiRequest req)
    {
        var result = await accountService.ResetPasswordAsync(
            new VShop.Application.Dtos.User.ResetPasswordRequestDto
            {
                Id = req.UserId,
                Token = req.Token,
                Password = req.Password
            });

        if (result.HasError)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { message = "Contraseña actualizada correctamente." });
    }

    // GET api/auth/me
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var roles = await userManager.GetRolesAsync(user);
        return Ok(new
        {
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Cedula,
            Roles = roles
        });
    }

    private string GenerateToken(AppUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"]!));
        var duration = int.Parse(config["JwtSettings:DurationInMinutes"] ?? "480");

        var token = new JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(duration),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// Request/Response records del módulo Auth
public record LoginApiRequest(string Email, string Password);
public record RegisterApiRequest(string Email, string Password, string ConfirmPassword, string FirstName, string LastName, string? Cedula);
public record ForgotPasswordApiRequest(string Email);
public record ResetPasswordApiRequest(string UserId, string Token, string Password);
public record AuthApiResponse(string Token, string UserId, string Email, string FirstName, string LastName, List<string> Roles);
