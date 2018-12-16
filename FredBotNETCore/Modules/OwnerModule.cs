using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules
{
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        [Command("setbalance", RunMode = RunMode.Async)]
        [Alias("balanceset")]
        [Summary("Sets balance for a user")]
        [RequireContext(ContextType.Guild)]
        [RequireOwner]
        public async Task SetBalance(string username = null, int bal = 0)
        {
            try
            {
                if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                {
                    SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                    Database.SetBalance(user, bal);
                    await Context.Channel.SendMessageAsync($"Successfully set **{Format.Sanitize(user.Username)}#{user.Discriminator}'s** balance to **${bal}**.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user **{Format.Sanitize(username)}** does not exist or could not be found.");
                }
            }
            catch (NullReferenceException)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
            await Context.Channel.SendMessageAsync($"{date.Day}/{date.Month}/{date.Year} - {date.TimeOfDay}");
        }

        [Command("leave", RunMode = RunMode.Async)]
        [Alias("leaveserver")]
        [Summary("Makes bot leave current server")]
        [RequireOwner]
        public async Task Leave()
        {
            await Context.Channel.SendMessageAsync("Leaving the server. Bye :frowning:");
            await Context.Guild.LeaveAsync();
        }

        [Command("Turnoff", RunMode = RunMode.Async)]
        [Alias("poweroff", "shutoff", "toff")]
        [Summary("Turns off bot")]
        [RequireOwner]
        public async Task TurnOff()
        {

            await Context.Channel.SendMessageAsync(":wave:");
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
                await CommandHandler._client.SetGameAsync(name, streamUrl, ActivityType.Streaming);
                await Context.Channel.SendMessageAsync($"Successfully set the game as *Streaming {name} at {streamUrl}*");
                Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to Streaming {name} at {streamUrl}");
            }
            else
            {
                ActivityType gameType = ActivityType.Playing;
                if (type.Equals("watching", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameType = ActivityType.Watching;
                    await CommandHandler._client.SetGameAsync(name, null, gameType);
                    await Context.Channel.SendMessageAsync($"Successfully set the game as *{type} {name}*");
                    Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to {type} {name}");
                }
                else if (type.Equals("listening", StringComparison.InvariantCultureIgnoreCase))
                {
                    gameType = ActivityType.Listening;
                    await CommandHandler._client.SetGameAsync(name, null, gameType);
                    await Context.Channel.SendMessageAsync($"Successfully set the game as *{type} to {name}*");
                    Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to {type} to {name}");
                }
                else
                {
                    await CommandHandler._client.SetGameAsync(name, null, gameType);
                    await Context.Channel.SendMessageAsync($"Successfully set the game as *{type} {name}*");
                    Console.WriteLine($"{DateTime.Now.ToUniversalTime()}: Game was changed to {type} {name}");
                }
            }
        }
    }
}
