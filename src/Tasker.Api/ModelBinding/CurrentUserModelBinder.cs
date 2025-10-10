using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tasker.Core.Users;

namespace Tasker.Api.ModelBinding;

public sealed class CurrentUserModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var httpContext = bindingContext.HttpContext;

        var userPrincipal = httpContext.User;

        if (userPrincipal is null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        string? idClaimValue = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idClaimValue))
        {
            idClaimValue = userPrincipal.FindFirst("sub")?.Value;
        }

        var repository = httpContext.RequestServices.GetService<IUserRepository>();

        if (repository == null)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        if (!string.IsNullOrEmpty(idClaimValue) && Guid.TryParse(idClaimValue, out var parsedGuid))
        {
            var userEntity = await repository.GetByIdAsync(parsedGuid, httpContext.RequestAborted);

            if (userEntity != null)
            {
                bindingContext.Result = ModelBindingResult.Success(userEntity);
                bindingContext.ValidationState[userEntity] = new ValidationStateEntry
                {
                    SuppressValidation = true
                };
                return;
            }

            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        var emailClaimValue = userPrincipal.FindFirst(ClaimTypes.Email)?.Value;

        if (!string.IsNullOrEmpty(emailClaimValue))
        {
            var userEntityByEmail = await repository.FindByEmailAsync(emailClaimValue, httpContext.RequestAborted);

            if (userEntityByEmail != null)
            {
                bindingContext.Result = ModelBindingResult.Success(userEntityByEmail);
                bindingContext.ValidationState[userEntityByEmail] = new ValidationStateEntry
                {
                    SuppressValidation = true
                };
                return;
            }

            bindingContext.Result = ModelBindingResult.Failed();
            return;
        }

        bindingContext.Result = ModelBindingResult.Failed();
        return;
    }
}
