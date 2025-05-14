using AspNetCoreRateLimit;
using CurrencyExchange.API.Extensions;
using CurrencyExchange.API.Filters;
using CurrencyExchange.API.Middlewares;
using CurrencyExchange.API.ResiliencePolicies;
using CurrencyExchange.Application.Abstractions.Providers;
using CurrencyExchange.Application.CQRS.Queries.Currency;
using CurrencyExchange.Application.Interfaces;
using CurrencyExchange.Application.Services;
using CurrencyExchange.Domain.Entites;
using CurrencyExchange.Infrastructure.Configurations;
using CurrencyExchange.Infrastructure.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Buffers.Text;
using System.Security.Claims;
using System.Text;

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
            var ipRateLimitOptions = Configuration.GetSection("IpRateLimiting").Get<IpRateLimitOptions>();


            if (configurationOptions?.JwtSettings == null)
            {
                throw new InvalidOperationException("JwtSettings configuration is missing or invalid.");
            }

            services.AddMemoryCache();
            services.AddInMemoryRateLimiting();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddRepositories();
            services.AddScoped<ICurrencyProviderFactory, CurrencyProviderFactory>();
            services.AddScoped<ICurrencyProvider, FrankFurtherCurrencyProvider>();
            services.AddHttpClient<FrankFurtherCurrencyProvider>();
            services.AddHttpClient<FrankFurtherCurrencyProvider>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<ConfigurationOptions>>().Value;
                client.BaseAddress = new Uri(configurationOptions.CurrencyProviders.FrankFurter.BaseUrl);
            })
            .AddPolicyHandler(PollyPolicies.GetRetryPolicy())
            .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy()); ;
            services.AddSingleton<ICurrencyValidator, CurrencyValidator>();
            services.AddValidators();
            services.ConfigureAppSettings(Configuration);
            services.AddScoped<ITokenService, TokenService>();

      
            services.AddIdentityCore<User>().AddRoles<IdentityRole>();

            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User, IdentityRole>>();

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
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationOptions.JwtSettings.SecretKey)),
                            // Fix this to match what's in your token
                            RoleClaimType = ClaimTypes.Role // Change from "role" to ClaimTypes.Role
                        };

                        // Add debugging to see what's happening
                        options.Events = new JwtBearerEvents
                        {
                            OnTokenValidated = context =>
                            {
                                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();
                                logger.LogInformation("Token validated successfully!");

                                // Log all claims for debugging
                                foreach (var claim in context.Principal.Claims)
                                {
                                    logger.LogInformation($"Claim type: {claim.Type}, Value: {claim.Value}");
                                }

                                // Check if user is in Admin role
                                var isAdmin = context.Principal.IsInRole("Admin");
                                logger.LogInformation($"Is user in Admin role: {isAdmin}");

                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = context =>
                            {
                                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();
                                logger.LogError($"Authentication failed: {context.Exception.Message}");
                                return Task.CompletedTask;
                            }
                        };
                    });

            // Then configure authorization
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
                    Description = "Please insert JWT with Bearer. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                      new OpenApiSecurityScheme
                      {
                        Reference = new OpenApiReference
                        {
                          Type = ReferenceType.SecurityScheme,
                          Id = "Bearer"
                        },
                        Scheme = "oauth2",
                        Name = "Bearer",
                        In = ParameterLocation.Header,

                      },
                      new List<string>()
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

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetConvertedCurrencyRate).Assembly));
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

            app.UseAuthentication();
            app.UseAuthorization(); 

            app.UseIpRateLimiting();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}