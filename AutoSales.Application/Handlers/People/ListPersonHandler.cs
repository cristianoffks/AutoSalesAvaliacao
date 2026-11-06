using Microsoft.Extensions.Logging;
using AutoSales.Application.Extensions;
using MediatR;
using AutoSales.Application.Requests;
using AutoSales.Application.Entities;
using AutoSales.Application.Repositories;
using AutoSales.Application.Results;

namespace AutoSales.Application.Handlers.People;

public sealed class ListPersonHandler : IRequestHandler<ListPersonQuery, Result<IReadOnlyList<Person>>>
{
    private readonly IPersonRepository _repository;
    private readonly ILogger<ListPersonHandler> _logger;

    public ListPersonHandler(
        IPersonRepository repository,
        ILogger<ListPersonHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<Person>>> Handle(ListPersonQuery message, CancellationToken cancellationToken)
    {
        try
        {
            var name = message.Name;
            var cpf = message.Cpf;

            var normalizedCpf = string.IsNullOrWhiteSpace(cpf) ? null : cpf.NormalizeCpf();
            var normalizedName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            var people = await _repository.ListAsync(normalizedName, normalizedCpf, cancellationToken);
            return Result<IReadOnlyList<Person>>.Success(people);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Erro ao executar {Handler}", nameof(ListPersonHandler));
            throw;
        }
    }
}
