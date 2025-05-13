using CurrencyExchange.API.Filters;
using CurrencyExchange.Infrastructure.Configurations;
using CurrencyExchange.Infrastructure.Repositories;
using CurrencyExchange.Infrastructure.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CurrencyExchange.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private static readonly string AppSettings = "AppSettings";

        public static void ConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var appSettings = configuration.GetSection(nameof(AppSettings));
            services.Configure<ConfigurationOptions>(appSettings);
        }

        public static void AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericWriteRepository<>), typeof(GenericWriteRepository<>));
            services.AddScoped(typeof(IGenericReadRepository<>), typeof(GenericReadRepository<>));
        }

        public static void AddDbContext<TDbContext>(this IServiceCollection services, ConfigurationOptions configurationOptions)
        where TDbContext : DbContext
        {
            services.AddDbContext<TDbContext>(options =>
            {
                options.EnableSensitiveDataLogging(true);
                options.UseSqlServer(StringBuilderUtils.BuildConnectionString(configurationOptions), sqlOptions =>
                {
                    sqlOptions.CommandTimeout(120);
                });
            });
        }
      
        public static void AddValidators(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        }
    }
}
