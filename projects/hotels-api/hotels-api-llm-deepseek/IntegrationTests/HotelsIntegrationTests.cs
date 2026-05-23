using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using HotelListing.API.Data;

namespace IntegrationTests
{
    public class HotelsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HotelsTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> CreateAdminUserAndGetToken()
        {
            var email = $"{Guid.NewGuid()}@admin.com";
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
            var email = $"{Guid.NewGuid()}@user.com";
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

        private async Task<int> CreateCountryAsync(string token)
        {
            var countryData = new
            {
                name = $"Country_{Guid.NewGuid()}",
                shortName = $"C{Guid.NewGuid().ToString().Substring(0, 4)}"
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/countries")
            {
                Content = JsonContent.Create(countryData),
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token) }
            };

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        private async Task<HttpResponseMessage> CreateHotelAsync(string token, object hotelData)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(hotelData),
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token) }
            };

            return await _client.SendAsync(request);
        }

        [Fact]
        public async Task TC301_Create_Hotel_Valid_Returns_Created()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var countryId = await CreateCountryAsync(adminToken);

            var hotelData = new
            {
                name = "Valid Hotel_" + Guid.NewGuid(),
                address = "Address_" + Guid.NewGuid(),
                countryId
            };

            // Act
            var response = await CreateHotelAsync(adminToken, hotelData);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(body["id"].GetValue<int>() > 0);
            Assert.Equal(hotelData.name, body["name"].ToString());
            Assert.Equal(hotelData.address, body["address"].ToString());
            Assert.Equal(countryId, body["countryId"].GetValue<int>());
        }

        [Fact]
        public async Task TC302_Create_Hotel_Invalid_CountryID_Returns_BadRequest()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();

            var hotelData = new
            {
                name = "Invalid Country Hotel",
                address = "Address",
                countryId = -1
            };

            // Act
            var response = await CreateHotelAsync(adminToken, hotelData);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC303_Get_Hotel_Pagination_Valid_Returns_OK()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var countryId = await CreateCountryAsync(adminToken);

            await CreateHotelAsync(adminToken, new
            {
                name = "Hotel 1",
                address = "Address 1",
                countryId
            });

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels?pageNumber=1&pageSize=10")
            {
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken) }
            };
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(body["totalCount"].GetValue<int>() >= 1);
            Assert.Equal(1, body["pageNumber"].GetValue<int>());
            Assert.Equal(10, body["recordNumber"].GetValue<int>());
        }

        [Fact]
        public async Task TC304_Get_Hotel_Pagination_Invalid_Size_Returns_BadRequest()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var countryId = await CreateCountryAsync(adminToken);

            await CreateHotelAsync(adminToken, new
            {
                name = "Hotel 1",
                address = "Address 1",
                countryId
            });

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels?pageNumber=0&pageSize=0")
            {
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken) }
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC305_Update_Hotel_Valid_Returns_NoContent()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var countryId = await CreateCountryAsync(adminToken);

            var createResponse = await CreateHotelAsync(adminToken, new
            {
                name = "Original Hotel",
                address = "Original Address",
                countryId
            });
            var hotelId = (await createResponse.Content.ReadFromJsonAsync<JsonObject>())["id"].GetValue<int>();

            var updateData = new
            {
                name = "Updated Hotel",
                address = "New Address",
                countryId
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{hotelId}")
            {
                Content = JsonContent.Create(updateData),
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken) }
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC401_Update_NonExistent_Hotel_Returns_NotFound()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var invalidHotelId = 9999;

            var updateData = new
            {
                name = "Invalid Hotel",
                address = "Invalid Address",
                countryId = 1
            };

            // Act
            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{invalidHotelId}")
            {
                Content = JsonContent.Create(updateData),
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken) }
            };
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC501_Get_Hotels_Without_Auth_Returns_Unauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/hotels/all");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC602_Create_Hotel_CountryID_Max_Value_Returns_OK()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();

            var hotelData = new
            {
                name = "Max Country Hotel",
                address = "Address",
                countryId = int.MaxValue
            };

            // Act
            var response = await CreateHotelAsync(adminToken, hotelData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC702_Get_Hotel_By_ID_Returns_OK()
        {
            // Arrange
            var adminToken = await CreateAdminUserAndGetToken();
            var countryId = await CreateCountryAsync(adminToken);

            var createResponse = await CreateHotelAsync(adminToken, new
            {
                name = "Specific Hotel",
                address = "Specific Address",
                countryId
            });
            var hotelId = (await createResponse.Content.ReadFromJsonAsync<JsonObject>())["id"].GetValue<int>();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels/{hotelId}")
            {
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken) }
            };
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(hotelId, body["id"].GetValue<int>());
            Assert.Equal("Specific Hotel", body["name"].ToString());
        }
    }
}
