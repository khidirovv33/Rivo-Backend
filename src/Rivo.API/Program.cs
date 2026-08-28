using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Rivo.API.Extensions;
using Rivo.Application;
using Rivo.Infrastructure;
using Rivo.Infrastructure.Persistence;
using Rivo.Infrastructure.Persistence.Seed;
using Serilog;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApiControllers();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddRivoCors(builder.Configuration);
builder.Services.AddRivoSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// ru/en/tg — выбирается по заголовку Accept-Language (фронт синхронизирует его с переключателем языка
// в интерфейсе); ru остаётся культурой по умолчанию, т.к. это исходный язык большей части текстов.
var supportedCultures = new[] { "ru", "en", "tg" }.Select(c => new CultureInfo(c)).ToList();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("ru"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);

    if (app.Environment.IsDevelopment())
    {
        await DemoDataSeeder.SeedAsync(app.Services);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRivoExceptionHandling();

app.UseHttpsRedirection();
app.UseCors("RivoDefault");

app.UseAuthentication();
app.UseAuthorization();

app.UseRivoTenantAndAuditLogging();

app.MapControllers();

app.Run();
