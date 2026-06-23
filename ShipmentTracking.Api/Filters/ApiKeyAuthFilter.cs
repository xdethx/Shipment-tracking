using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ShipmentTracking.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthFilter : Attribute, IAuthorizationFilter
{
    private const string ApiKeyHeader = "X-Api-Key";
    private const string ApiKeyConfigKey = "ApiKey";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        var expectedKey = config[ApiKeyConfigKey];

        // Fail closed: if no key is configured, deny all requests.
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var providedKey)
            || providedKey != expectedKey)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
