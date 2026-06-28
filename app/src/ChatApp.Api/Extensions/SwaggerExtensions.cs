using Microsoft.OpenApi.Models;

using System.Reflection;

namespace ChatApp.Api.Extensions
{
    public static class SwaggerExtensions
    {
        private const string SwaggerVersion = "v1";
        private const string SecuritySchemeName = "Bearer";

        public static IServiceCollection AddCustomSwagger(
            this IServiceCollection services,
            string title = "ChatApp API",
            string description = "API para o ChatApp")
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(SwaggerVersion, new OpenApiInfo
                {
                    Title = title,
                    Version = SwaggerVersion,
                    Description = description
                });

                AddJwtSecurityDefinition(c);
                AddXmlComments(c);
            });

            return services;
        }

        private static void AddJwtSecurityDefinition(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions c)
        {
            c.AddSecurityDefinition(SecuritySchemeName, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o token JWT no formato: Bearer {seu token}"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = SecuritySchemeName
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }

        private static void AddXmlComments(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions c)
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        }
    }
}