using System.Net;
using System.Text.Json;
using AutoEscola.Api.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AutoEscola.Api.Api.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (DomainException ex)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = ex.StatusCode;

            var problem = new ProblemDetails
            {
                Status = ex.StatusCode,
                Title = "Regra de Negócio Violada",
                Detail = ex.Message,
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var problem = new ProblemDetails
            {
                Status = 500,
                Title = "Erro Interno no Servidor",
                Detail = "Ocorreu um erro inesperado ao processar sua solicitação.",
                Instance = context.Request.Path
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
