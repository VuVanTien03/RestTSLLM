// File: ProductsIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using Supermarket.API;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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

        private async Task<HttpResponseMessage> ListProductsAsync(int categoryId, int page, int itemsPerPage)
        {
            return await _client.GetAsync($"/api/products?categoryId={categoryId}&page={page}&itemsPerPage={itemsPerPage}");
        }

        private async Task<HttpResponseMessage> SaveProductAsync(string name, int quantityInPackage, string unitOfMeasurement, int categoryId)
        {
            var requestBody = new
            {
                name = name,
                quantityInPackage = quantityInPackage,
                unitOfMeasurement = unitOfMeasurement,
                categoryId = categoryId
            };

            return await _client.PostAsJsonAsync("/api/products", requestBody);
        }

        private async Task<HttpResponseMessage> UpdateProductAsync(int id, string name, int quantityInPackage, string unitOfMeasurement, int categoryId)
        {
            var requestBody = new
            {
                name = name,
                quantityInPackage = quantityInPackage,
                unitOfMeasurement = unitOfMeasurement,
                categoryId = categoryId
            };

            return await _client.PutAsJsonAsync($"/api/products/{id}", requestBody);
        }

        private async Task<HttpResponseMessage> DeleteProductAsync(int id)
        {
            return await _client.DeleteAsync($"/api/products/{id}");
        }

        [Fact]
        public async Task TC019_List_Products_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await ListProductsAsync(categoryId, 1, 10);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC020_List_Products_When_Page_Value_Is_Less_Than_1_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await ListProductsAsync(categoryId, 0, 10);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_List_Products_When_ItemsPerPage_Value_Is_Less_Than_1_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await ListProductsAsync(categoryId, 1, 0);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_List_Products_When_Page_Is_Null_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var uri = $"/api/products?categoryId={categoryId}&itemsPerPage=10";
            var response = await _client.GetAsync(uri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_List_Products_When_ItemsPerPage_Is_Null_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var uri = $"/api/products?categoryId={categoryId}&page=1";
            var response = await _client.GetAsync(uri);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Save_Product_When_Valid_Data_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("Product", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Save_Product_When_Name_Is_Null_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync(null, 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Save_Product_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Save_Product_When_Name_Too_Short_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Save_Product_When_Name_Too_Long_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("This is a very long name for a product, lets test it for save.", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Save_Product_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("P", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Save_Product_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("This is product with a maximum size of 50 characters", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Save_Product_When_QuantityInPackage_Is_Null_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var requestBody = new
            {
                name = "Product",
                quantityInPackage = (int?)null,
                unitOfMeasurement = "Unity",
                categoryId = categoryId
            };
            var response = await _client.PostAsJsonAsync("/api/products", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Save_Product_When_QuantityInPackage_Below_Minimum_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("Product", -1, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Save_Product_When_QuantityInPackage_Above_Maximum_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("Product", 101, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Save_Product_When_QuantityInPackage_Has_Minimum_Value_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("Product", 0, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Save_Product_When_QuantityInPackage_Has_Maximum_Value_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await SaveProductAsync("Product", 100, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Update_Product_When_Valid_Data_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "Product", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Update_Product_When_ID_Not_Found_Returns_NotFound()
        {
            // Arrange
            var categoryId = await CreateCategoryId();

            // Act
            var response = await UpdateProductAsync(9999, "Product", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Update_Product_When_Name_Is_Null_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, null, 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Update_Product_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Update_Product_When_Name_Too_Short_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_Product_When_Name_Too_Long_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "This is a very long name for a product, lets test it for update.", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Product_When_Name_Has_Minimum_Size_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "P", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Product_When_Name_Has_Maximum_Size_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "This is product with a maximum size of 50 characters", 10, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Update_Product_When_QuantityInPackage_Is_Null_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var requestBody = new
            {
                name = "Product",
                quantityInPackage = (int?)null,
                unitOfMeasurement = "Unity",
                categoryId = categoryId
            };
            var response = await _client.PutAsJsonAsync($"/api/products/{productId}", requestBody);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Update_Product_When_QuantityInPackage_Below_Minimum_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "Product", -1, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Update_Product_When_QuantityInPackage_Above_Maximum_Returns_BadRequest()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "Product", 101, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Update_Product_When_QuantityInPackage_Has_Minimum_Value_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "Product", 0, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Update_Product_When_QuantityInPackage_Has_Maximum_Value_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await UpdateProductAsync(productId, "Product", 100, "Unity", categoryId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Delete_Product_When_ID_Found_Returns_OK()
        {
            // Arrange
            var categoryId = await CreateCategoryId();
            var productId = await CreateProductId(categoryId);

            // Act
            var response = await DeleteProductAsync(productId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC050_Delete_Product_When_ID_Not_Found_Returns_NotFound()
        {
            // Act
            var response = await DeleteProductAsync(9999);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<int> CreateCategoryId()
        {
            var requestBody = new
            {
                name = "Category"
            };

            var response = await _client.PostAsJsonAsync("/api/categories", requestBody);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<int> CreateProductId(int categoryId)
        {
            var requestBody = new
            {
                name = "Product",
                quantityInPackage = 10,
                unitOfMeasurement = "Unity",
                categoryId = categoryId
            };

            var response = await _client.PostAsJsonAsync("/api/products", requestBody);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }
    }
}