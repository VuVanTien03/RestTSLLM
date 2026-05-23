## Todo application with ASP.NET Core

This is a Todo application that features:
- [**TodoApi**](TodoApi) - An ASP.NET Core REST API backend using minimal APIs

![image](https://user-images.githubusercontent.com/95136/204161352-bc54ccb7-32cf-49ba-a6f7-f46d0f2d204f.png)

It showcases:
- Minimal APIs
- Using EntityFramework and SQLite for data access
- OpenAPI
- User management with ASP.NET Core Identity
- JWT authentication
- Proxying requests from the front end application server using YARP's IHttpForwarder
- Rate Limiting
- Writing integration tests for your REST API

## Prerequisites

### .NET
1. [Install .NET 7](https://dotnet.microsoft.com/en-us/download)

### Using the API standalone
The Todo REST API can run standalone as well. You can run the [TodoApi](TodoApi) project and make requests to various endpoints using the Swagger UI (or a client of your choice):

<img width="1200" alt="image" src="https://user-images.githubusercontent.com/95136/204315486-86d25a5f-1164-467a-9891-827343b9f0e8.png">

Before executing any requests, you need to create a user and get an auth token.

1. To create a new user, run the application and POST a JSON payload to `/users` endpoint:

    ```json
    {
      "username": "myuser",
      "password": "<put a password here>"
    }
    ```

1. You should be able to use this token to make authenticated requests to the todo endpoints.
