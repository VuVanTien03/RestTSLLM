using MediatR;
using Swashbuckle.AspNetCore.Annotations;

namespace Restaurants.Application.Restaurants.Commands.DeleteRestaurant;

public class DeleteRestaurantCommand(int id) : IRequest
{
    [SwaggerIgnore]
    public int Id { get; } = id;
}
