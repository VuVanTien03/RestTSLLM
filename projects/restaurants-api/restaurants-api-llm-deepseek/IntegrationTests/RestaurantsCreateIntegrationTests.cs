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
        private readonly string _baseUrl = "/api/restaurants";

        public RestaurantsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private string GenerateUniqueEmail() => $"{Guid.NewGuid().ToString()[..10]}@test.com";

        // TC005: Valid creation
        [Fact]
        public async Task TC005_Create_Restaurant_With_Valid_Data_Returns_Created()
        {
            // Arrange
            var request = new
            {
                name = "Valid Restaurant Name",
                description = "Valid description text",
                category = "Valid Category",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Valid City",
                street = "Valid Street",
                postalCode = "12345"
            };

            // Act
            var response = await _client.PostAsJsonAsync(_baseUrl, request);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(body["id"].GetValue<int>() > 0);
        }

        // TC006: Short name (edge case)
        [Fact]
        public async Task TC006_Create_Restaurant_With_Min_Name_Length_Returns_Created()
        {
            var request = new
            {
                name = new string('A', 3), // Minimum length
                description = new string('B', 3),
                category = new string('C', 3),
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "12345678", // Minimum digits
                city = new string('D', 3),
                street = new string('E', 3),
                postalCode = "123" // Minimum length
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // TC007: Invalid email format
        [Fact]
        public async Task TC007_Create_Restaurant_With_Invalid_Email_Returns_BadRequest()
        {
            var request = new
            {
                name = "Invalid Email Restaurant",
                description = "Valid description",
                category = "Valid Category",
                hasDelivery = true,
                contactEmail = "invalid-email-format",
                contactNumber = "1234567890",
                city = "Valid City",
                street = "Valid Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC008: Missing required fields
        [Fact]
        public async Task TC008_Create_Restaurant_Missing_Required_Fields_Returns_BadRequest()
        {
            var invalidRequest = new { }; // Missing all required fields

            var response = await _client.PostAsJsonAsync(_baseUrl, invalidRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC009: Over max name length
        [Fact]
        public async Task TC009_Create_Restaurant_With_Long_Name_Returns_BadRequest()
        {
            var request = new
            {
                name = new string('A', 33), // Max allowed 32
                description = "Valid description",
                category = "Valid Category",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Valid City",
                street = "Valid Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC010: Short description
        [Fact]
        public async Task TC010_Create_Restaurant_With_Short_Description_Returns_BadRequest()
        {
            var request = new
            {
                name = "Valid Name",
                description = "AB", // Min 3 required
                category = "Valid Category",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Valid City",
                street = "Valid Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC011: Invalid contact number format
        [Fact]
        public async Task TC011_Create_Restaurant_With_NonNumeric_Contact_Returns_BadRequest()
        {
            var request = new
            {
                name = "Valid Name",
                description = "Valid description",
                category = "Valid Category",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "12AB3456", // Contains letters
                city = "Valid City",
                street = "Valid Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC012: Invalid postal code format
        [Fact]
        public async Task TC012_Create_Restaurant_With_NonNumeric_PostalCode_Returns_BadRequest()
        {
            var request = new
            {
                name = "Valid Name",
                description = "Valid description",
                category = "Valid Category",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Valid City",
                street = "Valid Street",
                postalCode = "ABC123" // Contains letters
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC013: Over max category length
        [Fact]
        public async Task TC013_Create_Restaurant_With_Long_Category_Returns_BadRequest()
        {
            var request = new
            {
                name = "Valid Name",
                description = "Valid description",
                category = new string('C', 17), // Max 16
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Valid City",
                street = "Valid Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC014: Postal code boundary tests
        [Theory]
        [InlineData("12")]       // Below min (3)
        [InlineData("123")]     // Min
        [InlineData("123456789")] // Max (9)
        [InlineData("1234567890")] // Over max (10)
        public async Task TC014_Create_Restaurant_PostalCode_Validation_Tests(string postalCode)
        {
            var request = new
            {
                name = "PostalCode Test",
                description = "Validation test",
                category = "Test",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Test City",
                street = "Test Street",
                postalCode = postalCode
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);

            var expectedStatus = postalCode.Length switch
            {
                < 3 => HttpStatusCode.BadRequest,
                > 10 => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.Created
            };

            Assert.Equal(expectedStatus, response.StatusCode);
        }

        // TC015: City length validation
        [Theory]
        [InlineData(2, false)]
        [InlineData(3, true)]
        [InlineData(16, true)]
        [InlineData(17, false)]
        public async Task TC015_Create_Restaurant_City_Length_Validation(int length, bool isValid)
        {
            var request = new
            {
                name = "City Length Test",
                description = "Validation test",
                category = "Test",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = new string('C', length),
                street = "Test Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(isValid ? HttpStatusCode.Created : HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC016: Street max length validation
        [Fact]
        public async Task TC016_Create_Restaurant_With_Max_Street_Length_Returns_Created()
        {
            var request = new
            {
                name = "Street Length Test",
                description = "Validation test",
                category = "Test",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Test City",
                street = new string('S', 32), // Max allowed
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // TC017: Contact number length validation
        [Theory]
        [InlineData(7, false)]
        [InlineData(8, true)]
        [InlineData(13, true)]
        [InlineData(14, false)]
        public async Task TC017_Create_Restaurant_ContactNumber_Length_Validation(int length, bool isValid)
        {
            var number = new string('1', length);
            var request = new
            {
                name = "Contact Number Test",
                description = "Validation test",
                category = "Test",
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = number,
                city = "Test City",
                street = "Test Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(isValid ? HttpStatusCode.Created : HttpStatusCode.BadRequest, response.StatusCode);
        }

        // TC018: hasDelivery field type validation
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task TC018_Create_Restaurant_Valid_hasDelivery_Types(bool deliveryValue)
        {
            var request = new
            {
                name = "Delivery Test",
                description = "Validation test",
                category = "Test",
                hasDelivery = deliveryValue,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = "1234567890",
                city = "Test City",
                street = "Test Street",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // TC019: Full boundary verification - All max lengths
        [Fact]
        public async Task TC019_Create_Restaurant_All_Max_Lengths_Returns_Created()
        {
            var request = new
            {
                name = new string('N', 32),
                description = new string('D', 32),
                category = new string('C', 16),
                hasDelivery = true,
                contactEmail = GenerateUniqueEmail(),
                contactNumber = new string('1', 13),
                city = new string('C', 16),
                street = new string('S', 32),
                postalCode = new string('1', 10)
            };

            var response = await _client.PostAsJsonAsync(_baseUrl, request);
            var body = response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
    }
}