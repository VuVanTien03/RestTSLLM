// File: RestaurantsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

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

        private async Task<int> CreateRestaurantAndGetIdAsync(string name, string description, string category, bool hasDelivery, string contactEmail, string contactNumber, string city, string street, string postalCode)
        {
            var request = new
            {
                name = name,
                description = description,
                category = category,
                hasDelivery = hasDelivery,
                contactEmail = contactEmail,
                contactNumber = contactNumber,
                city = city,
                street = street,
                postalCode = postalCode
            };

            var response = await _client.PostAsJsonAsync("/api/restaurants", request);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return responseBody.RootElement.GetProperty("id").GetInt32();
        }

        private async Task<int> CreateDishAndGetIdAsync(int restaurantId, string name, string description, double price, int kiloCalories)
        {
            var request = new
            {
                name = name,
                description = description,
                price = price,
                kiloCalories = kiloCalories
            };

            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return responseBody.RootElement.GetProperty("id").GetInt32();
        }

        [Fact]
        public async Task TC022_Get_Restaurants_When_No_Query_Parameters_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Get_Restaurants_When_Valid_SearchPhrase_Returns_OK()
        {
            // arrange
            await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");

            // act
            var response = await _client.GetAsync("/api/restaurants?searchPhrase=Italian");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Get_Restaurants_When_Invalid_PageNumber_Returns_BadRequest()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?pageNumber=0");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Get_Restaurants_When_Valid_PageNumber_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?pageNumber=1");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Get_Restaurants_When_Invalid_PageSize_Returns_BadRequest()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?pageSize=0");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Get_Restaurants_When_Valid_PageSize_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?pageSize=10");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Get_Restaurants_When_Invalid_SortBy_Returns_BadRequest()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?sortBy=Address");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Get_Restaurants_When_Valid_SortBy_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?sortBy=Name");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Get_Restaurants_When_Invalid_SortDirection_Returns_BadRequest()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?sortDirection=AscendingDescending");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Get_Restaurants_When_Valid_SortDirection_Returns_OK()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants?sortDirection=Ascending");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Get_Restaurant_By_ID_When_Valid_ID_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Get_Restaurant_By_ID_When_Invalid_ID_Returns_NotFound()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants/9999999");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Patch_Restaurant_By_ID_When_Valid_Data_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Updated Pizza Place",
                description = "Updated delicious pizzas",
                hasDelivery = false
            };

            // act
            var response = await _client.PatchAsJsonAsync($"/api/restaurants/{restaurantId}", request);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Updated Pizza Place", responseBody.RootElement.GetProperty("name").GetString());
            Assert.Equal("Updated delicious pizzas", responseBody.RootElement.GetProperty("description").GetString());
            Assert.Equal(false, responseBody.RootElement.GetProperty("hasDelivery").GetBoolean());
        }

        [Fact]
        public async Task TC039_Patch_Restaurant_By_ID_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var request = new
            {
                name = "Updated Pizza Place",
                description = "Updated delicious pizzas",
                hasDelivery = false
            };

            // act
            var response = await _client.PatchAsJsonAsync("/api/restaurants/9999999", request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Delete_Restaurant_By_ID_When_Valid_ID_Returns_NoContent()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");

            // act
            var response = await _client.DeleteAsync($"/api/restaurants/{restaurantId}");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Delete_Restaurant_By_ID_When_Invalid_ID_Returns_NotFound()
        {
            // act
            var response = await _client.DeleteAsync("/api/restaurants/9999999");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Post_Restaurant_Logo_When_Valid_ID_And_Valid_File_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var streamContent = new ByteArrayContent(new byte[] { 0x01, 0x02, 0x03 }); // simulate file
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            using var formContent = new MultipartFormDataContent
            {
                { streamContent, "file", "somefile.jpg" }
            };

            // act
            var response = await _client.PostAsync($"/api/restaurants/{restaurantId}/logo", formContent);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Post_Restaurant_Logo_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var streamContent = new ByteArrayContent(new byte[] { 0x01, 0x02, 0x03 }); // simulate file
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            using var formContent = new MultipartFormDataContent
            {
                { streamContent, "file", "somefile.jpg" }
            };

            // act
            var response = await _client.PostAsync("/api/restaurants/9999999/logo", formContent);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Post_Restaurant_Logo_When_Invalid_File_Type_Returns_UnsupportedMediaType()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = JsonContent.Create(new { file = "somefile.txt" }); // invalid file type

            // act
            var response = await _client.PostAsync($"/api/restaurants/{restaurantId}/logo", request);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
    }
}