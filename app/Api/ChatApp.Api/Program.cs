using Asp.Versioning;

using Chat.Presentation.DependencyInjection;

using ChatApp.Api.Extensions;

using CorrelationId;
using CorrelationId.DependencyInjection;

using Identity.Presentation;

using Notification.Presentation;

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

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Composição dos módulos do monólito modular.
builder.Services
    .AddChatModule(builder.Configuration, builder.Environment)
    .AddIdentityModule(builder.Configuration)
    .AddNotificationModule(builder.Configuration);

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

app.UseCors();

if (!app.Environment.IsEnvironment("Testing"))
    app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

// Cada módulo expõe seus próprios endpoints (Minimal APIs).
app.MapChatEndpoints();
app.MapIdentityEndpoints();
app.MapNotificationEndpoints();

app.Run();

public partial class Program { }
