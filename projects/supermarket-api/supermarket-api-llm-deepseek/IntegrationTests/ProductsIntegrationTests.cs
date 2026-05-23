using Microsoft.AspNetCore.Mvc.Testing;
using Supermarket.API;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

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

        private async Task<int> CreateCategory()
        {
            var response = await _client.PostAsJsonAsync("/api/categories", new { name = $"Category 1" });
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();
            return body["id"].GetValue<int>();
        }

        private async Task<int> CreateProduct(int categoryId)
        {
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = $"Product 1",
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();
            return body["id"].GetValue<int>();
        }

        [Fact]
        public async Task TC011_Get_Products_Valid_Query()
        {
            var categoryId = await CreateCategory();
            for (int i = 0; i < 10; i++)
                await CreateProduct(categoryId);

            var response = await _client.GetAsync($"/api/products?categoryId={categoryId}&page=1&itemsPerPage=10");
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(10, body["items"].AsArray().Count);
            Assert.True(body["totalItems"].GetValue<int>() >= 10);
        }

        [Fact]
        public async Task TC012_Get_Products_Invalid_Category()
        {
            var response = await _client.GetAsync("/api/products?categoryId=aaa&page=1&itemsPerPage=10");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Get_Products_Page_Below_Minimum()
        {
            var response = await _client.GetAsync("/api/products?page=0&itemsPerPage=10");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Get_Products_Items_Per_Page_Below_Minimum()
        {
            var response = await _client.GetAsync("/api/products?page=1&itemsPerPage=0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_Product_Valid_Data()
        {
            var categoryId = await CreateCategory();
            var productName = $"Product 1";

            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = productName,
                quantityInPackage = 50,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });

            var body = await response.Content.ReadFromJsonAsync<JsonNode>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(productName, body["name"].GetValue<string>());
            Assert.Equal(50, body["quantityInPackage"].GetValue<int>());
        }

        [Fact]
        public async Task TC016_Create_Product_Name_Too_Short()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "",
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_Product_Name_Too_Long()
        {
            var categoryId = await CreateCategory();
            var longName = new string('A', 51);

            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = longName,
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_Product_Quantity_Below_Minimum()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "Product",
                quantityInPackage = -1,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_Product_Quantity_Above_Maximum()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "Product",
                quantityInPackage = 101,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Create_Product_Invalid_Unit()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "Product",
                quantityInPackage = 10,
                unitOfMeasurement = "InvalidUnit",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Update_Product_Valid_Data()
        {
            var categoryId = await CreateCategory();
            var productId = await CreateProduct(categoryId);
            var newCategoryId = await CreateCategory();

            var updateResponse = await _client.PutAsJsonAsync($"/api/products/{productId}", new
            {
                name = "UpdatedProduct",
                quantityInPackage = 75,
                unitOfMeasurement = "Kilogram",
                categoryId = newCategoryId
            });

            var body = await updateResponse.Content.ReadFromJsonAsync<JsonNode>();
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            Assert.Equal("UpdatedProduct", body["name"].GetValue<string>());
            Assert.Equal(75, body["quantityInPackage"].GetValue<int>());
            Assert.Equal("Kilogram", body["unitOfMeasurement"].GetValue<string>());
        }

        [Fact]
        public async Task TC022_Update_Product_Nonexistent_Product()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PutAsJsonAsync("/api/products/9999", new
            {
                name = "Product",
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Update_Product_Invalid_Category()
        {
            var categoryId = await CreateCategory();
            var productId = await CreateProduct(categoryId);

            var response = await _client.PutAsJsonAsync($"/api/products/{productId}", new
            {
                name = "Product",
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = 999
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Delete_Product_Success()
        {
            var categoryId = await CreateCategory();
            var productId = await CreateProduct(categoryId);

            var response = await _client.DeleteAsync($"/api/products/{productId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Delete_Product_Nonexistent_Product()
        {
            var response = await _client.DeleteAsync("/api/products/9999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Get_Products_Page_Min_Value()
        {
            var categoryId = await CreateCategory();
            await CreateProduct(categoryId);

            var response = await _client.GetAsync($"/api/products?categoryId={categoryId}&page=1&itemsPerPage=1");
            var body = await response.Content.ReadFromJsonAsync<JsonNode>();

            Assert.Equal(1, body["items"].AsArray().Count);
            Assert.True(body["totalItems"].GetValue<int>() >= 1);
        }

        [Fact]
        public async Task TC028_Create_Product_Valid_Units()
        {
            var categoryId = await CreateCategory();
            var validUnits = new[] { "Unity", "Milligram", "Gram", "Kilogram", "Liter" };

            foreach (var unit in validUnits)
            {
                var response = await _client.PostAsJsonAsync("/api/products", new
                {
                    name = $"Product_{unit}",
                    quantityInPackage = 10,
                    unitOfMeasurement = unit,
                    categoryId = categoryId
                });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task TC031_Create_Product_Name_Min_Length()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "A",
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Create_Product_Name_Max_Length()
        {
            var categoryId = await CreateCategory();
            var name = new string('A', 50);
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = name,
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Create_Product_Quantity_Min_Value()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "Product",
                quantityInPackage = 0,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Create_Product_Quantity_Max_Value()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "Product",
                quantityInPackage = 100,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Create_Product_Missing_Category()
        {
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "Product",
                quantityInPackage = 10,
                unitOfMeasurement = "Gram"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Create_Product_Missing_Quantity()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = "Product",
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Create_Product_Null_Name()
        {
            var categoryId = await CreateCategory();
            var response = await _client.PostAsJsonAsync("/api/products", new
            {
                name = (string)null,
                quantityInPackage = 10,
                unitOfMeasurement = "Gram",
                categoryId = categoryId
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}