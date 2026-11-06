using MediatR;
using AutoSales.Application.Results;
using AutoSales.Application.Entities;

namespace AutoSales.Application.Requests;

public sealed record CreatePersonRequest(
    string FullName,
    string Cpf,
    DateOnly? BirthDate,
    string Email,
    string Phone) : IRequest<Result<Person>>;
