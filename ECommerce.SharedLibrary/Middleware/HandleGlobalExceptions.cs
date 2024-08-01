using System.Text.Json;
using ECommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.SharedLibrary.Middleware;

public class HandleGlobalExceptions(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        //Declare default variables
        var message = "sorry, internal server error occured. Kindly try again";
        var statusCode = StatusCodes.Status500InternalServerError;
        var title = "Error";
        
        try
        {
            await next(context);
            
            //Check if exception is too many requests // 429 status
            if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
            {
                message = "Too many requests made, kindly try again later";
                statusCode = StatusCodes.Status429TooManyRequests;
                title = "Warning";
                
                await ModifyHeader(context, title, message, statusCode);
            }
            
            // check if response is Unauthorized // 401 status
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                message = "Unauthorized access";
                statusCode = StatusCodes.Status401Unauthorized;
                title = "Alert";
                
                await ModifyHeader(context, title, message, statusCode);
            }
            
            // check if response is Forbidden // 403 status
            if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
            {
                message = "Forbidden access";
                statusCode = StatusCodes.Status403Forbidden;
                title = "Out of Access";
                
                await ModifyHeader(context, title, message, statusCode);
            }
        }
        catch (Exception ex)
        {
            //log exceptions to file, console and debugger
            LogExceptions.LogExceptionsTo(ex);
            
            //check if exception is timeout // 408 status
            if (ex is TimeoutException || ex is TaskCanceledException)
            {
                message = "Request timeout, kindly try again later";
                statusCode = StatusCodes.Status408RequestTimeout;
                title = "Timeout";
            }
            // if none of exceptions then return the default error message
            await ModifyHeader(context, title, message, statusCode);
            
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal Server Error");
        }
    }

    private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
    {
        // display scary-free message to client
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
        {
            Detail = message,
            Status = statusCode,
            Title = title
        }), CancellationToken.None);
    }
}

