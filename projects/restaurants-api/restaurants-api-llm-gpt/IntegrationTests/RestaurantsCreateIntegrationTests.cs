// File: RestaurantsCreateIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class RestaurantsCreateIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsCreateIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateRestaurantAsync(string name, string description, string category, bool hasDelivery, string contactEmail, string contactNumber, string city, string street, string postalCode)
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

            return await _client.PostAsJsonAsync("/api/restaurants", request);
        }

        [Fact]
        public async Task TC012_Create_Restaurant_When_Valid_Data_Returns_Created()
        {
            // arrange
            string name = "Pizza Hut";
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.True(body["id"].GetValue<int>() > 0);
            Assert.Equal(name, body["name"].GetValue<string>());
            Assert.Equal(description, body["description"].GetValue<string>());
            Assert.Equal(category, body["category"].GetValue<string>());
            Assert.Equal(hasDelivery, body["hasDelivery"].GetValue<bool>());
            Assert.Equal(city, body["city"].GetValue<string>());
            Assert.Equal(street, body["street"].GetValue<string>());
            Assert.Equal(postalCode, body["postalCode"].GetValue<string>());
        }

        [Fact]
        public async Task TC013_Create_Restaurant_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = null; // null name
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_Restaurant_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string name = ""; // empty name
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_Restaurant_When_Name_Is_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "AB"; // too short
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_Restaurant_When_Name_Is_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "ThisNameIsWayTooLongForARestaurantNameMoreThanAllowed"; // too long
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_Restaurant_When_Contact_Email_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            string name = "Pizza Hut";
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "invalid-email"; // invalid email
            string contactNumber = "12345678";
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_Restaurant_When_Contact_Number_Is_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "Pizza Hut";
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "1234"; // too short
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_Restaurant_When_Contact_Number_Is_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "Pizza Hut";
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678901234"; // too long
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Create_Restaurant_When_City_Is_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "Pizza Hut";
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "NY"; // too short
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Create_Restaurant_When_City_Is_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "Pizza Hut";
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "CityNameThatIsTooLong"; // too long
            string street = "1st Avenue";
            string postalCode = "10001";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Create_Restaurant_When_Postal_Code_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            string name = "Pizza Hut";
            string description = "Famous pizza chain.";
            string category = "Fast Food";
            bool hasDelivery = true;
            string contactEmail = "contact@pizzahut.com";
            string contactNumber = "12345678";
            string city = "New York";
            string street = "1st Avenue";
            string postalCode = "Invalid!"; // invalid postal code

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}