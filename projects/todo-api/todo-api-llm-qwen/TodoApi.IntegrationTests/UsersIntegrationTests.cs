// File: UsersIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
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
            // arrange
            string username = "validUsername100";
            string password = "P@ssw0rd100";

            // act
            var response = await CreateUserAsync(username, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_When_Username_Already_Exists_Returns_BadRequest()
        {
            // arrange
            string existingUsername = "existingUsername";
            string password = "P@ssw0rd101";
            await CreateUserAsync(existingUsername, password); // precondition 1

            // act
            var response = await CreateUserAsync(existingUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_When_Invalid_Username_Format_Returns_BadRequest()
        {
            // arrange
            string invalidUsername = "invalid#username";
            string password = "P@ssw0rd101";

            // act
            var response = await CreateUserAsync(invalidUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_When_Username_Is_Null_Returns_BadRequest()
        {
            // arrange
            string nullUsername = null;
            string password = "P@ssw0rd102";

            // act
            var response = await CreateUserAsync(nullUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_When_Username_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string emptyUsername = "";
            string password = "P@ssw0rd103";

            // act
            var response = await CreateUserAsync(emptyUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_When_Username_Too_Short_Returns_BadRequest()
        {
            // arrange
            string shortUsername = "";
            string password = "P@ssw0rd104";

            // act
            var response = await CreateUserAsync(shortUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_When_Username_Too_Long_Returns_BadRequest()
        {
            // arrange
            string longUsername = "validusernameabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcab";
            string password = "P@ssw0rd105";

            // act
            var response = await CreateUserAsync(longUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_When_Username_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            string minSizeUsername = "a";
            string password = "P@ssw0rd106";

            // act
            var response = await CreateUserAsync(minSizeUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_When_Username_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            string maxSizeUsername = "validusernameabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabcabc";
            string password = "P@ssw0rd107";

            // act
            var response = await CreateUserAsync(maxSizeUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_When_Password_Is_Null_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername108";
            string nullPassword = null;

            // act
            var response = await CreateUserAsync(username, nullPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_When_Password_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername109";
            string emptyPassword = "";

            // act
            var response = await CreateUserAsync(username, emptyPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_When_Password_Too_Short_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername110";
            string shortPassword = "Abcde";

            // act
            var response = await CreateUserAsync(username, shortPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_User_When_Password_Too_Long_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername111";
            string longPassword = "Abcdefghijklmnopqrstuvwxyz1234";

            // act
            var response = await CreateUserAsync(username, longPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_User_When_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername112";
            string noUppercasePassword = "abcdef12!@#";

            // act
            var response = await CreateUserAsync(username, noUppercasePassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_User_When_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername113";
            string noLowercasePassword = "ABCDEFG12!@#";

            // act
            var response = await CreateUserAsync(username, noLowercasePassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_User_When_Password_Missing_Digit_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername114";
            string noDigitPassword = "Abcdefghijklmnop!@#";

            // act
            var response = await CreateUserAsync(username, noDigitPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_User_When_Password_Missing_Non_Alphanumeric_Character_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername115";
            string noNonAlphanumericPassword = "Abcdefgh123456";

            // act
            var response = await CreateUserAsync(username, noNonAlphanumericPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_User_When_Password_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            string username = "validUsername116";
            string minSizePassword = "Abc1!@";

            // act
            var response = await CreateUserAsync(username, minSizePassword);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_User_When_Password_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            string username = "validUsername117";
            string maxSizePassword = "Abcdefghijklmnop123456!@#$";

            // act
            var response = await CreateUserAsync(username, maxSizePassword);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Authenticate_User_When_Valid_Data_Returns_OK()
        {
            // arrange
            string username = "validUsername118";
            string password = "P@ssw0rd118";
            await CreateUserAsync(username, password); // precondition 1

            // act
            var response = await CreateTokenAsync(username, password);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            Assert.NotNull(body_token);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Authenticate_User_When_Invalid_Password_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername119";
            string password = "P@ssw0rd119";
            string wrongPassword = "WrongPass1";
            await CreateUserAsync(username, password); // precondition 1

            // act
            var response = await CreateTokenAsync(username, wrongPassword);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Authenticate_User_When_Invalid_Username_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername120";
            string password = "P@ssw0rd120";
            string invalidUsername = "invalidUsername";
            await CreateUserAsync(username, password); // precondition 1

            // act
            var response = await CreateTokenAsync(invalidUsername, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}