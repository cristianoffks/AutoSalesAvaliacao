using Microsoft.Extensions.Logging;
using AutoSales.Application.Extensions;
using AutoSales.Application.Results;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Repositories;
using FluentValidation;

namespace AutoSales.Application.Handlers.Vehicles;

public sealed class UpdateVehicleHandler : IRequestHandler<UpdateVehicleRequest, Result<bool>>
{
    private readonly IVehicleRepository _repository;
    private readonly IValidator<UpdateVehicleRequest> _validator;
    private readonly ILogger<UpdateVehicleHandler> _logger;

    public UpdateVehicleHandler(
        IVehicleRepository repository,
        IValidator<UpdateVehicleRequest> validator,
        ILogger<UpdateVehicleHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateVehicleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = request.Id;
            var entity = await _repository.FindAsync(id, cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("Veículo {VehicleId} não encontrado para atualização.", id);
                return Result<bool>.Failure("Veículo não encontrado.", errorType: ErrorType.NotFound);
            }

            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            if (await _repository.ExistsByPlateAsync(request.Plate.NormalizePlate(), id, cancellationToken))
            {
                return Result<bool>.Failure("Já existe um veículo cadastrado com esta placa.", "Plate");
            }

            entity.Brand = request.Brand.Trim();
            entity.Model = request.Model.Trim();
            entity.Plate = request.Plate.NormalizePlate();
            entity.ManufactureYear = request.ManufactureYear;
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(UpdateVehicleHandler));
            throw;
        }
    }


}
