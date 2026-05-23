using MediatR;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Restaurants.Application.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommand : IRequest
{
    [SwaggerIgnore]
    public int Id { get; set; }

    [Required]
    [Length(3, 32)]
    public string Name { get; set; } = default!;

    [Required]
    [Length(3, 32)]
    public string Description { get; set; } = default!;

    [Required]
    public bool? HasDelivery { get; set; }
}
