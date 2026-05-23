using System.Security.Claims;

namespace TodoApi;

public class CurrentUser
{
    public TodoUser? User { get; set; }
    public ClaimsPrincipal Principal { get; set; } = default!;

    public string Id => Principal.FindFirstValue(ClaimTypes.NameIdentifier)!;
}