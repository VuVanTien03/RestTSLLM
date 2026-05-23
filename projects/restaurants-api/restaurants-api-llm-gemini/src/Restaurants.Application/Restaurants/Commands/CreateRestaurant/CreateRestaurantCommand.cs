using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Restaurants.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommand : IRequest<int>
{
    [Required]
    [Length(3,32)]
    public string Name { get; set; } = default!;

    [Required]
    [Length(3, 32)]
    public string Description { get; set; } = default!;
    
    [Required]
    [Length(3, 16)]
    public string Category { get; set; } = default!;
    
    [Required]
    public bool? HasDelivery { get; set; }

    [Required]
    [MinLength(5)]
    [SwaggerSchema(Description = "<i>Accept Only Valid Email</i>")]
    public string? ContactEmail { get; set; }

    [Required]
    [Length(8, 13)]
    [SwaggerSchema(Description = "<i>Accept Only Numbers</i>")]
    public string? ContactNumber { get; set; }

    [Required]
    [Length(3, 16)]
    public string? City { get; set; }

    [Required]
    [Length(3, 32)]
    public string? Street { get; set; }

    [Required]
    [Length(3, 10)]
    [SwaggerSchema(Description = "<i>Accept Only Numbers</i>")]
    public string? PostalCode { get; set; }
}
