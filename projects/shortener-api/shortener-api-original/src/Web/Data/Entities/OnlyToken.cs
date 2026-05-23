namespace Web.Data.Entities;

public class OnlyToken
{
    public OnlyToken()
    {
            
    }

    public OnlyToken(string token)
    {
        this.Token = token;
    }

    public string Token { get; set; } = null!;
}