using AirportsDistanceApi.Interfaces.Services;
using AirportsDistanceApi.Services;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json.Serialization;

namespace AirportsDistanceApi;

public class Program
{
    public static void Main(string[] args)
    {
        var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        logger.Debug("init main");

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            // NLog: Setup NLog for Dependency injection
            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            // Add services to the container.
            builder.Services.AddScoped<IDistanceCalculationService, DistanceCalculationService>();

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<ISimpleRestApiClientService, SimpleRestApiClientService>();

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                var info = new OpenApiInfo()
                {
                    Title = "Airports distance api",
                    Version = "v1"
                };
                c.SwaggerDoc("v1", info);

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(
                builder.Configuration.GetValue<string>("RedisConnectionString") ?? throw new Exception("RedisConnectionString should be supplied")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception exception)
        {
            // NLog: catch setup errors
            logger.Error(exception, "Stopped program because of exception");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLog.LogManager.Shutdown();
        }
    }
}
