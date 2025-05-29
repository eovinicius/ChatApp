using System.Threading.RateLimiting;

using ChatApp.Api.Extensions;
using ChatApp.Application;
using ChatApp.Infrastructure;

using CorrelationId;
using CorrelationId.DependencyInjection;

using Microsoft.OpenApi.Models;

using Serilog;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultCorrelationId(options =>
{
    options.AddToLoggingScope = true;
    options.RequestHeader = "X-Correlation-ID";
    options.ResponseHeader = "X-Correlation-ID";
    options.IncludeInResponse = true;
});

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig
        // .ReadFrom.Configuration(context.Configuration)
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .Enrich.WithCorrelationId()
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] (CorrID: {CorrelationId}) {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] (CorrID: {CorrelationId}) {Message:lj}{NewLine}{Exception}")
);


builder.Services.AddCors();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();


builder.Services.AddCustomSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseCorrelationId();

app.UseRateLimiter();

app.UseCustomExceptionHandler();

app.UseRequestContextLogging();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();