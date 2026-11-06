using MediatR;
using AutoSales.Application.Results;

namespace AutoSales.Application.Requests;

public sealed record DeletePersonRequest(int Id) : IRequest<Result<bool>>;
