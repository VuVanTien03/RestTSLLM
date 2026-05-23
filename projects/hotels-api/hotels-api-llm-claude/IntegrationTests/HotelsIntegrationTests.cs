using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

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

        private async Task<int> CreateHotelAndGetIdAsync(string token, string name, string address, double? rating, int countryId)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(new
                {
                    name = name,
                    address = address,
                    rating = rating,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        [Fact]
        public async Task TC034_Get_All_Hotels_When_Valid_Token_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John34", "Doe34", "john34@test.com", "Test@34", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country34", "C34");
            await CreateHotelAndGetIdAsync(token, "Hotel34", "Address34", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels/all");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var hotels = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotEmpty(hotels);
        }

        [Fact]
        public async Task TC035_Get_All_Hotels_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels/all");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Get_Hotels_Paged_When_Valid_Parameters_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John36", "Doe36", "john36@test.com", "Test@36", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country36", "C36");
            await CreateHotelAndGetIdAsync(token, "Hotel36", "Address36", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels?pageNumber=1&pageSize=10");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var pagedResult = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(pagedResult["items"]);
        }

        [Fact]
        public async Task TC037_Get_Hotels_Paged_When_Invalid_Page_Number_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John37", "Doe37", "john37@test.com", "Test@37", false);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels?pageNumber=-1&pageSize=10");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Get_Hotels_Paged_When_Invalid_Page_Size_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John38", "Doe38", "john38@test.com", "Test@38", false);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels?pageNumber=1&pageSize=0");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Create_Hotel_When_Valid_Data_Returns_Created()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John39", "Doe39", "john39@test.com", "Test@39", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country39", "C39");

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(new
                {
                    name = "Hotel39",
                    address = "Address39",
                    rating = 4.5,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var hotel = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal("Hotel39", hotel["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC040_Create_Hotel_When_Empty_Name_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John40", "Doe40", "john40@test.com", "Test@40", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country40", "C40");

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(new
                {
                    name = "",
                    address = "Address40",
                    rating = 4.5,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Create_Hotel_When_Empty_Address_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John41", "Doe41", "john41@test.com", "Test@41", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country41", "C41");

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(new
                {
                    name = "Hotel41",
                    address = "",
                    rating = 4.5,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Create_Hotel_When_Invalid_Country_Id_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John42", "Doe42", "john42@test.com", "Test@42", false);

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(new
                {
                    name = "Hotel42",
                    address = "Address42",
                    rating = 4.5,
                    countryId = 0
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Get_Hotel_By_Id_When_Valid_Id_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John43", "Doe43", "john43@test.com", "Test@43", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country43", "C43");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel43", "Address43", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels/{hotelId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var hotel = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(hotelId, hotel["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC044_Get_Hotel_By_Id_When_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John44", "Doe44", "john44@test.com", "Test@44", false);

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/hotels/99999");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Update_Hotel_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John45", "Doe45", "john45@test.com", "Test@45", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country45", "C45");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel45", "Address45", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{hotelId}")
            {
                Content = JsonContent.Create(new
                {
                    name = "Hotel45 Updated",
                    address = "Address45 Updated",
                    rating = 4.8,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Update_Hotel_When_Empty_Name_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John46", "Doe46", "john46@test.com", "Test@46", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country46", "C46");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel46", "Address46", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{hotelId}")
            {
                Content = JsonContent.Create(new
                {
                    name = "",
                    address = "Address46",
                    rating = 4.5,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Update_Hotel_When_Empty_Address_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John47", "Doe47", "john47@test.com", "Test@47", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country47", "C47");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel47", "Address47", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{hotelId}")
            {
                Content = JsonContent.Create(new
                {
                    name = "Hotel47",
                    address = "",
                    rating = 4.5,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Update_Hotel_When_Invalid_Country_Id_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John48", "Doe48", "john48@test.com", "Test@48", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country48", "C48");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel48", "Address48", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{hotelId}")
            {
                Content = JsonContent.Create(new
                {
                    name = "Hotel48",
                    address = "Address48",
                    rating = 4.5,
                    countryId = 0
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Update_Hotel_When_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John49", "Doe49", "john49@test.com", "Test@49", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country49", "C49");

            var request = new HttpRequestMessage(HttpMethod.Put, "/api/hotels/99999")
            {
                Content = JsonContent.Create(new
                {
                    name = "Hotel49",
                    address = "Address49",
                    rating = 4.5,
                    countryId = countryId
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC050_Delete_Hotel_When_Valid_Id_And_Admin_Returns_NoContent()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John50", "Doe50", "john50@test.com", "Test@50", true);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country50", "C50");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel50", "Address50", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/hotels/{hotelId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Delete_Hotel_When_Valid_Id_And_Not_Admin_Returns_Forbidden()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John51", "Doe51", "john51@test.com", "Test@51", false);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country51", "C51");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel51", "Address51", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/hotels/{hotelId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task TC052_Delete_Hotel_When_Invalid_Id_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John52", "Doe52", "john52@test.com", "Test@52", true);

            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/hotels/99999");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC053_Delete_Hotel_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("John53", "Doe53", "john53@test.com", "Test@53", true);
            var countryId = await CreateCountryAndGetIdAsync(token, "Country53", "C53");
            var hotelId = await CreateHotelAndGetIdAsync(token, "Hotel53", "Address53", 4.5, countryId);

            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/hotels/{hotelId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

            // act
            var response = await _client.SendAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}