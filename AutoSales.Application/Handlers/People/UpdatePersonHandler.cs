using Microsoft.Extensions.Logging;
using AutoSales.Application.Extensions;
using AutoSales.Application.Results;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Repositories;
using FluentValidation;

namespace AutoSales.Application.Handlers.People;

public sealed class UpdatePersonHandler : IRequestHandler<UpdatePersonRequest, Result<bool>>
{
    private readonly IPersonRepository _repository;
    private readonly IValidator<CreatePersonRequest> _validator;
    private readonly ILogger<UpdatePersonHandler> _logger;

    public UpdatePersonHandler(
        IPersonRepository repository,
        IValidator<CreatePersonRequest> validator,
        ILogger<UpdatePersonHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdatePersonRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = request.Id;
            await _validator.ValidateAndThrowAsync(new CreatePersonRequest(request.FullName, request.Cpf,
                request.BirthDate, request.Email, request.Phone), cancellationToken);
            var entity = await _repository.FindAsync(id, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning("Pessoa {PersonId} não encontrada para atualização.", id);
                return Result<bool>.Failure("Pessoa não encontrada.", errorType: ErrorType.NotFound);
            }

            var cpf = request.Cpf.NormalizeCpf();

            if (cpf != entity.Cpf)
            {
                _logger.LogWarning("Tentativa de alterar CPF da pessoa {PersonId}.", id);
                return Result<bool>.Failure("O CPF não pode ser alterado.", nameof(request.Cpf), ErrorType.Validation);
            }

            entity.FullName = request.FullName.Trim();
            entity.BirthDate = request.BirthDate!.Value;
            entity.Email = request.Email.Trim();
            entity.Phone = request.Phone.Trim();
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<bool>.Success(true);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(UpdatePersonHandler));
            throw;
        }
    }

}
