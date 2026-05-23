// File: AccountIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class AccountIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AccountIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private string GenerateRandomEmail() => $"user{Guid.NewGuid()}@example.com";

        [Fact]
        public async Task TC001_Create_ApiUser_When_Valid_Data_Returns_OK()
        {
            // Arrange
            var requestBody = new
            {
                firstName = "John",
                lastName = "Doe",
                email = GenerateRandomEmail(),
                isAdmin = false,
                password = "Password1!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_ApiUser_When_Missing_Required_Field_Returns_BadRequest()
        {
            // Arrange
            var requestBody = new
            {
                firstName = (string?)null,
                lastName = "Doe",
                email = GenerateRandomEmail(),
                isAdmin = false,
                password = "Password1!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_ApiUser_When_Invalid_Email_Format_Returns_BadRequest()
        {
            // Arrange
            var requestBody = new
            {
                firstName = "John",
                lastName = "Doe",
                email = "invalid-email",
                isAdmin = false,
                password = "Password1!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_ApiUser_When_Password_Too_Short_Returns_BadRequest()
        {
            // Arrange
            var requestBody = new
            {
                firstName = "John",
                lastName = "Doe",
                email = GenerateRandomEmail(),
                isAdmin = false,
                password = "Pwd1!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private async Task CreateUserAsync(string email, string password)
        {
            var requestBody = new
            {
                firstName = "John",
                lastName = "Doe",
                email = email,
                isAdmin = false,
                password = password
            };

            await _client.PostAsJsonAsync("/api/accounts", requestBody);
        }

        [Fact]
        public async Task TC005_Authenticate_User_When_Valid_Data_Returns_OK()
        {
            // Arrange
            string email = GenerateRandomEmail();
            string password = "Password1!";
            await CreateUserAsync(email, password);

            var loginRequest = new
            {
                email = email,
                password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body?["token"]);
            Assert.NotNull(body?["refreshToken"]);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Authenticate_User_When_Invalid_Password_Returns_Unauthorized()
        {
            // Arrange
            string email = GenerateRandomEmail();
            string password = "Password1!";
            string wrongPassword = "WrongPassword!";
            await CreateUserAsync(email, password);

            var loginRequest = new
            {
                email = email,
                password = wrongPassword
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Authenticate_User_When_Invalid_Email_Returns_Unauthorized()
        {
            // Arrange
            string email = GenerateRandomEmail();
            string password = "Password1!";
            string invalidEmail = "invalid@example.com";
            await CreateUserAsync(email, password);

            var loginRequest = new
            {
                email = invalidEmail,
                password = password
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task<(string token, string refreshToken)> GenerateValidAuthTokensAsync()
        {
            string email = GenerateRandomEmail();
            string password = "Password1!";
            await CreateUserAsync(email, password);

            var loginRequest = new
            {
                email = email,
                password = password
            };

            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            return (body["token"].ToString(), body["refreshToken"].ToString());
        }

        [Fact]
        public async Task TC008_Refresh_Token_When_Valid_Data_Returns_OK()
        {
            // Arrange
            var (token, refreshToken) = await GenerateValidAuthTokensAsync();

            var requestBody = new
            {
                token = token,
                refreshToken = refreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts/refreshtokens", requestBody);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body?["token"]);
            Assert.NotNull(body?["refreshToken"]);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Refresh_Token_When_Invalid_RefreshToken_Returns_Unauthorized()
        {
            // Arrange
            var (token, _) = await GenerateValidAuthTokensAsync();
            string invalidRefreshToken = "invalid_refresh_token";

            var requestBody = new
            {
                token = token,
                refreshToken = invalidRefreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts/refreshtokens", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}