// File: CountriesIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

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

        private async Task<HttpResponseMessage> CreateCountryAsync(string token, string name, string shortName)
        {
            var requestBody = new
            {
                name = name,
                shortName = shortName
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(requestBody)
            };

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<int> CreateCountryAndGetIdAsync(string token, string name, string shortName)
        {
            var response = await CreateCountryAsync(token, name, shortName);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> UpdateCountryAsync(string token, int id, string name, string shortName)
        {
            var requestBody = new
            {
                name = name,
                shortName = shortName
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/countries/{id}")
            {
                Content = JsonContent.Create(requestBody)
            };

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> DeleteCountryAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{id}");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> GetCountryAsync(int id)
        {
            return await _client.GetAsync($"/api/countries/{id}");
        }

        private async Task<HttpResponseMessage> GetCountriesAsync()
        {
            return await _client.GetAsync("/api/countries");
        }

        private async Task<string> CreateUserAndGetTokenAsync(string firstName, string lastName, bool isAdmin, string email, string password)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                isAdmin = isAdmin,
                email = email,
                password = password
            };
            await _client.PostAsJsonAsync("/api/accounts", request);
            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", new { email, password });

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].AsValue().GetValue<string>();
        }

        [Fact]
        public async Task TC033_Get_Countries_Returns_OK()
        {
            // act
            var response = await GetCountriesAsync();

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.True(body.Count > 0);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Create_Country_When_Valid_Data_Returns_Created()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Brazil";
            string shortName = "BR";

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_shortName = body["shortName"].AsValue().GetValue<string>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal(name, body_name);
            Assert.Equal(shortName, body_shortName);
            Assert.True(body_id > 0);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Create_Country_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user1@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = null;
            string shortName = "BR";

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Create_Country_When_Name_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user2@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "";
            string shortName = "BR";

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Create_Country_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string name = "Brazil";
            string shortName = "BR";
            string invalid_token = "invalidtoken";

            // act
            var response = await CreateCountryAsync(invalid_token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Create_Country_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            string name = "Brazil";
            string shortName = "BR";
            string null_token = null;

            // act
            var response = await CreateCountryAsync(null_token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Get_Country_By_Id_When_Valid_Id_Returns_OK()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user3@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Brazil";
            string shortName = "BR";
            int id = await CreateCountryAndGetIdAsync(token, name, shortName);

            // act
            var response = await GetCountryAsync(id);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_shortName = body["shortName"].AsValue().GetValue<string>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal(name, body_name);
            Assert.Equal(shortName, body_shortName);
            Assert.Equal(id, body_id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Get_Country_By_Id_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            int invalid_id = 9999999;

            // act
            var response = await GetCountryAsync(invalid_id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_Country_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user4@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Brazil";
            string shortName = "BR";
            int id = await CreateCountryAndGetIdAsync(token, name, shortName);
            string updatedName = "Updated Brazil";

            // act
            var response = await UpdateCountryAsync(token, id, updatedName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Country_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user5@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Brazil";
            string shortName = "BR";
            int id = await CreateCountryAndGetIdAsync(token, name, shortName);
            string updatedName = null;

            // act
            var response = await UpdateCountryAsync(token, id, updatedName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Country_When_Name_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user6@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Brazil";
            string shortName = "BR";
            int id = await CreateCountryAndGetIdAsync(token, name, shortName);
            string updatedName = "";

            // act
            var response = await UpdateCountryAsync(token, id, updatedName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Update_Country_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user7@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int invalid_id = 9999999;
            string updatedName = "Updated Brazil";
            string shortName = "BR";

            // act
            var response = await UpdateCountryAsync(token, invalid_id, updatedName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Update_Country_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string invalid_token = "invalidtoken";
            string updatedName = "Updated Brazil";
            string shortName = "BR";

            // act
            var response = await UpdateCountryAsync(invalid_token, id, updatedName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Update_Country_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string null_token = null;
            string updatedName = "Updated Brazil";
            string shortName = "BR";

            // act
            var response = await UpdateCountryAsync(null_token, id, updatedName, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Delete_Country_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user8@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Brazil";
            string shortName = "BR";
            int id = await CreateCountryAndGetIdAsync(token, name, shortName);

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Delete_Country_When_User_Is_Not_Admin_Returns_Forbidden()
        {
            // arrange
            string firstName = "User";
            string lastName = "User";
            bool isAdmin = false;
            string email = "user.user@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int id = 1;

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Delete_Country_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user9@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int invalid_id = 9999999;

            // act
            var response = await DeleteCountryAsync(token, invalid_id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC050_Delete_Country_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string invalid_token = "invalidtoken";

            // act
            var response = await DeleteCountryAsync(invalid_token, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Delete_Country_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string null_token = null;

            // act
            var response = await DeleteCountryAsync(null_token, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
