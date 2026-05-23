using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;

namespace TodoApi;

public class TokenResponse
{
    public TokenResponse() { }

    public TokenResponse(string token) 
    { 
        Token = token;
    }

    [DefaultValue("eyJhbGc...")]
    [SwaggerSchema(Description = "Valid token to authenticate todos endpoints with header `Authorization: Bearer eyJhbGc...`")]
    public string Token { get; set; }
}
