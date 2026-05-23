// File: TodosIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Xunit;

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
            var request = new
            {
                username = username,
                password = password
            };
            await _client.PostAsJsonAsync("/users", request);
            var response = await _client.PostAsJsonAsync("/users/token", request);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].AsValue().GetValue<string>();
        }

        private async Task<HttpResponseMessage> CreateTodoAsync(string token, string title)
        {
            var request = new
            {
                title = title
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(request)
            };

            if (token != null)
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(httpRequest);
        }

        private async Task<HttpResponseMessage> UpdateTodoAsync(string token, int id, string title, bool isComplete)
        {
            var request = new
            {
                title = title,
                isComplete = isComplete
            };

            var httpRequest = new HttpRequestMessage(HttpMethod.Put, $"/todos/{id}")
            {
                Content = JsonContent.Create(request)
            };

            if (token != null)
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(httpRequest);
        }

        [Fact]
        public async Task TC023_Create_Todo_When_Valid_Data_Returns_Created()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername23", "ValidPass1!");

            // act
            var response = await CreateTodoAsync(valid_token, "Valid Todo");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_title = body["title"].AsValue().GetValue<string>();
            var body_isComplete = body["isComplete"].AsValue().GetValue<bool>();
            Assert.True(body_id > 0);
            Assert.Equal("Valid Todo", body_title);
            Assert.False(body_isComplete);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Todo_When_Title_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername24", "ValidPass1!");

            // act
            var response = await CreateTodoAsync(valid_token, "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Todo_When_Title_Exceeds_Maximum_Length_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername25", "ValidPass1!");
            var title = new string('a', 257);

            // act
            var response = await CreateTodoAsync(valid_token, title);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Todo_When_No_Token_Returns_Unauthorized()
        {
            // act
            var response = await CreateTodoAsync(null, "Valid Todo");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Create_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // act
            var response = await CreateTodoAsync("invalidtoken", "Valid Todo");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Update_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername28", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(valid_token, todo_id, "Updated Todo", true);

            // assert
            var updatedBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var updated_id = updatedBody["id"].AsValue().GetValue<int>();
            var updated_title = updatedBody["title"].AsValue().GetValue<string>();
            var updated_isComplete = updatedBody["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todo_id, updated_id);
            Assert.Equal("Updated Todo", updated_title);
            Assert.True(updated_isComplete);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Update_Todo_When_Title_Is_Empty_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername29", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(valid_token, todo_id, "", true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Update_Todo_When_Title_Exceeds_Maximum_Length_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername30", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();
            var title = new string('a', 257);

            // act
            var response = await UpdateTodoAsync(valid_token, todo_id, title, true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Update_Todo_When_No_Token_Returns_Unauthorized()
        {
            // arrange
            var createResponse = await CreateTodoAsync(null, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(null, todo_id, "Updated Todo", true);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Update_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername32", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync("invalidtoken", todo_id, "Updated Todo", true);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task<HttpResponseMessage> GetTodoAsync(string token, int id)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"/todos/{id}")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            };

            return await _client.SendAsync(httpRequest);
        }

        private async Task<HttpResponseMessage> DeleteTodoAsync(string token, int id)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{id}")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            };

            return await _client.SendAsync(httpRequest);
        }

        private async Task<HttpResponseMessage> ListTodosAsync(string token)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, "/todos")
            {
                Headers = { Authorization = new AuthenticationHeaderValue("Bearer", token) }
            };

            return await _client.SendAsync(httpRequest);
        }

        [Fact]
        public async Task TC033_Delete_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername33", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync(valid_token, todo_id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Delete_Todo_When_ID_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername34", "ValidPass1!");

            // act
            var response = await DeleteTodoAsync(valid_token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Delete_Todo_When_No_Token_Returns_Unauthorized()
        {
            // arrange
            var createResponse = await CreateTodoAsync(null, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync(null, todo_id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Delete_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername36", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync("invalidtoken", todo_id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Get_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername37", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync(valid_token, todo_id);

            // assert
            var getBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var get_id = getBody["id"].AsValue().GetValue<int>();
            var get_title = getBody["title"].AsValue().GetValue<string>();
            var get_isComplete = getBody["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todo_id, get_id);
            Assert.Equal("Valid Todo", get_title);
            Assert.False(get_isComplete);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Get_Todo_When_ID_Does_Not_Exist_Returns_NotFound()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername38", "ValidPass1!");

            // act
            var response = await GetTodoAsync(valid_token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Get_Todo_When_No_Token_Returns_Unauthorized()
        {
            // arrange
            var createResponse = await CreateTodoAsync(null, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync(null, todo_id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Get_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername40", "ValidPass1!");
            var createResponse = await CreateTodoAsync(valid_token, "Valid Todo");
            var body = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todo_id = body["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync("invalidtoken", todo_id);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC041_List_Todos_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername41", "ValidPass1!");

            // act
            var response = await ListTodosAsync(valid_token);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.IsAssignableFrom<JsonArray>(body);
            Assert.True(body.Count == 0);
        }

        [Fact]
        public async Task TC042_List_Todos_When_No_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername42", "ValidPass1!");
            await CreateTodoAsync(valid_token, "Valid Todo");

            // act
            var response = await ListTodosAsync(null);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC043_List_Todos_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("validUsername43", "ValidPass1!");
            await CreateTodoAsync(valid_token, "Valid Todo");

            // act
            var response = await ListTodosAsync("invalidtoken");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
