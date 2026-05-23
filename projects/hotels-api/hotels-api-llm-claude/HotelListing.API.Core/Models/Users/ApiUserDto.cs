using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace HotelListing.API.Core.Models.Users
{
    public class ApiUserDto : LoginDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public bool? IsAdmin { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [SwaggerSchema(Description = "Password must have at least 1 number, 1 lower letter, 1 upper letter and 1 non alphanumeric character")]
        [StringLength(15, ErrorMessage = "Your Password is limited to {2} to {1} characters", MinimumLength = 6)]
        public string Password { get; set; }
    }
}
