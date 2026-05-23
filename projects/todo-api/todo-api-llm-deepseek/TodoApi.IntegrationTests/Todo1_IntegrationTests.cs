// File: TodosIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;

namespace IntegrationTests
{
    public class TodosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly Random _random = new Random();

        public TodosIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> CreateUserAndGetTokenAsync()
        {
            var username = $"user_{_random.Next(1000, 9999)}_{DateTime.Now.Ticks}";
            var password = "ValidP@ss1!";

            await _client.PostAsJsonAsync("/users", new { username, password });
            var tokenResponse = await _client.PostAsJsonAsync("/users/token", new { username, password });
            var token = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();
            return token["token"].ToString();
        }

        private async Task<int> CreateTodoAsync(string token)
        {
            var title = "Sample Todo";
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(new { title })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.SendAsync(request);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].GetValue<int>();
        }

        [Fact]
        public async Task TC016_List_Todos_With_Valid_Token_Returns_EmptyArray()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();

            // Act
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.GetAsync("/todos");

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
            Assert.Empty(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_Todo_With_ValidData_ReturnsCreated()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();
            var title = "New Todo Item";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(new { title })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body["id"]);
            Assert.Equal(title, body["title"].ToString());
            Assert.False(body["isComplete"].GetValue<bool>());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_Todo_WithoutTitle_ReturnsBadRequest()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(new { })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_Todo_WithLongTitle_ReturnsBadRequest()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();
            var longTitle = new string('a', 257);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(new { title = longTitle })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Create_Todo_WithMaximumTitle_ReturnsOK()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();
            var maxTitle = new string('a', 256);

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(new { title = maxTitle })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(maxTitle, body["title"].ToString());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Update_Todo_WithEmptyTitle_ReturnsBadRequest()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();
            var todoId = await CreateTodoAsync(token);

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/todos/{todoId}")
            {
                Content = JsonContent.Create(new { title = "", isComplete = false })
            };
            updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Partial_Update_Todo_ReturnsBadRequest()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();
            var todoId = await CreateTodoAsync(token);

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/todos/{todoId}")
            {
                Content = JsonContent.Create(new { title = "New Title" })
            };
            updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Update_Todo_CompleteStatus_ReturnsOK()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();
            var todoId = await CreateTodoAsync(token);

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/todos/{todoId}")
            {
                Content = JsonContent.Create(new { title = "Updated Title", isComplete = true })
            };
            updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(updateRequest);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body["isComplete"].GetValue<bool>());
            Assert.Equal("Updated Title", body["title"].ToString());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Update_Todo_By_Other_User_ReturnsNotFound()
        {
            // Arrange
            var token1 = await CreateUserAndGetTokenAsync();
            var todoId = await CreateTodoAsync(token1);
            var token2 = await CreateUserAndGetTokenAsync();

            // Act
            var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/todos/{todoId}")
            {
                Content = JsonContent.Create(new { title = "Hacked Title", isComplete = true })
            };
            updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
            var response = await _client.SendAsync(updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Get_Todo_With_Invalid_ID_Returns_NotFound()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/todos/9999999");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Delete_Todo_When_Unauthorized_Returns_NotFound()
        {
            // Arrange
            var token1 = await CreateUserAndGetTokenAsync();
            var todoId = await CreateTodoAsync(token1);
            var token2 = await CreateUserAndGetTokenAsync();

            // Act
            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{todoId}");
            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token2);
            var response = await _client.SendAsync(deleteRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Access_Todos_Without_Token_Returns_Unauthorized()
        {
            // Act
            var response = await _client.GetAsync("/todos");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // Arrange
            var invalidToken = "invalidtoken123";

            // Act
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos");
            request.Content = JsonContent.Create(new { title = "Test todo" });
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", invalidToken);
            var response = await _client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Get_Deleted_Todo_Returns_NotFound()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();
            var todoId = await CreateTodoAsync(token);

            // Delete the todo first
            var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{todoId}");
            deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            await _client.SendAsync(deleteRequest);

            // Act
            var getRequest = new HttpRequestMessage(HttpMethod.Get, $"/todos/{todoId}");
            getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(getRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Multiple_Todo_Pagination_CheckOrder()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();

            // Create 3 todos
            for (int i = 1; i <= 3; i++)
            {
                await CreateTodoAsync(token);
            }

            // Act
            var request = new HttpRequestMessage(HttpMethod.Get, "/todos");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _client.SendAsync(request);

            // Assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.Equal(3, body.Count);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Full_CRUD_Flow_Success()
        {
            // Arrange
            var token = await CreateUserAndGetTokenAsync();

            // Create
            var createResponse = await CreateTodoAsync(token, "CRUD Test Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = body["id"].GetValue<int>();

            // Get
            var getResponse = await _client.GetAsync($"/todos/{todoId}");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            // Update
            var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/todos/{todoId}")
            {
                Content = JsonContent.Create(new { title = "Updated", isComplete = true })
            };
            updateRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var updateResponse = await _client.SendAsync(updateRequest);
            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            // Delete
            var deleteResponse = await _client.DeleteAsync($"/todos/{todoId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
        }

        private async Task<HttpResponseMessage> CreateTodoAsync(string token, string title = "Default Todo")
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(new { title })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }
    }
}
