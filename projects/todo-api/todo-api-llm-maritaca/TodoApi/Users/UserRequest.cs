using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Swashbuckle.AspNetCore.Annotations;

namespace TodoApi;

public class TodoUser : IdentityUser { }

public class CreateUserRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(128)]
    [DefaultValue("my_username")]
    [SwaggerSchema(Description = "Username must be between 1 and 128 characters long, can only contain uppercase and lowercase letters (a-z, A-Z), numbers (0-9), and the following special characters: hyphen (-), period (.), underscore (_), at symbol (@), and plus sign (+) and must not exist in the database.")]
    public string Username { get; set; } = default!;

    [Required]
    [MinLength(6)]
    [MaxLength(32)]
    [DefaultValue("P@ssw0rd")]
    [SwaggerSchema(Description = "Password must be between 6 and 32 characters long, contain at least one uppercase letter, one lowercase letter, one digit, and one non-alphanumeric character.")]
    public string Password { get; set; } = default!;
}