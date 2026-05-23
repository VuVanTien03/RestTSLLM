using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class AccountTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AccountTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateUserAsync(object userData)
        {
            return await _client.PostAsJsonAsync("/api/accounts", userData);
        }

        private async Task<HttpResponseMessage> LoginAsync(string email, string password)
        {
            var loginData = new { email, password };
            return await _client.PostAsJsonAsync("/api/accounts/tokens", loginData);
        }

        private string GenerateUniqueEmail() => $"{Guid.NewGuid()}@test.com";

        [Fact]
        public async Task TC001_Create_User_When_Valid_Data_Returns_OK()
        {
            // Arrange
            var userData = new
            {
                firstName = "John",
                lastName = "Doe",
                email = GenerateUniqueEmail(),
                password = "Val1d!Pass",
                isAdmin = false
            };

            // Act
            var response = await CreateUserAsync(userData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_When_FName_Null_Returns_BadRequest()
        {
            // Arrange
            var userData = new
            {
                firstName = (string)null,
                lastName = "Doe",
                email = GenerateUniqueEmail(),
                password = "Val1d!Pass",
                isAdmin = false
            };

            // Act
            var response = await CreateUserAsync(userData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_When_FName_Empty_Returns_BadRequest()
        {
            // Arrange
            var userData = new
            {
                firstName = "",
                lastName = "Doe",
                email = GenerateUniqueEmail(),
                password = "Val1d!Pass",
                isAdmin = false
            };

            // Act
            var response = await CreateUserAsync(userData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_When_Password_Short_Returns_BadRequest()
        {
            // Arrange
            var userData = new
            {
                firstName = "John",
                lastName = "Doe",
                email = GenerateUniqueEmail(),
                password = "V1!",
                isAdmin = false
            };

            // Act
            var response = await CreateUserAsync(userData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_When_Password_Invalid_Complexity_Returns_BadRequest()
        {
            // Arrange
            var userData = new
            {
                firstName = "John",
                lastName = "Doe",
                email = GenerateUniqueEmail(),
                password = "invalidpassword",
                isAdmin = false
            };

            // Act
            var response = await CreateUserAsync(userData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC101_Login_Valid_Credentials_Returns_Token()
        {
            // Arrange
            var email = GenerateUniqueEmail();
            var password = "Val1d!Pass";

            // Create user first
            await CreateUserAsync(new
            {
                firstName = "John",
                lastName = "Doe",
                email,
                password,
                isAdmin = false
            });

            // Act
            var response = await LoginAsync(email, password);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(string.IsNullOrEmpty(body["userId"].ToString()));
            Assert.False(string.IsNullOrEmpty(body["token"].ToString()));
            Assert.False(string.IsNullOrEmpty(body["refreshToken"].ToString()));
        }

        [Fact]
        public async Task TC102_Login_Invalid_Password_Returns_Unauthorized()
        {
            // Arrange
            var email = GenerateUniqueEmail();

            // Create user first with valid password
            await CreateUserAsync(new
            {
                firstName = "John",
                lastName = "Doe",
                email,
                password = "Val1d!Pass",
                isAdmin = false
            });

            // Act
            var response = await LoginAsync(email, "wrongpassword");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC502_Refresh_Token_Invalid_Returns_Unauthorized()
        {
            // Arrange
            var invalidToken = new
            {
                userId = "invalid",
                token = "invalid",
                refreshToken = "invalid"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/accounts/refreshtokens", invalidToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC601_Create_User_Password_Max_Length_Returns_OK()
        {
            // Arrange
            var userData = new
            {
                firstName = "Max",
                lastName = "Pass",
                email = GenerateUniqueEmail(),
                password = "A1!bcdefghijklm", // 15 characters
                isAdmin = false
            };

            // Act
            var response = await CreateUserAsync(userData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
