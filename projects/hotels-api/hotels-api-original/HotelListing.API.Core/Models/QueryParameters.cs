using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Core.Models
{
    public class QueryParameters
    {
        [Min(1)]
        [Required]
        public int? PageNumber { get; set; }
        
        [Min(1)]
        [Required]
        public int? PageSize { get; set; }
    }
}
