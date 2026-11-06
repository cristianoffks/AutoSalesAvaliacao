using Microsoft.Extensions.Logging;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Results;
using AutoSales.Application.Repositories;

namespace AutoSales.Application.Handlers.People;

public sealed class DeletePersonHandler : IRequestHandler<DeletePersonRequest, Result<bool>>
{
    private readonly IPersonRepository _repository;
    private readonly ILogger<DeletePersonHandler> _logger;

    public DeletePersonHandler(
        IPersonRepository repository,
        ILogger<DeletePersonHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeletePersonRequest message, CancellationToken cancellationToken)
    {
        try
        {
            var id = message.Id;
            var entity = await _repository.FindAsync(id, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Pessoa {PersonId} não encontrada para exclusão.", id);

                return Result<bool>.Failure("Pessoa não encontrada.", errorType: ErrorType.NotFound);
            }

            await _repository.DeleteAsync(entity, cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(DeletePersonHandler));
            throw;
        }
    }
}
