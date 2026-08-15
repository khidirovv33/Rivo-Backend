using Rivo.API.Extensions;
using Rivo.API.Filters;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddRivoInfrastructure(builder.Configuration);
builder.Services.AddRivoApplication();
builder.Services.AddRivoSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRivoPipeline();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
