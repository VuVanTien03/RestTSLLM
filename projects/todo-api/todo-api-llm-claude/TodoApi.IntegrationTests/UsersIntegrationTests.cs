using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json.Nodes;

namespace TodoApi.IntegrationTests;

public class UsersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public UsersIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<HttpResponseMessage> CreateUserAsync(string? username, string? password)
    {
        var request = new
        {
            username = username,
            password = password
        };

        return await _client.PostAsJsonAsync("/users", request);
    }

    private async Task<HttpResponseMessage> CreateTokenAsync(string? username, string? password)
    {
        var request = new
        {
            username = username,
            password = password
        };

        return await _client.PostAsJsonAsync("/users/token", request);
    }

    [Fact]
    public async Task TC001_Create_User_When_Valid_Data_Returns_OK()
    {
        // act
        var response = await CreateUserAsync("valid_username1", "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task TC002_Create_User_When_Username_Already_Exists_Returns_BadRequest()
    {
        // arrange
        await CreateUserAsync("existing_user2", "V@lid123");

        // act
        var response = await CreateUserAsync("existing_user2", "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC003_Create_User_When_Username_Is_Null_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync(null, "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC004_Create_User_When_Username_Is_Empty_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("", "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC005_Create_User_When_Username_Too_Long_Returns_BadRequest()
    {
        // arrange
        var username = new string('a', 129);

        // act
        var response = await CreateUserAsync(username, "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC006_Create_User_When_Username_Has_Invalid_Characters_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("invalid*username6", "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC007_Create_User_When_Password_Is_Null_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("valid_username7", null);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC008_Create_User_When_Password_Is_Empty_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("valid_username8", "");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC009_Create_User_When_Password_Too_Short_Returns_BadRequest()
    {
        // arrange
        var password = "V@l1d"; // 5 characters

        // act
        var response = await CreateUserAsync("valid_username9", password);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC010_Create_User_When_Password_Too_Long_Returns_BadRequest()
    {
        // arrange
        var password = "V@l1d" + new string('a', 28); // 33 characters

        // act
        var response = await CreateUserAsync("valid_username10", password);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC011_Create_User_When_Password_Missing_Uppercase_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("valid_username11", "v@lid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC012_Create_User_When_Password_Missing_Lowercase_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("valid_username12", "V@LID123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC013_Create_User_When_Password_Missing_Number_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("valid_username13", "V@lidpass");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC014_Create_User_When_Password_Missing_Special_Character_Returns_BadRequest()
    {
        // act
        var response = await CreateUserAsync("valid_username14", "Valid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC015_Generate_Token_When_Valid_Credentials_Returns_OK()
    {
        // arrange
        await CreateUserAsync("valid_user15", "V@lid123");

        // act
        var response = await CreateTokenAsync("valid_user15", "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(content);
        var token = content["token"]?.ToString();
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public async Task TC016_Generate_Token_When_Invalid_Username_Returns_BadRequest()
    {
        // act
        var response = await CreateTokenAsync("invalid_user16", "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC017_Generate_Token_When_Invalid_Password_Returns_BadRequest()
    {
        // arrange
        await CreateUserAsync("valid_user17", "V@lid123");

        // act
        var response = await CreateTokenAsync("valid_user17", "Wr@ng123");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Extra001_Create_User_When_Username_Has_Minimum_Size_Returns_OK()
    {
        // arrange
        var username = "a"; // 1 character

        // act
        var response = await CreateUserAsync(username, "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Extra002_Create_User_When_Username_Has_Maximum_Size_Returns_OK()
    {
        // arrange
        var username = new string('a', 128);

        // act
        var response = await CreateUserAsync(username, "V@lid123");

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Extra003_Create_User_When_Password_Has_Minimum_Size_Returns_OK()
    {
        // arrange
        var password = "V@l1d2"; // 6 characters

        // act
        var response = await CreateUserAsync("valid_username_extra3", password);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Extra004_Create_User_When_Password_Has_Maximum_Size_Returns_OK()
    {
        // arrange
        var password = "V@l1d" + new string('a', 27); // 32 characters

        // act
        var response = await CreateUserAsync("valid_username_extra4", password);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
