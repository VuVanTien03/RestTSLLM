using System.ComponentModel.DataAnnotations;
using TodoApi.Todos;

public static class TodoMappingExtensions
{
    public static TodoResponse AsTodoItemResponse(this Todo todo)
    {
        return new TodoResponse()
        {
            Id = todo.Id,
            Title = todo.Title,
            IsComplete = todo.IsComplete,
        };
    }

    public static TodoResponse AsTodoItemResponse(this UpdateTodoRequest todo, int id)
    {
        return new TodoResponse()
        {
            Id = id,
            Title = todo.Title,
            IsComplete = todo.IsComplete.Value,
        };
    }

    public static TodoResponse AsTodoItemResponse(this CreateTodoRequest todo, int id)
    {
        return new TodoResponse()
        {
            Id = id,
            Title = todo.Title,
            IsComplete = false
        };
    }
}