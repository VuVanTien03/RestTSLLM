using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class CountriesTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private int _countryCounter = 1;

        public CountriesTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private string GenerateUniqueName() => $"Country_{Guid.NewGuid()}";
        private string GenerateUniqueShortName() => $"C{Guid.NewGuid().ToString().Substring(0, 4)}";

        private async Task<HttpResponseMessage> CreateCountryAsync(string token, object countryData)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(countryData)
            };

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<string> CreateAdminUserAndGetToken()
        {
            var email = GenerateUniqueEmail();
            var password = "Adm1nP@ss";

            await _client.PostAsJsonAsync("/api/accounts", new
            {
                firstName = "Admin",
                lastName = "User",
                email,
                password,
                isAdmin = true
            });

            var loginResponse = await _client.PostAsJsonAsync("/api/accounts/tokens", new
            {
                email,
                password
            });

            var body = await loginResponse.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].ToString();
        }

        private async Task<string> CreateRegularUserAndGetToken()
        {
            var email = GenerateUniqueEmail();
            var password = "Us3rP@ss";

            await _client.PostAsJsonAsync("/api/accounts", new
            {
                firstName = "Regular",
                lastName = "User",
                email,
                password,
                isAdmin = false
            });

            var loginResponse = await _client.PostAsJsonAsync("/api/accounts/tokens", new
            {
                email,
                password
            });

            var body = await loginResponse.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].ToString();
        }

        private string GenerateUniqueEmail() => $"{Guid.NewGuid()}@test.com";

        [Fact]
        public async Task TC201_Create_Country_Valid_Returns_Created()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var countryData = new
            {
                name = GenerateUniqueName(),
                shortName = GenerateUniqueShortName()
            };

            // Act
            var response = await CreateCountryAsync(adminToken, countryData);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(body["id"].GetValue<int>() > 0);
            Assert.Equal(countryData.name, body["name"].ToString());
            Assert.Equal(countryData.shortName, body["shortName"].ToString());
        }

        [Fact]
        public async Task TC202_Create_Country_Without_Auth_Returns_Unauthorized()
        {
            // Arrange
            var countryData = new
            {
                name = GenerateUniqueName(),
                shortName = GenerateUniqueShortName()
            };

            // Act
            var response = await CreateCountryAsync(null, countryData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC203_Create_Country_Missing_Name_Returns_BadRequest()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var countryData = new
            {
                shortName = GenerateUniqueShortName()
            };

            // Act
            var response = await CreateCountryAsync(adminToken, countryData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC204_Get_Country_By_Valid_ID_Returns_OK()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();

            // Create country first
            var countryData = new
            {
                name = GenerateUniqueName(),
                shortName = GenerateUniqueShortName()
            };
            var createResponse = await CreateCountryAsync(adminToken, countryData);
            var createdCountry = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var countryId = createdCountry["id"].GetValue<int>();

            // Act
            var response = await _client.GetAsync($"/api/countries/{countryId}");

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(countryId, body["id"].GetValue<int>());
            Assert.Equal(countryData.name, body["name"].ToString());
            Assert.Equal(countryData.shortName, body["shortName"].ToString());
        }

        [Fact]
        public async Task TC205_Delete_Country_NonAdmin_Returns_Forbidden()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var userToken = await CreateRegularUserAndGetToken();

            // Create country first
            var countryData = new
            {
                name = GenerateUniqueName(),
                shortName = GenerateUniqueShortName()
            };
            var createResponse = await CreateCountryAsync(adminToken, countryData);
            var createdCountry = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var countryId = createdCountry["id"].GetValue<int>();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{countryId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userToken);
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TC402_Delete_Country_Not_Exist_Returns_NotFound()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var invalidCountryId = 9999;

            // Act
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/countries/{invalidCountryId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC701_Get_All_Countries_Returns_Array()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();

            // Create sample country
            await CreateCountryAsync(adminToken, new
            {
                name = GenerateUniqueName(),
                shortName = GenerateUniqueShortName()
            });

            // Act
            var response = await _client.GetAsync("/api/countries");

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body.Count > 0);
        }
    }
}
