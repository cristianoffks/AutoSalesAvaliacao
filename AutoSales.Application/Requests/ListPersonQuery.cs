using MediatR;
using AutoSales.Application.Entities;
using AutoSales.Application.Results;

namespace AutoSales.Application.Requests;

public sealed record ListPersonQuery(string? Name, string? Cpf) : IRequest<Result<IReadOnlyList<Person>>>;
