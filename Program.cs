using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ParsWoW.Api.Application.Abstractions.Auth;
using ParsWoW.Api.Application.Abstractions.Common;
using ParsWoW.Api.Application.Abstractions.Dbc;
using ParsWoW.Api.Application.Abstractions.Persistence;
using ParsWoW.Api.Application.Abstractions.Services;
using ParsWoW.Api.Application.Configuration;
using ParsWoW.Api.Application.Constants;
using ParsWoW.Api.Infrastructure.Auth;
using ParsWoW.Api.Infrastructure.Cache;
using ParsWoW.Api.Infrastructure.Dbc;
using ParsWoW.Api.Infrastructure.Dbc.Providers;
using ParsWoW.Api.Infrastructure.Dbc.Schemas.CATA;
using ParsWoW.Api.Infrastructure.Dbc.Schemas.LEGION;
using ParsWoW.Api.Infrastructure.Dbc.Schemas.MOP;
using ParsWoW.Api.Infrastructure.Dbc.Schemas.TBC;
using ParsWoW.Api.Infrastructure.Dbc.Schemas.WOTLK;
using ParsWoW.Api.Infrastructure.Persistence;
using ParsWoW.Api.Infrastructure.Services;
using Dapper;
using ParsWoW.Api.Presentation.Filters;
using ParsWoW.Api.Presentation.Swagger;

var builder = WebApplication.CreateBuilder(args);

// ----- Options -----
builder.Services.Configure<ParsWowOptions>(builder.Configuration.GetSection(ParsWowOptions.SectionName));

// ----- Memory cache + abstractions -----
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICachingService, MemoryCachingService>();

// ----- Persistence -----
builder.Services.AddSingleton<IExpansionConnectionFactory, ExpansionConnectionFactory>();
builder.Services.AddScoped<IAccountRepository, DapperAccountRepository>();
builder.Services.AddSingleton<IRefreshTokenStore, DapperRefreshTokenStore>();

// ----- Auth -----
builder.Services.AddSingleton<IPasswordHasher, BlizzCmsEmulatorPasswordHasher>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.RequireHttpsMetadata = false;
        opts.SaveToken = true;
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["ParsWow:Jwt:Issuer"],
            ValidAudience = builder.Configuration["ParsWow:Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["ParsWow:Jwt:SigningKey"]
                    ?? Environment.GetEnvironmentVariable("PARSWOW_JWT_KEY")
                    ?? "dev-key-replace-in-production-32-bytes-minimum-aaaaaaaa")),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ----- DBC providers (one per expansion, each owning its own FULL schema bundle) -----
builder.Services.AddSingleton<IDbcProvider>(sp => new TbcDbcProvider(
    new IDbcSchema[] {
        new TbcItemSchema(), new TbcSpellSchema(),
        new TbcMapSchema(), new TbcAreaTableSchema(), new TbcAchievementSchema(),
        new TbcFactionSchema(), new TbcItemSetSchema(), new TbcEnchantmentSchema(),
        new TbcChrClassesSchema(), new TbcChrRacesSchema(), new TbcTalentSchema(),
        new TbcCreatureDisplaySchema(), new TbcItemDisplaySchema(), new TbcGemPropertiesSchema()
    },
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ParsWowOptions>>(),
    sp.GetRequiredService<ILogger<TbcDbcProvider>>()));
builder.Services.AddSingleton<IDbcProvider>(sp => new WotlkDbcProvider(
    new IDbcSchema[] {
        new WotlkItemSchema(), new WotlkSpellSchema(),
        new WotlkMapSchema(), new WotlkAreaTableSchema(), new WotlkAchievementSchema(),
        new WotlkFactionSchema(), new WotlkItemSetSchema(), new WotlkEnchantmentSchema(),
        new WotlkChrClassesSchema(), new WotlkChrRacesSchema(), new WotlkTalentSchema(),
        new WotlkCreatureDisplaySchema(), new WotlkItemDisplaySchema(), new WotlkGemPropertiesSchema()
    },
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ParsWowOptions>>(),
    sp.GetRequiredService<ILogger<WotlkDbcProvider>>()));
builder.Services.AddSingleton<IDbcProvider>(sp => new CataDbcProvider(
    new IDbcSchema[] {
        new CataItemSchema(), new CataSpellSchema(),
        new CataMapSchema(), new CataAreaTableSchema(), new CataAchievementSchema(),
        new CataFactionSchema(), new CataItemSetSchema(), new CataEnchantmentSchema(),
        new CataChrClassesSchema(), new CataChrRacesSchema(), new CataTalentSchema(),
        new CataCreatureDisplaySchema(), new CataItemDisplaySchema(), new CataGemPropertiesSchema()
    },
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ParsWowOptions>>(),
    sp.GetRequiredService<ILogger<CataDbcProvider>>()));
