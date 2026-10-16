using MindSync.Api.Common;
using MindSync.Application.Common.Exceptions;
using MindSync.Domain.Exceptions;
using System.Text.Json;

namespace MindSync.Api.Middlewares;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção capturada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            DomainException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;

        var response = exception switch
        {
            ValidationException valEx => ApiResponse<object>.Fail("Erro de validação nos dados enviados.", valEx.Errors),
            DomainException domainEx => ApiResponse<object>.Fail(domainEx.Message),
            NotFoundException notFoundEx => ApiResponse<object>.Fail(notFoundEx.Message),
            _ => ApiResponse<object>.Fail("Ocorreu um erro interno no servidor.")
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}