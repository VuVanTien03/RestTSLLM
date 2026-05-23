// File: TodosIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using System.Text.Json.Nodes;

namespace TodoApi.IntegrationTests
{
    public class TodosIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public TodosIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> CreateUserAndGetTokenAsync(string username, string password)
        {
            var userRequest = new
            {
                username = username,
                password = password
            };

            await _client.PostAsJsonAsync("/users", userRequest);
            var tokenResponse = await _client.PostAsJsonAsync("/users/token", userRequest);

            var responseBody = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();
            return responseBody["token"].AsValue().GetValue<string>();
        }

        private async Task<HttpResponseMessage> CreateTodoAsync(string token, string title)
        {
            var request = new
            {
                title = title
            };

            var message = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(request)
            };

            if (token != null)
            {
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(message);
        }

        private async Task<HttpResponseMessage> UpdateTodoAsync(string token, int id, string title, bool isComplete)
        {
            var request = new
            {
                title = title,
                isComplete = isComplete
            };

            var message = new HttpRequestMessage(HttpMethod.Put, $"/todos/{id}")
            {
                Content = JsonContent.Create(request)
            };

            if (token != null)
            {
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(message);
        }

        [Fact]
        public async Task TC016_Create_Todo_With_Valid_Title_Returns_Created()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser10", "ValidP@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, "My first todo");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(body["id"]);
            Assert.Equal("My first todo", body["title"].ToString());
            Assert.False(body["isComplete"].AsValue().GetValue<bool>());
        }

        [Fact]
        public async Task TC017_Create_Todo_Without_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser11", "ValidP@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_Todo_Without_Token_Returns_Unauthorized()
        {
            // act
            var response = await CreateTodoAsync(null, "My first todo");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Create_Todo_With_Short_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser12", "ValidP@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Create_Todo_With_Long_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser13", "ValidP@ssw0rd");
            var longTitle = new string('T', 257);

            // act
            var response = await CreateTodoAsync(token, longTitle);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Update_Todo_With_Valid_Data_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser14", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Old title");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(token, todoId, "Updated title", true);

            // assert
            var updatedBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoId, updatedBody["id"].AsValue().GetValue<int>());
            Assert.Equal("Updated title", updatedBody["title"].ToString());
            Assert.True(updatedBody["isComplete"].AsValue().GetValue<bool>());
        }

        [Fact]
        public async Task TC024_Update_Todo_Without_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser15", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Some todo");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(null, todoId, "Another title", false);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Update_Todo_With_Invalid_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser16", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Some todo");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(token, todoId, "", true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Update_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser17", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Some todo");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync("invalid_token", todoId, "Updated title", true);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Update_Todo_With_Non_Existent_ID_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser18", "ValidP@ssw0rd");

            // act
            var response = await UpdateTodoAsync(token, 999999, "Updated title", false);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<HttpResponseMessage> GetTodoAsync(string token, int id)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"/todos/{id}");

            if (token != null)
            {
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(message);
        }

        private async Task<HttpResponseMessage> DeleteTodoAsync(string token, int id)
        {
            var message = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{id}");

            if (token != null)
            {
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(message);
        }

        private async Task<HttpResponseMessage> ListTodosAsync(string token)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "/todos");

            if (token != null)
            {
                message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(message);
        }

        [Fact]
        public async Task TC028_Delete_Todo_With_Valid_Token_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser19", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Todo to delete");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync(token, todoId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Delete_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser20", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Todo to delete");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync("invalid_token", todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Delete_Todo_With_Nonexistent_ID_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser21", "ValidP@ssw0rd");

            // act
            var response = await DeleteTodoAsync(token, 999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Get_Todo_With_Valid_Token_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser22", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Todo to get");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync(token, todoId);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(todoId, responseBody["id"].AsValue().GetValue<int>());
            Assert.Equal("Todo to get", responseBody["title"].ToString());
            Assert.False(responseBody["isComplete"].AsValue().GetValue<bool>());
        }

        [Fact]
        public async Task TC032_Get_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser23", "ValidP@ssw0rd");
            var createTodoResponse = await CreateTodoAsync(token, "Todo to get");
            var todoBody = await createTodoResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = todoBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync("invalid_token", todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Get_Todo_With_Nonexistent_ID_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser24", "ValidP@ssw0rd");

            // act
            var response = await GetTodoAsync(token, 999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC034_List_Todos_With_Valid_Token_No_Todos_Returns_EmptyList()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser25", "ValidP@ssw0rd");

            // act
            var response = await ListTodosAsync(token);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(body);
        }

        [Fact]
        public async Task TC035_List_Todos_With_Valid_Token_With_Todos_Returns_TodoList()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("validUser26", "ValidP@ssw0rd");
            await CreateTodoAsync(token, "First todo");
            await CreateTodoAsync(token, "Second todo");

            // act
            var response = await ListTodosAsync(token);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, body.Count);
        }

        [Fact]
        public async Task TC036_List_Todos_Without_Token_Returns_Unauthorized()
        {
            // act
            var response = await ListTodosAsync(null);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}