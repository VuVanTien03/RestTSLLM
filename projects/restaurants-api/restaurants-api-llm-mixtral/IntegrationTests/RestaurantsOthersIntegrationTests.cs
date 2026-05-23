// File: RestaurantsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace IntegrationTests
{
    public class RestaurantsIntegration2Tests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public RestaurantsIntegration2Tests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetRestaurantsAsync(string searchPhrase, int? pageNumber, int? pageSize, string sortBy, string sortDirection)
        {
            var query = $"?searchPhrase={searchPhrase}&pageNumber={pageNumber}&pageSize={pageSize}&sortBy={sortBy}&sortDirection={sortDirection}";
            return await _client.GetAsync($"/api/restaurants{query}");
        }

        private async Task<(HttpResponseMessage response, JsonObject body)> CreateRestaurantAsync()
        {
            var requestBody = new
            {
                name = "ValidName",
                description = "Valid description",
                category = "ValidCategory",
                hasDelivery = true,
                contactEmail = "valid@example.com",
                contactNumber = "1234567890",
                city = "ValidCity",
                street = "ValidStreet",
                postalCode = "12345"
            };

            var response = await _client.PostAsJsonAsync("/api/restaurants", requestBody);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return (response, body);
        }

        private async Task<HttpResponseMessage> GetRestaurantByIdAsync(int id)
        {
            return await _client.GetAsync($"/api/restaurants/{id}");
        }

        private async Task<HttpResponseMessage> PatchRestaurantAsync(int id, string name, string description, bool hasDelivery)
        {
            var requestBody = new
            {
                name = name,
                description = description,
                hasDelivery = hasDelivery
            };

            return await _client.PatchAsJsonAsync($"/api/restaurants/{id}", requestBody);
        }

        private async Task<HttpResponseMessage> DeleteRestaurantAsync(int id)
        {
            return await _client.DeleteAsync($"/api/restaurants/{id}");
        }

        private async Task<HttpResponseMessage> UpdateRestaurantLogoAsync(int id, byte[] fileContent)
        {
            var content = new MultipartFormDataContent
            {
                { new ByteArrayContent(fileContent), "file", "logo.jpg" }
            };

            return await _client.PostAsync($"/api/restaurants/{id}/logo", content);
        }

        [Fact]
        public async Task TC026_Get_Restaurants_When_Valid_Data_Returns_OK()
        {
            // arrange
            string searchPhrase = "search";
            int pageNumber = 1;
            int pageSize = 10;
            string sortBy = "Name";
            string sortDirection = "Ascending";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["items"].AsArray());
            Assert.True(body["totalPages"].AsValue().GetValue<int>() >= 0);
            Assert.True(body["totalItemsCount"].AsValue().GetValue<int>() >= 0);
            Assert.True(body["itemsFrom"].AsValue().GetValue<int>() >= 0);
            Assert.True(body["itemsTo"].AsValue().GetValue<int>() >= 0);
        }

        [Fact]
        public async Task TC027_Get_Restaurants_When_PageNumber_Is_Zero_Returns_BadRequest()
        {
            // arrange
            string searchPhrase = "search";
            int pageNumber = 0;
            int pageSize = 10;
            string sortBy = "Name";
            string sortDirection = "Ascending";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Get_Restaurants_When_PageNumber_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string searchPhrase = "search";
            int pageNumber = -1;
            int pageSize = 10;
            string sortBy = "Name";
            string sortDirection = "Ascending";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Get_Restaurants_When_PageSize_Is_Zero_Returns_BadRequest()
        {
            // arrange
            string searchPhrase = "search";
            int pageNumber = 1;
            int pageSize = 0;
            string sortBy = "Name";
            string sortDirection = "Ascending";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Get_Restaurants_When_PageSize_Is_Negative_Returns_BadRequest()
        {
            // arrange
            string searchPhrase = "search";
            int pageNumber = 1;
            int pageSize = -1;
            string sortBy = "Name";
            string sortDirection = "Ascending";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Get_Restaurants_When_SortBy_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            string searchPhrase = "search";
            int pageNumber = 1;
            int pageSize = 10;
            string sortBy = "Invalid";
            string sortDirection = "Ascending";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Get_Restaurants_When_SortDirection_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            string searchPhrase = "search";
            int pageNumber = 1;
            int pageSize = 10;
            string sortBy = "Name";
            string sortDirection = "Invalid";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Get_Restaurant_By_Id_When_Valid_Data_Returns_OK()
        {
            // arrange
            var (response, body) = await CreateRestaurantAsync();
            int id = body["id"].AsValue().GetValue<int>();

            // act
            var getResponse = await GetRestaurantByIdAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var getBody = await getResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(id, getBody["id"].AsValue().GetValue<int>());
            Assert.Equal("ValidName", getBody["name"].AsValue().GetValue<string>());
            Assert.Equal("Valid description", getBody["description"].AsValue().GetValue<string>());
            Assert.Equal("ValidCategory", getBody["category"].AsValue().GetValue<string>());
            Assert.Equal(true, getBody["hasDelivery"].AsValue().GetValue<bool>());
            Assert.Equal("ValidCity", getBody["city"].AsValue().GetValue<string>());
            Assert.Equal("ValidStreet", getBody["street"].AsValue().GetValue<string>());
            Assert.Equal("12345", getBody["postalCode"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC052_Get_Restaurant_By_Id_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int id = 9999999; // Invalid restaurant ID

            // act
            var response = await GetRestaurantByIdAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC053_Patch_Restaurant_When_Valid_Data_Returns_OK()
        {
            // arrange
            var (response, body) = await CreateRestaurantAsync();
            int id = body["id"].AsValue().GetValue<int>();
            string name = "UpdatedName";
            string description = "Updated description";
            bool hasDelivery = false;

            // act
            var patchResponse = await PatchRestaurantAsync(id, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
            var patchBody = await patchResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(id, patchBody["id"].AsValue().GetValue<int>());
            Assert.Equal(name, patchBody["name"].AsValue().GetValue<string>());
            Assert.Equal(description, patchBody["description"].AsValue().GetValue<string>());
            Assert.Equal(hasDelivery, patchBody["hasDelivery"].AsValue().GetValue<bool>());
        }

        [Fact]
        public async Task TC054_Patch_Restaurant_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int id = 9999999; // Invalid restaurant ID
            string name = "UpdatedName";
            string description = "Updated description";
            bool hasDelivery = false;

            // act
            var response = await PatchRestaurantAsync(id, name, description, hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC055_Delete_Restaurant_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var (response, body) = await CreateRestaurantAsync();
            int id = body["id"].AsValue().GetValue<int>();

            // act
            var deleteResponse = await DeleteRestaurantAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task TC056_Delete_Restaurant_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int id = 9999999; // Invalid restaurant ID

            // act
            var response = await DeleteRestaurantAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC057_Update_Restaurant_Logo_When_Valid_Content_Type_Returns_OK()
        {
            // arrange
            var (response, body) = await CreateRestaurantAsync();
            int id = body["id"].AsValue().GetValue<int>();
            byte[] fileContent = new byte[] { 0x01, 0x02, 0x03 }; // simulate file

            // act
            var logoResponse = await UpdateRestaurantLogoAsync(id, fileContent);

            // assert
            Assert.Equal(HttpStatusCode.OK, logoResponse.StatusCode);
            var logoBody = await logoResponse.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(id, logoBody["id"].AsValue().GetValue<int>());
            Assert.NotNull(logoBody["logoSasUrl"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC058_Update_Restaurant_Logo_When_Invalid_Content_Type_Returns_UnsupportedMediaType()
        {
            // arrange
            int id = 1; // Valid restaurant ID
            var content = JsonContent.Create(new { file = "invalidfile.jpg" }); // invalid content

            // act
            var response = await _client.PostAsync($"/api/restaurants/{id}/logo", content);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }

        [Fact]
        public async Task TC059_Update_Restaurant_Logo_When_Restaurant_Not_Found_Returns_NotFound()
        {
            // arrange
            int id = 9999999; // Invalid restaurant ID
            byte[] fileContent = new byte[] { 0x01, 0x02, 0x03 }; // simulate file

            // act
            var response = await UpdateRestaurantLogoAsync(id, fileContent);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}