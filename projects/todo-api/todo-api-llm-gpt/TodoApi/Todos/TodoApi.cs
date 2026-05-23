using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TodoApi.Extensions;
using TodoApi.GenericResponse;
using TodoApi.Todos;

namespace TodoApi;

internal static class TodoApi
{
    public static RouteGroupBuilder MapTodos(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/todos");

        group.WithTags("Todos");

        group.RequireAuthorization(pb => pb.RequireCurrentUser())
             .DescribeApiSecurityRequirement();

        group.RequirePerUserRateLimit();

        group.WithParameterValidation(typeof(CreateTodoRequest), typeof(UpdateTodoRequest));

        group.MapGet("/", async (TodoDbContext db, CurrentUser owner) =>
        {
            return await db.Todos.Where(todo => todo.OwnerId == owner.Id)
            .Select(t => t.AsTodoItemResponse())
            .AsNoTracking()
            .ToListAsync();
        })
        .DescribeListTodos();

        group.MapGet("/{id}", async Task<Results<Ok<TodoResponse>, NotFound>> (TodoDbContext db, int id, CurrentUser owner) =>
        {
            return await db.Todos.FindAsync(id) switch
            {
                Todo todo when todo != null && todo.OwnerId == owner.Id
                              => TypedResults.Ok(todo.AsTodoItemResponse()),
                            _ => TypedResults.NotFound()
            };
        })
        .DescribeGetTodo();

        group.MapPost("/", async Task<Results<Created<TodoResponse>, BadRequest<BadRequestResponse>>> (TodoDbContext db, CreateTodoRequest newTodo, CurrentUser owner) =>
        {
            var todo = new Todo
            {
                Title = newTodo.Title,
                OwnerId = owner.Id
            };

            db.Todos.Add(todo);
            await db.SaveChangesAsync();
            
            return TypedResults.Created($"/todos/{todo.Id}", newTodo.AsTodoItemResponse(todo.Id));
        })
        .DescribeCreateTodo();

        group.MapPut("/{id}", async Task<Results<Ok<TodoResponse>, NotFound, BadRequest<BadRequestResponse>>> (TodoDbContext db, int id, UpdateTodoRequest todo, CurrentUser owner) =>
        {
            var currentTodo = await db.Todos.FindAsync(id);
            if (currentTodo != null && currentTodo.OwnerId == owner.Id)
            {
                var rowsAffected = await db.Todos.Where(t => t.Id == id && t.OwnerId == owner.Id)
                                             .ExecuteUpdateAsync(updates =>
                                                updates.SetProperty(t => t.IsComplete, todo.IsComplete)
                                                       .SetProperty(t => t.Title, todo.Title));

                return TypedResults.Ok(todo.AsTodoItemResponse(id));
            }
            else
            {
                return TypedResults.NotFound();
            }
        })
        .DescribeUpdateTodo();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> (TodoDbContext db, int id, CurrentUser owner) =>
        {
            var rowsAffected = await db.Todos.Where(t => t.Id == id && t.OwnerId == owner.Id)
                                             .ExecuteDeleteAsync();

            return rowsAffected == 0 ? TypedResults.NotFound() : TypedResults.Ok();
        })
        .DescribeDeleteTodo(); ;

        return group;
    }
}