builder.Services.AddSingleton<IDbcProvider>(sp => new MopDbcProvider(
    new IDbcSchema[] {
        new MopItemSchema(), new MopSpellSchema(),
        new MopMapSchema(), new MopAreaTableSchema(), new MopAchievementSchema(),
        new MopFactionSchema(), new MopItemSetSchema(), new MopEnchantmentSchema(),
        new MopChrClassesSchema(), new MopChrRacesSchema(), new MopTalentSchema(),
        new MopCreatureDisplaySchema(), new MopItemDisplaySchema(), new MopGemPropertiesSchema()
    },
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ParsWowOptions>>(),
    sp.GetRequiredService<ILogger<MopDbcProvider>>()));
builder.Services.AddSingleton<IDbcProvider>(sp => new LegionDbcProvider(
    new IDbcSchema[] {
        new LegionItemSchema(), new LegionSpellSchema(),
        new LegionMapSchema(), new LegionAreaTableSchema(), new LegionAchievementSchema(),
        new LegionFactionSchema(), new LegionItemSetSchema(), new LegionEnchantmentSchema(),
        new LegionChrClassesSchema(), new LegionChrRacesSchema(), new LegionTalentSchema(),
        new LegionCreatureDisplaySchema(), new LegionItemDisplaySchema(), new LegionGemPropertiesSchema()
    },
    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ParsWowOptions>>(),
    sp.GetRequiredService<ILogger<LegionDbcProvider>>()));

builder.Services.AddSingleton<IDbcProviderFactory, DbcProviderFactory>();
builder.Services.AddScoped<IDbcService, DbcService>();
builder.Services.AddScoped<ITooltipService, TooltipService>();
builder.Services.AddScoped<ICharacterOwnershipValidator, DapperCharacterOwnershipValidator>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IArmoryService, ArmoryService>();

// ----- Shop -----
builder.Services.AddSingleton<InMemoryPaymentService>();
builder.Services.AddSingleton<IPaymentService>(sp => sp.GetRequiredService<InMemoryPaymentService>());
builder.Services.AddScoped<IShopService, ShopService>();

// ----- MVC + Swagger + ProblemDetails -----
builder.Services.AddControllers(opts =>
{
    opts.Filters.Add<ApiExceptionFilter>();
})
.AddJsonOptions(o =>
{
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddProblemDetails();
SwaggerConfiguration.Apply(builder.Services);

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ----- Swagger (must be after exception handler, before routing) -----
app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "Pars-WoW Master API v1");
    o.RoutePrefix = "swagger";
});

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseRouting();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// ----- Endpoints -----
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();

// ----- DBC startup (after pipeline is built; isolated try/catch so startup failure does not block Swagger) -----
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ParsWowOptions>>().Value;
    var factory = sp.GetRequiredService<IDbcProviderFactory>();
    foreach (var (kind, provider) in factory.Providers)
    {
        if (!opts.Expansions.TryGetValue(kind, out var eopts) || !eopts.Enabled) continue;
        try
        {
            var missing = await provider.LoadAsync();
            if (missing.Count > 0 && opts.Dbc.FailFastOnMissing)
            {
                var report = string.Join("\n", missing.Select(f => $"Missing DBC file: {f}"));
                Console.Error.WriteLine($"[FATAL] DBC startup failed for expansion {kind}.\n{report}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[ERROR] Exception during DBC load for {kind}: {ex.Message}");
        }
    }

    // ----- Auto-create refresh_tokens table if it doesn't exist -----
    try
    {
        var connFactory = sp.GetRequiredService<IExpansionConnectionFactory>();
        await using var conn = await connFactory.OpenAsync(ExpansionDatabase.Auth, ExpansionKind.WOTLK, CancellationToken.None);
        const string createTable = @"
            CREATE TABLE IF NOT EXISTS refresh_tokens (
                jti VARCHAR(64) NOT NULL PRIMARY KEY,
                account_id INT UNSIGNED NOT NULL,
                issued_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                expires_at DATETIME NOT NULL,
                revoked TINYINT(1) NOT NULL DEFAULT 0,
                replaced_by VARCHAR(64) DEFAULT NULL,
                INDEX idx_account (account_id),
                INDEX idx_expires (expires_at)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
        await conn.ExecuteAsync(createTable);
        Console.WriteLine("[OK] refresh_tokens table ready.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[WARN] Could not auto-create refresh_tokens table: {ex.Message}");
        Console.Error.WriteLine("[WARN] Run the migration SQL manually or ensure the auth database is accessible.");
    }
}

app.Run();