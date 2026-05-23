using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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

        private async Task<string> CreateUserAndGetTokenAsync(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            var createAccountResponse = await CreateAccountAsync(firstName, lastName, email, password, isAdmin);
            Assert.Equal(HttpStatusCode.OK, createAccountResponse.StatusCode);

            var loginRequest = new { email = email, password = password };
            var loginResponse = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var body = await loginResponse.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].GetValue<string>();
        }

        private async Task<HttpResponseMessage> CreateAccountAsync(string firstName, string lastName, string email, string password, bool isAdmin)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password,
                isAdmin = isAdmin
            };
            return await _client.PostAsJsonAsync("/api/accounts", request);
        }

        private async Task<HttpResponseMessage> CreateCountryAsync(string token, string name, string shortName)
        {
            var request = new { name = name, shortName = shortName };
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(request)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> GetCountryAsync(int id)
        {
            return await _client.GetAsync($"/api/countries/{id}");
        }

        private async Task<HttpResponseMessage> UpdateCountryAsync(string token, int id, string name, string shortName)
        {
            var request = new { name = name, shortName = shortName };
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/countries/{id}")
            {
                Content = JsonContent.Create(request)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> DeleteCountryAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{id}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }


        [Fact]
        public async Task TC021_Get_All_Countries_Returns_OK()
        {
            // arrange

            // act
            var response = await _client.GetAsync("/api/countries");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);

        }

        [Fact]
        public async Task TC022_Create_Country_With_Valid_Data_Returns_Created()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe22@example.com", "Password123!", false);
            string name = "Test Country 1";
            string shortName = "TC1";

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(name, body["name"].GetValue<string>());
            Assert.Equal(shortName, body["shortName"].GetValue<string>());
            Assert.True(body["id"].GetValue<int>() > 0);
        }

        [Fact]
        public async Task TC023_Create_Country_With_Missing_Name_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe23@example.com", "Password123!", false);
            string name = "";
            string shortName = "TC2";

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task TC024_Create_Country_With_Duplicate_Name_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe24@example.com", "Password123!", false);
            string name = "Test Country 3";
            string shortName = "TC3";
            await CreateCountryAsync(token, name, shortName);

            // act
            var response = await CreateCountryAsync(token, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Country_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            string name = "Test Country 4";
            string shortName = "TC4";

            // act
            var response = await CreateCountryAsync(null, name, shortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Get_Country_by_ID_Returns_OK()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe26@example.com", "Password123!", false);
            string name = "Test Country 5";
            string shortName = "TC5";
            var createResponse = await CreateCountryAsync(token, name, shortName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody["id"].GetValue<int>();

            // act
            var response = await GetCountryAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(id, body["id"].GetValue<int>());
            Assert.Equal(name, body["name"].GetValue<string>());
            Assert.Equal(shortName, body["shortName"].GetValue<string>());

        }

        [Fact]
        public async Task TC027_Get_Country_by_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int id = 999999;

            // act
            var response = await GetCountryAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Update_Country_With_Valid_Data_Returns_NoContent()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe28@example.com", "Password123!", false);
            string name = "Test Country 6";
            string shortName = "TC6";
            var createResponse = await CreateCountryAsync(token, name, shortName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody["id"].GetValue<int>();
            string updatedName = "Updated Country Name";
            string updatedShortName = "UCN";

            // act
            var response = await UpdateCountryAsync(token, id, updatedName, updatedShortName);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await GetCountryAsync(id);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getBody = await getResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(updatedName, getBody["name"].GetValue<string>());
            Assert.Equal(updatedShortName, getBody["shortName"].GetValue<string>());
        }

        [Fact]
        public async Task TC029_Update_Country_With_Missing_Name_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe29@example.com", "Password123!", false);
            string name = "Test Country 7";
            string shortName = "TC7";
            var createResponse = await CreateCountryAsync(token, name, shortName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody["id"].GetValue<int>();
            string updatedName = "";
            string updatedShortName = "UCN";


            // act
            var response = await UpdateCountryAsync(token, id, updatedName, updatedShortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Update_Country_With_Duplicate_Name_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe30@example.com", "Password123!", false);
            string name = "Test Country 8";
            string shortName = "TC8";
            var createResponse = await CreateCountryAsync(token, name, shortName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody["id"].GetValue<int>();
            string updatedName = "Test Country 1";
            string updatedShortName = "UCN";
            await CreateCountryAsync(token, updatedName, "TC1");


            // act
            var response = await UpdateCountryAsync(token, id, updatedName, updatedShortName);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Update_Country_With_Invalid_ID_Returns_NotFound()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe31@example.com", "Password123!", false);
            int id = 999999;
            string updatedName = "Updated Country Name";
            string updatedShortName = "UCN";

            // act
            var response = await UpdateCountryAsync(token, id, updatedName, updatedShortName);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Update_Country_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string updatedName = "Updated Country Name";
            string updatedShortName = "UCN";

            // act
            var response = await UpdateCountryAsync(null, id, updatedName, updatedShortName);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Delete_Country_by_ID_Returns_NoContent()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe33@example.com", "Password123!", true);
            string name = "Test Country 9";
            string shortName = "TC9";
            var createResponse = await CreateCountryAsync(token, name, shortName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody["id"].GetValue<int>();

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await GetCountryAsync(id);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        }

        [Fact]
        public async Task TC034_Delete_Country_by_Invalid_ID_Returns_NotFound()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe34@example.com", "Password123!", true);
            int id = 999999;

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Delete_Country_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            int id = 1;

            // act
            var response = await DeleteCountryAsync(null, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Delete_Country_Without_Admin_Rights_Returns_Forbidden()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe36@example.com", "Password123!", false);
            string name = "Test Country 10";
            string shortName = "TC10";
            var createResponse = await CreateCountryAsync(token, name, shortName);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody["id"].GetValue<int>();

            // act
            var response = await DeleteCountryAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
