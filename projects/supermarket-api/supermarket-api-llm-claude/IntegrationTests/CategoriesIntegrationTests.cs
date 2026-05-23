using Microsoft.AspNetCore.Mvc.Testing;
using Supermarket.API;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests;

public class CategoriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CategoriesIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<HttpResponseMessage> ListCategoriesAsync()
    {
        return await _client.GetAsync("/api/categories");
    }

    private async Task<HttpResponseMessage> CreateCategoryAsync(string name)
    {
        var request = new
        {
            name = name
        };

        return await _client.PostAsJsonAsync("/api/categories", request);
    }

    private async Task<int> CreateCategoryAndGetIdAsync(string name)
    {
        var response = await CreateCategoryAsync(name);
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        return body["id"].GetValue<int>();
    }

    private async Task<HttpResponseMessage> UpdateCategoryAsync(int id, string name)
    {
        var request = new
        {
            name = name
        };

        return await _client.PutAsJsonAsync($"/api/categories/{id}", request);
    }

    private async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
    {
        return await _client.DeleteAsync($"/api/categories/{id}");
    }

    [Fact]
    public async Task TC001_List_Categories_When_Empty_Returns_Empty_Array()
    {
        // act
        var response = await ListCategoriesAsync();

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonArray>();
        Assert.Empty(body);
    }

    [Fact]
    public async Task TC002_List_Categories_When_Has_Items_Returns_Array()
    {
        // arrange
        await CreateCategoryAsync("Category 1");
        await CreateCategoryAsync("Category 2");

        Thread.Sleep(3000);
        // act
        var response = await ListCategoriesAsync();

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonArray>();
        Assert.True(body.Count >= 2);

        var first = body[0].AsObject();
        Assert.True(first["id"].GetValue<int>() > 0);
        Assert.Equal("Category 1", first["name"].GetValue<string>());

        var second = body[1].AsObject();
        Assert.True(second["id"].GetValue<int>() > 0);
        Assert.Equal("Category 2", second["name"].GetValue<string>());
    }

    [Fact]
    public async Task TC003_Create_Category_When_Valid_Data_Returns_OK()
    {
        // arrange
        string name = "Category 1";

        // act
        var response = await CreateCategoryAsync(name);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.True(body["id"].GetValue<int>() > 0);
        Assert.Equal(name, body["name"].GetValue<string>());
    }

    [Fact]
    public async Task TC004_Create_Category_When_Name_Is_Empty_Returns_BadRequest()
    {
        // arrange
        string name = "";

        // act
        var response = await CreateCategoryAsync(name);

        // assert  
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC005_Create_Category_When_Name_Is_Null_Returns_BadRequest()
    {
        // arrange
        string name = null;

        // act
        var response = await CreateCategoryAsync(name);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC006_Create_Category_When_Name_Too_Long_Returns_BadRequest()
    {
        // arrange
        string name = "1234567890123456789012345678901"; // 31 chars

        // act
        var response = await CreateCategoryAsync(name);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC007_Create_Category_When_Name_Has_Maximum_Size_Returns_OK()
    {
        // arrange
        string name = "123456789012345678901234567890"; // 30 chars

        // act
        var response = await CreateCategoryAsync(name);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.True(body["id"].GetValue<int>() > 0);
        Assert.Equal(name, body["name"].GetValue<string>());
    }

    [Fact]
    public async Task TC008_Update_Category_When_Valid_Data_Returns_OK()
    {
        // arrange
        var id = await CreateCategoryAndGetIdAsync("Old Name");

        // act
        var response = await UpdateCategoryAsync(id, "New Name");

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(id, body["id"].GetValue<int>());
        Assert.Equal("New Name", body["name"].GetValue<string>());
    }

    [Fact]
    public async Task TC009_Update_Category_When_Invalid_ID_Returns_NotFound()
    {
        // arrange
        int invalidId = 999999;

        // act
        var response = await UpdateCategoryAsync(invalidId, "New Name");

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TC010_Update_Category_When_Name_Is_Empty_Returns_BadRequest()
    {
        // arrange
        var id = await CreateCategoryAndGetIdAsync("Old Name");

        // act
        var response = await UpdateCategoryAsync(id, "");

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC011_Update_Category_When_Name_Is_Null_Returns_BadRequest()
    {
        // arrange
        var id = await CreateCategoryAndGetIdAsync("Old Name");

        // act
        var response = await UpdateCategoryAsync(id, null);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC012_Update_Category_When_Name_Too_Long_Returns_BadRequest()
    {
        // arrange
        var id = await CreateCategoryAndGetIdAsync("Old Name");
        string tooLongName = "1234567890123456789012345678901"; // 31 chars

        // act
        var response = await UpdateCategoryAsync(id, tooLongName);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC013_Delete_Category_When_Valid_ID_Returns_OK()
    {
        // arrange
        var id = await CreateCategoryAndGetIdAsync("Category");

        // act
        var response = await DeleteCategoryAsync(id);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(id, body["id"].GetValue<int>());
        Assert.Equal("Category", body["name"].GetValue<string>());
    }

    [Fact]
    public async Task TC014_Delete_Category_When_Invalid_ID_Returns_NotFound()
    {
        // arrange
        int invalidId = 999999;

        // act
        var response = await DeleteCategoryAsync(invalidId);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}