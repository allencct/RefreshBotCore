using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
//using Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
using RefreshBot.DataAccess;
using RefreshBot.Services;

namespace RefreshBot
{
    class Program
    {
        //static void Main(string[] args)
        //{
        //    try
        //    {
        //        new Bot().StartAsync().GetAwaiter().GetResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}
        private static async Task Main()
        {
            //.ConfigureDiscordHost()
            //var builder = Host.CreateDefaultBuilder()
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                //.ConfigureLogging(x =>
                //{
                //    x.AddConsole();
                //    x.SetMinimumLevel(LogLevel.Debug);
                //})
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                    };

                    config.Token = context.Configuration["Token"];
                })
                .UseCommandService()
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddHostedService<CommandHandler>();
                    services.AddEntityFrameworkNpgsql().AddDbContext<EntityContext>(options => options.UseNpgsql("Host=host.docker.internal;Port=5432;Database=refresh;Username=postgres;Password=password"));
                });
                //.UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
