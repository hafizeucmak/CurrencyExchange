using CurrencyExchange.API.Extensions;
using CurrencyExchange.Application.Abstractions.Providers;
using CurrencyExchange.Application.CQRS.Queries.Currency;
using CurrencyExchange.Infrastructure.Configurations;
using CurrencyExchange.Infrastructure.DbContext;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog.Extensions.Logging;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text;
using CurrencyExchange.API.Middlewares;

namespace CurrencyExchange.API
{
    public class Startup
    {
        private readonly IHostEnvironment _env;
        public readonly string _apiTitle = "Currency Exchange API";
        public Startup(ILogger<Startup> logger, IConfiguration configuration, IHostEnvironment env)
        {
            Logger = logger;
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public ILogger<Startup> Logger { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            var configurationOptions = Configuration.GetSection("AppSettings").Get<ConfigurationOptions>();

            if (configurationOptions?.JwtSettings == null)
            {
                throw new InvalidOperationException("JwtSettings configuration is missing or invalid.");
            }

            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddMemoryCache();
            services.AddRepositories();
            services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();
            services.AddScoped<ICurrencyProvider, IFrankFurtherCurrencyProvider>();
            services.AddValidators();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = configurationOptions.JwtSettings.ValidIssuer,
                            ValidAudience = configurationOptions.JwtSettings.ValidAudience,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationOptions.JwtSettings.SecretKey))
                        };
                    });

           services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = _apiTitle, Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            if (configurationOptions != null)
            {
                services.AddDbContext<BaseDbContext>(configurationOptions);
            }

            services.AddLogging(options =>
            {
                if (!_env.IsDevelopment())
                {
                    options.AddProvider(new SerilogLoggerProvider(Log.Logger));
                }

            });

            if (_env.IsDevelopment())
            {
                services.AddLogging(config => config.AddConsole());
            }

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetCurrencyRateQuery).Assembly));
        }

        private void ConfigureSwaggerUI(SwaggerUIOptions options)
        {
            options.DocExpansion(DocExpansion.None);
            options.DisplayRequestDuration();
            options.SwaggerEndpoint("/swagger/v1/swagger.json", _apiTitle);
            options.InjectJavascript("https://code.jquery.com/jquery-3.6.0.min.js");
            options.InjectJavascript("../js/swagger-seed-dropdown-sorting.js");
        }

        public void Configure(IApplicationBuilder app)
        {
            if (_env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(ConfigureSwaggerUI);
            }

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
