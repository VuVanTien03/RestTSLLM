// File: DishesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

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

        private async Task<int> CreateRestaurantAsync(string name, string description, string contactEmail, string contactNumber, string category, bool hasDelivery, string city, string street, string postalCode)
        {
            var requestBody = new
            {
                name = name,
                description = description,
                contactEmail = contactEmail,
                contactNumber = contactNumber,
                category = category,
                hasDelivery = hasDelivery,
                city = city,
                street = street,
                postalCode = postalCode
            };

            var response = await _client.PostAsJsonAsync("/api/restaurants", requestBody);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> CreateDishAsync(int restaurantId, string name, string description, double? price, int? kiloCalories)
        {
            var requestBody = new
            {
                name = name,
                description = description,
                price = price,
                kiloCalories = kiloCalories
            };

            return await _client.PostAsJsonAsync($"/api/restaurants/{restaurantId}/dishes", requestBody);
        }

        private async Task<HttpResponseMessage> GetDishesAsync(int restaurantId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");
        }

        private async Task<HttpResponseMessage> DeleteDishesAsync(int restaurantId)
        {
            return await _client.DeleteAsync($"/api/restaurants/{restaurantId}/dishes");
        }

        private async Task<HttpResponseMessage> GetDishAsync(int restaurantId, int dishId)
        {
            return await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");
        }

        [Fact]
        public async Task TC001_Create_Dish_When_Valid_Data_Returns_Created()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "Sushi", "Japanese dish", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Dish_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, null, "Japanese dish", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Dish_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "", "Japanese dish", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Dish_When_Name_Length_Less_Than_3_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "ab", "Japanese dish", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Dish_When_Name_Length_Greater_Than_16_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "abcdefghiklmnopqrstuvwxyz", "Japanese dish", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Dish_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "Sushi", null, 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Dish_When_Description_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "Sushi", "", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Dish_When_Description_Length_Less_Than_3_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "Sushi", "ab", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Dish_When_Description_Length_Greater_Than_32_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "Sushi", "abcdefghiklmnopqrstuvwxyzabcdefghiklmnop", 15.99, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Dish_When_Price_Is_Null_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "Sushi", "Japanese dish", null, 500);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Dish_When_KiloCalories_Is_Null_Returns_BadRequest()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await CreateDishAsync(restaurantId, "Sushi", "Japanese dish", 15.99, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Get_Dishes_When_Valid_Data_Returns_Ok()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await GetDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Delete_Dishes_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await DeleteDishesAsync(restaurantId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Delete_Dishes_When_Restaurant_Id_Does_Not_Exist_Returns_NotFound()
        {
            // act
            var response = await DeleteDishesAsync(9999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Get_Dish_By_Id_When_Valid_Data_Returns_Ok()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var dishResponse = await CreateDishAsync(restaurantId, "Sushi", "Japanese dish", 15.99, 500);
            var body = await dishResponse.Content.ReadFromJsonAsync<JsonObject>();
            var dishId = body["id"].AsValue().GetValue<int>();

            var response = await GetDishAsync(restaurantId, dishId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Get_Dish_By_Id_When_Dish_Id_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            var restaurantId = await CreateRestaurantAsync("McDonald's", "Fast Food", "mcdonalds@email.com", "1234567890", "Fast Food", true, "New York", "5th Avenue", "10001");

            // act
            var response = await GetDishAsync(restaurantId, 9999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}