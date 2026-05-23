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

        private async Task<HttpResponseMessage> CreateDishAsync(int restaurantId, string name, string description, double? price, int? kiloCalories)
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

        private async Task<HttpResponseMessage> CreateRestaurantAsync(string name, string description, string category, bool hasDelivery, string email, string phone, string city, string street, string postalCode)
        {
            var request = new
            {
                name = name,
                description = description,
                category = category,
                hasDelivery = hasDelivery,
                contactEmail = email,
                contactNumber = phone,
                city = city,
                street = street,
                postalCode = postalCode
            };

            return await _client.PostAsJsonAsync("/api/restaurants", request);
        }

        private async Task<int> CreateRestaurantAndGetIdAsync()
        {
            var response = await CreateRestaurantAsync(
                "Test Restaurant",
                "Test Description",
                "Test Category",
                true,
                "test@email.com",
                "12345678",
                "Test City",
                "Test Street",
                "12345"
            );

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC001_Create_Dish_When_Valid_Data_Returns_Created()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "Valid Dish";
            string description = "Valid Description";
            double price = 10.50;
            int kiloCalories = 500;

            // act 
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_description = body["description"].AsValue().GetValue<string>();
            var body_price = body["price"].AsValue().GetValue<double>();
            var body_kiloCalories = body["kiloCalories"].AsValue().GetValue<int>();

            Assert.True(body_id > 0);
            Assert.Equal(name, body_name);
            Assert.Equal(description, body_description);
            Assert.Equal(price, body_price);
            Assert.Equal(kiloCalories, body_kiloCalories);
        }

        [Fact]
        public async Task TC002_Create_Dish_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int invalidRestaurantId = 999999;
            string name = "Valid Dish";
            string description = "Valid Description";
            double price = 10.50;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(invalidRestaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_Dish_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = null;
            string description = "Valid Description";
            double price = 10.50;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Dish_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "ab"; // 2 chars, minimum is 3
            string description = "Valid Description";
            double price = 10.50;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Dish_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "ThisNameIsTooLong"; // 17 chars, maximum is 16
            string description = "Valid Description";
            double price = 10.50;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Dish_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "Valid Dish";
            string description = null;
            double price = 10.50;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Dish_When_Description_Too_Short_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "Valid Dish";
            string description = "ab"; // 2 chars, minimum is 3
            double price = 10.50;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Dish_When_Description_Too_Long_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "Valid Dish";
            string description = "This description is too long for a dish desc"; // 33 chars, maximum is 32
            double price = 10.50;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Dish_When_Price_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "Valid Dish";
            string description = "Valid Description";
            double? price = null;
            int kiloCalories = 500;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Dish_When_KiloCalories_Is_Null_Returns_BadRequest()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "Valid Dish";
            string description = "Valid Description";
            double price = 10.50;
            int? kiloCalories = null;

            // act
            var response = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Get_All_Dishes_When_Restaurant_Exists_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
        }

        [Fact]
        public async Task TC012_Get_All_Dishes_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int invalidRestaurantId = 999999;

            // act
            var response = await _client.GetAsync($"/api/restaurants/{invalidRestaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Delete_All_Dishes_When_Restaurant_Exists_Returns_NoContent()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();

            // act
            var response = await _client.DeleteAsync($"/api/restaurants/{restaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Delete_All_Dishes_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int invalidRestaurantId = 999999;

            // act
            var response = await _client.DeleteAsync($"/api/restaurants/{invalidRestaurantId}/dishes");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Get_Dish_By_Id_When_Found_Returns_OK()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            string name = "Valid Dish";
            string description = "Valid Description";
            double price = 10.50;
            int kiloCalories = 500;

            var createResponse = await CreateDishAsync(restaurantId, name, description, price, kiloCalories);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var dishId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{dishId}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(dishId, body["id"].AsValue().GetValue<int>());
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(price, body["price"].AsValue().GetValue<double>());
            Assert.Equal(kiloCalories, body["kiloCalories"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC016_Get_Dish_By_Id_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int invalidRestaurantId = 999999;
            int dishId = 1;

            // act
            var response = await _client.GetAsync($"/api/restaurants/{invalidRestaurantId}/dishes/{dishId}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Get_Dish_By_Id_When_Dish_Not_Found_Returns_NotFound()
        {
            // arrange
            int restaurantId = await CreateRestaurantAndGetIdAsync();
            int invalidDishId = 999999;

            // act
            var response = await _client.GetAsync($"/api/restaurants/{restaurantId}/dishes/{invalidDishId}");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
