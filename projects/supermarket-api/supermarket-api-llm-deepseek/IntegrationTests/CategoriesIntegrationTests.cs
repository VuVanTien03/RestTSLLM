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

        [Fact]
        public async Task TC001_Get_All_Categories_Success()
        {
            var response = await _client.GetAsync("/api/categories");
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
        }

        [Fact]
        public async Task TC002_Create_Category_Valid_Data()
        {
            var uniqueName = $"Category 1";
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = uniqueName });
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body["id"].GetValue<int>() > 0);
            Assert.Equal(uniqueName, body["name"].GetValue<string>());
        }

        [Fact]
        public async Task TC003_Create_Category_Name_Too_Short()
        {
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = "" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Category_Name_Too_Long()
        {
            var longName = new string('A', 31);
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = longName });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Category_Duplicate_Name()
        {
            var uniqueName = $"Category_{Guid.NewGuid()}";
            await _client.PostAsJsonAsync("/api/categories", new { name = uniqueName });
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = uniqueName });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Update_Category_Valid_Data()
        {
            var uniqueName = $"Category 1";
            var createResponse = await _client.PostAsJsonAsync("/api/categories", new { name = uniqueName });
            var createdId = (await createResponse.Content.ReadFromJsonAsync<JsonNode>())["id"].GetValue<int>();

            var newName = $"Updated 1";
            var updateResponse = await _client.PutAsJsonAsync($"/api/categories/{createdId}", new { name = newName });
            var updatedBody = await updateResponse.Content.ReadFromJsonAsync<JsonNode>();

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            Assert.Equal(createdId, updatedBody["id"].GetValue<int>());
            Assert.Equal(newName, updatedBody["name"].GetValue<string>());
        }

        [Fact]
        public async Task TC007_Update_Category_Invalid_ID()
        {
            var response = await _client.PutAsJsonAsync("/api/categories/invalid", new { name = "Test" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Update_Category_Nonexistent_ID()
        {
            var response = await _client.PutAsJsonAsync("/api/categories/9999", new { name = "Test" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Delete_Category_Success()
        {
            var uniqueName = $"Category 1";
            var createResponse = await _client.PostAsJsonAsync("/api/categories", new { name = uniqueName });
            var createdId = (await createResponse.Content.ReadFromJsonAsync<JsonNode>())["id"].GetValue<int>();

            var deleteResponse = await _client.DeleteAsync($"/api/categories/{createdId}");

            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task TC010_Delete_Category_Nonexistent_ID()
        {
            var response = await _client.DeleteAsync("/api/categories/9999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Category_Name_Min_Length()
        {
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = "A" });
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("A", body["name"].GetValue<string>());
        }

        [Fact]
        public async Task TC030_Create_Category_Name_Max_Length()
        {
            var validName = new string('A', 30);
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = validName });
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(validName, body["name"].GetValue<string>());
        }

        [Fact]
        public async Task TC035_Create_Category_Missing_Name()
        {
            var response = await _client.PostAsJsonAsync("/api/categories", new { });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Create_Category_Null_Name()
        {
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = (string)null });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}