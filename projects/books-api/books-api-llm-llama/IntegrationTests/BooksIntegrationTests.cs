// File: BooksIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace IntegrationTests
{
    public class BooksIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public BooksIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateBookAsync(object book)
        {
            return await _client.PostAsJsonAsync("/books", book);
        }

        private async Task<HttpResponseMessage> GetBookByIdAsync(int id)
        {
            return await _client.GetAsync($"/books/{id}");
        }

        private async Task<HttpResponseMessage> UpdateBookAsync(int id, object book)
        {
            return await _client.PutAsJsonAsync($"/books/{id}", book);
        }

        private async Task<HttpResponseMessage> DeleteBookByIdAsync(int id)
        {
            return await _client.DeleteAsync($"/books/{id}");
        }

        [Fact]
        public async Task TC001_Get_All_Books_When_Exist_Returns_OK()
        {
            // arrange

            // act
            var response = await _client.GetAsync("/books");

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Get_All_Books_When_None_Exist_Returns_Empty_Array()
        {
            // arrange

            // act
            var response = await _client.GetAsync("/books");

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonArray>();
            Assert.Empty(body);
        }

        [Fact]
        public async Task TC003_Create_Book_When_Valid_Data_Returns_OK()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_Book_When_Invalid_Title_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = (string)null,
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_Book_When_Invalid_Title_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_Book_When_Invalid_Title_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "   ",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_Book_When_Invalid_ISBN_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = (string)null,
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_Book_When_Invalid_ISBN_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_Book_When_Invalid_ISBN_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "   ",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_Book_When_Invalid_Description_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = (string)null,
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_Book_When_Invalid_Description_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_Book_When_Invalid_Description_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "   ",
                author = "autor válido"
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_Book_When_Invalid_Author_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = (string)null
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_Book_When_Invalid_Author_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = ""
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_Book_When_Invalid_Author_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "   "
            };

            // act
            var response = await CreateBookAsync(request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Get_Book_By_Id_When_Exist_Returns_OK()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();

            // act
            response = await GetBookByIdAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Get_Book_By_Id_When_None_Exist_Returns_Not_Found()
        {
            // arrange

            // act
            var response = await GetBookByIdAsync(999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Update_Book_When_Valid_Data_Returns_No_Content()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "novo título válido",
                isbn = "novo isbn válido",
                description = "nova descrição válida",
                author = "novo autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Update_Book_When_Invalid_Title_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = (string)null,
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Update_Book_When_Invalid_Title_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Update_Book_When_Invalid_Title_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "   ",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Update_Book_When_Invalid_ISBN_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = (string)null,
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Update_Book_When_Invalid_ISBN_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Update_Book_When_Invalid_ISBN_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "   ",
                description = "descrição válida",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Update_Book_When_Invalid_Description_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = (string)null,
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Update_Book_When_Invalid_Description_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Update_Book_When_Invalid_Description_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "   ",
                author = "autor válido"
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Update_Book_When_Invalid_Author_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = (string)null
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Update_Book_When_Invalid_Author_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = ""
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Update_Book_When_Invalid_Author_Returns_Bad_Request()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();
            request = new
            {
                title = "título válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "   "
            };

            // act
            response = await UpdateBookAsync(id, request);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Delete_Book_By_Id_When_Exist_Returns_No_Content()
        {
            // arrange
            var request = new
            {
                title = "livro válido",
                isbn = "isbn válido",
                description = "descrição válida",
                author = "autor válido"
            };
            var response = await CreateBookAsync(request);
            var id = (await response.Content.ReadFromJsonAsync<JsonObject>())["id"].AsValue().GetValue<int>();

            // act
            response = await DeleteBookByIdAsync(id);

            // assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Delete_Book_By_Id_When_None_Exist_Returns_Not_Found()
        {
            // arrange

            // act
            var response = await DeleteBookByIdAsync(999999);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}