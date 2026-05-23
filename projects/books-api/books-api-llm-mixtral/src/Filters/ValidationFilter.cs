using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Books.Api.Docker.Filters
{
    public class ValidationFilter<T>(IValidator<T> validator) : IEndpointFilter where T : class
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (context.Arguments.FirstOrDefault(x => x is T) is not T model)
            {
                var result = new ErrorResponse { Error = "Invalid Request" };
                return Results.Json(result, statusCode: 400);
            }

            var validationResult = await validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(m => m.ErrorMessage).FirstOrDefault());
                var result = new ErrorResponse { Error = errors.FirstOrDefault().Value ?? "Unknow error" };
                return Results.Json(result, statusCode: 400);
            }

            return await next(context);
        }
    }
}
