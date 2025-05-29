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


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ChatApp API", Version = "v1" });

    c.AddSecurityDefinition("Token", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Insira o token JWT aqui",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Token"
                }
            },
            Array.Empty<string>()
        }
    });
});

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