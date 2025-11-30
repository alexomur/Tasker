using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Tasker.Shared.Kernel.Errors;

namespace Tasker.Shared.Web;

public class AppExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<AppExceptionFilter> _logger;

    public AppExceptionFilter(ILogger<AppExceptionFilter> logger)
    {
        _logger = logger;
    }
    
    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;

        if (exception is AppException appException)
        {
            var problem = new ProblemDetails
            {
                Status = appException.StatusCode,
                Title = appException.Message,
            };
            
            problem.Extensions["code"] = appException.Code;
            
            context.Result = new ObjectResult(problem)
            {
                StatusCode = appException.StatusCode,
            };
            context.ExceptionHandled = true;
            return Task.CompletedTask;
        }
        
        _logger.LogError(exception, "Unhandled exception");

        var unknown = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
        };

        context.Result = new ObjectResult(unknown)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
        context.ExceptionHandled = true;
        return Task.CompletedTask;
    }
}