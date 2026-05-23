using DataAnnotationsExtensions;
using MediatR;
using Restaurants.Application.Common;
using Restaurants.Application.Restaurants.Dtos;
using Restaurants.Domain.Constants;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Restaurants.Application.Restaurants.Queries.GetAllRestaurants;

public class GetAllRestaurantsQuery : IRequest<PagedResult<RestaurantDto>>
{
    public string? SearchPhrase { get; set; }
    
    [Min(1)]
    [SwaggerSchema(Description = "<i>Available values</i>: Positive integer greater than or equal 1")]
    public int PageNumber { get; set; } = 1;

    [Min(1)]
    [SwaggerSchema(Description = "<i>Available values</i>: Positive integer greater than or equal 1")]
    public int PageSize { get; set; } = 10;
    
    [SwaggerSchema(Description = "<i>Available values</i>: Name, Category, Description")]
    public string? SortBy { get; set; } = "Name";
    
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}

