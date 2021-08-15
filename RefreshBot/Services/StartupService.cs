using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace RefreshBot.Services
{
    public class StartupService
    {
        private readonly DiscordShardedClient _discord;
        private readonly CommandService _commandService;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _services;
        private DiscordSocketClient _client;
        
        public StartupService(DiscordSocketClient client, CommandService commandService){
            _client = client;
            _commandService = commandService;
        }

        public async Task StartAsync()
        {

            // When working with events that have Cacheable<IMessage, ulong> parameters,
            // you must enable the message cache in your config settings if you plan to
            // use the cached message entity. '
            //var _config = new DiscordSocketConfig { MessageCacheSize = 100 };

            var token = "token";
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("Token missing from config.json! Please enter your token there (root directory)");
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.MessageUpdated += MessageUpdated;
            _client.Ready += () =>
            {
                Console.WriteLine("Bot is connected!");
                return Task.CompletedTask;
            };
        }

        private async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            // If the message was not in the cache, downloading it will result in getting a copy of `after`.
            var message = await before.GetOrDownloadAsync();
            Console.WriteLine($"{message} -> {after}");
        }
    }
}