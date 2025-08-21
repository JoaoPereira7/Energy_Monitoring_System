using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Energy_Monitoring_System.Infrastructure.Data;
using Energy_Monitoring_System.Infrastructure.Repositories;
using Energy_Monitoring_System.Domain.Interfaces;
using Energy_Monitoring_System.Application.Validators;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .CreateLogger();

    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("database"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

    builder.Services.AddScoped<ILeituraRepository, LeituraRepository>();
    builder.Services.AddScoped<LeituraInputDtoValidator>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema de Monitoramento Energético API", Version = "v1" });
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();