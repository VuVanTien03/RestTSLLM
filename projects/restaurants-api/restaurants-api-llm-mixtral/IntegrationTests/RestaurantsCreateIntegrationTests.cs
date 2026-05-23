// File: RestaurantsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace IntegrationTests
{
    public class RestaurantsIntegration1Tests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsIntegration1Tests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateRestaurantAsync(string name, string description, string category, bool hasDelivery, string contactEmail, string contactNumber, string city, string street, string postalCode)
        {
            var requestBody = new
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

            return await _client.PostAsJsonAsync("/api/restaurants", requestBody);
        }

        [Fact]
        public async Task TC033_Create_Restaurant_When_Valid_Data_Returns_Created()
        {
            // arrange
            string name = "ValidName";
            string description = "Valid description";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(category, body["category"].AsValue().GetValue<string>());
            Assert.Equal(hasDelivery, body["hasDelivery"].AsValue().GetValue<bool>());
            Assert.Equal(city, body["city"].AsValue().GetValue<string>());
            Assert.Equal(street, body["street"].AsValue().GetValue<string>());
            Assert.Equal(postalCode, body["postalCode"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC034_Create_Restaurant_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = null;
            string description = "Valid description";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Create_Restaurant_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string name = "";
            string description = "Valid description";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Create_Restaurant_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "ab";
            string description = "Valid description";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Create_Restaurant_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "ThisIsAVeryLongRestaurantName1234";
            string description = "Valid description";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Create_Restaurant_When_Name_Has_Minimum_Size_Returns_Created()
        {
            // arrange
            string name = "abc";
            string description = "Valid description";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(category, body["category"].AsValue().GetValue<string>());
            Assert.Equal(hasDelivery, body["hasDelivery"].AsValue().GetValue<bool>());
            Assert.Equal(city, body["city"].AsValue().GetValue<string>());
            Assert.Equal(street, body["street"].AsValue().GetValue<string>());
            Assert.Equal(postalCode, body["postalCode"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC039_Create_Restaurant_When_Name_Has_Maximum_Size_Returns_Created()
        {
            // arrange
            string name = "ThisIsAVeryLongRestaurant123";
            string description = "Valid description";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(category, body["category"].AsValue().GetValue<string>());
            Assert.Equal(hasDelivery, body["hasDelivery"].AsValue().GetValue<bool>());
            Assert.Equal(city, body["city"].AsValue().GetValue<string>());
            Assert.Equal(street, body["street"].AsValue().GetValue<string>());
            Assert.Equal(postalCode, body["postalCode"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC040_Create_Restaurant_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = null;
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Create_Restaurant_When_Description_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = "";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Create_Restaurant_When_Description_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = "ab";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Create_Restaurant_When_Description_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = "ThisIsAVeryLongDescription1234567";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Create_Restaurant_When_Description_Has_Minimum_Size_Returns_Created()
        {
            // arrange
            string name = "ValidName";
            string description = "abc";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(category, body["category"].AsValue().GetValue<string>());
            Assert.Equal(hasDelivery, body["hasDelivery"].AsValue().GetValue<bool>());
            Assert.Equal(city, body["city"].AsValue().GetValue<string>());
            Assert.Equal(street, body["street"].AsValue().GetValue<string>());
            Assert.Equal(postalCode, body["postalCode"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC045_Create_Restaurant_When_Description_Has_Maximum_Size_Returns_Created()
        {
            // arrange
            string name = "ValidName";
            string description = "ThisIsAVeryLongDescription1234";
            string category = "ValidCategory";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["id"].AsValue().GetValue<int>() > 0);
            Assert.Equal(name, body["name"].AsValue().GetValue<string>());
            Assert.Equal(description, body["description"].AsValue().GetValue<string>());
            Assert.Equal(category, body["category"].AsValue().GetValue<string>());
            Assert.Equal(hasDelivery, body["hasDelivery"].AsValue().GetValue<bool>());
            Assert.Equal(city, body["city"].AsValue().GetValue<string>());
            Assert.Equal(street, body["street"].AsValue().GetValue<string>());
            Assert.Equal(postalCode, body["postalCode"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC046_Create_Restaurant_When_Category_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = "Valid description";
            string category = null;
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Create_Restaurant_When_Category_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = "Valid description";
            string category = "";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Create_Restaurant_When_Category_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = "Valid description";
            string category = "ab";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Create_Restaurant_When_Category_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "ValidName";
            string description = "Valid description";
            string category = "ThisIsAVeryLongCategory123";
            bool hasDelivery = true;
            string contactEmail = "valid@example.com";
            string contactNumber = "1234567890";
            string city = "ValidCity";
            string street = "ValidStreet";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery, contactEmail, contactNumber, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    } 
}