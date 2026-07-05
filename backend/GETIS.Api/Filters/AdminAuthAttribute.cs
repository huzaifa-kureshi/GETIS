using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace GETIS.Api.Filters;

/// <summary>
/// Simple demo-grade authorization filter. Checks for a matching
/// "X-Admin-Token" header before allowing the request through.
/// This is NOT production-grade security (see README for how to
/// upgrade this to real JWT authentication with hashed passwords).
/// </summary>
public class AdminAuthAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedToken = config["AdminToken"];

        if (string.IsNullOrEmpty(expectedToken) ||
            !context.HttpContext.Request.Headers.TryGetValue("X-Admin-Token", out var providedToken) ||
            providedToken != expectedToken)
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Admin authentication required." });
            return;
        }

        await next();
    }
}
