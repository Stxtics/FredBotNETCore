using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Database;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FredBotNETCore
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _provider;

        public CommandHandler(IServiceProvider provider)
        {
            _provider = provider;
            _client = _provider.GetService<DiscordSocketClient>();
            _commands = _provider.GetService<CommandService>();
            _client.MessageReceived += OnMessageReceived;
        }

        public async Task OnMessageReceived(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg))
            {
                return;
            }

            bool badMessage = false;
            if (msg.Author.IsWebhook)
            {
                HandleJVVerification(msg);
                return;
            }
            if (msg.Author.IsBot)
            {
                return;
            }

            if (msg.Channel is SocketTextChannel channel)
            {
                if (Extensions.GetLogChannel(channel.Guild) != null && channel != Extensions.GetLogChannel(channel.Guild))
                {
                    if (DiscordStaff.Get(channel.Guild.Id, "u-" + msg.Author.Id).Count <= 0 || (channel.Guild.GetUser(msg.Author.Id).Roles.Count > 1 && DiscordStaff.Get(channel.Guild.Id, "r-" + channel.Guild.GetUser(msg.Author.Id).Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0))
                    {
                        AutoMod mod = new AutoMod(_client);
                        badMessage = await mod.FilterMessage(msg, channel);
                    }
                }
            }
            if (!badMessage)
            {
                await HandleCommand(msg);
            }
        }

        public async Task HandleCommand(SocketUserMessage msg)
        {
            SocketCommandContext context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            string prefix = "/";
            if (context.Channel is SocketTextChannel)
            {
                prefix = Guild.Get(context.Guild).Prefix;
            }
            if (msg.HasStringPrefix(prefix, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                if (context.Channel is SocketTextChannel channel)
                {
                    if (channel.Guild.CurrentUser.GetPermissions(channel).SendMessages == false)
                    {
                        return;
                    }
                    else if (channel.Guild.CurrentUser.GetPermissions(channel).EmbedLinks == false)
                    {
                        await channel.SendMessageAsync("Error: I am missing permission **Embed Links**.");
                        return;
                    }
                }
                IResult result = await _commands.ExecuteAsync(context, argPos, _provider);

                LogCommandUsage(context);
                if (result.Error.HasValue && result.Error.Value == CommandError.Exception)
                {
                    if (result.Error.Value != CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync($"Oh no an error occurred. Details of this error have been sent to **{(await _client.GetApplicationInfoAsync()).Owner.Username}#{(await _client.GetApplicationInfoAsync()).Owner.Discriminator}** so that he can fix it.");
                    }
                }
            }
        }

        private void HandleJVVerification(SocketUserMessage msg)
        {
            if (msg.Channel.Id == 809403597058080798)
            {
                int argPos = 0;
                if (msg.HasStringPrefix("/", ref argPos))
                {
                    string[] msgSplit = msg.Content.Split(" ");
                    if (msgSplit.First().Equals("/jv_verify_complete") && msgSplit.Count() == 3)
                    {
                        User user = User.GetUser("user_id", msgSplit[1]);
                        if (user != null)
                        {
                            User.SetJVID(msgSplit[1], msgSplit[2]);
                        }
                    }
                }
            }
        }

        private void LogCommandUsage(SocketCommandContext context)
        {
            if (context.Channel is IGuildChannel)
            {
                string logTxt = $"User: [{context.User.Username}] Discord Server: [{context.Guild.Name}] -> [{context.Message.Content}]";
                Console.WriteLine(logTxt);
            }
            else
            {
                string logTxt = $"User: [{context.User.Username}] -> [{context.Message.Content}]";
                Console.WriteLine(logTxt);
            }
        }
    }
}
