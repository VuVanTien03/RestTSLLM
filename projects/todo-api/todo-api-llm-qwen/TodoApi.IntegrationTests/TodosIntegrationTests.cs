// File: TodosIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class Todos1IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public Todos1IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<string> CreateUserAndGetTokenAsync(string username, string password)
        {
            var createUserResponse = await CreateUserAsync(username, password);
            var tokenResponse = await CreateTokenAsync(username, password);

            var responseBody = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();
            return responseBody["token"].AsValue().GetValue<string>();
        }

        private async Task<HttpResponseMessage> CreateUserAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users", request);
        }

        private async Task<HttpResponseMessage> CreateTokenAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users/token", request);
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

        private async Task<int> CreateTodoAndGetIdAsync(string token, string title)
        {
            var response = await CreateTodoAsync(token, title);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            return responseBody["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> UpdateTodoAsync(string token, int id, string title, bool? isComplete)
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

        [Fact]
        public async Task TC027_Create_Todo_When_Valid_Data_Returns_Created()
        {
            // arrange
            string username = "validUsername121";
            string password = "P@ssw0rd121";
            string validTitle = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await CreateTodoAsync(token, validTitle);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var responseBodyId = responseBody["id"].AsValue().GetValue<int>();
            var responseBodyTitle = responseBody["title"].AsValue().GetValue<string>();
            var responseBodyIsComplete = responseBody["isComplete"].AsValue().GetValue<bool>();
            Assert.True(responseBodyId > 0);
            Assert.Equal(validTitle, responseBodyTitle);
            Assert.Equal(false, responseBodyIsComplete);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Todo_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername122";
            string password = "P@ssw0rd122";
            string nullTitle = null;
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await CreateTodoAsync(token, nullTitle);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Todo_When_Title_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername123";
            string password = "P@ssw0rd123";
            string emptyTitle = "";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await CreateTodoAsync(token, emptyTitle);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Create_Todo_When_Title_Too_Short_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername124";
            string password = "P@ssw0rd124";
            string shortTitle = "";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await CreateTodoAsync(token, shortTitle);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Create_Todo_When_Title_Too_Long_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername125";
            string password = "P@ssw0rd125";
            string longTitle = "A long title that exceeds the maximum length allowed for a todo title in this application to trigger the validation error that was configured in the application to ensure titles have a maximum length of 256 characters  long title that exceeds the maximum al";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await CreateTodoAsync(token, longTitle);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Create_Todo_When_Title_Has_Minimum_Size_Returns_Created()
        {
            // arrange
            string username = "validUsername126";
            string password = "P@ssw0rd126";
            string minSizeTitle = "?";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await CreateTodoAsync(token, minSizeTitle);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var responseBodyId = responseBody["id"].AsValue().GetValue<int>();
            var responseBodyTitle = responseBody["title"].AsValue().GetValue<string>();
            var responseBodyIsComplete = responseBody["isComplete"].AsValue().GetValue<bool>();
            Assert.True(responseBodyId > 0);
            Assert.Equal(minSizeTitle, responseBodyTitle);
            Assert.Equal(false, responseBodyIsComplete);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Create_Todo_When_Title_Has_Maximum_Size_Returns_Created()
        {
            // arrange
            string username = "validUsername127";
            string password = "P@ssw0rd127";
            string maxSizeTitle = "A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the.lazy dog.";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await CreateTodoAsync(token, maxSizeTitle);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var responseBodyId = responseBody["id"].AsValue().GetValue<int>();
            var responseBodyTitle = responseBody["title"].AsValue().GetValue<string>();
            var responseBodyIsComplete = responseBody["isComplete"].AsValue().GetValue<bool>();
            Assert.True(responseBodyId > 0);
            Assert.Equal(maxSizeTitle, responseBodyTitle);
            Assert.Equal(false, responseBodyIsComplete);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Create_Todo_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string invalidToken = "invalidtoken";
            string title = "Important Todo";

            // act
            var response = await CreateTodoAsync(invalidToken, title);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Create_Todo_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            string title = "Important Todo";

            // act
            var response = await CreateTodoAsync(null, title);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            string username = "validUsername128";
            string password = "P@ssw0rd128";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = "Buy Milk Later";
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var responseBodyId = responseBody["id"].AsValue().GetValue<int>();
            var responseBodyNewTitle = responseBody["title"].AsValue().GetValue<string>();
            var responseBodyIsComplete = responseBody["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todoId, responseBodyId);
            Assert.Equal(newTitle, responseBodyNewTitle);
            Assert.Equal(isComplete, responseBodyIsComplete);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC042_Update_Todo_When_Title_Is_Null_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername129";
            string password = "P@ssw0rd129";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = null;
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC043_Update_Todo_When_Title_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername130";
            string password = "P@ssw0rd130";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = "";
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC044_Update_Todo_When_Title_Too_Short_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername131";
            string password = "P@ssw0rd131";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = "";
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC045_Update_Todo_When_Title_Too_Long_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername132";
            string password = "P@ssw0rd132";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = "A long title that exceeds the maximum length allowed for a todo title in this application to trigger the validation error that was configured in the application to ensure titles have a maximum length of 256 characters  long title that exceeds the maximum al";
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC046_Update_Todo_When_Title_Has_Minimum_Size_Returns_OK()
        {
            // arrange
            string username = "validUsername133";
            string password = "P@ssw0rd133";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = "?";
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var responseBodyId = responseBody["id"].AsValue().GetValue<int>();
            var responseBodyTitle = responseBody["title"].AsValue().GetValue<string>();
            var responseBodyIsComplete = responseBody["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todoId, responseBodyId);
            Assert.Equal(newTitle, responseBodyTitle);
            Assert.Equal(isComplete, responseBodyIsComplete);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC047_Update_Todo_When_Title_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            string username = "validUsername134";
            string password = "P@ssw0rd134";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = "A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the lazy dog. A quick brown fox jumps over the.lazy dog.";
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var responseBodyId = responseBody["id"].AsValue().GetValue<int>();
            var responseBodyTitle = responseBody["title"].AsValue().GetValue<string>();
            var responseBodyIsComplete = responseBody["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todoId, responseBodyId);
            Assert.Equal(newTitle, responseBodyTitle);
            Assert.Equal(isComplete, responseBodyIsComplete);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC048_Update_Todo_When_Missing_IsComplete_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername135";
            string password = "P@ssw0rd135";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = "Buy Milk Later";
            bool? isComplete = null;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC049_Update_Todo_When_Missing_Title_Returns_BadRequest()
        {
            // arrange
            string username = "validUsername136";
            string password = "P@ssw0rd136";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            string newTitle = null;
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(token, todoId, newTitle, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC050_Update_Todo_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string invalidToken = "invalidtoken";
            string title = "Buy Milk Later";
            int todoId = 1;
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(invalidToken, todoId, title, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC051_Update_Todo_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
            string title = "Buy Milk Later";
            int todoId = 1;
            bool isComplete = true;

            // act
            var response = await UpdateTodoAsync(null, todoId, title, isComplete);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

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
            var createUserResponse = await CreateUserAsync(username, password);
            var tokenResponse = await CreateTokenAsync(username, password);

            var responseBody = await tokenResponse.Content.ReadFromJsonAsync<JsonObject>();
            return responseBody["token"].AsValue().GetValue<string>();
        }

        private async Task<HttpResponseMessage> CreateUserAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users", request);
        }

        private async Task<HttpResponseMessage> CreateTokenAsync(string username, string password)
        {
            var request = new
            {
                username = username,
                password = password
            };

            return await _client.PostAsJsonAsync("/users/token", request);
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

        private async Task<int> CreateTodoAndGetIdAsync(string token, string title)
        {
            var response = await CreateTodoAsync(token, title);
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            return responseBody["id"].AsValue().GetValue<int>();
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
            var request = new HttpRequestMessage(HttpMethod.Get, $"/todos");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        [Fact]
        public async Task TC023_List_Todos_When_No_Todos_Returns_OK()
        {
            // arrange
            string username = "validUsername150";
            string password = "P@ssw0rd150";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2

            // act
            var response = await ListTodosAsync(token);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<List<JsonObject>>();
            Assert.Empty(responseBody);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC024_List_Todos_When_There_Are_Todos_Returns_OK()
        {
            // arrange
            string username = "validUsername151";
            string password = "P@ssw0rd151";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            // act
            var response = await ListTodosAsync(token);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<List<JsonObject>>();
            Assert.NotEmpty(responseBody);
            Assert.Equal(todoId, responseBody[0]["id"].AsValue().GetValue<int>());
            Assert.Equal(title, responseBody[0]["title"].AsValue().GetValue<string>());
            Assert.Equal(false, responseBody[0]["isComplete"].AsValue().GetValue<bool>());
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC025_List_Todos_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
            string invalidToken = "invalidtoken";

            // act
            var response = await ListTodosAsync(invalidToken);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC026_List_Todos_When_Without_Token_Returns_Unauthorized()
        {
            // arrange

            // act
            var response = await ListTodosAsync(null);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Get_Todo_When_Valid_Data_Returns_OK()
        {
            // arrange
            string username = "validUsername160";
            string password = "P@ssw0rd160";
            string title = "Buy Milk";
            var token = await CreateUserAndGetTokenAsync(username, password); // precondition 1 and 2
            int todoId = await CreateTodoAndGetIdAsync(token, title); // precondition 3

            // act
            var response = await GetTodoAsync(token, todoId);

            // assert
            var responseBody = await response.Content.ReadFromJsonAsync<JsonObject>();
            var responseBodyId = responseBody["id"].AsValue().GetValue<int>();
            var responseBodyTitle = responseBody["title"].AsValue().GetValue<string>();
            var responseBodyIsComplete = responseBody["isComplete"].AsValue().GetValue<bool>();
            Assert.Equal(todoId, responseBodyId);
            Assert.Equal(title, responseBodyTitle);
            Assert.Equal(false, responseBodyIsComplete);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}

