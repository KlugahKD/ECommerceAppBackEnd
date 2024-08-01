using Microsoft.AspNetCore.Http;

namespace ECommerce.SharedLibrary.Middleware;

public class ListenToOnlyApiGateway(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        //Extract specific header from request
        var signedHeader = context.Request.Headers["Api-Gateway"];

        // null means the request is not from the Api Gateway
        if (signedHeader.FirstOrDefault() is null)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync("Sorry Service is Unavailable");
        }

        await next(context);
    }
}