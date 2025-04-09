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
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.MessageCommands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.UserCommands;
using DSharpPlus.Extensions;
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

namespace Abyss.Web;

public class Program
{
    public static JsonSerializerOptions JsonSerializerOptions = ConfigureJsonOptions(new JsonSerializerOptions());

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder);

        var app = builder.Build();
        Configure(app);

        app.Run();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var config = builder.Configuration;

        services.AddControllers();
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
                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue<int>("Authentication:ExternalLogin:ValidMinutes"));
                options.Cookie.Name = AuthSchemes.ExternalLogin;
            })
            .AddCookie(AuthSchemes.RefreshToken, options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(config.GetValue<int>("Authentication:RefreshToken:ValidMinutes"));
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
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = Data.TokenType.Access.ToString(),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"])),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            })
            .AddSteam(AuthSchemes.Steam.Name, options =>
            {
                options.CallbackPath = "/auth/steam";
                options.ApplicationKey = config["Authentication:Steam:ApplicationKey"];
                options.SignInScheme = AuthSchemes.ExternalLogin;
            })
            .AddGoogle(AuthSchemes.Google.Name, options =>
            {
                options.CallbackPath = "/auth/google";
                options.ClientId = config["Authentication:Google:ClientId"];
                options.ClientSecret = config["Authentication:Google:ClientSecret"];
                options.SignInScheme = AuthSchemes.ExternalLogin;
            })
            .AddDiscord(AuthSchemes.Discord.Name, options =>
            {
                options.CallbackPath = "/auth/discord";
                options.ClientId = config["Authentication:Discord:ClientId"];
                options.ClientSecret = config["Authentication:Discord:ClientSecret"];
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
        services.AddSingleton<IMinecraftHelper, MinecraftHelper>();
        services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        services.AddTransient<IWhoSaidHelper, WhoSaidHelper>();
        services.AddHttpClient();
        services.AddHttpClient<IGPTClient, GPTClient>(client =>
        {
            client.BaseAddress = new Uri(config["GPTClient:BaseUrl"]);
        });
        services.AddHttpClient<ISpaceEngineersHelper, SpaceEngineersHelper>();
        services.AddHttpClient<IOvhHelper, OvhHelper>(client =>
        {
            client.BaseAddress = new Uri(config["Ovh:BaseUrl"]);
        });
        services.AddTransient<IKubernetesHelper, KubernetesHelper>();
        services.AddDbContext<AbyssContext>(options =>
            options.UseNpgsql(config.GetConnectionString("Abyss")));

        // Check database connection
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AbyssContext>();
            if (!dbContext.Database.CanConnect())
            {
                throw new Exception("Failed to connect to database. Check connection string and database server.");
            }
        }

        services.AddPredictionEnginePool<InputData, Prediction>().FromFile(config["WhoSaidIt:ModelPath"]);

        services.AddDiscordClient(config["Discord:Token"], DiscordIntents.GuildMembers | SlashCommandProcessor.RequiredIntents)
            .ConfigureEventHandlers(eventBuilder =>
            {
                eventBuilder.HandleGuildMemberRemoved(async (client, e) =>
                {
                    var commands = client.ServiceProvider.GetServices<IDiscordCommand>();
                    await Task.WhenAll(commands.Select(x => x.MemberRemoved(e)).ToArray());
                });
            });

        services.AddCommandsExtension((serviceProvider, commandsExtension) =>
            {
                commandsExtension.AddCommands(typeof(Program).Assembly);
                commandsExtension.AddProcessors(
                    new SlashCommandProcessor(),
                    new UserCommandProcessor(),
                    new MessageCommandProcessor()
                );
            },
            new CommandsConfiguration()
            {
                DebugGuildId = config.GetValue<ulong?>("Discord:GuildId") ?? default,
                RegisterDefaultCommandProcessors = false
            }
        );

        services.AddSingleton<TumblrClientFactory>();
        services.AddTransient(serviceProvider =>
            serviceProvider.GetRequiredService<TumblrClientFactory>().Create<TumblrClient>(
                config["Tumblr:ConsumerKey"], config["Tumblr:ConsumerSecret"], new DontPanic.TumblrSharp.OAuth.Token(config["Tumblr:Token"], config["Tumblr:TokenSecret"])));

        var azureCredentials = new ClientSecretCredential(
            config["Azure:TenantId"],
            config["Azure:ClientId"],
            config["Azure:ClientSecret"]
        );

        services.AddTransient(serviceProvider => new ArmClient(azureCredentials, config["Azure:SubscriptionId"]));
        services.AddTransient<IDiscordCommand, RegisterCommand>();
        services.AddTransient<IDiscordCommand, PingCommand>();
        services.AddTransient<IDiscordCommand, QuoteCommand>();
        services.AddTransient<IDiscordCommand, ServerCommand>();
        services.Configure<JwtOptions>(config.GetSection("Jwt"));
        services.Configure<AuthenticationOptions>(config.GetSection("Authentication"));
        services.Configure<CleanupOptions>(config.GetSection("Services:Cleanup"));
        services.Configure<CloudflareOptions>(config.GetSection("Cloudflare"));
        services.Configure<OvhOptions>(config.GetSection("Ovh"));
        services.Configure<DiscordOptions>(config.GetSection("Discord"));
        services.Configure<TeamSpeakOptions>(config.GetSection("TeamSpeak"));
        services.Configure<TumblrOptions>(config.GetSection("Tumblr"));
        services.Configure<ReminderOptions>(config.GetSection("Reminder"));
        services.Configure<QuoteOfTheDayOptions>(config.GetSection("QuoteOfTheDay"));
        services.Configure<AzureOptions>(config.GetSection("Azure"));
        services.Configure<GModOptions>(config.GetSection("GMod"));
        services.Configure<PushoverOptions>(config.GetSection("Pushover"));
        services.Configure<WebhookRelayOptions>(config.GetSection("WebhookRelay"));
        services.AddHttpContextAccessor();
        services.AddHttpClient("cloudflare", options =>
        {
            options.BaseAddress = new Uri(config["Cloudflare:BaseUrl"]);
            options.DefaultRequestHeaders.Add("X-Auth-Email", config["Cloudflare:Email"]);
            options.DefaultRequestHeaders.Add("X-Auth-Key", config["Cloudflare:ApiKey"]);
        });
        services.AddHttpClient("gmod", options =>
        {
            options.BaseAddress = new Uri(config["GMod:BaseUrl"]);
            options.DefaultRequestHeaders.Add("ApiKey", config["GMod:ApiKey"]);
        });
        services.AddHttpClient<INotificationHelper, PushoverHelper>(x => x.BaseAddress = new Uri(config.GetValue<string>("Pushover:BaseUrl")));
        services.AddHostedService<CleanupService>();
        services.AddHostedService<DiscordService>();
        services.AddHostedService<TeamSpeakService>();
        services.AddHostedService<ReminderService>();
        services.AddHostedService<BackgroundWorkerService>();
        services.AddCronJob<QuoteOfTheDayService>(c =>
        {
            c.TimeZoneInfo = TimeZoneInfo.Utc;
            c.CronExpression = config["QuoteOfTheDay:CronExpression"];
        });

        ErrorStore errorStore = new MemoryErrorStore();
        Exceptional.Configure(settings =>
        {
            settings.DefaultStore = errorStore;
            config.GetSection("Exceptional").Bind(settings);
        });
        services.AddExceptional(settings =>
        {
            settings.DefaultStore = errorStore;
            config.GetSection("Exceptional").Bind(settings);
        });

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddDebug();
            loggingBuilder.AddConsole();
            loggingBuilder.AddExceptional();
        });

        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/dist/browser";
        });

        services.AddSignalR().AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions = JsonSerializerOptions;
        });

        services.AddRouting();

        services.AddHealthChecks();
    }

    public static void Configure(WebApplication app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        app.UseHttpsRedirection();

        if (!app.Environment.IsDevelopment())
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

        app.MapHealthChecks("/healthz");
        app.MapHub<OnlineHub>("/hub/online");
        app.MapHub<ServerManagerHub>("/hub/servermanager");
        app.MapControllers();
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

        app.UseWhen(ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api/webhookrelay")), then => then.Use(async (context, next) =>
        {
            context.Request.EnableBuffering();
            await next();
        }));

        var excludedPaths = new PathString[] { "/api", "/hub", "/healthz" };
        app.UseWhen((ctx) =>
        {
            var path = ctx.Request.Path;
            return !Array.Exists(excludedPaths, excluded => path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase));
        }, then =>
        {
            if (app.Environment.IsProduction())
            {
                then.UseSpaStaticFiles();
            }

            then.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (app.Environment.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:34564");
                }
            });
        });
    }

    private static JsonSerializerOptions ConfigureJsonOptions(JsonSerializerOptions o)
    {
        o.PropertyNameCaseInsensitive = true;
        o.PropertyNamingPolicy = null;
        o.DictionaryKeyPolicy = null;
        o.IncludeFields = true;
        return o;
    }
}
