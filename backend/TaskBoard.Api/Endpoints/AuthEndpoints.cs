using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskBoard.Api.Data;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Helpers;
using TaskBoard.Api.Models;

namespace TaskBoard.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register", async (RegisterDto dto, TaskBoardDbContext db) =>
        {
            if (ValidationHelper.Validate(dto) is { } problem)
                return problem;

            var email = dto.Email.Trim().ToLowerInvariant();
            var exists = await db.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return Results.Conflict(new { message = "Email is already registered." });

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/auth/register/{user.Id}", new { user.Id, user.Email });
        });

        app.MapPost("/api/auth/login", async (LoginDto dto, TaskBoardDbContext db, IConfiguration config) =>
        {
            if (ValidationHelper.Validate(dto) is { } problem)
                return problem;

            var email = dto.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Results.Unauthorized();

            var jwtKey = config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing.");
            var jwtIssuer = config["Jwt:Issuer"] ?? "TaskBoard";
            var jwtAudience = config["Jwt:Audience"] ?? "TaskBoard";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new
            {
                token = tokenString,
                email = user.Email,
                userId = user.Id
            });
        });
    }
}