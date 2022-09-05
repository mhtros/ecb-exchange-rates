using System.Net;
using System.Reflection;
using EcbWebApp.Database;
using EcbWebApp.HostedServices;
using EcbWebApp.Repositories;
using EcbWebApp.Services;
using EuropeanCentralBank;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.UseSerilog((_, loggerConfiguration) =>
{
    loggerConfiguration.MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Logger(lc => lc.WriteTo.Console())
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly("RequestPath like '/api/v1/Rates'")
            .WriteTo.File(new JsonFormatter(), "currencyRates.txt", rollingInterval: RollingInterval.Day));
});

// Add services to the container.

builder.Services.AddControllers();

#region Swagger

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "ν1",
        Title = "Code Canvas",
        Description = "Project assignment based on the concept of currency exchange rates.",
        License = new OpenApiLicense {Name = "No License"}
    });

    // Setup swagger to display XML comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add swagger versioning
builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
    config.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("x-api-version"),
        new QueryStringApiVersionReader("api-version")
    );
});

#endregion

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    options.UseSqlite(connectionString);
});

builder.Services.AddEuropeanCentralBank(configuration);

builder.Services.AddHostedService<UpdateRatesHostedService>();

builder.Services.AddScoped<ICurrencyRatesRepository, CurrencyRatesRepository>();

builder.Services.AddScoped<IWalletRepository, WalletRepository>();

builder.Services.AddScoped<IWalletAdjustmentService, WalletAdjustmentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

#region Global Exception Handler

app.UseExceptionHandler(c => c.Run(async context =>
{
    var isDevelopment = app.Environment.IsDevelopment();

    var exception = context.Features
        .Get<IExceptionHandlerPathFeature>()
        ?.Error;

    var response = new
    {
        Error = isDevelopment ? exception?.Message : "Internal Server Error. Please contact the administrators",
        Source = isDevelopment ? exception?.Source : string.Empty,
        StackTrace = isDevelopment ? exception?.StackTrace : string.Empty
    };

    context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
    await context.Response.WriteAsJsonAsync(response);
}));

#endregion

app.UseRouting();

app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();