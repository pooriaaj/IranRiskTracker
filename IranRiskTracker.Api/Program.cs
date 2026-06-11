using IranRiskTracker.Application.Interfaces;
using IranRiskTracker.Application.Services;
using IranRiskTracker.Infrastructure.Seeding;

var builder = WebApplication.CreateBuilder(args);

// Listen on Railway's PORT env var (defaults to 8080 for Docker)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Open CORS — public read-only API, no auth
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var seedDataPath = Path.Combine(AppContext.BaseDirectory, "Seeding", "Data");
builder.Services.AddSingleton<ISeedDataProvider>(_ => new JsonSeedDataProvider(seedDataPath));
// Register in-memory live store as a singleton to preserve events during app lifetime.
builder.Services.AddSingleton<IranRiskTracker.Application.Interfaces.ILiveEventStore, IranRiskTracker.Infrastructure.Storage.InMemoryLiveEventStore>();
builder.Services.AddSingleton<IranRiskTracker.Application.Interfaces.IOwnerOverrideStore, IranRiskTracker.Infrastructure.Storage.InMemoryOwnerOverrideStore>();
builder.Services.AddScoped<IranRiskTracker.Application.Interfaces.IOwnerOverrideService, IranRiskTracker.Application.Services.OwnerOverrideService>();
builder.Services.AddScoped<IEventQueryService, EventQueryService>();
builder.Services.AddScoped<IRiskCalculator, RiskCalculator>();
builder.Services.AddSingleton<IranRiskTracker.Application.Interfaces.IRiskSnapshotStore, IranRiskTracker.Infrastructure.Storage.InMemoryRiskSnapshotStore>();
builder.Services.AddScoped<IranRiskTracker.Application.Interfaces.IDashboardSummaryService, IranRiskTracker.Application.Services.DashboardSummaryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
