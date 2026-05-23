using Microsoft.AspNetCore.Mvc.Testing;
using Supermarket.API;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace IntegrationTests;

public class ProductsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProductsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<HttpResponseMessage> ListProductsAsync(int page, int itemsPerPage, int? categoryId = null)
    {
        var url = $"/api/products?page={page}&itemsPerPage={itemsPerPage}";
        if (categoryId.HasValue)
        {
            url += $"&categoryId={categoryId}";
        }
        return await _client.GetAsync(url);
    }

    private async Task<HttpResponseMessage> CreateProductAsync(string name, int quantityInPackage, string unitOfMeasurement, int categoryId)
    {
        var request = new
        {
            name = name,
            quantityInPackage = quantityInPackage,
            unitOfMeasurement = unitOfMeasurement,
            categoryId = categoryId
        };

        return await _client.PostAsJsonAsync("/api/products", request);
    }

    private async Task<int> CreateProductAndGetIdAsync(string name, int quantityInPackage, string unitOfMeasurement, int categoryId)
    {
        var response = await CreateProductAsync(name, quantityInPackage, unitOfMeasurement, categoryId);
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        return body["id"].GetValue<int>();
    }

    private async Task<HttpResponseMessage> UpdateProductAsync(int id, string name, int quantityInPackage, string unitOfMeasurement, int categoryId)
    {
        var request = new
        {
            name = name,
            quantityInPackage = quantityInPackage,
            unitOfMeasurement = unitOfMeasurement,
            categoryId = categoryId
        };

        return await _client.PutAsJsonAsync($"/api/products/{id}", request);
    }

    private async Task<HttpResponseMessage> DeleteProductAsync(int id)
    {
        return await _client.DeleteAsync($"/api/products/{id}");
    }

    private async Task<int> CreateCategoryAndGetIdAsync(string name)
    {
        var request = new { name = name };
        var response = await _client.PostAsJsonAsync("/api/categories", request);
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        return body["id"].GetValue<int>();
    }

    [Fact]
    public async Task TC015_List_Products_When_Empty_Returns_Empty_Result()
    {
        // act
        var response = await ListProductsAsync(page: 1, itemsPerPage: 10);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(0, body["totalItems"].GetValue<int>());
        Assert.Empty(body["items"].AsArray());
    }

    [Fact]
    public async Task TC016_List_Products_When_Has_Items_Returns_Paginated_Result()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category A");
        await CreateProductAsync("Product A1", 10, "Unity", categoryId);
        await CreateProductAsync("Product A2", 20, "Kilogram", categoryId);

        // act
        var response = await ListProductsAsync(page: 1, itemsPerPage: 10);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(2, body["totalItems"].GetValue<int>());

        var items = body["items"].AsArray();
        Assert.Equal(2, items.Count);

        var first = items[0].AsObject();
        Assert.True(first["id"].GetValue<int>() > 0);
        Assert.Equal("Product A1", first["name"].GetValue<string>());
        Assert.Equal(10, first["quantityInPackage"].GetValue<int>());
        Assert.Equal("Unity", first["unitOfMeasurement"].GetValue<string>());
        Assert.Equal(categoryId, first["category"].AsObject()["id"].GetValue<int>());

        var second = items[1].AsObject();
        Assert.True(second["id"].GetValue<int>() > 0);
        Assert.Equal("Product A2", second["name"].GetValue<string>());
        Assert.Equal(20, second["quantityInPackage"].GetValue<int>());
        Assert.Equal("Kilogram", second["unitOfMeasurement"].GetValue<string>());
        Assert.Equal(categoryId, second["category"].AsObject()["id"].GetValue<int>());
    }

    [Fact]
    public async Task TC017_List_Products_When_Page_Less_Than_One_Returns_BadRequest()
    {
        // act
        var response = await ListProductsAsync(page: 0, itemsPerPage: 10);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC018_List_Products_When_Items_Per_Page_Less_Than_One_Returns_BadRequest()
    {
        // act
        var response = await ListProductsAsync(page: 1, itemsPerPage: 0);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC019_List_Products_When_Filter_By_Category_Returns_Filtered_Result()
    {
        // arrange
        var category1Id = await CreateCategoryAndGetIdAsync("Category B");
        var category2Id = await CreateCategoryAndGetIdAsync("Category C");

        await CreateProductAsync("Product B1", 10, "Unity", category1Id);
        await CreateProductAsync("Product C1", 20, "Kilogram", category2Id);

        // act
        var response = await ListProductsAsync(page: 1, itemsPerPage: 10, categoryId: category1Id);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(1, body["totalItems"].GetValue<int>());

        var items = body["items"].AsArray();
        Assert.Single(items);

        var first = items[0].AsObject();
        Assert.True(first["id"].GetValue<int>() > 0);
        Assert.Equal("Product B1", first["name"].GetValue<string>());
        Assert.Equal(10, first["quantityInPackage"].GetValue<int>());
        Assert.Equal("Unity", first["unitOfMeasurement"].GetValue<string>());
        Assert.Equal(category1Id, first["category"].AsObject()["id"].GetValue<int>());
    }

    [Fact]
    public async Task TC020_Create_Product_When_Valid_Data_Returns_OK()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category D");

        // act
        var response = await CreateProductAsync("Product D1", 10, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.True(body["id"].GetValue<int>() > 0);
        Assert.Equal("Product D1", body["name"].GetValue<string>());
        Assert.Equal(10, body["quantityInPackage"].GetValue<int>());
        Assert.Equal("Unity", body["unitOfMeasurement"].GetValue<string>());
        Assert.Equal(categoryId, body["category"].AsObject()["id"].GetValue<int>());
    }

    [Fact]
    public async Task TC021_Create_Product_When_Name_Is_Empty_Returns_BadRequest()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category E");

        // act
        var response = await CreateProductAsync("", 10, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC022_Create_Product_When_Name_Is_Null_Returns_BadRequest()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category F");

        // act
        var response = await CreateProductAsync(null, 10, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC023_Create_Product_When_Name_Too_Long_Returns_BadRequest()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category G");
        var name = "123456789012345678901234567890123456789012345678901"; // 51 chars

        // act
        var response = await CreateProductAsync(name, 10, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC024_Create_Product_When_Name_Has_Maximum_Size_Returns_OK()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category H");
        var name = "12345678901234567890123456789012345678901234567890"; // 50 chars

        // act
        var response = await CreateProductAsync(name, 10, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.True(body["id"].GetValue<int>() > 0);
        Assert.Equal(name, body["name"].GetValue<string>());
        Assert.Equal(10, body["quantityInPackage"].GetValue<int>());
        Assert.Equal("Unity", body["unitOfMeasurement"].GetValue<string>());
        Assert.Equal(categoryId, body["category"].AsObject()["id"].GetValue<int>());
    }

    [Fact]
    public async Task TC025_Create_Product_When_Quantity_Less_Than_Zero_Returns_BadRequest()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category I");

        // act
        var response = await CreateProductAsync("Product I1", -1, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC026_Create_Product_When_Quantity_Above_Maximum_Returns_BadRequest()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category J");

        // act
        var response = await CreateProductAsync("Product J1", 101, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC027_Create_Product_When_Invalid_Unit_Of_Measurement_Returns_BadRequest()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category K");

        // act
        var response = await CreateProductAsync("Product K1", 10, "Invalid", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC028_Create_Product_When_Invalid_Category_Returns_BadRequest()
    {
        // act
        var response = await CreateProductAsync("Product L1", 10, "Unity", 999999);

        // assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TC029_Update_Product_When_Valid_Data_Returns_OK()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category M");
        var productId = await CreateProductAndGetIdAsync("Product M1", 10, "Unity", categoryId);

        // act
        var response = await UpdateProductAsync(productId, "Product M2", 20, "Kilogram", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(productId, body["id"].GetValue<int>());
        Assert.Equal("Product M2", body["name"].GetValue<string>());
        Assert.Equal(20, body["quantityInPackage"].GetValue<int>());
        Assert.Equal("Kilogram", body["unitOfMeasurement"].GetValue<string>());
        Assert.Equal(categoryId, body["category"].AsObject()["id"].GetValue<int>());
    }

    [Fact]
    public async Task TC030_Update_Product_When_Invalid_ID_Returns_NotFound()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category N");

        // act
        var response = await UpdateProductAsync(999999, "Product N1", 10, "Unity", categoryId);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task TC031_Delete_Product_When_Valid_ID_Returns_OK()
    {
        // arrange
        var categoryId = await CreateCategoryAndGetIdAsync("Category O");
        var productId = await CreateProductAndGetIdAsync("Product O1", 10, "Unity", categoryId);

        // act
        var response = await DeleteProductAsync(productId);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        Assert.Equal(productId, body["id"].GetValue<int>());
        Assert.Equal("Product O1", body["name"].GetValue<string>());
        Assert.Equal(10, body["quantityInPackage"].GetValue<int>());
        Assert.Equal("Unity", body["unitOfMeasurement"].GetValue<string>());
        Assert.Equal(categoryId, body["category"].AsObject()["id"].GetValue<int>());
    }

    [Fact]
    public async Task TC032_Delete_Product_When_Invalid_ID_Returns_NotFound()
    {
        // act
        var response = await DeleteProductAsync(999999);

        // assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}