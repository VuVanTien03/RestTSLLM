// File: TodosIntegrationTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
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
            var createUserRequest = new
            {
                username = username,
                password = password
            };
            await _client.PostAsJsonAsync("/users", createUserRequest);
            var createTokenResponse = await _client.PostAsJsonAsync("/users/token", createUserRequest);
            createTokenResponse.EnsureSuccessStatusCode();
            var body = await createTokenResponse.Content.ReadFromJsonAsync<JsonObject>();
            return body?["token"]?.AsValue()?.GetValue<string>();
        }

        private async Task<HttpResponseMessage> CreateTodoAsync(string token, string title)
        {
            var requestBody = new { title = title };
            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> UpdateTodoAsync(string token, int id, string title, bool? isComplete)
        {
            var requestBody = new { title = title, isComplete = isComplete };
            var request = new HttpRequestMessage(HttpMethod.Put, $"/todos/{id}")
            {
                Content = JsonContent.Create(requestBody)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }

        private async Task<int> CreateTodoAndGetIdAsync(string token, string title)
        {
            var response = await CreateTodoAsync(token, title);
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body?["id"]?.AsValue()?.GetValue<int>() ?? 0;
        }


        [Fact]
        public async Task TC026_Create_Todo_With_Valid_Data_Returns_Created()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_15", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, "My first todo");

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body?["id"]?.AsValue()?.GetValue<int>() > 0);
            Assert.Equal("My first todo", body?["title"]?.AsValue()?.GetValue<string>());
            Assert.False(body?["isComplete"]?.AsValue()?.GetValue<bool>() ?? true);
        }

        [Fact]
        public async Task TC027_Create_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // act
            var response = await CreateTodoAsync("invalid_token", "My first todo");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Todo_Without_Token_Returns_Unauthorized()
        {
            // act
            var response = await CreateTodoAsync(null, "My first todo");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Todo_With_Null_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_16", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Create_Todo_With_Empty_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_17", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Create_Todo_With_Title_Too_Short_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_18", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, "a");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Create_Todo_With_Title_Too_Long_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_19", "P@ssw0rd");
            string longTitle = new string('a', 257);

            // act
            var response = await CreateTodoAsync(token, longTitle);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Create_Todo_With_Minimum_Title_Length_Returns_Created()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_20", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(token, "a");

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body?["id"]?.AsValue()?.GetValue<int>() > 0);
            Assert.Equal("a", body?["title"]?.AsValue()?.GetValue<string>());
            Assert.False(body?["isComplete"]?.AsValue()?.GetValue<bool>() ?? true);
        }

        [Fact]
        public async Task TC034_Create_Todo_With_Maximum_Title_Length_Returns_Created()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_21", "P@ssw0rd");
            string maxTitle = new string('a', 256);

            // act
            var response = await CreateTodoAsync(token, maxTitle);

            // assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.True(body?["id"]?.AsValue()?.GetValue<int>() > 0);
            Assert.Equal(maxTitle, body?["title"]?.AsValue()?.GetValue<string>());
            Assert.False(body?["isComplete"]?.AsValue()?.GetValue<bool>() ?? true);
        }

        [Fact]
        public async Task TC039_Update_Todo_With_Valid_Data_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_22", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync(token, todoId, "My updated todo", true);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(todoId, body?["id"]?.AsValue()?.GetValue<int>());
            Assert.Equal("My updated todo", body?["title"]?.AsValue()?.GetValue<string>());
            Assert.True(body?["isComplete"]?.AsValue()?.GetValue<bool>() ?? false);
        }

        [Fact]
        public async Task TC040_Update_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_23", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync("invalid_token", todoId, "My updated todo", true);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_Todo_Without_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_24", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync(null, todoId, "My updated todo", true);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Todo_With_Non_Existing_ID_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_25", "P@ssw0rd");

            // act
            var response = await UpdateTodoAsync(token, 9999999, "My updated todo", true);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Todo_With_Null_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_26", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync(token, todoId, null, true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Update_Todo_With_Empty_Title_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_27", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync(token, todoId, "", true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Update_Todo_With_Title_Too_Short_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_28", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync(token, todoId, "a", true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Update_Todo_With_Title_Too_Long_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_29", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");
            string longTitle = new string('a', 257);

            // act
            var response = await UpdateTodoAsync(token, todoId, longTitle, true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Update_Todo_With_Minimum_Title_Length_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_30", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync(token, todoId, "a", true);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(todoId, body?["id"]?.AsValue()?.GetValue<int>());
            Assert.Equal("a", body?["title"]?.AsValue()?.GetValue<string>());
            Assert.True(body?["isComplete"]?.AsValue()?.GetValue<bool>() ?? false);
        }

        [Fact]
        public async Task TC048_Update_Todo_With_Maximum_Title_Length_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_31", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");
            string maxTitle = new string('a', 256);

            // act
            var response = await UpdateTodoAsync(token, todoId, maxTitle, true);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(todoId, body?["id"]?.AsValue()?.GetValue<int>());
            Assert.Equal(maxTitle, body?["title"]?.AsValue()?.GetValue<string>());
            Assert.True(body?["isComplete"]?.AsValue()?.GetValue<bool>() ?? false);
        }

        [Fact]
        public async Task TC049_Update_Todo_With_Null_isComplete_Returns_BadRequest()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_32", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await UpdateTodoAsync(token, todoId, "My updated todo", null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private async Task<HttpResponseMessage> GetTodoAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/todos/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> DeleteTodoAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> ListTodosAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/todos");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _client.SendAsync(request);
        }

        [Fact]
        public async Task TC023_List_Todos_With_Valid_Token_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_33", "P@ssw0rd");

            // act
            var response = await ListTodosAsync(token);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            Assert.NotEmpty(body); //Check if it's not an empty array
        }

        [Fact]
        public async Task TC024_List_Todos_With_Invalid_Token_Returns_Unauthorized()
        {
            // act
            var response = await ListTodosAsync("invalid_token");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC025_List_Todos_Without_Token_Returns_Unauthorized()
        {
            // act
            var response = await ListTodosAsync(null);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Get_Todo_With_Valid_Data_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_34", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await GetTodoAsync(token, todoId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.Equal(todoId, body?["id"]?.AsValue()?.GetValue<int>());
            Assert.Equal("My todo", body?["title"]?.AsValue()?.GetValue<string>());
            Assert.False(body?["isComplete"]?.AsValue()?.GetValue<bool>() ?? true);
        }

        [Fact]
        public async Task TC036_Get_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_35", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await GetTodoAsync("invalid_token", todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Get_Todo_Without_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_36", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await GetTodoAsync(null, todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Get_Todo_With_Non_Existing_ID_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_37", "P@ssw0rd");

            // act
            var response = await GetTodoAsync(token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC050_Delete_Todo_With_Valid_Data_Returns_OK()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_38", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await DeleteTodoAsync(token, todoId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Delete_Todo_With_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_39", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await DeleteTodoAsync("invalid_token", todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC052_Delete_Todo_Without_Token_Returns_Unauthorized()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_40", "P@ssw0rd");
            var todoId = await CreateTodoAndGetIdAsync(token, "My todo");

            // act
            var response = await DeleteTodoAsync(null, todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC053_Delete_Todo_With_Non_Existing_ID_Returns_NotFound()
        {
            // arrange
            var token = await CreateUserAndGetTokenAsync("valid_username_41", "P@ssw0rd");

            // act
            var response = await DeleteTodoAsync(token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
