using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Core.Models.Users
{
    public class AuthResponseDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
