using ChatApp.Api.Extensions;
using ChatApp.Application;
using ChatApp.Infrastructure;

using CorrelationId;
using CorrelationId.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultCorrelationId(options =>
{
    options.AddToLoggingScope = true;
    options.RequestHeader = "X-Correlation-ID";
    options.ResponseHeader = "X-Correlation-ID";
    options.IncludeInResponse = true;
});

builder.UseSerilogCustom();

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