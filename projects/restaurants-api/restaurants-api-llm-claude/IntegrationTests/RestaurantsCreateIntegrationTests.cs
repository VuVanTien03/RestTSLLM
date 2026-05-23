using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public partial class RestaurantsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
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

        [Fact]
        public async Task TC018_Create_Restaurant_When_Valid_Data_Returns_Created()
        {
            // arrange
            string name = "Valid Restaurant 1";
            string description = "Valid Description 1";
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid1@email.com";
            string phone = "12345678";
            string city = "Valid City 1";
            string street = "Valid Street 1";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_description = body["description"].AsValue().GetValue<string>();
            var body_category = body["category"].AsValue().GetValue<string>();
            var body_hasDelivery = body["hasDelivery"].AsValue().GetValue<bool>();
            var body_city = body["city"].AsValue().GetValue<string>();
            var body_street = body["street"].AsValue().GetValue<string>();
            var body_postalCode = body["postalCode"].AsValue().GetValue<string>();

            Assert.True(body_id > 0);
            Assert.Equal(name, body_name);
            Assert.Equal(description, body_description);
            Assert.Equal(category, body_category);
            Assert.Equal(hasDelivery, body_hasDelivery);
            Assert.Equal(city, body_city);
            Assert.Equal(street, body_street);
            Assert.Equal(postalCode, body_postalCode);
        }

        [Fact]
        public async Task TC019_Create_Restaurant_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = null;
            string description = "Valid Description 2";
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid2@email.com";
            string phone = "12345678";
            string city = "Valid City 2";
            string street = "Valid Street 2";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Create_Restaurant_When_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "ab"; // 2 chars, minimum is 3
            string description = "Valid Description 3";
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid3@email.com";
            string phone = "12345678";
            string city = "Valid City 3";
            string street = "Valid Street 3";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Create_Restaurant_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "This restaurant name is way too long to"; // 33 chars, maximum is 32
            string description = "Valid Description 4";
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid4@email.com";
            string phone = "12345678";
            string city = "Valid City 4";
            string street = "Valid Street 4";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("invalidmail")] // missing @
        [InlineData("invalid@")] // missing domain
        [InlineData("@invalid.com")] // missing local part
        public async Task TC022_Create_Restaurant_When_Invalid_Email_Returns_BadRequest(string invalidEmail)
        {
            // arrange
            string name = "Valid Restaurant 5";
            string description = "Valid Description 5";
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = invalidEmail;
            string phone = "12345678";
            string city = "Valid City 5";
            string street = "Valid Street 5";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("123abc")] // contains letters
        [InlineData("123456!")] // contains special chars
        [InlineData("123 456")] // contains space
        [InlineData("1234567")] // too short (7 chars, min is 8)
        [InlineData("12345678901234")] // too long (14 chars, max is 13)
        public async Task TC023_Create_Restaurant_When_Invalid_Contact_Number_Returns_BadRequest(string invalidPhone)
        {
            // arrange
            string name = "Valid Restaurant 6";
            string description = "Valid Description 6";
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid6@email.com";
            string phone = invalidPhone;
            string city = "Valid City 6";
            string street = "Valid Street 6";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("12")] // too short (2 chars, min is 3)
        [InlineData("12345678901")] // too long (11 chars, max is 10)
        [InlineData("123abc")] // contains letters
        [InlineData("123-456")] // contains special chars
        [InlineData("123 456")] // contains space
        public async Task TC024_Create_Restaurant_When_Invalid_Postal_Code_Returns_BadRequest(string invalidPostalCode)
        {
            // arrange
            string name = "Valid Restaurant 7";
            string description = "Valid Description 7";
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid7@email.com";
            string phone = "12345678";
            string city = "Valid City 7";
            string street = "Valid Street 7";
            string postalCode = invalidPostalCode;

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC_Create_Restaurant_When_Description_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = "Valid Restaurant 8";
            string description = null;
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid8@email.com";
            string phone = "12345678";
            string city = "Valid City 8";
            string street = "Valid Street 8";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC_Create_Restaurant_When_Description_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "Valid Restaurant 9";
            string description = "ab"; // 2 chars, minimum is 3
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid9@email.com";
            string phone = "12345678";
            string city = "Valid City 9";
            string street = "Valid Street 9";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC_Create_Restaurant_When_Description_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "Valid Restaurant 10";
            string description = "This description is way too long to be"; // 33 chars, maximum is 32
            string category = "Valid Category";
            bool hasDelivery = true;
            string email = "valid10@email.com";
            string phone = "12345678";
            string city = "Valid City 10";
            string street = "Valid Street 10";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC_Create_Restaurant_When_Category_Is_Null_Returns_BadRequest()
        {
            // arrange
            string name = "Valid Restaurant 11";
            string description = "Valid Description 11";
            string category = null;
            bool hasDelivery = true;
            string email = "valid11@email.com";
            string phone = "12345678";
            string city = "Valid City 11";
            string street = "Valid Street 11";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC_Create_Restaurant_When_Category_Too_Short_Returns_BadRequest()
        {
            // arrange
            string name = "Valid Restaurant 12";
            string description = "Valid Description 12";
            string category = "ab"; // 2 chars, minimum is 3
            bool hasDelivery = true;
            string email = "valid12@email.com";
            string phone = "12345678";
            string city = "Valid City 12";
            string street = "Valid Street 12";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC_Create_Restaurant_When_Category_Too_Long_Returns_BadRequest()
        {
            // arrange
            string name = "Valid Restaurant 13";
            string description = "Valid Description 13";
            string category = "ThisCategoryIsTooLong"; // 17 chars, maximum is 16
            bool hasDelivery = true;
            string email = "valid13@email.com";
            string phone = "12345678";
            string city = "Valid City 13";
            string street = "Valid Street 13";
            string postalCode = "12345";

            // act
            var response = await CreateRestaurantAsync(name, description, category, hasDelivery,
                email, phone, city, street, postalCode);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}