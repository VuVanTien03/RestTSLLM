using HotelListing.API.Core.Models.Hotel;

namespace HotelListing.API.Core.Models.Country
{
    public class CountryResumedDto : BaseCountryDto, IBaseDto
    {
        public int Id { get; set; }
    }
}
