using Serilog;

namespace ChatApp.Api.Extensions;

public static class SerilogExtensions
{
    public static void UseSerilogCustom(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig
                .ReadFrom.Configuration(context.Configuration);
        });
    }
}
