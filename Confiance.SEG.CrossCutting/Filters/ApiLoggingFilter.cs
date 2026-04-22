using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Confiance.SEG.CrossCutting.Filters;

public class ApiLoggingFilter : IActionFilter
{
    private readonly ILogger<ApiLoggingFilter> _logger;

    public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // no-op
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInformation($"Executing {context.ActionDescriptor.DisplayName}");
    }
}
