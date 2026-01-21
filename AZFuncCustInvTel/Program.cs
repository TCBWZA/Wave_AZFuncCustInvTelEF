using AZFuncCustInvTel.DTOs;
using AZFuncCustInvTel.Models;
using AZFuncCustInvTel.Repositories;
using AZFuncCustInvTel.Validators;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights telemetry
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register Entity Framework Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration["DefaultConnection"]));

// Register repositories
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ITelephoneNumberRepository, TelephoneNumberRepository>();

// Register FluentValidation validators
// These are used by CustomerFunctionsWithFluentValidation
builder.Services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
builder.Services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerValidator>();

// Alternative: Register all validators in the assembly automatically
// builder.Services.AddValidatorsFromAssemblyContaining<CreateCustomerValidator>();

builder.Build().Run();
