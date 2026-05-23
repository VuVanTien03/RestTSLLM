// File: ProductsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using Supermarket.API;

namespace IntegrationTests
{
    public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> GetProductsAsync(int? categoryId = null, int page = 1, int itemsPerPage = 10)
        {
            string url = $"/api/products?page={page}&itemsPerPage={itemsPerPage}";
            if (categoryId.HasValue)
            {
                url += $"&categoryId={categoryId.Value}";
            }

            return await _client.GetAsync(url);
        }

        private async Task<HttpResponseMessage> CreateCategoryAsync(string name)
        {
            var request = new { name };
            var response = await _client.PostAsJsonAsync("/api/categories", request);
            return response;
        }

        private async Task<int> CreateCategoryAndGetIdAsync(string name)
        {
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> CreateProductAsync(string name, int quantityInPackage, string unitOfMeasurement, int categoryId)
        {
            var request = new
            {
                name,
                quantityInPackage,
                unitOfMeasurement,
                categoryId
            };
            return await _client.PostAsJsonAsync("/api/products", request);
        }

        [Fact]
        public async Task TC014_Get_Products_When_No_Data_Exists_Returns_Empty()
        {
            // act
            var response = await GetProductsAsync();

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal(0, body["totalItems"].AsValue().GetValue<int>());
            Assert.Empty(body["items"].AsArray());
        }

        [Fact]
        public async Task TC015_Get_Products_When_Data_Exists_Using_Category_Filter_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Dairy");
            await CreateProductAsync("Milk", 1, "Liter", categoryId);

            // act
            var response = await GetProductsAsync(categoryId, page: 1, itemsPerPage: 10);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            Assert.Equal(1, body["totalItems"].AsValue().GetValue<int>());
            Assert.NotEmpty(body["items"].AsArray());
        }

        [Fact]
        public async Task TC016_Save_Product_When_Valid_Data_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Dairy");
            string productName = "Eggs";
            int quantityInPackage = 12;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(productName, quantityInPackage, unitOfMeasurement, categoryId);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(body);
            var productId = body["id"].AsValue().GetValue<int>();
            Assert.True(productId > 0);
            Assert.Equal(productName, body["name"].AsValue().GetValue<string>());
            Assert.Equal(quantityInPackage, body["quantityInPackage"].AsValue().GetValue<int>());
            Assert.Equal(unitOfMeasurement, body["unitOfMeasurement"].AsValue().GetValue<string>());
            Assert.Equal(categoryId, body["category"]["id"].AsValue().GetValue<int>());
        }

        [Fact]
        public async Task TC017_Save_Product_When_QuantityInPackage_Below_Minimum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Dairy");
            string productName = "Cheese";
            int invalidQuantity = -1; // below minimum
            string unitOfMeasurement = "Kilogram";

            // act
            var response = await CreateProductAsync(productName, invalidQuantity, unitOfMeasurement, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Save_Product_When_QuantityInPackage_Above_Maximum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Spices");
            string productName = "Pepper";
            int invalidQuantity = 101; // above maximum
            string unitOfMeasurement = "Gram";

            // act
            var response = await CreateProductAsync(productName, invalidQuantity, unitOfMeasurement, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Save_Product_When_QuantityInPackage_Has_Minimum_Value_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Beverages");
            string productName = "Juice";
            int quantityInPackage = 0; // minimum valid
            string unitOfMeasurement = "Liter";

            // act
            var response = await CreateProductAsync(productName, quantityInPackage, unitOfMeasurement, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(productName, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC020_Save_Product_When_QuantityInPackage_Has_Maximum_Value_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Snacks");
            string productName = "Popcorn";
            int quantityInPackage = 100; // maximum valid
            string unitOfMeasurement = "Gram";

            // act
            var response = await CreateProductAsync(productName, quantityInPackage, unitOfMeasurement, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(productName, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC021_Save_Product_When_Name_Is_Too_Long_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Seafood");
            string productName = new string('A', 51); // Name length: 51
            int quantityInPackage = 1;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(productName, quantityInPackage, unitOfMeasurement, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Save_Product_When_Name_Has_Maximum_Length_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Seafood");
            string productName = new string('A', 50); // Name length: 50
            int quantityInPackage = 2;
            string unitOfMeasurement = "Kilogram";

            // act
            var response = await CreateProductAsync(productName, quantityInPackage, unitOfMeasurement, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(productName, body["name"].AsValue().GetValue<string>());
        }

        [Fact]
        public async Task TC023_Save_Product_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("Vegetables");
            string productName = null; // invalid name
            int quantityInPackage = 5;
            string unitOfMeasurement = "Kilogram";

            // act
            var response = await CreateProductAsync(productName, quantityInPackage, unitOfMeasurement, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}