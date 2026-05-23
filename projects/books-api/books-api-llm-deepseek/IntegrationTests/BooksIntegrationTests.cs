using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests
{
    public class BookEndpointsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public BookEndpointsIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }
        
        private async Task<int> CreateValidBookAsync()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Unique Title " + Guid.NewGuid(),
                isbn = "1234567890",
                description = "Valid Description",
                author = "Author Name"
            });

            var book = await response.Content.ReadFromJsonAsync<JsonElement>();
            return book.GetProperty("id").GetInt32();
        }

        [Fact]
        public async Task TC001_Get_all_books_when_empty_returns_OK()
        {
            var response = await _client.GetAsync("/books");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("[]", content);
        }

        [Fact]
        public async Task TC002_Get_all_books_with_data_returns_OK()
        {
            var bookId = await CreateValidBookAsync();

            var response = await _client.GetAsync("/books");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var books = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
            Assert.True(books.Count > 0);
        }

        [Fact]
        public async Task TC003_Create_book_with_valid_data_returns_OK()
        {
            var request = new
            {
                title = "New Book Title",
                isbn = "1234567890",
                description = "Valid description",
                author = "Author Name"
            };

            var response = await _client.PostAsJsonAsync("/books", request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var book = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(book.GetProperty("id").GetInt32() > 0);
            Assert.Equal(request.title, book.GetProperty("title").GetString());
        }

        [Fact]
        public async Task TC004_Create_book_missing_title_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                isbn = "123456",
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_book_with_empty_title_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "",
                isbn = "123456",
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_book_with_minimum_title_length_returns_OK()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "A",
                isbn = "123456",
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_book_missing_ISBN_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_book_with_empty_ISBN_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "",
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_book_with_minimum_ISBN_length_returns_OK()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "1",
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_book_missing_description_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_book_with_empty_description_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                description = "",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_book_with_minimum_description_length_returns_OK()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                description = "D",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_book_missing_author_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                description = "Descrição válida"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_book_with_empty_author_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                description = "Descrição válida",
                author = ""
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_book_with_minimum_author_length_returns_OK()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                description = "Descrição válida",
                author = "A"
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Get_existing_book_by_ID_returns_OK()
        {
            var bookId = await CreateValidBookAsync();

            var response = await _client.GetAsync($"/books/{bookId}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Get_non_existent_book_returns_NotFound()
        {
            var response = await _client.GetAsync("/books/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Update_book_with_valid_data_returns_NoContent()
        {
            var bookId = await CreateValidBookAsync();

            var updateRequest = new
            {
                title = "Updated Title",
                isbn = "654321",
                description = "Updated Description",
                author = "New Author"
            };

            var response = await _client.PutAsJsonAsync($"/books/{bookId}", updateRequest);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Update_non_existent_book_returns_NotFound()
        {
            var response = await _client.PutAsJsonAsync("/books/999999", new
            {
                title = "Título",
                isbn = "123456",
                description = "Descrição",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Delete_existing_book_returns_NoContent()
        {
            var bookId = await CreateValidBookAsync();

            var response = await _client.DeleteAsync($"/books/{bookId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Delete_non_existent_book_returns_NotFound()
        {
            var response = await _client.DeleteAsync("/books/999999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC022_Create_book_with_null_title_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = (string)null,
                isbn = "123456",
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Create_book_with_null_ISBN_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = (string)null,
                description = "Descrição válida",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC024_Create_book_with_null_description_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                description = (string)null,
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_book_with_null_author_returns_BadRequest()
        {
            var response = await _client.PostAsJsonAsync("/books", new
            {
                title = "Título válido",
                isbn = "123456",
                description = "Descrição válida",
                author = (string)null
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Update_book_with_empty_title_returns_BadRequest()
        {
            var bookId = await CreateValidBookAsync();

            var response = await _client.PutAsJsonAsync($"/books/{bookId}", new
            {
                title = "",
                isbn = "654321",
                description = "Descrição",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Update_book_with_long_valid_title_returns_NoContent()
        {
            var bookId = await CreateValidBookAsync();
            var longTitle = new string('A', 1000);

            var response = await _client.PutAsJsonAsync($"/books/{bookId}", new
            {
                title = longTitle,
                isbn = "123456",
                description = "Descrição",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Update_book_with_invalid_ID_format_returns_BadRequest()
        {
            var response = await _client.PutAsJsonAsync("/books/abc", new
            {
                title = "Título",
                isbn = "123456",
                description = "Descrição",
                author = "Autor"
            });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
