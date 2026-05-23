namespace HotelListing.API.Core.Models.Hotel
{
    public class UpdateHotelDto : BaseHotelDto
    {
    }

    public class HotelDto : UpdateHotelDto, IBaseDto
    {
        public int Id { get; set; }
    }
}
