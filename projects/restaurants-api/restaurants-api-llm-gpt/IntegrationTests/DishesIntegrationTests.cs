// File: DishesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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

        private async Task<HttpResponseMessage> CreateDishAsync(int restaurantId, string name, string description, double price, int kiloCalories)
        {
            var request = new
            {
                name = name,
                description = description,
                price = price,
                kiloCalories = kiloCalories
            };

            return await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", request);
        }

        private async Task<HttpResponseMessage> GetDishesAsync(int restaurantId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");
        }

        private async Task<HttpResponseMessage> DeleteDishesAsync(int restaurantId)
        {
            return await _client.DeleteAsync($"/api/restaurants/{restaurantId}/dishes");
        }

        private async Task<HttpResponseMessage> GetDishByIdAsync(int restaurantId, int dishId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");
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

        private async Task<int> CreateDishAndGetIdAsync(int restaurantId, string name, string description, double price, int kiloCalories)
        {
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        [Fact]
        public async Task TC001_Create_Dish_When_Valid_Data_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant A", "Test Description", "Test Category");
            string name = "Pasta";
            string description = "Delicious pasta";
            double price = 12.99;
            int kiloCalories = 200;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.True(body["id"].GetValue<int>() > 0);
            Assert.Equal(name, body["name"].GetValue<string>());
            Assert.Equal(description, body["description"].GetValue<string>());
            Assert.Equal(price, body["price"].GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].GetValue<int>());
        }

        [Fact]
        public async Task TC002_Create_Dish_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant B", "Test Description", "Test Category");
            string name = null;
            string description = "Delicious pasta";
            double price = 12.99;
            int kiloCalories = 200;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Dish_When_Name_Is_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant C", "Test Description", "Test Category");
            string name = "AB"; // Too short 
            string description = "Delicious pasta";
            double price = 12.99;
            int kiloCalories = 200;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Dish_When_Name_Is_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant D", "Test Description", "Test Category");
            string name = "ThisNameIsWayTooLongForDish"; // Too long
            string description = "Delicious pasta";
            double price = 12.99;
            int kiloCalories = 200;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Dish_When_Restaurant_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = 99999; // Non-existent restaurant
            string name = "Pasta";
            string description = "Delicious pasta";
            double price = 12.99;
            int kiloCalories = 200;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Get_Dishes_When_Valid_Restaurant_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant E", "Test Description", "Test Category");
            await CreateDishAsync(restaurantId, "Dish A", "Description A", 10.0, 100);

            // act
            var response = await GetDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
            Assert.True(body.Count > 0);
        }

        [Fact]
        public async Task TC007_Get_Dishes_When_Restaurant_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = 99999; // Non-existent restaurant

            // act
            var response = await GetDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Delete_Dishes_When_Valid_Restaurant_Returns_NoContent()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant F", "Test Description", "Test Category");
            await CreateDishAsync(restaurantId, "Dish B", "Description B", 10.0, 100);

            // act
            var response = await DeleteDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Delete_Dishes_When_Restaurant_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = 99999; // Non-existent restaurant

            // act
            var response = await DeleteDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Get_Dish_By_ID_When_Valid_Data_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant G", "Test Description", "Test Category");
            int dishId = await CreateDishAndGetIdAsync(restaurantId, "Dish C", "Description C", 10.0, 100);

            // act
            var response = await GetDishByIdAsync(restaurantId, dishId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(dishId, body["id"].GetValue<int>());
            Assert.Equal("Dish C", body["name"].GetValue<string>());
            Assert.Equal("Description C", body["description"].GetValue<string>());
            Assert.Equal(10.0, body["price"].GetValue<double>());
            Assert.Equal(100, body["kiloCalories"].GetValue<int>());
        }

        [Fact]
        public async Task TC011_Get_Dish_By_ID_When_Dish_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("Restaurant H", "Test Description", "Test Category");
            int dishId = 99999; // Non-existent dish

            // act
            var response = await GetDishByIdAsync(restaurantId, dishId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}