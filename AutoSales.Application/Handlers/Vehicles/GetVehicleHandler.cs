using Microsoft.Extensions.Logging;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Responses;
using AutoSales.Application.Results;
using AutoSales.Application.Repositories;

namespace AutoSales.Application.Handlers.Vehicles;

public sealed class GetVehicleHandler : IRequestHandler<GetVehicleRequest, Result<VehicleResponse>>
{
    private readonly IVehicleRepository _repository;
    private readonly ILogger<GetVehicleHandler> _logger;

    public GetVehicleHandler(
        IVehicleRepository repository,
        ILogger<GetVehicleHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<VehicleResponse>> Handle(GetVehicleRequest message, CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await _repository.FindAsync(message.Id, cancellationToken);

            if (vehicle is null)
            {
                _logger.LogWarning("Veículo {VehicleId} não encontrado para consulta.", message.Id);
                return Result<VehicleResponse>.Failure("Veículo não encontrado.", errorType: ErrorType.NotFound);
            }

            return Result<VehicleResponse>.Success(VehicleResponse.From(vehicle));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(GetVehicleHandler));
            throw;
        }
    }
}
