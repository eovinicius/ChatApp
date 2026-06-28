using ChatApp.Api.Extensions;
using ChatApp.Application;
using ChatApp.Infrastructure;

using CorrelationId;
using CorrelationId.DependencyInjection;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultCorrelationId(options =>
{
    options.AddToLoggingScope = true;
    options.RequestHeader = "X-Correlation-ID";
    options.ResponseHeader = "X-Correlation-ID";
    options.IncludeInResponse = true;
});

builder.UseSerilogCustom();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddControllers();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddApplication();

builder.Services.AddCustomSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCustomSwagger();

    app.ApplyMigrations();
}

app.UseHttpsRedirection();

app.UseCorrelationId();

app.UseCustomExceptionHandler();

app.UseRequestContextLogging();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.00} ms";
});

app.UseCors();

if (!app.Environment.IsEnvironment("Testing"))
    app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<ChatApp.Infrastructure.RealTime.ChatHub>("/chatHub");

app.Run();

public partial class Program { }
