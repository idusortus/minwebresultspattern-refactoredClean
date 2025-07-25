using System.Reflection;
using Api.Application.Behaviors;
using Api.Extensions;
using Api.Infrastructure.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services));

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("localdb")));

    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails();
    builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    // add mediatr validation pipeline behavior
    builder.Services.AddScoped(
        typeof(IPipelineBehavior<,>), 
        typeof(ValidationBehavior<,>));
    // register validators
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.MapEndpoints();
    app.MapScalarApiReference();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "An unhandled exception occurred during startup");
}
finally
{
    Log.CloseAndFlush();
}