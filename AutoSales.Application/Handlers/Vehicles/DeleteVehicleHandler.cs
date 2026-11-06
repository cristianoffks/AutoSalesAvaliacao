using Microsoft.Extensions.Logging;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Results;
using AutoSales.Application.Repositories;

namespace AutoSales.Application.Handlers.Vehicles;

public sealed class DeleteVehicleHandler : IRequestHandler<DeleteVehicleRequest, Result<bool>>
{
    private readonly IVehicleRepository _repository;
    private readonly ILogger<DeleteVehicleHandler> _logger;

    public DeleteVehicleHandler(
        IVehicleRepository repository,
        ILogger<DeleteVehicleHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteVehicleRequest message, CancellationToken cancellationToken)
    {
        try
        {
            var id = message.Id;
            var entity = await _repository.FindAsync(id, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Veículo {VehicleId} não encontrado para exclusão.", id);
                return Result<bool>.Failure("Veículo não encontrado.", errorType: ErrorType.NotFound);
            }

            await _repository.DeleteAsync(entity, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(DeleteVehicleHandler));
            throw;
        }
    }

}
