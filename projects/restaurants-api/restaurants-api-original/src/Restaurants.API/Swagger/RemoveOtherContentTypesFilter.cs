using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Restaurants.API.Swagger
{
    public class RemoveOtherContentTypesFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var response in operation.Responses)
            {
                response.Value.Content = response.Value.Content
                    .Where(c => c.Key == "application/json")
                    .ToDictionary(c => c.Key, c => c.Value);
            }

            if (operation.RequestBody != null &&
                operation.RequestBody.Content.First().Key == "multipart/form-data")
            {
                operation.RequestBody.Content = operation.RequestBody.Content
                    .Where(c => c.Key == "multipart/form-data")
                    .ToDictionary(c => c.Key, c => c.Value);
            }
            else if (operation.RequestBody != null)
            {
                operation.RequestBody.Content = operation.RequestBody.Content
                    .Where(c => c.Key == "application/json")
                    .ToDictionary(c => c.Key, c => c.Value);
            }
        }
    }
}
