using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using snoopy_airlines_backend.Repositories;
using snoopy_airlines_backend.Services;
using SnoopyAirlines.Infrastructure.Dapper;
using SnoopyAirlines.Infrastructure.Json;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using SnoopyAirlines.Services.PartnerAirlines;
using SnoopyAirlines.Util.Email;
using SnoopyAirlines.Util.Pdf;
using System.Text;
using Dapper;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);
//builder.Configuration.AddJsonFile("appsettings.local-jordan.json", optional: true, reloadOnChange: true);
SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());


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
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRouteRepository, RouteRepository>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
    });
builder.Services.AddScoped<RouteService>();
builder.Services.AddScoped<IRouteService>(serviceProvider => serviceProvider.GetRequiredService<RouteService>());
builder.Services.AddScoped(sp => new UserRepository(builder.Configuration));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped(sp => new AirportRepository(builder.Configuration));
builder.Services.AddScoped<IAirportRepository, AirportRepository>();
builder.Services.AddScoped<IAirportService, AirportService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<IAirplaneRepository, AirplaneRepository>();
builder.Services.AddScoped<IAirplaneService, AirplaneService>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<PurchaseOrderRepository>();
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<BookingRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IPartnerAirlineRepository, PartnerAirlineRepository>();
builder.Services.AddScoped<IExternalFlightSearchService, ExternalFlightSearchService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IInvoicePdfGenerator, InvoicePdfGenerator>();
builder.Services.AddScoped<IModifyLuggageService, ModifyLuggageService>();
builder.Services.AddScoped<IPassengerLuggageRepository, PassengerLuggageRepository>();
builder.Services.AddScoped<IModifyLuggageService, ModifyLuggageService>();
builder.Services.AddScoped<IModifyLuggageRepository, ModifyLuggageRepository>();
builder.Services.AddScoped<IPassengerLuggageRepository, PassengerLuggageRepository>();
builder.Services.AddScoped<IFlightLuggageRepository, FlightLuggageRepository>();
builder.Services.AddScoped<BookingService>();
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

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT bearer token."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
