using AutoSales.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AutoSales.Api.Middleware;

public sealed class ErrorHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var (status, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Dados inválidos"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflito"),
                _ => (StatusCodes.Status500InternalServerError, "Erro inesperado")
            };

            context.Response.StatusCode = status;
            ProblemDetails problem = exception is ValidationException validation
                ? new ValidationProblemDetails(validation.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray()))
                : new ProblemDetails();

            problem.Status = status;
            problem.Title = title;
            problem.Instance = context.Request.Path;
            if (exception is ConflictException)
            {
                problem.Detail = exception.Message;
            }

            if (exception is ConflictException { Field: not null } conflict)
            {
                problem.Extensions["errors"] = new Dictionary<string, string[]>
                {
                    [conflict.Field] = [conflict.Message]
                };
            }

            await context.Response.WriteAsJsonAsync(problem,
                problem.GetType(),
                options: null,
                contentType: "application/problem+json",
                cancellationToken: context.RequestAborted);
        }
    }
}
