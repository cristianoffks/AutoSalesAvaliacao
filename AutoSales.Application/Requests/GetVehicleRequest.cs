using MediatR;
using AutoSales.Application.Responses;
using AutoSales.Application.Results;

namespace AutoSales.Application.Requests;

public sealed record GetVehicleRequest(int Id) : IRequest<Result<VehicleResponse>>;
