// File: UsersIntegrationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

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
        public async Task TC001_Create_User_With_Valid_Data_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("valid_username_1", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_With_Existing_Username_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("existing_username_1", "P@ssw0rd");

            // act
            var response = await CreateUserAsync("existing_username_1", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_With_Invalid_Username_Format_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("invalid_username_1!", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_With_Null_Username_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync(null, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_With_Empty_Username_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_With_Username_Too_Short_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("a", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_With_Username_Too_Long_Returns_BadRequest()
        {
            // arrange
            string longUsername = new string('a', 129);

            // act
            var response = await CreateUserAsync(longUsername, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_With_Minimum_Username_Length_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("a", "P@ssw0rd");
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_With_Maximum_Username_Length_Returns_OK()
        {
            // arrange
            string maxUsername = new string('a', 128);

            // act
            var response = await CreateUserAsync(maxUsername, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_With_Null_Password_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username_2", null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_With_Empty_Password_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username_3", "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_With_Password_Too_Short_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username_4", "abc");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_User_With_Password_Too_Long_Returns_BadRequest()
        {
            // arrange
            string longPassword = new string('a', 33);

            // act
            var response = await CreateUserAsync("valid_username_5", longPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_User_With_Minimum_Password_Length_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("valid_username_6", "abcdef");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_User_With_Maximum_Password_Length_Returns_OK()
        {
            // arrange
            string maxPassword = new string('a', 32);

            // act
            var response = await CreateUserAsync("valid_username_7", maxPassword);
            
            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_User_With_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username_8", "abcdef");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_User_With_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username_9", "ABCDEF");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_User_With_Password_Missing_Digit_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username_10", "ABCDEFg");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_User_With_Password_Missing_Non_Alphanumeric_Character_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username_11", "Abcdef1");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Authenticate_User_With_Valid_Credentials_Returns_OK()
        {
            // arrange
            await CreateUserAsync("valid_username_12", "P@ssw0rd");

            // act
            var response = await CreateTokenAsync("valid_username_12", "P@ssw0rd");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body?["token"]?.AsValue()?.GetValue<string>();
            Assert.NotEmpty(token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Authenticate_User_With_Invalid_Password_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("valid_username_13", "P@ssw0rd");

            // act
            var response = await CreateTokenAsync("valid_username_13", "wrong_password");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Authenticate_User_With_Invalid_Username_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("valid_username_14", "P@ssw0rd");

            // act
            var response = await CreateTokenAsync("wrong_username", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
