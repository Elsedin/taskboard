using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Data;
using TaskBoard.Api.Dtos;
using TaskBoard.Api.Helpers;
using TaskBoard.Api.Models;

namespace TaskBoard.Api.Endpoints;

public static class TaskEndpoints
{
    public static void MapTaskEndpoints(this WebApplication app)
    {
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
    }
}