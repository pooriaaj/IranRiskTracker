var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Register application services
builder.Services.AddScoped<IranRiskTracker.Application.Interfaces.IRiskCalculator, IranRiskTracker.Application.Services.RiskCalculator>();
// Allow configuration for seed paths if desired
builder.Services.AddSingleton(builder.Configuration);
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
