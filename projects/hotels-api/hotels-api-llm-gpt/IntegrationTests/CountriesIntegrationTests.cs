// File: CountriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class CountriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public CountriesIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private string GenerateRandomString() => $"Country_{Guid.NewGuid()}";

        private async Task<string> CreateAuthenticatedUserAndGetTokenAsync()
        {
            string email = $"user{Guid.NewGuid()}@example.com";
            string password = "Password1!";

            // Create user
            var createUserRequest = new
            {
                firstName = "John",
                lastName = "Doe",
                email = email,
                isAdmin = false,
                password = password
            };
            await _client.PostAsJsonAsync("/api/accounts", createUserRequest);

            // Authenticate and get token
            var authenticateRequest = new
            {
                email = email,
                password = password
            };
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", authenticateRequest);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].ToString();
        }

        [Fact]
        public async Task TC010_Get_All_Countries_When_Valid_Data_Returns_OK()
        {
            // Act
            var response = await _client.GetAsync("/api/countries");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
        }

        [Fact]
        public async Task TC011_Create_Country_When_Valid_Data_Returns_Created()
        {
            // Arrange
            var token = await CreateAuthenticatedUserAndGetTokenAsync();
            var requestBody = new
            {
                name = GenerateRandomString(),
                shortName = "CN"
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(requestBody),
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body?["id"]);
            Assert.Equal(requestBody.name, body?["name"].ToString());
            Assert.Equal(requestBody.shortName, body?["shortName"].ToString());
        }

        [Fact]
        public async Task TC012_Create_Country_When_Unauthorized_Returns_Unauthorized()
        {
            // Arrange
            var requestBody = new
            {
                name = GenerateRandomString(),
                shortName = "CN"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/countries", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task<int> CreateCountryAndGetIdAsync(string token)
        {
            var requestBody = new
            {
                name = GenerateRandomString(),
                shortName = "CN"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(requestBody),
                Headers = { { "Authorization", $"Bearer {token}" } }
            };
            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        [Fact]
        public async Task TC013_Get_Country_By_ID_When_Valid_ID_Returns_OK()
        {
            // Arrange
            var token = await CreateAuthenticatedUserAndGetTokenAsync();
            int countryId = await CreateCountryAndGetIdAsync(token);

            // Act
            var response = await _client.GetAsync($"/api/countries/{countryId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(countryId, body?["id"].GetValue<int>());
            Assert.NotNull(body?["name"]);
            Assert.NotNull(body?["shortName"]);
        }

        [Fact]
        public async Task TC014_Get_Country_By_ID_When_Invalid_ID_Returns_NotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/countries/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}