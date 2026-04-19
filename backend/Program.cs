using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;

var builder = WebApplication.CreateBuilder(args);

foreach (var localSettingsFile in Directory
    .EnumerateFiles(builder.Environment.ContentRootPath, "appsettings.local*.json")
    .OrderBy(Path.GetFileName))
{
    builder.Configuration.AddJsonFile(Path.GetFileName(localSettingsFile), optional: true, reloadOnChange: true);
}

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IFlightService, FlightService>();

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
