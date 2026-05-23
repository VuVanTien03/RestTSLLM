// File: HotelsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

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

        private async Task<HttpResponseMessage> CreateHotelAsync(string token, string name, string address, double? rating, int countryId)
        {
            var requestBody = new
            {
                name = name,
                address = address,
                rating = rating,
                countryId = countryId
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(requestBody)
            };

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<int> CreateHotelAndGetIdAsync(string token, string name, string address, double? rating, int countryId)
        {
            var response = await CreateHotelAsync(token, name, address, rating, countryId);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> UpdateHotelAsync(string token, int id, string name, string address, double? rating, int countryId)
        {
            var requestBody = new
            {
                name = name,
                address = address,
                rating = rating,
                countryId = countryId
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{id}")
            {
                Content = JsonContent.Create(requestBody)
            };

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> DeleteHotelAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/hotels/{id}");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> GetHotelAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels/{id}");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> GetHotelsAsync(string token, int? pageNumber, int? pageSize)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels?pageNumber={pageNumber}&pageSize={pageSize}");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> GetAllHotelsAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels/all");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
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

        private async Task<int> CreateCountryAndGetIdAsync(string token, string name, string shortName)
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

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC052_Get_All_Hotels_When_Valid_Data_Returns_OK()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user10@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);

            // act
            var response = await GetAllHotelsAsync(token);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.True(body.Count > 0);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC053_Get_All_Hotels_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string invalid_token = "invalidtoken";

            // act
            var response = await GetAllHotelsAsync(invalid_token);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC054_Get_All_Hotels_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            string null_token = null;

            // act
            var response = await GetAllHotelsAsync(null_token);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC055_Get_Hotels_When_Valid_Data_Returns_OK()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user11@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int pageNumber = 1;
            int pageSize = 10;

            // act
            var response = await GetHotelsAsync(token, pageNumber, pageSize);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_totalCount = body["totalCount"].AsValue().GetValue<int>();
            var body_pageNumber = body["pageNumber"].AsValue().GetValue<int>();
            var body_recordNumber = body["recordNumber"].AsValue().GetValue<int>();
            var body_items = body["items"].AsArray();
            Assert.True(body_totalCount > 0);
            Assert.Equal(pageNumber, body_pageNumber);
            Assert.True(body_recordNumber > 0);
            Assert.True(body_items.Count > 0);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC056_Get_Hotels_When_Page_Number_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user12@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int? pageNumber = null;
            int pageSize = 10;

            // act
            var response = await GetHotelsAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC057_Get_Hotels_When_Page_Number_Is_Zero_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user13@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int pageNumber = 0;
            int pageSize = 10;

            // act
            var response = await GetHotelsAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC058_Get_Hotels_When_Page_Number_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user14@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int pageNumber = -1;
            int pageSize = 10;

            // act
            var response = await GetHotelsAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC059_Get_Hotels_When_Page_Size_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user15@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int pageNumber = 1;
            int? pageSize = null;

            // act
            var response = await GetHotelsAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC060_Get_Hotels_When_Page_Size_Is_Zero_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user16@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int pageNumber = 1;
            int pageSize = 0;

            // act
            var response = await GetHotelsAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC061_Get_Hotels_When_Page_Size_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user17@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int pageNumber = 1;
            int pageSize = -1;

            // act
            var response = await GetHotelsAsync(token, pageNumber, pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC062_Create_Hotel_When_Valid_Data_Returns_Created()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user18@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_address = body["address"].AsValue().GetValue<string>();
            var body_rating = body["rating"].AsValue().GetValue<double>();
            var body_countryId = body["countryId"].AsValue().GetValue<int>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal(name, body_name);
            Assert.Equal(address, body_address);
            Assert.Equal(rating, body_rating);
            Assert.Equal(countryId, body_countryId);
            Assert.True(body_id > 0);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC063_Create_Hotel_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user19@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = null;
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC064_Create_Hotel_When_Name_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user20@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC065_Create_Hotel_When_Address_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user21@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = null;
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC066_Create_Hotel_When_Address_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user22@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC067_Create_Hotel_When_Country_Id_Is_Zero_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user23@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = 0;

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC068_Create_Hotel_When_Country_Id_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user24@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = -1;

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC069_Create_Hotel_When_Country_Id_Is_Valid_Returns_Created()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user25@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_address = body["address"].AsValue().GetValue<string>();
            var body_rating = body["rating"].AsValue().GetValue<double>();
            var body_countryId = body["countryId"].AsValue().GetValue<int>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal(name, body_name);
            Assert.Equal(address, body_address);
            Assert.Equal(rating, body_rating);
            Assert.Equal(countryId, body_countryId);
            Assert.True(body_id > 0);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC070_Get_Hotel_By_Id_When_Valid_Id_Returns_OK()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user26@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);

            // act
            var response = await GetHotelAsync(token, id);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_address = body["address"].AsValue().GetValue<string>();
            var body_rating = body["rating"].AsValue().GetValue<double>();
            var body_countryId = body["countryId"].AsValue().GetValue<int>();
            var body_id = body["id"].AsValue().GetValue<int>();
            Assert.Equal(name, body_name);
            Assert.Equal(address, body_address);
            Assert.Equal(rating, body_rating);
            Assert.Equal(countryId, body_countryId);
            Assert.Equal(id, body_id);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC071_Get_Hotel_By_Id_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user27@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int invalid_id = 9999999;

            // act
            var response = await GetHotelAsync(token, invalid_id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC072_Get_Hotel_By_Id_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string invalid_token = "invalidtoken";

            // act
            var response = await GetHotelAsync(invalid_token, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC073_Get_Hotel_By_Id_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string null_token = null;

            // act
            var response = await GetHotelAsync(null_token, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC074_Update_Hotel_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user28@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            string updatedName = "Updated Hotel Paradise";

            // act
            var response = await UpdateHotelAsync(token, id, updatedName, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC075_Update_Hotel_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user29@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            string updatedName = null;

            // act
            var response = await UpdateHotelAsync(token, id, updatedName, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC076_Update_Hotel_When_Name_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user30@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            string updatedName = "";

            // act
            var response = await UpdateHotelAsync(token, id, updatedName, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC077_Update_Hotel_When_Address_Is_Null_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user31@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            string updatedAddress = null;

            // act
            var response = await UpdateHotelAsync(token, id, name, updatedAddress, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC078_Update_Hotel_When_Address_Is_Empty_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user32@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            string updatedAddress = "";

            // act
            var response = await UpdateHotelAsync(token, id, name, updatedAddress, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC079_Update_Hotel_When_Country_Id_Is_Zero_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user33@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            int updatedCountryId = 0;

            // act
            var response = await UpdateHotelAsync(token, id, name, address, rating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC080_Update_Hotel_When_Country_Id_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user34@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            int updatedCountryId = -1;

            // act
            var response = await UpdateHotelAsync(token, id, name, address, rating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC081_Update_Hotel_When_Country_Id_Is_Valid_Returns_NoContent()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user35@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);
            int updatedCountryId = await CreateCountryAndGetIdAsync(token, "Updated Brazil", "UBR");

            // act
            var response = await UpdateHotelAsync(token, id, name, address, rating, updatedCountryId);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC082_Update_Hotel_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user36@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int invalid_id = 9999999;
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");

            // act
            var response = await UpdateHotelAsync(token, invalid_id, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC083_Update_Hotel_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string invalid_token = "invalidtoken";
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = 1;

            // act
            var response = await UpdateHotelAsync(invalid_token, id, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC084_Update_Hotel_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string null_token = null;
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = 1;

            // act
            var response = await UpdateHotelAsync(null_token, id, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC085_Delete_Hotel_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user37@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            string name = "Hotel Paradise";
            string address = "123 Paradise St";
            double? rating = 4.5;
            int countryId = await CreateCountryAndGetIdAsync(token, "Brazil", "BR");
            int id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);

            // act
            var response = await DeleteHotelAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC086_Delete_Hotel_When_User_Is_Not_Admin_Returns_Forbidden()
        {
            // arrange
            string firstName = "User";
            string lastName = "User";
            bool isAdmin = false;
            string email = "user.user1@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int id = 1;

            // act
            var response = await DeleteHotelAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TC087_Delete_Hotel_When_Id_Not_Exists_Returns_NotFound()
        {
            // arrange
            string firstName = "Admin";
            string lastName = "User";
            bool isAdmin = true;
            string email = "admin.user38@example.com";
            string password = "Password1!";
            string token = await CreateUserAndGetTokenAsync(firstName, lastName, isAdmin, email, password);
            int invalid_id = 9999999;

            // act
            var response = await DeleteHotelAsync(token, invalid_id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC088_Delete_Hotel_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string invalid_token = "invalidtoken";

            // act
            var response = await DeleteHotelAsync(invalid_token, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC089_Delete_Hotel_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            int id = 1;
            string null_token = null;

            // act
            var response = await DeleteHotelAsync(null_token, id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
