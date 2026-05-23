// File: UsersIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

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

        private async Task<HttpResponseMessage> CreateTokenAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users/token", request);
        }

        [Fact]
        public async Task TC001_Create_User_When_Valid_Data_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("validUsername1", "ValidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_When_Username_Already_Exists_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("existingUser2", "ValidPass1!");

            // act
            var response = await CreateUserAsync("existingUser2", "ValidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_When_Username_Is_Empty_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("", "ValidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_When_Username_Has_Minimum_Size_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("a", "ValidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_When_Username_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var username = new string('a', 128);

            // act
            var response = await CreateUserAsync(username, "ValidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_When_Username_Exceeds_Maximum_Size_Returns_BadRequest()
        {
            // arrange
            var username = new string('a', 129);

            // act
            var response = await CreateUserAsync(username, "ValidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_When_Password_Is_Null_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUsername7", null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_When_Password_Is_Empty_String_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUsername8", "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_When_Password_Too_Short_Returns_BadRequest()
        {
            // arrange
            var password = "Pass!"; // 5 characters

            // act
            var response = await CreateUserAsync("validUsername9", password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_When_Password_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            var password = "Pass1!"; // 6 characters

            // act
            var response = await CreateUserAsync("validUsername10", password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_When_Password_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var password = new string('P', 31) + "!"; // 32 characters

            // act
            var response = await CreateUserAsync("validUsername11", password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_When_Password_Exceeds_Maximum_Size_Returns_BadRequest()
        {
            // arrange
            var password = new string('P', 32) + "!"; // 33 characters

            // act
            var response = await CreateUserAsync("validUsername12", password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_User_When_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUsername13", "p@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_User_When_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUsername14", "P@SSW0RD");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_User_When_Password_Missing_Digit_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUsername15", "P@ssword");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_User_When_Password_Missing_Non_Alphanumeric_Character_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("validUsername16", "Passw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Authenticate_User_When_Valid_Data_Returns_OK()
        {
            // arrange
            await CreateUserAsync("validUsername17", "ValidPass1!");

            // act
            var response = await CreateTokenAsync("validUsername17", "ValidPass1!");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            Assert.False(string.IsNullOrEmpty(body_token));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Authenticate_User_When_Invalid_Password_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("validUsername18", "ValidPass1!");

            // act
            var response = await CreateTokenAsync("validUsername18", "InvalidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Authenticate_User_When_Invalid_Username_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("validUsername19", "ValidPass1!");

            // act
            var response = await CreateTokenAsync("invalidUsername", "ValidPass1!");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}