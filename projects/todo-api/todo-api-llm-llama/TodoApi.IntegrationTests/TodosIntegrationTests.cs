// File: TodosIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

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

        private async Task<HttpResponseMessage> AuthenticateUserAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users/token", request);
        }

        private async Task<string> CreateUserAndGetTokenAsync(string username, string password)
        {
            await _client.PostAsJsonAsync("/users", new { username = username, password = password });
            var response = await AuthenticateUserAsync(username, password);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            return body_token;
        }

        private async Task<HttpResponseMessage> CreateTodoAsync(string token, string title)
        {
            var request = new
            {
                title = title
            };

            var requestBody = JsonContent.Create(request);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = requestBody
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> UpdateTodoAsync(string token, int id, string title, bool isComplete)
        {
            var request = new
            {
                title = title,
                isComplete = isComplete
            };

            var requestBody = JsonContent.Create(request);

            var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"/todos/{id}")
            {
                Content = requestBody
            };

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<int> CreateTodoAndGetIdAsync(string token, string title)
        {
            var response = await CreateTodoAsync(token, title);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            return body_id;
        }

        [Fact]
        public async Task TC023_Create_Todo_When_Valid_Data_Returns_Created()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username13", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, "Something todo in the future");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_title = body["title"].AsValue().GetValue<string>();
            var body_isComplete = body["isComplete"].AsValue().GetValue<bool>();
            Assert.True(body_id > 0);
            Assert.Equal("Something todo in the future", body_title);
            Assert.Equal(false, body_isComplete);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_Todo_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username14", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, null);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Todo_When_Title_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username15", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, "");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Todo_When_Title_Too_Short_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username16", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, "a");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Create_Todo_When_Title_Too_Long_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username17", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, new string('a', 257));

            // assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Todo_When_Title_Has_Minimum_Size_Returns_Created()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username18", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, "a");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_title = body["title"].AsValue().GetValue<string>();
            var body_isComplete = body["isComplete"].AsValue().GetValue<bool>();
            Assert.True(body_id > 0);
            Assert.Equal("a", body_title);
            Assert.Equal(false, body_isComplete);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Todo_When_Title_Has_Maximum_Size_Returns_Created()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username19", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, new string('a', 256));

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_title = body["title"].AsValue().GetValue<string>();
            var body_isComplete = body["isComplete"].AsValue().GetValue<bool>();
            Assert.True(body_id > 0);
            Assert.Equal(new string('a', 256), body_title);
            Assert.Equal(false, body_isComplete);
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Update_Todo_When_Exists_And_Valid_Token_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username20", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await UpdateTodoAsync(valid_token, todo_id, "Something todo in the future updated", true);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_title = body["title"].AsValue().GetValue<string>();
            var body_isComplete = body["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todo_id, body_id);
            Assert.Equal("Something todo in the future updated", body_title);
            Assert.Equal(true, body_isComplete);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Update_Todo_When_Not_Exists_Returns_NotFound()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username21", "P@ssw0rd");

            // act
            var response = await UpdateTodoAsync(valid_token, 999999, "Something todo in the future updated", true);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Update_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username22", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await UpdateTodoAsync("invalid_token", todo_id, "Something todo in the future updated", true);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Update_Todo_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username23", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await UpdateTodoAsync(null, todo_id, "Something todo in the future updated", true);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private async Task<HttpResponseMessage> DeleteTodoAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{id}");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> GetTodoAsync(string token, int id)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"/todos/{id}");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        private async Task<HttpResponseMessage> ListTodosAsync(string token)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "/todos");

            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await _client.SendAsync(requestMessage);
        }

        [Fact]
        public async Task TC030_Delete_Todo_When_Exists_And_Valid_Token_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username24", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await DeleteTodoAsync(valid_token, todo_id);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Delete_Todo_When_Not_Exists_Returns_NotFound()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username25", "P@ssw0rd");

            // act
            var response = await DeleteTodoAsync(valid_token, 999999);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Delete_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username26", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await DeleteTodoAsync("invalid_token", todo_id);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Delete_Todo_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username27", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await DeleteTodoAsync(null, todo_id);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Get_Todo_When_Exists_And_Valid_Token_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username28", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await GetTodoAsync(valid_token, todo_id);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_title = body["title"].AsValue().GetValue<string>();
            var body_isComplete = body["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todo_id, body_id);
            Assert.Equal("Something todo in the future", body_title);
            Assert.Equal(false, body_isComplete);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Get_Todo_When_Not_Exists_Returns_NotFound()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username29", "P@ssw0rd");

            // act
            var response = await GetTodoAsync(valid_token, 999999);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Get_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username30", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await GetTodoAsync("invalid_token", todo_id);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Get_Todo_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username31", "P@ssw0rd");
            var todo_id = await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await GetTodoAsync(null, todo_id);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC030_List_Todos_When_Exists_And_Valid_Token_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username32", "P@ssw0rd");
            await CreateTodoAndGetIdAsync(valid_token, "Something todo in the future");

            // act
            var response = await ListTodosAsync(valid_token);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.True(body.Count > 0);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC047_List_Todos_When_Invalid_Token_Returns_Unauthorized()
        {
            // act
            var response = await ListTodosAsync("invalid_token");

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC048_List_Todos_When_Without_Token_Returns_Unauthorized()
        {
            // act
            var response = await ListTodosAsync(null);

            // assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}