// File: UsersIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
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

        [Fact]
        public async Task TC001_Create_User_When_Valid_Data_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("valid_username1", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_When_Username_Already_Exists_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("existing_username", "P@ssw0rd");

            // act
            var response = await CreateUserAsync("existing_username", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_When_Username_Is_Null_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync(null, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_When_Username_Is_Empty_String_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("", "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_When_Username_Too_Short_Returns_BadRequest()
        {
            // arrange
            var username = "";

            // act
            var response = await CreateUserAsync(username, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_When_Username_Too_Long_Returns_BadRequest()
        {
            // arrange
            var username = new string('a', 129);

            // act
            var response = await CreateUserAsync(username, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_When_Username_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            var username = "a";

            // act
            var response = await CreateUserAsync(username, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_When_Username_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var username = new string('a', 128);

            // act
            var response = await CreateUserAsync(username, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_When_Username_Contains_Invalid_Characters_Returns_BadRequest()
        {
            // arrange
            var username = "invalid*username";

            // act
            var response = await CreateUserAsync(username, "P@ssw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_When_Password_Is_Null_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username", null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_When_Password_Is_Empty_String_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username", "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_When_Password_Too_Short_Returns_BadRequest()
        {
            // arrange
            var password = "Pass";

            // act
            var response = await CreateUserAsync("valid_username2", password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_User_When_Password_Too_Long_Returns_BadRequest()
        {
            // arrange
            var password = "P@ssw0rd123456789012345678901234567890";

            // act
            var response = await CreateUserAsync("valid_username3", password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_User_When_Password_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            var password = "P@ssw0";

            // act
            var response = await CreateUserAsync("valid_username4", password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_User_When_Password_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var password = "P@ssw0rd123456789012345678901234";

            // act
            var response = await CreateUserAsync("valid_username5", password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_User_When_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username6", "passw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_User_When_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username7", "PASSW0RD");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_User_When_Password_Missing_Digit_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username8", "Passw0rd");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_User_When_Password_Missing_Special_Character_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username9", "Password1");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}