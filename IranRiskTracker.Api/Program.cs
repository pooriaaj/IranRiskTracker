using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var seedDataPath = Path.Combine(AppContext.BaseDirectory, "Seeding", "Data");
builder.Services.AddSingleton<ISeedDataProvider>(_ => new JsonSeedDataProvider(seedDataPath));
builder.Services.AddScoped<IEventQueryService, EventQueryService>();
builder.Services.AddScoped<IRiskCalculator, RiskCalculator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
