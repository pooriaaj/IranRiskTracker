var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Register application services
builder.Services.AddScoped<IranRiskTracker.Application.Interfaces.IRiskCalculator, IranRiskTracker.Application.Services.RiskCalculator>();

// Seed data provider for JSON-first approach
var seedBase = Path.Combine(builder.Environment.ContentRootPath, "Seeding", "Data");
builder.Services.AddSingleton<IranRiskTracker.Application.Interfaces.ISeedDataProvider>(sp =>
    new IranRiskTracker.Infrastructure.Seeding.JsonSeedDataProvider(seedBase));

// Event query service
builder.Services.AddScoped<IranRiskTracker.Application.Interfaces.IEventQueryService, IranRiskTracker.Application.Services.EventQueryService>();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
