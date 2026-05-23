using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Diagnostics;

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
            var userRequest = new
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                password = password,
                isAdmin = isAdmin
            };
            await _client.PostAsJsonAsync("/api/accounts", userRequest);
            var loginRequest = new
            {
                email = email,
                password = password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/accounts/tokens", loginRequest);
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonObject>();
            return loginBody["token"].AsValue().GetValue<string>();
        }

        private async Task<int> CreateCountryAndGetIdAsync(string token, string name, string shortName)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(new { name = name, shortName = shortName })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC020_Get_All_Countries_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John20", "Doe20", "john20@test.com", "Test@20", false);
            await CreateCountryAndGetIdAsync(token, "Country20", "C20");

            // act
            var response = await _client.GetAsync("/api/countries");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var countries = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotEmpty(countries);
        }

        [Fact]
        public async Task TC021_Create_Country_When_Valid_Data_Returns_Created()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John21", "Doe21", "john21@test.com", "Test@123", false);
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(new
                {
                    name = "Country21",
                    shortName = "C21"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var country = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal("Country21", country["name"].AsValue().GetValue<string>());
            Assert.Equal("C21", country["shortName"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC022_Create_Country_When_Empty_Name_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John22", "Doe22", "john22@test.com", "Test@22", false);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(new
                {
                    name = "",
                    shortName = "C22"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Create_Country_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(new
                {
                    name = "Country23",
                    shortName = "C23"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Get_Country_By_Id_When_Valid_Id_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John24", "Doe24", "john24@test.com", "Test@24", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country24", "C24");

            // act
            var response = await _client.GetAsync($"/api/countries/{countryId}");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var country = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(countryId, country["id"].AsValue().GetValue<int>());
            Assert.Equal("Country24", country["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC025_Get_Country_By_Id_When_Invalid_Id_Returns_NotFound()
        {
            // act
            var response = await _client.GetAsync("/api/countries/99999");

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Update_Country_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John26", "Doe26", "john26@test.com", "Test@26", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country26", "C26");

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/countries/{countryId}")
            {
                Content = JsonContent.Create(new
                {
                    name = "Country26 Updated",
                    shortName = "C26U"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Update_Country_When_Empty_Name_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John27", "Doe27", "john27@test.com", "Test@27", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country27", "C27");

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/countries/{countryId}")
            {
                Content = JsonContent.Create(new
                {
                    name = "",
                    shortName = "C27"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Update_Country_When_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John28", "Doe28", "john28@test.com", "Test@28", false);

            var request = new HttpRequestMessage(HttpMethod.Put, "/api/countries/99999")
            {
                Content = JsonContent.Create(new
                {
                    name = "Country28",
                    shortName = "C28"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Update_Country_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John29", "Doe29", "john29@test.com", "Test@29", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country29", "C29");

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/countries/{countryId}")
            {
                Content = JsonContent.Create(new
                {
                    name = "Country29 Updated",
                    shortName = "C29U"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Delete_Country_When_Valid_Id_And_Admin_Returns_NoContent()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John30", "Doe30", "john30@test.com", "Test@30", true);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country30", "C30");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{countryId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Delete_Country_When_Valid_Id_And_Not_Admin_Returns_Forbidden()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John31", "Doe31", "john31@test.com", "Test@31", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country31", "C31");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{countryId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Delete_Country_When_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John32", "Doe32", "john32@test.com", "Test@32", true);

            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/countries/99999");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Delete_Country_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John33", "Doe33", "john33@test.com", "Test@33", true);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country33", "C33");

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{countryId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}