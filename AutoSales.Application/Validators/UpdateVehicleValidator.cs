using AutoSales.Application.Requests;
using FluentValidation;

namespace AutoSales.Application.Validators
{
    public sealed class UpdateVehicleValidator
        : AbstractValidator<UpdateVehicleRequest>
    {
        public UpdateVehicleValidator()
        {
            RuleFor(request => request.Plate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Matches(@"\A[A-Za-z]{3}-?(?:[0-9]{4}|[0-9][A-Za-z][0-9]{2})\z")
                .WithMessage("Informe uma placa válida, como ABC1234 ou ABC1D23.")
                .WithName("Placa");

            RuleFor(request => request.ManufactureYear)
                .Cascade(CascadeMode.Stop)
                .GreaterThanOrEqualTo(1000)
                .WithMessage("O ano de fabricação deve ter quatro dígitos.")
                .LessThanOrEqualTo(DateTime.Today.Year)
                .WithMessage("O ano de fabricação não pode estar no futuro.");

            RuleFor(request => request.Brand)
                .NotEmpty()
                .MaximumLength(80)
                .WithName("Marca");

            RuleFor(request => request.Model)
                .NotEmpty()
                .MaximumLength(100)
                .WithName("Modelo");
        }
    }
}
