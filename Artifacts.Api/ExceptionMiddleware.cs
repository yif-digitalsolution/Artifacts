using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Utils.Exceptions;
// using FluentValidation;

namespace Artifacts.Api;
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ocurrió un error en la aplicación");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        object response;

        switch (exception)
        {
            // case ValidationException valEx:
            //     statusCode = (int)HttpStatusCode.BadRequest;
            //     response = new
            //     {
            //         StatusCode = statusCode,
            //         Message = "Errores de validación",
            //         Errors = valEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            //     };
            //     break;

            case ArgumentException argEx:
                statusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    StatusCode = statusCode,
                    Message = argEx.Message
                };
                break;

            case NotFoundException nfEx:
                statusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    StatusCode = statusCode,
                    Message = nfEx.Message
                };
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                
                response = new
                {
                    StatusCode = statusCode,
                    Message = "Ocurrió un error interno en el servidor."
                };
                break;
        }

        context.Response.StatusCode = statusCode;
        var json = JsonSerializer.Serialize(response);

        return context.Response.WriteAsync(json);
    }
}

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionMiddleware>();
    }
}