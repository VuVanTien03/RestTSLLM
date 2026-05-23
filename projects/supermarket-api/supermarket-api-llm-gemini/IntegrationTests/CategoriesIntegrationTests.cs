using Microsoft.AspNetCore.Mvc.Testing;
using Supermarket.API;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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
            var requestBody = new { name = name };
            return await _client.PostAsJsonAsync("/api/categories", requestBody);
        }

        private async Task<HttpResponseMessage> UpdateCategoryAsync(int id, string name)
        {
            var requestBody = new { name = name };
            return await _client.PutAsJsonAsync($"/api/categories/{id}", requestBody);
        }

        private async Task<HttpResponseMessage> DeleteCategoryAsync(int id)
        {
            return await _client.DeleteAsync($"/api/categories/{id}");
        }

        [Fact]
        public async Task TC001_List_Categories_Returns_OK()
        {
            // arrange

            // act
            var response = await GetCategoriesAsync();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Category_Valid_Data_Returns_Created()
        {
            // arrange
            string name = "Category A";

            // act
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            int id = body!["id"].GetValue<int>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(id > 0);
        }

        [Fact]
        public async Task TC003_Create_Category_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "";

            // act
            var response = await CreateCategoryAsync(name);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Category_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "ThisCategoryNameIsLongerThanTheMaximumAllowedLengthOfThirtyCharacters";

            // act
            var response = await CreateCategoryAsync(name);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Category_Name_Null_Returns_BadRequest()
        {
            // arrange
            string? name = null;

            // act
            var response = await CreateCategoryAsync(name);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task TC006_Update_Category_Valid_Data_Returns_OK()
        {
            // arrange
            string initialName = "CategoryToUpdate";
            var createResponse = await CreateCategoryAsync(initialName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string updatedName = "Updated Category";

            // act
            var updateResponse = await UpdateCategoryAsync(id, updatedName);
            var updateBody = await updateResponse.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            Assert.Equal(updatedName, updateBody!["name"].GetValue<string>());
        }

        [Fact]
        public async Task TC007_Update_Category_Invalid_ID_Returns_NotFound()
        {
            // arrange
            string name = "Category B";
            int invalidId = 99999;

            // act
            var response = await UpdateCategoryAsync(invalidId, name);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Update_Category_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            string initialName = "CategoryToUpdate";
            var createResponse = await CreateCategoryAsync(initialName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "";

            // act
            var response = await UpdateCategoryAsync(id, name);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Update_Category_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            string initialName = "CategoryToUpdate";
            var createResponse = await CreateCategoryAsync(initialName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "ThisCategoryNameIsLongerThanTheMaximumAllowedLengthOfThirtyCharacters";

            // act
            var response = await UpdateCategoryAsync(id, name);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Update_Category_Name_Null_Returns_BadRequest()
        {
            // arrange
            string initialName = "CategoryToUpdate";
            var createResponse = await CreateCategoryAsync(initialName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string? name = null;

            // act
            var response = await UpdateCategoryAsync(id, name);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Delete_Category_Valid_ID_Returns_OK()
        {
            // arrange
            string name = "CategoryToDelete";
            var createResponse = await CreateCategoryAsync(name);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();

            // act
            var response = await DeleteCategoryAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Delete_Category_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int invalidId = 99999;

            // act
            var response = await DeleteCategoryAsync(invalidId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
