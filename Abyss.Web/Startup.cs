using Abyss.Web.Clients;
using Abyss.Web.Clients.Interfaces;
using Abyss.Web.Commands.Discord;
using Abyss.Web.Commands.Discord.Interfaces;
using Abyss.Web.Contexts;
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
using Azure.Identity;
using Azure.ResourceManager;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DSharpPlus;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.ML;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Exceptional;
using StackExchange.Exceptional.Stores;
using System.Text;
using System.Text.Json;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Abyss.Web;

public class Startup
{
    public readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public static JsonSerializerOptions JsonSerializerOptions = ConfigureJsonOptions(new JsonSerializerOptions());

    public Startup(IWebHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>(true);


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
            .AddJsonOptions(o => ConfigureJsonOptions(o.JsonSerializerOptions));
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();
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
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            })
            .AddSteam(AuthSchemes.Steam.Name, options =>
            {
                options.CallbackPath = "/auth/steam";
                options.ApplicationKey = _config["Authentication:Steam:ApplicationKey"];
                options.SignInScheme = AuthSchemes.ExternalLogin;
            })
            .AddGoogle(AuthSchemes.Google.Name, options =>
            {
                options.CallbackPath = "/auth/google";
                options.ClientId = _config["Authentication:Google:ClientId"];
                options.ClientSecret = _config["Authentication:Google:ClientSecret"];
                options.SignInScheme = AuthSchemes.ExternalLogin;
            })
            .AddDiscord(AuthSchemes.Discord.Name, options =>
            {
                options.CallbackPath = "/auth/discord";
                options.ClientId = _config["Authentication:Discord:ClientId"];
                options.ClientSecret = _config["Authentication:Discord:ClientSecret"];
                options.Scope.Add("identify");
                options.SignInScheme = AuthSchemes.ExternalLogin;
            });

        services.AddTransient<IUserManager, UserManager>();
        services.AddTransient<IOnlineManager, OnlineManager>();
        services.AddTransient<IWhoSaidManager, WhoSaidManager>();
        services.AddTransient<IGPTManager, GPTManager>();
        services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IRoleRepository, RoleRepository>();
        services.AddTransient<IGPTModelRepository, GPTModelRepository>();
        services.AddTransient<IUserHelper, UserHelper>();
        services.AddTransient<IServerManager, ServerManager>();
        services.AddTransient<ICloudflareHelper, CloudflareHelper>();
        services.AddTransient<IAzureHelper, AzureHelper>();
        services.AddTransient<IGModHelper, GModHelper>();
        services.AddSingleton<ITeamSpeakHelper, TeamSpeakHelper>();
        services.AddSingleton<IQuoteHelper, QuoteHelper>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddTransient<IWhoSaidHelper, WhoSaidHelper>();
        services.AddHttpClient();
        services.AddHttpClient<IGPTClient, GPTClient>(client =>
        {
            client.BaseAddress = new Uri(_config["GPTClient:BaseUrl"]);
        });
        services.AddHttpClient<ISpaceEngineersHelper, SpaceEngineersHelper>();
        services.AddDbContext<AbyssContext>(options =>
            options.UseNpgsql(_config.GetConnectionString("Abyss")));

        services.AddPredictionEnginePool<InputData, Prediction>().FromFile(_config["WhoSaidIt:ModelPath"]);

        services.AddTransient(_ => new DiscordClient(new DiscordConfiguration
        {
            Token = _config["Discord:Token"],
            TokenType = DSharpPlus.TokenType.Bot
        }));
        services.AddSingleton<TumblrClientFactory>();
        services.AddTransient(serviceProvider =>
            serviceProvider.GetRequiredService<TumblrClientFactory>().Create<TumblrClient>(
                _config["Tumblr:ConsumerKey"], _config["Tumblr:ConsumerSecret"], new DontPanic.TumblrSharp.OAuth.Token(_config["Tumblr:Token"], _config["Tumblr:TokenSecret"])));

        var azureCredentials = new ClientSecretCredential(
            _config["Azure:TenantId"],
            _config["Azure:ClientId"],
            _config["Azure:ClientSecret"]
        );

        services.AddTransient(serviceProvider => new ArmClient(azureCredentials, _config["Azure:SubscriptionId"]));
        services.AddTransient<IDiscordCommand, RegisterCommand>();
        services.AddTransient<IDiscordCommand, PingCommand>();
        services.AddTransient<IDiscordCommand, QuoteCommand>();
        services.AddTransient<IDiscordCommand, ServerCommand>();
        services.Configure<JwtOptions>(_config.GetSection("Jwt"));
        services.Configure<AuthenticationOptions>(_config.GetSection("Authentication"));
        services.Configure<CleanupOptions>(_config.GetSection("Services:Cleanup"));
        services.Configure<CloudflareOptions>(_config.GetSection("Cloudflare"));
        services.Configure<DiscordOptions>(_config.GetSection("Discord"));
        services.Configure<TeamSpeakOptions>(_config.GetSection("TeamSpeak"));
        services.Configure<TumblrOptions>(_config.GetSection("Tumblr"));
        services.Configure<ReminderOptions>(_config.GetSection("Reminder"));
        services.Configure<QuoteOfTheDayOptions>(_config.GetSection("QuoteOfTheDay"));
        services.Configure<AzureOptions>(_config.GetSection("Azure"));
        services.Configure<GModOptions>(_config.GetSection("GMod"));
        services.Configure<PushoverOptions>(_config.GetSection("Pushover"));
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
        services.AddHttpClient<INotificationHelper, PushoverHelper>(x => x.BaseAddress = new Uri(_config.GetValue<string>("Pushover:BaseUrl")));
        services.AddHostedService<CleanupService>();
        services.AddHostedService<DiscordService>();
        services.AddHostedService<TeamSpeakService>();
        services.AddHostedService<ReminderService>();
        services.AddHostedService<BackgroundWorkerService>();
        services.AddCronJob<QuoteOfTheDayService>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Utc;
            c.CronExpression = _config["QuoteOfTheDay:CronExpression"];
        });

        ErrorStore errorStore = new MemoryErrorStore();
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
            loggingBuilder.AddConsole();
            loggingBuilder.AddExceptional();
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

        services.AddHealthChecks();
    }

    private static JsonSerializerOptions ConfigureJsonOptions(JsonSerializerOptions o)
    {
        o.PropertyNameCaseInsensitive = true;
        o.PropertyNamingPolicy = null;
        o.DictionaryKeyPolicy = null;
        o.IncludeFields = true;
        return o;
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
            routes.MapHealthChecks("/healthz");
            routes.MapHub<OnlineHub>("/hub/online");
            routes.MapHub<ServerManagerHub>("/hub/servermanager");
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
