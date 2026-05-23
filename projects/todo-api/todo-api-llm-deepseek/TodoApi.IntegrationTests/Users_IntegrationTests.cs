// File: UsersIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class UsersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly Random _random = new();

        public UsersIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private string GenerateUniqueUsername(string prefix = "user")
            => $"{prefix}_{_random.Next(1000, 9999)}_{DateTime.Now.Ticks}";

        [Fact]
        public async Task TC001_Create_User_With_Valid_Data_Returns_OK()
        {
            // Arrange
            var username = GenerateUniqueUsername();
            var request = new { username, password = "P@ssw0rd123" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_With_Existing_Username_Returns_BadRequest()
        {
            // Arrange
            var username = GenerateUniqueUsername();
            await _client.PostAsJsonAsync("/users", new { username, password = "P@ssw0rd1!" });

            var request = new { username, password = "NewPass123!" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_With_Invalid_Username_Format_Returns_BadRequest()
        {
            // Arrange
            var request = new { username = "invalid@user!", password = "ValidPass1!" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_With_Long_Username_Returns_BadRequest()
        {
            // Arrange
            var longUsername = new string('a', 129);
            var request = new { username = longUsername, password = "ValidPass1!" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_With_Empty_Username_Returns_BadRequest()
        {
            // Arrange
            var request = new { username = "", password = "ValidPass1!" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_When_Missing_Username_Returns_BadRequest()
        {
            // Arrange
            var request = new { password = "ValidPass1!" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_With_Minimum_Username_Size_Returns_OK()
        {
            // Arrange
            var request = new { username = "a", password = "P@ss1!" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_With_Invalid_Password_Complexity_Returns_BadRequest()
        {
            // Arrange
            var username = GenerateUniqueUsername();
            var request = new { username, password = "missingcomplexity" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_With_Short_Password_Returns_BadRequest()
        {
            // Arrange
            var username = GenerateUniqueUsername();
            var request = new { username, password = "P@a1" };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_With_Long_Password_Returns_BadRequest()
        {
            // Arrange
            var username = GenerateUniqueUsername();
            var longPassword = new string('a', 33) + "A1!";
            var request = new { username, password = longPassword };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_When_Missing_Password_Returns_BadRequest()
        {
            // Arrange
            var request = new { username = GenerateUniqueUsername() };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_With_Valid_Password_Complexity_Returns_OK()
        {
            // Arrange
            var request = new
            {
                username = GenerateUniqueUsername(),
                password = "ValidP@ss1!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Authenticate_With_Valid_Credentials_Returns_Token()
        {
            // Arrange
            var username = GenerateUniqueUsername();
            var password = "ValidP@ss1!";
            await _client.PostAsJsonAsync("/users", new { username, password });

            // Act
            var response = await _client.PostAsJsonAsync("/users/token", new { username, password });

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["token"]);
            Assert.False(string.IsNullOrEmpty(body["token"].ToString()));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Authenticate_With_Invalid_Password_Returns_BadRequest()
        {
            // Arrange
            var username = GenerateUniqueUsername();
            await _client.PostAsJsonAsync("/users", new { username, password = "CorrectP@ss1!" });

            // Act
            var response = await _client.PostAsJsonAsync("/users/token", new
            {
                username,
                password = "WrongP@ss2!"
            });

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Authenticate_With_NonExistingUser_Returns_BadRequest()
        {
            // Arrange
            var request = new { username = "nonexistent", password = "AnyP@ss1!" };

            // Act
            var response = await _client.PostAsJsonAsync("/users/token", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Complex_Username_With_Allowed_Specials_Returns_OK()
        {
            // Arrange
            var request = new
            {
                username = "user.name+test@domain",
                password = "ValidP@ss1!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Authenticate_With_Additional_Properties_Returns_BadRequest()
        {
            // Arrange
            var request = new
            {
                username = "testUser",
                password = "testPass",
                extra = "invalid"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/users/token", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Create_User_With_Minimum_Password_Returns_OK()
        {
            // Arrange
            var request = new
            {
                username = GenerateUniqueUsername("minpass"),
                password = "Aa1!bb"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/users", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}