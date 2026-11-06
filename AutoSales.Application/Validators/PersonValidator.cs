using AutoSales.Application.Requests;
using FluentValidation;

namespace AutoSales.Application.Validators;

public sealed class PersonValidator : AbstractValidator<CreatePersonRequest>
{
    public PersonValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(150).WithName("Nome completo");
        RuleFor(request => request.Cpf).Cascade(CascadeMode.Stop).NotEmpty()
            .Matches(@"\A[0-9]{11}\z")
            .WithMessage("Informe um CPF com 11 dígitos.").WithName("CPF");
        RuleFor(request => request.BirthDate).Cascade(CascadeMode.Stop).NotNull()
            .Must(date => date <= DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("A data de nascimento não pode estar no futuro.").WithName("Data de nascimento");
        RuleFor(request => request.Email).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(254)
            .EmailAddress().Must(email => !email.Contains('\r') && !email.Contains('\n'))
            .WithMessage("Informe um e-mail válido.").WithName("E-mail");
        RuleFor(request => request.Phone).NotEmpty().MaximumLength(30).WithName("Telefone");
    }
}
