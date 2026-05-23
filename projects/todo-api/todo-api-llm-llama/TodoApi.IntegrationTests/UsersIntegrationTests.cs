// File: UsersIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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
        public async Task TC001_Create_User_When_Valid_Data_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("valid_username", "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_When_Username_Already_Exists_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("existing_username", "P@ssw0rd");

            // act
            var response = await CreateUserAsync("existing_username", "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_When_Invalid_Username_Format_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("invalid!Username", "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_When_Username_Is_Null_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync(null, "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_When_Username_Is_Empty_String_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("", "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_When_Username_Too_Short_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("a", "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_When_Username_Too_Long_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync(new string('a', 129), "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_When_Username_Has_Minimumm_Size_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("a", "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_When_Username_Has_Maximum_Size_Returns_OK()
        {
            // act
            var response = await CreateUserAsync(new string('a', 128), "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_When_Password_Is_Null_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username2", null);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_When_Password_Is_Empty_String_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username2", "");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_When_Password_Too_Short_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username3", "P@ss");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_User_When_Password_Too_Long_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username3", new string('a', 33));

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_User_When_Password_Has_Minimum_Size_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("valid_username4", "P@ssw");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_User_When_Password_Has_Maximum_Size_Returns_OK()
        {
            // act
            var response = await CreateUserAsync("valid_username5", new string('a', 32));

            // assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_User_When_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username6", "p@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_User_When_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username7", "P@SSW0RD");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_User_When_Password_Missing_Digit_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username8", "P@ssword");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_User_When_Password_Missing_Non_Alphanumeric_Character_Returns_BadRequest()
        {
            // act
            var response = await CreateUserAsync("valid_username9", "Password123");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Authenticate_User_When_Valid_Data_Returns_OK()
        {
            // arrange
            await CreateUserAsync("valid_username10", "P@ssw0rd");

            // act
            var response = await AuthenticateUserAsync("valid_username10", "P@ssw0rd");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            Assert.True(!string.IsNullOrEmpty(body_token));
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Authenticate_User_When_Invalid_Password_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("valid_username11", "P@ssw0rd");

            // act
            var response = await AuthenticateUserAsync("valid_username11", "invalid_password");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Authenticate_User_When_Invalid_Username_Returns_BadRequest()
        {
            // arrange
            await CreateUserAsync("valid_username12", "P@ssw0rd");

            // act
            var response = await AuthenticateUserAsync("invalid_username", "P@ssw0rd");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
