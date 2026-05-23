using DataAnnotationsExtensions;
using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Restaurants.Application.Dishes.Commands.CreateDish;

public class CreateDishCommand : IRequest<int>
{
    [Length(3, 16)]
    [Required]
    public string Name { get; set; } = default!;

    [Length(3, 32)]
    [Required]
    public string Description { get; set; } = default!;

    [Min(0.01)]
    [Required]
    public decimal? Price { get; set; }

    [Min(0)]
    [Required]
    public int? KiloCalories { get; set; }
    
    [SwaggerIgnore]
    public int RestaurantId { get; set; }
}
