using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace TodoApi.IntegrationTests;

public class TodosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TodosIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<string> CreateUserAndGetTokenAsync(string username, string password)
    {
        var request = new
        {
            username = username,
            password = password
        };

        await _client.PostAsJsonAsync("/users", request);
        var response = await _client.PostAsJsonAsync("/users/token", request);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();

        return content!["token"]!.GetValue<string>();
    }

    private async Task<HttpResponseMessage> CreateTodoAsync(string? token, string? title)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
        {
            Content = JsonContent.Create(new { title = title })
        };

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _client.SendAsync(request);
    }

    private async Task<(int id, string title)> CreateTodoAndGetIdAsync(string token, string title)
    {
        var response = await CreateTodoAsync(token, title);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        return (content!["id"]!.GetValue<int>(), content["title"]!.GetValue<string>());
    }

    private async Task<HttpResponseMessage> UpdateTodoAsync(string? token, int id, string? title, bool? isComplete)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"/todos/{id}")
        {
            Content = JsonContent.Create(new { title = title, isComplete = isComplete })
        };

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _client.SendAsync(request);
    }

    [Fact]
    public async Task TC018_Create_Todo_When_Valid_Data_Returns_Created()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc018", "V@lid123");

        // act
        var response = await CreateTodoAsync(token, "Valid todo title");

        // assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(content);
        Assert.True(content["id"]!.GetValue<int>() > 0);
        Assert.Equal("Valid todo title", content["title"]!.GetValue<string>());
        Assert.False(content["isComplete"]!.GetValue<bool>());
    }

    [Fact]
    public async Task TC019_Create_Todo_When_Title_Is_Null_Returns_BadRequest()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc019", "V@lid123");

        // act
        var response = await CreateTodoAsync(token, null);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC020_Create_Todo_When_Title_Is_Empty_Returns_BadRequest()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc020", "V@lid123");

        // act
        var response = await CreateTodoAsync(token, "");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC021_Create_Todo_When_Title_Too_Long_Returns_BadRequest()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc021", "V@lid123");
        var title = new string('a', 257); // Max is 256

        // act
        var response = await CreateTodoAsync(token, title);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC022_Create_Todo_When_Invalid_Token_Returns_Unauthorized()
    {
        // act
        var response = await CreateTodoAsync("invalid_token", "Valid todo title");

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Extra005_Create_Todo_When_Title_Has_Minimum_Size_Returns_Created()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_extra005", "V@lid123");
        var title = "a"; // Min is 1

        // act
        var response = await CreateTodoAsync(token, title);

        // assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(content);
        Assert.Equal(title, content["title"]!.GetValue<string>());
    }

    [Fact]
    public async Task Extra006_Create_Todo_When_Title_Has_Maximum_Size_Returns_Created()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_extra006", "V@lid123");
        var title = new string('a', 256);

        // act
        var response = await CreateTodoAsync(token, title);

        // assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(content);
        Assert.Equal(title, content["title"]!.GetValue<string>());
    }

    [Fact]
    public async Task TC029_Update_Todo_When_Valid_Data_Returns_OK()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc029", "V@lid123");
        var (id, _) = await CreateTodoAndGetIdAsync(token, "Original title");

        // act
        var response = await UpdateTodoAsync(token, id, "Updated todo title", true);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(content);
        Assert.Equal(id, content["id"]!.GetValue<int>());
        Assert.Equal("Updated todo title", content["title"]!.GetValue<string>());
        Assert.True(content["isComplete"]!.GetValue<bool>());
    }

    [Fact]
    public async Task TC030_Update_Todo_When_Title_Is_Null_Returns_BadRequest()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc030", "V@lid123");
        var (id, _) = await CreateTodoAndGetIdAsync(token, "Original title");

        // act
        var response = await UpdateTodoAsync(token, id, null, true);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC031_Update_Todo_When_IsComplete_Is_Null_Returns_BadRequest()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc031", "V@lid123");
        var (id, _) = await CreateTodoAndGetIdAsync(token, "Original title");

        // act
        var response = await UpdateTodoAsync(token, id, "Valid todo title", null);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC032_Update_Todo_When_Title_Too_Long_Returns_BadRequest()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc032", "V@lid123");
        var (id, _) = await CreateTodoAndGetIdAsync(token, "Original title");
        var title = new string('a', 257);

        // act
        var response = await UpdateTodoAsync(token, id, title, true);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC033_Update_Todo_When_Invalid_ID_Returns_NotFound()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc033", "V@lid123");

        // act
        var response = await UpdateTodoAsync(token, 999, "Valid todo title", true);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TC034_Update_Todo_When_Other_User_Todo_Returns_NotFound()
    {
        // arrange
        var tokenUser1 = await CreateUserAndGetTokenAsync("user_tc034_1", "V@lid123");
        var tokenUser2 = await CreateUserAndGetTokenAsync("user_tc034_2", "V@lid123");
        var (id, _) = await CreateTodoAndGetIdAsync(tokenUser1, "Original title");

        // act
        var response = await UpdateTodoAsync(tokenUser2, id, "Valid todo title", true);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Extra007_Update_Todo_When_Title_Has_Minimum_Size_Returns_OK()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_extra007", "V@lid123");
        var (id, _) = await CreateTodoAndGetIdAsync(token, "Original title");
        var title = "a"; // Min is 1

        // act
        var response = await UpdateTodoAsync(token, id, title, true);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(content);
        Assert.Equal(title, content["title"]!.GetValue<string>());
    }

    [Fact]
    public async Task Extra008_Update_Todo_When_Title_Has_Maximum_Size_Returns_OK()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_extra008", "V@lid123");
        var (id, _) = await CreateTodoAndGetIdAsync(token, "Original title");
        var title = new string('a', 256);

        // act
        var response = await UpdateTodoAsync(token, id, title, true);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(content);
        Assert.Equal(title, content["title"]!.GetValue<string>());
    }

    private async Task<HttpResponseMessage> GetTodoAsync(string? token, int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/todos/{id}");

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> ListTodosAsync(string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/todos");

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _client.SendAsync(request);
    }

    private async Task<HttpResponseMessage> DeleteTodoAsync(string? token, int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{id}");

        if (token != null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await _client.SendAsync(request);
    }

    [Fact]
    public async Task TC023_List_Todos_When_User_Has_Todos_Returns_OK()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc023", "V@lid123");
        await CreateTodoAsync(token, "Todo 1 for user tc023");
        await CreateTodoAsync(token, "Todo 2 for user tc023");

        // act
        var response = await ListTodosAsync(token);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(content);
        Assert.Equal(2, content.Count);
    }

    [Fact]
    public async Task TC024_List_Todos_When_User_Has_No_Todos_Returns_Empty_Array()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc024", "V@lid123");

        // act
        var response = await ListTodosAsync(token);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<JsonArray>();
        Assert.NotNull(content);
        Assert.Empty(content);
    }

    [Fact]
    public async Task TC025_List_Todos_When_Invalid_Token_Returns_Unauthorized()
    {
        // act
        var response = await ListTodosAsync("invalid_token");

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TC026_Get_Todo_When_Valid_ID_Returns_OK()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc026", "V@lid123");
        var response = await CreateTodoAsync(token, "Todo for user tc026");
        var content = await response.Content.ReadFromJsonAsync<JsonObject>();
        var todoId = content!["id"]!.GetValue<int>();

        // act
        var getTodoResponse = await GetTodoAsync(token, todoId);

        // assert
        Assert.Equal(HttpStatusCode.OK, getTodoResponse.StatusCode);
        var getTodoContent = await getTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
        Assert.NotNull(getTodoContent);
        Assert.Equal(todoId, getTodoContent["id"]!.GetValue<int>());
        Assert.Equal("Todo for user tc026", getTodoContent["title"]!.GetValue<string>());
        Assert.False(getTodoContent["isComplete"]!.GetValue<bool>());
    }

    [Fact]
    public async Task TC027_Get_Todo_When_Invalid_ID_Returns_NotFound()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc027", "V@lid123");

        // act
        var response = await GetTodoAsync(token, 99999);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TC028_Get_Todo_When_Other_User_Todo_Returns_NotFound()
    {
        // arrange
        var token1 = await CreateUserAndGetTokenAsync("user_tc028_1", "V@lid123");
        var createResponse = await CreateTodoAsync(token1, "Todo for user tc028_1");
        var content = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
        var todoId = content!["id"]!.GetValue<int>();

        var token2 = await CreateUserAndGetTokenAsync("user_tc028_2", "V@lid123");

        // act
        var getTodoResponse = await GetTodoAsync(token2, todoId);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, getTodoResponse.StatusCode);
    }

    [Fact]
    public async Task TC035_Delete_Todo_When_Valid_ID_Returns_OK()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc035", "V@lid123");
        var createResponse = await CreateTodoAsync(token, "Todo for user tc035");
        var content = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
        var todoId = content!["id"]!.GetValue<int>();

        // act
        var deleteResponse = await DeleteTodoAsync(token, todoId);

        // assert
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        // verify todo was deleted
        var getTodoResponse = await GetTodoAsync(token, todoId);
        Assert.Equal(HttpStatusCode.NotFound, getTodoResponse.StatusCode);
    }

    [Fact]
    public async Task TC036_Delete_Todo_When_Invalid_ID_Returns_NotFound()
    {
        // arrange
        var token = await CreateUserAndGetTokenAsync("user_tc036", "V@lid123");

        // act
        var response = await DeleteTodoAsync(token, 99999);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TC037_Delete_Todo_When_Other_User_Todo_Returns_NotFound()
    {
        // arrange
        var token1 = await CreateUserAndGetTokenAsync("user_tc037_1", "V@lid123");
        var createResponse = await CreateTodoAsync(token1, "Todo for user tc037_1");
        var content = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
        var todoId = content!["id"]!.GetValue<int>();

        var token2 = await CreateUserAndGetTokenAsync("user_tc037_2", "V@lid123");

        // act
        var deleteResponse = await DeleteTodoAsync(token2, todoId);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task Extra009_List_Todos_When_Without_Token_Returns_Unauthorized()
    {
        // act
        var response = await ListTodosAsync(null);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Extra010_Get_Todo_When_Without_Token_Returns_Unauthorized()
    {
        // act
        var response = await GetTodoAsync(null, 1);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Extra011_Delete_Todo_When_Without_Token_Returns_Unauthorized()
    {
        // act
        var response = await DeleteTodoAsync(null, 1);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Extra012_List_Todos_Multiple_Users_Only_See_Their_Todos()
    {
        // arrange
        var token1 = await CreateUserAndGetTokenAsync("user_extra012_1", "V@lid123");
        await CreateTodoAsync(token1, "Todo 1 for user extra012_1");
        await CreateTodoAsync(token1, "Todo 2 for user extra012_1");

        var token2 = await CreateUserAndGetTokenAsync("user_extra012_2", "V@lid123");
        await CreateTodoAsync(token2, "Todo 1 for user extra012_2");

        // act
        var response1 = await ListTodosAsync(token1);
        var response2 = await ListTodosAsync(token2);

        // assert
        var content1 = await response1.Content.ReadFromJsonAsync<JsonArray>();
        var content2 = await response2.Content.ReadFromJsonAsync<JsonArray>();

        Assert.Equal(2, content1!.Count);
        Assert.Equal(1, content2!.Count);

        Assert.Contains(content1.AsArray(), j => j!["title"]!.GetValue<string>() == "Todo 1 for user extra012_1");
        Assert.Contains(content1.AsArray(), j => j!["title"]!.GetValue<string>() == "Todo 2 for user extra012_1");
        Assert.Contains(content2.AsArray(), j => j!["title"]!.GetValue<string>() == "Todo 1 for user extra012_2");
    }
}
