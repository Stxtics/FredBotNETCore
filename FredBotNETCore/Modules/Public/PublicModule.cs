using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using static FredBotNETCore.WeatherDataCurrent;
using Newtonsoft.Json.Linq;

namespace FredBotNETCore.Modules.Public
{
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        #region Functions and Stuff
        public static string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "TextFiles");
        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }
        public static Random rand = new Random();
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(false) / (1024.0 * 1024.0), 2).ToString();

        public static async Task ExceptionInfo(DiscordSocketClient client, string message, string stacktrace)
        {
            IUser user = client.GetUser(181853112045142016);
            try
            {
                await user.SendMessageAsync(message + stacktrace);
                return;
            }
            catch(Exception)
            {
                Console.WriteLine(message + stacktrace);
                return;
            }
        }

        public static bool CheckStaff(string userID, string roleID)
        {
            var staff = new StreamReader(path: Path.Combine(downloadPath, "DiscordStaff.txt"));
            var staffRoles = new StreamReader(path: Path.Combine(downloadPath, "DiscordStaffRoles.txt"));
            string line = staff.ReadLine();
            bool isStaff = false;
            while (line != null)
            {
                if (line.Equals(userID))
                {
                    isStaff = true;
                    break;
                }
                line = staff.ReadLine();
            }
            if (!isStaff)
            {
                line = staffRoles.ReadLine();
                while (line != null)
                {
                    if (line.Equals(roleID))
                    {
                        isStaff = true;
                        break;
                    }
                    line = staffRoles.ReadLine();
                }
            }
            staff.Close();
            staffRoles.Close();
            return isStaff;
        }

        public static String HexConverter(Color c)
        {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static ITextChannel ChannelExists(IReadOnlyCollection<ITextChannel> channels, string channel)
        {
            ITextChannel tChannel = null;
            foreach (ITextChannel textChannel in channels)
            {
                if (textChannel.Name.Equals(channel, StringComparison.InvariantCultureIgnoreCase))
                {
                    tChannel = textChannel;
                    break;
                }
            }
            return tChannel;
        }

        public static async Task<IUser> UserInGuildAsync(IGuild guild, string username)
        {
            IUser user = null;
            ulong userId = 0;
            if (username.ElementAt(0).Equals('<') && username.ElementAt(username.Length - 1).Equals('>'))
            {
                if (username.Contains("<@"))
                {
                    if (!username.Contains("<@&"))
                    {
                        if (username.Contains("<@!"))
                        {
                            if (!ulong.TryParse(username.Substring(3, username.Length - 4), out ulong id))
                            {
                                userId = 0;
                            }
                            else
                            {
                                userId = id;
                            }
                        }
                        else
                        {
                            if (!ulong.TryParse(username.Substring(2, username.Length - 3), out ulong id))
                            {
                                userId = 0;
                            }
                            else
                            {
                                userId = id;
                            }
                        }
                    }
                }
            }
            if (userId != 0)
            {
                foreach (IGuildUser gUser in await guild.GetUsersAsync())
                {
                    if (gUser.Id == userId)
                    {
                        user = CommandHandler._client.GetUser(gUser.Id) as IUser;
                        break;
                    }
                }
            }
            else
            {
                foreach (IGuildUser gUser in await guild.GetUsersAsync())
                {
                    if (gUser.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        user = CommandHandler._client.GetUser(gUser.Id) as IUser;
                        break;
                    }
                }
            }
            return user;
        }

        public static IRole RoleInGuild(IGuild guild, string roleName)
        {
            IRole role = null;
            ulong roleId = 0;
            if (roleName.ElementAt(0).Equals('<') && roleName.ElementAt(roleName.Length - 1).Equals('>'))
            {
                if (roleName.Contains("<@&"))
                {
                    if (!ulong.TryParse(roleName.Substring(3, roleName.Length - 4), out ulong id))
                    {
                        roleId = 0;
                    }
                    else
                    {
                        roleId = Convert.ToUInt64(id);
                    }
                }
            }
            if (roleId != 0)
            {
                foreach (IRole gRole in guild.Roles)
                {
                    if (gRole.Id == roleId)
                    {
                        role = guild.GetRole(gRole.Id);
                        break;
                    }
                }
            }
            else
            {
                foreach (IRole gRole in guild.Roles)
                {
                    if (gRole.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        role = guild.GetRole(gRole.Id);
                        break;
                    }
                }
            }
            return role;
        }

        public static async Task<IChannel> ChannelInGuildAsync(IGuild guild, string channelName)
        {
            IChannel channel = null;
            ulong channelId = 0;
            if (channelName.ElementAt(0).Equals('<') && channelName.ElementAt(channelName.Length - 1).Equals('>') && channelName.Contains("<#"))
            {
                if (!ulong.TryParse(channelName.Substring(2, channelName.Length - 3), out ulong id))
                {
                    channelId = 0;
                }
                else
                {
                    channelId = id;
                }
            }
            if (channelId != 0)
            {
                foreach (IChannel gChannel in await guild.GetChannelsAsync())
                {
                    if (gChannel.Id == channelId)
                    {
                        channel = await guild.GetChannelAsync(gChannel.Id);
                        break;
                    }
                }
            }
            else
            {
                foreach (IChannel gChannel in await guild.GetChannelsAsync())
                {
                    if (gChannel.Name.Equals(channelName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        channel = await guild.GetChannelAsync(gChannel.Id);
                        break;
                    }
                }
            }
            return channel;
        }

        readonly string appid = new StreamReader(path: Path.Combine(downloadPath, "WeatherAppID.txt")).ReadLine();
        public async Task<String> GetWeatherAsync(string city)
        {
            var httpClient = new HttpClient();
            string URL = "http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=" + appid;
            var response = await httpClient.GetAsync(URL);
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }

        #endregion

        #region Owner

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

        [Command("getrole", RunMode = RunMode.Async)]
        [Alias("grole")]
        [Summary("Gets role id")]
        [RequireOwner]
        public async Task GetRole(IRole role)
        {
            ulong roleId = role.Id;
            await Context.Channel.SendMessageAsync($"{roleId}");
        }

        [Command("updatetoken", RunMode = RunMode.Async)]
        [Alias("utoken", "changetoken")]
        [Summary("Updates token used in some commands")]
        [RequireOwner]
        public async Task UpdateToken(string newToken)
        {
            var pr2token = new StreamReader(path: Path.Combine(downloadPath, "PR2Token.txt"));
            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the token was successfully changed from `{pr2token.ReadLine()}` to `{newToken}`.");
            pr2token.Close();
            File.WriteAllText(Path.Combine(downloadPath, "PR2Token.txt"), newToken);
        }

        #endregion

        #region Help

        [Command("help", RunMode = RunMode.Async)]
        [Alias("commands")]
        [Summary("List of commands for the bot.")]
        public async Task Help()
        {
            if (!(Context.Channel is IDMChannel))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I've just sent my commands to your DMs. :grinning:");
            }
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
            };
            string help = "**Manager**\n" +
                "/clearwarn - Clear warnings for a user.\n" +
                "/addmod - Add a server mod or role.\n" +
                "/delmod - Delete a server mod or role.\n" +
                "/listmods - List server mod roles and users.\n" +
                "/addjoinablerole - Add a joinable role.\n" +
                "/deljoinablerole - Remove a joinable role.\n" +
                "**Moderator**\n" +
                "/blacklistmusic - Blacklist a user from using music commands.\n" +
                "/unblacklistmusic - Unblacklist a user from using music commands.\n" +
                "/listblacklistedmusic - List blacklisted users from music commands.\n" +
                "/blacklistsuggestions - Blacklist a user from using /suggest.\n" +
                "/unblacklistsuggestions - Unblacklist a user from using /suggest.\n" +
                "/listblacklistedsuggestions - List blacklisted users from suggestions.\n" +
                "/channelinfo - Get info about a channel.\n" +
                "/rolecolor - Change color of a role.\n" +
                "/nick - Set bot nickname.\n" +
                "/setnick - Set nickname of a user.\n" +
                "/mentionable - Toggle making a role mentionable on/off.\n" +
                "/delrole - Delete a role.\n" +
                "/addrole - Create a role, with optional color and hoist.\n" +
                "/membercount - Get server member count.\n" +
                "/uptime - Get bot uptime.\n" +
                "/roleinfo - Get information about a role.\n" +
                "/roles - Get a list of server roles and member counts.\n" +
                "/warnings - Get warnings for the server or user.\n" +
                "/unban - Unban a user, optional reason.\n" +
                "/undeafen - Undeafen a user, optional reason.\n" +
                "/deafen - Deafen a user, optional reason.\n" +
                "/softban - Softban a member (ban and immediate unban to delete user messages).\n" +
                "/reason - Edits reason for case provided. Usage: case, reason.\n" +
                "/modlogs - Gets all priors for user mentioned.\n" +
                "/getcase - Gets info on a case number.\n" +
                "/endgiveaway - Ends the most recent giveaway.\n" +
                "/giveaway - Creates a giveaway. Usage: channel, time, item\n" +
                "/repick - Repicks a winner for most recent giveaway in channel\n" +
                "/userinfo - Returns info of a user -mention needed.\n" +
                "/guildinfo - Returns info about the discord server.\n" +
                "/ping - Gets ping for the bot.\n" +
                "/botinfo - Gets some info about Fred.\n" +
                "/purge - Deleted the number of messages specified.Optional user mention.\n" +
                "/temp - Adds Temp Mod role to a user.Usage: user, time.\n" +
                "/untemp - Removes Temp mod role from a user.\n" +
                "/warn - Warns the user mentioned with reason given.\n" +
                "/mute - Adds Muted role to a user.Usage: user, time, reason.\n" +
                "/unmute - Unmuted the user mentioned.\n" +
                "/kick - Kicks the user mentioned.Reason needed.\n" +
                "/ban - Bans the user mentioned.Reason needed.\n" +
                "**PR2 Staff Member Only**\n" +
                "/promote - Says about a PR2 Promotion. PR2 Admins only\n" +
                "**PR2 Discussion Only**\n" +
                "/hint - Tell you the current hint for the artifact location.\n" +
                "/view - Gives you info of a PR2 user, or multiple if each name seperated by | .\n" +
                "/viewid - Same as /view but with user ID instead of username.\n" +
                "/guild - Tells you guild info of the guild named.\n" +
                "/guildid - Tells you guild info for the guild ID given.\n" +
                "/exp - Gives EXP needed to next rank or exp from one rank to another.\n" +
                "/role - Adds/removes one of the joinable roles.Usage: /role <role>\n" +
                "/joinableroles - Lists all joinable roles.\n" +
                "/bans - Gives you info on a PR2 ban, type ban id after command.\n" +
                "/fah - Gives you info on a fah user from Team Jiggmin.\n" +
                "/pop - Tells you total number of users on PR2.\n" +
                "/stats - Gives info about any PR2 server.\n" +
                "/guildmembers - Gets members for the PR2 Guild specified.\n" +
                "/guildmembersid - Gets members for the PR2 guild ID specified.\n" +
                "/hh - Tells you current servers with happy hour on them.\n" +
                "/level - Tells you info about a level.\n" +
                "/verifyguild - Creates a server for your guild if you are owner.\n" +
                "/joinguild - Adds you to the role of the guild you are in if it exists.\n" +
                "/servers - Lists all servers how you see them on PR2.\n" +
                "/staff - Returns all PR2 Staff online is there is any.\n" +
                "**Music Moderator**\n" +
                "/play - Resumes music.\n" +
                "/pause - Pauses music.\n" +
                "/loop - Loops music queue.\n" +
                "/qclear - Clears queue.\n" +
                "/qremove - Remove song from queue.\n" +
                "/come - Brings bot to voice channel.\n" +
                "/forceskip - Skips the current song.\n" +
                "/voicelatency - Gets bot latency to voice channel.\n" +
                "**Music**\n" +
                "/add - Adds a song to the queue.\n" +
                "/skip - Vote to skip current song.\n" +
                "/np - Displays current playing song.\n" +
                "/queue - Displays song queue.\n" +
                "**Everyone**\n" +
                "/help - Tells you commands that you can use for me.\n" +
                "/suggest - Lets you add a suggestion for the suggestions channel.\n" +
                "/verify - Gives you instructions on how to get verified(if you are not).\n" +
                "/weather - Get weather for a city.";
            embed.Title = "Fred the G. Cactus Commands";
            var parts = help.SplitInParts(2000);
            foreach(string part in parts)
            {
                embed.Description = part;
                await Context.User.SendMessageAsync("", false, embed.Build());
                embed.Title = "";
            }
        }

        #endregion

        #region Everyone

        [Command("verify", RunMode = RunMode.Async)]
        [Alias("verifyme")]
        [Summary("Verifies a user on the server.")]
        public async Task Verify()
        {
            IDMChannel channel = await Context.User.GetOrCreateDMChannelAsync();
            if (Context.Channel is IDMChannel)
            {
                await channel.SendMessageAsync($"Hello {Context.User.Mention} , to verify your PR2 account please send a PM to `FredTheG.CactusBot` on PR2 " +
                    $"saying only `{channel.Id}`.\nThen once you have sent the PM type `/verifycomplete <PR2 account name>` here without <>. PR2 account name = name of " +
                    $"account you sent the PM from.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} check your DMs to verify your PR2 Account. ");
                await channel.SendMessageAsync($"Hello {Context.User.Mention} , to verify your PR2 account please send a PM to `FredTheG.CactusBot` on PR2 " +
                    $"saying only `{channel.Id}`.\nThen once you have sent the PM type `/verifycomplete <PR2 account name>` without <>. PR2 account name = name of " +
                    $"account you sent the PM from.");
            }
        }

        [Command("verifycomplete", RunMode = RunMode.Async)]
        [Alias("verifydone", "verified")]
        [Summary("Makes Fred the G. Cactus check pms for user.")]
        [RequireContext(ContextType.DM)]
        public async Task Verified([Remainder] string username)
        {
            var guild = CommandHandler._client.GetGuild(249657315576381450);
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /verifycomplete";
                    embed.Title = "**Description:** Verify your PR2 account.\n**Usage:** /verifycomplete [PR2 username]\n**Example:** /verifycomplete Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                    return;
                }
                SocketGuildUser user = null;
                try
                {
                    user = guild.GetUser(Context.User.Id);
                }
                catch
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you are not a memeber of The Platform Racing Group.");
                }
                IRole verified = guild.GetRole(255513962798514177);
                //if (user.Roles.Any(e => e.Name == "Verified"))
                //{
                //    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you are already verified.");
                //    return;
                //}
                //if (user.Roles.Any(e => e.Position > verified.Position))
                //{
                //    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you already have a role higher than verified.");
                //    return;
                //}
                //else
                //{
                var pr2token = new StreamReader(path: Path.Combine(downloadPath, "PR2Token.txt"));
                var values = new Dictionary<string, string>
                {
                    { "count", "10" },
                    { "start", "0" },
                    { "token", pr2token.ReadLine() }
                };
                pr2token.Close();
                HttpClient web = new HttpClient();
                var content = new FormUrlEncodedContent(values);
                var response = await web.PostAsync("https://pr2hub.com/messages_get.php?", content);
                var responseString = await response.Content.ReadAsStringAsync();
                string[] pms = responseString.Split('}');
                int tries = 0;
                foreach (string message_id in pms)
                {
                    string name = GetBetween(message_id, "name\":\"", "\",\"group");
                    if (name.ToLower().Equals(username.ToLower()))
                    {
                        string message = GetBetween(message_id, "message\":\"", "\",\"time");
                        if (message.Equals(Context.Channel.Id.ToString()))
                        {
                            var result = Database.CheckExistingUser(user);
                            if (result.Count() <= 0)
                            {
                                Database.EnterUser(user);
                            }
                            result = Database.CheckForVerified(user, "Not verified");
                            bool isVerified = false;
                            if (!(result.Count() <= 0))
                            {
                                Database.VerifyUser(user, username);
                            }
                            else
                            {
                                isVerified = true;
                            }
                            if (isVerified)
                            {
                                string pr2name = Database.GetPR2Name(user);
                                if (pr2name.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} that is already your verified account.");
                                    return;
                                }
                                Database.VerifyUser(user, username);
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully changed your verified account from {pr2name} to {username}.");
                                SocketTextChannel channel = guild.GetTextChannel(327575359765610496);
                                await channel.SendMessageAsync($":pencil: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` {Context.User.Mention} changed their verified account from **{pr2name}** to **{username}**.");
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully verified your PR2 Account.");
                                SocketTextChannel channel = guild.GetTextChannel(327575359765610496);
                                await channel.SendMessageAsync($":pencil: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Verified {Context.User.Mention} who is **{username}** on PR2.");
                                IEnumerable<SocketRole> role = guild.Roles.Where(input => input.Name.ToUpper() == "Verified".ToUpper());
                                IEnumerable<SocketRole> role2 = guild.Roles.Where(input => input.Name.ToUpper() == "Members".ToUpper());
                                RequestOptions options = new RequestOptions()
                                {
                                    AuditLogReason = "Verifying User."
                                };
                                await user.AddRolesAsync(role, options);
                                await user.RemoveRolesAsync(role2, options);
                            }
                            break;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I found a PM from {username} but it did not say what I was " +
                                $"expecting it to say ({Context.Channel.Id}).\nPlease send resend the PM and then do /verifycomplete with your PR2 " +
                                $"name after.");
                            break;
                        }
                    }
                    else if (tries == 10)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} , something went wrong in the verification process. " +
                            $"Make sure you typed your PR2 name correctly, or actually sent the PM.");
                        break;
                    }
                    tries = tries + 1;
                }
                //}
            }
            catch (Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("suggest", RunMode = RunMode.Async)]
        [Alias("suggestion")]
        [Summary("Adds suggestion to suggestion channel")]
        [RequireContext(ContextType.Guild)]
        public async Task Suggest([Remainder] string suggestion = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (File.ReadAllText(path: Path.Combine(downloadPath, "BlacklistedSuggestions.txt")).Contains(Context.User.Id.ToString()))
            {
                return;
            }
            if (suggestion == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /suggest";
                embed.Description = "**Description:** Suggest something for the Discord Server.\n**Usage:** /suggest [suggestion]\n**Example:** /suggest Make Fred admin";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (suggestion.Length < 18 || suggestion.Length > 800)
                {
                    await ReplyAsync($"{Context.User.Mention} your suggestion must be between at least 18 and no more than 800 characters long.");
                    return;
                }
                SocketTextChannel channel = Context.Guild.GetChannel(249684395454234624) as SocketTextChannel;
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder() // Shows the Name of the user
                {
                    Name = Context.User.Username + "#" + Context.User.Discriminator,
                    IconUrl = Context.User.GetAvatarUrl(),
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    Author = auth
                };
                embed.WithCurrentTimestamp();
                embed.Description = $"**Suggestion:** {suggestion}";
                IUserMessage msg = await channel.SendMessageAsync("", false, embed.Build());
                await msg.AddReactionAsync(new Emoji("👍"));
                await msg.AddReactionAsync(new Emoji("👎"));
            }
        }

        [Command("weather", RunMode = RunMode.Async)]
        [Alias("weath")]
        [Summary("Tells you weather on a location")]
        public async Task Weather([Remainder] string city = null)
        {
            try
            {
                if (city == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /weather";
                    embed.Description = "**Description:** Get weather about a city.\n**Usage:** /weather [city]\n**Example:** /weather Bristol";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    WeatherReportCurrent weather;
                    weather = JsonConvert.DeserializeObject<WeatherReportCurrent>(GetWeatherAsync(city).Result);
                    double lon = 0;
                    try
                    {
                        lon = weather.Coord.Lon;
                    }
                    catch (NullReferenceException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the city `{city}` does not exist or could not be found.");
                        return;
                    }
                    double lat = weather.Coord.Lat;
                    double temp = weather.Main.Temp;
                    int pressure = weather.Main.Pressure;
                    int humidity = weather.Main.Humidity;
                    double minTemp = weather.Main.TempMin;
                    double maxTemp = weather.Main.TempMax;
                    double speed = weather.Wind.Speed;
                    double deg = weather.Wind.Deg;
                    var directions = new string[]
                    {
                        "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest", "North"
                    };
                    int index = Convert.ToInt32(Math.Floor((deg + 23) / 45));
                    string direction = directions[index];
                    int all = weather.Clouds.All;
                    //int type = weather.Sys.Type;
                    //int id = weather.Sys.Id;
                    //double message = weather.Sys.Message;
                    string country = weather.Sys.Country;
                    int sunrise = weather.Sys.Sunrise;
                    int sunset = weather.Sys.Sunset;
                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime sunriseDate = start.AddSeconds(sunrise).ToLocalTime();
                    DateTime sunsetDate = start.AddSeconds(sunset).ToLocalTime();
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256))
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = $"{city}, {country}"
                    };
                    embed.WithFooter(footer);
                    embed.WithAuthor(author);
                    embed.WithCurrentTimestamp();
                    embed.AddField(y =>
                    {
                        y.Name = "Coordinates";
                        y.Value = $"Longitude: **{lon}**\nLatitude: **{lat}**";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Wind";
                        y.Value = $"Speed: **{speed} m/s**\nDirection: **{direction}({deg}°)**";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Main";
                        y.Value = $"Temperature: **{Math.Round(temp - 273.15)}°C/{Math.Round(temp * 9 / 5 - 459.67)}°F**\nPressure: **{pressure} hpa**\nHumidity: **{humidity}%**\nMinimum Temperature: **{Math.Round(minTemp - 273.15)}°C/{Math.Round(minTemp * 9 / 5 - 459.67)}°F**\nMaximum Temperature: **{Math.Round(maxTemp - 273.15)}°C/{Math.Round(maxTemp * 9 / 5 - 459.67)}°F**";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Cloudiness";
                        y.Value = $"Clouds: **{all}%**";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Extra";
                        y.Value = $"Country: **{country}**\nSunrise: **{sunriseDate.TimeOfDay.ToString().Substring(0, sunriseDate.TimeOfDay.ToString().Length - 3)}**\nSunset: **{sunsetDate.TimeOfDay.ToString().Substring(0, sunsetDate.TimeOfDay.ToString().Length - 3)}**";
                        y.IsInline = true;
                    });
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }
        }

        #endregion

        #region PR2 Commands

        [Command("hint", RunMode = RunMode.Async)]
        [Alias("arti","artifact")]
        [Summary("Tells the user the current artifact hint.")]
        public async Task Hint()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                HttpClient web = new HttpClient();
                String text = await web.GetStringAsync("http://pr2hub.com/files/artifact_hint.txt");

                string levelname = GetBetween(text, "hint\":\"", "\"");
                string person = GetBetween(text, "finder_name\":\"", "\"");
                if (person.Length < 1)
                {
                    await Context.Channel.SendMessageAsync($"Here's what I remember: `{levelname}`. Maybe I can remember more later!!");
                }
                else
                {
                    ulong userID = Convert.ToUInt64(Database.GetUserID(person));
                    if (userID != 1)
                    {
                        SocketGuild guild = CommandHandler._client.GetGuild(249657315576381450);
                        try
                        {
                            IUser user = guild.GetUser(userID);
                            await Context.Channel.SendMessageAsync($"Here's what I remember: `{Uri.UnescapeDataString(levelname)}`. Maybe I can remember more later!!\nThe first person to find this artifact was {Uri.UnescapeDataString(person)} ({user.Username}#{user.Discriminator})!!");
                        }
                        catch
                        {
                            await Context.Channel.SendMessageAsync($"Here's what I remember: `{Uri.UnescapeDataString(levelname)}`. Maybe I can remember more later!!\nThe first person to find this artifact was {Uri.UnescapeDataString(person)}!!");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"Here's what I remember: `{Uri.UnescapeDataString(levelname)}`. Maybe I can remember more later!!\nThe first person to find this artifact was {Uri.UnescapeDataString(person)}!!");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("view", RunMode = RunMode.Async)]
        [Summary("Tells information about pr2 name put after the command.")]
        public async Task View([Remainder] string pr2name = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(pr2name))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /view";
                    embed.Description = "**Description:** View a PR2 account by name.\n**Usage:** /view [PR2 username]\n**Example:** /view Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (pr2name.Contains("<@"))
                    {
                        var server = CommandHandler._client.GetGuild(249657315576381450);
                        ulong id = 0;
                        try
                        {
                            if (pr2name[2].Equals('!'))
                            {
                                id = Convert.ToUInt64(pr2name.Substring(3, pr2name.Length - 4));
                            }
                            else
                            {
                                id = Convert.ToUInt64(pr2name.Substring(2, pr2name.Length - 3));
                            }
                        }
                        catch (Exception)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you must only mention the user to view their PR2 account.");
                            return;
                        }
                        IUser user = server.GetUser(id);
                        var result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        pr2name = Database.GetPR2Name(user);
                        if (pr2name.Equals("Not verified"))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                    }
                    pr2name = Uri.EscapeDataString(pr2name);
                    if (pr2name.Contains("_"))
                    {
                        pr2name = pr2name.Replace("_", " ");
                    }
                    HttpClient web = new HttpClient();
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    if (pr2name.Contains("%20%7C%20") && Context.Channel is IDMChannel)
                    {
                        try
                        {
                            string[] pr2users = pr2name.Split("%20%7C%20");
                            if (pr2users.Count() <= 5)
                            {
                                foreach (string pr2user in pr2users)
                                {
                                    String pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2user);

                                    string rank = GetBetween(pr2info, "{\"rank\":", ",\"hats\":");
                                    string hats = GetBetween(pr2info, ",\"hats\":", ",\"group\":\"");
                                    string group = GetBetween(pr2info, ",\"group\":\"", "\",\"friend\":");
                                    string status = GetBetween(pr2info, ",\"status\":\"", "\",\"loginDate\":\"");
                                    string lastlogin = GetBetween(pr2info, "\",\"loginDate\":\"", "\",\"registerDate\":\"");
                                    string createdat = GetBetween(pr2info, "\",\"registerDate\":\"", "\",\"hat\":\"");
                                    string guild = GetBetween(pr2info, "\",\"guildName\":\"", "\",\"name\":\"");
                                    string name = Uri.UnescapeDataString(GetBetween(pr2info, "\",\"name\":\"", "\",\"userId"));
                                    if (group == "0")
                                    {
                                        group = "Guest";
                                    }
                                    if (group == "1")
                                    {
                                        group = "Member";
                                    }
                                    if (group == "2")
                                    {
                                        group = "Moderator";
                                    }
                                    if (group == "3")
                                    {
                                        group = "Admin";
                                    }
                                    if (guild.Length < 1)
                                    {
                                        guild = "none";
                                    }
                                    if (createdat.Contains("1970"))
                                    {
                                        createdat = "Age of Heroes";
                                    }
                                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                    {
                                        Name = $"-- {name} --",
                                        Url = "https://pr2hub.com/player_search.php?name=" + name
                                    };
                                    embed.WithAuthor(author);
                                    embed.Description = $"{status}\n**Group:** {group}\n**Guild:** {guild}\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                                    if (pr2info.Contains(value: "{\"error\":\""))
                                    {
                                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{Uri.UnescapeDataString(pr2user)}` does not exist or could not be found.");
                                    }
                                    else
                                    {
                                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                                    }
                                    await Task.Delay(1000);
                                }
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you can only view a maximum of 5 users at a time.");
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
                        }
                    }
                    else
                    {
                        String pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
                        if (pr2info.Contains(value: "{\"error\":\""))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{Uri.UnescapeDataString(pr2name)}` does not exist or could not be found.");
                            return;
                        }
                        string rank = GetBetween(pr2info, "{\"rank\":", ",\"hats\":");
                        string hats = GetBetween(pr2info, ",\"hats\":", ",\"group\":\"");
                        string group = GetBetween(pr2info, ",\"group\":\"", "\",\"friend\":");
                        string status = GetBetween(pr2info, ",\"status\":\"", "\",\"loginDate\":\"");
                        string lastlogin = GetBetween(pr2info, "\",\"loginDate\":\"", "\",\"registerDate\":\"");
                        string createdat = GetBetween(pr2info, "\",\"registerDate\":\"", "\",\"hat\":\"");
                        string guild = GetBetween(pr2info, "\",\"guildName\":\"", "\",\"name\":\"");
                        string name = Uri.UnescapeDataString(GetBetween(pr2info, "\",\"name\":\"", "\",\"userId"));
                        if (group == "0")
                        {
                            group = "Guest";
                        }
                        if (group == "1")
                        {
                            group = "Member";
                        }
                        if (group == "2")
                        {
                            group = "Moderator";
                        }
                        if (group == "3")
                        {
                            group = "Admin";
                        }
                        if (guild.Length < 1)
                        {
                            guild = "none";
                        }
                        if (createdat.Contains("1970"))
                        {
                            createdat = "Age of Heroes";
                        }
                        try
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"-- {name} --",
                                Url = "https://pr2hub.com/player_search.php?name=" + pr2name
                            };
                            embed.WithAuthor(author);
                        }
                        catch(Exception e)
                        {
                            await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
                        }
                        embed.Description = $"{status}\n**Group:** {group}\n**Guild:** {guild}\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("viewid", RunMode = RunMode.Async)]
        [Alias("vid")]
        [Summary("Info about a user using their ID.")]
        public async Task ViewID([Remainder] string id = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /viewid";
                    embed.Description = "**Description:** View a PR2 account by ID.\n**Usage:** /viewid [PR2 user ID]\n**Example:** /viewid 1";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    if (id.Contains("%20%7C%20") && Context.Channel is IDMChannel)
                    {
                        try
                        {
                            string[] pr2users = id.Split("%20%7C%20");
                            if (pr2users.Count() <= 5)
                            {
                                foreach (string pr2user in pr2users)
                                {
                                    String pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?user_id=" + pr2user);

                                    string rank = GetBetween(pr2info, "{\"rank\":", ",\"hats\":");
                                    string hats = GetBetween(pr2info, ",\"hats\":", ",\"group\":\"");
                                    string group = GetBetween(pr2info, ",\"group\":\"", "\",\"friend\":");
                                    string status = GetBetween(pr2info, ",\"status\":\"", "\",\"loginDate\":\"");
                                    string lastlogin = GetBetween(pr2info, "\",\"loginDate\":\"", "\",\"registerDate\":\"");
                                    string createdat = GetBetween(pr2info, "\",\"registerDate\":\"", "\",\"hat\":\"");
                                    string guild = GetBetween(pr2info, "\",\"guildName\":\"", "\",\"name\":\"");
                                    string name = Uri.UnescapeDataString(GetBetween(pr2info, "\",\"name\":\"", "\",\"userId"));
                                    if (group == "0")
                                    {
                                        group = "Guest";
                                    }
                                    if (group == "1")
                                    {
                                        group = "Member";
                                    }
                                    if (group == "2")
                                    {
                                        group = "Moderator";
                                    }
                                    if (group == "3")
                                    {
                                        group = "Admin";
                                    }
                                    if (guild.Length < 1)
                                    {
                                        guild = "none";
                                    }
                                    if (createdat.Contains("1970"))
                                    {
                                        createdat = "Age of Heroes";
                                    }
                                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                    {
                                        Name = $"-- {name} --",
                                        Url = "https://pr2hub.com/player_search.php?name=" + name
                                    };
                                    embed.WithAuthor(author);
                                    embed.Description = $"{status}\n**Group:** {group}\n**Guild:** {guild}\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                                    if (pr2info.Contains(value: "{\"error\":\""))
                                    {
                                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user with ID `{pr2user}` does not exist or could not be found.");
                                    }
                                    else
                                    {
                                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                                    }
                                    await Task.Delay(1000);
                                }
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you can only view a maximum of 5 users at a time.");
                                return;
                            }
                        }
                        catch (Exception e)
                        {
                            await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
                        }
                    }
                    else
                    {
                        String pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?user_id=" + id);
                        if (pr2info.Contains(value: "{\"error\":\""))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user with ID `{id}` does not exist or could not be found.");
                            return;
                        }
                        string rank = GetBetween(pr2info, "{\"rank\":", ",\"hats\":");
                        string hats = GetBetween(pr2info, ",\"hats\":", ",\"group\":\"");
                        string group = GetBetween(pr2info, ",\"group\":\"", "\",\"friend\":");
                        string status = GetBetween(pr2info, ",\"status\":\"", "\",\"loginDate\":\"");
                        string lastlogin = GetBetween(pr2info, "\",\"loginDate\":\"", "\",\"registerDate\":\"");
                        string createdat = GetBetween(pr2info, "\",\"registerDate\":\"", "\",\"hat\":\"");
                        string guild = GetBetween(pr2info, "\",\"guildName\":\"", "\",\"name\":\"");
                        string name = Uri.UnescapeDataString(GetBetween(pr2info, "\",\"name\":\"", "\",\"userId"));
                        if (group == "0")
                        {
                            group = "Guest";
                        }
                        if (group == "1")
                        {
                            group = "Member";
                        }
                        if (group == "2")
                        {
                            group = "Moderator";
                        }
                        if (group == "3")
                        {
                            group = "Admin";
                        }
                        if (guild.Length < 1)
                        {
                            guild = "none";
                        }
                        if (createdat.Contains("1970"))
                        {
                            createdat = "Age of Heroes";
                        }
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = $"-- {name} --",
                            Url = "https://pr2hub.com/player_search.php?name=" + name
                        };
                        embed.WithAuthor(author);
                        embed.Description = $"{status}\n**Group:** {group}\n**Guild:** {guild}\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("guild", RunMode = RunMode.Async)]
        [Summary("Info about the guild named after.")]
        public async Task Guild([Remainder] string guildname = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                HttpClient web = new HttpClient();
                if (guildname == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /guild";
                    embed.Description = "**Description:** View a PR2 guild by name.\n**Usage:** /guild [PR2 guild name]\n**Example:** /guild PR2 Staff";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (guildname.Contains("<@"))
                    {
                        var server = CommandHandler._client.GetGuild(249657315576381450);
                        ulong id = 0;
                        try
                        {
                            if (guildname[2].Equals('!'))
                            {
                                id = Convert.ToUInt64(guildname.Substring(3, guildname.Length - 4));
                            }
                            else
                            {
                                id = Convert.ToUInt64(guildname.Substring(2, guildname.Length - 3));
                            }
                        }
                        catch (Exception)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you must only mention the user to view their PR2 accounts guild.");
                            return;
                        }
                        IUser user = server.GetUser(id);
                        var result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        guildname = Database.GetPR2Name(user);
                        if (guildname.Equals("Not verified"))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                        String pr2userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + guildname);
                        string[] userinfo = pr2userinfo.Split(',');
                        string guild = userinfo[17].Substring(13).TrimEnd(new Char[] { '"', ' ' });
                        if (guild.Length <= 0)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that users account is not a member of a guild.");
                            return;
                        }
                        guildname = guild;
                    }
                    String pr2info = await web.GetStringAsync("http://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname);

                    if (pr2info.Contains(value: "{\"error\":\""))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the guild `{guildname}` does not exist or could not be found.");
                        return;
                    }
                    string name = GetBetween(pr2info, "guild_name\":\"", "\",\"creation");
                    string createdat = GetBetween(pr2info, "creation_date\":\"", "\"");
                    string members = GetBetween(pr2info, "member_count\":\"", "\"");
                    string gptotal = Int32.Parse(GetBetween(pr2info, "gp_total\":\"", "\"")).ToString("N0");
                    string gptoday = Int32.Parse(GetBetween(pr2info, "gp_today\":\"", "\"")).ToString("N0");
                    string guildpic = GetBetween(pr2info, "emblem\":\"", "\"");
                    string note = GetBetween(pr2info, "note\":\"", "\"");
                    string active = GetBetween(pr2info, "active_count\":\"", "\"");
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = $"-- {name} --",
                        Url = "https://pr2hub.com/guild_search.php?name=" + guildname.Replace(" ", "%20")
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Author = author,
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256))
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.ThumbnailUrl = "http://pr2hub.com/emblems/" + guildpic;
                    embed.Description = $"**Created At:** {createdat}\n**Members:** {members} ({active} active)\n**GP Total:** {gptotal}\n**GP Today:** {gptoday}\n**Description:** {note}";

                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("guildid", RunMode = RunMode.Async)]
        [Alias("gid")]
        [Summary("Gets info of a guild via ID")]
        public async Task GuildID(string id = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /guildid";
                    embed.Description = "**Description:** View a PR2 guild by ID.\n**Usage:** /guildid [PR2 guild ID]\n**Example:** /guildid 183";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    String pr2info = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&id=" + id);
                    if (pr2info.Contains(value: "{\"error\":\""))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the guild with ID `{id}` does not exist or could not be found.");
                        return;
                    }
                    string name = GetBetween(pr2info, "\"guild_name\":\"", "\",\"");
                    string createdat = GetBetween(pr2info, "creation_date\":\"", "\"");
                    string members = GetBetween(pr2info, "member_count\":\"", "\"");
                    string gptotal = Int32.Parse(GetBetween(pr2info, "gp_total\":\"", "\"")).ToString("N0");
                    string gptoday = Int32.Parse(GetBetween(pr2info, "gp_today\":\"", "\"")).ToString("N0");
                    string guildpic = GetBetween(pr2info, "emblem\":\"", "\"");
                    string note = GetBetween(pr2info, "note\":\"", "\"");
                    string active = GetBetween(pr2info, "active_count\":\"", "\"");
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = $"-- {name} --",
                        Url = "http://pr2hub.com/guild_search.php?id=" + id
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Author = author,
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256))
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.ThumbnailUrl = "http://pr2hub.com/emblems/" + guildpic;
                    embed.Description = $"**Created At:** {createdat}\n**Members:** {members} ({active} active)\n**GP Total:** {gptotal}\n**GP Today:** {gptoday}\n**Description:** {note}";

                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("exp", RunMode = RunMode.Async)]
        [Alias("experience")]
        [Summary("Tells exp needed to rank up at that rank.")]
        public async Task EXP([Remainder] string lvl)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                string lvl2 = "0";
                if (string.IsNullOrEmpty(lvl))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /exp";
                    embed.Description = "**Description:** Get EXP needed from one rank to another.\n**Usage:** /exp [rank], [optional rank]\n**Example:** /exp 28, 30";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (lvl.Contains(" "))
                    {
                        string[] levels = lvl.Split(" ");
                        lvl = levels[0];
                        lvl2 = levels[1];
                    }
                    if (!int.TryParse(lvl, out int level_))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that does not seem to be an integer.");
                        return;
                    }
                    if (level_ < 0 || level_ > 99)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you can only do a level between 0 and 100");
                        return;
                    }
                    else
                    {
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                        };

                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                        };
                        try
                        {
                            if (lvl2.Equals("0"))
                            {
                                string exp = "";
                                if (level_ == 0)
                                {
                                    exp = "1";
                                }
                                else
                                {
                                    exp = Math.Round((Math.Pow(1.25, level_)) * 30).ToString("N0");
                                }
                                embed.WithFooter(footer);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"__EXP - {lvl} to {level_ + 1}__"
                                };
                                embed.WithAuthor(author);
                                embed.WithCurrentTimestamp();
                                embed.Description = $"**From rank {lvl} to rank {level_ + 1} you need {exp} EXP.**";
                                await Context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                if (!int.TryParse(lvl2, out int level_2))
                                {
                                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} that does not seem to be an integer.");
                                    return;
                                }
                                else if (level_2 < 0 || level_2 > 100)
                                {
                                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you can only do a level between 0 and 100");
                                    return;
                                }
                                if (level_ > level_2)
                                {
                                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} your EXP can't go down.");
                                    return;
                                }
                                if (level_ == level_2)
                                {
                                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} that would just be 0.");
                                    return;
                                }
                                double exp = 0;
                                for (int i = level_; i < level_2; i++)
                                {
                                    if (i == 0)
                                    {
                                        exp = exp + 1;
                                    }
                                    else
                                    {
                                        exp = exp + Math.Round((Math.Pow(1.25, i)) * 30);
                                    }
                                }
                                embed.WithFooter(footer);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"__EXP - {level_} to {level_2}__"
                                };
                                embed.WithAuthor(author);
                                embed.WithCurrentTimestamp();
                                embed.Description = $"**From rank {level_} to rank {level_2} you need {exp.ToString("N0")} EXP.**";
                                await Context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        catch (Exception)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} length of number is too long. (Don't use an excessive amount of 0's).");
                            return;
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("role", RunMode = RunMode.Async)]
        [Alias("joinrole","leaverole")]
        [Summary("Adds HH, Arti, Trapper, Glitcher, Fruster, Racer or PR2 role.")]
        public async Task Role([Remainder] string roleName = null)
        {
            try
            {
                SocketGuildUser user;
                SocketGuild guild;
                if (Context.Channel is IDMChannel)
                {
                    guild = CommandHandler._client.GetGuild(249657315576381450);
                    user = guild.GetUser(Context.User.Id);
                }
                else if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675)
                {
                    user = Context.User as SocketGuildUser;
                    guild = Context.Guild as SocketGuild;
                }
                else
                {
                    return;
                }
                if (roleName == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /role";
                    embed.Description = "**Description:** Add/remove one of the joinable roles.\n**Usage:** /role [role name]\n**Example:** /role Arti";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (RoleInGuild(Context.Guild, roleName) != null)
                    {
                        IRole role = RoleInGuild(Context.Guild, roleName);
                        string joinableRoles = File.ReadAllText(path: Path.Combine(downloadPath, "JoinableRoles.txt"));
                        if (joinableRoles.Contains(role.Id.ToString()))
                        {
                            if (user.Roles.Any(e => e.Name.ToUpperInvariant() == roleName.ToUpperInvariant()))
                            {
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                                };
                                RequestOptions options = new RequestOptions()
                                {
                                    AuditLogReason = "Removing Joinable Role"
                                };
                                await user.RemoveRoleAsync(role, options);

                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = Context.User.GetAvatarUrl(),
                                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                                };
                                embed.WithFooter(footer);
                                embed.Title = $"__{role.Name} Remove__";
                                embed.WithCurrentTimestamp();
                                if (Context.Channel is IDMChannel)
                                {
                                    embed.Description = $"**You have been removed from the {role.Name} role.**";
                                }
                                else
                                {
                                    embed.Description = $"**{Context.User.Mention} has been removed from the {role.Name} role({role.Mention}).**";
                                }
                                await Context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                                };
                                RequestOptions options = new RequestOptions()
                                {
                                    AuditLogReason = "Adding Joinable Role"
                                };
                                await user.AddRoleAsync(role, options);

                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = Context.User.GetAvatarUrl(),
                                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                                };
                                embed.WithFooter(footer);
                                embed.Title = $"__{role.Name} Add__";
                                embed.WithCurrentTimestamp();
                                if (Context.Channel is IDMChannel)
                                {
                                    embed.Description = $"**You have been added to the {role.Name} role.**";
                                }
                                else
                                {
                                    embed.Description = $"**{Context.User.Mention} has been added to the {role.Name} role({role.Mention}).**";
                                }
                                await Context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{role.Name}` is not a joinable role.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{roleName}` does not exist or could not be found.");
                    }
                }
            }
            catch(Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("listjoinableroles")]
        [Alias("ljr", "listjroles", "joinableroles")]
        [Summary("Lists all joinable roles")]
        public async Task ListJoinableRoles()
        {
            SocketGuild guild;
            if (Context.Channel is IDMChannel)
            {
                guild = CommandHandler._client.GetGuild(249657315576381450);
            }
            else if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675)
            {
                guild = Context.Guild as SocketGuild;
            }
            else
            {
                return;
            }
            var joinableRoles = new StreamReader(path: Path.Combine(downloadPath, "JoinableRoles.txt"));
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = guild.IconUrl,
                Name = "List Joinable Roles"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(rand.Next(255), rand.Next(255), rand.Next(255)),
                Author = auth
            };
            string jRoles = "";
            string line = joinableRoles.ReadLine();
            while (line != null)
            {
                string role = (guild.GetRole(Convert.ToUInt64(line))).Name;
                jRoles = jRoles + role + "\n";
                line = joinableRoles.ReadLine();
            }
            joinableRoles.Close();
            if (jRoles.Length <= 0)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} there are no joinable roles.");
            }
            else
            {
                embed.AddField(y =>
                {
                    y.Name = "Joinable Roles";
                    y.Value = jRoles;
                    y.IsInline = false;
                });
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("topguilds", RunMode = RunMode.Async)]
        [Alias("guildstop", "topg", "gtop")]
        [Summary("Returns current top 10 guild on pr2.")]
        public async Task TopGuilds()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                HttpClient web = new HttpClient();
                String text = await web.GetStringAsync("http://pr2hub.com/guilds_top.php?");

                string[] guildlist = text.Split('}');
                string guild1name = GetBetween(guildlist[0], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild1gp = Int32.Parse(GetBetween(guildlist[0], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild2name = GetBetween(guildlist[1], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild2gp = Int32.Parse(GetBetween(guildlist[1], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild3name = GetBetween(guildlist[2], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild3gp = Int32.Parse(GetBetween(guildlist[2], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild4name = GetBetween(guildlist[3], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild4gp = Int32.Parse(GetBetween(guildlist[3], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild5name = GetBetween(guildlist[4], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild5gp = Int32.Parse(GetBetween(guildlist[4], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild6name = GetBetween(guildlist[5], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild6gp = Int32.Parse(GetBetween(guildlist[5], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild7name = GetBetween(guildlist[6], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild7gp = Int32.Parse(GetBetween(guildlist[6], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild8name = GetBetween(guildlist[7], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild8gp = Int32.Parse(GetBetween(guildlist[7], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild9name = GetBetween(guildlist[8], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild9gp = Int32.Parse(GetBetween(guildlist[8], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                string guild10name = GetBetween(guildlist[9], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                string guild10gp = Int32.Parse(GetBetween(guildlist[9], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");

                EmbedBuilder embed = new EmbedBuilder();
                embed.WithColor(new Color(rand.Next(255), rand.Next(255), rand.Next(255)));
                embed.AddField(y =>
                {
                    y.Name = "Guild";
                    y.Value = $"{guild1name}\n" +
                              $"{guild2name}\n" +
                              $"{guild3name}\n" +
                              $"{guild4name}\n" +
                              $"{guild5name}\n" +
                              $"{guild6name}\n" +
                              $"{guild7name}\n" +
                              $"{guild8name}\n" +
                              $"{guild9name}\n" +
                              $"{guild10name}";
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "GP Today";
                    y.Value = $"{guild1gp}\n" +
                              $"{guild2gp}\n" +
                              $"{guild3gp}\n" +
                              $"{guild4gp}\n" +
                              $"{guild5gp}\n" +
                              $"{guild6gp}\n" +
                              $"{guild7gp}\n" +
                              $"{guild8gp}\n" +
                              $"{guild9gp}\n" +
                              $"{guild10gp}";
                    y.IsInline = true;
                });

                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                };
                embed.WithFooter(footer);
                embed.Title = $"__PR2 Top 10 Guilds__";
                embed.WithCurrentTimestamp();

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                return;
            }
        }

        [Command("f@h", RunMode = RunMode.Async)]
        [Alias("fah", "fold")]
        [Summary("Gets the specified names f@h points")]
        public async Task Fah([Remainder] string fahuser = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (fahuser == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /fah";
                    embed.Description = "**Description:** Get F@H info for a user.\n**Usage:** /fah [PR2 name]\n**Example:** /fah Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (fahuser.Contains("<@") && fahuser.Contains(">"))
                    {
                        var server = CommandHandler._client.GetGuild(249657315576381450);
                        ulong id = 0;
                        try
                        {
                            if (fahuser[2].Equals('!'))
                            {
                                id = Convert.ToUInt64(fahuser.Substring(3, fahuser.Length - 4));
                            }
                            else
                            {
                                id = Convert.ToUInt64(fahuser.Substring(2, fahuser.Length - 3));
                            }
                        }
                        catch (Exception)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you must only mention the user to view their F@H stats.");
                            return;
                        }
                        IUser user = server.GetUser(id);
                        var result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        fahuser = Database.GetPR2Name(user);
                        if (fahuser.Equals("Not verified"))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                    }
                    HttpClient web = new HttpClient();
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = fahuser,
                        IconUrl = "https://pbs.twimg.com/profile_images/53706032/Fold003_400x400.png",
                        Url = "https://stats.foldingathome.org/donor/" + fahuser.Replace(' ', '_')
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                        Author = author
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    string text;
                    try
                    {
                        text = await web.GetStringAsync("https://stats.foldingathome.org/api/donor/" + fahuser.Replace(' ', '_'));
                    }
                    catch(HttpRequestException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{fahuser}` does not exist or could not be found.");
                        return;
                    }
                    var o = JObject.Parse(text).GetValue("teams");
                    JArray array = JArray.Parse(o.ToString());
                    JObject stats = new JObject();
                    foreach (JObject jobject in array)
                    {
                        if (Convert.ToInt32(jobject.GetValue("team")) == 143016)
                        {
                            stats = jobject;
                            break;
                        }
                    }
                    embed.AddField(y =>
                    {
                        y.Name = $"Score";
                        y.Value = $"{Convert.ToInt32(stats.GetValue("credit")).ToString("N0")}";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Completed WUs";
                        y.Value = $"{Convert.ToInt32(stats.GetValue("wus")).ToString("N0")}";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Last WU";
                        y.Value = $"{stats.GetValue("last")}";
                        y.IsInline = true;
                    });
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("bans", RunMode = RunMode.Async)]
        [Alias("pr2ban")]
        [Summary("Gets a PR2 Ban with ID from ban log.")]
        public async Task Bans([Remainder] string id = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (id == null || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /bans";
                    embed.Description = "**Description:** Get PR2 ban info.\n**Usage:** /bans [PR2 ban ID]\n**Example:** /bans 59098";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    try
                    {
                        HttpClient web = new HttpClient();
                        String text = await web.GetStringAsync("http://pr2hub.com/bans/show_record.php?ban_id=" + id);
                        if (text.Contains("banned for 0 seconds on Jan 1, 1970 12:00 AM."))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the ban with the Id `{id}` does not exist or could not be found.");
                            return;
                        }
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = $"Ban ID - {id}",
                            Url = "http://pr2hub.com/bans/show_record.php?ban_id=" + id
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Author = author,
                            Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256))
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        string[] ban = text.Split('<');
                        if (text.Contains("<p>--- This ban has been lifted by "))
                        {
                            string lifted = ban[19].Substring(6).TrimEnd(new char[] { '-', '-', '-', ' ' });
                            string reason = ban[21].Substring(14).TrimEnd(new char[] { '-', '-', '-', ' ' });
                            string bantext = ban[28].Substring(2);
                            string reason1 = ban[30].Substring(10);
                            string expire = ban[32].Substring(2);
                            if (reason1.Length <= 0)
                            {
                                reason1 = "No reason was provided.";
                            }
                            embed.AddField(y =>
                            {
                                y.Name = "Lifted Ban";
                                y.Value = $"{lifted}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = $"{reason}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Ban Info";
                                y.Value = $"{bantext}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = $"{reason1}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Expires";
                                y.Value = $"{expire}";
                                y.IsInline = true;
                            });

                            await Context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            string bantext = ban[16].Substring(2);
                            string reason = ban[18].Substring(10);
                            string expire = ban[20].Substring(26);
                            embed.AddField(y =>
                            {
                                y.Name = "Ban Info";
                                y.Value = $"{bantext}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = $"{reason}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Expires";
                                y.Value = $"{expire}";
                                y.IsInline = true;
                            });

                            await Context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    catch (Exception)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the ban with the Id `{id}` does not exist or could not be found.");
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("Pop", RunMode = RunMode.Async)]
        [Alias("population")]
        [Summary("Tells you the number of users on pr2. Does not include private servers.")]
        public async Task Pop()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                HttpClient web = new HttpClient();
                String text = await web.GetStringAsync("http://pr2hub.com/files/server_status_2.txt");

                string[] pops = text.Split('}');
                int pop = 0;
                foreach (string server in pops)
                {
                    if (server.Length > 5)
                    {
                        int population = Convert.ToInt32(GetBetween(server, "population\":\"", "\","));
                        pop += population;
                    }
                }

                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                };
                embed.WithFooter(footer);
                embed.Title = "__PR2 Total Online Users__";
                embed.WithCurrentTimestamp();
                embed.Description = $"The total number of users on PR2 currently is {pop}";

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                return;
            }
        }

        [Command("stats", RunMode = RunMode.Async)]
        [Alias("s")]
        [Summary("Gets stats of a server on PR2.")]
        public async Task Stats([Remainder] string server = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(server))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /stats";
                    embed.Description = "**Description:** Get a PR2 server info.\n**Usage:** /stats [PR2 server name]\n**Example:** /stats Derron";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    if (server.Contains("<@"))
                    {
                        var guild = CommandHandler._client.GetGuild(249657315576381450);
                        ulong id = 0;
                        try
                        {
                            if (server[2].Equals('!'))
                            {
                                id = Convert.ToUInt64(server.Substring(3, server.Length - 4));
                            }
                            else
                            {
                                id = Convert.ToUInt64(server.Substring(2, server.Length - 3));
                            }
                        }
                        catch (Exception)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you must only mention the user to view the stats of the server they are on.");
                            return;
                        }
                        IUser user = guild.GetUser(id);
                        var result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        string pr2name = Database.GetPR2Name(user);
                        if (pr2name.Equals("Not verified"))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                        String pr2userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
                        string status = GetBetween(pr2userinfo, ",\"status\":\"", "\",\"loginDate\":\"");
                        if (status.Equals("offline"))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that users account is offline.");
                            return;
                        }
                        status = status.Substring(11);
                        server = status;
                    }
                    String text = await web.GetStringAsync("http://pr2hub.com/files/server_status_2.txt");
                    if (text.ToLower().Contains(server.ToLower()))
                    {
                        string serverInfo = GetBetween(text.ToLower(), server.ToLower(), "}");
                        string pop = GetBetween(serverInfo, "\",\"population\":\"", "\",\"status\":\"");
                        string status = GetBetween(serverInfo, "\",\"status\":\"", "\",\"guild_id\":\"");
                        int tournament = Convert.ToInt32(GetBetween(serverInfo, "\",\"tournament\":\"", "\",\"happy_hour\":\""));
                        int happyHour = Convert.ToInt32(GetBetween(serverInfo, "\",\"happy_hour\":\"", "\""));
                        string hh = "No";
                        string tourn = "No";
                        if (happyHour == 1)
                        {
                            hh = "Yes";
                        }
                        if (tournament == 1)
                        {
                            tourn = "Yes";
                        }
                        if (status == "open")
                        {
                            status = "Open";
                        }
                        if (status == "down")
                        {
                            status = "Down";
                        }
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                        };
                        embed.WithFooter(footer);
                        embed.Title = $"__Server Stats - {server}__";
                        embed.AddField(y =>
                        {
                            y.Name = "Population";
                            y.Value = $"{pop}";
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Status";
                            y.Value = $"{status}";
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Happy Hour";
                            y.Value = $"{hh}";
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Tournament";
                            y.Value = $"{tourn}";
                            y.IsInline = true;
                        });
                        embed.WithCurrentTimestamp();

                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the server `{server}` does not exist or could not be found.");
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("guildmembers", RunMode = RunMode.Async)]
        [Alias("gmembers", "membersguild", "membersg")]
        [Summary("Gets members of a pr2 guild")]
        public async Task GuildMembers([Remainder] string guildname = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (guildname == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /guildmembers";
                    embed.Description = "**Description:** Get a members for a PR2 guild by name.\n**Usage:** /guildmembers [PR2 guild name]\n**Example:** /guildmembers PR2 Staff";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    if (guildname.Contains("<@"))
                    {
                        var server = CommandHandler._client.GetGuild(249657315576381450);
                        ulong id = 0;
                        try
                        {
                            if (guildname[2].Equals('!'))
                            {
                                id = Convert.ToUInt64(guildname.Substring(3, guildname.Length - 4));
                            }
                            else
                            {
                                id = Convert.ToUInt64(guildname.Substring(2, guildname.Length - 3));
                            }
                        }
                        catch (Exception)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you must only mention the user to view their PR2 accounts guild members.");
                            return;
                        }
                        IUser user = server.GetUser(id);
                        var result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        guildname = Database.GetPR2Name(user);
                        if (guildname.Equals("Not verified"))
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                        String pr2userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + guildname);
                        string[] userinfo = pr2userinfo.Split(',');
                        string guild = userinfo[17].Substring(13).TrimEnd(new Char[] { '"', ' ' });
                        if (guild.Length <= 0)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that users account is not a member of a guild.");
                            return;
                        }
                        guildname = guild;
                    }
                    string text = await web.GetStringAsync("http://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname);
                    if (text.Contains(value: "{\"error\":\""))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the guild `{guildname}` does not exist or could not be found.");
                        return;
                    }
                    string[] users = text.Split('}');
                    string guildMembers = "";
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Url = "http://pr2hub.com/guild_search.php?name=" + guildname.Replace(" ", "%20"),
                        Name = "Guild Members - " + guildname
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Author = author,
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256))
                    };
                    foreach (string user_id in users)
                    {
                        string name = GetBetween(user_id, "name\":\"", "\",\"power");
                        guildMembers = guildMembers + name + ", ";
                    }
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.Description = $"{guildMembers.Substring(2).TrimEnd(new char[] { ',', ' ', ',', ' ', ',' })}";

                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("guildmembersid", RunMode = RunMode.Async)]
        [Alias("gmembersid", "membersguildid", "membersgid")]
        [Summary("Gets members of a pr2 guild with id")]
        public async Task GuildMembersID([Remainder] string id = null)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (id == null || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /guildmembersid";
                    embed.Description = "**Description:** Get a members for a PR2 guild by ID.\n**Usage:** /guildmembersid [PR2 guild ID]\n**Example:** /guildmembersid 183";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    string text = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&id=" + id);

                    if (text.Contains(value: "{\"error\":\""))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the guild with ID `{id}` does not exist or could not be found.");
                        return;
                    }
                    string gName = GetBetween(text, "\"guild_name\":\"", "\",\"");
                    string[] users = text.Split('}');
                    string guildMembers = "";
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Url = "http://pr2hub.com/guild_search.php?id=" + id,
                        Name = "Guild Members - " + gName
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Author = author,
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256))
                    };
                    foreach (string user_id in users)
                    {
                        string name = GetBetween(user_id, "name\":\"", "\",\"power");
                        guildMembers = guildMembers + name + ", ";
                    }
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.Description = $"{guildMembers.Substring(2).TrimEnd(new char[] { ',', ' ', ',', ' ', ',' })}";

                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("hh", RunMode = RunMode.Async)]
        [Alias("happyhour")]
        [Summary("Returns a list of servers with happy hour on them.")]
        public async Task HH()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                HttpClient web = new HttpClient();
                string hhServers = "", happyHour = "";
                String text = await web.GetStringAsync("http://pr2hub.com/files/server_status_2.txt");
                string[] servers = text.Split('}');
                foreach (string server_name in servers)
                {
                    happyHour = GetBetween(server_name, "hour\":\"", "\"");
                    if (happyHour.Equals("1"))
                    {
                        string serverName = GetBetween(server_name, "server_name\":\"", "\"");
                        hhServers = hhServers + serverName + ", ";
                    }
                }
                if (hhServers.Length < 1)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} no servers currently have happy hour on them.");
                    return;
                }
                hhServers = hhServers.TrimEnd(new char[] { ' ', ',', ',' });
                int count = hhServers.Split(',').Length - 1;
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Url = "http://pr2hub.com/server_status.php"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    Author = author
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                };
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();
                if (count == 0)
                {
                    author.Name = $"__Happy Hour Server__";
                    embed.Description = $"This server currently has a happy hour on it: {hhServers.TrimEnd(new char[] { ' ', ',' })}";
                }
                else
                {
                    author.Name = $"__Happy Hour Servers__";
                    embed.Description = $"These are the current servers with happy hour on them: {hhServers}";
                }

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                return;
            }
        }

        [Command("level", RunMode = RunMode.Async)]
        [Alias("levelinfo", "li")]
        [Summary("Gets info about a level")]
        public async Task Level([Remainder] string level)
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(level))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /level";
                    embed.Description = "**Description:** Get PR2 level info.\n**Usage:** /level [PR2 level name]\n**Example:** /level Newbieland 2";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    var pr2token = new StreamReader(path: Path.Combine(downloadPath, "PR2Token.txt"));
                    var values = new Dictionary<string, string>
                    {
                        { "dir", "desc" },
                        { "mode", "title" },
                        { "order", "popularity" },
                        { "page", "1" },
                        { "search_str", level },
                        { "token", pr2token.ReadLine() }
                    };
                    pr2token.Close();
                    HttpClient web = new HttpClient();
                    var content = new FormUrlEncodedContent(values);
                    var response = await web.PostAsync("https://pr2hub.com/search_levels.php?", content);
                    var responseString = await response.Content.ReadAsStringAsync();
                    string version = GetBetween(responseString, "&version0=", "&title0=");
                    string title = Uri.UnescapeDataString(GetBetween(responseString, "&title0=", "&rating0=")).Replace("+", " ");
                    string rating = GetBetween(responseString, "&rating0=", "&playCount0=");
                    string plays = GetBetween(responseString, "&playCount0=", "&minLevel0=");
                    string minLevel = GetBetween(responseString, "&minLevel0=", "&note0=");
                    string note = Uri.UnescapeDataString(GetBetween(responseString, "&note0=", "&userName0=")).Replace("+", " ");
                    string user = Uri.UnescapeDataString(GetBetween(responseString, "&userName0=", "&group0=")).Replace("+", " ");
                    if (title.Length <= 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the level `{level}` does not exist or could not be found.");
                        return;
                    }
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.Title = $"-- {title} --";
                    if (note.Length <= 0)
                    {
                        embed.Description = $"**By:** {user}\n**Version:** {version}\n**Min Rank:** {minLevel}\n**Plays:** {plays}\n**Rating:** {rating}\n-----";
                    }
                    else
                    {
                        embed.Description = $"**By:** {user}\n**Version:** {version}\n**Min Rank:** {minLevel}\n**Plays:** {plays}\n**Rating:** {rating}\n-----\n{note}";
                    }
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        [Command("verifyguild", RunMode = RunMode.Async)]
        [Alias("addguild")]
        [Summary("Creates a channel for the users guild.")]
        public async Task VerifyGuild()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675)
            {
                var result = Database.CheckExistingUser(Context.User);
                if (result.Count() <= 0)
                {
                    Database.EnterUser(Context.User);
                }
                string pr2name = Database.GetPR2Name(Context.User);
                if (pr2name.Equals("Not verified"))
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have not linked your PR2 account.");
                    return;
                }
                HttpClient web = new HttpClient();
                String userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
                string guild = GetBetween(userinfo, "\",\"guildName\":\"", "\",\"name\":\"");
                string id = GetBetween(userinfo, "\",\"userId\":\"", "\",\"hatColor2\":");
                String guildinfo = await web.GetStringAsync("http://pr2hub.com/guild_info.php?getMembers=yes&name=" + guild);
                string owner = GetBetween(guildinfo, "\",\"owner_id\":\"", "\",\"note\":\"");
                if (id.Equals(owner))
                {
                    int memberCount = Int32.Parse(GetBetween(guildinfo, "\",\"member_count\":\"", "\",\"emblem\":\""));
                    if (memberCount < 15)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you need at least 15 members in your guild.");
                        return;
                    }
                    var roles = Context.Guild.Roles;
                    bool exists = false;
                    foreach (IRole role in roles)
                    {
                        if (role.Name == guild)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        var guildRole = await Context.Guild.CreateRoleAsync(guild, null, null, false);
                        var everyoneRole = Context.Guild.EveryoneRole;
                        var mutedRole = Context.Guild.GetRole(308331455602229268);
                        var guildChannel = await Context.Guild.CreateTextChannelAsync(guild.Replace(" ", "-"));
                        await guildChannel.ModifyAsync(x => x.CategoryId = 361634557067526146);
                        await guildChannel.AddPermissionOverwriteAsync(guildRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Inherit));
                        await guildChannel.AddPermissionOverwriteAsync(mutedRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Deny));
                        await guildChannel.AddPermissionOverwriteAsync(everyoneRole, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny));
                        var guildVoice = await Context.Guild.CreateVoiceChannelAsync(guild);
                        await guildVoice.ModifyAsync(x => x.CategoryId = 361634557067526146);
                        await guildVoice.AddPermissionOverwriteAsync(guildRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Allow));
                        await guildVoice.AddPermissionOverwriteAsync(mutedRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Deny, speak: PermValue.Deny));
                        await guildVoice.AddPermissionOverwriteAsync(everyoneRole, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny));
                        IGuildUser user = Context.Guild.GetUser(Context.User.Id);
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = "Adding Guild Role"
                        };
                        await user.AddRoleAsync(guildRole, options);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} your guild already has a guild channel.");
                        return;
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you are not the owner of this guild.");
                    return;
                }
            }
            else
            {
                return;
            }
        }

        [Command("joinguild", RunMode = RunMode.Async)]
        [Alias("jguild", "guildjoin", "guildj")]
        [Summary("Adds user to guild role of their guild if it exists.")]
        public async Task JoinGuild()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675)
            {
                var result = Database.CheckExistingUser(Context.User);
                if (result.Count() <= 0)
                {
                    Database.EnterUser(Context.User);
                }
                string pr2name = Database.GetPR2Name(Context.User);
                if (pr2name.Equals("Not verified"))
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have not linked your PR2 account.");
                    return;
                }
                HttpClient web = new HttpClient();
                String userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
                string guild = GetBetween(userinfo, "\",\"guildName\":\"", "\",\"name\":\"");
                var roles = Context.Guild.Roles;
                foreach (IRole role in roles)
                {
                    if (role.Name == guild)
                    {
                        SocketGuildUser user = Context.User as SocketGuildUser;
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = "Adding Guild Role"
                        };
                        await user.AddRoleAsync(role, options);
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have been added to the guild role `{guild}`");
                        break;
                    }
                }
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} your guild does not have a guild channel.");
            }
            else
            {
                return;
            }
        }

        [Command("servers", RunMode = RunMode.Async)]
        [Alias("sl", "status", "serverstatus", "ss")]
        [Summary("Shows all servers and population")]
        public async Task Servers()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                HttpClient web = new HttpClient();
                String text = await web.GetStringAsync("http://pr2hub.com/files/server_status_2.txt");
                string[] serversinfo = text.Split('}');
                string pop = "", name = "", status = "", happyHour = "";
                int serverId = 0;
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Url = "http://pr2hub.com/server_status.php",
                    Name = "Server Status"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    Author = author
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                };
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();
                embed.Description = "";
                foreach (string server_id in serversinfo)
                {
                    name = GetBetween(server_id, "\",\"server_name\":\"", "\",\"address\":\"");
                    if (name.Length <= 0)
                    {
                        break;
                    }
                    pop = GetBetween(server_id, "\",\"population\":\"", "\",\"status\":\"");
                    status = GetBetween(server_id, "\",\"status\":\"", "\",\"guild_id\":\"");
                    happyHour = GetBetween(server_id, "\",\"happy_hour\":\"", "\"");
                    serverId = Int32.Parse(GetBetween(server_id, "\"server_id\":\"", "\",\"server_name\":\""));
                    if (status.Equals("down", StringComparison.InvariantCultureIgnoreCase) && serverId < 11)
                    {
                        embed.Description = embed.Description + name + " (down)\n";
                    }
                    else if (happyHour.Equals("1") && serverId < 11)
                    {
                        embed.Description = embed.Description + "!! " + name + " (" + pop + " online)\n";
                    }
                    else if (happyHour.Equals("1") && serverId > 10)
                    {
                        embed.Description = embed.Description + "* !! " + name + " (" + pop + " online)\n";
                    }
                    else if (status.Equals("down", StringComparison.InvariantCultureIgnoreCase) && serverId > 10)
                    {
                        embed.Description = embed.Description + "* " + name + " (down)\n";
                    }
                    else if (serverId > 10)
                    {
                        embed.Description = embed.Description + "* " + name + " (" + pop + " online)\n";
                    }
                    else
                    {
                        embed.Description = embed.Description + name + " (" + pop + " online)\n";
                    }
                }
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                return;
            }
        }

        [Command("staff")]
        [Alias("staffonline", "so")]
        [Summary("Returns currect names of currect staff online and what server.")]
        public async Task Staff()
        {
            if (Context.Channel.Id == 249678944956055562 || Context.Channel.Id == 327232898061041675 || Context.Channel is IDMChannel || Context.Guild.Id != 249657315576381450)
            {
                HttpClient web = new HttpClient();
                String text = await web.GetStringAsync("http://pr2hub.com/staff.php");
                string[] staff = text.Split("player_search");
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Name = "Staff Online",
                    Url = "http://pr2hub.com/staff.php"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Author = author,
                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256))
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                };
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();
                embed.Description = "";
                foreach (string name in staff)
                {
                    string status = GetBetween(name, "</a></td><td>", "</td><td>");
                    if (status.Contains("Playing on"))
                    {
                        status = GetBetween(name, "</a></td><td>Playing on ", "</td><td>");
                        string pr2Name = GetBetween(name, "; text-decoration: underline;'>", "</a></td><td>").Replace("&nbsp;"," ");
                        embed.Description = embed.Description + pr2Name + "(" + status + "), ";
                    }
                };
                if (embed.Description.Length <= 0)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} there is no PR2 Staff online currently.");
                    return;
                }
                else
                {
                    embed.Description = embed.Description.TrimEnd(new char[] { ' ', ',', ',' });
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        #endregion

        #region Moderator

        [Command("blacklistmusic", RunMode = RunMode.Async)]
        [Alias("musicblacklist")]
        [Summary("Blacklist a user from using music commands.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistMusic([Remainder] string username = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklistmusic";
                    embed.Description = "**Description:** Blacklist a user from using music commands.\n**Usage:** /blacklistmusic [user]\n**Example:** /blacklistmusic Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser user = await UserInGuildAsync(Context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(downloadPath, "BlacklistedMusic.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers);
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{user.Username}` is already blacklisted from using music commands.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers + user.Id.ToString() + "\n");
                            await Context.Channel.SendMessageAsync($"Blacklisted **{user.Username}#{user.Discriminator}** from using music commands.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("unblacklistmusic", RunMode = RunMode.Async)]
        [Alias("musicunblacklist")]
        [Summary("Unblacklist a user from using music commands.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task UnblacklistMusic([Remainder] string username = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /unblacklistmusic";
                    embed.Description = "**Description:** Unblacklist a user from using music commands.\n**Usage:** /unblacklistmusic [user]\n**Example:** /unblacklistmusic Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser user = await UserInGuildAsync(Context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(downloadPath, "BlacklistedMusic.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            currentBlacklistedUsers = currentBlacklistedUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers);
                            await Context.Channel.SendMessageAsync($"Removed blacklisted music user **{user.Username}#{user.Discriminator}**.");
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{user.Username}` is not blacklisted from using music commands.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("listblacklistedmusic", RunMode = RunMode.Async)]
        [Alias("lbm", "blacklistedmusic")]
        [Summary("Lists blacklisted users from music commands.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ListBlacklistedMusic()
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                var blacklistedMusic = new StreamReader(path: Path.Combine(downloadPath, "BlacklistedMusic.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = "List Blacklisted Music"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(rand.Next(255), rand.Next(255), rand.Next(255)),
                    Author = auth
                };
                string blacklistedUsers = "";
                string line = blacklistedMusic.ReadLine();
                while (line != null)
                {
                    string user = (Context.Guild.GetUser(Convert.ToUInt64(line))).Username + "#" + (Context.Guild.GetUser(Convert.ToUInt64(line))).Discriminator;
                    blacklistedUsers = blacklistedUsers + user + "\n";
                    line = blacklistedMusic.ReadLine();
                }
                blacklistedMusic.Close();
                if (blacklistedUsers.Length <= 0)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} there are no blacklisted users from music commands.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Music Users";
                        y.Value = blacklistedUsers;
                        y.IsInline = false;
                    });
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("blacklistsuggestions", RunMode = RunMode.Async)]
        [Alias("blacklistsuggestion","suggestionblacklist","suggestionsblacklist")]
        [Summary("Blacklist a user from using the /suggest command")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistSuggestions([Remainder] string username = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklistsuggestions";
                    embed.Description = "**Description:** Blacklist a user from using the /suggest command.\n**Usage:** /blacklistsuggestions [user]\n**Example:** /blacklistsuggestions Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser user = await UserInGuildAsync(Context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(downloadPath, "BlacklistedSuggestions.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers);
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{user.Username}` is already blacklisted from suggestions.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers + user.Id.ToString() + "\n");
                            SocketTextChannel suggestions = Context.Guild.GetTextChannel(249684395454234624);
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = $"Blacklisting User | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                            };
                            await suggestions.AddPermissionOverwriteAsync(user, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny), options);
                            await Context.Channel.SendMessageAsync($"Blacklisted **{user.Username}#{user.Discriminator}** from suggestions.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("unblacklistsuggestions", RunMode = RunMode.Async)]
        [Alias("unblacklistsuggestion", "suggestionunblacklist", "suggestionsunblacklist")]
        [Summary("Unblacklist a user from using the /suggest command")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task UnblacklistSuggestions([Remainder] string username = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /unblacklistsuggestions";
                    embed.Description = "**Description:** Unblacklist a user from using the /suggest command.\n**Usage:** /unblacklistsuggestions [user]\n**Example:** /unblacklistsuggestions Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser user = await UserInGuildAsync(Context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(downloadPath, "BlacklistedSuggestions.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            currentBlacklistedUsers = currentBlacklistedUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers);
                            SocketTextChannel suggestions = Context.Guild.GetTextChannel(249684395454234624);
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = $"Unblacklisting User | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                            };
                            await suggestions.RemovePermissionOverwriteAsync(user, options);
                            await Context.Channel.SendMessageAsync($"Removed blacklisted suggestions user **{user.Username}#{user.Discriminator}**.");
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{user.Username}` is not blacklisted from suggestions.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("listblacklistedsuggestions", RunMode = RunMode.Async)]
        [Alias("lbs", "listblacklistedsuggestion", "blacklistedsuggestions")]
        [Summary("Lists blacklisted users from suggestions.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ListBlacklistedSuggestions()
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                var blacklistedsuggestions = new StreamReader(path: Path.Combine(downloadPath, "BlacklistedSuggestions.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = "List Blacklisted Suggestions"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(rand.Next(255), rand.Next(255), rand.Next(255)),
                    Author = auth
                };
                string blacklistedUsers = "";
                string line = blacklistedsuggestions.ReadLine();
                while (line != null)
                {
                    string user = (Context.Guild.GetUser(Convert.ToUInt64(line))).Username + "#" + (Context.Guild.GetUser(Convert.ToUInt64(line))).Discriminator;
                    blacklistedUsers = blacklistedUsers + user + "\n";
                    line = blacklistedsuggestions.ReadLine();
                }
                blacklistedsuggestions.Close();
                if (blacklistedUsers.Length <= 0)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} there are no blacklisted suggestions users.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Suggestions Users";
                        y.Value = blacklistedUsers;
                        y.IsInline = false;
                    });
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("channelinfo", RunMode = RunMode.Async)]
        [Alias("infochannel", "channel", "ci")]
        [Summary("Gets information about a channel")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ChannelInfo([Remainder] string channelName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(channelName))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /channelinfo";
                    embed.Description = "**Description:** Get information about a channel.\n**Usage:** /channelinfo [channel name]\n**Example:** /channelinfo rules";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await ChannelInGuildAsync(Context.Guild, channelName) != null)
                    {
                        IGuildChannel channel = await ChannelInGuildAsync(Context.Guild, channelName) as IGuildChannel;
                        string type = "Text";
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = "Channel Created"
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                            Footer = footer,
                            Timestamp = channel.CreatedAt.Date
                        };
                        embed.AddField(y =>
                        {
                            y.Name = "ID";
                            y.Value = channel.Id;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Name";
                            y.Value = channel.Name;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Position";
                            y.Value = channel.Position;
                            y.IsInline = true;
                        });
                        if (channel is IVoiceChannel vChannel)
                        {
                            type = "Voice";
                            embed.AddField(y =>
                            {
                                y.Name = "Type";
                                y.Value = type;
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Bitrate";
                                y.Value = vChannel.Bitrate;
                                y.IsInline = true;
                            });
                            if (vChannel.CategoryId != null)
                            {
                                embed.AddField(y =>
                                {
                                    y.Name = "Category ID";
                                    y.Value = vChannel.CategoryId;
                                    y.IsInline = true;
                                });
                                embed.AddField(async y =>
                                {
                                    y.Name = "Category Name";
                                    y.Value = (await vChannel.GetCategoryAsync()).Name;
                                    y.IsInline = true;
                                });
                            }
                            if (vChannel.UserLimit != null)
                            {
                                embed.AddField(y =>
                                {
                                    y.Name = "User Limit";
                                    y.Value = vChannel.UserLimit;
                                    y.IsInline = true;
                                });
                            }
                            else
                            {
                                embed.AddField(y =>
                                {
                                    y.Name = "User Limit";
                                    y.Value = "Unlimited";
                                    y.IsInline = true;
                                });
                            }
                        }
                        else if (channel is ICategoryChannel cChannel)
                        {
                            type = "Category";
                            embed.AddField(y =>
                            {
                                y.Name = "Type";
                                y.Value = type;
                                y.IsInline = true;
                            });
                        }
                        else if (channel is ITextChannel tChannel)
                        {
                            embed.AddField(y =>
                            {
                                y.Name = "Type";
                                y.Value = type;
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Mention";
                                y.Value = $"`{tChannel.Mention.ToString()}`";
                                y.IsInline = true;
                            });
                            if (tChannel.CategoryId != null)
                            {
                                embed.AddField(y =>
                                {
                                    y.Name = "Category ID";
                                    y.Value = tChannel.CategoryId;
                                    y.IsInline = true;
                                });
                                embed.AddField(async y =>
                                {
                                    y.Name = "Category Name";
                                    y.Value = (await tChannel.GetCategoryAsync()).Name;
                                    y.IsInline = true;
                                });
                            }
                            string nsfw = "No";
                            if (tChannel.IsNsfw)
                            {
                                nsfw = "Yes";
                            }
                            embed.AddField(y =>
                            {
                                y.Name = "NSFW";
                                y.Value = nsfw;
                                y.IsInline = true;
                            });
                        }
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find channel `{channelName}`.");
                    }
                }
            }
            catch(Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("membercount", RunMode = RunMode.Async)]
        [Alias("mcount","usercount","ucount")]
        [Summary("Get the server member count")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task MemberCount()
        {
            await Context.Channel.SendMessageAsync($"Member count: **{Context.Guild.Users.Count}**.");
        }

        [Command("uptime", RunMode = RunMode.Async)]
        [Alias("utime", "ut")]
        [Summary("Get bot uptime")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Uptime()
        {
            var process = Process.GetCurrentProcess();
            var time = DateTime.Now - process.StartTime;
            var sb = new StringBuilder();
            if (time.Days > 0)
            {
                sb.Append($"{time.Days}d ");  /*Pulls the Uptime in Days*/
            }
            if (time.Hours > 0)
            {
                sb.Append($"{time.Hours}h ");  /*Pulls the Uptime in Hours*/
            }
            if (time.Minutes > 0)
            {
                sb.Append($"{time.Minutes}m ");  /*Pulls the Uptime in Minutes*/
            }
            sb.Append($"{time.Seconds}s ");  /*Pulls the Uptime in Seconds*/
            await Context.Channel.SendMessageAsync($"Current uptime: **{sb.ToString()}**");
        }

        [Command("roleinfo", RunMode = RunMode.Async)]
        [Alias("rinfo")]
        [Summary("Get information about a role.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task RoleInfo([Remainder] string roleName = null)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                embed.Title = "Command: /roleinfo";
                embed.Description = "**Description:** Get information about a role.\n**Usage:** /roleinfo [role name]\n**Example:** /roleinfo Admins";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (RoleInGuild(Context.Guild, roleName) != null)
                {
                    IRole role = RoleInGuild(Context.Guild, roleName);
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = "Role Created"
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = role.Color,
                        ThumbnailUrl = "https://dummyimage.com/80x80/" + HexConverter(role.Color) + "/" + HexConverter(role.Color),
                        Footer = footer,
                        Timestamp = role.CreatedAt.Date
                    };
                    embed.AddField(y =>
                    {
                        y.Name = "ID";
                        y.Value = role.Id;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Name";
                        y.Value = role.Name;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Color";
                        y.Value = "`#" + HexConverter(role.Color) + "`";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Mention";
                        y.Value = $"`{role.Mention.ToString()}`";
                        y.IsInline = true;
                    });
                    int roleMembers = 0;
                    foreach (IGuildUser user in Context.Guild.Users)
                    {
                        var roles = user.RoleIds;
                        foreach (ulong id in roles)
                        {
                            if (id == role.Id)
                            {
                                roleMembers = roleMembers + 1;
                                break;
                            }
                        }
                    }
                    embed.AddField(y =>
                    {
                        y.Name = "Members";
                        y.Value = roleMembers;
                        y.IsInline = true;
                    });
                    string hoisted = "";
                    if (role.IsHoisted)
                    {
                        hoisted = "Yes";
                    }
                    else
                    {
                        hoisted = "No";
                    }
                    embed.AddField(y =>
                    {
                        y.Name = "Hoisted";
                        y.Value = hoisted;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Position";
                        y.Value = role.Position;
                        y.IsInline = true;
                    });
                    string mentionable = "";
                    if (role.IsMentionable)
                    {
                        mentionable = "Yes";
                    }
                    else
                    {
                        mentionable = "No";
                    }
                    embed.AddField(y =>
                    {
                        y.Name = "Mentionable";
                        y.Value = mentionable;
                        y.IsInline = true;
                    });
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role `{roleName}`.");
                }
            }
        }

        [Command("roles", RunMode = RunMode.Async)]
        [Alias("rolelist")]
        [Summary("Get a list of server roles and member counts.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Roles()
        {
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                Name = "Server Roles",
                IconUrl = Context.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                IconUrl = Context.User.GetAvatarUrl(),
                Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(rand.Next(255), rand.Next(255), rand.Next(255)),
                Author = auth,
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            string roleNames = "", roleMemberCounts = "";
            foreach (IRole role in Context.Guild.Roles.OrderByDescending(x => x.Position))
            {
                int memberCount = 0;
                if (role.Name.Equals("@everyone"))
                {
                    //skip
                }
                else
                {
                    foreach (IGuildUser user in Context.Guild.Users)
                    {
                        var roles = user.RoleIds;
                        foreach (ulong id in roles)
                        {
                            if (id == role.Id)
                            {
                                memberCount = memberCount + 1;
                                break;
                            }
                        }
                    }
                    roleNames = roleNames + role.Name + "\n";
                    roleMemberCounts = roleMemberCounts + memberCount + " members\n";
                }
            }
            embed.AddField(y =>
            {
                y.Name = "Role";
                y.Value = roleNames;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Member Count";
                y.Value = roleMemberCounts;
                y.IsInline = true;
            });
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("warnings", RunMode = RunMode.Async)]
        [Alias("warns")]
        [Summary("Get warnings for the server or user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Warnings([Remainder] string username = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder();
            List<String> warnings = null;
            if (string.IsNullOrWhiteSpace(username))
            {
                auth.Name = $"Warnings - {Context.Guild.Name}";
                auth.IconUrl = Context.Guild.IconUrl;
                warnings = Database.Warnings();
                if (warnings.Count <= 0)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} nobody has been warned on this server.");
                    return;
                }
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser user = await UserInGuildAsync(Context.Guild, username);
                    auth.Name = $"Warnings - {user.Username}#{user.Discriminator}";
                    auth.IconUrl = user.GetAvatarUrl();
                    warnings = Database.Warnings(user);
                    if (warnings.Count <= 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} this user has no warnings.");
                        return;
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    return;
                }
            }
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(220, 220, 220),
                Author = auth
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                IconUrl = Context.User.GetAvatarUrl(),
                Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
            };
            embed.WithFooter(footer);
            embed.WithCurrentTimestamp();
            string temp = "";
            bool first = true, sent = false;
            foreach (string warn in warnings)
            {
                temp = temp + warn + "\n";
                if (temp.Length > 2000)
                {
                    if (first)
                    {
                        await Context.Channel.SendMessageAsync($"{warnings.Count} Logs Found:", false, embed.Build());
                        first = false;
                        sent = true;
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                    temp = "";
                    embed.Description = "";
                }
                embed.Description = embed.Description + warn + "\n";
            }
            if (!sent)
            {
                await Context.Channel.SendMessageAsync($"{warnings.Count} Logs Found:", false, embed.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("unban", RunMode = RunMode.Async)]
        [Alias("removeban")]
        [Summary("Unban a member")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Unban(string username = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /unban";
                embed.Description = "**Description:** Unban a member\n**Usage:** /unban [user], [optional reason]\n**Example:** /unban @Jiggmin, Appealed!";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else
            {
                bool isBanned = false;
                IUser user = null;
                var bans = await Context.Guild.GetBansAsync();
                foreach(IBan ban in bans)
                {
                    if (ban.User.Username == username || ban.User.Id.ToString() == username)
                    {
                        user = ban.User;
                        isBanned = true;
                    }
                }
                if (isBanned)
                {
                    ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Database.CaseCount() + 1} | Unban | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = ($"ID: {user.Id}")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.AddField(y =>
                    {
                        y.Name = "User";
                        y.Value = user.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Moderator";
                        y.Value = Context.User.Mention;
                        y.IsInline = true;
                    });
                    if (reason == null)
                    {
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unban", Context.User.Username + "#" + Context.User.Discriminator, "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was unbanned.");
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"No Reason Given | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await Context.Guild.RemoveBanAsync(user, options);
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = true;
                        });
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unban", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was unbanned.");
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await Context.Guild.RemoveBanAsync(user, options);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} `{username}` is not banned.");
                }
            }
        }

        [Command("undeafen", RunMode = RunMode.Async)]
        [Alias("undeaf")]
        [Summary("Undeafen a member")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.DeafenMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Undeafen(string username = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /undeafen";
                embed.Description = "**Description:** Undeafen a member\n**Usage:** /undeafen [user], [optional reason]\n**Example:** /undeafen @Jiggmin Listen now!";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser user = await UserInGuildAsync(Context.Guild, username);
                    if (CheckStaff(user.Id.ToString(), (user as SocketGuildUser).Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if ((user as SocketGuildUser).Roles.ElementAt(1).Position >= ((Context.Client as DiscordSocketClient).GetUser(383927022583545859) as SocketGuildUser).Roles.ElementAt(1).Position)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Database.CaseCount() + 1} | Undeafen | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 0, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = ($"ID: {user.Id}")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.AddField(y =>
                    {
                        y.Name = "User";
                        y.Value = user.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Moderator";
                        y.Value = Context.User.Mention;
                        y.IsInline = true;
                    });
                    if (reason == null)
                    {
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Undeafen", Context.User.Username + "#" + Context.User.Discriminator, "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was undeafened.");
                        await user.SendMessageAsync($"You have been undeafened on {Context.Guild.Name} by {Context.User.Mention}");
                        await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = false);
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = true;
                        });
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Undeafen", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was undeafened.");
                        await user.SendMessageAsync($"You have been undeafened on {Context.Guild.Name} by {Context.User.Mention} with reason {reason}");
                        await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = false);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                }
            }
        }

        [Command("deafen", RunMode = RunMode.Async)]
        [Alias("deaf")]
        [Summary("Deafen a member")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.DeafenMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Deafen(string username = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /deafen";
                embed.Description = "**Description:** Deafen a member\n**Usage:** /deafen [user], [optional reason]\n**Example:** /deafen @Jiggmin Don't listen!";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser iUser = await UserInGuildAsync(Context.Guild, username);
                    SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                    if (CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if (user.Roles.ElementAt(1).Position >= Context.Guild.GetUser(383927022583545859).Roles.ElementAt(1).Position)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Database.CaseCount() + 1} | Deafen | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 0, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = ($"ID: {user.Id}")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.AddField(y =>
                    {
                        y.Name = "User";
                        y.Value = user.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Moderator";
                        y.Value = Context.User.Mention;
                        y.IsInline = true;
                    });
                    if (reason == null)
                    {
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Deafen", Context.User.Username + "#" + Context.User.Discriminator, "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was deafened.");
                        await user.SendMessageAsync($"You have been deafened on {Context.Guild.Name} by {Context.User.Mention}");
                        await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = true);
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = true;
                        });
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Deafen", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was deafened.");
                        await user.SendMessageAsync($"You have been deafened on {Context.Guild.Name} by {Context.User.Mention} with reason {reason}");
                        await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = true);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                }
            }
        }

        [Command("softban", RunMode = RunMode.Async)]
        [Alias("sb")]
        [Summary("Softban a member (ban and immediate unban to delete user messages)")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Softban(string username = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(reason))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /softban";
                embed.Description = "**Description:** Softban a member (ban and immediate unban to delete user messages)\n**Usage:** /softban [user] [reason]\n**Example:** /softban @Jiggmin Not making Fred admin!";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser iUser = await UserInGuildAsync(Context.Guild, username);
                    SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                    if (CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if (user.Roles.ElementAt(1).Position >= Context.Guild.GetUser(383927022583545859).Roles.ElementAt(1).Position)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Database.CaseCount() + 1} | Softban | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = ($"ID: {user.Id}")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.AddField(y =>
                    {
                        y.Name = "User";
                        y.Value = user.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Moderator";
                        y.Value = Context.User.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = true;
                    });
                    Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Softban", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                    await banlog.SendMessageAsync("", false, embed.Build());
                    await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was softbanned.");
                    await user.SendMessageAsync($"You have been softbanned on {Context.Guild.Name} by {Context.User.Mention} with reason {reason}");
                    await Context.Guild.AddBanAsync(iUser, 7, $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}");
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Softban | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                    };
                    await Context.Guild.RemoveBanAsync(iUser, options);
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                }
            }
        }


        [Command("getcase", RunMode = RunMode.Async)]
        [Alias("getprior","case")]
        [Summary("Get info on a case")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task GetCase([Remainder] string caseN = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(caseN) || !int.TryParse(caseN, out int level_))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /getcase";
                    embed.Description = "**Description:** Get info on a case.\n**Usage:** /getcase [case number]\n**Example:** /getcase 1";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case Info",
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.Description = Database.GetCase(caseN);
                    if (embed.Description.Length <= 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} case could not be found.");
                        return;
                    }
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            catch(Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("modlogs", RunMode = RunMode.Async)]
        [Alias("priors")]
        [Summary("Get a list of mod logs for a user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Modlogs([Remainder] string username)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /getcase";
                    embed.Description = "**Description:** Get a list of mod logs for a user.\n**Usage:** /modlogs [user]\n**Example:** /modlogs @Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser user = await UserInGuildAsync(Context.Guild, username);
                        List<string> modlogs = Database.Modlogs(user);
                        EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                        {
                            Name = $"Mod Logs - {user.Username}#{user.Discriminator}",
                            IconUrl = user.GetAvatarUrl()
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(220, 220, 220),
                            Author = auth
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        if (modlogs.Count <= 0)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} this user has no priors.");
                            return;
                        }
                        string temp = "";
                        bool first = true, sent = false;
                        foreach (string modlog in modlogs)
                        {
                            temp = temp + modlog + "\n";
                            if (temp.Length > 2000)
                            {
                                if (first)
                                {
                                    await Context.Channel.SendMessageAsync($"{modlogs.Count} Logs Found:", false, embed.Build());
                                    first = false;
                                    sent = true;
                                }
                                else
                                {
                                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                                }
                                temp = "";
                                embed.Description = "";
                            }
                            embed.Description = embed.Description + modlog + "\n";
                        }
                        if (!sent)
                        {
                            await Context.Channel.SendMessageAsync($"{modlogs.Count} Logs Found:", false, embed.Build());
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            catch(Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("reason", RunMode = RunMode.Async)]
        [Alias("edit","editcase")]
        [Summary("Edit a reason for a mod log")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Reason(string caseN = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (caseN == null || reason == null || !int.TryParse(caseN, out int level_))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /getcase";
                embed.Description = "**Description:** Edit a reason for a mod log.\n**Usage:** /reason [case number] [reason]\n**Example:** /reason 1 Be nice :)";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                var messages = (banlog.GetMessagesAsync().Flatten()).Where(x => x.Embeds.Count > 0);
                string author = "", iconUrl = "", footerText = "";
                IUserMessage msgToEdit = null;
                IEmbed msgEmbed = null;
                foreach (IMessage message in messages.ToEnumerable())
                {
                    msgEmbed = message.Embeds.ElementAt(0);
                    if (msgEmbed.Author.Value.Name.Contains($"Case {caseN} | "))
                    {
                        msgToEdit = message as IUserMessage;
                        author = msgEmbed.Author.Value.Name;
                        iconUrl = msgEmbed.Author.Value.IconUrl;
                        footerText = msgEmbed.Footer.ToString();
                        break;
                    }
                }
                if (msgToEdit == null)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} that case could not be found.");
                    return;
                }
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = $"{author}",
                    IconUrl = iconUrl,
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = msgEmbed.Color,
                    Author = auth
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    Text = ($"{footerText}")
                };
                foreach (EmbedField field in msgEmbed.Fields)
                {
                    if (field.Name.Equals("Reason"))
                    {
                        embed.AddField(y =>
                        {
                            y.Name = field.Name;
                            y.Value = reason;
                            y.IsInline = field.Inline;
                        });
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = field.Name;
                            y.Value = field.Value;
                            y.IsInline = field.Inline;
                        });
                    }
                }
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();
                await msgToEdit.ModifyAsync(x => x.Embed = embed.Build());
                Database.UpdateReason(caseN, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                await Context.Channel.SendMessageAsync($"Updated case {caseN}.");
            }
        }

        [Command("warn", RunMode = RunMode.Async)]
        [Alias("w")]
        [Summary("Warn a member")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Warn(string username = null, [Remainder] string reason = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(reason))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /warn";
                    embed.Description = "**Description:** Warn a member.\n**Usage:** /warn [user] [reason]\n**Example:** /warn @Jiggmin No flooding";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser iUser = await UserInGuildAsync(Context.Guild, username);
                        SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                        if (CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if (user.Roles.ElementAt(1).Position >= Context.Guild.GetUser(383927022583545859).Roles.ElementAt(1).Position)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                            return;
                        }
                        ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                        EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                        {
                            Name = $"Case {Database.CaseCount() + 1} | Warn | {user.Username}#{user.Discriminator}",
                            IconUrl = user.GetAvatarUrl(),
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(220, 220, 220),
                            Author = auth
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = ($"ID: {user.Id}")
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        embed.AddField(y =>
                        {
                            y.Name = "User";
                            y.Value = user.Mention;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Moderator";
                            y.Value = Context.User.Mention;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = true;
                        });
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Warn", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was warned.");
                        await user.SendMessageAsync($"You have been warned on {Context.Guild.Name} by {Context.User.Mention} with reason {reason}");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            catch(Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("endgiveaway", RunMode = RunMode.Async)]
        [Alias("endg","gend")]
        [Summary("Ends the giveaway")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task EndGiveaway()
        {
            try
            {
                var messages = Context.Channel.GetMessagesAsync(100).ToEnumerable();
                IUserMessage message = null;
                IEmbed msgEmbed = null;
                EmbedBuilder embed = new EmbedBuilder();
                string item = null;
                foreach (var msg in messages)
                {
                    foreach (IMessage msg2 in msg)
                    {
                        if (msg2.Content.Equals(":confetti_ball: **Giveaway** :confetti_ball:") && msg2.Author.Id == 383927022583545859)
                        {
                            message = msg2 as IUserMessage;
                            var msgEmbeds = msg2.Embeds;
                            msgEmbed = msgEmbeds.ElementAt(0);
                            item = msgEmbed.Title;
                            embed.Title = msgEmbed.Title;
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = msgEmbed.Footer.Value.Text,
                                IconUrl = msgEmbed.Footer.Value.IconUrl
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            break;
                        }
                    }
                }
                var user = message.GetReactionUsersAsync(Emote.Parse("<:artifact:260898610734956574>"), 9999);
                var users = user.ElementAt(0).Result;
                if (users.Count <= 1)
                {
                    await Context.Channel.SendMessageAsync("Nobody Entered the Giveaway.");
                    await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");
                    embed.Description = "No winner.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else
                {
                    IUser randomUser = users.GetRandomElement();
                    while (randomUser.Id == 383927022583545859)
                    {
                        randomUser = users.GetRandomElement();
                    }
                    embed.Description = $"Winner: {randomUser.Mention}";
                    await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await Context.Channel.SendMessageAsync($"The winner of the {item} is {randomUser.Mention} !");
                }
            }
            catch (NullReferenceException)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find a giveaway in this channel.");
            }
        }

        [Command("repick", RunMode = RunMode.Async)]
        [Alias("reroll","redo")]
        [Summary("Repicks giveaway winner")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Repick()
        {
            try
            {
                var messages = Context.Channel.GetMessagesAsync(100).ToEnumerable();
                IUserMessage message = null;
                IEmbed msgEmbed = null;
                EmbedBuilder embed = new EmbedBuilder();
                string oldWinner = null, item = null;
                foreach (var msg in messages)
                {
                    foreach(IMessage msg2 in msg)
                    {
                        if (msg2.Content.Equals(":confetti_ball: **Giveaway Ended** :confetti_ball:") && msg2.Author.Id == 383927022583545859)
                        {
                            message = msg2 as IUserMessage;
                            var msgEmbeds = msg2.Embeds;
                            msgEmbed = msgEmbeds.ElementAt(0);
                            oldWinner = msgEmbed.Description.Substring(11, msgEmbed.Description.Length - 12);
                            item = msgEmbed.Title;
                            embed.Title = msgEmbed.Title;
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = msgEmbed.Footer.Value.Text,
                                IconUrl = msgEmbed.Footer.Value.IconUrl
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            break;
                        }
                    }
                }
                var user = message.GetReactionUsersAsync(Emote.Parse("<:artifact:260898610734956574>"), 9999);
                var users = user.ElementAt(0).Result;
                if (users.Count <= 1)
                {
                    await Context.Channel.SendMessageAsync("Nobody Entered the Giveaway.");
                    embed.Description = $"No winner.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else
                {
                    IUser randomUser = users.GetRandomElement();
                    while (randomUser.Id == 383927022583545859 || randomUser.Id.ToString().Equals(oldWinner))
                    {
                        if (user.ElementAt(users.Count - 2).Id.ToString().Equals(oldWinner) && users.ElementAt(users.Count - 1).Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync("Nobody else can win this giveaway.");
                            embed.Description = $"No winner.";
                            await message.ModifyAsync(x => x.Embed = embed.Build());
                            return;
                        }
                        randomUser = users.GetRandomElement();
                    }
                    embed.Description = $"Winner: {randomUser.Mention}";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await Context.Channel.SendMessageAsync($"The new winner of the {item} is {randomUser.Mention} !");
                }
            }
            catch(Exception)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find a giveaway in this channel.");
            }
        }

        [Command("giveaway", RunMode = RunMode.Async)]
        [Alias("give")]
        [Summary("Create a giveaway.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.AddReactions)]
        [RequireContext(ContextType.Guild)]
        public async Task Giveaway(string channel = null, string time = null, [Remainder] string item = null)
        {
            try
            {
                var channels = Context.Guild.TextChannels;
                ITextChannel giveawayChannel = ChannelExists(channels, channel);
                if (channel == null || giveawayChannel == null || time == null || !double.TryParse(time, out double num2) || Math.Round(Convert.ToDouble(time), 0) < 0 || item == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /giveaway";
                    embed.Description = "**Description:** Create a giveaway.\n**Usage:** /giveaway [channel] [time] [item]\n**Example:** /giveaway pr2-discussion 60 Cowboy Hat";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    double minutes = Math.Round(Convert.ToDouble(time), 0);
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                    };
                    embed.WithFooter(footer);
                    embed.Title = $"{item}";
                    embed.WithCurrentTimestamp();
                    embed.Description = $"React with <:artifact:260898610734956574> to enter the giveaway.\nTime left: {minutes} minutes.";
                    IUserMessage message = null;
                    try
                    {
                        message = await giveawayChannel.SendMessageAsync(":confetti_ball: **Giveaway** :confetti_ball:", false, embed.Build());
                    }
                    catch (Exception)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I do not have permission to speak in that channel.");
                        return;
                    }
                    await message.AddReactionAsync(Emote.Parse("<:artifact:260898610734956574>"));
                    int temptime = Convert.ToInt32(minutes) * 60000, divide = Convert.ToInt32(minutes / (minutes / 10)), count = 1;
                    bool ended = false;
                    while (count < divide)
                    {
                        await Task.Delay(temptime / divide);
                        if (message.Content.Equals(":confetti_ball: **Giveaway Ended** :confetti_ball:"))
                        {
                            count = count + divide;
                            ended = true;
                        }
                        else
                        {
                            embed.Description = $"React with <:artifact:260898610734956574> to enter the giveaway.\nTime left: {minutes - (minutes / 10 * count)} minutes.";
                            await message.ModifyAsync(x => x.Embed = embed.Build());
                            count = count + 1;
                        }
                    }
                    if (!ended)
                    {
                        var user = message.GetReactionUsersAsync(Emote.Parse("<:artifact:260898610734956574>"), 9999);
                        var users = user.ElementAt(0).Result;
                        if (users.Count <= 1)
                        {
                            await giveawayChannel.SendMessageAsync("Nobody Entered the Giveaway.");
                            embed.Description = $"Nobody Entered the Giveaway.";
                            await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");
                            await message.ModifyAsync(x => x.Embed = embed.Build());
                            return;
                        }
                        IUser randomUser = users.GetRandomElement();
                        while (randomUser.Id == 383927022583545859)
                        {
                            randomUser = users.GetRandomElement();
                        }
                        embed.Description = $"Winner: {randomUser.Mention}";
                        await giveawayChannel.SendMessageAsync($"The winner of the {item} is {randomUser.Mention} !");
                        await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                }
            }
            catch(Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("promote", RunMode = RunMode.Async)]
        [Alias("prom")]
        [Summary("Announces user promoted to temp/trial/perm mod on pr2")]
        [RequireContext(ContextType.Guild)]
        public async Task Promote(string type = null, [Remainder] string username = null)
        {
            if (Context.User.Id == 286922861044563969 || Context.User.Id == 239157630591959040 || Context.User.Id == 364951508955037696)
            {
                if (type == null || username == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /promote";
                    embed.Description = "**Description:** Annonce a PR2 promotion.\n**Usage:** /promote [type] [user]\n**Example:** /promote temp @Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                    return;
                }
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser iUser = await UserInGuildAsync(Context.Guild, username);
                    SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                    if (type.Equals("temp", StringComparison.InvariantCultureIgnoreCase) || type.Equals("temporary", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await Context.Channel.SendMessageAsync($"*{Context.User.Mention} has promoted {user.Mention} to a temporary moderator! " +
                        $"May they reign in hours of peace and prosperity! " +
                        $"Make sure you read the moderator guidelines at https://jiggmin2.com/forums/showthread.php?tid=12*");
                    }
                    else if (type.Equals("trial", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await Context.Channel.SendMessageAsync($"*{Context.User.Mention} has promoted {user.Mention} to a trial moderator! " +
                        $"May they reign in days of peace and prosperity! " +
                        $"Make sure you read the moderator guidelines at https://jiggmin2.com/forums/showthread.php?tid=12*");
                        IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "PR2 Staff Member".ToUpper());
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"New PR2 Staff Member | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await user.AddRolesAsync(role, options);
                    }
                    else if (type.Equals("perm", StringComparison.InvariantCultureIgnoreCase) || type.Equals("permanent", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await Context.Channel.SendMessageAsync($"*{Context.User.Mention} has promoted {user.Mention} to a permanent moderator! " +
                        $"May they reign in 1,000 years of peace and prosperity! " +
                        $"Make sure you read the moderator guidelines at https://jiggmin2.com/forums/showthread.php?tid=12*");
                        IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "PR2 Staff Member".ToUpper());
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"New PR2 Staff Member | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await user.AddRolesAsync(role, options);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the promotion type `{type}` was not recognised.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                }
            }
        }

        [Command("untemp", RunMode = RunMode.Async)]
        [Alias("removetemp")]
        [Summary("Removes temp mod from user mentioned")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task UnTemp([Remainder] string username = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /untemp";
                embed.Description = "**Description:** Untemp a user.\n**Usage:** /untemp [user]\n**Example:** /untemp @Jiggmin";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            if (await UserInGuildAsync(Context.Guild, username) != null)
            {
                IUser iUser = await UserInGuildAsync(Context.Guild, username);
                SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                if (!user.Roles.Any(e => e.Name == "Temp Mod"))
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} this user is not a temp mod.");
                    return;
                }
                IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Temp Mod".ToUpper());
                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = $"Untemp Modding User | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                };
                await user.RemoveRolesAsync(role, options);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} has removed temp mod from {user.Mention}");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
            }
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [Alias("removemute")]
        [Summary("Unmutes a user")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task UnMute(string username = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /unmute";
                embed.Description = "**Description:** Unmute a user.\n**Usage:** /unmute [user], [optional reason]\n**Example:** /unmute @Jiggmin, Speak now";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser iUser = await UserInGuildAsync(Context.Guild, username);
                    SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                    if (!user.Roles.Any(e => e.Name == "Muted"))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} this user is not muted.");
                        return;
                    }
                    IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Muted".ToUpper());
                    RequestOptions options = new RequestOptions();
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Database.CaseCount() + 1} | Unmute | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 255, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = ($"ID: {user.Id}")
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.AddField(y =>
                    {
                        y.Name = "User";
                        y.Value = user.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Moderator";
                        y.Value = Context.User.Mention;
                        y.IsInline = true;
                    });
                    if (reason == null)
                    {
                        options.AuditLogReason = $"Unmuting User | Mod: {Context.User.Username}#{Context.User.Discriminator}";
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unmute", Context.User.Username + "#" + Context.User.Discriminator, "No Reason - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                    }
                    else
                    {
                        options.AuditLogReason = $"Unmuting User | Mod: {Context.User.Username}#{Context.User.Discriminator} | Reason: {reason}";
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = true;
                        });
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unmute", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                    }
                    ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                    await banlog.SendMessageAsync("", false, embed.Build());
                    await user.RemoveRolesAsync(role, options);
                    await Context.Channel.SendMessageAsync($"Unmuted {user.Username}#{user.Discriminator}");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                }
            }
        }

        [Command("ping", RunMode = RunMode.Async)]
        [Alias("latency")]
        [Summary("Returns the latency between the bot and Discord")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Ping()
        {
            DiscordSocketClient client = Context.Client as DiscordSocketClient;
            await Context.Channel.SendMessageAsync($"`{client.Latency.ToString()} ms`");
        }

        [Command("botinfo", RunMode = RunMode.Async)]
        [Alias("fredinfo")]
        [Summary("Shows all bot info.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task BotInfo()
        {
            using (var process = Process.GetCurrentProcess())
            {
                var embed = new EmbedBuilder();
                var application = await Context.Client.GetApplicationInfoAsync();  /*for lib version*/
                embed.ImageUrl = application.IconUrl;  /*pulls bot Avatar. Not needed can be removed*/
                embed.WithCurrentTimestamp();
                embed.Title = "__**Bot Info**__";
                embed.WithColor(new Color(0x4900ff))  /*Hexacode colours*/

                .AddField(y =>  /*Adds a Field*/
                {
                    /*new embed field*/
                    y.Name = "Author.";  /*Field name here*/
                    y.Value = application.Owner.Username; application.Owner.Id.ToString();  /*Pulls the owner's name*/
                    y.IsInline = false;
                })

                .AddField(y =>  /*add new field, rinse and repeat*/
                {
                    y.Name = "Uptime.";
                    var time = DateTime.Now - process.StartTime;  /*Subtracts current time and start time to get Uptime*/
                    var sb = new StringBuilder();
                    if (time.Days > 0)
                    {
                        sb.Append($"{time.Days}d ");  /*Pulls the Uptime in Days*/
                    }
                    if (time.Hours > 0)
                    {
                        sb.Append($"{time.Hours}h ");  /*Pulls the Uptime in Hours*/
                    }
                    if (time.Minutes > 0)
                    {
                        sb.Append($"{time.Minutes}m ");  /*Pulls the Uptime in Minutes*/
                    }
                    sb.Append($"{time.Seconds}s ");  /*Pulls the Uptime in Seconds*/
                    y.Value = sb.ToString();
                    y.IsInline = true;
                })

                .AddField(y =>
                {
                    y.Name = "Discord.net version";  /*Title*/
                    y.Value = DiscordConfig.Version;  /*pulls discord lib version*/
                    y.IsInline = true;
                })

                .AddField(y =>
                {
                    y.Name = "Server Amount";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Count.ToString();  /*Numbers of servers the bot is in*/
                    y.IsInline = false;
                })

                .AddField(y =>
                {
                    y.Name = "Heap Size";
                    y.Value = GetHeapSize();   /*pulls ram usage of modules/heaps*/
                    y.IsInline = true;
                })

                .AddField(y =>
                {
                    y.Name = "Number Of Users";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Users.Count).ToString();  /*Counts users*/
                    y.IsInline = true;
                })

                .AddField(y =>
                {
                    y.Name = "Channels";
                    y.Value = (Context.Client as DiscordSocketClient).Guilds.Sum(g => g.Channels.Count).ToString();  /*Gets Number of channels*/
                    y.IsInline = true;
                });
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                };
                embed.WithFooter(footer);
                
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("userinfo", RunMode = RunMode.Async)]
        [Alias("uinfo", "UserInfo","whois")]
        [Summary("Returns information about the mentioned user")]
        [Name("userinfo `<user>`")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task UserInfo([Remainder] string username = null)
        {
            if (username == null)
            {
                username = Context.User.Username;
            }
            if (await UserInGuildAsync(Context.Guild, username) != null)
            {
                IUser iUser = await UserInGuildAsync(Context.Guild, username);
                SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                IApplication application = await Context.Client.GetApplicationInfoAsync(); // Gets The Client's info
                string createdMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(user.CreatedAt.Month);
                string createdDay = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(user.CreatedAt.DayOfWeek);
                string date = $"{createdDay}, {createdMonth} {user.CreatedAt.Day}, {user.CreatedAt.Year} {user.CreatedAt.DateTime.ToString("h:mm tt")}"; // Shows the date the account was made
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder() // Shows the Name of the user
                {
                    Name = user.Username + "#" + user.Discriminator,
                    IconUrl = user.GetAvatarUrl(),
                };
                EmbedBuilder embed = new EmbedBuilder()

                {
                    Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                    Author = auth
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})")
                };
                SocketGuildUser us = user as SocketGuildUser;
                String A = us.Discriminator; //Pulls the Discriminator
                ulong id = us.Id; //Gets the user's Id
                String S = date; //Pulls the Date the User's accound was created
                UserStatus status = us.Status; //Pulls The Status of the user
                string game = "";
                if (us.Activity == null)
                {
                    game = "None";
                }
                else
                {
                    game = us.Activity.Name;
                }
                string nickname = us.Nickname;
                var roles = us.Roles;
                string roleList = "";
                var guild = Context.Guild as SocketGuild;
                var guildusers = guild.Users;
                var guildusers2 = guildusers.OrderBy(x => x.JoinedAt);
                int position = 0;
                string joinedMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(user.JoinedAt.Value.Month);
                string joinedDay = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(user.JoinedAt.Value.DayOfWeek);
                string joined = $"{joinedDay}, {joinedMonth} {user.JoinedAt.Value.Day}, {user.JoinedAt.Value.Year} {user.JoinedAt.Value.LocalDateTime.ToString("h:mm tt")}";
                string pr2name = Database.GetPR2Name(user);
                if (pr2name == null || pr2name.Length <= 0)
                {
                    pr2name = "N/A";
                }
                foreach (SocketGuildUser member in guildusers2)
                {
                    if (member.Id == user.Id)
                    {
                        position = guildusers2.ToList().IndexOf(member) + 1;
                    }
                }
                foreach (SocketRole role in roles)
                {
                    roleList = roleList + role + ", ";
                }
                roleList = roleList.Substring(11).TrimEnd(new char[] { ' ', ',', ',' });
                if (nickname == null)
                {
                    nickname = "None";
                }
                embed.AddField(y =>
                {
                    y.Name = "ID";
                    y.Value = id;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Nickname";
                    y.Value = nickname;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Status";
                    y.Value = status;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Game";
                    y.Value = game;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Joined";
                    y.Value = joined;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Join Position";
                    y.Value = position;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Registered";
                    y.Value = date;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "PR2 Name";
                    y.Value = pr2name;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = $"Roles [{roles.Count - 1}]";
                    y.Value = roleList;
                    y.IsInline = false;
                });
                embed.ThumbnailUrl = user.GetAvatarUrl();
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
            }
        }

        [Command("guildinfo", RunMode = RunMode.Async)]
        [Alias("ginfo","serverinfo")]
        [Summary("Information about current server")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ServerInfo()
        {
            var gld = Context.Guild as SocketGuild;
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder() // Shows the Name of the user
            {
                Name = gld.Name,
                IconUrl = gld.IconUrl,
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(rand.Next(256), rand.Next(256), rand.Next(256)),
                Author = auth
            };
            string guildName = gld.Name;
            ulong guildID = gld.Id;
            SocketGuildUser owner = gld.Owner;
            var guildCreatedAt = gld.CreatedAt;
            string guildRegion = gld.VoiceRegionId;
            int guildUsers = gld.MemberCount;
            int guildRoles = gld.Roles.Count;
            int guildChannels = gld.Channels.Count;
            int guildHumans = gld.Users.Where(x => x.IsBot == false).Count();
            int guildBots = gld.Users.Where(x => x.IsBot == true).Count();
            int guildUsersOnline = gld.Users.Where(x => x.Status == UserStatus.Online || x.Status == UserStatus.Idle || x.Status == UserStatus.DoNotDisturb).Count();
            string createdMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(gld.CreatedAt.Month);
            string createdDay = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(gld.CreatedAt.DayOfWeek);
            string createdAt = $"{createdDay}, {createdMonth} {gld.CreatedAt.Day}, {gld.CreatedAt.Year} {gld.CreatedAt.DateTime.ToString("h:mm tt")}";
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                IconUrl = Context.User.GetAvatarUrl(),
                Text = ($"Server Created | {createdAt}")
            };
            embed.AddField(y =>
            {
                y.Name = "ID";
                y.Value = guildID;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Name";
                y.Value = guildName;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Owner";
                y.Value = owner.Mention;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Region";
                y.Value = guildRegion;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Channels";
                y.Value = guildChannels;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Members";
                y.Value = guildUsers;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Humans";
                y.Value = guildHumans;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Bots";
                y.Value = guildBots;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Online";
                y.Value = guildUsersOnline;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Roles";
                y.Value = guildRoles;
                y.IsInline = true;
            });
            embed.WithFooter(footer);
            embed.ThumbnailUrl = Context.Guild.IconUrl;
            
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("purge", RunMode = RunMode.Async)]
        [Alias("delete")]
        [Summary("Deletes number of messages specified, optional user mention.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task Purge(string amount = null, [Remainder] string username = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(amount) || !double.TryParse(amount, out double num2))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /purge";
                embed.Description = "**Description:** Delete a number of messages from a channel.\n**Usage:** /purge [amount], [optional user]\n**Example:** /purge 10, @Jiggmin";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            ITextChannel channel = Context.Channel as ITextChannel, log = Context.Guild.GetTextChannel(327575359765610496);
            int delete = Convert.ToInt32(amount);
            if (username == null)
            {
                var items = Context.Channel.GetMessagesAsync(delete).Flatten();
                if (delete == 1)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} message in {channel.Mention} .");
                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(items.ToEnumerable());
                    await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{Context.User.Username}#{Context.User.Discriminator}** " +
                    $"purged **{amount}** message in {channel.Mention}.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} messages in {channel.Mention} .");
                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(items.ToEnumerable());
                    await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{Context.User.Username}#{Context.User.Discriminator}** " +
                    $"purged **{amount}** messages in {channel.Mention}.");
                }
                return;
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser iUser = await UserInGuildAsync(Context.Guild, username);
                    SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                    var usermessages = (Context.Channel.GetMessagesAsync().Flatten()).Where(x => x.Author == user).Take(delete);
                    if (delete == 1)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} message from {user.Mention} in {channel.Mention} .");
                        await (Context.Channel as ITextChannel).DeleteMessagesAsync(usermessages.ToEnumerable());
                        await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{Context.User.Username}#{Context.User.Discriminator}** " +
                        $"purged **{amount}** message in {channel.Mention} from **{user.Username}#{user.Discriminator}**.");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} messages from {user.Mention} in {channel.Mention} .");
                        await (Context.Channel as ITextChannel).DeleteMessagesAsync(usermessages.ToEnumerable());
                        await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{Context.User.Username}#{Context.User.Discriminator}** " +
                        $"purged **{amount}** messages in {channel.Mention} from **{user.Username}#{user.Discriminator}**.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                }
            }
        }

        [Command("kick", RunMode = RunMode.Async)]
        [Alias("kick")]
        [Summary("Kicks the user mentioned")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task KickAsync(string username = null, [Remainder] string reason = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
                {
                    return;
                }

                if (username == null || reason == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /kick";
                    embed.Description = "**Description:** Kick a member.\n**Usage:** /kick [user] [reason]\n**Example:** /kick @Jiggmin Be nice :)";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser iUser = await UserInGuildAsync(Context.Guild, username);
                        SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                        if (CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if ((user as SocketGuildUser).Roles.ElementAt(1).Position >= Context.Guild.GetUser(383927022583545859).Roles.ElementAt(1).Position)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                            return;
                        }
                        ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                        EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                        {
                            Name = $"Case {Database.CaseCount() + 1} | Kick | {user.Username}#{user.Discriminator}",
                            IconUrl = user.GetAvatarUrl(),
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(255, 0, 0),
                            Author = auth
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = ($"ID: {user.Id}")
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        embed.AddField(y =>
                        {
                            y.Name = "User";
                            y.Value = user.Mention;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Moderator";
                            y.Value = Context.User.Mention;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = true;
                        });
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await user.SendMessageAsync($"You have been kicked from {Context.Guild.Name} by {Context.User.Mention} with reason {reason}.");
                        await user.KickAsync(null, options);
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Kick", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was kicked.");
                        await banlog.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            catch(Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Alias("Ban")]
        [Summary("Bans the user mentioned, needs reason.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Ban(string username = null, [Remainder] string reason = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
                {
                    return;
                }
                if (username == null || string.IsNullOrWhiteSpace(reason))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /ban";
                    embed.Description = "**Description:** Ban a member.\n**Usage:** /ban [user] [reason]\n**Example:** /ban @Jiggmin botting";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (await UserInGuildAsync(Context.Guild, username) != null)
                    {
                        IUser iUser = await UserInGuildAsync(Context.Guild, username);
                        SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                        if (CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if (user.Roles.ElementAt(1).Position >= Context.Guild.GetUser(383927022583545859).Roles.ElementAt(1).Position)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                            return;
                        }
                        ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                        EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                        {
                            Name = $"Case {Database.CaseCount() + 1} | Ban | {user.Username}#{user.Discriminator}",
                            IconUrl = user.GetAvatarUrl(),
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(220, 220, 220),
                            Author = auth
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = ($"ID: {user.Id}")
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        embed.AddField(y =>
                        {
                            y.Name = "User";
                            y.Value = user.Mention;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Moderator";
                            y.Value = Context.User.Mention;
                            y.IsInline = true;
                        });
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = true;
                        });
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await user.SendMessageAsync($"You have been banned from {Context.Guild.Name} by {Context.User.Mention} with reason {reason}.");
                        await Context.Guild.AddBanAsync(user, 1, null, options);
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Ban", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was banned.");
                        await banlog.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
            }
            catch (Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("mute", RunMode = RunMode.Async)]
        [Alias("Mute")]
        [Summary("Mutes the user mentioned")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Mute(string username = null, string time = null, [Remainder] string reason = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }

            if (username == null || string.IsNullOrWhiteSpace(time) || string.IsNullOrWhiteSpace(reason) || Math.Round(Convert.ToDouble(time), 0) < 1)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /mute";
                embed.Description = "**Description:** Mute a member.\n**Usage:** /mute [user] [time] [reason]\n**Example:** /mute @Jiggmin 10 Flooding";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser iUser = await UserInGuildAsync(Context.Guild, username);
                    SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                    if (CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if ((user as SocketGuildUser).Roles.ElementAt(1).Position >= Context.Guild.GetUser(383927022583545859).Roles.ElementAt(1).Position)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    double minutes = Math.Round(Convert.ToDouble(time), 0);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Database.CaseCount() + 1} | Mute | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = ($"ID: {user.Id}")
                    };
                    ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                    IEnumerable<IRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Muted".ToUpper());
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Muting User | Reason: {reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                    };
                    await user.AddRolesAsync(role, options);
                    embed.WithCurrentTimestamp();
                    embed.WithFooter(footer);
                    embed.AddField(y =>
                    {
                        y.Name = "User";
                        y.Value = user.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Moderator";
                        y.Value = Context.User.Mention;
                        y.IsInline = true;
                    });
                    if (minutes == 1)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Length";
                            y.Value = $"{minutes} minute";
                            y.IsInline = true;
                        });
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Length";
                            y.Value = $"{minutes} minutes";
                            y.IsInline = true;
                        });
                    }
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = true;
                    });
                    await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was muted.");
                    await banlog.SendMessageAsync("", false, embed.Build());
                    Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Mute", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                    if (minutes == 1)
                    {
                        await user.SendMessageAsync($"You have been muted in {Context.Guild.Name} by {Context.User.Mention} for {reason} and for a length of {minutes} minute.");
                    }
                    else
                    {
                        await user.SendMessageAsync($"You have been muted in {Context.Guild.Name} by {Context.User.Mention} for {reason} and for a length of {minutes} minutes.");
                    }
                    int mutetime = Convert.ToInt32(minutes) * 60000;
                    Task task = Task.Run(async () =>
                    {
                        await Task.Delay(mutetime);
                        SocketGuildUser usr = user as SocketGuildUser;
                        if (usr.Roles.Any(e => e.Name.ToUpperInvariant() == "Muted".ToUpperInvariant()))
                        {
                            options.AuditLogReason = "Unmuting User | Reason: Mute expired";
                            await user.RemoveRolesAsync(role, options);
                            EmbedAuthorBuilder auth2 = new EmbedAuthorBuilder()
                            {
                                Name = $"Case {Database.CaseCount() + 1} | Unmute | {user.Username}#{user.Discriminator}",
                                IconUrl = user.GetAvatarUrl(),
                            };
                            EmbedBuilder embed2 = new EmbedBuilder()
                            {
                                Color = new Color(0, 255, 0),
                                Author = auth2
                            };
                            EmbedFooterBuilder footer2 = new EmbedFooterBuilder()
                            {
                                Text = ($"ID: {user.Id}")
                            };
                            embed2.WithCurrentTimestamp();
                            embed2.WithFooter(footer2);
                            embed2.AddField(y =>
                            {
                                y.Name = "User";
                                y.Value = user.Mention;
                                y.IsInline = true;
                            });
                            embed2.AddField(y =>
                            {
                                y.Name = "Moderator";
                                y.Value = "<@383927022583545859>";
                                y.IsInline = true;
                            });
                            embed2.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = "Auto";
                                y.IsInline = true;
                            });
                            await banlog.SendMessageAsync("", false, embed2.Build());
                            await user.SendMessageAsync($"You are now unmuted.");
                            Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unmute", moderator: "Fred the G. Cactus#1000", reason: "Auto - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        }
                        else
                        {
                            return;
                        }
                    });
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                }
            }
        }

        [Command("temp", RunMode = RunMode.Async)]
        [Alias("tempmod")]
        [Summary("Makes a user temp mod on the server")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Temp(string username = null, string time = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (username == null || string.IsNullOrWhiteSpace(time) || !double.TryParse(time, out double num) || Math.Round(Convert.ToDouble(time), 0) < 1)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /temp";
                embed.Description = "**Description:** Temp mod a memeber.\n**Usage:** /temp [user] [time]\n**Example:** /temp @Jiggmin 60";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            if (username == Context.User.Username)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you cannot temp mod yourself.");
                return;
            }
            if (await UserInGuildAsync(Context.Guild, username) != null)
            {
                IUser iUser = await UserInGuildAsync(Context.Guild, username);
                SocketGuildUser user = Context.Guild.GetUser(iUser.Id);
                double minutes = Math.Round(Convert.ToDouble(time), 0);
                ITextChannel roles = user.Guild.GetChannel(260272249976782848) as ITextChannel;

                IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Temp Mod".ToUpper());
                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = $"Temp Modding User | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                };
                await user.AddRolesAsync(role, options);
                if (time.Equals("1"))
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for {time} minute. " +
                                $"May they reign in hours of peace and prosperity! Read more about mods and what they do in {roles.Mention}");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for {time} minutes. " +
                                $"May they reign in hours of peace and prosperity! Read more about mods and what they do in {roles.Mention}");
                }
                int temptime = Convert.ToInt32(minutes) * 60000;
                Task task = Task.Run(async () =>
                {
                    await Task.Delay(temptime);
                    if (user.Roles.Any(e => e.Name.ToUpperInvariant() == "Temp Mod".ToUpperInvariant()))
                    {
                        options.AuditLogReason = "Untemp Modding User | Reason: Temp Time Over";
                        await user.RemoveRolesAsync(role, options);
                    }
                });
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
            }
        }

        #endregion

        #region Manager

        [Command("addrole", RunMode = RunMode.Async)]
        [Alias("+role","createrole")]
        [Summary("Add a new role, with optional color and hoist.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task AddRole([Remainder] string settings = null)
        {
            if (string.IsNullOrWhiteSpace(settings))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /addrole";
                embed.Description = "**Description:** Add a new role, with optional color and hoist.\n**Usage:** /addrole [name] [hex color] [hoist]\n**Example:** /addrole Test #FF0000 true";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = $"Created by: {Context.User.Username}#{Context.User.Discriminator}"
                };
                if (settings.Contains("#"))
                {
                    string[] settingsSplit = settings.Split("#");
                    if (settingsSplit[1].Contains("true"))
                    {
                        string[] settingsSplit2 = settingsSplit[1].Split(" ");
                        try
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(settingsSplit2[0].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                            bool hoisted = Convert.ToBoolean(settingsSplit2[1]);
                            await Context.Guild.CreateRoleAsync(settingsSplit[0], null, new Color(color.R, color.G, color.B), hoisted, options);
                            await Context.Channel.SendMessageAsync($"Created role **{settingsSplit[0]}** with color **#{settingsSplit2[0]}**, and is displayed separately.");
                        }
                        catch (FormatException)
                        {
                            await Context.Guild.CreateRoleAsync(settings, null, null, false, options);
                            await Context.Channel.SendMessageAsync($"Created role **{settings}**.");
                        }
                    }
                    else
                    {
                        try
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(settingsSplit[1].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                            await Context.Guild.CreateRoleAsync(settingsSplit[0], null, new Color(color.R, color.G, color.B), false, options);
                            await Context.Channel.SendMessageAsync($"Created role **{settingsSplit[0]}** with color **#{settingsSplit[1]}**.");
                        }
                        catch(FormatException)
                        {
                            await Context.Guild.CreateRoleAsync(settings, null, null, false, options);
                            await Context.Channel.SendMessageAsync($"Created role **{settings}**.");
                        }
                    }
                }
                else
                {
                    await Context.Guild.CreateRoleAsync(settings);
                    await Context.Channel.SendMessageAsync($"Created role **{settings}**.");
                }
            }
        }

        [Command("clearwarn", RunMode = RunMode.Async)]
        [Alias("clearwarns", "removewarnigs")]
        [Summary("Clear warnings a user")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ClearWarn([Remainder] string username = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /clearwarn";
                embed.Description = "**Description:** Clear warnings a user.\n**Usage:** /clearwarn [user]\n**Example:** /clearwarn @Jiggmin";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser user = await UserInGuildAsync(Context.Guild, username);
                    int warnCount = Convert.ToInt32(Database.WarnCount(user));
                    if (warnCount == 0)
                    {
                        await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** has no warnings.");
                    }
                    else
                    {
                        Database.ClearWarn(user);
                        if (warnCount == 1)
                        {
                            await Context.Channel.SendMessageAsync($"Cleared **{warnCount}** warning for **{user.Username}#{user.Discriminator}**.");
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"Cleared **{warnCount}** warnings for **{user.Username}#{user.Discriminator}**.");
                        }
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{username}**.");
                }
            }
        }

        [Command("delrole", RunMode = RunMode.Async)]
        [Alias("-role","deleterole")]
        [Summary("Delete a role")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task DelRole([Remainder] string roleName = null)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /delrole";
                embed.Description = "**Description:** Delete a role.\n**Usage:** /delrole [role]\n**Example:** /delrole Admins";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (RoleInGuild(Context.Guild, roleName) != null)
                {
                    IRole role = RoleInGuild(Context.Guild, roleName);
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Deleted by: {Context.User.Username}#{Context.User.Discriminator}"
                    };
                    await role.DeleteAsync(options);
                    await Context.Channel.SendMessageAsync($"Deleted role **{roleName}**");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role **{roleName}**.");
                }
            }
        }

        [Command("mentionable", RunMode = RunMode.Async)]
        [Alias("rolementionable")]
        [Summary("Toggle making a role mentionable on/off")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Mentionable([Remainder] string roleName = null)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /mentionable";
                embed.Description = "**Description:** Toggle making a role mentionable on/off.\n**Usage:** /mentionable [role]\n**Example:** /mentionable Admins";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (RoleInGuild(Context.Guild, roleName) != null)
                {
                    IRole role = RoleInGuild(Context.Guild, roleName);
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Toggled Mentionable by {Context.User.Username}#{Context.User.Discriminator}"
                    };
                    if (role.IsMentionable)
                    {
                        await role.ModifyAsync(x => x.Mentionable = false, options);
                        await Context.Channel.SendMessageAsync($"The role **{role.Name}** is no longer mentionable");
                    }
                    else
                    {
                        await role.ModifyAsync(x => x.Mentionable = true, options);
                        await Context.Channel.SendMessageAsync($"The role **{role.Name}** is now mentionable");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role **{roleName}**.");
                }
            }
        }

        [Command("setnick", RunMode = RunMode.Async)]
        [Alias("setnickname")]
        [Summary("Change the nickname of a user.")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        [RequireContext(ContextType.Guild)]
        public async Task SetNick(string username = null, [Remainder] string nickname = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(nickname))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /setnick";
                embed.Description = "**Description:** Change the nickname of a user.\n**Usage:** /setnick [user] [new nickname]\n**Example:** /setnick Jiggmin Jiggy";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (await UserInGuildAsync(Context.Guild, username) != null)
                {
                    IUser user = await UserInGuildAsync(Context.Guild, username);
                    SocketGuildUser gUser = Context.Guild.GetUser(user.Id);
                    if (nickname.Length > 32)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} a users nickname cannot be longer than 32 characters.");
                    }
                    else
                    {
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"Changed by: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await gUser.ModifyAsync(x => x.Nickname = nickname, options);
                        await Context.Channel.SendMessageAsync($"Successfully set the nickname of **{gUser.Username}#{gUser.Discriminator}** to **{nickname}**.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{username}**.");
                }
            }
        }

        [Command("nick", RunMode = RunMode.Async)]
        [Alias("botnick")]
        [Summary("Change the bot nickname.")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        [RequireContext(ContextType.Guild)]
        public async Task Nick([Remainder] string nickname = null)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /nick";
                embed.Description = "**Description:** Change the bot nickname.\n**Usage:** /nick [new nickname]\n**Example:** /nick Fred the Giant Cactus";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (nickname.Length > 32)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} my nickname cannot be longer than 32 characters.");
                }
                else
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Changed by: {Context.User.Mention}#{Context.User.Discriminator}"
                    };
                    await Context.Guild.GetUser(383927022583545859).ModifyAsync(x => x.Nickname = nickname, options);
                    await Context.Channel.SendMessageAsync($"Successfully set my nickname to **{nickname}**.");
                }
            }
        }

        [Command("rolecolor", RunMode = RunMode.Async)]
        [Alias("colorrole","setcolor","rolecolour")]
        [Summary("Change the color of a role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task RoleColor([Remainder] string roleNameAndColor = null)
        {
            if (string.IsNullOrWhiteSpace(roleNameAndColor))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /rolecolor";
                embed.Description = "**Description:** Change the color of a role.\n**Usage:** /rolecolor [role] [hex color]\n**Example:** /rolecolor Admins #FF0000";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (roleNameAndColor.Contains("#"))
                {
                    string[] split = roleNameAndColor.Split("#");
                    string roleName = split[0];
                    if (RoleInGuild(Context.Guild, roleName) != null)
                    {
                        IRole role = RoleInGuild(Context.Guild, roleName);
                        try
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(split[1].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                            await role.ModifyAsync(x => x.Color = new Color(color.R, color.G, color.B));
                            await Context.Channel.SendMessageAsync($"Successfully changed the color of **{role.Name}** to **#{split[1]}**.");
                        }
                        catch(FormatException)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the hex color **#{split[1]}** is not a valid hex color.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role **{roleName}**.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you need to enter a hex color to change the role to.");
                }
            }
        }

        [Command("addjoinablerole")]
        [Alias("addjoinrole","ajr","+jr","+joinablerole")]
        [Summary("Adds a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddJoinableRole([Remainder] string roleName = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (roleName == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /addjoinablerole";
                embed.Description = "**Description:** Add a role that users can add themselves to.\n**Usage:** /addjoinablerole [role]\n**Example:** /addjoinablerole Arti";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (RoleInGuild(Context.Guild, roleName) != null)
                {
                    IRole role = RoleInGuild(Context.Guild, roleName);
                    string currentJoinableRoles = File.ReadAllText(path: Path.Combine(downloadPath, "JoinableRoles.txt"));
                    if (currentJoinableRoles.Contains(role.Id.ToString()))
                    {
                        File.WriteAllText(Path.Combine(downloadPath, "JoinableRoles.txt"), currentJoinableRoles);
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{role.Name}` is already a joinable role.");
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(downloadPath, "JoinableRoles.txt"), currentJoinableRoles + role.Id.ToString() + "\n");
                        await Context.Channel.SendMessageAsync($"Added joinable role **{role.Name}**.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{roleName}` does not exist or could not be found.");
                }
            }
        }

        [Command("deljoinablerole")]
        [Alias("deljoinrole", "djr", "-jr", "-joinablerole")]
        [Summary("Removes a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DelJoinableRole([Remainder] string roleName = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (roleName == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /deljoinablerole";
                embed.Description = "**Description:** Delete a role that users can add themselves to.\n**Usage:** /deljoinablerole [role]\n**Example:** /deljoinablerole Arti";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (RoleInGuild(Context.Guild, roleName) != null)
                {
                    IRole role = RoleInGuild(Context.Guild, roleName);
                    string joinableRoles = File.ReadAllText(path: Path.Combine(downloadPath, "JoinableRoles.txt"));
                    if (joinableRoles.Contains(role.Id.ToString()))
                    {
                        joinableRoles = joinableRoles.Replace(role.Id.ToString() + "\n", string.Empty);
                        File.WriteAllText(Path.Combine(downloadPath, "JoinableRoles.txt"), joinableRoles);
                        await Context.Channel.SendMessageAsync($"Removed joinable role **{role.Name}**.");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{role.Name}` is not a joinable role.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{roleName}` does not exist or could not be found.");
                }
            }
        }

        [Command("addmod", RunMode = RunMode.Async)]
        [Alias("+mod")]
        [Summary("Add a bot moderator or group of moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddModUser([Remainder] string mod = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
                {
                    return;
                }
                if (mod == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /addmod";
                    embed.Description = "**Description:** Add a bot moderator or group of moderators.\n**Usage:** /addmod [user or role]\n**Example:** /addmod Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (RoleInGuild(Context.Guild, mod) != null)
                    {
                        IRole role = RoleInGuild(Context.Guild, mod);
                        string currentModRoles = File.ReadAllText(path: Path.Combine(downloadPath, "DiscordStaffRoles.txt"));
                        if (currentModRoles.Contains(role.Id.ToString()))
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "DiscordStaffRoles.txt"), currentModRoles);
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{role.Name}` is already a mod role.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "DiscordStaffRoles.txt"), currentModRoles + role.Id.ToString() + "\n");
                            await Context.Channel.SendMessageAsync($"Added mod role **{role.Name}**.");
                        }
                    }
                    else if (await UserInGuildAsync(Context.Guild, mod) != null)
                    {
                        IUser user = await UserInGuildAsync(Context.Guild, mod);
                        string currentModUsers = File.ReadAllText(path: Path.Combine(downloadPath, "DiscordStaff.txt"));
                        if (currentModUsers.Contains(user.Id.ToString()))
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "DiscordStaff.txt"), currentModUsers);
                            await Context.Channel.SendMessageAsync($"{ Context.User.Mention} the user `{user.Username}` is already a mod.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(downloadPath, "DiscordStaff.txt"), currentModUsers + user.Id.ToString() + "\n");
                            await Context.Channel.SendMessageAsync($"Added mod **{user.Username}#{user.Discriminator}**.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role `{mod}`.");
                    }
                }
            }
            catch (Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("delmod", RunMode = RunMode.Async)]
        [Alias("-mod","deletemod")]
        [Summary("Remove a bot moderator or group of moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DelMod([Remainder] string mod = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(mod))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /delmod";
                    embed.Description = "**Description:** Remove a bot moderator or group of moderators.\n**Usage:** /delmod [user or role]\n**Example:** /delmod Jiggmin";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (RoleInGuild(Context.Guild, mod) != null)
                    {
                        ulong roleID = (RoleInGuild(Context.Guild, mod)).Id;
                        string modRoles = File.ReadAllText(path: Path.Combine(downloadPath, "DiscordStaffRoles.txt"));
                        if (modRoles.Contains(roleID.ToString()))
                        {
                            modRoles = modRoles.Replace(roleID.ToString() + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(downloadPath, "DiscordStaffRoles.txt"), modRoles);
                            await Context.Channel.SendMessageAsync($"Removed mod role **{mod}**.");
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{mod}` is not a mod role.");
                        }
                    }
                    else if (await UserInGuildAsync(Context.Guild, mod) != null)
                    {
                        ulong userID = (await UserInGuildAsync(Context.Guild, mod)).Id;
                        string modUsers = File.ReadAllText(path: Path.Combine(downloadPath, "DiscordStaff.txt"));
                        if (modUsers.Contains(userID.ToString()))
                        {
                            modUsers = modUsers.Replace(userID.ToString() + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(downloadPath, "DiscordStaff.txt"), modUsers);
                            await Context.Channel.SendMessageAsync($"Removed mod **{mod}**.");
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{mod}` is not a mod.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role `{mod}`.");
                    }
                }
            }
            catch (Exception e)
            {
                await ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("listmods", RunMode = RunMode.Async)]
        [Alias("listmod","showmods")]
        [Summary("List moderators")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListMods()
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            var modRoles = new StreamReader(path: Path.Combine(downloadPath, "DiscordStaffRoles.txt"));
            var modUsers = new StreamReader(path: Path.Combine(downloadPath, "DiscordStaff.txt"));
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = Context.Guild.IconUrl,
                Name = "List Mods"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(rand.Next(255), rand.Next(255), rand.Next(255)),
                Author = auth
            };
            string modU = "", modR = "";
            string line = modUsers.ReadLine();
            while (line != null)
            {
                string user = (Context.Guild.GetUser(Convert.ToUInt64(line))).Username + "#" + (Context.Guild.GetUser(Convert.ToUInt64(line))).Discriminator;
                modU = modU + user + "\n";
                line = modUsers.ReadLine();
            }
            modUsers.Close();
            line = modRoles.ReadLine();
            while (line != null)
            {
                string role = (Context.Guild.GetRole(Convert.ToUInt64(line))).Name;
                modR = modR + role + "\n";
                line = modRoles.ReadLine();
            }
            modRoles.Close();
            if (modR.Length > 0)
            {
                embed.AddField(y =>
                {
                    y.Name = "Mod Roles";
                    y.Value = modR;
                    y.IsInline = false;
                });
            }
            if (modU.Length > 0)
            {
                embed.AddField(y =>
                {
                    y.Name = "Mod Users";
                    y.Value = modU;
                    y.IsInline = false;
                });
            }
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        #endregion

    }
}