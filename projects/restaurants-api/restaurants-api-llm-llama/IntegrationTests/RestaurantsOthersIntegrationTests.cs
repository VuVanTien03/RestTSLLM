// File: RestaurantsIntegrationTests.cs (continuação)

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace IntegrationTests
{
    public partial class RestaurantsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private async Task<HttpResponseMessage> UpdateLogoAsync(int id, HttpContent? content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/restaurants/{id}/logo")
            {
                Content = content
            };

            return await _client.SendAsync(request);
        }

        [Fact]
        public async Task TC030_Get_Restaurants_When_Valid_Data_Returns_Ok()
        {
            // arrange
            var searchPhrase = "McDonald's";
            var pageNumber = 1;
            var pageSize = 10;
            var sortBy = "Name";
            var sortDirection = "Ascending";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber, pageSize, sortBy, sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Get_Restaurants_When_Search_Phrase_Is_Empty_String_Returns_OK()
        {
            // arrange
            var searchPhrase = string.Empty;

            // act
            var response = await GetRestaurantsAsync(searchPhrase);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Get_Restaurants_When_Page_Number_Is_Not_Valid_Positive_Integer_Returns_BadRequest()
        {
            // arrange
            var searchPhrase = "McDonald's";
            var pageNumber = 0;

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageNumber);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Get_Restaurants_When_Page_Size_Is_Not_Valid_Positive_Integer_Returns_BadRequest()
        {
            // arrange
            var searchPhrase = "McDonald's";
            var pageSize = 0;

            // act
            var response = await GetRestaurantsAsync(searchPhrase, pageSize: pageSize);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Get_Restaurants_When_Sort_By_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            var searchPhrase = "McDonald's";
            var sortBy = "Invalid";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, sortBy: sortBy);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Get_Restaurants_When_Sort_Direction_Is_Invalid_Returns_BadRequest()
        {
            // arrange
            var searchPhrase = "McDonald's";
            var sortDirection = "Invalid";

            // act
            var response = await GetRestaurantsAsync(searchPhrase, sortDirection: sortDirection);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Get_Restaurant_When_Valid_Data_Returns_Ok()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            var restaurantResponse = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);
            var body = await restaurantResponse.Content.ReadFromJsonAsync<JsonObject>();
            var id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await GetRestaurantAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Get_Restaurant_When_Id_Does_Not_Exist_Returns_NotFound()
        {
            // act
            var response = await GetRestaurantAsync(9999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Update_Restaurant_When_Valid_Data_Returns_Ok()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            var restaurantResponse = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);
            var body = await restaurantResponse.Content.ReadFromJsonAsync<JsonObject>();
            var id = body["id"].AsValue().GetValue<int>();

            var requestBody = new
            {
                name = "Burger King",
                description = "Fast Food",
                hasDelivery = false
            };

            // act
            var response = await UpdateRestaurantAsync(id, requestBody.name, requestBody.description, requestBody.hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Update_Restaurant_When_Id_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            var requestBody = new
            {
                name = "Burger King",
                description = "Fast Food",
                hasDelivery = false
            };

            // act
            var response = await UpdateRestaurantAsync(9999, requestBody.name, requestBody.description, requestBody.hasDelivery);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Delete_Restaurant_When_Valid_Data_Returns_NoContent()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            var restaurantResponse = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);
            var body = await restaurantResponse.Content.ReadFromJsonAsync<JsonObject>();
            var id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteRestaurantAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Delete_Restaurant_When_Id_Does_Not_Exist_Returns_NotFound()
        {
            // act
            var response = await DeleteRestaurantAsync(9999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Logo_When_Valid_Data_Returns_Ok()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            var restaurantResponse = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);
            var body = await restaurantResponse.Content.ReadFromJsonAsync<JsonObject>();
            var id = body["id"].AsValue().GetValue<int>();

            using var content = new MultipartFormDataContent
            {
                { new StreamContent(new MemoryStream(new byte[] { 0x01, 0x02, 0x03 })), "file", "image.jpg" }
            };

            // act
            var response = await UpdateLogoAsync(id, content);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Logo_When_Id_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            using var content = new MultipartFormDataContent
            {
                { new StreamContent(new MemoryStream(new byte[] { 0x01, 0x02, 0x03 })), "file", "image.jpg" }
            };

            // act
            var response = await UpdateLogoAsync(9999, content);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Update_Logo_When_Logo_Is_Invalid_Returns_UnsupportedMediaType()
        {
            // arrange
            var name = "McDonald's";
            var description = "Fast Food";
            var contactEmail = "mcdonalds@email.com";
            var contactNumber = "1234567890";
            var category = "Fast Food";
            var hasDelivery = true;
            var city = "New York";
            var street = "5th Avenue";
            var postalCode = "10001";

            var restaurantResponse = await CreateRestaurantAsync(name, description, contactEmail, contactNumber, category, hasDelivery, city, street, postalCode);
            var body = await restaurantResponse.Content.ReadFromJsonAsync<JsonObject>();
            var id = body["id"].AsValue().GetValue<int>();

            var requestBody = new
            {
                file = "invalid_logo"
            };

            // act
            var response = await UpdateLogoAsync(id, JsonContent.Create(requestBody));

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
    }
}