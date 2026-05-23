using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TodoApi.Todos;

public class CreateTodoRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(256)]
    [DefaultValue("Something todo in the future")]
    [SwaggerSchema(Description = "Title is the description from todo and must be between 1 and 256 characters long.")]

    public string Title { get; set; } = default!;
}

public class UpdateTodoRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(256)]
    [DefaultValue("Something todo in the future")]
    [SwaggerSchema(Description = "Title is the description from todo and must be between 1 and 256 characters long.")]
    public string Title { get; set; } = default!;

    [Required]
    [DefaultValue(false)]
    [SwaggerSchema(Description = "IsComplete is a boolean flag indicating that a todo has been completed. It can be marked again as incomplete.")]
    public bool? IsComplete { get; set; } = default!;
}
