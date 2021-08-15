using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RefreshBot.DataAccess;
using RefreshBot.Services;
using System.Configuration;
using System.Threading.Tasks;

namespace RefreshBot
{
    class Bot
    {
        public async Task StartAsync()
        {
            var services = new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<ScheduleService>()
                .AddSingleton<StartupService>();

            ConfigureServices(services);

            //Build services
            var serviceProvider = services.BuildServiceProvider();

            //Start the bot
            await serviceProvider.GetRequiredService<StartupService>().StartAsync();

            //Load up services
            serviceProvider.GetRequiredService<CommandHandler>();
            //serviceProvider.GetRequiredService<UserInteraction>();

            await Task.Delay(-1);


            //_client = new DiscordSocketClient();

            //_client.Log += Log;

            ////  You can assign your bot token to a string, and pass that in to connect.
            ////  This is, however, insecure, particularly if you plan to have your code hosted in a public repository.
            //var token = "ODQyOTcxMzA2ODUyNzQ1Mjc2.YJ9ENQ.09jw3ZKM-aGoiP89iWdNxOth_yE";

            //// Some alternative options would be to keep your token in an Environment Variable or a standalone file.
            //// var token = Environment.GetEnvironmentVariable("NameOfYourEnvironmentVariable");
            //// var token = File.ReadAllText("token.txt");
            //// var token = JsonConvert.DeserializeObject<AConfigurationClass>(File.ReadAllText("config.json")).Token;

            //await _client.LoginAsync(TokenType.Bot, token);
            //await _client.StartAsync();

            //// Block this task until the program is closed.
            //await Task.Delay(-1);
            ////Create the configuration
            //var _builder = new ConfigurationBuilder()
            //    .SetBasePath(AppContext.BaseDirectory)
            //    .AddJsonFile(path: "config.json");
            //_config = _builder.Build();

            ////Configure services
            //var services = new ServiceCollection()
            //    .AddSingleton(new DiscordShardedClient(new DiscordSocketConfig
            //    {
            //        //LogLevel = LogSeverity.Debug,
            //        GatewayIntents =
            //            GatewayIntents.GuildMembers |
            //            GatewayIntents.GuildMessages |
            //            GatewayIntents.GuildIntegrations |
            //            GatewayIntents.Guilds |
            //            GatewayIntents.GuildBans |
            //            GatewayIntents.GuildVoiceStates |
            //            GatewayIntents.GuildEmojis |
            //            GatewayIntents.GuildInvites |
            //            GatewayIntents.GuildMessageReactions |
            //            GatewayIntents.GuildMessageTyping |
            //            GatewayIntents.GuildWebhooks |
            //            GatewayIntents.DirectMessageReactions |
            //            GatewayIntents.DirectMessages |
            //            GatewayIntents.DirectMessageTyping,
            //        LogLevel = LogSeverity.Error,
            //        MessageCacheSize = 1000,
            //    }))
            //    .AddSingleton(_config)
            //    .AddSingleton(new CommandService(new CommandServiceConfig
            //    {
            //        DefaultRunMode = RunMode.Async,
            //        LogLevel = LogSeverity.Verbose,
            //        CaseSensitiveCommands = false,
            //        ThrowOnError = false
            //    }))
            //    .AddHttpClient()
            //    .AddSingleton<WowApi>()
            //    .AddSingleton<WowUtilities>()
            //    .AddSingleton<WarcraftLogs>()
            //    .AddSingleton<ChannelCheck>()
            //    .AddSingleton<OxfordApi>()
            //    .AddSingleton<AwayCommands>()
            //    .AddSingleton<UserInteraction>()
            //    .AddSingleton<CommandHandler>()
            //    .AddSingleton<StartupService>()
            //    .AddSingleton<SteamApi>()
            //    .AddSingleton<GiphyApi>()
            //    .AddSingleton<WeatherApi>()
            //    .AddSingleton<RaiderIOApi>()
            //    .AddSingleton<YouTubeApi>()
            //    .AddSingleton<AudioService>()
            //    .AddSingleton<LoggingService>();

            ////Add logging      
            //ConfigureServices(services);

            ////Build services
            //var serviceProvider = services.BuildServiceProvider();

            ////Instantiate logger/tie-in logging
            //serviceProvider.GetRequiredService<LoggingService>();

            ////Start the bot
            //await serviceProvider.GetRequiredService<StartupService>().StartAsync();

            ////Load up services
            //serviceProvider.GetRequiredService<CommandHandler>();
            //serviceProvider.GetRequiredService<UserInteraction>();

            ////Block this program until it is closed.
            //await Task.Delay(-1);
        }
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFrameworkNpgsql().AddDbContext<EntityContext>(options => options.UseNpgsql("Host=localhost;Port=5432;Database=refresh;Username=postgres;Password=password"));


            //Add SeriLog
            //services.AddLogging(configure => configure.AddSerilog());
            ////Remove default HttpClient logging as it is extremely verbose
            //services.RemoveAll<IHttpMessageHandlerBuilderFilter>();
            ////Configure logging level              
            //var logLevel = Environment.GetEnvironmentVariable("NJA_LOG_LEVEL");
            //var level = Serilog.Events.LogEventLevel.Error;
            //if (!string.IsNullOrEmpty(logLevel))
            //{
            //    switch (logLevel.ToLower())
            //    {
            //        case "error":
            //            {
            //                level = Serilog.Events.LogEventLevel.Error;
            //                break;
            //            }
            //        case "info":
            //            {
            //                level = Serilog.Events.LogEventLevel.Information;
            //                break;
            //            }
            //        case "debug":
            //            {
            //                level = Serilog.Events.LogEventLevel.Debug;
            //                break;
            //            }
            //        case "crit":
            //            {
            //                level = Serilog.Events.LogEventLevel.Fatal;
            //                break;
            //            }
            //        case "warn":
            //            {
            //                level = Serilog.Events.LogEventLevel.Warning;
            //                break;
            //            }
            //        case "trace":
            //            {
            //                level = Serilog.Events.LogEventLevel.Debug;
            //                break;
            //            }
            //    }
            //}
            //Log.Logger = new LoggerConfiguration()
            //        .WriteTo.File("logs/njabot.log", rollingInterval: RollingInterval.Day)
            //        .WriteTo.Console()
            //        .MinimumLevel.Is(level)
            //        .CreateLogger();
        }
    }
}
