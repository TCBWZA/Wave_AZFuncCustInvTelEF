using AZFuncCustInvTel.DTOs;
using AZFuncCustInvTel.Models;
using AZFuncCustInvTel.Repositories;
using AZFuncCustInvTel.Validators;
using FluentValidation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Register Entity Framework Core DbContext
        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            var configuration = serviceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
            options.UseSqlServer(configuration["DefaultConnection"]);
        });

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ITelephoneNumberRepository, TelephoneNumberRepository>();

        // Register FluentValidation validators
        services.AddScoped<IValidator<CreateCustomerDto>, CreateCustomerValidator>();
        services.AddScoped<IValidator<UpdateCustomerDto>, UpdateCustomerValidator>();
    })
    .Build();

host.Run();
