using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskBoard.Api.Models;
using TaskBoard.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Helpers;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here (without the word Bearer)."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddDbContext<TaskBoardDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing. Set it with user-secrets.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TaskBoard";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TaskBoard";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

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

app.MapPost("/api/auth/login", async (LoginDto dto, TaskBoardDbContext db) =>
{
    if (ValidationHelper.Validate(dto) is { } problem)
        return problem;

    var email = dto.Email.Trim().ToLowerInvariant();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        return Results.Unauthorized();

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

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");



app.MapGet("/api/tasks", async (ClaimsPrincipal user, TaskBoardDbContext db) =>
{
    var userId = user.GetUserId();
    var tasks = await db.Tasks.Where(t => t.UserId == userId).ToListAsync();
    return Results.Ok(tasks);
}).RequireAuthorization();

app.MapGet("/api/tasks/{id}", async (int id, ClaimsPrincipal user, TaskBoardDbContext db) =>
{
    var userId = user.GetUserId();
    var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    if (task is null)
        return Results.NotFound();

    return Results.Ok(task);
}).RequireAuthorization();

app.MapPost("/api/tasks", async (CreateTaskDto dto, ClaimsPrincipal user, TaskBoardDbContext db) =>
{
    if (ValidationHelper.Validate(dto) is { } problem)
        return problem;

    var userId = user.GetUserId();

    var task = new TaskItem
    {
        Title = dto.Title,
        Description = dto.Description,
        Status = dto.Status,
        CreatedAt = DateTime.UtcNow,
        UserId = userId
    };

    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    return Results.Created($"/api/tasks/{task.Id}", task);
}).RequireAuthorization();

app.MapPut("/api/tasks/{id}", async (int id, UpdateTaskDto dto, ClaimsPrincipal user, TaskBoardDbContext db) =>
{
    if (ValidationHelper.Validate(dto) is { } problem)
        return problem;

    var userId = user.GetUserId();
    var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    if (task is null)
        return Results.NotFound();

    task.Title = dto.Title;
    task.Description = dto.Description;
    task.Status = dto.Status;

    await db.SaveChangesAsync();

    return Results.Ok(task);
}).RequireAuthorization();

app.MapDelete("/api/tasks/{id}", async (int id, ClaimsPrincipal user, TaskBoardDbContext db) =>
{
    var userId = user.GetUserId();
    var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    if (task is null)
        return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();

    return Results.NoContent();
}).RequireAuthorization();


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
