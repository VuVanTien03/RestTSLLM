// File: RestaurantsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class RestaurantsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<int> CreateRestaurantAndGetIdAsync(string name, string description, string category)
        {
            var request = new
            {
                name = name,
                description = description,
                category = category,
                hasDelivery = true,
                contactEmail = "restaurant@example.com",
                contactNumber = "12345678",
                city = "Test City",
                street = "123 Test Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync("/api/restaurants", request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        private async Task<HttpResponseMessage> UpdateRestaurantAsync(int restaurantId, string name, string description, bool hasDelivery)
        {
            var request = new
            {
                name = name,
                description = description,
                hasDelivery = hasDelivery
            };

            return await _client.PatchAsJsonAsync($"/api/restaurants/{restaurantId}", request);
        }

        private async Task<HttpResponseMessage> GetRestaurantAsync(int restaurantId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}");
        }

        private async Task<HttpResponseMessage> DeleteRestaurantAsync(int restaurantId)
        {
            return await _client.DeleteAsync($"/api/restaurants/{restaurantId}");
        }

        private async Task<HttpResponseMessage> UploadLogoAsync(int restaurantId, HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/restaurants/{restaurantId}/logo")
            {
                Content = content
            };

            return await _client.SendAsync(request);
        }

        [Fact]
        public async Task TC015_Get_Restaurant_By_ID_When_Restaurant_Exists_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant A", "Test Description", "Test Category");

            // act
            var response = await GetRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(restaurantId, body["id"].GetValue<int>());
        }

        [Fact]
        public async Task TC016_Get_Restaurant_By_ID_When_Restaurant_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = 99999; // Non-existent restaurant

            // act
            var response = await GetRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Update_Restaurant_When_Valid_Data_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant B", "Initial Description", "Initial Category");
            string updatedName = "Updated Restaurant B";
            string updatedDescription = "Updated Description";
            bool updatedHasDelivery = false;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, updatedName, updatedDescription, updatedHasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(updatedName, body["name"].GetValue<string>());
            Assert.Equal(updatedDescription, body["description"].GetValue<string>());
            Assert.Equal(updatedHasDelivery, body["hasDelivery"].GetValue<bool>());
        }

        [Fact]
        public async Task TC018_Update_Restaurant_When_Restaurant_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = 99999; // Non-existent restaurant
            string updatedName = "Non-existent Restaurant";
            string updatedDescription = "Should return not found";
            bool updatedHasDelivery = true;

            // act
            var response = await UpdateRestaurantAsync(restaurantId, updatedName, updatedDescription, updatedHasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Delete_Restaurant_When_Restaurant_Exists_Returns_NoContent()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant C", "Test Description", "Test Category");

            // act
            var response = await DeleteRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Delete_Restaurant_When_Restaurant_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = 99999; // Non-existent restaurant

            // act
            var response = await DeleteRestaurantAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Upload_Restaurant_Logo_When_Valid_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant D", "Test Description", "Test Category");
            var fileContent = new StreamContent(new MemoryStream(new byte[] { 0x01, 0x02, 0x03 }));
            string filename = "logo.jpg";
            var content = new MultipartFormDataContent
            {
                { fileContent, "file", filename }
            };

            // act
            var response = await UploadLogoAsync(restaurantId, content);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(restaurantId, body["id"].GetValue<int>());
            Assert.False(string.IsNullOrEmpty(body["logoSasUrl"].GetValue<string>()));
        }

        [Fact]
        public async Task TC022_Upload_Restaurant_Logo_When_Restaurant_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = 99999; // Non-existent restaurant
            var fileContent = new StreamContent(new MemoryStream(new byte[] { 0x01, 0x02, 0x03 }));
            string filename = "logo.jpg";
            var content = new MultipartFormDataContent
            {
                { fileContent, "file", filename }
            };

            // act
            var response = await UploadLogoAsync(restaurantId, content);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Upload_Restaurant_Logo_With_Invalid_Content_Returns_UnsupportedMediaType()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant E", "Test Description", "Test Category");
            var content = JsonContent.Create(new { file = "invalid_logo.txt" }); // invalid content type

            // act
            var response = await UploadLogoAsync(restaurantId, content);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
    }
}