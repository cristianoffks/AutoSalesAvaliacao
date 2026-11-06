using Microsoft.Extensions.Logging;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Results;
using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;

namespace AutoSales.Application.Handlers.People;

public sealed class GetPersonHandler : IRequestHandler<GetPersonRequest, Result<Person>>
{
    private readonly IPersonRepository _repository;
    private readonly ILogger<GetPersonHandler> _logger;

    public GetPersonHandler(
        IPersonRepository repository,
        ILogger<GetPersonHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<Person>> Handle(GetPersonRequest message, CancellationToken cancellationToken)
    {
        try
        {
            var id = message.Id;
            var person = await _repository.FindAsync(id, cancellationToken);
            if (person == null)
            {
                _logger.LogWarning("Pessoa {PersonId} não encontrada para consulta.", id);
                return Result<Person>.Failure("Pessoa não encontrada.", errorType: ErrorType.NotFound);
            }

            return Result<Person>.Success(person);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(GetPersonHandler));
            throw;
        }
    }
}
