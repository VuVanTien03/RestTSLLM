// File: UsersIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net.Http.Json;
using System.Net;

namespace TodoApi.IntegrationTests
{
    public class UsersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UsersIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateUserAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users", request);
        }

        private async Task<HttpResponseMessage> AuthenticateUserAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users/token", request);
        }

        [Fact]
        public async Task TC001_Create_User_With_Valid_Data_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("validUser1", "ValidP@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_With_Existing_Username_Returns_BadRequest()
        {
            // arrange
            var existingUsername = "existingUser";
            await CreateUserAsync(existingUsername, "ValidP@ssw0rd");

            // act
            var response = await CreateUserAsync(existingUsername, "ValidP@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_With_Null_Username_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync(null, "ValidP@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_With_Empty_Username_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("", "ValidP@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_With_Short_Username_Returns_BadRequest()
        {
            // arrange
            var shortUsername = new string('a', 0);

            // act
            var response = await CreateUserAsync(shortUsername, "ValidP@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_With_Long_Username_Returns_BadRequest()
        {
            // arrange
            var longUsername = new string('a', 129);

            // act
            var response = await CreateUserAsync(longUsername, "ValidP@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_With_Null_Password_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUser2", null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_With_Empty_Password_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUser3", "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_With_Short_Password_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUser4", "A1@");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_With_Long_Password_Returns_BadRequest()
        {
            // arrange
            var longPassword = "A1@" + new string('a', 30); // total length = 33

            // act
            var response = await CreateUserAsync("validUser5", longPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Authenticate_User_With_Valid_Data_Returns_OK()
        {
            // arrange
            var username = "authUserValid";
            var password = "ValidP@ssw0rd";
            await CreateUserAsync(username, password);

            // act
            var response = await AuthenticateUserAsync(username, password);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            Assert.False(string.IsNullOrEmpty(token));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Authenticate_User_With_Invalid_Password_Returns_BadRequest()
        {
            // arrange
            var username = "authUserInvalidPass";
            var validPassword = "ValidP@ssw0rd";
            var invalidPassword = "WrongP@ssw0rd";
            await CreateUserAsync(username, validPassword);

            // act
            var response = await AuthenticateUserAsync(username, invalidPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Authenticate_User_With_Nonexistent_Username_Returns_BadRequest()
        {
            // act
            var response = await AuthenticateUserAsync("nonexistentUser", "ValidP@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
