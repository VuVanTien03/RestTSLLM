using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Supermarket.API.Resources
{
    public record ProductsQueryResource : QueryResource
    {

        [SwaggerSchema(Description = "A existing category ID")]
        public int? CategoryId { get; init; }
    }
}