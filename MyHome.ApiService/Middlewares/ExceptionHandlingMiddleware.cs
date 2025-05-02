using Microsoft.AspNetCore.Mvc;
using MyHome.Core.Models.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Item not found: {Message}", ex.Message);
            await WriteErrorResponse(httpContext, StatusCodes.Status404NotFound, "Item Not Found", ex.Message);
        }
    }

    private static async Task WriteErrorResponse(HttpContext httpContext, int statusCode, string title, string detail)
    {
        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails);
    }
}