```csharp
// File: UsersIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;

namespace PeopleAPI.IntegrationTests
{
    public class UsersIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public UsersIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreateUserAsync(string nickname, string password)
        {
            var request = new
            {
                nickname = nickname,
                password = password
            };

            return await _client.PostAsJsonAsync("/users", request);
        }

        private async Task<HttpResponseMessage> CreateTokenAsync(string nickname, string password)
        {
            var request = new
            {
                nickname = nickname,
                password = password
            };

            return await _client.PostAsJsonAsync("/users/token", request);
        }

        [Fact]
        public async Task TC001_Create_User_When_Valid_Data_Returns_OK()
        {
			// arrange
			string nickname = "validNickname1";  // valid nickname 
            string password = "ValidPass1"; // valid password 
			
            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC002_Create_User_When_Nickname_Already_Exists_Returns_BadRequest()
        {
            // arrange
			string nickname = "existingNick"; // duplicated nickname 
            string password = "ValidPass1";
            var response1 = await CreateUserAsync(nickname, password); // precondition 1

            // act
            var response2 = await CreateUserAsync(nickname, password); 

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }

        [Fact]
        public async Task TC003_Create_User_When_Invalid_Nickname_Format_Returns_BadRequest()
        {
			string nickname = "invalid!Nick"; // invalid nickname 
            string password = "ValidPass1";
			
            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC004_Create_User_When_Nickname_Is_Null_Returns_BadRequest()
        {
			string nickname = null; // null nickname 
            string password = "ValidPass1";
			
            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC005_Create_User_When_Nickname_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
            string nickname = ""; // empty nickname 
            string password = "ValidPass1";
			
			// act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC006_Create_User_When_Nickname_Too_Short_Returns_BadRequest()
        {
            // arrange
            string nickname = "invalidNi"; // nickname length: 9
            string password = "ValidPass1";

            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC007_Create_User_When_Nickname_Too_Long_Returns_BadRequest()
        {
            // arrange
            string nickname = "validNick1abcabcabcdabcabcabcd123"; // nickname length: 33
			string password = "ValidPass1";

            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC008_Create_User_When_Nickname_Has_Minimumm_Size_Returns_OK()
        {
            // arrange
            string nickname = "validNick0"; // nickname length: 10
			string password = "ValidPass1";

            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC009_Create_User_When_Nickname_Has_Maximum_Size_Returns_OK()
        {
            // arrange
            string nickname = "validNick1abcabcabcdabcabcabcd12"; // nickname length: 32
			string password = "ValidPass1";

            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC010_Create_User_When_Password_Is_Null_Returns_OK()
        {
			// arrange
            string nickname = "validNick1"; 
			string password = null; // null password
			
            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC011_Create_User_When_Password_Is_Empty_String_Returns_OK()
        {
			// arrange
            string nickname = "validNick2"; 
			string password = ""; // empty password
			
            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC012_Create_User_When_Password_Too_Short_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick3"; 
			string password = "Abcd1"; // valid password with lenght 5

            // act
			var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC013_Create_User_When_Password_Too_Long_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick4"; 
			string password = "Ab1Abc2bc2bc2bc2bc2bc"; // valid password with lenght 21

            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC014_Create_User_When_Password_Has_Minimum_Size_Returns_OK()
        {
            // arrange
			string nickname = "validNick5"; 
			string password = "Abcde1"; // valid password with lenght 6

            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC015_Create_User_When_Password_Has_Maximum_Size_Returns_OK()
        {
            // arrange
			string nickname = "validNick6"; 
			string password = "Ab1Abc2bc2bc2bc2bc2b"; // valid password with lenght 20

            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC016_Create_User_When_Password_Missing_Uppercase_Letter_Returns_BadRequest()
        {
			// arrange
			string nickname = "validNick7"; 
			string password = "validpass1"; // password missing uppercase
			
            // act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC017_Create_User_When_Password_Missing_Lowercase_Letter_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick8"; 
			string password = "VALIDPASS1"; // password missing lowercase
			
			// act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC018_Create_User_When_Password_Missing_Digit_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick9"; 
			string password = "ValidPassX"; // password missing digit
			
			// act
            var response = await CreateUserAsync(nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC019_Authenticate_User_When_Valid_Data_Returns_OK()
        {
            // arrange
			string nickname = "validNick10"; 
			string password = "ValidPass1"; 
            await CreateUserAsync(nickname, password); // precondition 1

            // act
            var response = await CreateTokenAsync(nickname, password); 

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_token = body["token"].AsValue().GetValue<string>();
            Assert.True(String.IsNullOrEmpty(body_token));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC020_Create_User_When_Invalid_Password_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick11"; 
			string password = "ValidPass1"; 
			string wrong_password = "WrongPass1"; 
            await CreateUserAsync(nickname, password); // precondition 1

            // act
            var response = await CreateTokenAsync(nickname, wrong_password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC021_Authenticate_User_When_Invalid_Nickname_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick12"; 
			string password = "ValidPass1"; 
			string invalid_nickname = "validNickname12";
            await CreateUserAsync(nickname, password); // precondition 1

            // act
            var response = await CreateTokenAsync(invalid_nickname, password);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
```

