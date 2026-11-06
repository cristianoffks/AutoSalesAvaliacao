using AutoSales.Api.Middleware;
using AutoSales.Application.Requests;
using AutoSales.Application.Validators;
using AutoSales.Application.Repositories;
using AutoSales.Infrastructure;
using AutoSales.Infrastructure.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Serilog;
using AutoSales.Application.Handlers.People;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Services.AddSerilog(configuration => configuration
    .MinimumLevel.Warning()
    .WriteTo.Console());

ValidatorOptions.Global.LanguageManager.Culture = CultureInfo.GetCultureInfo("pt-BR");

builder.Services.AddControllers(options =>
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(field => field.Value?.Errors.Count > 0)
                .ToDictionary(field => field.Key, _ => new[] { "O valor informado é inválido." });
            return new BadRequestObjectResult(new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dados inválidos"
            });
        };
    });
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Registry");

#if DEBUG
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.Local.json", optional: true)
        .Build()
        .GetConnectionString("Registry");
}
#endif

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A conexão com o banco de dados não foi configurada.");
}

builder.Services.AddDbContext<RegistryDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssemblyContaining<CreatePersonHandler>();
});
builder.Services.AddScoped<IValidator<CreatePersonRequest>, PersonValidator>();
builder.Services.AddScoped<IValidator<CreateVehicleRequest>, CreateVehicleValidator>();
builder.Services.AddScoped<IValidator<UpdateVehicleRequest>, UpdateVehicleValidator>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
