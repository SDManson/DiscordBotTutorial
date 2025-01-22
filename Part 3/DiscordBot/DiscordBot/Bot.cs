using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DiscordBot
{
    public class Bot : IBot
    {
        private ServiceProvider? _serviceProvider;

        private readonly ILogger<Bot> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public Bot(ILogger<Bot> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            DiscordSocketConfig config = new()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
        }

        public async Task StartAsync(ServiceProvider services)
        {
            string discordToken = _configuration["DiscordToken"] ?? throw new Exception("Missing Discord token");

            _serviceProvider = services;


            _logger.LogInformation($"Starting up with token {discordToken}");

            _serviceProvider = services;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            _client.MessageReceived += HandleCommandAsync;


            DumpChannels(_client);
        }

        public async Task StopAsync()
        {
            if (_client != null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
            }
        }


        private async Task HandleCommandAsync(SocketMessage arg)
        {
            // Ignore messages from bots
            if (arg is not SocketUserMessage message || message.Author.IsBot)
            {
             //   _logger.LogInformation($"Message {message.Author}")
                DumpSocketMessage(arg);
                _logger.LogInformation($"Ignoring message {arg} ");
                return;
            }
            _logger.LogInformation($"{DateTime.Now.ToShortTimeString()}: {message.Author} {message.Channel} : {message.Content}");
            // Check if the message starts with !
            int position = 0;
            bool messageIsCommand = message.HasCharPrefix('!', ref position);

            if (messageIsCommand)
            {
                // Execute the command if it exists in the ServiceCollection
                await _commands.ExecuteAsync(
                    new SocketCommandContext(_client, message),
                    position,
                    _serviceProvider);

                return;
            }
            // check if message is in the pathfinder-updates channel
            if (message.Channel.ToString().Contains("-updates"))
            {
                _logger.LogInformation($"{message.Author} -- {message.Channel} {message.Content}");
                DiscordSocketClient client = _client;
                ulong channelNum = 1087039208260124856;
                var chnl = _client.GetChannel(channelNum) as IMessageChannel;
                //_ = await chnl.SendMessageAsync(message.Content);
                return;
            }


        }


        private void DumpChannels(DiscordSocketClient client)
        {
            foreach (var guild in client.Guilds)
            {
                _logger.LogInformation($"Guild: {guild.Name}");
                foreach (var channel in guild.Channels)
                {
                    _logger.LogInformation($"Channel: {channel.Name} : {channel.Id}");
                }
            }
        }

        private void DumpSocketMessage(SocketMessage arg)
        {
        //    _logger.LogInformation($"Message: {arg.Content}");
            _logger.LogInformation($"Author: {arg.Author}");
        //    _logger.LogInformation($"Channel: {arg.Channel}");
            _logger.LogInformation($"Embeds: {arg.Embeds}");
            _logger.LogInformation($"----------------------");
            Embed? embed = arg.Embeds.FirstOrDefault();

            if (embed != null)
            {
                _logger.LogInformation($"Embed: {nameof(embed.Title)}: {embed.Title}");
                _logger.LogInformation($"Embed: {nameof(embed.Description)}: {embed.Description}");
                //_logger.LogInformation($"Embed: {nameof(embed.Url)}: {embed.Url}");
                //_logger.LogInformation($"Embed: {nameof(embed.Type)}: {embed.Type}");
                _logger.LogInformation($"Embed: {nameof(embed.Timestamp)}: {embed.Timestamp}");
                _logger.LogInformation($"Embed: {nameof(embed.Author)}: {embed.Author}");
                // _logger.LogInformation($"Embed: {nameof(embed.Footer)}: {embed.Footer}");
                //_logger.LogInformation($"Embed: {nameof(embed.Image)}: {embed.Image}");
                //_logger.LogInformation($"Embed: {nameof(embed.Thumbnail)}: {embed.Thumbnail}");
                //_logger.LogInformation($"Embed: {nameof(embed.Video)}: {embed.Video}");
                //_logger.LogInformation($"Embed: {nameof(embed.Provider)}: {embed.Provider}");
                _logger.LogInformation($"Embed: {nameof(embed.Fields)}: {embed.Fields}");
                _logger.LogInformation($"------");
                if (embed.Fields != null)
                {
                    EmbedField? field = embed.Fields.FirstOrDefault();
                    _logger.LogInformation($"Fields  {field.Value}");
                }
            }
        }
    }
}