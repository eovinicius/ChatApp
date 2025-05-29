using Serilog;

namespace ChatApp.Api.Extensions;

public static class SerilogExtensions
{
    public static void UseSerilogCustom(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, loggerConfig) =>
        {
            var configuration = context.Configuration;

            loggerConfig
                .ReadFrom.Configuration(configuration);
        });
    }
}
