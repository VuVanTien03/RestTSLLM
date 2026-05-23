using System.Text.Json.Serialization;

namespace HotelListing.API.Core.Models.Country
{
    public class UpdateCountryDto : BaseCountryDto, IBaseDto
    {
        [JsonIgnore]
        public int Id { get; set; }
    }
}
