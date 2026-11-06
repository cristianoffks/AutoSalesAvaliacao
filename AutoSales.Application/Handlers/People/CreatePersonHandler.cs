using Microsoft.Extensions.Logging;
using AutoSales.Application.Extensions;
using AutoSales.Application.Results;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;
using FluentValidation;

namespace AutoSales.Application.Handlers.People;

public sealed class CreatePersonHandler : IRequestHandler<CreatePersonRequest, Result<Person>>
{
    private readonly IPersonRepository _repository;
    private readonly IValidator<CreatePersonRequest> _validator;
    private readonly ILogger<CreatePersonHandler> _logger;

    public CreatePersonHandler(
        IPersonRepository repository,
        IValidator<CreatePersonRequest> validator,
        ILogger<CreatePersonHandler> logger)
    {
        _repository = repository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<Person>> Handle(CreatePersonRequest request, CancellationToken cancellationToken)
    {
        try
        {

            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var cpf = request.Cpf.NormalizeCpf();

            if (await _repository.ExistsByCpfAsync(cpf, cancellationToken))
            {
                var result = Result<Person>.Failure("Já existe uma pessoa cadastrada com este CPF.", "Cpf");

                _logger.LogWarning(result.Message);

                return result;
            }

            var entity = new Person
            {
                FullName = request.FullName.Trim(),
                Cpf = cpf,
                BirthDate = request.BirthDate!.Value,
                Email = request.Email.Trim(),
                Phone = request.Phone.Trim()
            };

            await _repository.AddAsync(entity, cancellationToken);

            return Result<Person>.Success(entity);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(CreatePersonHandler));
            throw;
        }
    }
}
