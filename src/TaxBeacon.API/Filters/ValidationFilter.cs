using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaxBeacon.API.Extensions;

namespace TaxBeacon.API.Filters;

public class ValidationFilter: IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // TODO: Rewrite it to handle query parameters as well
        var parameter =
            context.ActionDescriptor.Parameters.SingleOrDefault(p =>
                p.BindingInfo?.BindingSource == BindingSource.Body);

        if (parameter is not null)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(parameter.ParameterType);

            if (context.HttpContext.RequestServices.GetService(validatorType) is IValidator validator
                && context.ActionArguments.TryGetValue(parameter.Name, out var argument)
                && argument is not null)
            {
                var validationContextType = typeof(ValidationContext<>).MakeGenericType(argument.GetType());
                var validationContext = Activator.CreateInstance(validationContextType, argument) as IValidationContext;
                var validationResult = await validator.ValidateAsync(validationContext);
                if (!validationResult.IsValid)
                {
                    context.Result =
                        new BadRequestObjectResult(validationResult.Errors.ConvertPropertyNameToCamelCase());
                    return;
                }
            }
        }

        await next();
    }
}
