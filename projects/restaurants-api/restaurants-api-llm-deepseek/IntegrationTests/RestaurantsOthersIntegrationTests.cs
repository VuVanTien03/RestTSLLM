// File: RestaurantsIntegrationTests.cs (Other Endpoints)

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class RestaurantsOtherEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _baseUrl = "/api/restaurants";

        public RestaurantsOtherEndpointsTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<int> CreateRestaurantAsync()
        {
            var uniqueId = Guid.NewGuid().ToString("N")[..10];
            var request = new
            {
                name = $"Restaurant {uniqueId}",
                description = $"Description {uniqueId}",
                category = "Test",
                hasDelivery = true,
                contactEmail = $"{uniqueId}@test.com",
                contactNumber = "1234567890",
                city = "Test City",
                street = "Test Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        // Search restaurants (GET /api/restaurants)
        [Fact]
        public async Task TC001_Get_Restaurants_With_Valid_Search_Returns_OK()
        {
            // Arrange
            await CreateRestaurantAsync();

            // Act
            var response = await _client.GetAsync($"{_baseUrl}?searchPhrase=Test&pageNumber=1&pageSize=10&sortBy=Name&sortDirection=Ascending");

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body["items"].AsArray().Count > 0);
        }

        [Theory]
        [InlineData(0, 10)]  // Invalid pageNumber
        [InlineData(1, 0)]   // Invalid pageSize
        public async Task TC002_Get_Restaurants_With_Invalid_Pagination_Returns_BadRequest(int pageNumber, int pageSize)
        {
            var response = await _client.GetAsync($"{_baseUrl}?pageNumber={pageNumber}&pageSize={pageSize}");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Get_Restaurants_With_Invalid_SortBy_Returns_BadRequest()
        {
            var response = await _client.GetAsync($"{_baseUrl}?sortBy=InvalidField");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Get by ID (GET /api/restaurants/{id})
        [Fact]
        public async Task TC004_Get_Restaurant_By_Valid_Id_Returns_OK()
        {
            // Arrange
            var restaurantId = await CreateRestaurantAsync();

            // Act
            var response = await _client.GetAsync($"{_baseUrl}/{restaurantId}");

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(restaurantId, body["id"].GetValue<int>());
        }

        [Fact]
        public async Task TC005_Get_Nonexistent_Restaurant_Returns_NotFound()
        {
            var response = await _client.GetAsync($"{_baseUrl}/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Update restaurant (PATCH /api/restaurants/{id})
        [Fact]
        public async Task TC006_Update_Restaurant_With_Valid_Data_Returns_OK()
        {
            // Arrange
            var restaurantId = await CreateRestaurantAsync();
            var updateData = new
            {
                name = "Updated Name",
                description = "New description",
                hasDelivery = false
            };

            // Act
            var response = await _client.PatchAsJsonAsync($"{_baseUrl}/{restaurantId}", updateData);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(updateData.name, body["name"].GetValue<string>());
        }

        [Fact]
        public async Task TC007_Update_Restaurant_With_Invalid_Data_Returns_BadRequest()
        {
            var restaurantId = await CreateRestaurantAsync();
            var invalidUpdate = new
            {
                name = "A", // Below min length
                description = "Valid description",
                hasDelivery = "NotABoolean" // Invalid type
            };

            var response = await _client.PatchAsJsonAsync($"{_baseUrl}/{restaurantId}", invalidUpdate);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Update_Nonexistent_Restaurant_Returns_NotFound()
        {
            // arrange
            var updateData = new
            {
                name = "Updated Name",
                description = "New description",
                hasDelivery = false
            };

            // act
            var response = await _client.PatchAsJsonAsync($"{_baseUrl}/99999", updateData);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Delete restaurant (DELETE /api/restaurants/{id})
        [Fact]
        public async Task TC009_Delete_Valid_Restaurant_Returns_NoContent()
        {
            // Arrange
            var restaurantId = await CreateRestaurantAsync();

            // Act
            var deleteResponse = await _client.DeleteAsync($"{_baseUrl}/{restaurantId}");
            var getResponse = await _client.GetAsync($"{_baseUrl}/{restaurantId}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task TC010_Delete_Nonexistent_Restaurant_Returns_NotFound()
        {
            var response = await _client.DeleteAsync($"{_baseUrl}/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Upload logo (POST /api/restaurants/{id}/logo)
        [Fact]
        public async Task TC011_Upload_Logo_With_Valid_Image_Returns_OK()
        {
            // Arrange
            var restaurantId = await CreateRestaurantAsync();
            var content = new MultipartFormDataContent
            {
                {
                    new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF }),
                    "file",
                    "logo.jpg"
                }
            };

            // Act
            var response = await _client.PostAsync($"{_baseUrl}/{restaurantId}/logo", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Upload_Logo_With_Invalid_FileType_Returns_UnsupportedMediaType()
        {
            var restaurantId = await CreateRestaurantAsync();
            var content = new MultipartFormDataContent
            {
                { new StringContent("Not an image"), "file", "invalid.txt" }
            };

            var response = await _client.PostAsync($"{_baseUrl}/{restaurantId}/logo", content);
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Upload_Logo_To_Nonexistent_Restaurant_Returns_NotFound()
        {
            var content = new MultipartFormDataContent
            {
                {
                    new ByteArrayContent(new byte[] { 0xFF, 0xD8, 0xFF }),
                    "file",
                    "logo.jpg"
                }
            };
            var response = await _client.PostAsync($"{_baseUrl}/99999/logo", content);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Complex update scenarios
        [Theory]
        [InlineData(3, true)]    // Min name length
        [InlineData(32, true)]   // Max name length
        [InlineData(2, false)]    // Below min
        [InlineData(33, false)]   // Above max
        public async Task TC014_Update_Name_Length_Validation(int length, bool isValid)
        {
            var restaurantId = await CreateRestaurantAsync();
            var updateData = new
            {
                name = new string('N', length),
                description = "Valid description",
                hasDelivery = true
            };

            var response = await _client.PatchAsJsonAsync($"{_baseUrl}/{restaurantId}", updateData);
            Assert.Equal(isValid ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Cache validation after updates
        [Fact]
        public async Task TC016_Update_Reflected_In_Get()
        {
            var restaurantId = await CreateRestaurantAsync();
            var original = await _client.GetAsync($"{_baseUrl}/{restaurantId}");

            var update = new { name = "Updated Name" };
            await _client.PatchAsJsonAsync($"{_baseUrl}/{restaurantId}", update);

            var updated = await _client.GetAsync($"{_baseUrl}/{restaurantId}");

            var originalBody = await original.Content.ReadFromJsonAsync<JsonObject>();
            var updatedBody = await updated.Content.ReadFromJsonAsync<JsonObject>();

            Assert.NotEqual(originalBody["name"], updatedBody["name"]);
        }

        // Bulk operations
        [Fact]
        public async Task TC017_Multiple_Operations_Sequence()
        {
            // Create
            var restaurantId = await CreateRestaurantAsync();

            // Update
            var update = new { description = "Updated description" };
            await _client.PatchAsJsonAsync($"{_baseUrl}/{restaurantId}", update);

            // Get
            var getResponse = await _client.GetAsync($"{_baseUrl}/{restaurantId}");

            // Delete
            await _client.DeleteAsync($"{_baseUrl}/{restaurantId}");

            // Verify
            var finalGet = await _client.GetAsync($"{_baseUrl}/{restaurantId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, finalGet.StatusCode);
        }

        // Content negotiation tests
        [Fact]
        public async Task TC018_Get_Paged_Results_With_Correct_Structure()
        {
            await CreateRestaurantAsync();
            var response = await _client.GetAsync($"{_baseUrl}?pageNumber=1&pageSize=10");

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["totalItemsCount"]);
            Assert.NotNull(body["items"]);
            Assert.True(body["totalItemsCount"].GetValue<int>() > 0);
        }
    }
}