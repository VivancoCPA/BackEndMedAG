using FluentValidation;

namespace SamplVSSkill.Infrastructure.Middleware;

/// <summary>
/// Generic endpoint filter that resolves IValidator&lt;T&gt; from DI and validates the request body.
/// If validation fails, returns 400 with a structured error response.
/// Usage: endpoint.AddEndpointFilter&lt;ValidationFilter&lt;MyCommand&gt;&gt;()
/// </summary>
public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is null)
            return await next(context);

        // Find the argument of type T in the endpoint parameters
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
            return await next(context);

        var validationResult = await validator.ValidateAsync(argument);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}
