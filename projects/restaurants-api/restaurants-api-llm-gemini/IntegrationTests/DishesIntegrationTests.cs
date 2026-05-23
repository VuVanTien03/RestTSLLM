// File: DishesIntegrationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json.Nodes;
using System;

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

        private async Task<HttpResponseMessage> CreateDishAsync(int restaurantId, string name, string description, double price, int? kiloCalories)
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


        [Fact]
        public async Task TC001_Create_Dish_Valid_Data()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurant", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "Dish Description";
            double price = 10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_Dish_Missing_Name()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantMissingName", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = null;
            string description = "Dish Description";
            double price = 10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Dish_Name_Too_Short()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantNameTooShort", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Di";
            string description = "Dish Description";
            double price = 10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Dish_Name_Too_Long()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantNameTooLong", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "ThisDishNameIsWayTooLongToMeetTheRequirementsOfTheTest";
            string description = "Dish Description";
            double price = 10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Dish_Missing_Description()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantMissingDescription", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = null;
            double price = 10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Dish_Description_Too_Short()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantDescriptionTooShort", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "De";
            double price = 10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Dish_Description_Too_Long()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantDescriptionTooLong", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "ThisDishDescriptionIsWayTooLongToMeetTheRequirementsOfTheTest";
            double price = 10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Dish_Missing_Price()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantMissingPrice", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "Dish Description";
            double price = 0;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Dish_Missing_KiloCalories()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantMissingKiloCalories", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "Dish Description";
            double price = 10.99;
            int? kiloCalories = null;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Dish_Invalid_Price_Negative()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantInvalidPriceNegative", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "Dish Description";
            double price = -10.99;
            int kiloCalories = 250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Dish_Invalid_KiloCalories_Negative()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantInvalidKiloCaloriesNegative", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "Dish Description";
            double price = 10.99;
            int kiloCalories = -250;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Get_Dishes_Valid_Restaurant_ID()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantGetDishes", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Get_Dishes_Invalid_Restaurant_ID()
        {
            // arrange
            int restaurantId = 999999;

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Delete_Dishes_Valid_Restaurant_ID()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantDeleteDishes", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");

            // act
            var response = await _client.DeleteAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Delete_Dishes_Invalid_Restaurant_ID()
        {
            // arrange
            int restaurantId = 999999;

            // act
            var response = await _client.DeleteAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Get_Dish_Valid_Restaurant_And_Dish_IDs()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantGetDish", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            string name = "Dish Name";
            string description = "Dish Description";
            double price = 10.99;
            int kiloCalories = 250;
            var createDishResponse = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);
            int dishId = (await createDishResponse.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Get_Dish_Invalid_Restaurant_ID()
        {
            // arrange
            int restaurantId = 999999;
            int dishId = 1;

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Get_Dish_Invalid_Dish_ID()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync("ItalianRestaurantGetDishInvalidDishId", "Italian Description", "Italian", true, "test@example.com", "12345678", "New York", "Main Street", "10001");
            int dishId = 999999;

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<int> CreateRestaurantAndGetIdAsync(string name, string description, string category, bool hasDelivery, string contactEmail, string contactNumber, string city, string street, string postalCode)
        {
            var requestBody = new
            {
                name = name.Substring(0, (name.Length > 32 ? 32 : name.Length)),
                description = description,
                category = category,
                hasDelivery = hasDelivery,
                contactEmail = contactEmail,
                contactNumber = contactNumber,
                city = city,
                street = street,
                postalCode = postalCode
            };
            var response = await _client.PostAsJsonAsync("/api/restaurants", requestBody);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            response.EnsureSuccessStatusCode();
            return body["id"].AsValue().GetValue<int>();
        }
    }
}
