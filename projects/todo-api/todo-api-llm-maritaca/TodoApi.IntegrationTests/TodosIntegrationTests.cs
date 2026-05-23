// File: TodosIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
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

        private async Task<HttpResponseMessage> CreateTodoAsync(string token, string title)
        {
            var requestBody = new
            {
                title = title
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/todos")
            {
                Content = JsonContent.Create(requestBody)
            };

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> UpdateTodoAsync(string token, int id, string title, bool isComplete)
        {
            var requestBody = new
            {
                title = title,
                isComplete = isComplete
            };

            var request = new HttpRequestMessage(HttpMethod.Put, $"/todos/{id}")
            {
                Content = JsonContent.Create(requestBody)
            };

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
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

        [Fact]
        public async Task TC027_Create_Todo_When_Valid_Data_Returns_Created()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username10", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Todo_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username11", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, null);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Todo_When_Title_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username12", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, "");

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Create_Todo_When_Title_Too_Long_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username13", "P@ssw0rd");
            var title = new string('a', 257);

            // act
            var response = await CreateTodoAsync(valid_token, title);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Create_Todo_When_Title_Has_Minimum_Size_Returns_Created()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username14", "P@ssw0rd");

            // act
            var response = await CreateTodoAsync(valid_token, "a");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Create_Todo_When_Title_Has_Maximum_Size_Returns_Created()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username15", "P@ssw0rd");
            var title = new string('a', 256);

            // act
            var response = await CreateTodoAsync(valid_token, title);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Update_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username16", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(valid_token, todoId, "Algo atualizado para fazer no futuro", true);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_Todo_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username17", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(valid_token, todoId, null, true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Todo_When_Title_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username18", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(valid_token, todoId, "", true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Todo_When_Title_Too_Long_Returns_BadRequest()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username19", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();
            var title = new string('a', 257);

            // act
            var response = await UpdateTodoAsync(valid_token, todoId, title, true);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Update_Todo_When_Title_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username20", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await UpdateTodoAsync(valid_token, todoId, "a", true);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Update_Todo_When_Title_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username21", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();
            var title = new string('a', 256);

            // act
            var response = await UpdateTodoAsync(valid_token, todoId, title, true);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private async Task<HttpResponseMessage> DeleteTodoAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/todos/{id}");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> GetTodoAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/todos/{id}");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> ListTodosAsync(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/todos");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        [Fact]
        public async Task TC050_Delete_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username22", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync(valid_token, todoId);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Delete_Todo_When_ID_Not_Exists_Returns_NotFound()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username23", "P@ssw0rd");

            // act
            var response = await DeleteTodoAsync(valid_token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC052_Delete_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username24", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync("invalid_token", todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC053_Delete_Todo_When_Missing_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username25", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await DeleteTodoAsync(null, todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC054_Delete_Todo_When_Todo_Does_Not_Belong_To_User_Returns_NotFound()
        {
            // arrange
            var valid_token_1 = await CreateUserAndGetTokenAsync("user1", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token_1, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();
            var valid_token_2 = await CreateUserAndGetTokenAsync("user2", "P@ssw0rd");

            // act
            var response = await DeleteTodoAsync(valid_token_2, todoId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Get_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username26", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync(valid_token, todoId);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Get_Todo_When_ID_Not_Exists_Returns_NotFound()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username27", "P@ssw0rd");

            // act
            var response = await GetTodoAsync(valid_token, 9999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Get_Todo_When_Invalid_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username28", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync("invalid_token", todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Get_Todo_When_Missing_Token_Returns_Unauthorized()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username29", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();

            // act
            var response = await GetTodoAsync(null, todoId);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Get_Todo_When_Todo_Does_Not_Belong_To_User_Returns_NotFound()
        {
            // arrange
            var valid_token_1 = await CreateUserAndGetTokenAsync("user1", "P@ssw0rd");
            var createResponse = await CreateTodoAsync(valid_token_1, "Algo para fazer no futuro");
            var createBody = await createResponse.Content.ReadFromJsonAsync<JsonObject>();
            var todoId = createBody["id"].AsValue().GetValue<int>();
            var valid_token_2 = await CreateUserAndGetTokenAsync("user2", "P@ssw0rd");

            // act
            var response = await GetTodoAsync(valid_token_2, todoId);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC023_List_Todos_When_Valid_Data_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username30", "P@ssw0rd");

            // act
            var response = await ListTodosAsync(valid_token);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC024_List_Todos_When_User_Has_No_Todos_Returns_OK()
        {
            // arrange
            var valid_token = await CreateUserAndGetTokenAsync("valid_username31", "P@ssw0rd");

            // act
            var response = await ListTodosAsync(valid_token);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.NotNull(body);
            Assert.Empty(body);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC025_List_Todos_When_Invalid_Token_Returns_Unauthorized()
        {
            // act
            var response = await ListTodosAsync("invalid_token");

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC026_List_Todos_When_Missing_Token_Returns_Unauthorized()
        {
            // act
            var response = await ListTodosAsync(null);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}