using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules.Public
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        public HelpModule(CommandService commands, IServiceProvider provider)
        {
            _commands = commands;
            _provider = provider;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Alias("help", "h", "commands")]
        [Summary("Finds all the modules and prints out it's summary tag.")]
        public async Task HelpAsync()
        {
            IUserMessage msg = null;
            if (!(Context.Channel is IDMChannel))
            {
                msg = await ReplyAsync($"{Context.User.Mention} I've just sent my commands to your DMs. :grinning:");
            }
            System.Collections.Generic.IEnumerable<ModuleInfo> modules = _commands.Modules.Where(x => !string.IsNullOrWhiteSpace(x.Summary));
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
            };
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Name = "Fred the G. Cactus Commands"
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = "Use /help <module_name> to list commands in a module"
            };
            embed.WithAuthor(author);
            embed.WithFooter(footer);
            foreach (ModuleInfo module in modules)
            {
                embed.AddField(module.Name, module.Summary);
            }
            try
            {
                if (embed.Fields.Count <= 0)
                {
                    await Context.User.SendMessageAsync("Module information cannot be found, please try again later.");
                }
                else
                {
                    await Context.User.SendMessageAsync("", false, embed.Build());
                }
            }
            catch (Discord.Net.HttpException)
            {
                if (msg != null)
                {
                    await msg.ModifyAsync(x => x.Content = $"{Context.User.Mention} I was unable to send you my commands. To fix this goto Settings --> Privacy & Safety --> And allow direct messages from server members.");
                }
            }
        }

        [Command("help", RunMode = RunMode.Async)]
        [Alias("help", "h", "commands")]
        [Summary("Finds all the commands from a specific module and prints out it's summary tag.")]
        public async Task HelpAsync(string moduleName)
        {
            IUserMessage msg = null;
            if (!(Context.Channel is IDMChannel))
            {
                msg = await ReplyAsync($"{Context.User.Mention} I've just sent my commands to your DMs. :grinning:");
            }
            ModuleInfo module = _commands.Modules.FirstOrDefault(x => x.Name.ToLower() == moduleName.ToLower());
            if (module == null)
            {
                try
                {
                    await Context.User.SendMessageAsync($"The module **{Format.Sanitize(moduleName)}** does not exist or could not be found.");
                }
                catch (Discord.Net.HttpException)
                {
                    if (msg != null)
                    {
                        await msg.ModifyAsync(x => x.Content = $"{Context.User.Mention} I was unable to send you my commands. To fix this goto Settings --> Privacy & Safety --> And allow direct messages from server members.");
                    }
                }
            }
            else
            {
                System.Collections.Generic.IEnumerable<CommandInfo> commands = module.Commands.Where(x => !string.IsNullOrWhiteSpace(x.Summary)).GroupBy(x => x.Name).Select(x => x.First());

                if (!commands.Any())
                {
                    await ReplyAsync($"The module **{Format.Sanitize(module.Name)}** has no available commands.");
                }
                else
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    };
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                        Name = $"Fred the G. Cactus Commands - {Format.Sanitize(module.Name)}"
                    };
                    embed.WithAuthor(author);

                    foreach (CommandInfo command in commands)
                    {
                        string title = command.Aliases.First();
                        embed.AddField(title, command.Summary);
                        if (embed.Fields.Count == 25)
                        {
                            try
                            {
                                await Context.User.SendMessageAsync("", false, embed.Build());
                            }
                            catch (Discord.Net.HttpException)
                            {
                                await msg.ModifyAsync(x => x.Content = $"{Context.User.Mention} I was unable to send you my commands. To fix this goto Settings --> Privacy & Safety --> And allow direct messages from server members.");
                                return;
                            }
                            embed.Fields.Clear();
                        }
                    }

                    try
                    {
                        if (embed.Fields.Count <= 0)
                        {
                            await Context.User.SendMessageAsync("Command information cannot be found, please try again later.");
                        }
                        else
                        {
                            await Context.User.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    catch (Discord.Net.HttpException)
                    {
                        if (msg != null)
                        {
                            await msg.ModifyAsync(x => x.Content = $"{Context.User.Mention} I was unable to send you my commands. To fix this goto Settings --> Privacy & Safety --> And allow direct messages from server members.");
                        }
                    }
                }
            }
        }
    }
}
