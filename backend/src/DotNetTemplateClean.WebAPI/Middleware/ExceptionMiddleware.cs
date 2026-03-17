using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DotNetTemplateClean.WebAPI;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;
    private readonly IHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    { 
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            var statusCode = GetStatusCode(ex);

            
            _logger.LogError(ex, "Exception: {Message} | TraceId: {TraceId}", ex.Message, traceId);

            //Création de l'objet ProblemDetails (Standard RFC 7807)
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(ex),
                Detail = (_env.IsDevelopment() || _env.IsEnvironment("Test")) ? ex.Message : "Une erreur interne est survenue.",
                Instance = context.Request.Path,
                Type = $"https://httpstatuses.com/{statusCode}"
            };

            //Ajout d'extensions personnalisées (TraceId et StackTrace si Dev)
            problemDetails.Extensions.Add("traceId", traceId);

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions.Add("stackTrace", ex.StackTrace);
            }

            //Envoi de la réponse
            context.Response.ContentType = "application/problem+json"; // Content-Type spécifique au standard
            context.Response.StatusCode = statusCode;

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsJsonAsync(problemDetails, options);
        }
    }

    private static int GetStatusCode(Exception ex) =>
        ex switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string GetTitle(Exception ex) =>
        ex switch
        {
            KeyNotFoundException => "Ressource introuvable",
            UnauthorizedAccessException => "Accès non autorisé",
            ArgumentException => "Requête invalide",
            InvalidOperationException => "Conflit d'opération",
            _ => "Erreur interne du serveur"
        };
}
