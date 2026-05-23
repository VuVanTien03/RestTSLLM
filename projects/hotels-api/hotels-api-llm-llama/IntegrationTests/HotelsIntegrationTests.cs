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

        private async Task<string> CreateUserAndGetTokenAsync(string email, string firstName, string lastName, bool isAdmin, string password)
        {
            var request = new
            {
                email = email,
                firstName = firstName,
                lastName = lastName,
                isAdmin = isAdmin,
                password = password
            };

            await _client.PostAsJsonAsync("/api/accounts", request);

            var response = await _client.PostAsJsonAsync("/api/accounts/tokens", new
            {
                email = email,
                password = password
            });

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var token = body["token"].AsValue().GetValue<string>();
            return token;
        }

        private async Task<HttpResponseMessage> CreateHotelAsync(string token, string name, string address, double rating, int countryId)
        {
            var request = new
            {
                name = name,
                address = address,
                rating = rating,
                countryId = countryId
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/api/hotels")
            {
                Content = JsonContent.Create(request)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<int> CreateHotelAndGetIdAsync(string token, string name, string address, double rating, int countryId)
        {
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> GetHotelsAsync(string token, int pageNumber, int pageSize)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels?pageNumber={pageNumber}&pageSize={pageSize}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> GetHotelAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/api/hotels/{id}");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> UpdateHotelAsync(string token, int id, string name, string address, double rating, int countryId)
        {
            var request = new
            {
                name = name,
                address = address,
                rating = rating,
                countryId = countryId
            };

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/api/hotels/{id}")
            {
                Content = JsonContent.Create(request)
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> DeleteHotelAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/api/hotels/{id}")
            {
                Headers =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", token)
                }
            };

            return await _client.SendAsync(requestMessage);
        }

        [Fact]
        public async Task TC037_Get_Hotels_When_Valid_Data_Returns_OK()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);
            
            // act
            var response = await GetHotelsAsync(token, 1, 10);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Get_Hotel_When_Valid_Data_Returns_OK()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Hotel";
            string address = "Endereço";
            double rating = 5;
            int countryId = 1;
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);
            var id = await CreateHotelAndGetIdAsync(token, name, address, rating, countryId);

            // act
            var response = await GetHotelAsync(token, id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Create_Hotel_When_Valid_Data_Returns_Created()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = "Hotel";
            string address = "Endereço";
            double rating = 5;
            int countryId = 1;
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Create_Hotel_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            string email = "valido@email.com";
            string firstName = "Nome";
            string lastName = "Sobrenome";
            bool isAdmin = true;
            string password = "Senha123!";
            string name = null;
            string address = "Endereço";
            double rating = 5;
            int countryId = 1;
            var token = await CreateUserAndGetTokenAsync(email, firstName, lastName, isAdmin, password);

            // act
            var response = await CreateHotelAsync(token, name, address, rating, countryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // outros testes
    }
}