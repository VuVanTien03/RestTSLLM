using DataAnnotationsExtensions;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Supermarket.API.Resources
{
    public record QueryResource
    {
        [Min(1)]
        [Required]
        [SwaggerSchema(Description = "Optional. Min value 1")]
        public int? Page { get; init; }

        [Min(1)]
        [Required]
        [SwaggerSchema(Description = "Optional. Min value 1")]
        public int? ItemsPerPage { get; init; }
    }
}