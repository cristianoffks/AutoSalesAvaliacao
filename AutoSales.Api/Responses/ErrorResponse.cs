using AutoSales.Application.Results;
using Microsoft.AspNetCore.Mvc;

namespace AutoSales.Api.Responses;

public static class ErrorResponse
{
    public static ObjectResult Create<T>(Result<T> result, string path)
    {
        var (status, title) = result.ErrorType switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Dados inválidos"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Registro não encontrado"),
            _ => (StatusCodes.Status409Conflict, "Conflito")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = result.Message,
            Instance = path
        };

        if (result.Field is not null)
        {
            problem.Extensions["errors"] = new Dictionary<string, string[]>
            {
                [result.Field] = [result.Message!]
            };
        }

        var response = new ObjectResult(problem) { StatusCode = status };
        response.ContentTypes.Add("application/problem+json");
        return response;
    }
}
