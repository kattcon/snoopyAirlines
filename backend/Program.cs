using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:8080");
                          policy.AllowAnyMethod();
                          policy.AllowAnyHeader();
                      });
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<FlightRepository>();
builder.Services.AddScoped<FlightService>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AirportRepository>();
builder.Services.AddScoped<AirportService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AirportRepository>();
builder.Services.AddScoped<AirportService>();
builder.Services.AddScoped<SmtpEmailSender>();
builder.Services.AddScoped<ConsoleEmailSender>();
builder.Services.AddScoped<IEmailSender>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

    if (string.IsNullOrWhiteSpace(configuration["Email:Host"]))
    {
        return serviceProvider.GetRequiredService<ConsoleEmailSender>();
    }

    return serviceProvider.GetRequiredService<SmtpEmailSender>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(MyAllowSpecificOrigins);

app.MapControllers();

app.Run();
