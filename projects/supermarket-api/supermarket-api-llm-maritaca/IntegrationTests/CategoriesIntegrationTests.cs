// File: CategoriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Nodes;
using Supermarket.API;

namespace IntegrationTests
{
    public class CategoriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CategoriesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetCategoriesAsync()
        {
            return await _client.GetAsync("/api/categories");
        }

        private async Task<HttpResponseMessage> CreateCategoryAsync(string name)
        {
            var request = new { name = name };
            return await _client.PostAsJsonAsync("/api/categories", request);
        }

        private async Task<HttpResponseMessage> UpdateCategoryAsync(int id, string name)
        {
            var request = new { name = name };
            return await _client.PutAsJsonAsync($"/api/categories/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
        {
            return await _client.DeleteAsync($"/api/categories/{id}");
        }

        [Fact]
        public async Task TC001_List_All_Categories_Returns_OK()
        {
            // act
            var response = await GetCategoriesAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
        }

        [Fact]
        public async Task TC002_Save_Valid_Category_Returns_OK()
        {
            // arrange
            string validName = "New Category";

            // act
            var response = await CreateCategoryAsync(validName);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(validName, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC003_Save_Category_With_Empty_Name_Returns_BadRequest()
        {
            // arrange
            string emptyName = "";

            // act
            var response = await CreateCategoryAsync(emptyName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Save_Category_With_Null_Name_Returns_BadRequest()
        {
            // arrange
            string? nullName = null;

            // act
            var response = await CreateCategoryAsync(nullName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Update_Valid_Category_Returns_OK()
        {
            // arrange
            string newName = "Updated Category";
            var createResponse = await CreateCategoryAsync("New Category");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();

            // act
            var updateResponse = await UpdateCategoryAsync(categoryId, newName);

            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updatedBody = await updateResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(categoryId, updatedBody["id"].AsValue().GetValue<int>());
            Assert.Equal(newName, updatedBody["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC006_Update_Category_With_Empty_Name_Returns_BadRequest()
        {
            // arrange
            string emptyName = "";
            var createResponse = await CreateCategoryAsync("New Category");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();

            // act
            var updateResponse = await UpdateCategoryAsync(categoryId, emptyName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);
        }

        [Fact]
        public async Task TC007_Update_Category_That_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            string newName = "Nonexistent Category";
            int nonExistentId = 9999; // Assuming 9999 is a non-existent category ID

            // act
            var updateResponse = await UpdateCategoryAsync(nonExistentId, newName);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
        }

        [Fact]
        public async Task TC008_Delete_Category_Returns_OK()
        {
            // arrange
            var createResponse = await CreateCategoryAsync("New Category");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int categoryId = body["id"].AsValue().GetValue<int>();

            // act
            var deleteResponse = await DeleteCategoryAsync(categoryId);

            // assert
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
            var deleteBody = await deleteResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(categoryId, deleteBody["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC009_Delete_Category_That_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int nonExistentId = 9999; // Assuming 9999 is a non-existent category ID

            // act
            var deleteResponse = await DeleteCategoryAsync(nonExistentId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }
    }
}