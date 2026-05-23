using FluentValidation;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Books.Api.Docker.Dtos;

public class CreateBookRequest
{
    [Required]
    [SwaggerParameter(Required = true)]
    public string Title { get; set; }

    [Required]
    [SwaggerParameter(Required = true)]
    public string ISBN { get; set; }

    [Required]
    [SwaggerParameter(Required = true)]
    public string Description { get; set; }

    [Required]
    [SwaggerParameter(Required = true)]
    public string Author { get; set; }
}

public sealed record BookResponse(
    int Id,
    string Title,
    string ISBN,
    string Description,
    string Author);

public class UpdateBookRequest
{
    [Required]
    [SwaggerParameter(Required = true)]
    public string Title { get; set; }

    [Required]
    [SwaggerParameter(Required = true)]
    public string ISBN { get; set; }

    [Required]
    [SwaggerParameter(Required = true)]
    public string Description { get; set; }

    [Required]
    [SwaggerParameter(Required = true)]
    public string Author { get; set; }
}

public class CreateBookRequestValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().NotNull();
        RuleFor(x => x.Description).NotEmpty().NotNull();
        RuleFor(x => x.ISBN).NotEmpty().NotNull();
        RuleFor(x => x.Author).NotEmpty().NotNull();
    }
}

public class UpdateBookRequestValidator : AbstractValidator<UpdateBookRequest>
{
    public UpdateBookRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().NotNull();
        RuleFor(x => x.Description).NotEmpty().NotNull();
        RuleFor(x => x.ISBN).NotEmpty().NotNull();
        RuleFor(x => x.Author).NotEmpty().NotNull();
    }
}
