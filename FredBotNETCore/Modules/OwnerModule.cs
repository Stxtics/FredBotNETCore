using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Database;
using System;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules
{
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Command("listservers")]
        [Alias("serverlist", "listguilds", "guildlist", "serverslist")]
        [Summary("Lists servers the bot is in.")]
        [RequireOwner]
        public async Task ListServers()
        {
            string ids = "", names = "";
            foreach (SocketGuild guild in Context.Client.Guilds)
            {
                ids += guild.Id + "\n";
                names += guild.Name + "\n";
            }

            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = Context.Client.CurrentUser.Username + " Servers",
                IconUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                Author = author
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                IconUrl = Context.User.GetAvatarUrl(),
                Text = $"Servers: {Context.Client.Guilds.Count}"
            };
            embed.WithFooter(footer);
            embed.AddField(y =>
            {
                y.Name = "ID";
                y.Value = ids;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Name";
                y.Value = names;
                y.IsInline = true;
            });

            await ReplyAsync("", false, embed.Build());
        }

        [Command("leaveserver")]
        [Alias("serverleave")]
        [Summary("Leaves the server with the specified ID.")]
        [RequireOwner]
        public async Task LeaveServer(ulong id)
        {
            SocketGuild guild = Context.Client.GetGuild(id);
            await guild.LeaveAsync();
            await ReplyAsync($"Successfully left server: **{Format.Sanitize(guild.Name)}**.");
        }

        [Command("setbalance", RunMode = RunMode.Async)]
        [Alias("balanceset")]
        [Summary("Sets balance for a user")]
        [RequireContext(ContextType.DM)]
        [RequireOwner]
        public async Task SetBalance(string username = null, int bal = 0)
        {
            if (Extensions.UserInGuild(Context.Message, Context.Client.GetGuild(528679522707701760), username) != null)
            {
                SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                User.SetValue(user, "balance", bal.ToString());
                await ReplyAsync($"{Context.User.Mention} successfully set **{Format.Sanitize(user.Username)}#{user.Discriminator}'s** balance to **${bal}**.");
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention} the user with name or ID **{Format.Sanitize(username)}** does not exist or could not be found.");
            }
        }

        [Command("gettime", RunMode = RunMode.Async)]
        [Alias("gt")]
        [Summary("Converts time from int to date time")]
        [RequireOwner]
        public async Task GetTime(int time)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddSeconds(time).ToLocalTime();
            await ReplyAsync($"{date.Day}/{date.Month}/{date.Year} - {date.TimeOfDay}");
        }

        [Command("Turnoff", RunMode = RunMode.Async)]
        [Alias("poweroff", "shutoff", "toff")]
        [Summary("Turns off bot")]
        [RequireOwner]
        public async Task TurnOff()
        {

            await ReplyAsync(":wave:");
            Environment.Exit(0);
        }

        [Command("SetGame", RunMode = RunMode.Async)]
        [Alias("game")]
        [Summary("Sets the game the bot is 'playing'")]
        [RequireOwner]
        public async Task SetGame(string type = null, string streamUrl = null, [Remainder] string name = null)
        {

            bool result = Uri.TryCreate(streamUrl, UriKind.Absolute, out Uri uriResult)
                       && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
            if (result)
            {
                await Context.Client.SetGameAsync(name, streamUrl, ActivityType.Streaming);
                await ReplyAsync($"Successfully set the game as *Streaming {name} at {streamUrl}*");
                Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to Streaming {name} at {streamUrl}");
            }
            else
            {
                ActivityType gameType = ActivityType.Playing;
                if (type.Equals("watching", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameType = ActivityType.Watching;
                    await Context.Client.SetGameAsync(name, null, gameType);
                    await ReplyAsync($"Successfully set the game as *{type} {name}*");
                    Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to {type} {name}");
                }
                else if (type.Equals("listening", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameType = ActivityType.Listening;
                    await Context.Client.SetGameAsync(name, null, gameType);
                    await ReplyAsync($"Successfully set the game as *{type} to {name}*");
                    Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to {type} to {name}");
                }
                else
                {
                    await Context.Client.SetGameAsync(name, null, gameType);
                    await ReplyAsync($"Successfully set the game as *{type} {name}*");
                    Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to {type} {name}");
                }
            }
        }
    }
}
