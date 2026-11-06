using MediatR;
using AutoSales.Application.Entities;
using AutoSales.Application.Results;

namespace AutoSales.Application.Requests;

public sealed record GetPersonRequest(int Id) : IRequest<Result<Person>>;
