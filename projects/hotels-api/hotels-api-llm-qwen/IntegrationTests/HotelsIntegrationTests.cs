// File: HotelsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace IntegrationTests
{
    public class HotelsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public HotelsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateAccountAsync(string firstName, string lastName, bool isAdmin, string email, string password)
        {
            var request = new
            {
                firstName = firstName,
                lastName = lastName,
                isAdmin = isAdmin,
                email = email,
                password = password
            };

            return await _client.PostAsJsonAsync("/api/accounts", request);
        }

        private async Task<HttpResponseMessage> LoginAsync(string email, string password)
        {
            var request = new
            {
                email = email,
                password = password
            };

            return await _client.PostAsJsonAsync("/api/accounts/tokens", request);
        }

        private async Task<string> GetTokenForUserAsync(string firstName, string lastName, bool isAdmin, string email, string password)
        {
            await CreateAccountAsync(firstName, lastName, isAdmin, email, password);
            var response = await LoginAsync(email, password);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].AsValue().GetValue<string>();
        }

        private async Task<HttpResponseMessage> GetHotelsAllAsync(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.GetAsync("/api/hotels/all");
        }

        private async Task<HttpResponseMessage> GetHotelsPagedAsync(string token, int pageNumber, int pageSize)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.GetAsync($"/api/hotels?pageNumber={pageNumber}&pageSize={pageSize}");
        }

        private async Task<HttpResponseMessage> CreateHotelAsync(string token, string name, string address, int countryId, double? rating)
        {
            var request = new
            {
                name = name,
                address = address,
                countryId = countryId,
                rating = rating
            };

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.PostAsJsonAsync("/api/hotels", request);
        }

        private async Task<HttpResponseMessage> GetHotelByIdAsync(string token, int id)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.GetAsync($"/api/hotels/{id}");
        }

        private async Task<HttpResponseMessage> UpdateHotelAsync(string token, int id, string name, string address, int countryId, double? rating)
        {
            var request = new
            {
                name = name,
                address = address,
                countryId = countryId,
                rating = rating
            };

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.PutAsJsonAsync($"/api/hotels/{id}", request);
        }

        private async Task<HttpResponseMessage> DeleteHotelAsync(string token, int id)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.DeleteAsync($"/api/hotels/{id}");
        }

        private async Task<HttpResponseMessage> CreateCountryAsync(string token, string name, string shortName)
        {
            var request = new
            {
                name = name,
                shortName = shortName
            };

            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.PostAsJsonAsync("/api/countries", request);
        }

        [Fact]
        public async Task TC021_Get_Hotels_When_No_Authorization_Returns_Unauthorized()
        {
            // arrange
            // no token provided

            // act
            var response = await GetHotelsPagedAsync(null, 1, 10);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Get_Hotels_When_Valid_Authorization_Returns_OK()
        {
            // arrange
            var user = "user22";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await GetHotelsPagedAsync(token, 1, 10);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Get_Hotels_When_Invalid_Query_Parameters_Returns_BadRequest()
        {
            // arrange
            var user = "user23";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            // act
            var response = await GetHotelsPagedAsync(token, 0, -1);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Hotel_When_Valid_Data_Returns_Created()
        {
            // arrange
            var user = "user24";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            await CreateCountryForHotel(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, "Hotel Example", "Rua dos Exemplos, 123", 1, 4.5);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_address = body["address"].AsValue().GetValue<string>();
            var body_countryId = body["countryId"].AsValue().GetValue<int>();
            var body_rating = body["rating"].AsValue().GetValue<double>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal("Hotel Example", body_name);
            Assert.Equal("Rua dos Exemplos, 123", body_address);
            Assert.Equal(1, body_countryId);
            Assert.Equal(4.5, body_rating);
            Assert.True(body_id > 0);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private async Task<int> CreateCountryForHotel(string token, string name, string shortName)
        {
            var createCountryResponse = await CreateCountryAsync(token, name, shortName);
            var createCountryBody = await createCountryResponse.Content.ReadFromJsonAsync<JsonObject>();
            var countryId = createCountryBody["id"].AsValue().GetValue<int>();
            return countryId;
        }

        [Fact]
        public async Task TC025_Create_Hotel_When_Invalid_Name_Returns_BadRequest()
        {
            // arrange
            var user = "user25";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            await CreateCountryForHotel(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, "", "Rua dos Exemplos, 123", 1, 4.5);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Get_Hotel_When_Valid_ID_Returns_OK()
        {
            // arrange
            var user = "user26";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            var countryId = await CreateCountryForHotel(token, "Brazil", "BR");
            var createHotelResponse = await CreateHotelAsync(token, "Hotel Example", "Rua dos Exemplos, 123", countryId, 4.5);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            var hotelId = createHotelBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetHotelByIdAsync(token, hotelId);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal(hotelId, body_id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Get_Hotel_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var user = "user27";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            await CreateCountryForHotel(token, "Brazil", "BR");

            // act
            var response = await GetHotelByIdAsync(token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Update_Hotel_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var user = "user28";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            var countryId = await CreateCountryForHotel(token, "Brazil", "BR");
            var createHotelResponse = await CreateHotelAsync(token, "Hotel Example", "Rua dos Exemplos, 123", countryId, 4.5);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            var hotelId = createHotelBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateHotelAsync(token, hotelId, "Hotel Example (Updated)", "Rua dos Exemplos, 1234", countryId, 4.7);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Update_Hotel_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var user = "user29";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: false,
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            await CreateCountryForHotel(token, "Brazil", "BR");

            // act
            var response = await UpdateHotelAsync(token, 9999999, "Hotel Example (Updated)", "Rua dos Exemplos, 1234", 1, 4.7);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Delete_Hotel_When_Valid_ID_Returns_NoContent()
        {
            // arrange
            var user = "user30";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: true, // Assuming admin can delete
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            var countryId = await CreateCountryForHotel(token, "Brazil", "BR");
            var createHotelResponse = await CreateHotelAsync(token, "Hotel Example", "Rua dos Exemplos, 123", countryId, 4.5);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            var hotelId = createHotelBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteHotelAsync(token, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Delete_Hotel_When_Invalid_ID_Returns_NotFound()
        {
            // arrange
            var user = "user31";
            var token = await GetTokenForUserAsync(
                firstName: $"{user}John",
                lastName: $"{user}Doe",
                isAdmin: true, // Assuming admin can delete
                email: $"{user}john.doe@example.com",
                password: "Pass123!");

            await CreateCountryForHotel(token, "Brazil", "BR");

            // act
            var response = await DeleteHotelAsync(token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
