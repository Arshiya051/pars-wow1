using System.Reflection;
using Microsoft.OpenApi.Models;

namespace ParsWoW.Api.Presentation.Swagger;

internal static class SwaggerConfiguration
{
    public static void Apply(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Pars-WoW Master API",
                Version = "v1",
                Description = "Multi-expansion World of Warcraft master data API.",
                Contact = new OpenApiContact { Name = "Pars-WoW Development" }
            });

            // Use fully-qualified type names to avoid schema-ID collisions
            // (e.g. Dbc.TalentDto vs Armory.TalentDto both wrapped in ApiResponse<T>).
            c.CustomSchemaIds(type => type.FullName?.Replace("+", "."));

            var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = Array.Empty<string>()
            });
        });
    }
}
