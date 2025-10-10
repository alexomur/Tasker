using Microsoft.AspNetCore.Mvc;

namespace Tasker.Api.ModelBinding;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FromCurrentUserAttribute : ModelBinderAttribute
{
    public FromCurrentUserAttribute() : base(typeof(CurrentUserModelBinder))
    {
    }
}