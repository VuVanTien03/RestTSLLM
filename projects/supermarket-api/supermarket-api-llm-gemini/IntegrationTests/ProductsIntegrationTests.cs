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

        private async Task<HttpResponseMessage> GetProductsAsync(int page, int itemsPerPage, int? categoryId = null)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["page"] = page.ToString();
            queryParams["itemsPerPage"] = itemsPerPage.ToString();
            if (categoryId.HasValue)
            {
                queryParams["categoryId"] = categoryId.Value.ToString();
            }
            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={x.Value}"));
            return await _client.GetAsync($"/api/products?{queryString}");
        }

        private async Task<HttpResponseMessage> CreateProductAsync(int categoryId, string name, int quantityInPackage, string unitOfMeasurement)
        {
            var requestBody = new
            {
                categoryId = categoryId,
                name = name,
                quantityInPackage = quantityInPackage,
                unitOfMeasurement = unitOfMeasurement
            };
            return await _client.PostAsJsonAsync("/api/products", requestBody);
        }

        private async Task<HttpResponseMessage> UpdateProductAsync(int id, int categoryId, string name, int quantityInPackage, string unitOfMeasurement)
        {
            var requestBody = new
            {
                categoryId = categoryId,
                name = name,
                quantityInPackage = quantityInPackage,
                unitOfMeasurement = unitOfMeasurement
            };
            return await _client.PutAsJsonAsync($"/api/products/{id}", requestBody);
        }

        private async Task<HttpResponseMessage> DeleteProductAsync(int id)
        {
            return await _client.DeleteAsync($"/api/products/{id}");
        }

        private async Task<HttpResponseMessage> CreateCategoryAsync(string name)
        {
            var requestBody = new
            {
                name = name
            };
            return await _client.PostAsJsonAsync("/api/categories", requestBody);
        }

        private async Task<int> CreateCategoryAndGetIdAsync(string name)
        {
            var response = await CreateCategoryAsync(name);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body!["id"].GetValue<int>();
        }

        private async Task<int> CreateProductAndGetIdAsync(int categoryId, string name, int quantityInPackage, string unitOfMeasurement)
        {
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body!["id"].GetValue<int>();
        }


        [Fact]
        public async Task TC013_List_Products_Valid_Parameters_Returns_OK()
        {
            // arrange
            int page = 1;
            int itemsPerPage = 10;

            // act
            var response = await GetProductsAsync(page, itemsPerPage);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC014_List_Products_Invalid_Page_Returns_BadRequest()
        {
            // arrange
            int page = 0;
            int itemsPerPage = 10;

            // act
            var response = await GetProductsAsync(page, itemsPerPage);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_List_Products_Invalid_ItemsPerPage_Returns_BadRequest()
        {
            // arrange
            int page = 1;
            int itemsPerPage = 0;

            // act
            var response = await GetProductsAsync(page, itemsPerPage);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_List_Products_Valid_CategoryId_Returns_OK()
        {
            // arrange
            int page = 1;
            int itemsPerPage = 10;
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");

            // act
            var response = await GetProductsAsync(page, itemsPerPage, categoryId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_List_Products_Invalid_CategoryId_Returns_OK_Empty_Result()
        {
            // arrange
            int page = 1;
            int itemsPerPage = 10;
            int invalidCategoryId = 99999;

            // act
            var response = await GetProductsAsync(page, itemsPerPage, invalidCategoryId);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(0, body!["totalItems"].GetValue<int>());
            Assert.Empty(body!["items"].AsArray());
        }

        [Fact]
        public async Task TC018_Create_Product_Valid_Data_Returns_Created()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "Product A";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            int id = body!["id"].GetValue<int>();

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(id > 0);
        }

        [Fact]
        public async Task TC019_Create_Product_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Create_Product_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "ThisProductNameIsLongerThanTheMaximumAllowedLengthOfFiftyCharacters";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Create_Product_Name_Null_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string? name = null;
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Create_Product_QuantityInPackage_Below_Minimum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "Product B";
            int quantityInPackage = -1;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Create_Product_QuantityInPackage_Above_Maximum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "Product C";
            int quantityInPackage = 101;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Product_Invalid_CategoryId_Returns_BadRequest()
        {
            // arrange
            int invalidCategoryId = 99999;
            string name = "Product D";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";

            // act
            var response = await CreateProductAsync(invalidCategoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Product_Invalid_UnitOfMeasurement_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "Product E";
            int quantityInPackage = 10;
            string invalidUnitOfMeasurement = "InvalidUnit";

            // act
            var response = await CreateProductAsync(categoryId, name, quantityInPackage, invalidUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Update_Product_Valid_Data_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string updatedName = "Updated Product";
            int updatedQuantity = 20;

            // act
            var updateResponse = await UpdateProductAsync(id, categoryId, updatedName, updatedQuantity, unitOfMeasurement);
            var updateBody = await updateResponse.Content.ReadFromJsonAsync<JsonObject>();


            // assert
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            Assert.Equal(updatedName, updateBody!["name"].GetValue<string>());
            Assert.Equal(updatedQuantity, updateBody!["quantityInPackage"].GetValue<int>());

        }

        [Fact]
        public async Task TC027_Update_Product_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int invalidId = 99999;
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "Updated Product B";
            int quantityInPackage = 20;
            string unitOfMeasurement = "Unity";

            // act
            var response = await UpdateProductAsync(invalidId, categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Update_Product_Name_Too_Short_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "";
            int quantityInPackage = 20;

            // act
            var response = await UpdateProductAsync(id, categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Update_Product_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "ThisProductNameIsLongerThanTheMaximumAllowedLengthOfFiftyCharacters";
            int quantityInPackage = 20;

            // act
            var response = await UpdateProductAsync(id, categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Update_Product_Name_Null_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string? name = null;
            int quantityInPackage = 20;

            // act
            var response = await UpdateProductAsync(id, categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Update_Product_QuantityInPackage_Below_Minimum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "Updated Product C";
            int quantityInPackage = -1;

            // act
            var response = await UpdateProductAsync(id, categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Update_Product_QuantityInPackage_Above_Maximum_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "Updated Product D";
            int quantityInPackage = 101;

            // act
            var response = await UpdateProductAsync(id, categoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Update_Product_Invalid_CategoryId_Returns_BadRequest()
        {
            // arrange
            int invalidCategoryId = 99999;
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(await CreateCategoryAndGetIdAsync("CategoryForProduct"), initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "Updated Product E";
            int quantityInPackage = 20;


            // act
            var response = await UpdateProductAsync(id, invalidCategoryId, name, quantityInPackage, unitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Update_Product_Invalid_UnitOfMeasurement_Returns_BadRequest()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string initialName = "ProductToUpdate";
            int initialQuantity = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, initialName, initialQuantity, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();
            string name = "Updated Product F";
            int quantityInPackage = 20;
            string invalidUnitOfMeasurement = "InvalidUnit";

            // act
            var response = await UpdateProductAsync(id, categoryId, name, quantityInPackage, invalidUnitOfMeasurement);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Delete_Product_Valid_ID_Returns_OK()
        {
            // arrange
            int categoryId = await CreateCategoryAndGetIdAsync("CategoryForProduct");
            string name = "ProductToDelete";
            int quantityInPackage = 10;
            string unitOfMeasurement = "Unity";
            var createResponse = await CreateProductAsync(categoryId, name, quantityInPackage, unitOfMeasurement);
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            int id = createBody!["id"].GetValue<int>();

            // act
            var response = await DeleteProductAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Delete_Product_Invalid_ID_Returns_NotFound()
        {
            // arrange
            int invalidId = 99999;

            // act
            var response = await DeleteProductAsync(invalidId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
