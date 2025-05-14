using CurrencyExchange.API;
using CurrencyExchange.Infrastructure.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .CreateLogger();

        try
        {
            Log.Information("Starting up the app...");
            var host = BuildWebHost(args);

            // Run migrations
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<BaseDbContext>();
                    context.Database.Migrate();
                    Log.Information("Migrations applied successfully");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Migration error");
                    throw;
                }
            }

            host.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IWebHost BuildWebHost(string[] args)
    {
        return WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog();
            })
            .Build();
    }
}