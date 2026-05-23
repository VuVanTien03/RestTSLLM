// File: DishesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class DishesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public DishesIntegrationTests(WebApplicationFactory<Program> factory)
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
        public async Task TC001_Post_Dish_When_Valid_Data_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonDocument>();

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Spaghetti", responseBody.RootElement.GetProperty("name").GetString());
            Assert.Equal("Delicious spaghetti carbonara", responseBody.RootElement.GetProperty("description").GetString());
            Assert.Equal(15.99, responseBody.RootElement.GetProperty("price").GetDouble());
            Assert.Equal(500, responseBody.RootElement.GetProperty("kiloCalories").GetInt32());
        }

        [Fact]
        public async Task TC002_Post_Dish_When_Invalid_Restaurant_ID_Returns_NotFound()
        {
            // arrange
            var request = new
            {
                name = "Spaghetti",
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync("/api/restaurants/9999999/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Post_Dish_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = (string)null,
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Post_Dish_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "",
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Post_Dish_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Sp",
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Post_Dish_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti carbonara with too many characters in the name",
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Post_Dish_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = (string)null,
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Post_Dish_When_Description_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Post_Dish_When_Description_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "Sp",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Post_Dish_When_Description_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "Spaghetti carbonara with too many characters in the description",
                price = 15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Post_Dish_When_Price_Is_Negative_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "Delicious spaghetti carbonara",
                price = -15.99,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Post_Dish_When_Price_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "Delicious spaghetti carbonara",
                price = (double?)null,
                kiloCalories = 500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Post_Dish_When_KiloCalories_Is_Negative_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = -500
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Post_Dish_When_KiloCalories_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            var request = new
            {
                name = "Spaghetti",
                description = "Delicious spaghetti carbonara",
                price = 15.99,
                kiloCalories = (int?)null
            };

            // act
            var response = await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Get_Dishes_When_Valid_Restaurant_ID_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            await CreateDishAndGetIdAsync(restaurantId, "Spaghetti", "Delicious spaghetti carbonara", 15.99, 500);

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Get_Dishes_When_Invalid_Restaurant_ID_Returns_NotFound()
        {
            // act
            var response = await _client.GetAsync("/api/restaurants/9999999/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Delete_Dishes_When_Valid_Restaurant_ID_Returns_NoContent()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");

            // act
            var response = await _client.DeleteAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Delete_Dishes_When_Invalid_Restaurant_ID_Returns_NotFound()
        {
            // act
            var response = await _client.DeleteAsync("/api/restaurants/9999999/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Get_Dish_By_ID_When_Valid_Restaurant_ID_And_Dish_ID_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            int dishId = await CreateDishAndGetIdAsync(restaurantId, "Spaghetti", "Delicious spaghetti carbonara", 15.99, 500);

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Get_Dish_By_ID_When_Invalid_Restaurant_ID_Returns_NotFound()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");
            int dishId = await CreateDishAndGetIdAsync(restaurantId, "Spaghetti", "Delicious spaghetti carbonara", 15.99, 500);

            // act
            var response = await _client.GetAsync($"/api/restaurants/9999999/dishes/{dishId}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Get_Dish_By_ID_When_Invalid_Dish_ID_Returns_NotFound()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Pizza Place", "Delicious pizzas", "Italian", true, "contact@example.com", "123456789", "Example City", "Example Street", "12345");

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/9999999");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
