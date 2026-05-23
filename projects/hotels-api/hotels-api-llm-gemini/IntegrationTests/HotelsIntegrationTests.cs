using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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

        private async Task<int> CreateCountryAndGetIdAsync(string token, string name, string shortName)
        {
            var createCountryResponse = await CreateCountryAsync(token, name, shortName);
            Assert.Equal(HttpStatusCode.Created, createCountryResponse.StatusCode);
            var createCountryBody = await createCountryResponse.Content.ReadFromJsonAsync<JsonObject>();
            return createCountryBody["id"].GetValue<int>();
        }

        private async Task<HttpResponseMessage> GetAllHotelsAsync(string token)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/api/hotels/all");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> GetHotelsByPageAsync(string token, int pageNumber, int pageSize)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels?pageNumber={pageNumber}&pageSize={pageSize}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> CreateHotelAsync(string token, string name, string address, double? rating, int countryId)
        {
            var request = new { name = name, address = address, rating = rating, countryId = countryId };
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(request)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> GetHotelAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels/{id}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> UpdateHotelAsync(string token, int id, string name, string address, double? rating, int countryId)
        {
            var request = new { name = name, address = address, rating = rating, countryId = countryId };
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{id}")
            {
                Content = JsonContent.Create(request)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> DeleteHotelAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/hotels/{id}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(requestMessage);
        }

        [Fact]
        public async Task TC037_Get_All_Hotels_Returns_OK()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe37@example.com", "Password123!", false);

            // act
            var response = await GetAllHotelsAsync(token);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(content);
        }

        [Fact]
        public async Task TC038_Get_All_Hotels_Without_Authentication_Returns_Unauthorized()
        {
            // arrange

            // act
            var response = await GetAllHotelsAsync(null);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Get_Hotels_by_Page_Returns_OK()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe39@example.com", "Password123!", true);
            int pageNumber = 1;
            int pageSize = 10;

            // act
            var response = await GetHotelsByPageAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["totalCount"].GetValue<int>() >= 0);
            Assert.Equal(pageNumber, body["pageNumber"].GetValue<int>());
            Assert.True(body["recordNumber"].GetValue<int>() >= 0);
            Assert.True(body["items"].AsArray().Count >= 0);

        }

        [Fact]
        public async Task TC040_Get_Hotels_by_Invalid_Page_Number_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe40@example.com", "Password123!", true);
            int pageNumber = 0;
            int pageSize = 10;

            // act
            var response = await GetHotelsByPageAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Get_Hotels_by_Invalid_Page_Size_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe41@example.com", "Password123!", true);
            int pageNumber = 1;
            int pageSize = 0;

            // act
            var response = await GetHotelsByPageAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Get_Hotels_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            int pageNumber = 1;
            int pageSize = 10;

            // act
            var response = await GetHotelsByPageAsync(null, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Create_Hotel_With_Valid_Data_Returns_Created()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe43@example.com", "Password123!", false);
            string name = "Test Hotel 1";
            string address = "Test Address 1";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 1", "CFH1");


            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(name, body["name"].GetValue<string>());
            Assert.Equal(address, body["address"].GetValue<string>());
            Assert.Equal(rating, body["rating"].GetValue<double?>());
            Assert.Equal(countryId, body["countryId"].GetValue<int>());
            Assert.True(body["id"].GetValue<int>() > 0);

        }

        [Fact]
        public async Task TC044_Create_Hotel_With_Missing_Name_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe44@example.com", "Password123!", false);
            string name = "";
            string address = "Test Address 2";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 2", "CFH2");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Create_Hotel_With_Missing_Address_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe45@example.com", "Password123!", false);
            string name = "Test Hotel 3";
            string address = "";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 3", "CFH3");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Create_Hotel_With_Missing_CountryId_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe46@example.com", "Password123!", false);
            string name = "Test Hotel 4";
            string address = "Test Address 4";
            double? rating = 4.5;
            int countryId = 0;

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Create_Hotel_With_Invalid_CountryId_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe47@example.com", "Password123!", false);
            string name = "Test Hotel 5";
            string address = "Test Address 5";
            double? rating = 4.5;
            int countryId = -1;

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Create_Hotel_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            string name = "Test Hotel 6";
            string address = "Test Address 6";
            double? rating = 4.5;
            int countryId = 1;

            // act
            var response = await CreateHotelAsync(null, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Get_Hotel_by_ID_Returns_OK()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe49@example.com", "Password123!", false);
            string name = "Test Hotel 7";
            string address = "Test Address 7";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 7", "CFH7");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();

            // act
            var response = await GetHotelAsync(token, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(hotelId, body["id"].GetValue<int>());
            Assert.Equal(name, body["name"].GetValue<string>());
            Assert.Equal(address, body["address"].GetValue<string>());
            Assert.Equal(rating, body["rating"].GetValue<double?>());
            Assert.Equal(countryId, body["countryId"].GetValue<int>());
        }

        [Fact]
        public async Task TC050_Get_Hotel_by_Invalid_ID_Returns_NotFound()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe50@example.com", "Password123!", false);
            int hotelId = 999999;

            // act
            var response = await GetHotelAsync(token, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Get_Hotel_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            int hotelId = 1;

            // act
            var response = await GetHotelAsync(null, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC052_Update_Hotel_With_Valid_Data_Returns_NoContent()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe52@example.com", "Password123!", false);
            string name = "Test Hotel 8";
            string address = "Test Address 8";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 8", "CFH8");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();
            string updatedName = "Updated Hotel Name";
            string updatedAddress = "Updated Hotel Address";
            double? updatedRating = 4.8;
            int updatedCountryId = countryId;

            // act
            var response = await UpdateHotelAsync(token, hotelId, updatedName, updatedAddress, updatedRating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await GetHotelAsync(token, hotelId);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getBody = await getResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(updatedName, getBody["name"].GetValue<string>());
            Assert.Equal(updatedAddress, getBody["address"].GetValue<string>());
            Assert.Equal(updatedRating, getBody["rating"].GetValue<double?>());
            Assert.Equal(updatedCountryId, getBody["countryId"].GetValue<int>());
        }

        [Fact]
        public async Task TC053_Update_Hotel_With_Missing_Name_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe53@example.com", "Password123!", false);
            string name = "Test Hotel 9";
            string address = "Test Address 9";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 9", "CFH9");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();
            string updatedName = "";
            string updatedAddress = "Updated Hotel Address";
            double? updatedRating = 4.8;
            int updatedCountryId = countryId;


            // act
            var response = await UpdateHotelAsync(token, hotelId, updatedName, updatedAddress, updatedRating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC054_Update_Hotel_With_Missing_Address_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe54@example.com", "Password123!", false);
            string name = "Test Hotel 10";
            string address = "Test Address 10";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 10", "CFH10");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();
            string updatedName = "Updated Hotel Name";
            string updatedAddress = "";
            double? updatedRating = 4.8;
            int updatedCountryId = countryId;

            // act
            var response = await UpdateHotelAsync(token, hotelId, updatedName, updatedAddress, updatedRating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC055_Update_Hotel_With_Missing_CountryId_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe55@example.com", "Password123!", false);
            string name = "Test Hotel 11";
            string address = "Test Address 11";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 11", "CFH11");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();
            string updatedName = "Updated Hotel Name";
            string updatedAddress = "Updated Hotel Address";
            double? updatedRating = 4.8;
            int updatedCountryId = 0;

            // act
            var response = await UpdateHotelAsync(token, hotelId, updatedName, updatedAddress, updatedRating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC056_Update_Hotel_With_Invalid_CountryId_Returns_BadRequest()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe56@example.com", "Password123!", false);
            string name = "Test Hotel 12";
            string address = "Test Address 12";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 12", "CFH12");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();
            string updatedName = "Updated Hotel Name";
            string updatedAddress = "Updated Hotel Address";
            double? updatedRating = 4.8;
            int updatedCountryId = -1;

            // act
            var response = await UpdateHotelAsync(token, hotelId, updatedName, updatedAddress, updatedRating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC057_Update_Hotel_With_Invalid_ID_Returns_NotFound()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe57@example.com", "Password123!", false);
            int hotelId = 999999;
            string updatedName = "Updated Hotel Name";
            string updatedAddress = "Updated Hotel Address";
            double? updatedRating = 4.8;
            int updatedCountryId = 1;

            // act
            var response = await UpdateHotelAsync(token, hotelId, updatedName, updatedAddress, updatedRating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC058_Update_Hotel_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            int hotelId = 1;
            string updatedName = "Updated Hotel Name";
            string updatedAddress = "Updated Hotel Address";
            double? updatedRating = 4.8;
            int updatedCountryId = 1;

            // act
            var response = await UpdateHotelAsync(null, hotelId, updatedName, updatedAddress, updatedRating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC059_Delete_Hotel_by_ID_Returns_NoContent()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe59@example.com", "Password123!", true);
            string name = "Test Hotel 13";
            string address = "Test Address 13";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 13", "CFH13");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();

            // act
            var response = await DeleteHotelAsync(token, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            var getResponse = await GetHotelAsync(token, hotelId);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task TC060_Delete_Hotel_by_Invalid_ID_Returns_NotFound()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe60@example.com", "Password123!", true);
            int hotelId = 999999;

            // act
            var response = await DeleteHotelAsync(token, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC061_Delete_Hotel_Without_Authentication_Returns_Unauthorized()
        {
            // arrange
            int hotelId = 1;

            // act
            var response = await DeleteHotelAsync(null, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC062_Delete_Hotel_Without_Admin_Rights_Returns_Forbidden()
        {
            // arrange
            string token = await CreateUserAndGetTokenAsync("John", "Doe", "john.doe62@example.com", "Password123!", false);
            string name = "Test Hotel 14";
            string address = "Test Address 14";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Country for Hotel 14", "CFH14");
            var createHotelResponse = await CreateHotelAsync(token, name, address, rating, countryId);
            var createHotelBody = await createHotelResponse.Content.ReadFromJsonAsync<JsonObject>();
            int hotelId = createHotelBody["id"].GetValue<int>();

            // act
            var response = await DeleteHotelAsync(token, hotelId);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
