using Abyss.Web.Commands.Discord;
using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Contexts;
using Abyss.Web.Contexts.Interfaces;
using Abyss.Web.Data;
using Abyss.Web.Data.Options;
using Abyss.Web.Helpers;
using Abyss.Web.Helpers.Interfaces;
using Abyss.Web.Hubs;
using Abyss.Web.Logging;
using Abyss.Web.Managers;
using Abyss.Web.Managers.Interfaces;
using Abyss.Web.Middleware;
using Abyss.Web.Repositories;
using Abyss.Web.Repositories.Interfaces;
using Abyss.Web.Services;
using DigitalOcean.API;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DSharpPlus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ML;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
using System;
using System.Text;
using System.Text.Json;

namespace Abyss.Web
{
    public class Startup
    {
        public readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;

        public static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
            DictionaryKeyPolicy = null
        };

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            if (!env.IsDevelopment())
            {
                builder.AddJsonFile($"appsettings.Release.json", optional: false, reloadOnChange: true);
            }

            _config = builder.Build();
            _env = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = true;
                options.InputFormatters.Add(new TextPlainInputFormatter());
            })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddJsonOptions(o =>
                {
                    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    o.JsonSerializerOptions.PropertyNamingPolicy = null;
                    o.JsonSerializerOptions.DictionaryKeyPolicy = null;
                });
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });
            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = AuthSchemes.JsonWebToken;
                })
                .AddCookie(AuthSchemes.ExternalLogin, options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(_config.GetValue<int>("Authentication:ExternalLogin:ValidMinutes"));
                    options.Cookie.Name = AuthSchemes.ExternalLogin;
                })
                .AddCookie(AuthSchemes.RefreshToken, options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(_config.GetValue<int>("Authentication:RefreshToken:ValidMinutes"));
                    options.Cookie.Name = AuthSchemes.RefreshToken;
                })
                .AddJwtBearer(AuthSchemes.JsonWebToken, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidAudience = Data.TokenType.Access.ToString(),
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                        ClockSkew = TimeSpan.FromHours(DateTime.Now.Hour - DateTimeOffset.UtcNow.Hour)
                    };
                })
                .AddSteam(AuthSchemes.Steam.Id, options =>
                {
                    options.CallbackPath = "/auth/steam";
                    options.ApplicationKey = _config["Authentication:Steam:ApplicationKey"];
                    options.SignInScheme = AuthSchemes.ExternalLogin;
                })
                .AddGoogle(AuthSchemes.Google.Id, options =>
                {
                    options.CallbackPath = "/auth/google";
                    options.ClientId = _config["Authentication:Google:ClientId"];
                    options.ClientSecret = _config["Authentication:Google:ClientSecret"];
                    options.SignInScheme = AuthSchemes.ExternalLogin;
                })
                .AddDiscord(AuthSchemes.Discord.Id, options =>
                {
                    options.CallbackPath = "/auth/discord";
                    options.AppId = _config["Authentication:Discord:ClientId"];
                    options.AppSecret = _config["Authentication:Discord:ClientSecret"];
                    options.Scope.Add("identify");
                    options.SignInScheme = AuthSchemes.ExternalLogin;
                });

            services.AddTransient<IUserManager, UserManager>();
            services.AddTransient<IOnlineManager, OnlineManager>();
            services.AddTransient<IWhoSaidManager, WhoSaidManager>();
            services.AddTransient<IAbyssContext, AbyssContext>();
            services.AddTransient<IMongoClient>(_ => new MongoClient(_config.GetConnectionString("Abyss")));
            services.AddTransient(serviceProvider => serviceProvider.GetRequiredService<IMongoClient>().GetDatabase(_config["Database:Name"]));
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserHelper, UserHelper>();
            services.AddTransient<IDigitalOceanHelper, DigitalOceanHelper>();
            services.AddTransient(_ => new DigitalOceanClient(_config["DigitalOcean:ApiKey"]));
            services.AddTransient<IServerManager, ServerManager>();
            services.AddTransient<ICloudflareHelper, CloudflareHelper>();
            services.AddTransient<IAzureHelper, AzureHelper>();
            services.AddTransient<IGModHelper, GModHelper>();
            services.AddSingleton<ITeamSpeakHelper, TeamSpeakHelper>();
            services.AddTransient<IWhoSaidHelper, WhoSaidHelper>();

            services.AddPredictionEnginePool<InputData, Prediction>().FromFile("Models/whosaidit.zip");

            services.AddTransient(_ => new DiscordClient(new DiscordConfiguration
            {
                Token = _config["Discord:Token"],
                TokenType = DSharpPlus.TokenType.Bot
            }));
            services.AddSingleton<TumblrClientFactory>();
            services.AddTransient(serviceProvider =>
                serviceProvider.GetRequiredService<TumblrClientFactory>().Create<TumblrClient>(
                    _config["Tumblr:ConsumerKey"], _config["Tumblr:ConsumerSecret"], new DontPanic.TumblrSharp.OAuth.Token(_config["Tumblr:Token"], _config["Tumblr:TokenSecret"])));
            services.AddTransient(serviceProvider => Azure.Authenticate("azure.json").WithSubscription(_config["Azure:SubscriptionId"]));
            //services.AddTransient<IDiscordCommand, AddonsCommand>();
            services.AddTransient<IDiscordCommand, RegisterCommand>();
            services.AddTransient<IDiscordCommand, PingCommand>();
            services.AddTransient<IDiscordCommand, QuoteCommand>();
            services.AddTransient<IDiscordCommand, ServerCommand>();
            services.Configure<JwtOptions>(_config.GetSection("Jwt"));
            services.Configure<AuthenticationOptions>(_config.GetSection("Authentication"));
            services.Configure<CleanupOptions>(_config.GetSection("Services:Cleanup"));
            services.Configure<DigitalOceanOptions>(_config.GetSection("DigitalOcean"));
            services.Configure<CloudflareOptions>(_config.GetSection("Cloudflare"));
            services.Configure<DiscordOptions>(_config.GetSection("Discord"));
            services.Configure<TeamSpeakOptions>(_config.GetSection("TeamSpeak"));
            services.Configure<TumblrOptions>(_config.GetSection("Tumblr"));
            services.Configure<ReminderOptions>(_config.GetSection("Reminder"));
            services.Configure<AzureOptions>(_config.GetSection("Azure"));
            services.AddHttpContextAccessor();
            services.AddHttpClient("cloudflare", options =>
            {
                options.BaseAddress = new Uri(_config["Cloudflare:BaseUrl"]);
                options.DefaultRequestHeaders.Add("X-Auth-Email", _config["Cloudflare:Email"]);
                options.DefaultRequestHeaders.Add("X-Auth-Key", _config["Cloudflare:ApiKey"]);
            });
            services.AddHttpClient("gmod", options =>
            {
                options.BaseAddress = new Uri(_config["GMod:BaseUrl"]);
                options.DefaultRequestHeaders.Add("ApiKey", _config["GMod:ApiKey"]);
            });
            services.AddHostedService<CleanupService>();
            services.AddHostedService<DiscordService>();
            services.AddHostedService<TeamSpeakService>();
            services.AddHostedService<ReminderService>();

            ErrorStore errorStore;
            var databaseLogging = _config.GetValue<bool>("Logging:Database");
            if (databaseLogging)
            {
                errorStore = new MongoDBErrorStore(_config.GetConnectionString("Abyss"), _config["Database:Name"], _config["ApplicationId"]);
            }
            else
            {
                errorStore = new MemoryErrorStore();
            }
            Exceptional.Configure(settings =>
            {
                settings.DefaultStore = errorStore;
                _config.GetSection("Exceptional").Bind(settings);
            });
            services.AddExceptional(settings =>
            {
                settings.DefaultStore = errorStore;
                _config.GetSection("Exceptional").Bind(settings);
            });

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddDebug();
                loggingBuilder.AddExceptional();
                if (databaseLogging)
                {
                    loggingBuilder.AddMongoDB(_config.GetConnectionString("Abyss"), _config["Database:Name"], _config["Logging:CollectionName"], _config.GetValue<int>("Logging:MaxEntries"), _config);
                }
            });

            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions = JsonSerializerOptions;
            });

            services.AddRouting();

            var pack = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("Abyss Conventions", pack, t => true);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseHttpsRedirection();

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseErrorHandlingMiddleware();

            app.UseErrorViewer();

            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".webmanifest"] = "application/manifest+json"; // https://github.com/aspnet/AspNetCore/issues/2442

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(routes =>
            {
                routes.MapHub<OnlineHub>("/hub/online");
                routes.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:34564");
                }
            });
        }
    }
}