```csharp
// File: PeopleIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace PeopleAPI.IntegrationTests
{
    public class PeopleIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public PeopleIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> CreatePersonAsync(string token, string name, int? age)
        {
            var requestBody = new
            {
                name = name,
                age = age
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/people")
            {
                Content = JsonContent.Create(requestBody)
            };

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<int> CreatePersonAndGetIdAsync(string token, string name, int? age)
        {
            var response = await CreatePersonAsync(token, name, age); 

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["id"].AsValue().GetValue<int>();
        }

        private async Task<HttpResponseMessage> GetPersonAsync(string token, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/people/{id}");

            if (token != null)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _client.SendAsync(request);
        }

        private async Task<string> CreateUserAndGetTokenAsync(string nickname, string password)
        {
            var request = new
            {
                nickname = nickname,
                password = password
            };
            await _client.PostAsJsonAsync("/users", request);
            var response = await _client.PostAsJsonAsync("/users/token", request);

            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            return body["token"].AsValue().GetValue<string>();
        }

        [Fact]
        public async Task TC022_Create_Person_When_Valid_Data_Returns_Created()
        {
            // arrange
			string nickname = "validNick13"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; // valid name
			int age = 30; // valid age
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2
            
            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_age = body["age"].AsValue().GetValue<int>();
            Assert.True(body_id > 0);
            Assert.Equal(name, body_name);
            Assert.Equal(age, body_age);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC023_Create_Person_When_Name_Is_Null_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick14"; 
			string password = "ValidPass1"; 
			string name = null; // null name
			int age = 30;
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


        [Fact]
        public async Task TC024_Create_Person_When_Name_Is_Empty_String_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick15"; 
			string password = "ValidPass1"; 
			string name = ""; // empty name
			int age = 30;
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);
            
            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC025_Create_Person_When_Name_Too_Short_Returns_BadRequest()
        {
			// arrange
			string nickname = "validNick16"; 
			string password = "ValidPass1"; 
			string name = ""; // name lenght: 0
			int age = 30;
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC026_Create_Person_When_Name_Too_Long_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick17"; 
			string password = "ValidPass1"; 
			string name = "John Doe abcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcg"; // name lenght: 129
			int age = 30;
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC027_Create_Person_When_Name_Has_Minimum_Size_Returns_Created()
        {
            // arrange
			string nickname = "validNick18"; 
			string password = "ValidPass1"; 
			string name = "J"; // name lenght: 1
			int age = 30;
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_age = body["age"].AsValue().GetValue<int>();
            Assert.True(body_id > 0);
            Assert.Equal(name, body_name);
            Assert.Equal(age, body_age);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC028_Create_Person_When_Name_Has_Maximum_Size_Returns_Created()
        {
            // arrange
			string nickname = "validNick19"; 
			string password = "ValidPass1"; 
			string name = "John Doeabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcgabcabcabcg"; // name lenght: 128
			int age = 30;
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);
			
            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_age = body["age"].AsValue().GetValue<int>();
            Assert.True(body_id > 0);
            Assert.Equal(name, body_name);
            Assert.Equal(age, body_age);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC029_Create_Person_When_Age_Is_Null_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick20"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; 
			int? age = null; // null age
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC030_Create_Person_When_Age_Below_Minimum_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick21"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; 
			int age = -1; // age below minimum
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC031_Create_Person_When_Age_Above_Maximum_Returns_BadRequest()
        {
            // arrange
			string nickname = "validNick22"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; 
			int age = 151; // age above maximum
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);
            
            // assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TC032_Create_Person_When_Age_Has_Minimum_Value_Returns_Created()
        {
            // arrange
			string nickname = "validNick23"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; 
			int age = 0; // minimum accepted age
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_age = body["age"].AsValue().GetValue<int>();
            Assert.True(body_id > 0);
            Assert.Equal(name, body_name);
            Assert.Equal(age, body_age);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC033_Create_Person_When_Age_Has_Maximum_Value_Returns_Created()
        {
            // arrange
			string nickname = "validNick24"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; 
			int age = 150; // maximum accepted age
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await CreatePersonAsync(valid_token, name, age);
			
            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_age = body["age"].AsValue().GetValue<int>();
            Assert.True(body_id > 0);
            Assert.Equal(name, body_name);
            Assert.Equal(age, body_age);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task TC034_Create_Person_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
			string invalid_token = "invalidtoken"; // invalid token
			string name = "John Doe"; 
			int age = 30;

            // act
            var response = await CreatePersonAsync(invalid_token, name, age);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC035_Create_Person_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
			string null_token = null; // null token
			string name = "John Doe"; 
			int age = 30;

            // act
            var response = await CreatePersonAsync(null_token, name, age);

            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC036_Get_Person_When_Valid_Data_Returns_OK()
        {
            // arrange
			string nickname = "validNick25"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; 
			int age = 30; 
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2
            var person_id = await CreatePersonAndGetIdAsync(valid_token, name, age); // precondition 3

            // act
            var response = await GetPersonAsync(valid_token, person_id);

            // assert
            var body = await response.Content.ReadFromJsonAsync<JsonObject>();
            var body_id = body["id"].AsValue().GetValue<int>();
            var body_name = body["name"].AsValue().GetValue<string>();
            var body_age = body["age"].AsValue().GetValue<int>();
            Assert.Equal(person_id, body_id);
            Assert.Equal(name, body_name);
            Assert.Equal(age, body_age);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC037_Get_Person_When_ID_Not_Exists_Returns_NotFound()
        {
            // arrange
			string nickname = "validNick26"; 
			string password = "ValidPass1"; 
			int invalid_person_id = 9999999; // invalid id
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2

            // act
            var response = await GetPersonAsync(valid_token, invalid_person_id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task TC038_Get_Person_When_Token_Is_Invalid_Returns_Unauthorized()
        {
            // arrange
			int person_id = 1; 
            string invalid_token = "invalidtoken"; // invalid token

            // act
            var response = await GetPersonAsync(invalid_token, person_id);
            
            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC039_Get_Person_When_Without_Token_Returns_Unauthorized()
        {
            // arrange
			int person_id = 1; 
            string null_token = null; // null token

            // act
            var response = await GetPersonAsync(null_token, person_id);
			
            // assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TC040_Get_Person_When_Token_From_Other_User_Valid_Data_Returns_NotFound()
        {
            // arrange
			string nickname = "validNick27"; 
			string password = "ValidPass1"; 
			string name = "John Doe"; 
			int age = 30; 
			string other_nickname = "validNick28"; 
			string other_password = "ValidPass1"; 
            var valid_token = await CreateUserAndGetTokenAsync(nickname, password); // precondition 1 and 2
            var person_id = await CreatePersonAndGetIdAsync(valid_token, name, age); // precondition 3
            var other_valid_token = await CreateUserAndGetTokenAsync(other_nickname, other_password); // precondition 4 and 5

            // act
            var response = await GetPersonAsync(other_valid_token, person_id);

            // assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
```

```csharp
// File: AppIntegrationTests.cs

using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace PeopleAPI.IntegrationTests
{
    public class AppIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AppIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        private async Task<HttpResponseMessage> UpdateAppLogoAsync(HttpContent? content)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"/app/logo")
            {
                Content = content
            };

            return await _client.SendAsync(request);
        }

        [Fact]
        public async Task TC040_Update_App_Logo_With_Valid_Content_Type_Returns_OK()
        {
            // arrange
            var property = "file";
            var filename = "somefile.jpg";
            var fileContent = new byte[] { 0x01, 0x02, 0x03 }; // simulate file
            var streamContent = new StreamContent(new MemoryStream(fileContent));
			using var content = new MultipartFormDataContent
			{
				{
					streamContent, property, filename 
				}
			};

            // act
            var response = await UpdateAppLogoAsync(content);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task TC041_Update_App_Logo_With_Invalid_Content_Type_Returns_UnsupportedMediaType()
        {
            // arrange
            var content = JsonContent.Create(new { file = "somefile.jpg" }); // invalid content

            // act
            var response = await UpdateAppLogoAsync(content);

            // assert
            Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
        }
    }
}
```