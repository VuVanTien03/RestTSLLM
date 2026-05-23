using MediatR;
using Microsoft.AspNetCore.Mvc;
using Restaurants.API.Swagger;
using Restaurants.Application.Common;
using Restaurants.Application.Dishes.Queries.GetDishesForRestaurant;
using Restaurants.Application.Restaurants.Commands.CreateRestaurant;
using Restaurants.Application.Restaurants.Commands.DeleteRestaurant;
using Restaurants.Application.Restaurants.Commands.UpdateRestaurant;
using Restaurants.Application.Restaurants.Commands.UploadRestaurantLogo;
using Restaurants.Application.Restaurants.Dtos;
using Restaurants.Application.Restaurants.Queries.GetAllRestaurants;
using Restaurants.Application.Restaurants.Queries.GetRestaurantById;
using Restaurants.Domain.Entities;
using Swashbuckle.AspNetCore.Annotations;

namespace Restaurants.API.Controllers;

[ApiController]
[Route("api/restaurants")]
[Consumes("application/json")]
public class RestaurantsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(BadRequestSample), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PagedResult<RestaurantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetAll([FromQuery] GetAllRestaurantsQuery query)
    {
        var restaurants = await mediator.Send(query);

        foreach (var r in restaurants.Items)
        {
            var dishes = await mediator.Send(new GetDishesForRestaurantQuery(r.Id));
            r.Dishes = dishes.ToList();
        }

        return Ok(restaurants);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RestaurantDto?>> GetById([FromRoute]int id)
    {
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));
        return Ok(restaurant);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRestaurant([FromRoute] int id, UpdateRestaurantCommand command)
    {
        command.Id = id;
        await mediator.Send(command);
        
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));

        return Ok(restaurant);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRestaurant([FromRoute] int id)
    {
        await mediator.Send(new DeleteRestaurantCommand(id));

        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(typeof(BadRequestSample), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRestaurant(CreateRestaurantCommand command)
    {
        int id = await mediator.Send(command);
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));
        return CreatedAtAction(nameof(GetById), new { id }, restaurant);
    }

    [HttpPost("{id}/logo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(void), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(RestaurantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadLogo([FromRoute]int id, [SwaggerSchema(Description = "Form Data with 'file' property with a image file")] IFormFile file)
    {
        var restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));

        if (restaurant == null)
        {
            return NotFound();
        }

        if(file.FileName.EndsWith(".jpg") == false &&
           file.FileName.EndsWith(".jpeg") == false &&
           file.FileName.EndsWith(".gif") == false &&
           file.FileName.EndsWith(".bmp") == false &&
           file.FileName.EndsWith(".png") == false &&
           file.FileName.EndsWith(".tiff") == false)
        {
            return new ObjectResult(null) { StatusCode = 415 };
        }

        using var stream = file.OpenReadStream();

        var command = new UploadRestaurantLogoCommand()
        {
            RestaurantId = id,
            FileName = $"{id}={Guid.NewGuid().ToString()}-{file.FileName}",
            File = stream
        };

        await mediator.Send(command);

        restaurant = await mediator.Send(new GetRestaurantByIdQuery(id));
        return Ok(restaurant);
    }
}
