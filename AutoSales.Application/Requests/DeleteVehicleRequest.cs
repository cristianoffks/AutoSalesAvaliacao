using MediatR;
using AutoSales.Application.Results;

namespace AutoSales.Application.Requests;

public sealed record DeleteVehicleRequest(int Id) : IRequest<Result<bool>>;
