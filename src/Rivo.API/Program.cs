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

var app = builder.Build();

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
