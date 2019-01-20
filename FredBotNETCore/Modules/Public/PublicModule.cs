using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static FredBotNETCore.WeatherDataCurrent;

namespace FredBotNETCore.Modules.Public
{
    [Name("Public")]
    [Summary("Module of commands that anyone can use.")]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        #region Everyone

        [Command("pay", RunMode = RunMode.Async)]
        [Alias("give")]
        [Summary("Pay money to another user.")]
        [RequireContext(ContextType.DM)]
        public async Task Pay(string amount = null, [Remainder] string username = null)
        {
            List<string> result = Database.CheckExistingUser(Context.User);
            if (result.Count() <= 0)
            {
                Database.EnterUser(Context.User);
            }
            string pr2name = Database.GetPR2Name(Context.User);
            if (pr2name.Equals("Not verified") || pr2name.Length <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(amount) || !int.TryParse(amount, out int money) || string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /pay";
                    embed.Description = "**Description:** Pay money to another user.\n**Usage:** /pay [amount] [user]\n**Example:** /pay 100 Jiggmin";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    if (Database.GetBalance(Context.User) > money)
                    {
                        if (Extensions.UserInGuild(Context.Message, Context.Client.GetGuild(528679522707701760), username) != null)
                        {
                            SocketUser user = Extensions.UserInGuild(Context.Message, Context.Client.GetGuild(528679522707701760), username);
                            Database.SetBalance(Context.User, Database.GetBalance(Context.User) - money);
                            Database.SetBalance(user, Database.GetBalance(user) + money);
                            await ReplyAsync($"{Context.User.Mention} you have successfully paid **{Format.Sanitize(user.Username)}#{user.Discriminator} ${money.ToString("N0")}**.\nYour new balance is ${Database.GetBalance(Context.User).ToString("N0")}.");
                            try
                            {
                                await user.SendMessageAsync($"{user.Mention} you have been paid **${money}** by **{Format.Sanitize(Context.User.Username)}#{Context.User.Discriminator}**");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                await ReplyAsync($"{Context.User.Mention} I could not DM **{Format.Sanitize(user.Username)}#{user.Discriminator}** to notify them of your payment.");
                            }
                        }
                        else
                        {
                            await ReplyAsync($"{Context.User.Mention} the user with name or ID **{Format.Sanitize(username)}** does not exist or could not be found.");
                        }
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} you do not have enough money to do that.");
                    }
                }
            }
        }

        [Command("balance", RunMode = RunMode.Async)]
        [Alias("bal")]
        [Summary("Tells a user how much money they have.")]
        [RequireContext(ContextType.DM)]
        public async Task Balance([Remainder] string username = null)
        {
            List<string> result = Database.CheckExistingUser(Context.User);
            if (result.Count() <= 0)
            {
                Database.EnterUser(Context.User);
            }
            string pr2name = Database.GetPR2Name(Context.User);
            if (pr2name.Equals("Not verified") || pr2name.Length <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    int bal = Database.GetBalance(Context.User);
                    await ReplyAsync($"{Context.User.Mention} your balance is **${bal.ToString("N0")}**.");
                }
                else
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Client.GetGuild(528679522707701760), username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(Context.Message, Context.Client.GetGuild(528679522707701760), username);
                        result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        int bal = Database.GetBalance(user);
                        await ReplyAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}'s** balance is **${bal.ToString("N0")}**.");
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the user with name or ID **{Format.Sanitize(username)}** does not exist or could not be found.");
                    }
                }
            }
        }

        [Command("jackpot", RunMode = RunMode.Async)]
        [Alias("lottobal")]
        [Summary("See how much money is in the jackpot.")]
        [RequireContext(ContextType.DM)]
        public async Task Jackpot()
        {
            List<string> result = Database.CheckExistingUser(Context.User);
            if (result.Count() <= 0)
            {
                Database.EnterUser(Context.User);
            }
            string pr2name = Database.GetPR2Name(Context.User);
            if (pr2name.Equals("Not verified") || pr2name.Length <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                int lottobal = int.Parse(File.ReadAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt")));
                await ReplyAsync($"{Context.User.Mention} the jackpot is currently worth **${lottobal.ToString("N0")}**.");
            }
        }

        [Command("leaderboard", RunMode = RunMode.Async)]
        [Alias("lb")]
        [Summary("Shows users with the most money.")]
        [RequireContext(ContextType.DM)]
        public async Task Leaderboard()
        {
            List<string> result = Database.CheckExistingUser(Context.User);
            if (result.Count() <= 0)
            {
                Database.EnterUser(Context.User);
            }
            string pr2name = Database.GetPR2Name(Context.User);
            if (pr2name.Equals("Not verified") || pr2name.Length <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = "Leaderboard",
                    IconUrl = Context.Client.GetGuild(528679522707701760).IconUrl
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    Author = auth,
                };
                List<string> topUsers = Database.GetTop();
                List<string> topBalance = new List<string>();
                foreach (string userid in topUsers)
                {
                    try
                    {
                        topBalance.Add("$" + Database.GetBalance(Context.Client.GetUser(Convert.ToUInt64(userid))).ToString("N0"));
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
                string leaderboard = null;
                int i = 0;
                foreach (string userid in topUsers)
                {
                    try
                    {
                        leaderboard = leaderboard + "**" + (i + 1).ToString() + ".** " + Format.Sanitize(Context.Client.GetUser(Convert.ToUInt64(userid)).Username) + "#" + Context.Client.GetUser(Convert.ToUInt64(userid)).Discriminator + " - " + topBalance.ElementAt(i) + "\n";
                        i++;
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
                embed.Description = leaderboard;
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("lotto", RunMode = RunMode.Async)]
        [Alias("lottery")]
        [Summary("Have a go at winning the jackpot.")]
        [RequireContext(ContextType.DM)]
        public async Task Lotto(string ticketsS = null)
        {
            List<string> result = Database.CheckExistingUser(Context.User);
            if (result.Count() <= 0)
            {
                Database.EnterUser(Context.User);
            }
            string pr2name = Database.GetPR2Name(Context.User);
            if (pr2name.Equals("Not verified") || pr2name.Length <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(ticketsS) || !int.TryParse(ticketsS, out int tickets))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /lotto";
                    embed.Description = "**Description:** Enter the lottery.\n**Usage:** /lotto [tickets]\n**Example:** /lotto 10";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    if (tickets > Database.GetBalance(Context.User))
                    {
                        await ReplyAsync($"{Context.User.Mention} you don't have enough money to buy that many tickets.");
                    }
                    else
                    {
                        Database.SetBalance(Context.User, Database.GetBalance(Context.User) - tickets);
                        int lottobal = int.Parse(File.ReadAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt")));
                        int chance = Convert.ToInt32(tickets / (lottobal + 0.00) * 100);
                        EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                        {
                            Name = "Lottery",
                            IconUrl = Context.User.GetAvatarUrl()
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            Author = auth,
                            Description = $"Jackpot: ${lottobal.ToString("N0")}\nScratching Tickets..."
                        };
                        IUserMessage message = await ReplyAsync("", false, embed.Build());
                        await Task.Delay(500);
                        if (chance >= 100)
                        {
                            embed.Description = $"{Format.Sanitize(Context.User.Username)}#{Context.User.Discriminator} won the jackpot of ${lottobal.ToString("N0")}!";
                            await message.ModifyAsync(x => x.Embed = embed.Build());
                            Database.SetBalance(Context.User, Database.GetBalance(Context.User) + lottobal);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt"), "100");
                        }
                        else
                        {
                            int random = Extensions.random.Next(100);
                            if (random <= chance)
                            {
                                embed.Description = $"{Format.Sanitize(Context.User.Username)}#{Context.User.Discriminator} won the jackpot of ${lottobal.ToString("N0")}!";
                                await message.ModifyAsync(x => x.Embed = embed.Build());
                                Database.SetBalance(Context.User, Database.GetBalance(Context.User) + lottobal);
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt"), "100");
                            }
                            else
                            {
                                embed.Description = $"{Format.Sanitize(Context.User.Username)}#{Context.User.Discriminator} did not win the jackpot of ${lottobal.ToString("N0")}.";
                                int newbal = lottobal + tickets;
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt"), newbal.ToString());
                                await message.ModifyAsync(x => x.Embed = embed.Build());
                            }
                        }
                    }
                }
            }
        }

        [Command("daily", RunMode = RunMode.Async)]
        [Alias("work")]
        [Summary("Collect daily cash.")]
        [RequireContext(ContextType.DM)]
        public async Task Daily()
        {
            List<string> result = Database.CheckExistingUser(Context.User);
            if (result.Count() <= 0)
            {
                Database.EnterUser(Context.User);
            }
            string pr2name = Database.GetPR2Name(Context.User);
            if (pr2name.Equals("Not verified") || pr2name.Length <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                string date = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
                if (Database.GetLastUsed(Context.User).Equals(date))
                {
                    await ReplyAsync($"{Context.User.Mention} you have already used this command today.");
                }
                else
                {
                    int money = Extensions.random.Next(5, 50);
                    await ReplyAsync($"{Context.User.Mention} you worked for ${money} today.");
                    Database.SetBalance(Context.User, Database.GetBalance(Context.User) + money);
                    Database.SetLastUsed(Context.User, date);
                }
            }
        }

        [Command("verify", RunMode = RunMode.Async)]
        [Alias("verifyme")]
        [Summary("Verifies a user on the server.")]
        public async Task Verify([Remainder] string s1 = null)
        {
            if (!(Context.Channel is SocketDMChannel))
            {
                await ReplyAsync($"{Context.User.Mention} check your DMs to verify your PR2 account.");
            }
            await Context.User.SendMessageAsync($"Hello {Context.User.Mention} , to verify your PR2 account please send a PM to `FredTheG.CactusBot` on PR2 " +
                $"saying only `{(await Context.User.GetOrCreateDMChannelAsync()).Id}`.\nThen once you have sent the PM type `/verifycomplete <PR2 account name>` without <> in this channel. PR2 account name = name of " +
                $"account you sent the PM from.");
            //\n\nOr to verify your JV2 account type `/verifyjv2 <JV2 account name>` whitout <> in this channel. JV2 account name = name of your JV2 account.
        }

        [Command("verifycomplete", RunMode = RunMode.Async)]
        [Alias("verifydone", "verified")]
        [Summary("Makes Fred the G. Cactus check pms for user.")]
        [RequireContext(ContextType.DM)]
        public async Task Verified([Remainder] string username)
        {
            SocketGuild guild = Context.Client.GetGuild(528679522707701760);
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /verifycomplete";
                embed.Description = "**Description:** Verify your PR2 account.\n**Usage:** /verifycomplete [PR2 username]\n**Example:** /verifycomplete Jiggmin";
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                SocketGuildUser user = null;
                try
                {
                    user = guild.GetUser(Context.User.Id);
                }
                catch
                {
                    await ReplyAsync($"{Context.User.Mention} you are not a memeber of **{Format.Sanitize(guild.Name)}**.");
                }
                string pr2token = File.ReadAllText(Path.Combine(Extensions.downloadPath, "PR2Token.txt"));
                Dictionary<string, string> values = new Dictionary<string, string>
                {
                    { "count", "10" },
                    { "start", "0" },
                    { "token", pr2token }
                };
                HttpClient web = new HttpClient();
                FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = await web.PostAsync("https://pr2hub.com/messages_get.php?", content);
                string responseString = await response.Content.ReadAsStringAsync();
                string[] pms = responseString.Split('}');
                int tries = 0;
                foreach (string message_id in pms)
                {
                    string name = Extensions.GetBetween(message_id, "name\":\"", "\",\"group");
                    if (name.ToLower().Equals(username.ToLower()))
                    {
                        string message = Extensions.GetBetween(message_id, "message\":\"", "\",\"time");
                        if (message.Equals(Context.Channel.Id.ToString()))
                        {
                            List<string> result = Database.CheckExistingUser(user);
                            if (result.Count() <= 0)
                            {
                                Database.EnterUser(user);
                            }
                            if (int.Parse(Extensions.GetBetween(await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + name), "{\"rank\":", ",\"hats\":")) < 15)
                            {
                                await ReplyAsync($"{Context.User.Mention} your PR2 account must be at least rank 15 if you want to link it to your Discord account.");
                            }
                            else
                            {
                                result = Database.CheckForVerified(user, "Not verified");
                                bool isVerified = false;
                                if (result.Count() <= 0)
                                {
                                    isVerified = true;
                                }
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "User Verify",
                                    IconUrl = guild.IconUrl
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    Text = $"ID: {Context.User.Id}"
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Author = author,
                                    Color = new Color(0, 255, 0),
                                    Footer = footer
                                };
                                embed.WithCurrentTimestamp();
                                if (isVerified)
                                {
                                    string pr2name = Database.GetPR2Name(user);
                                    if (pr2name.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        await ReplyAsync($"{Context.User.Mention} that is already your verified account.");
                                        return;
                                    }
                                    ulong id = Database.GetDiscordID(username);
                                    if (id != 0)
                                    {
                                        await ReplyAsync($"{Context.User.Mention} the Discord user with ID: **{id}** has already verified themselves with the PR2 Account: **{Format.Sanitize(username)}**. Please contact an admin on the {Format.Sanitize(guild.Name)} Discord Server for futher assistance.");
                                        return;
                                    }
                                    Database.VerifyUser(user, username);
                                    SocketTextChannel channel = guild.GetTextChannel(Extensions.GetLogChannel());
                                    embed.Description = $"{Context.User.Mention} changed their verified account from **{Format.Sanitize(pr2name)}** to **{Format.Sanitize(username)}**.";
                                    await channel.SendMessageAsync("", false, embed.Build());
                                    if (!user.Username.Equals(username))
                                    {
                                        RequestOptions options = new RequestOptions()
                                        {
                                            AuditLogReason = "Setting nickname to PR2 name."
                                        };
                                        await user.ModifyAsync(x => x.Nickname = username, options);
                                    }
                                    await ReplyAsync($"{Context.User.Mention} you have successfully changed your verified account from **{Format.Sanitize(pr2name)}** to **{Format.Sanitize(username)}**.");
                                }
                                else
                                {
                                    ulong id = Database.GetDiscordID(username);
                                    if (id != 0)
                                    {
                                        await ReplyAsync($"{Context.User.Mention} the Discord user with ID: **{id}** has already verified themselves with the PR2 Account: **{Format.Sanitize(username)}**. Please contact an admin on the Platform Racing Group Discord Server for futher assistance.");
                                        return;
                                    }
                                    Database.VerifyUser(user, username);
                                    SocketTextChannel channel = guild.GetTextChannel(Extensions.GetLogChannel());
                                    embed.Description = $"Verified {Context.User.Mention} who is **{Format.Sanitize(username)}** on PR2.";
                                    await channel.SendMessageAsync("", false, embed.Build());
                                    IEnumerable<SocketRole> role = guild.Roles.Where(input => input.Name.ToUpper() == "Verified".ToUpper());
                                    RequestOptions options = new RequestOptions()
                                    {
                                        AuditLogReason = "Verifying User."
                                    };
                                    await user.AddRolesAsync(role, options);
                                    if (!user.Username.Equals(username))
                                    {
                                        options.AuditLogReason = "Setting nickname to PR2 name.";
                                        await user.ModifyAsync(x => x.Nickname = username, options);
                                    }
                                    await ReplyAsync($"{Context.User.Mention} you have successfully verified your PR2 Account.");
                                }
                                WebClient wc = new WebClient();
                                wc.Headers.Add("Referer", "https://pr2hub.com/");
                                System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection
                                {
                                    { "message", $"Hey {username}! Thank you for verifying your PR2 Account with Fred the G. Cactus on the Discord Server. You verified your PR2 Account with the Discord Account {Context.User.Username}#{Context.User.Discriminator} ({Context.User.Id}). If you did not do this you should change your password and your email on your account as well to make sure it is secure." },
                                    { "to_name", username },
                                    { "token", pr2token }
                                };
                                byte[] responsebytes = wc.UploadValues("https://pr2hub.com/message_send.php", "POST", reqparm);
                                responseString = Encoding.UTF8.GetString(responsebytes);
                                while (responseString.Contains("Error: You've sent 4 messages in the past 60 seconds. Please wait a bit before sending another message.") || responseString.Contains("Error: Slow down a bit, yo."))
                                {
                                    await Task.Delay(10000);
                                    response = await web.PostAsync("https://pr2hub.com/message_send.php?", content);
                                    responseString = await response.Content.ReadAsStringAsync();
                                }
                                break;
                            }
                        }
                        else
                        {
                            await ReplyAsync($"{Context.User.Mention} I found a PM from {Format.Sanitize(username)} but it did not say what I was " +
                                $"expecting it to say ({Context.Channel.Id}).\nPlease send resend the PM and then do /verifycomplete with your PR2 " +
                                $"name after.");
                            break;
                        }
                    }
                    else if (tries == 10)
                    {
                        await ReplyAsync($"{Context.User.Mention} , something went wrong in the verification process. " +
                            $"Make sure you typed your PR2 name correctly, or actually sent the PM.");
                        break;
                    }
                    tries = tries + 1;
                }
                if (responseString.Equals("{\"error\":\"Could not find a valid login token. Please log in again.\"}"))
                {
                    await ReplyAsync($"{Context.User.Mention} the token of FredTheG.CactusBot has expired. Please contact a PR2 Staff Member so that they can update it.");
                }
            }
        }

        [Command("suggest", RunMode = RunMode.Async)]
        [Alias("suggestion")]
        [Summary("Adds suggestion to suggestion channel")]
        [RequireContext(ContextType.Guild)]
        public async Task Suggest([Remainder] string suggestion = null)
        {
            if (Context.Guild.Id != 528679522707701760)
            {
                return;
            }
            if (File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt")).Contains(Context.User.Id.ToString()))
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
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (suggestion.Length < 18 || suggestion.Length > 800)
                {
                    await ReplyAsync($"{Context.User.Mention} your suggestion must be between at least 18 and no more than 800 characters long.");
                    return;
                }
                SocketTextChannel channel = Context.Guild.Channels.Where(x => x.Name.ToUpper() == "suggestions".ToUpper()).First() as SocketTextChannel;
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = Context.User.Username + "#" + Context.User.Discriminator,
                    IconUrl = Context.User.GetAvatarUrl(),
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    Author = auth
                };
                embed.WithCurrentTimestamp();
                embed.Description = $"**Suggestion:** {Format.Sanitize(suggestion)}";
                IUserMessage msg = await channel.SendMessageAsync("", false, embed.Build());
                await msg.AddReactionAsync(new Emoji("👍"));
                await msg.AddReactionAsync(new Emoji("👎"));
            }
        }

        [Command("weather", RunMode = RunMode.Async)]
        [Alias("weath")]
        [Summary("Tells you weather on a location")]
        [RequireContext(ContextType.DM)]
        public async Task Weather([Remainder] string city = null)
        {
            if (city == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /weather";
                embed.Description = "**Description:** Get weather about a city.\n**Usage:** /weather [city]\n**Example:** /weather Bristol";
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                WeatherReportCurrent weather;
                weather = JsonConvert.DeserializeObject<WeatherReportCurrent>(Extensions.GetWeatherAsync(city).Result);
                double lon = 0;
                try
                {
                    lon = weather.Coord.Lon;
                }
                catch (NullReferenceException)
                {
                    await ReplyAsync($"{Context.User.Mention} the city `{Format.Sanitize(city)}` does not exist or could not be found.");
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
                string[] directions = new string[]
                {
                    "North", "NorthEast", "East", "SouthEast", "South", "SouthWest", "West", "NorthWest", "North"
                };
                int index = Convert.ToInt32(Math.Floor((deg + 23) / 45));
                string direction = directions[index];
                int all = weather.Clouds.All;
                string country = weather.Sys.Country;
                int sunrise = weather.Sys.Sunrise;
                int sunset = weather.Sys.Sunset;
                DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime sunriseDate = start.AddSeconds(sunrise).ToLocalTime();
                DateTime sunsetDate = start.AddSeconds(sunset).ToLocalTime();
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                };
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Name = $"{Format.Sanitize(city)}, {country}"
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
                await ReplyAsync("", false, embed.Build());
            }
        }

        #endregion

        #region PR2 Commands

        [Command("hint", RunMode = RunMode.Async)]
        [Alias("arti", "artifact")]
        [Summary("Tells the user the current artifact hint.")]
        public async Task Hint()
        {
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/files/artifact_hint.txt");
                }
                catch (HttpRequestException)
                {
                    await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    string levelname = Extensions.GetBetween(text, "hint\":\"", "\"");
                    string person = Extensions.GetBetween(text, "finder_name\":\"", "\"");
                    if (person.Length < 1)
                    {
                        await ReplyAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(levelname))}**. Maybe I can remember more later!!");
                    }
                    else
                    {
                        ulong userID = Convert.ToUInt64(Database.GetUserID(person));
                        if (userID != 1)
                        {
                            SocketGuild guild = Context.Client.GetGuild(528679522707701760);
                            if (Extensions.UserInGuild(null, guild, userID.ToString()) != null)
                            {
                                SocketGuildUser user = guild.GetUser(userID);
                                await ReplyAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(levelname))}**. Maybe I can remember more later!!\nThe first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(person))} ({Format.Sanitize(user.Username)}#{user.Discriminator})**!!");
                            }
                            else
                            {
                                await ReplyAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(levelname))}**. Maybe I can remember more later!!\nThe first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(person))}**!!");
                            }
                        }
                        else
                        {
                            await ReplyAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(levelname))}**. Maybe I can remember more later!!\nThe first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(person))}**!!");
                        }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(pr2name))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /view";
                    embed.Description = "**Description:** View a PR2 account by name.\n**Usage:** /view [PR2 username]\n**Example:** /view Jiggmin";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    if (Context.Message.MentionedUsers.Count > 0)
                    {
                        SocketUser user = null;
                        int argPos = 0;
                        if (Context.Message.HasStringPrefix("/", ref argPos))
                        {
                            user = Context.Message.MentionedUsers.First();
                        }
                        else if (Context.Message.HasMentionPrefix(Context.Client.CurrentUser, ref argPos) && Context.Message.MentionedUsers.Count > 1)
                        {
                            user = Context.Message.MentionedUsers.ElementAt(1);
                        }
                        if (user == null)
                        {
                            await ReplyAsync($"{Context.User.Mention} that user does not exist or could not be found.");
                            return;
                        }
                        List<string> result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        pr2name = Database.GetPR2Name(user);
                        if (pr2name.Equals("Not verified"))
                        {
                            await ReplyAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
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
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    if (pr2name.Contains("%20%7C%20") && Context.Channel is IDMChannel)
                    {
                        string[] pr2users = pr2name.Split("%20%7C%20");
                        if (pr2users.Count() <= 5)
                        {
                            foreach (string pr2user in pr2users)
                            {
                                string pr2info = null;
                                try
                                {
                                    pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2user);
                                }
                                catch (HttpRequestException)
                                {
                                    await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                                }
                                if (pr2info != null)
                                {
                                    if (pr2info.Contains(value: "{\"success\":false,\"error\":\""))
                                    {
                                        await ReplyAsync($"{Context.User.Mention} the user **{Format.Sanitize(Uri.UnescapeDataString(pr2user))}** does not exist or could not be found.");
                                    }
                                    else
                                    {
                                        string rank = Extensions.GetBetween(pr2info, "\"rank\":", ",\"hats\":");
                                        string hats = Extensions.GetBetween(pr2info, ",\"hats\":", ",\"group\":\"");
                                        string group = Extensions.GetBetween(pr2info, ",\"group\":\"", "\",\"friend\":");
                                        string status = Extensions.GetBetween(pr2info, ",\"status\":\"", "\",\"loginDate\":\"");
                                        string lastlogin = Extensions.GetBetween(pr2info, "\",\"loginDate\":\"", "\",\"registerDate\":\"");
                                        string createdat = Extensions.GetBetween(pr2info, "\",\"registerDate\":\"", "\",\"hat\":\"");
                                        string guild = Extensions.GetBetween(pr2info, "\",\"guildName\":\"", "\",\"name\":\"");
                                        string name = Uri.UnescapeDataString(Extensions.GetBetween(pr2info, "\",\"name\":\"", "\",\"userId"));
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
                                            group = "[Moderator](https://pr2hub.com/staff.php)";
                                        }
                                        if (group == "3")
                                        {
                                            group = "[Admin](https://pr2hub.com/staff.php)";
                                        }
                                        if (createdat.Contains("1970"))
                                        {
                                            createdat = "Age of Heroes";
                                        }
                                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                        {
                                            Name = $"-- {name} --",
                                            Url = "https://pr2hub.com/player_search.php?name=" + Uri.EscapeDataString(name)
                                        };
                                        embed.WithAuthor(author);
                                        if (guild.Length < 1)
                                        {
                                            embed.Description = $"{status}\n**Group:** {group}\n**Guild:** none\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                                        }
                                        else
                                        {
                                            embed.Description = $"{status}\n**Group:** {group}\n**Guild:** [{Format.Sanitize(Uri.UnescapeDataString(guild))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild)})\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                                        }
                                        await ReplyAsync("", false, embed.Build());
                                    }
                                    await Task.Delay(1000);
                                }
                            }
                        }
                        else
                        {
                            await ReplyAsync($"{Context.User.Mention} you can only view a maximum of 5 users at a time.");
                            return;
                        }
                    }
                    else
                    {
                        string pr2info = null;
                        try
                        {
                            pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
                        }
                        catch (HttpRequestException)
                        {
                            await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                        }
                        if (pr2info != null)
                        {
                            if (pr2info.Contains(value: "{\"success\":false,\"error\":\""))
                            {
                                await ReplyAsync($"{Context.User.Mention} the user **{Format.Sanitize(Uri.UnescapeDataString(pr2name))}** does not exist or could not be found.");
                            }
                            else
                            {
                                string rank = Extensions.GetBetween(pr2info, "\"rank\":", ",\"hats\":");
                                string hats = Extensions.GetBetween(pr2info, ",\"hats\":", ",\"group\":\"");
                                string group = Extensions.GetBetween(pr2info, ",\"group\":\"", "\",\"friend\":");
                                string status = Extensions.GetBetween(pr2info, ",\"status\":\"", "\",\"loginDate\":\"");
                                string lastlogin = Extensions.GetBetween(pr2info, "\",\"loginDate\":\"", "\",\"registerDate\":\"");
                                string createdat = Extensions.GetBetween(pr2info, "\",\"registerDate\":\"", "\",\"hat\":\"");
                                string guild = Extensions.GetBetween(pr2info, "\",\"guildName\":\"", "\",\"name\":\"");
                                string name = Uri.UnescapeDataString(Extensions.GetBetween(pr2info, "\",\"name\":\"", "\",\"userId"));
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
                                    group = "[Moderator](https://pr2hub.com/staff.php)";
                                }
                                if (group == "3")
                                {
                                    group = "[Admin](https://pr2hub.com/staff.php)";
                                }
                                if (createdat.Contains("1970"))
                                {
                                    createdat = "Age of Heroes";
                                }
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"-- {name} --",
                                    Url = "https://pr2hub.com/player_search.php?name=" + Uri.EscapeDataString(pr2name)
                                };
                                embed.WithAuthor(author);
                                if (guild.Length < 1)
                                {
                                    embed.Description = $"{status}\n**Group:** {group}\n**Guild:** none\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                                }
                                else
                                {
                                    embed.Description = $"{status}\n**Group:** {group}\n**Guild:** [{Format.Sanitize(Uri.UnescapeDataString(guild))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild)})\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                                }
                                await ReplyAsync("", false, embed.Build());
                            }
                        }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /viewid";
                    embed.Description = "**Description:** View a PR2 account by ID.\n**Usage:** /viewid [PR2 user ID]\n**Example:** /viewid 1";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();

                    string pr2info = null;
                    try
                    {
                        await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?user_id=" + id);
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (pr2info != null)
                    {
                        if (pr2info.Contains(value: "{\"success\":false,\"error\":\""))
                        {
                            await ReplyAsync($"{Context.User.Mention} the user with ID **{id}** does not exist or could not be found.");
                        }
                        else
                        {
                            string rank = Extensions.GetBetween(pr2info, "\"rank\":", ",\"hats\":");
                            string hats = Extensions.GetBetween(pr2info, ",\"hats\":", ",\"group\":\"");
                            string group = Extensions.GetBetween(pr2info, ",\"group\":\"", "\",\"friend\":");
                            string status = Extensions.GetBetween(pr2info, ",\"status\":\"", "\",\"loginDate\":\"");
                            string lastlogin = Extensions.GetBetween(pr2info, "\",\"loginDate\":\"", "\",\"registerDate\":\"");
                            string createdat = Extensions.GetBetween(pr2info, "\",\"registerDate\":\"", "\",\"hat\":\"");
                            string guild = Extensions.GetBetween(pr2info, "\",\"guildName\":\"", "\",\"name\":\"");
                            string name = Uri.UnescapeDataString(Extensions.GetBetween(pr2info, "\",\"name\":\"", "\",\"userId"));
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
                                group = "[Moderator](https://pr2hub.com/staff.php)";
                            }
                            if (group == "3")
                            {
                                group = "[Admin](https://pr2hub.com/staff.php)";
                            }
                            if (createdat.Contains("1970"))
                            {
                                createdat = "Age of Heroes";
                            }
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"-- {name} --",
                                Url = "https://pr2hub.com/player_search.php?name=" + Uri.EscapeDataString(name)
                            };
                            embed.WithAuthor(author);
                            if (guild.Length < 1)
                            {
                                embed.Description = $"{status}\n**Group:** {group}\n**Guild:** none\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                            }
                            else
                            {
                                embed.Description = $"{status}\n**Group:** {group}\n**Guild:** [{Format.Sanitize(Uri.UnescapeDataString(guild))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild)})\n**Rank:** {rank}\n**Hats:** {hats}\n**Joined:** {createdat}\n**Active:** {lastlogin}";
                            }
                            await ReplyAsync("", false, embed.Build());
                        }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
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
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    if (Context.Message.MentionedUsers.Count > 0)
                    {
                        SocketUser user = null;
                        int argPos = 0;
                        if (Context.Message.HasStringPrefix("/", ref argPos))
                        {
                            user = Context.Message.MentionedUsers.First();
                        }
                        else if (Context.Message.HasMentionPrefix(Context.Client.CurrentUser, ref argPos) && Context.Message.MentionedUsers.Count > 1)
                        {
                            user = Context.Message.MentionedUsers.ElementAt(1);
                        }
                        if (user == null)
                        {
                            await ReplyAsync($"{Context.User.Mention} that user does not exist or could not be found.");
                            return;
                        }
                        List<string> result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        string pr2Name = Database.GetPR2Name(user);
                        if (guildname.Equals("Not verified"))
                        {
                            await ReplyAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                        string pr2userinfo = null;
                        try
                        {
                            pr2userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2Name);
                        }
                        catch (HttpRequestException)
                        {
                            await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                            return;
                        }
                        string guild = Extensions.GetBetween(pr2userinfo, "\",\"guildName\":\"", "\",\"name\":\"");
                        if (guild.Length <= 0)
                        {
                            await ReplyAsync($"{Context.User.Mention} that users account is not a member of a guild.");
                            return;
                        }
                        guildname = guild;
                    }
                    string pr2info = null;
                    try
                    {
                        pr2info = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname);
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (pr2info != null)
                    {
                        if (pr2info.Contains(value: "{\"success\":false,\"error\":\""))
                        {
                            await ReplyAsync($"{Context.User.Mention} the guild **{Format.Sanitize(guildname)}** does not exist or could not be found.");
                        }
                        else
                        {
                            string name = Extensions.GetBetween(pr2info, "guild_name\":\"", "\",\"creation");
                            string createdat = Extensions.GetBetween(pr2info, "creation_date\":\"", "\"");
                            string members = Extensions.GetBetween(pr2info, "member_count\":\"", "\"");
                            string gptotal = int.Parse(Extensions.GetBetween(pr2info, "gp_total\":\"", "\"")).ToString("N0");
                            string gptoday = int.Parse(Extensions.GetBetween(pr2info, "gp_today\":\"", "\"")).ToString("N0");
                            string guildpic = Extensions.GetBetween(pr2info, "emblem\":\"", "\"");
                            string note = Extensions.GetBetween(pr2info, "note\":\"", "\"");
                            string active = Extensions.GetBetween(pr2info, "active_count\":\"", "\"");
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"-- {name} --",
                                Url = "https://pr2hub.com/guild_search.php?name=" + guildname.Replace(" ", "%20")
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = Context.User.GetAvatarUrl(),
                                Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            embed.ThumbnailUrl = "https://pr2hub.com/emblems/" + guildpic;
                            embed.Description = $"**Created At:** {createdat}\n**Members:** {members} ({active} active)\n**GP Total:** {gptotal}\n**GP Today:** {gptoday}\n**Description:** {Format.Sanitize(note)}";
                            await ReplyAsync("", false, embed.Build());
                        }
                    }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /guildid";
                    embed.Description = "**Description:** View a PR2 guild by ID.\n**Usage:** /guildid [PR2 guild ID]\n**Example:** /guildid 183";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    string pr2info = null;
                    try
                    {
                        pr2info = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&id=" + id);
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (pr2info != null)
                    {
                        if (pr2info.Contains(value: "{\"success\":false,\"error\":\""))
                        {
                            await ReplyAsync($"{Context.User.Mention} the guild with ID **{id}** does not exist or could not be found.");
                        }
                        else
                        {
                            string name = Extensions.GetBetween(pr2info, "\"guild_name\":\"", "\",\"");
                            string createdat = Extensions.GetBetween(pr2info, "creation_date\":\"", "\"");
                            string members = Extensions.GetBetween(pr2info, "member_count\":\"", "\"");
                            string gptotal = int.Parse(Extensions.GetBetween(pr2info, "gp_total\":\"", "\"")).ToString("N0");
                            string gptoday = int.Parse(Extensions.GetBetween(pr2info, "gp_today\":\"", "\"")).ToString("N0");
                            string guildpic = Extensions.GetBetween(pr2info, "emblem\":\"", "\"");
                            string note = Extensions.GetBetween(pr2info, "note\":\"", "\"");
                            string active = Extensions.GetBetween(pr2info, "active_count\":\"", "\"");
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"-- {name} --",
                                Url = "https://pr2hub.com/guild_search.php?id=" + id
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = Context.User.GetAvatarUrl(),
                                Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            embed.ThumbnailUrl = "https://pr2hub.com/emblems/" + guildpic;
                            embed.Description = $"**Created At:** {createdat}\n**Members:** {members} ({active} active)\n**GP Total:** {gptotal}\n**GP Today:** {gptoday}\n**Description:** {Format.Sanitize(note)}";
                            await ReplyAsync("", false, embed.Build());
                        }
                    }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
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
                    await ReplyAsync("", false, embed.Build());
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
                        await ReplyAsync($"{Context.User.Mention} that does not seem to be an integer.");
                        return;
                    }
                    if (level_ < 0 || level_ > 99)
                    {
                        await ReplyAsync($"{Context.User.Mention} you can only do a level between 0 and 100");
                        return;
                    }
                    else
                    {
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                        };

                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
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
                                    exp = Math.Round(Math.Pow(1.25, level_) * 30).ToString("N0");
                                }
                                embed.WithFooter(footer);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"EXP - {lvl} to {level_ + 1}"
                                };
                                embed.WithAuthor(author);
                                embed.WithCurrentTimestamp();
                                embed.Description = $"**From rank {lvl} to rank {level_ + 1} you need {exp} EXP.**";
                                await ReplyAsync("", false, embed.Build());
                            }
                            else
                            {
                                if (!int.TryParse(lvl2, out int level_2))
                                {
                                    await ReplyAsync($"{Context.User.Mention} that does not seem to be an integer.");
                                    return;
                                }
                                else if (level_2 < 0 || level_2 > 100)
                                {
                                    await ReplyAsync($"{Context.User.Mention} you can only do a level between 0 and 100");
                                    return;
                                }
                                if (level_ > level_2)
                                {
                                    await ReplyAsync($"{Context.User.Mention} your EXP can't go down.");
                                    return;
                                }
                                if (level_ == level_2)
                                {
                                    await ReplyAsync($"{Context.User.Mention} that would just be 0.");
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
                                        exp = exp + Math.Round(Math.Pow(1.25, i) * 30);
                                    }
                                }
                                embed.WithFooter(footer);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"EXP - {level_} to {level_2}"
                                };
                                embed.WithAuthor(author);
                                embed.WithCurrentTimestamp();
                                embed.Description = $"**From rank {level_} to rank {level_2} you need {exp.ToString("N0")} EXP.**";
                                await ReplyAsync("", false, embed.Build());
                            }
                        }
                        catch (ArgumentException)
                        {
                            await ReplyAsync($"{Context.User.Mention} length of number is too long. (Don't use an excessive amount of 0's).");
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
        [Alias("joinrole", "leaverole")]
        [Summary("Adds HH, Arti, Trapper, Glitcher, Fruster, Racer or PR2 role.")]
        public async Task Role([Remainder] string roleName = null)
        {
            SocketGuildUser user;
            SocketGuild guild;
            if (Context.Channel is IDMChannel)
            {
                guild = Context.Client.GetGuild(528679522707701760);
                user = guild.GetUser(Context.User.Id);
            }
            else if (Extensions.AllowedChannels().Contains(Context.Channel.Id))
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
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(Context.Message, Context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, roleName);
                    string joinableRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"));
                    if (joinableRoles.Contains(role.Id.ToString()))
                    {
                        if (user.Roles.Any(e => e.Name.ToUpperInvariant() == roleName.ToUpperInvariant()))
                        {
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            };
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = "Removing Joinable Role"
                            };
                            await user.RemoveRoleAsync(role, options);

                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = Context.User.GetAvatarUrl(),
                                Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"{role.Name} Remove"
                            };
                            embed.WithAuthor(author);
                            embed.WithCurrentTimestamp();
                            if (Context.Channel is IDMChannel)
                            {
                                embed.Description = $"**You have been removed from the {Format.Sanitize(role.Name)} role.**";
                            }
                            else
                            {
                                embed.Description = $"**{Context.User.Mention} has been removed from the {Format.Sanitize(role.Name)} role({role.Mention}).**";
                            }
                            await ReplyAsync("", false, embed.Build());
                        }
                        else
                        {
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            };
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = "Adding Joinable Role"
                            };
                            await user.AddRoleAsync(role, options);

                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = Context.User.GetAvatarUrl(),
                                Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"{role.Name} Add"
                            };
                            embed.WithAuthor(author);
                            embed.WithCurrentTimestamp();
                            if (Context.Channel is IDMChannel)
                            {
                                embed.Description = $"**You have been added to the {Format.Sanitize(role.Name)} role.**";
                            }
                            else
                            {
                                embed.Description = $"**{Context.User.Mention} has been added to the {Format.Sanitize(role.Name)} role({role.Mention}).**";
                            }
                            await ReplyAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is not a joinable role.");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} the role with name or ID **{Format.Sanitize(roleName)}** does not exist or could not be found.");
                }
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
                guild = Context.Client.GetGuild(528679522707701760);
            }
            else if (Extensions.AllowedChannels().Contains(Context.Channel.Id))
            {
                guild = Context.Guild as SocketGuild;
            }
            else
            {
                return;
            }
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = guild.IconUrl,
                Name = "List Joinable Roles"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                Author = auth
            };
            string[] joinableRoles = File.ReadAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt")).Split("\n", StringSplitOptions.RemoveEmptyEntries);
            string joinableRolesS = "";
            foreach (string role in joinableRoles)
            {
                string currentRole = Context.Guild.GetRole(Convert.ToUInt64(role)).Name;
                joinableRolesS = joinableRolesS + currentRole + "\n";
            }
            if (joinableRolesS.Length <= 0)
            {
                await ReplyAsync($"{Context.User.Mention} there are no joinable roles.");
            }
            else
            {
                embed.AddField(y =>
                {
                    y.Name = "Joinable Roles";
                    y.Value = joinableRolesS;
                    y.IsInline = false;
                });
                await ReplyAsync("", false, embed.Build());
            }
        }

        [Command("topguilds", RunMode = RunMode.Async)]
        [Alias("guildstop", "topg", "gtop")]
        [Summary("Returns current top 10 guild on pr2.")]
        public async Task TopGuilds()
        {
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/guilds_top.php");
                }
                catch (HttpRequestException)
                {
                    await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    string[] guildlist = text.Split('}');
                    string guild1name = Extensions.GetBetween(guildlist[0], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild1gp = int.Parse(Extensions.GetBetween(guildlist[0], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild2name = Extensions.GetBetween(guildlist[1], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild2gp = int.Parse(Extensions.GetBetween(guildlist[1], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild3name = Extensions.GetBetween(guildlist[2], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild3gp = int.Parse(Extensions.GetBetween(guildlist[2], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild4name = Extensions.GetBetween(guildlist[3], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild4gp = int.Parse(Extensions.GetBetween(guildlist[3], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild5name = Extensions.GetBetween(guildlist[4], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild5gp = int.Parse(Extensions.GetBetween(guildlist[4], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild6name = Extensions.GetBetween(guildlist[5], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild6gp = int.Parse(Extensions.GetBetween(guildlist[5], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild7name = Extensions.GetBetween(guildlist[6], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild7gp = int.Parse(Extensions.GetBetween(guildlist[6], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild8name = Extensions.GetBetween(guildlist[7], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild8gp = int.Parse(Extensions.GetBetween(guildlist[7], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild9name = Extensions.GetBetween(guildlist[8], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild9gp = int.Parse(Extensions.GetBetween(guildlist[8], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    string guild10name = Extensions.GetBetween(guildlist[9], "\",\"guild_name\":\"", "\",\"gp_today\":\"");
                    string guild10gp = int.Parse(Extensions.GetBetween(guildlist[9], "\",\"gp_today\":\"", "\",\"gp_total\":\"")).ToString("N0");
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithColor(new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)));
                    embed.AddField(y =>
                    {
                        y.Name = "Guild";
                        y.Value = $"[{Format.Sanitize(Uri.UnescapeDataString(guild1name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild1name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild2name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild2name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild3name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild3name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild4name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild4name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild5name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild5name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild6name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild6name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild7name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild7name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild8name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild8name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild9name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild9name)})\n" +
                                  $"[{Format.Sanitize(Uri.UnescapeDataString(guild10name))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guild10name)})\n";
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
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = $"PR2 Top 10 Guilds",
                        Url = "https://pr2hub.com/guilds_top.php"
                    };
                    embed.WithAuthor(author);
                    embed.WithCurrentTimestamp();
                    await ReplyAsync("", false, embed.Build());
                }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (fahuser == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /fah";
                    embed.Description = "**Description:** Get F@H info for a user.\n**Usage:** /fah [PR2 name]\n**Example:** /fah Jiggmin";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    if (Context.Message.MentionedUsers.Count > 0)
                    {
                        SocketUser user = null;
                        int argPos = 0;
                        if (Context.Message.HasStringPrefix("/", ref argPos))
                        {
                            user = Context.Message.MentionedUsers.First();
                        }
                        else if (Context.Message.HasMentionPrefix(Context.Client.CurrentUser, ref argPos) && Context.Message.MentionedUsers.Count > 1)
                        {
                            user = Context.Message.MentionedUsers.ElementAt(1);
                        }
                        if (user == null)
                        {
                            await ReplyAsync($"{Context.User.Mention} that user does not exist or could not be found.");
                            return;
                        }
                        List<string> result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        fahuser = Database.GetPR2Name(user);
                        if (fahuser.Equals("Not verified"))
                        {
                            await ReplyAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
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
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                        Author = author
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    string text;
                    try
                    {
                        text = await web.GetStringAsync("https://stats.foldingathome.org/api/donor/" + fahuser.Replace(' ', '_'));
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} the user **{Format.Sanitize(fahuser)}** does not exist or could not be found.");
                        return;
                    }
                    try
                    {
                        JToken o = JObject.Parse(text).GetValue("teams");
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
                        if (stats.Count == 0)
                        {
                            await ReplyAsync($"{Context.User.Mention} the user **{Format.Sanitize(fahuser)}** does not exist or could not be found.");
                            return;
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
                        if (stats.GetValue("last") != null)
                        {
                            embed.AddField(y =>
                            {
                                y.Name = "Last WU";
                                y.Value = $"{stats.GetValue("last")}";
                                y.IsInline = true;
                            });
                        }
                        else
                        {
                            embed.AddField(y =>
                            {
                                y.Name = "Last WU";
                                y.Value = $"N/A";
                                y.IsInline = true;
                            });
                        }
                        await ReplyAsync("", false, embed.Build());
                    }
                    catch (JsonReaderException)
                    {
                        await ReplyAsync($"{Context.User.Mention} the F@H Api is currently down.");
                    }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (id == null || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /bans";
                    embed.Description = "**Description:** Get PR2 ban info.\n**Usage:** /bans [PR2 ban ID]\n**Example:** /bans 59098";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    string text = null;
                    try
                    {
                        text = await web.GetStringAsync("https://pr2hub.com/bans/show_record.php?ban_id=" + id);
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (text != null)
                    {
                        if (Extensions.GetBetween(text, "<title>", "</title>").Contains("PR2 Hub - Error Fetching Ban"))
                        {
                            await ReplyAsync($"{Context.User.Mention} the ban with the ID **{id}** does not exist or could not be found.");
                            return;
                        }
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = $"Ban ID - {id}",
                            Url = "https://pr2hub.com/bans/show_record.php?ban_id=" + id
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Author = author,
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        if (Extensions.GetBetween(text, "---------------------------------------------</p><p>--- ", " ---</p><p>--- Reason: ").Contains("This ban has been lifted by "))
                        {
                            string liftedMod = Extensions.GetBetween(text, "</p><p>--- This ban has been lifted by ", " ---</p><p>--- Reason: ");
                            string liftedReason = Extensions.GetBetween(text, $"{liftedMod} ---</p><p>--- Reason: ", " ---</p><p>-----");
                            string mod = Extensions.GetBetween(text, "</p><p>&nbsp;</p></b><p>", " banned ");
                            string user = Extensions.GetBetween(text, $"{mod} banned ", " for ");
                            string length = Extensions.GetBetween(text, $"{user} for ", " on ");
                            string date = Extensions.GetBetween(text, $"{length} on ", ".</p>");
                            string reason = Extensions.GetBetween(text, "<p>Reason: ", "</p>");
                            string expires = Extensions.GetBetween(text, "<p>This ban will expire on ", ".</p>");
                            liftedMod = $"[{Format.Sanitize(Uri.UnescapeDataString(liftedMod))}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(liftedMod)})";
                            mod = $"[{Format.Sanitize(Uri.UnescapeDataString(mod))}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(mod)})";
                            if (!user.Equals("<i>an IP</i>"))
                            {
                                user = $"[{Format.Sanitize(Uri.UnescapeDataString(user))}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(user)})";
                            }
                            else
                            {
                                user = "an IP";
                            }
                            if (reason.Length <= 0)
                            {
                                reason = "No reason was provided.";
                            }
                            embed.AddField(y =>
                            {
                                y.Name = "Lifted Ban";
                                y.Value = $"{Format.Sanitize("This ban has been lifted by " + liftedMod)}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = $"{Format.Sanitize(liftedReason)}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Ban Info";
                                y.Value = Format.Sanitize($"{mod} banned {user} for {length} on {date}.");
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = $"{Format.Sanitize(reason)}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Expires";
                                y.Value = $"{expires}";
                                y.IsInline = true;
                            });
                            await ReplyAsync("", false, embed.Build());
                        }
                        else
                        {
                            string mod = Extensions.GetBetween(text, "<div class='content'><p>", " banned ");
                            string user = Extensions.GetBetween(text, $"{mod} banned ", " for ");
                            string length = Extensions.GetBetween(text, $"{user} for ", " on ");
                            string date = Extensions.GetBetween(text, $"{length} on ", ".</p>");
                            string reason = Extensions.GetBetween(text, "<p>Reason: ", "</p>");
                            string expires = Extensions.GetBetween(text, "<p>This ban will expire on ", ".</p>");
                            mod = $"[{Format.Sanitize(Uri.UnescapeDataString(mod))}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(mod)})";
                            if (!user.Equals("<i>an IP</i>"))
                            {
                                user = $"[{Format.Sanitize(Uri.UnescapeDataString(user))}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(user)})";
                            }
                            else
                            {
                                user = "an IP";
                            }
                            embed.AddField(y =>
                            {
                                y.Name = "Ban Info";
                                y.Value = Format.Sanitize($"{mod} banned {user} for {length} on {date}.");
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = $"{Format.Sanitize(reason)}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Expires";
                                y.Value = $"{expires}";
                                y.IsInline = true;
                            });
                            await ReplyAsync("", false, embed.Build());
                        }
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
        public async Task Pop([Remainder] string s = null)
        {
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (s != null)
                {
                    await Stats(s);
                    return;
                }
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/files/server_status_2.txt");
                }
                catch (HttpRequestException)
                {
                    await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    string[] pops = text.Split('}');
                    int pop = 0;
                    foreach (string server in pops)
                    {
                        if (server.Length > 5)
                        {
                            int population = Convert.ToInt32(Extensions.GetBetween(server, "population\":\"", "\","));
                            pop += population;
                        }
                    }
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = "PR2 Total Online Users",
                        Url = "https://pr2hub.com/server_status.php"
                    };
                    embed.WithFooter(footer);
                    embed.WithAuthor(author);
                    embed.WithCurrentTimestamp();
                    embed.Description = $"The total number of users on PR2 currently is {pop}";
                    await ReplyAsync("", false, embed.Build());
                }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(server))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /stats";
                    embed.Description = "**Description:** Get a PR2 server info.\n**Usage:** /stats [PR2 server name]\n**Example:** /stats Derron";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    if (Context.Message.MentionedUsers.Count > 0)
                    {
                        SocketUser user = null;
                        int argPos = 0;
                        if (Context.Message.HasStringPrefix("/", ref argPos))
                        {
                            user = Context.Message.MentionedUsers.First();
                        }
                        else if (Context.Message.HasMentionPrefix(Context.Client.CurrentUser, ref argPos) && Context.Message.MentionedUsers.Count > 1)
                        {
                            user = Context.Message.MentionedUsers.ElementAt(1);
                        }
                        if (user == null)
                        {
                            await ReplyAsync($"{Context.User.Mention} that user does not exist or could not be found.");
                            return;
                        }
                        List<string> result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        string pr2name = Database.GetPR2Name(user);
                        if (pr2name.Equals("Not verified"))
                        {
                            await ReplyAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                        string pr2userinfo = null;
                        try
                        {
                            pr2userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
                        }
                        catch (HttpRequestException)
                        {
                            await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                            return;
                        }
                        string status = Extensions.GetBetween(pr2userinfo, ",\"status\":\"", "\",\"loginDate\":\"");
                        if (status.Equals("offline"))
                        {
                            await ReplyAsync($"{Context.User.Mention} that users account is offline.");
                            return;
                        }
                        status = status.Substring(11);
                        server = status;
                    }
                    string text = null;
                    try
                    {
                        text = await web.GetStringAsync("https://pr2hub.com/files/server_status_2.txt");
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (text != null)
                    {
                        if (text.ToLower().Contains(server.ToLower()))
                        {
                            string serverInfo = Extensions.GetBetween(text.ToLower(), server.ToLower(), "}");
                            string pop = Extensions.GetBetween(serverInfo, "\",\"population\":\"", "\",\"status\":\"");
                            string status = Extensions.GetBetween(serverInfo, "\",\"status\":\"", "\",\"guild_id\":\"");
                            int tournament = Convert.ToInt32(Extensions.GetBetween(serverInfo, "\",\"tournament\":\"", "\",\"happy_hour\":\""));
                            int happyHour = Convert.ToInt32(Extensions.GetBetween(serverInfo, "\",\"happy_hour\":\"", "\""));
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
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = Context.User.GetAvatarUrl(),
                                Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                            };
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"Server Stats - {server}",
                                Url = "https://pr2hub.com/server_status.php"
                            };
                            embed.WithFooter(footer);
                            embed.WithAuthor(author);
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
                            await ReplyAsync("", false, embed.Build());
                        }
                        else
                        {
                            await ReplyAsync($"{Context.User.Mention} the server **{Format.Sanitize(server)}** does not exist or could not be found.");
                        }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (guildname == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /guildmembers";
                    embed.Description = "**Description:** Get a members for a PR2 guild by name.\n**Usage:** /guildmembers [PR2 guild name]\n**Example:** /guildmembers PR2 Staff";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    if (Context.Message.MentionedUsers.Count > 0)
                    {
                        SocketUser user = null;
                        int argPos = 0;
                        if (Context.Message.HasStringPrefix("/", ref argPos))
                        {
                            user = Context.Message.MentionedUsers.First();
                        }
                        else if (Context.Message.HasMentionPrefix(Context.Client.CurrentUser, ref argPos) && Context.Message.MentionedUsers.Count > 1)
                        {
                            user = Context.Message.MentionedUsers.ElementAt(1);
                        }
                        if (user == null)
                        {
                            await ReplyAsync($"{Context.User.Mention} that user does not exist or could not be found.");
                            return;
                        }
                        List<string> result = Database.CheckExistingUser(user);
                        if (result.Count() <= 0)
                        {
                            Database.EnterUser(user);
                        }
                        string pr2name = Database.GetPR2Name(user);
                        if (guildname.Equals("Not verified"))
                        {
                            await ReplyAsync($"{Context.User.Mention} that user has not linked their PR2 account.");
                            return;
                        }
                        string pr2userinfo = null;
                        try
                        {
                            pr2userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
                        }
                        catch (HttpRequestException)
                        {
                            await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                            return;
                        }
                        string guild = Extensions.GetBetween(pr2userinfo, "\",\"guildName\":\"", "\",\"name\":\"");
                        if (guild.Length <= 0)
                        {
                            await ReplyAsync($"{Context.User.Mention} that users account is not a member of a guild.");
                            return;
                        }
                        guildname = guild;
                    }
                    string text = null;
                    try
                    {
                        text = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname);
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (text != null)
                    {
                        if (text.Contains(value: "{\"error\":\""))
                        {
                            await ReplyAsync($"{Context.User.Mention} the guild **{Format.Sanitize(guildname)}** does not exist or could not be found.");
                            return;
                        }
                        string[] users = Extensions.GetBetween(text, "[", "]").Split('}');
                        List<string> guildMembers = new List<string>();
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Url = "https://pr2hub.com/guild_search.php?name=" + Uri.EscapeDataString(guildname),
                            Name = "Guild Members - " + guildname
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Author = author,
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                        };
                        foreach (string user_id in users)
                        {
                            string name = Extensions.GetBetween(user_id, "name\":\"", "\",\"power");
                            guildMembers.Add($"[{Format.Sanitize(name)}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(name)})");
                        }
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        embed.Description = $"{string.Join(", ", guildMembers)}";
                        await ReplyAsync("", false, embed.Build());
                    }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (id == null || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /guildmembersid";
                    embed.Description = "**Description:** Get a members for a PR2 guild by ID.\n**Usage:** /guildmembersid [PR2 guild ID]\n**Example:** /guildmembersid 183";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    string text = null;
                    try
                    {
                        text = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&id=" + id);
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (text != null)
                    {
                        if (text.Contains(value: "{\"error\":\""))
                        {
                            await ReplyAsync($"{Context.User.Mention} the guild with ID **{id}** does not exist or could not be found.");
                            return;
                        }
                        string gName = Extensions.GetBetween(text, "\"guild_name\":\"", "\",\"");
                        string[] users = Extensions.GetBetween(text, "[", "]").Split('}');
                        List<string> guildMembers = new List<string>();
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Url = "https://pr2hub.com/guild_search.php?id=" + id,
                            Name = "Guild Members - " + gName
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Author = author,
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                        };
                        foreach (string user_id in users)
                        {
                            string name = Extensions.GetBetween(user_id, "name\":\"", "\",\"power");
                            guildMembers.Add($"[{Format.Sanitize(name)}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(name)})");
                        }
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        embed.Description = $"{string.Join(", ", guildMembers)}";
                        await ReplyAsync("", false, embed.Build());
                    }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                HttpClient web = new HttpClient();
                string hhServers = "", happyHour = "";
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/files/server_status_2.txt");
                }
                catch (HttpRequestException)
                {
                    await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    string[] servers = text.Split('}');
                    foreach (string server_name in servers)
                    {
                        happyHour = Extensions.GetBetween(server_name, "hour\":\"", "\"");
                        if (happyHour.Equals("1"))
                        {
                            string serverName = Extensions.GetBetween(server_name, "server_name\":\"", "\"");
                            hhServers = hhServers + serverName + ", ";
                        }
                    }
                    if (hhServers.Length < 1)
                    {
                        await ReplyAsync($"{Context.User.Mention} no servers currently have happy hour on them.");
                        return;
                    }
                    hhServers = hhServers.TrimEnd(new char[] { ' ', ',', ',' });
                    int count = hhServers.Split(',').Length - 1;
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Url = "https://pr2hub.com/server_status.php"
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                        Author = author
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    if (count == 0)
                    {
                        author.Name = $"Happy Hour Server";
                        embed.Description = $"This server currently has a happy hour on it: {Format.Sanitize(hhServers.TrimEnd(new char[] { ' ', ',' }))}";
                    }
                    else
                    {
                        author.Name = $"Happy Hour Servers";
                        embed.Description = $"These are the current servers with happy hour on them: {Format.Sanitize(hhServers)}";
                    }
                    await ReplyAsync("", false, embed.Build());
                }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(level))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /level";
                    embed.Description = "**Description:** Get PR2 level info.\n**Usage:** /level [PR2 level name]\n**Example:** /level Newbieland 2";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    string pr2token = File.ReadAllText(Path.Combine(Extensions.downloadPath, "PR2Token.txt"));
                    Dictionary<string, string> values = new Dictionary<string, string>
                    {
                        { "dir", "desc" },
                        { "mode", "title" },
                        { "order", "popularity" },
                        { "page", "1" },
                        { "search_str", level },
                        { "token", pr2token }
                    };
                    HttpClient web = new HttpClient();
                    FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                    HttpResponseMessage response = null;
                    try
                    {
                        response = await web.PostAsync("https://pr2hub.com/search_levels.php?", content);
                    }
                    catch (HttpRequestException)
                    {
                        await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (response != null)
                    {
                        string responseString = await response.Content.ReadAsStringAsync();
                        string version = Extensions.GetBetween(responseString, "&version0=", "&title0=");
                        string title = Uri.UnescapeDataString(Extensions.GetBetween(responseString, "&title0=", "&rating0=")).Replace("+", " ");
                        string rating = Extensions.GetBetween(responseString, "&rating0=", "&playCount0=");
                        string plays = int.Parse(Extensions.GetBetween(responseString, "&playCount0=", "&minLevel0=")).ToString("N0");
                        string minLevel = Extensions.GetBetween(responseString, "&minLevel0=", "&note0=");
                        string note = Uri.UnescapeDataString(Extensions.GetBetween(responseString, "&note0=", "&userName0=")).Replace("+", " ");
                        string user = Uri.UnescapeDataString(Extensions.GetBetween(responseString, "&userName0=", "&group0=")).Replace("+", " ");
                        if (title.Length <= 0)
                        {
                            await ReplyAsync($"{Context.User.Mention} the level **{Format.Sanitize(level)}** does not exist or could not be found.");
                            return;
                        }
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = $"-- {title} --",
                            Url = "https://pr2hub.com/levels/" + Extensions.GetBetween(responseString, "levelID0=", "&version0=") + ".txt?version=" + version
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            Author = author
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = Context.User.GetAvatarUrl(),
                            Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        embed.Description = $"**By:** {Format.Sanitize(user)}\n**Version:** {version}\n**Min Rank:** {minLevel}\n**Plays:** {plays}\n**Rating:** {rating}\n";
                        if (note.Length > 0)
                        {
                            embed.Description = embed.Description + $"-----\n{Format.Sanitize(note)}";
                        }
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }
        }

        //[Command("verifyguild", RunMode = RunMode.Async)]
        //[Alias("addguild")]
        //[Summary("Creates a channel for the users guild.")]
        //public async Task VerifyGuild()
        //{
        //    if (Extensions.AllowedChannels().Contains(Context.Channel.Id))
        //    {
        //        List<string> result = Database.CheckExistingUser(Context.User);
        //        if (result.Count() <= 0)
        //        {
        //            Database.EnterUser(Context.User);
        //        }
        //        string pr2name = Database.GetPR2Name(Context.User);
        //        if (pr2name.Equals("Not verified"))
        //        {
        //            await ReplyAsync($"{Context.User.Mention} you have not linked your PR2 account.");
        //            return;
        //        }
        //        HttpClient web = new HttpClient();
        //        string userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
        //        string guild = Extensions.GetBetween(userinfo, "\",\"guildName\":\"", "\",\"name\":\"");
        //        string id = Extensions.GetBetween(userinfo, "\",\"userId\":\"", "\",\"hatColor2\":");
        //        string guildinfo = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guild);
        //        string owner = Extensions.GetBetween(guildinfo, "\",\"owner_id\":\"", "\",\"note\":\"");
        //        if (id.Equals(owner))
        //        {
        //            int memberCount = int.Parse(Extensions.GetBetween(guildinfo, "\",\"member_count\":\"", "\",\"emblem\":\""));
        //            if (memberCount < 15)
        //            {
        //                await ReplyAsync($"{Context.User.Mention} you need at least 15 members in your guild.");
        //                return;
        //            }
        //            IReadOnlyCollection<SocketRole> roles = Context.Guild.Roles;
        //            bool exists = false;
        //            foreach (IRole role in roles)
        //            {
        //                if (role.Name == guild)
        //                {
        //                    exists = true;
        //                    break;
        //                }
        //            }
        //            if (!exists)
        //            {
        //                Discord.Rest.RestRole guildRole = await Context.Guild.CreateRoleAsync(guild, null, null, false);
        //                SocketRole everyoneRole = Context.Guild.EveryoneRole;
        //                SocketRole mutedRole = Context.Guild.Roles.Where(x => x.Name.ToUpper() == "Muted".ToUpper()).First();
        //                Discord.Rest.RestTextChannel guildChannel = await Context.Guild.CreateTextChannelAsync(guild.Replace(" ", "-"));
        //                await guildChannel.ModifyAsync(x => x.CategoryId = 528704611289399296);
        //                await guildChannel.AddPermissionOverwriteAsync(guildRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Inherit));
        //                await guildChannel.AddPermissionOverwriteAsync(mutedRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Deny));
        //                await guildChannel.AddPermissionOverwriteAsync(everyoneRole, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny));
        //                Discord.Rest.RestVoiceChannel guildVoice = await Context.Guild.CreateVoiceChannelAsync(guild);
        //                await guildVoice.ModifyAsync(x => x.CategoryId = 528704611289399296);
        //                await guildVoice.AddPermissionOverwriteAsync(guildRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Allow));
        //                await guildVoice.AddPermissionOverwriteAsync(mutedRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Deny, speak: PermValue.Deny));
        //                await guildVoice.AddPermissionOverwriteAsync(everyoneRole, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny));
        //                IGuildUser user = Context.Guild.GetUser(Context.User.Id);
        //                RequestOptions options = new RequestOptions()
        //                {
        //                    AuditLogReason = "Adding Guild Role"
        //                };
        //                await user.AddRoleAsync(guildRole, options);
        //            }
        //            else
        //            {
        //                await ReplyAsync($"{Context.User.Mention} your guild already has a guild channel.");
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            await ReplyAsync($"{Context.User.Mention} you are not the owner of this guild.");
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}

        //[Command("joinguild", RunMode = RunMode.Async)]
        //[Alias("jguild", "guildjoin", "guildj")]
        //[Summary("Adds user to guild role of their guild if it exists.")]
        //public async Task JoinGuild()
        //{
        //    if (Extensions.AllowedChannels().Contains(Context.Channel.Id))
        //    {
        //        List<string> result = Database.CheckExistingUser(Context.User);
        //        if (result.Count() <= 0)
        //        {
        //            Database.EnterUser(Context.User);
        //        }
        //        string pr2name = Database.GetPR2Name(Context.User);
        //        if (pr2name.Equals("Not verified"))
        //        {
        //            await ReplyAsync($"{Context.User.Mention} you have not linked your PR2 account.");
        //            return;
        //        }
        //        HttpClient web = new HttpClient();
        //        string userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info_2.php?name=" + pr2name);
        //        string guild = Extensions.GetBetween(userinfo, "\",\"guildName\":\"", "\",\"name\":\"");
        //        IReadOnlyCollection<SocketRole> roles = Context.Guild.Roles;
        //        foreach (IRole role in roles)
        //        {
        //            if (role.Name == guild)
        //            {
        //                SocketGuildUser user = Context.User as SocketGuildUser;
        //                RequestOptions options = new RequestOptions()
        //                {
        //                    AuditLogReason = "Adding Guild Role"
        //                };
        //                await user.AddRoleAsync(role, options);
        //                await ReplyAsync($"{Context.User.Mention} you have been added to the guild role **{Format.Sanitize(guild)}**.");
        //                break;
        //            }
        //        }
        //        await ReplyAsync($"{Context.User.Mention} your guild does not have a guild channel.");
        //    }
        //    else
        //    {
        //        return;
        //    }
        //}

        [Command("servers", RunMode = RunMode.Async)]
        [Alias("sl", "status", "serverstatus", "ss")]
        [Summary("Shows all servers and population")]
        public async Task Servers([Remainder] string s1 = null)
        {
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                if (s1 != null)
                {
                    await Stats(s1);
                    return;
                }
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/files/server_status_2.txt");
                }
                catch (HttpRequestException)
                {
                    await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    string[] serversinfo = text.Split('}');
                    string pop = "", name = "", status = "", happyHour = "";
                    int serverId = 0;
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Url = "https://pr2hub.com/server_status.php",
                        Name = "Server Status"
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                        Author = author
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.Description = "";
                    foreach (string server_id in serversinfo)
                    {
                        name = Extensions.GetBetween(server_id, "\",\"server_name\":\"", "\",\"address\":\"");
                        if (name.Length <= 0)
                        {
                            break;
                        }
                        pop = Extensions.GetBetween(server_id, "\",\"population\":\"", "\",\"status\":\"");
                        status = Extensions.GetBetween(server_id, "\",\"status\":\"", "\",\"guild_id\":\"");
                        happyHour = Extensions.GetBetween(server_id, "\",\"happy_hour\":\"", "\"");
                        serverId = int.Parse(Extensions.GetBetween(server_id, "\"server_id\":\"", "\",\"server_name\":\""));
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
                            embed.Description = embed.Description + "* !! " + Format.Sanitize(name) + " (" + pop + " online)\n";
                        }
                        else if (status.Equals("down", StringComparison.InvariantCultureIgnoreCase) && serverId > 10)
                        {
                            embed.Description = embed.Description + "* " + Format.Sanitize(name) + " (down)\n";
                        }
                        else if (serverId > 10)
                        {
                            embed.Description = embed.Description + "* " + Format.Sanitize(name) + " (" + pop + " online)\n";
                        }
                        else
                        {
                            embed.Description = embed.Description + name + " (" + pop + " online)\n";
                        }
                    }
                    await ReplyAsync("", false, embed.Build());
                }
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
            if (Extensions.AllowedChannels().Contains(Context.Channel.Id) || Context.Channel is IDMChannel || Context.Guild.Id != 528679522707701760)
            {
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/staff.php");
                }
                catch (HttpRequestException)
                {
                    await ReplyAsync($"{Context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    string[] staff = text.Split("player_search");
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = "Staff Online",
                        Url = "https://pr2hub.com/staff.php"
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Author = author,
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.Description = "";
                    foreach (string name in staff)
                    {
                        string status = Extensions.GetBetween(name, "</a></td><td>", "</td><td>");
                        if (status.Contains("Playing on"))
                        {
                            status = Extensions.GetBetween(name, "</a></td><td>Playing on ", "</td><td>");
                            string pr2Name = Extensions.GetBetween(name, "; text-decoration: underline;'>", "</a></td><td>").Replace("&nbsp;", " ");
                            embed.Description = embed.Description + "[" + Format.Sanitize(pr2Name) + "](https://pr2hub.com/player_search.php?name=" + Uri.EscapeDataString(pr2Name) + ")(" + status + "), ";
                        }
                    };
                    if (embed.Description.Length <= 0)
                    {
                        await ReplyAsync($"{Context.User.Mention} there is no PR2 Staff online currently.");
                        return;
                    }
                    else
                    {
                        embed.Description = embed.Description.TrimEnd(new char[] { ' ', ',', ',' });
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }
            else
            {
                return;
            }
        }

        #endregion
    }
}