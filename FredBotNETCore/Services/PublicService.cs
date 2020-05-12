using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Database;
using FredBotNETCore.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static FredBotNETCore.WeatherDataCurrent;

namespace FredBotNETCore.Services
{
    public class PublicService
    {
        public PublicService()
        {

        }

        public async Task PayAsync(SocketCommandContext context, string amount, [Remainder] string username)
        {
            if (!User.Exists(context.User))
            {
                User.Add(context.User);
            }
            if (!User.IsVerified(context.User))
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify yourself to use this command.");
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
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (User.GetUser("user_id", context.User.Id.ToString()).Balance > money)
                    {
                        if (Extensions.UserInGuild(context.Message, context.Client.GetGuild(528679522707701760), username) != null)
                        {
                            SocketUser user = Extensions.UserInGuild(context.Message, context.Client.GetGuild(528679522707701760), username);
                            User.SetValue(context.User, "balance", (User.GetUser("user_id", context.User.Id.ToString()).Balance - money).ToString());
                            User.SetValue(user, "balance", (User.GetUser("user_id", user.Id.ToString()).Balance + money).ToString());
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully paid **{Format.Sanitize(user.Username)}#{user.Discriminator} ${money.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}**.\nYour new balance is ${(User.GetUser("user_id", context.User.Id.ToString()).Balance - money).ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}.");
                            try
                            {
                                await user.SendMessageAsync($"{user.Mention} you have been paid **${money}** by **{Format.Sanitize(context.User.Username)}#{context.User.Discriminator}**");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not DM **{Format.Sanitize(user.Username)}#{user.Discriminator}** to notify them of your payment.");
                            }
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the user with name or ID **{Format.Sanitize(username)}** does not exist or could not be found.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you do not have enough money to do that.");
                    }
                }
            }
        }

        public async Task BalanceAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (!User.Exists(context.User))
            {
                User.Add(context.User);
            }
            if (!User.IsVerified(context.User))
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    int bal = User.GetUser("user_id", context.User.Id.ToString()).Balance;
                    await context.Channel.SendMessageAsync($"{context.User.Mention} your balance is **${bal.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}**.");
                }
                else
                {
                    if (Extensions.UserInGuild(context.Message, context.Client.GetGuild(528679522707701760), username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(context.Message, context.Client.GetGuild(528679522707701760), username);
                        if (User.Exists(user))
                        {
                            User.Add(user);
                        }
                        int bal = User.GetUser("user_id", user.Id.ToString()).Balance;
                        await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}'s** balance is **${bal.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}**.");
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the user with name or ID **{Format.Sanitize(username)}** does not exist or could not be found.");
                    }
                }
            }
        }

        public async Task JackpotAsync(SocketCommandContext context)
        {
            if (!User.Exists(context.User))
            {
                User.Add(context.User);
            }
            if (!User.IsVerified(context.User))
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                int lottobal = int.Parse(File.ReadAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt")));
                await context.Channel.SendMessageAsync($"{context.User.Mention} the jackpot is currently worth **${lottobal.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}**.");
            }
        }

        public async Task LeaderboardAsync(SocketCommandContext context)
        {
            if (!User.Exists(context.User))
            {
                User.Add(context.User);
            }
            if (!User.IsVerified(context.User))
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = "Leaderboard",
                    IconUrl = context.Client.GetGuild(528679522707701760).IconUrl
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    Author = auth,
                };
                List<User> topUsers = User.GetTop();
                string leaderboard = null;
                int i = 0;
                foreach (User user in topUsers)
                {
                    try
                    {
                        leaderboard = leaderboard + "**" + (i + 1).ToString() + ".** " + Format.Sanitize(context.Client.GetUser(ulong.Parse(user.UserID.ToString())).Username) + "#" + context.Client.GetUser(ulong.Parse(user.UserID.ToString())).Discriminator + " - $" + user.Balance.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB")) + "\n";
                        i++;
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
                embed.Description = leaderboard;
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task LottoAsync(SocketCommandContext context, string ticketsS)
        {
            if (!User.Exists(context.User))
            {
                User.Add(context.User);
            }
            if (!User.IsVerified(context.User))
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify yourself to use this command.");
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
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    int balance = User.GetUser("user_id", context.User.Id.ToString()).Balance;
                    if (tickets > balance)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you don't have enough money to buy that many tickets.");
                    }
                    else
                    {
                        User.SetValue(context.User, "balance", (balance - tickets).ToString());
                        int lottobal = int.Parse(File.ReadAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt")));
                        int chance = Convert.ToInt32(tickets / (lottobal + 0.00) * 100);
                        EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                        {
                            Name = "Lottery",
                            IconUrl = context.User.GetAvatarUrl()
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            Author = auth,
                            Description = $"Jackpot: ${lottobal.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}\nScratching Tickets..."
                        };
                        IUserMessage message = await context.Channel.SendMessageAsync("", false, embed.Build());
                        await Task.Delay(500);
                        if (chance >= 100)
                        {
                            embed.Description = $"{Format.Sanitize(context.User.Username)}#{context.User.Discriminator} won the jackpot of ${lottobal.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}!";
                            await message.ModifyAsync(x => x.Embed = embed.Build());
                            User.SetValue(context.User, "balance", (balance + lottobal).ToString());
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt"), "100");
                        }
                        else
                        {
                            int random = Extensions.random.Next(100);
                            if (random <= chance)
                            {
                                embed.Description = $"{Format.Sanitize(context.User.Username)}#{context.User.Discriminator} won the jackpot of ${lottobal.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}!";
                                await message.ModifyAsync(x => x.Embed = embed.Build());
                                User.SetValue(context.User, "balance", (balance + lottobal).ToString());
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt"), "100");
                            }
                            else
                            {
                                embed.Description = $"{Format.Sanitize(context.User.Username)}#{context.User.Discriminator} did not win the jackpot of ${lottobal.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}.";
                                int newbal = lottobal + tickets;
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "LottoBalance.txt"), newbal.ToString());
                                await message.ModifyAsync(x => x.Embed = embed.Build());
                            }
                        }
                    }
                }
            }
        }

        public async Task DailyAsync(SocketCommandContext context)
        {
            if (!User.Exists(context.User))
            {
                User.Add(context.User);
            }
            if (!User.IsVerified(context.User))
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify yourself to use this command.");
            }
            else
            {
                string date = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year;
                if (User.GetUser("user_id", context.User.Id.ToString()).LastUsed.Equals(date))
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you have already used this command today.");
                }
                else
                {
                    int money = Extensions.random.Next(5, 50);
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you worked for ${money} today.");
                    User.SetValue(context.User, "balance", (User.GetUser("user_id", context.User.Id.ToString()).Balance + money).ToString());
                    User.SetValue(context.User, "last_used", date);
                }
            }
        }

        public async Task VerifyAsync(SocketCommandContext context)
        {
            IUserMessage msg = null;
            if (!(context.Channel is SocketDMChannel))
            {
                msg = await context.Channel.SendMessageAsync($"{context.User.Mention} check your DMs to verify your PR2 account.");
            }
            try
            {
                await context.User.SendMessageAsync($"Hello {context.User.Mention} , to verify your PR2 account please send a PM to `FredTheG.CactusBot` on PR2 " +
                    $"saying only `{(await context.User.GetOrCreateDMChannelAsync()).Id}`.\nThen once you have sent the PM type `/verifycomplete <PR2 account name>` without <> in **this channel**. PR2 account name = name of " +
                    $"account you sent the PM from.");
            }
            catch (Discord.Net.HttpException)
            {
                await msg.ModifyAsync(x => x.Content = $"{context.User.Mention} I was unable to send you instructions to verify your PR2 account. To fix this goto Settings --> Privacy & Safety --> And allow direct messages from server members.");
            }
        }

        public async Task VerifiedAsync(SocketCommandContext context, [Remainder] string username)
        {
            SocketGuild guild = context.Client.GetGuild(528679522707701760);
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = $"Command: /verifycomplete";
                embed.Description = "**Description:** Verify your PR2 account.\n**Usage:** /verifycomplete [PR2 username]\n**Example:** /verifycomplete Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                SocketGuildUser user = null;
                try
                {
                    user = guild.GetUser(context.User.Id);
                }
                catch
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you are not a memeber of **{Format.Sanitize(guild.Name)}**.");
                    return;
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
                while (responseString.Equals("{\"success\":false,\"error\":\"Slow down a bit, yo.\"}"))
                {
                    await Task.Delay(500);
                    response = await web.PostAsync("https://pr2hub.com/messages_get.php?", content);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                string[] pms = responseString.Split('}');
                int tries = 0;
                foreach (string message_id in pms)
                {
                    string name = Extensions.GetBetween(message_id, "name\":\"", "\",\"group");
                    if (name.ToLower().Equals(username.ToLower()))
                    {
                        string message = Extensions.GetBetween(message_id, "message\":\"", "\",\"time");
                        if (message.Equals(context.Channel.Id.ToString()))
                        {
                            if (!User.Exists(context.User))
                            {
                                User.Add(context.User);
                            }
                            if (int.Parse(Extensions.GetBetween(await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + name), "\"rank\":", ",\"hats\":")) < 15)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} your PR2 account must be at least rank 15 if you want to link it to your Discord account.");
                            }
                            else
                            {
                                bool isVerified = User.IsVerified(context.User);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "User Verify",
                                    IconUrl = guild.IconUrl
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    Text = $"ID: {context.User.Id}"
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
                                    string pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                                    if (pr2name.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        await context.Channel.SendMessageAsync($"{context.User.Mention} that is already your verified account.");
                                        web.Dispose();
                                        return;
                                    }
                                    long id = User.GetUser("pr2_name", username).UserID;
                                    if (id != 0)
                                    {
                                        await context.Channel.SendMessageAsync($"{context.User.Mention} the Discord user with ID: **{id}** has already verified themselves with the PR2 Account: **{Format.Sanitize(username)}**. Please contact an admin on the {Format.Sanitize(guild.Name)} Discord Server for futher assistance.");
                                        web.Dispose();
                                        return;
                                    }
                                    User.SetValue(context.User, "pr2_name", username);
                                    SocketTextChannel channel = Extensions.GetLogChannel(guild);
                                    embed.Description = $"{context.User.Mention} changed their verified account from **{Format.Sanitize(pr2name)}** to **{Format.Sanitize(username)}**.";
                                    await channel.SendMessageAsync("", false, embed.Build());
                                    if (!user.Username.Equals(username))
                                    {
                                        RequestOptions options = new RequestOptions()
                                        {
                                            AuditLogReason = "Setting nickname to PR2 name."
                                        };
                                        await user.ModifyAsync(x => x.Nickname = username, options);
                                    }
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully changed your verified account from **{Format.Sanitize(pr2name)}** to **{Format.Sanitize(username)}**.");
                                }
                                else
                                {
                                    long id = User.GetUser("pr2_name", username).UserID;
                                    if (id != 0)
                                    {
                                        await context.Channel.SendMessageAsync($"{context.User.Mention} the Discord user with ID: **{id}** has already verified themselves with the PR2 Account: **{Format.Sanitize(username)}**. Please contact an admin on the Platform Racing Group Discord Server for futher assistance.");
                                        web.Dispose();
                                        return;
                                    }
                                    User.SetValue(context.User, "pr2_name", username);
                                    SocketTextChannel channel = Extensions.GetLogChannel(guild);
                                    embed.Description = $"Verified {context.User.Mention} who is **{Format.Sanitize(username)}** on PR2.";
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
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully verified your PR2 Account.");
                                }
                                WebClient wc = new WebClient();
                                wc.Headers.Add("Referer", "https://pr2hub.com/");
                                System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection
                                {
                                    { "message", $"Hey {username}! Thank you for verifying your PR2 Account with Fred the G. Cactus on the Discord Server. You verified your PR2 Account with the Discord Account {context.User.Username}#{context.User.Discriminator} ({context.User.Id}). If you did not do this you should change your password and your email on your account as well to make sure it is secure." },
                                    { "to_name", username },
                                    { "token", pr2token }
                                };
                                byte[] responsebytes = wc.UploadValues("https://pr2hub.com/message_send.php", "POST", reqparm);
                                wc.Dispose();
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
                            await context.Channel.SendMessageAsync($"{context.User.Mention} I found a PM from {Format.Sanitize(username)} but it did not say what I was " +
                                $"expecting it to say ({context.Channel.Id}).\nPlease send resend the PM and then do /verifycomplete with your PR2 " +
                                $"name after.");
                            break;
                        }
                    }
                    else if (tries == 10)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} , something went wrong in the verification process. " +
                            $"Make sure you typed your PR2 name correctly, or actually sent the PM.");
                        break;
                    }
                    tries += 1;
                }
                web.Dispose();
                if (responseString.Equals("{\"success\":false,\"error\":\"Could not find a valid login token. Please log in again.\"}"))
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} the token of FredTheG.CactusBot has expired. Please contact a PR2 Staff Member so that they can update it.");
                }
            }
        }

        public async Task SuggestAsync(SocketCommandContext context, [Remainder] string suggestion)
        {
            if (File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt")).Contains(context.User.Id.ToString()))
            {
                return;
            }
            if (suggestion == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}suggest";
                embed.Description = $"**Description:** Suggest something for the Discord Server.\n**Usage:** {prefix}suggest [suggestion]\n**Example:** {prefix}suggest Make Fred admin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (suggestion.Length < 18 || suggestion.Length > 800)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} your suggestion must be between at least 18 and no more than 800 characters long.");
                    return;
                }
                SocketTextChannel channel = context.Guild.Channels.Where(x => x.Name.ToUpper() == "suggestions".ToUpper()).First() as SocketTextChannel;
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = context.User.Username + "#" + context.User.Discriminator,
                    IconUrl = context.User.GetAvatarUrl(),
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

        public async Task WeatherAsync(SocketCommandContext context, [Remainder] string city)
        {
            if (city == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = $"Command: /weather";
                embed.Description = "**Description:** Get weather about a city.\n**Usage:** /weather [city]\n**Example:** /weather Bristol";
                await context.Channel.SendMessageAsync("", false, embed.Build());
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
                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That city does not exist or could not be found.");
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
                    IconUrl = context.User.GetAvatarUrl(),
                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
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
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task HintAsync(SocketCommandContext context)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/files/artifact_hint.txt");
                }
                catch (HttpRequestException)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                web.Dispose();
                if (text != null)
                {
                    PR2Hint hint = JsonConvert.DeserializeObject<PR2Hint>(text);
                    if (hint.FinderName == null || hint.FinderName.Length < 1)
                    {
                        await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!");
                    }
                    else
                    {
                        SocketGuild guild = context.Client.GetGuild(528679522707701760);
                        long finderID = User.GetUser("pr2_name", hint.FinderName).UserID;
                        if (hint.BubblesName != null && hint.BubblesName.Length > 0)
                        {
                            long bubblesID = User.GetUser("pr2_name", hint.BubblesName).UserID;
                            if (finderID != 0 && bubblesID != 0)
                            {
                                if (Extensions.UserInGuild(null, guild, finderID.ToString()) != null)
                                {
                                    SocketGuildUser user = guild.GetUser(ulong.Parse(finderID.ToString()));
                                    if (hint.FinderName == hint.BubblesName)
                                    {
                                        await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                            $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))} ({Format.Sanitize(user.Username)}#{user.Discriminator})**!\n");
                                    }
                                    else if (Extensions.UserInGuild(null, guild, bubblesID.ToString()) != null)
                                    {
                                        SocketGuildUser user2 = guild.GetUser(ulong.Parse(bubblesID.ToString()));
                                        await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                            $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))} ({Format.Sanitize(user.Username)}#{user.Discriminator})**!\n" +
                                            $"Since they already have the bubble set, the prize was awarded to **{Format.Sanitize(Uri.UnescapeDataString(hint.BubblesName))} ({Format.Sanitize(user2.Username)}#{user2.Discriminator})** instead!");
                                    }
                                    else
                                    {
                                        await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                            $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))} ({Format.Sanitize(user.Username)}#{user.Discriminator})**!\n" +
                                            $"Since they already have the bubble set, the prize was awarded to **{Format.Sanitize(Uri.UnescapeDataString(hint.BubblesName))}** instead!");
                                    }
                                }
                                else if (Extensions.UserInGuild(null, guild, bubblesID.ToString()) != null)
                                {
                                    SocketGuildUser user = guild.GetUser(ulong.Parse(bubblesID.ToString()));
                                    await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                        $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))}**!\n" +
                                        $"Since they already have the bubble set, the prize was awarded to **{Format.Sanitize(Uri.UnescapeDataString(hint.BubblesName))} ({Format.Sanitize(user.Username)}#{user.Discriminator})** instead!");
                                }
                                else if (hint.FinderName == hint.BubblesName)
                                {
                                    await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                        $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))}**!\n");
                                }
                                else
                                {
                                    await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                        $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))}**!\n" +
                                        $"Since they already have the bubble set, the prize was awarded to **{Format.Sanitize(Uri.UnescapeDataString(hint.BubblesName))}** instead!");
                                }
                            }
                            else if (hint.FinderName == hint.BubblesName)
                            {
                                await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                        $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))}**!\n");
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                    $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))}**!\n" +
                                    $"Since they already have the bubble set, the prize was awarded to **{Format.Sanitize(Uri.UnescapeDataString(hint.BubblesName))}** instead!");
                            }
                        }
                        else if (finderID != 0)
                        {
                            if (Extensions.UserInGuild(null, guild, finderID.ToString()) != null)
                            {
                                SocketGuildUser user = guild.GetUser(ulong.Parse(finderID.ToString()));
                                await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                    $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))} ({Format.Sanitize(user.Username)}#{user.Discriminator})**!\n" +
                                    $"The bubble set will be awarded to the first person to find the artifact that doesn't have the set already!");
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                    $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))}**!\n" +
                                    $"The bubble set will be awarded to the first person to find the artifact that doesn't have the set already!");
                            }
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint.Hint))}**. Maybe I can remember more later!!\n" +
                                $"The first person to find this artifact was **{Format.Sanitize(Uri.UnescapeDataString(hint.FinderName))}**!\n" +
                                $"The bubble set will be awarded to the first person to find the artifact that doesn't have the set already!");
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ViewAsync(SocketCommandContext context, [Remainder] string pr2name)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (string.IsNullOrWhiteSpace(pr2name))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /view";
                        embed.Description = "**Description:** View a PR2 account by name.\n**Usage:** /view [PR2 username]\n**Example:** /view Jiggmin";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}view";
                        embed.Description = $"**Description:** View a PR2 account by name.\n**Usage:** {prefix}view [PR2 username]\n**Example:** {prefix}view Jiggmin";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (context.Message.MentionedUsers.Count > 0)
                    {
                        int argPos = 0;
                        if (!(context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count == 1))
                        {
                            SocketUser user = null;
                            if ((context.Channel is IDMChannel && context.Message.HasStringPrefix("/", ref argPos)) || (context.Channel is SocketTextChannel && context.Message.HasStringPrefix(Guild.Get(context.Guild).Prefix, ref argPos)))
                            {
                                user = context.Message.MentionedUsers.First();
                            }
                            else if (context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count > 1)
                            {
                                user = context.Message.MentionedUsers.ElementAt(1);
                            }
                            if (user == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user does not exist or could not be found.");
                                return;
                            }
                            if (!User.Exists(user))
                            {
                                User.Add(user);
                            }
                            pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                            if (pr2name == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user has not linked their PR2 account.");
                                return;
                            }
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
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    if (pr2name.Contains("%20%7C%20") && context.Channel is IDMChannel)
                    {
                        string[] pr2users = pr2name.Split("%20%7C%20");
                        if (pr2users.Count() <= 5)
                        {
                            foreach (string pr2user in pr2users)
                            {
                                string pr2info = null;
                                try
                                {
                                    pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2user);
                                }
                                catch (HttpRequestException)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                                }
                                if (pr2info != null)
                                {
                                    PR2User user = JsonConvert.DeserializeObject<PR2User>(pr2info);
                                    while (!user.Success && user.Error.Equals("Slow down a bit, yo."))
                                    {
                                        await Task.Delay(500);
                                        pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2user);
                                        user = JsonConvert.DeserializeObject<PR2User>(pr2info);
                                    }
                                    if (!user.Success)
                                    {
                                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {user.Error}");
                                    }
                                    else
                                    {
                                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                        {
                                            Name = $"-- {Uri.UnescapeDataString(user.Name)} --",
                                            Url = "https://pr2hub.com/player_search.php?name=" + Uri.EscapeDataString(pr2name)
                                        };
                                        embed.WithAuthor(author);
                                        if (user.GuildId.Equals("0"))
                                        {
                                            embed.Description = $"{user.Status}\n" +
                                                $"**Group:** {user.Group}\n" +
                                                $"**Guild:** none\n" +
                                                $"**Rank:** {user.Rank}\n" +
                                                $"**Hats:** {user.Hats}\n" +
                                                $"**Joined:** {user.RegisterDate}\n" +
                                                $"**Active:** {user.LoginDate}";
                                        }
                                        else
                                        {
                                            embed.Description = $"{user.Status}\n" +
                                                $"**Group:** {user.Group}\n" +
                                                $"**Guild:** [{Format.Sanitize(Uri.UnescapeDataString(user.GuildName))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(user.GuildName)})\n" +
                                                $"**Rank:** {user.Rank}\n" +
                                                $"**Hats:** {user.Hats}\n" +
                                                $"**Joined:** {user.RegisterDate}\n" +
                                                $"**Active:** {user.LoginDate}";
                                        }
                                        await context.Channel.SendMessageAsync("", false, embed.Build());
                                    }
                                    await Task.Delay(1000);
                                }
                            }
                            web.Dispose();
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: You can only view a maximum of 5 users at a time.");
                            web.Dispose();
                            return;
                        }
                    }
                    else
                    {
                        string pr2info = null;
                        try
                        {
                            pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name);
                        }
                        catch (HttpRequestException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                        }                       
                        if (pr2info != null)
                        {
                            PR2User user = JsonConvert.DeserializeObject<PR2User>(pr2info);
                            while (!user.Success && user.Error.Equals("Slow down a bit, yo."))
                            {
                                await Task.Delay(500);
                                pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name);
                                user = JsonConvert.DeserializeObject<PR2User>(pr2info);
                            }
                            if (!user.Success)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {user.Error}");
                            }
                            else
                            {
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"-- {Uri.UnescapeDataString(user.Name)} --",
                                    Url = "https://pr2hub.com/player_search.php?name=" + Uri.EscapeDataString(pr2name)
                                };
                                embed.WithAuthor(author);
                                if (user.GuildId.Equals("0"))
                                {
                                    embed.Description = $"{user.Status}\n" +
                                        $"**Group:** {user.Group}\n" +
                                        $"**Guild:** none\n" +
                                        $"**Rank:** {user.Rank}\n" +
                                        $"**Hats:** {user.Hats}\n" +
                                        $"**Joined:** {user.RegisterDate}\n" +
                                        $"**Active:** {user.LoginDate}";
                                }
                                else
                                {
                                    embed.Description = $"{user.Status}\n" +
                                        $"**Group:** {user.Group}\n" +
                                        $"**Guild:** [{Format.Sanitize(Uri.UnescapeDataString(user.GuildName))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(user.GuildName)})\n" +
                                        $"**Rank:** {user.Rank}\n" +
                                        $"**Hats:** {user.Hats}\n" +
                                        $"**Joined:** {user.RegisterDate}\n" +
                                        $"**Active:** {user.LoginDate}";
                                }
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        web.Dispose();
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ViewIDAsync(SocketCommandContext context, string id)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out _))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = $"Command: /viewid";
                        embed.Description = "**Description:** View a PR2 account by ID.\n**Usage:** /viewid [PR2 user ID]\n**Example:** /viewid 1";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}viewid";
                        embed.Description = $"**Description:** View a PR2 account by ID.\n**Usage:** {prefix}viewid [PR2 user ID]\n**Example:** {prefix}viewid 1";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
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
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    string pr2info = null;
                    try
                    {
                        pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?user_id=" + id);
                    }
                    catch (HttpRequestException)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                    }               
                    if (pr2info != null)
                    {
                        PR2User user = JsonConvert.DeserializeObject<PR2User>(pr2info);
                        while (!user.Success && user.Error.Equals("Slow down a bit, yo."))
                        {
                            await Task.Delay(500);
                            pr2info = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?user_id=" + id);
                            user = JsonConvert.DeserializeObject<PR2User>(pr2info);
                        }
                        if (!user.Success)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {user.Error}");
                        }
                        else
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"-- {Uri.UnescapeDataString(user.Name)} --",
                                Url = "https://pr2hub.com/player_search.php?name=" + Uri.EscapeDataString(user.Name)
                            };
                            embed.WithAuthor(author);
                            if (user.GuildId.Equals("0"))
                            {
                                embed.Description = $"{user.Status}\n" +
                                    $"**Group:** {user.Group}\n" +
                                    $"**Guild:** none\n" +
                                    $"**Rank:** {user.Rank}\n" +
                                    $"**Hats:** {user.Hats}\n" +
                                    $"**Joined:** {user.RegisterDate}\n" +
                                    $"**Active:** {user.LoginDate}";
                            }
                            else
                            {
                                embed.Description = $"{user.Status}\n" +
                                    $"**Group:** {user.Group}\n" +
                                    $"**Guild:** [{Format.Sanitize(Uri.UnescapeDataString(user.GuildName))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(user.GuildName)})\n" +
                                    $"**Rank:** {user.Rank}\n" +
                                    $"**Hats:** {user.Hats}\n" +
                                    $"**Joined:** {user.RegisterDate}\n" +
                                    $"**Active:** {user.LoginDate}";
                            }
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    web.Dispose();
                }
            }
            else
            {
                return;
            }
        }

        public async Task GuildAsync(SocketCommandContext context, [Remainder] string guildname)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (guildname == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /guild";
                        embed.Description = "**Description:** View a PR2 guild by name.\n**Usage:** /guild [PR2 guild name]\n**Example:** /guild PR2 Staff";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}guild";
                        embed.Description = $"**Description:** View a PR2 guild by name.\n**Usage:** {prefix}guild [PR2 guild name]\n**Example:** {prefix}guild PR2 Staff";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    if (context.Message.MentionedUsers.Count > 0)
                    {
                        int argPos = 0;
                        if (!(context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count == 1))
                        {
                            SocketUser user = null;
                            if ((context.Channel is IDMChannel && context.Message.HasStringPrefix("/", ref argPos)) || (context.Channel is SocketTextChannel && context.Message.HasStringPrefix(Guild.Get(context.Guild).Prefix, ref argPos)))
                            {
                                user = context.Message.MentionedUsers.First();
                            }
                            else if (context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count > 1)
                            {
                                user = context.Message.MentionedUsers.ElementAt(1);
                            }
                            if (user == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user does not exist or could not be found.");
                                web.Dispose();
                                return;
                            }
                            if (!User.Exists(user))
                            {
                                User.Add(user);
                            }
                            string pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                            if (pr2name == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user has not linked their PR2 account.");
                                web.Dispose();
                                return;
                            }
                            string text;
                            try
                            {
                                text = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name);
                            }
                            catch (HttpRequestException)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                                web.Dispose();
                                return;
                            }
                            PR2User pr2User = JsonConvert.DeserializeObject<PR2User>(text);
                            while (!pr2User.Success && pr2User.Error.Equals("Slow down a bit, yo."))
                            {
                                await Task.Delay(500);
                                text = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name);
                                pr2User = JsonConvert.DeserializeObject<PR2User>(text);
                            }
                            if (!pr2User.Success)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That users account no longer exists.");
                                web.Dispose();
                                return;
                            }
                            if (pr2User.GuildName == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That users account is not a member of a guild.");
                                web.Dispose();
                                return;
                            }
                            guildname = pr2User.GuildName;
                        }
                    }
                    string pr2info = null;
                    try
                    {
                        pr2info = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname);
                    }
                    catch (HttpRequestException)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} connection to PR2 Hub was not successfull.");
                    }
                    if (pr2info != null)
                    {
                        PR2GuildResponse response = JsonConvert.DeserializeObject<PR2GuildResponse>(pr2info);
                        while (!response.Success && response.Error.Equals("Slow down a bit, yo."))
                        {
                            await Task.Delay(500);
                            pr2info = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname);
                            response = JsonConvert.DeserializeObject<PR2GuildResponse>(pr2info);
                        }
                        if (!response.Success)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {response.Error}");
                        }
                        else
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"-- {response.Guild.Name} --",
                                Url = "https://pr2hub.com/guild_search.php?name=" + guildname.Replace(" ", "%20")
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = context.User.GetAvatarUrl(),
                                Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            embed.ThumbnailUrl = "https://pr2hub.com/emblems/" + response.Guild.Emblem;
                            embed.Description = $"**Created At:** {response.Guild.CreationDate}\n" +
                                $"**Members:** {response.Guild.MemberCount} ({response.Guild.ActiveCount} active)\n" +
                                $"**GP Total:** {response.Guild.GPTotal}\n" +
                                $"**GP Today:** {response.Guild.GPToday}\n" +
                                $"**Description:** {Format.Sanitize(response.Guild.Note)}";
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    web.Dispose();
                }
            }
            else
            {
                return;
            }
        }

        public async Task GuildIDAsync(SocketCommandContext context, string id)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (string.IsNullOrWhiteSpace(id) || !int.TryParse(id, out _))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /guildid";
                        embed.Description = "**Description:** View a PR2 guild by ID.\n**Usage:** /guildid [PR2 guild ID]\n**Example:** /guildid 183";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}guildid";
                        embed.Description = $"**Description:** View a PR2 guild by ID.\n**Usage:** {prefix}guildid [PR2 guild ID]\n**Example:** {prefix}guildid 183";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                    }
                    if (pr2info != null)
                    {
                        PR2GuildResponse response = JsonConvert.DeserializeObject<PR2GuildResponse>(pr2info);
                        while (!response.Success && response.Error.Equals("Slow down a bit, yo."))
                        {
                            await Task.Delay(500);
                            pr2info = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&id=" + id);
                            response = JsonConvert.DeserializeObject<PR2GuildResponse>(pr2info);
                        }
                        if (!response.Success)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {response.Error}");
                        }
                        else
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"-- {response.Guild.Name} --",
                                Url = "https://pr2hub.com/guild_search.php?id=" + id
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = context.User.GetAvatarUrl(),
                                Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            embed.ThumbnailUrl = "https://pr2hub.com/emblems/" + response.Guild.Emblem;
                            embed.Description = $"**Created At:** {response.Guild.CreationDate}" +
                                $"\n**Members:** {response.Guild.MemberCount} ({response.Guild.ActiveCount} active)" +
                                $"\n**GP Total:** {response.Guild.GPTotal}" +
                                $"\n**GP Today:** {response.Guild.GPToday}" +
                                $"\n**Description:** {Format.Sanitize(response.Guild.Note)}";
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    web.Dispose();
                }
            }
            else
            {
                return;
            }
        }

        public async Task EXPAsync(SocketCommandContext context, [Remainder] string lvl)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                string lvl2 = "0";
                if (string.IsNullOrEmpty(lvl))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /exp";
                        embed.Description = "**Description:** Get EXP needed from one rank to another.\n**Usage:** /exp [rank], [optional rank]\n**Example:** /exp 28, 30";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}exp";
                        embed.Description = $"**Description:** Get EXP needed from one rank to another.\n**Usage:** {prefix}exp [rank], [optional rank]\n**Example:** {prefix}exp 28, 30";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That does not seem to be an integer.");
                    }
                    else if (level_ < 0 || level_ > 149)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: You can only do a level from 0 to 149");
                    }
                    else
                    {
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                        };

                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl(),
                            Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
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
                                    exp = Math.Round(Math.Pow(1.25, level_) * 30).ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"));
                                }
                                embed.WithFooter(footer);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"EXP - {lvl} to {level_ + 1}"
                                };
                                embed.WithAuthor(author);
                                embed.WithCurrentTimestamp();
                                embed.Description = $"**From rank {lvl} to rank {level_ + 1} you need {exp} EXP.**";
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                if (!int.TryParse(lvl2, out int level_2))
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That does not seem to be an integer.");
                                }
                                else if (level_2 < 0 || level_2 > 150)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: You can only do a level from 0 to 149");
                                }
                                else if (level_ > level_2)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Your EXP can't go down.");
                                }
                                else if (level_ == level_2)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That would just be 0.");
                                }
                                else
                                {
                                    double exp = 0;
                                    for (int i = level_; i < level_2; i++)
                                    {
                                        if (i == 0)
                                        {
                                            exp += 1;
                                        }
                                        else
                                        {
                                            exp += Math.Round(Math.Pow(1.25, i) * 30);
                                        }
                                    }
                                    embed.WithFooter(footer);
                                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                    {
                                        Name = $"EXP - {level_} to {level_2}"
                                    };
                                    embed.WithAuthor(author);
                                    embed.WithCurrentTimestamp();
                                    embed.Description = $"**From rank {level_} to rank {level_2} you need {exp.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))} EXP.**";
                                    await context.Channel.SendMessageAsync("", false, embed.Build());
                                }
                            }
                        }
                        catch (ArgumentException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Length of number is too long. (Don't use an excessive amount of 0's).");
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

        public async Task RoleAsync(SocketCommandContext context, string roleName)
        {
            List<AllowedChannel> channels = AllowedChannel.Get(context.Guild.Id);
            if (channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (roleName == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command: {prefix}role";
                    embed.Description = $"**Description:** Add/remove one of the joinable roles.\n**Usage:** {prefix}role [role name]\n**Example:** {prefix}role Arti";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                    {
                        SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                        if (JoinableRole.Get(context.Guild.Id, role.Id).Count > 0)
                        {
                            SocketGuildUser user = context.Guild.GetUser(context.User.Id);
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
                                    IconUrl = context.User.GetAvatarUrl(),
                                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                };
                                embed.WithFooter(footer);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"{role.Name} Remove"
                                };
                                embed.WithAuthor(author);
                                embed.WithCurrentTimestamp();
                                embed.Description = $"**{context.User.Mention} has been removed from the {Format.Sanitize(role.Name)} role({role.Mention}).**";
                                await context.Channel.SendMessageAsync("", false, embed.Build());
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
                                    IconUrl = context.User.GetAvatarUrl(),
                                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                };
                                embed.WithFooter(footer);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"{role.Name} Add"
                                };
                                embed.WithAuthor(author);
                                embed.WithCurrentTimestamp();
                                embed.Description = $"**{context.User.Mention} has been added to the {Format.Sanitize(role.Name)} role({role.Mention}).**";
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That role is not a joinable role.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That role does not exist or could not be found.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ListJoinableRolesAsync(SocketCommandContext context)
        {
            List<AllowedChannel> channels = AllowedChannel.Get(context.Guild.Id);
            if (channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = context.Guild.IconUrl,
                    Name = "Joinable Roles"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                    Author = auth
                };
                List<JoinableRole> joinableRoles = JoinableRole.Get(context.Guild.Id);
                if (joinableRoles.Count <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: There are no joinable roles.");
                }
                else
                {
                    bool removed = false;
                    List<string> jRoles = new List<string>();
                    foreach (JoinableRole jRole in joinableRoles)
                    {
                        SocketRole role = context.Guild.GetRole(ulong.Parse(jRole.RoleID.ToString()));
                        if (role != null)
                        {
                            jRoles.Add(Format.Sanitize(role.Name + " - " + jRole.Description));
                        }
                        else
                        {
                            JoinableRole.Remove(context.Guild.Id, ulong.Parse(jRole.RoleID.ToString()));
                            removed = true;
                        }
                    }
                    jRoles.Sort();
                    foreach (string role in jRoles)
                    {
                        embed.Description += role + "\n";
                    }
                    if (removed)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: 1 or more joinable roles were removed because they no longer exist.");
                    }
                    if (embed.Description == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: There are no joinable roles.");
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task TopGuildsAsync(SocketCommandContext context)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/guilds_top.php");
                }
                catch (HttpRequestException)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    PR2GuildList guildList = JsonConvert.DeserializeObject<PR2GuildList>(text);
                    while (!guildList.Success)
                    {
                        await Task.Delay(500);
                        text = await web.GetStringAsync("https://pr2hub.com/guilds_top.php");
                        guildList = JsonConvert.DeserializeObject<PR2GuildList>(text);
                    }
                    int count = 0;
                    string guilds = "", gps = "";
                    foreach (PR2Guild guild in guildList.Guilds)
                    {
                        string guildName = Regex.Unescape(guild.Name);
                        string guildGP = guild.GPToday.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"));
                        guilds += $"[{Format.Sanitize(Uri.UnescapeDataString(guildName))}](https://pr2hub.com/guild_search.php?name=" + $"{Uri.EscapeDataString(guildName)})\n";
                        gps += guildGP + "\n";
                        count++;
                        if (count >= 10)
                        {
                            break;
                        }
                    }
                    EmbedBuilder embed = new EmbedBuilder();
                    embed.WithColor(new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)));
                    embed.AddField(y =>
                    {
                        y.Name = "Guild";
                        y.Value = guilds;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "GP Today";
                        y.Value = gps;
                        y.IsInline = true;
                    });
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = $"PR2 Top 10 Guilds",
                        Url = "https://pr2hub.com/guilds_top.php"
                    };
                    embed.WithAuthor(author);
                    embed.WithCurrentTimestamp();
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                web.Dispose();
            }
            else
            {
                return;
            }
        }

        public async Task FahAsync(SocketCommandContext context, [Remainder] string fahuser)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (fahuser == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /fah";
                        embed.Description = "**Description:** Get F@H info for a user.\n**Usage:** /fah [PR2 name]\n**Example:** /fah Jiggmin";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}fah";
                        embed.Description = $"**Description:** Get F@H info for a user.\n**Usage:** {prefix}fah [PR2 name]\n**Example:** {prefix}fah Jiggmin";
                    }

                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (context.Message.MentionedUsers.Count > 0)
                    {
                        int argPos = 0;
                        if (!(context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count == 1))
                        {
                            SocketUser user = null;
                            if ((context.Channel is IDMChannel && context.Message.HasStringPrefix("/", ref argPos)) || (context.Channel is SocketTextChannel && context.Message.HasStringPrefix(Guild.Get(context.Guild).Prefix, ref argPos)))
                            {
                                user = context.Message.MentionedUsers.First();
                            }
                            else if (context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count > 1)
                            {
                                user = context.Message.MentionedUsers.ElementAt(1);
                            }
                            if (user == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user does not exist or could not be found.");
                                return;
                            }
                            if (!User.Exists(user))
                            {
                                User.Add(user);
                            }
                            fahuser = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                            if (fahuser == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user has not linked their PR2 account.");
                                return;
                            }
                        }
                    }
                    HttpClient web = new HttpClient();
                    try
                    {
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
                            IconUrl = context.User.GetAvatarUrl(),
                            Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        string text;
                        try
                        {
                            text = await web.GetStringAsync("https://stats.foldingathome.org/api/donor/" + fahuser.Replace(' ', '_'));
                            web.Dispose();
                        }
                        catch (OperationCanceledException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: The F@H API took too long to respond.");
                            web.Dispose();
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
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user does not exist or could not be found.");
                                return;
                            }
                            embed.AddField(y =>
                            {
                                y.Name = "Score";
                                y.Value = $"{Convert.ToInt32(stats.GetValue("credit")).ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Completed WUs";
                                y.Value = $"{Convert.ToInt32(stats.GetValue("wus")).ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}";
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
                            embed.AddField(y =>
                            {
                                y.Name = "Active clients (within 50 days)";
                                y.Value = $"{Convert.ToInt32(stats.GetValue("active_50")).ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Active clients (within 7 days)";
                                y.Value = $"{Convert.ToInt32(stats.GetValue("active_7")).ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}";
                                y.IsInline = true;
                            });
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                        catch (JsonReaderException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: The F@H Api is currently down.");
                        }
                    }
                    catch (HttpRequestException)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user does not exist or could not be found.");
                        web.Dispose();
                        return;
                    }
                    catch (AuthenticationException)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: The F@H certificate date is invalid.");
                        web.Dispose();
                        return;
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task BansAsync(SocketCommandContext context, string id)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (id == null || !int.TryParse(id, out int id2))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /bans";
                        embed.Description = "**Description:** Get PR2 ban info.\n**Usage:** /bans [PR2 ban ID]\n**Example:** /bans 59098";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}bans";
                        embed.Description = $"**Description:** Get PR2 ban info.\n**Usage:** {prefix}bans [PR2 ban ID]\n**Example:** {prefix}bans 59098";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                    }           
                    if (text != null)
                    {
                        if (Extensions.GetBetween(text, "<title>", "</title>").Contains("PR2 Hub - Error Fetching Ban"))
                        {
                            if (text.Contains("Error: Slow down a bit, yo."))
                            {
                                while (Extensions.GetBetween(text, "<title>", "</title>").Contains("PR2 Hub - Error Fetching Ban") && text.Contains("Error: Slow down a bit, yo."))
                                {
                                    await Task.Delay(500);
                                    text = await web.GetStringAsync("https://pr2hub.com/bans/show_record.php?ban_id=" + id);
                                }
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: The ban with that ID does not exist or could not be found.");
                                web.Dispose();
                                return;
                            }
                        }
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = $"Ban ID - {id}",
                            Url = "https://pr2hub.com/bans/show_record.php?ban_id=" + id
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl(),
                            Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
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
                            await context.Channel.SendMessageAsync("", false, embed.Build());
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
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    web.Dispose();
                }
            }
            else
            {
                return;
            }
        }

        public async Task PopAsync(SocketCommandContext context, [Remainder] string s)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (s != null)
                {
                    await StatsAsync(context, s);
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
                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                }
                web.Dispose();
                if (text != null)
                {
                    PR2ServerList serverList = JsonConvert.DeserializeObject<PR2ServerList>(text);
                    int pop = 0;
                    foreach (PR2Server server in serverList.Servers)
                    {
                        pop += server.Population;
                    }
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
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
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        public async Task StatsAsync(SocketCommandContext context, [Remainder] string server)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (string.IsNullOrWhiteSpace(server))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /stats";
                        embed.Description = "**Description:** Get a PR2 server info.\n**Usage:** /stats [PR2 server name]\n**Example:** /stats Derron";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}stats";
                        embed.Description = $"**Description:** Get a PR2 server info.\n**Usage:** {prefix}stats [PR2 server name]\n**Example:** {prefix}stats Derron";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    if (context.Message.MentionedUsers.Count > 0)
                    {
                        int argPos = 0;
                        if (!(context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count == 1))
                        {
                            SocketUser user = null;
                            if ((context.Channel is IDMChannel && context.Message.HasStringPrefix("/", ref argPos)) || (context.Channel is SocketTextChannel && context.Message.HasStringPrefix(Guild.Get(context.Guild).Prefix, ref argPos)))
                            {
                                user = context.Message.MentionedUsers.First();
                            }
                            else if (context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count > 1)
                            {
                                user = context.Message.MentionedUsers.ElementAt(1);
                            }
                            if (user == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user does not exist or could not be found.");
                                web.Dispose();
                                return;
                            }
                            if (!User.Exists(user))
                            {
                                User.Add(user);
                            }
                            string pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                            if (pr2name == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user has not linked their PR2 account.");
                                web.Dispose();
                                return;
                            }
                            PR2User pr2user = null;
                            try
                            {
                                pr2user = JsonConvert.DeserializeObject<PR2User>(await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name));
                            }
                            catch (HttpRequestException)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                                web.Dispose();
                                return;
                            }
                            while (!pr2user.Success && pr2user.Error.Equals("Slow down a bit, yo."))
                            {
                                await Task.Delay(500);
                                pr2user = JsonConvert.DeserializeObject<PR2User>(await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name));
                            }
                            if (!pr2user.Success)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {pr2user.Error}");
                                web.Dispose();
                                return;
                            }
                            if (pr2user.Status.Equals("offline"))
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That users account is offline.");
                                web.Dispose();
                                return;
                            }
                            server = pr2user.Status.Substring(11);
                        }
                    }
                    string text = null;
                    try
                    {
                        text = await web.GetStringAsync("https://pr2hub.com/files/server_status_2.txt");
                    }
                    catch (HttpRequestException)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                    }
                    web.Dispose();
                    if (text != null)
                    {
                        PR2ServerList serverList = JsonConvert.DeserializeObject<PR2ServerList>(text);
                        if (serverList.Servers.Where(x => x.Name.Equals(server, StringComparison.InvariantCultureIgnoreCase)).Any())
                        {
                            PR2Server pr2Server = serverList.Servers.Where(x => x.Name.Equals(server, StringComparison.InvariantCultureIgnoreCase)).First();
                            string hh = "No";
                            string tourn = "No";
                            if (pr2Server.HappyHour == 1)
                            {
                                hh = "Yes";
                            }
                            if (pr2Server.Tournament == 1)
                            {
                                tourn = "Yes";
                            }
                            if (pr2Server.Status == "open")
                            {
                                pr2Server.Status = "Open";
                            }
                            if (pr2Server.Status == "down")
                            {
                                pr2Server.Status = "Down";
                            }
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = context.User.GetAvatarUrl(),
                                Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                            };
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = $"Server Stats - {pr2Server.Name}",
                                Url = "https://pr2hub.com/server_status.php"
                            };
                            embed.WithFooter(footer);
                            embed.WithAuthor(author);
                            embed.AddField(y =>
                            {
                                y.Name = "Population";
                                y.Value = $"{pr2Server.Population}";
                                y.IsInline = true;
                            });
                            embed.AddField(y =>
                            {
                                y.Name = "Status";
                                y.Value = $"{pr2Server.Status}";
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
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That server does not exist or could not be found.");
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task GuildMembersAsync(SocketCommandContext context, [Remainder] string guildname)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (guildname == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /guildmembers";
                        embed.Description = "**Description:** Get a members for a PR2 guild by name.\n**Usage:** /guildmembers [PR2 guild name]\n**Example:** /guildmembers PR2 Staff";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}guildmembers";
                        embed.Description = $"**Description:** Get a members for a PR2 guild by name.\n**Usage:** {prefix}guildmembers [PR2 guild name]\n**Example:** {prefix}guildmembers PR2 Staff";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    HttpClient web = new HttpClient();
                    if (context.Message.MentionedUsers.Count > 0)
                    {
                        int argPos = 0;
                        if (!(context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count == 1))
                        {
                            SocketUser user = null;
                            if ((context.Channel is IDMChannel && context.Message.HasStringPrefix("/", ref argPos)) || (context.Channel is SocketTextChannel && context.Message.HasStringPrefix(Guild.Get(context.Guild).Prefix, ref argPos)))
                            {
                                user = context.Message.MentionedUsers.First();
                            }
                            else if (context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count > 1)
                            {
                                user = context.Message.MentionedUsers.ElementAt(1);
                            }
                            if (user == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user does not exist or could not be found.");
                                web.Dispose();
                                return;
                            }
                            if (!User.Exists(user))
                            {
                                User.Add(user);
                            }
                            string pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                            if (pr2name == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user has not linked their PR2 account.");
                                web.Dispose();
                                return;
                            }
                            PR2User pr2user;
                            try
                            {
                                pr2user = JsonConvert.DeserializeObject<PR2User>(await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name));
                            }
                            catch (HttpRequestException)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                                web.Dispose();
                                return;
                            }
                            while (!pr2user.Success && pr2user.Error.Equals("Slow down a bit, yo."))
                            {
                                await Task.Delay(500);
                                pr2user = JsonConvert.DeserializeObject<PR2User>(await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name));
                            }
                            if (!pr2user.Success)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {pr2user.Error}");
                                web.Dispose();
                                return;
                            }
                            if (pr2user.GuildId.Equals("0"))
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That users account is not a member of a guild.");
                                web.Dispose();
                                return;
                            }
                            guildname = pr2user.GuildName;
                        }
                    }
                    string text = null;
                    try
                    {
                        text = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname);
                    }
                    catch (HttpRequestException)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                    }
                    if (text != null)
                    {
                        PR2GuildResponse response = JsonConvert.DeserializeObject<PR2GuildResponse>(text);
                        while (!response.Success && response.Error.Equals("Slow down a bit, yo."))
                        {
                            await Task.Delay(500);
                            response = JsonConvert.DeserializeObject<PR2GuildResponse>(await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guildname));
                        }
                        if (!response.Success)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {response.Error}");
                        }
                        else
                        {
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
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = context.User.GetAvatarUrl(),
                                Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            bool overflow = false;
                            foreach (PR2GuildMember member in response.Guild.Members)
                            {
                                guildMembers.Add($"[{Format.Sanitize(member.Name)}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(member.Name)})");
                                if (string.Join(", ", guildMembers).Length + 14 > 2048)
                                {
                                    guildMembers.RemoveAt(guildMembers.Count - 1);
                                    if (context.Channel is ITextChannel)
                                    {
                                        overflow = true;
                                        break;
                                    }
                                    else
                                    {
                                        embed.Description = $"{string.Join(", ", guildMembers)}";
                                        await context.Channel.SendMessageAsync("", false, embed.Build());
                                        guildMembers.Clear();
                                        guildMembers.Add($"[{Format.Sanitize(member.Name)}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(member.Name)})");
                                    }
                                }
                            }
                            if (context.Channel is ITextChannel)
                            {
                                if (overflow)
                                {
                                    embed.Description = $"{string.Join(", ", guildMembers)}, and {response.Guild.MemberCount - guildMembers.Count} more";
                                }
                                else
                                {
                                    embed.Description = $"{string.Join(", ", guildMembers)}";
                                }
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                embed.Description = $"{string.Join(", ", guildMembers)}";
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                    web.Dispose();
                }
            }
            else
            {
                return;
            }
        }

        public async Task GuildMembersIDAsync(SocketCommandContext context, string id)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (id == null || !int.TryParse(id, out _))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /guildmembersid";
                        embed.Description = "**Description:** Get a members for a PR2 guild by ID.\n**Usage:** /guildmembersid [PR2 guild ID]\n**Example:** /guildmembersid 183";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}guildmembersid";
                        embed.Description = $"**Description:** Get a members for a PR2 guild by ID.\n**Usage:** {prefix}guildmembersid [PR2 guild ID]\n**Example:** {prefix}guildmembersid 183";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                    }
                    if (text != null)
                    {
                        PR2GuildResponse response = JsonConvert.DeserializeObject<PR2GuildResponse>(text);
                        while (!response.Success && response.Error.Equals("Slow down a bit, yo."))
                        {
                            await Task.Delay(500);
                            response = JsonConvert.DeserializeObject<PR2GuildResponse>(await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&id=" + id));
                        }
                        if (!response.Success)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {response.Error}");
                        }
                        else
                        {
                            List<string> guildMembers = new List<string>();
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Url = "https://pr2hub.com/guild_search.php?id=" + id,
                                Name = "Guild Members - " + response.Guild.Name
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256))
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                IconUrl = context.User.GetAvatarUrl(),
                                Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            bool overflow = false;
                            foreach (PR2GuildMember member in response.Guild.Members)
                            {
                                guildMembers.Add($"[{Format.Sanitize(member.Name)}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(member.Name)})");

                                if (string.Join(", ", guildMembers).Length + 14 > 2048)
                                {
                                    guildMembers.RemoveAt(guildMembers.Count - 1);
                                    if (context.Channel is ITextChannel)
                                    {
                                        overflow = true;
                                        break;
                                    }
                                    else
                                    {
                                        embed.Description = $"{string.Join(", ", guildMembers)}";
                                        await context.Channel.SendMessageAsync("", false, embed.Build());
                                        guildMembers.Clear();
                                        guildMembers.Add($"[{Format.Sanitize(member.Name)}](https://pr2hub.com/player_search.php?name=" + $"{Uri.EscapeDataString(member.Name)})");
                                    }
                                }
                            }
                            if (context.Channel is ITextChannel)
                            {
                                if (overflow)
                                {
                                    embed.Description = $"{string.Join(", ", guildMembers)}, and {response.Guild.MemberCount - guildMembers.Count} more";
                                }
                                else
                                {
                                    embed.Description = $"{string.Join(", ", guildMembers)}";
                                }
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                embed.Description = $"{string.Join(", ", guildMembers)}";
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                    web.Dispose();
                }
            }
            else
            {
                return;
            }
        }

        public async Task HHAsync(SocketCommandContext context)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                HttpClient web = new HttpClient();
                List<string> hhServers = new List<string>();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/files/server_status_2.txt");
                }
                catch (HttpRequestException)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                }
                web.Dispose();
                if (text != null)
                {
                    PR2ServerList servers = JsonConvert.DeserializeObject<PR2ServerList>(text);
                    foreach (PR2Server server in servers.Servers)
                    {
                        if (server.HappyHour == 1)
                        {
                            hhServers.Add(server.Name);
                        }
                    }
                    if (hhServers.Count < 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} No servers currently have happy hour on them.");
                        return;
                    }
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
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    if (hhServers.Count == 1)
                    {
                        author.Name = $"Happy Hour Server";
                        embed.Description = $"This server currently has a happy hour on it: {Format.Sanitize(hhServers.First())}";
                    }
                    else
                    {
                        author.Name = $"Happy Hour Servers";
                        embed.Description = $"These are the current servers with happy hour on them: {Format.Sanitize(string.Join(", ", hhServers))}";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        public async Task LevelAsync(SocketCommandContext context, string option, [Remainder] string search)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (string.IsNullOrWhiteSpace(option))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /level";
                        embed.Description = "**Description:** Get PR2 level info.\n**Usage:** /level [t, id, u] [search]\n**Example:** /level id 50815";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}level";
                        embed.Description = $"**Description:** Get PR2 level info.\n**Usage:** {prefix}level [t, id, u] [search]\n**Example:** {prefix}level id 50815";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string mode = "";
                    if (option.Equals("u") && !string.IsNullOrWhiteSpace(search))
                    {
                        mode = "user";
                    }
                    else if (option.Equals("id") && !string.IsNullOrWhiteSpace(search))
                    {
                        mode = "id";
                    }
                    else if (option.Equals("t") && !string.IsNullOrWhiteSpace(search))
                    {
                        mode = "title";
                    }
                    else
                    {
                        mode = "title";
                        if (search == null)
                        {
                            search = option;
                        }
                        else
                        {
                            search = option + " " + search;
                        }
                    }

                    string pr2token = File.ReadAllText(Path.Combine(Extensions.downloadPath, "PR2Token.txt"));
                    Dictionary<string, string> values = new Dictionary<string, string>
                        {
                            { "dir", "desc" },
                            { "mode", "title" },
                            { "order", "popularity" },
                            { "page", "1" },
                            { "search_str", search },
                            { "token", pr2token }
                        };
                    HttpClient web = new HttpClient();

                    if (mode.Equals("title"))
                    {
                        FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                        HttpResponseMessage response = null;
                        try
                        {
                            response = await web.PostAsync("https://pr2hub.com/search_levels.php?", content);
                        }
                        catch (HttpRequestException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                        }
                        content.Dispose();
                        if (response != null)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            if (responseString.StartsWith("error="))
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {responseString.Split('=').Last()}");
                            }
                            else
                            {
                                string id = Extensions.GetBetween(responseString, "levelID0=", "&version0=");
                                if (id.Length < 1)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That level does not exist or could not be found.");
                                }
                                else
                                {
                                    string text = await web.GetStringAsync("https://pr2hub.com/level_data.php?level_id=" + id + "&token=" + pr2token);
                                    PR2Level level = JsonConvert.DeserializeObject<PR2Level>(text);
                                    text = await web.GetStringAsync("https://pr2hub.com/levels/" + id + ".txt");
                                    string data = text.Substring(text.IndexOf("&data=") + 6);
                                    int blockCount = data.Split("`").ElementAt(2).Count(x => x == ',') + 1;
                                    level.GameMode = CultureInfo.CreateSpecificCulture("en-GB").TextInfo.ToTitleCase(level.GameMode);

                                    switch (level.Song)
                                    {
                                        case "0":
                                            {
                                                level.Song = "None";
                                                break;
                                            }
                                        case "1":
                                            {
                                                level.Song = "Miniature Fantasy - Dreamscaper";
                                                break;
                                            }
                                        case "2":
                                            {
                                                level.Song = "Under Fire - AP";
                                                break;
                                            }
                                        case "3":
                                            {
                                                level.Song = "Paradise on E - API";
                                                break;
                                            }
                                        case "4":
                                            {
                                                level.Song = "Crying Soul - Bounc3";
                                                break;
                                            }
                                        case "5":
                                            {
                                                level.Song = "My Vision - MrMaestro";
                                                break;
                                            }
                                        case "6":
                                            {
                                                level.Song = "Switchblade - SKAzini";
                                                break;
                                            }
                                        case "7":
                                            {
                                                level.Song = "The Wires - Cheez-R-Us";
                                                break;
                                            }
                                        case "8":
                                            {
                                                level.Song = "Before Mydnite - F-777";
                                                break;
                                            }
                                        case "10":
                                            {
                                                level.Song = "Broked It - SWiTCH";
                                                break;
                                            }
                                        case "11":
                                            {
                                                level.Song = "Hello? - TMM43";
                                                break;
                                            }
                                        case "12":
                                            {
                                                level.Song = "Pyrokinesis - Sean Tucker";
                                                break;
                                            }
                                        case "13":
                                            {
                                                level.Song = "Flowerz 'n' Herbz - Brunzolaitis";
                                                break;
                                            }
                                        case "14":
                                            {
                                                level.Song = "Instrumental #4 - Reasoner";
                                                break;
                                            }
                                        case "15":
                                            {
                                                level.Song = "Prismatic - Lunanova";
                                                break;
                                            }
                                        default:
                                            {
                                                level.Song = "Random";
                                                break;
                                            }
                                    }
                                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    DateTime date = start.AddSeconds(level.Time).ToLocalTime();
                                    List<string> itemList = level.Items.Split('`').ToList();
                                    if (int.TryParse(itemList.First(), out int result))
                                    {
                                        List<string> itemList2 = new List<string>();
                                        foreach (string item in itemList)
                                        {
                                            if (item.Equals("1"))
                                            {
                                                itemList2.Add("Laser Gun");
                                            }
                                            else if (item.Equals("2"))
                                            {
                                                itemList2.Add("Mine");
                                            }
                                            else if (item.Equals("3"))
                                            {
                                                itemList2.Add("Lightning");
                                            }
                                            else if (item.Equals("4"))
                                            {
                                                itemList2.Add("Teleport");
                                            }
                                            else if (item.Equals("5"))
                                            {
                                                itemList2.Add("Super Jump");
                                            }
                                            else if (item.Equals("6"))
                                            {
                                                itemList2.Add("Jet Pack");
                                            }
                                            else if (item.Equals("7"))
                                            {
                                                itemList2.Add("Speed Burst");
                                            }
                                            else if (item.Equals("8"))
                                            {
                                                itemList2.Add("Sword");
                                            }
                                            else if (item.Equals("9"))
                                            {
                                                itemList2.Add("Ice Wave");
                                            }
                                        }
                                        itemList = itemList2;
                                    }
                                    if (itemList.First().Length < 1)
                                    {
                                        itemList[0] = "None";
                                    }

                                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                    {
                                        Name = $"-- {level.Title} ({id}) --",
                                        Url = "https://pr2hub.com/levels/" + id + ".txt?version=" + level.Version
                                    };
                                    EmbedBuilder embed = new EmbedBuilder()
                                    {
                                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                        Author = author
                                    };
                                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                    {
                                        IconUrl = context.User.GetAvatarUrl(),
                                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                    };
                                    embed.WithFooter(footer);
                                    embed.WithCurrentTimestamp();
                                    embed.Description = string.Format("{0,-20} {1,-20} {2, 20}", $"**By:** [{Format.Sanitize(level.Username)}](https://pr2hub.com/player_search.php?name={Uri.EscapeDataString(level.Username)}) ({level.UserId})", $"**Gravity:** {level.Gravity}", $"**Cowboy Chance:** {level.CowboyChance}%") +
                                        $"\n{string.Format("{0,-20} {1,-20} {2, 20}", $"**Version:** {level.Version}", $"**Max Time:** {(level.MaxTime == 0 ? "Infinite" : level.MaxTime.ToString())}", $"**Block Count:** {blockCount.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Min Rank:** {level.MinLevel}", $"**Song:** {level.Song}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Plays:** {level.PlayCount}", $"**Pass:** {(level.HasPass ? "Yes" : "No")}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Rating:** {level.Rating}", $"**Items:** {string.Join(", ", itemList)}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Updated:** {date.Day}/{date:MMM}/{date.Year} - {date.TimeOfDay}", $"**Game Mode:** {level.GameMode}")}";
                                    if (level.Note != null)
                                    {
                                        embed.Description += $"\n-----\n{Format.Sanitize(level.Note)}";
                                    }
                                    await context.Channel.SendMessageAsync("", false, embed.Build());
                                }
                            }
                        }
                    }
                    else if (mode.Equals("id"))
                    {
                        if (!int.TryParse(search, out int id) || id < 1)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the level ID must be a positive whole number.");
                        }
                        else
                        {
                            string text = null;
                            try
                            {
                                text = await web.GetStringAsync("https://pr2hub.com/levels/" + id + ".txt?");
                            }
                            catch (HttpRequestException e)
                            {
                                if (e.Message.Equals("Response status code does not indicate success: 404 (Not Found)."))
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: The level that ID does not exist or could not be found.");
                                }
                                else
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                                }
                            }
                            if (text != null)
                            {
                                string data = text.Substring(text.IndexOf("&data=") + 6);
                                int blockCount = data.Split("`").ElementAt(2).Count(x => x == ',') + 1;
                                text = await web.GetStringAsync("https://pr2hub.com/level_data.php?level_id=" + id + "&token=" + pr2token);
                                PR2Level level = JsonConvert.DeserializeObject<PR2Level>(text);
                                while (!level.Success && level.Error.Equals("Slow down a bit, yo."))
                                {
                                    text = await web.GetStringAsync("https://pr2hub.com/level_data.php?level_id=" + id + "&token=" + pr2token);
                                    level = JsonConvert.DeserializeObject<PR2Level>(text);
                                }
                                if (!level.Success)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {level.Error}");
                                }
                                else
                                {
                                    level.GameMode = CultureInfo.CreateSpecificCulture("en-GB").TextInfo.ToTitleCase(level.GameMode);

                                    switch (level.Song)
                                    {
                                        case "0":
                                            {
                                                level.Song = "None";
                                                break;
                                            }
                                        case "1":
                                            {
                                                level.Song = "Miniature Fantasy - Dreamscaper";
                                                break;
                                            }
                                        case "2":
                                            {
                                                level.Song = "Under Fire - AP";
                                                break;
                                            }
                                        case "3":
                                            {
                                                level.Song = "Paradise on E - API";
                                                break;
                                            }
                                        case "4":
                                            {
                                                level.Song = "Crying Soul - Bounc3";
                                                break;
                                            }
                                        case "5":
                                            {
                                                level.Song = "My Vision - MrMaestro";
                                                break;
                                            }
                                        case "6":
                                            {
                                                level.Song = "Switchblade - SKAzini";
                                                break;
                                            }
                                        case "7":
                                            {
                                                level.Song = "The Wires - Cheez-R-Us";
                                                break;
                                            }
                                        case "8":
                                            {
                                                level.Song = "Before Mydnite - F-777";
                                                break;
                                            }
                                        case "10":
                                            {
                                                level.Song = "Broked It - SWiTCH";
                                                break;
                                            }
                                        case "11":
                                            {
                                                level.Song = "Hello? - TMM43";
                                                break;
                                            }
                                        case "12":
                                            {
                                                level.Song = "Pyrokinesis - Sean Tucker";
                                                break;
                                            }
                                        case "13":
                                            {
                                                level.Song = "Flowerz 'n' Herbz - Brunzolaitis";
                                                break;
                                            }
                                        case "14":
                                            {
                                                level.Song = "Instrumental #4 - Reasoner";
                                                break;
                                            }
                                        case "15":
                                            {
                                                level.Song = "Prismatic - Lunanova";
                                                break;
                                            }
                                        default:
                                            {
                                                level.Song = "Random";
                                                break;
                                            }
                                    }
                                    DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    DateTime date = start.AddSeconds(level.Time).ToLocalTime();
                                    List<string> itemList = level.Items.Split('`').ToList();
                                    if (int.TryParse(itemList.First(), out int result))
                                    {
                                        List<string> itemList2 = new List<string>();
                                        foreach (string item in itemList)
                                        {
                                            if (item.Equals("1"))
                                            {
                                                itemList2.Add("Laser Gun");
                                            }
                                            else if (item.Equals("2"))
                                            {
                                                itemList2.Add("Mine");
                                            }
                                            else if (item.Equals("3"))
                                            {
                                                itemList2.Add("Lightning");
                                            }
                                            else if (item.Equals("4"))
                                            {
                                                itemList2.Add("Teleport");
                                            }
                                            else if (item.Equals("5"))
                                            {
                                                itemList2.Add("Super Jump");
                                            }
                                            else if (item.Equals("6"))
                                            {
                                                itemList2.Add("Jet Pack");
                                            }
                                            else if (item.Equals("7"))
                                            {
                                                itemList2.Add("Speed Burst");
                                            }
                                            else if (item.Equals("8"))
                                            {
                                                itemList2.Add("Sword");
                                            }
                                            else if (item.Equals("9"))
                                            {
                                                itemList2.Add("Ice Wave");
                                            }
                                        }
                                        itemList = itemList2;
                                    }
                                    if (itemList.First().Length < 1)
                                    {
                                        itemList[0] = "None";
                                    }

                                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                    {
                                        Name = $"-- {level.Title} ({id}) --",
                                        Url = "https://pr2hub.com/levels/" + id + ".txt?version=" + level.Version
                                    };
                                    EmbedBuilder embed = new EmbedBuilder()
                                    {
                                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                        Author = author
                                    };
                                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                    {
                                        IconUrl = context.User.GetAvatarUrl(),
                                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                    };
                                    embed.WithFooter(footer);
                                    embed.WithCurrentTimestamp();
                                    embed.Description = string.Format("{0,-20} {1,-20} {2, 20}", $"**By:** [{Format.Sanitize(level.Username)}](https://pr2hub.com/player_search.php?name={Uri.EscapeDataString(level.Username)}) ({level.UserId})", $"**Gravity:** {level.Gravity}", $"**Cowboy Chance:** {level.CowboyChance}%") +
                                        $"\n{string.Format("{0,-20} {1,-20} {2, 20}", $"**Version:** {level.Version}", $"**Max Time:** {(level.MaxTime == 0 ? "Infinite" : level.MaxTime.ToString())}", $"**Block Count:** {blockCount.ToString("N0", CultureInfo.CreateSpecificCulture("en-GB"))}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Min Rank:** {level.MinLevel}", $"**Song:** {level.Song}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Plays:** {level.PlayCount}", $"**Pass:** {(level.HasPass ? "Yes" : "No")}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Rating:** {level.Rating}", $"**Items:** {string.Join(", ", itemList)}")}" +
                                        $"\n{string.Format("{0,-25} {1,25}", $"**Updated:** {date.Day}/{date:MMM}/{date.Year} - {date.TimeOfDay}", $"**Game Mode:** {level.GameMode}")}";
                                    if (level.Note != null)
                                    {
                                        embed.Description += $"\n-----\n{Format.Sanitize(level.Note)}";
                                    }
                                    await context.Channel.SendMessageAsync("", false, embed.Build());
                                }
                            }
                        }
                    }
                    else if (mode.Equals("user"))
                    {
                        PR2User pr2user = null;
                        string pr2name = search;
                        if (context.Message.MentionedUsers.Count > 0)
                        {
                            int argPos = 0;
                            if (!(context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count == 1))
                            {
                                SocketUser user = null;
                                if ((context.Channel is IDMChannel && context.Message.HasStringPrefix("/", ref argPos)) || (context.Channel is SocketTextChannel && context.Message.HasStringPrefix(Guild.Get(context.Guild).Prefix, ref argPos)))
                                {
                                    user = context.Message.MentionedUsers.First();
                                }
                                else if (context.Message.HasMentionPrefix(context.Client.CurrentUser, ref argPos) && context.Message.MentionedUsers.Count > 1)
                                {
                                    user = context.Message.MentionedUsers.ElementAt(1);
                                }
                                if (user == null)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That Discord user does not exist or could not be found.");
                                    web.Dispose();
                                    return;
                                }
                                if (!User.Exists(user))
                                {
                                    User.Add(user);
                                }
                                pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                                if (pr2name == null)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: That user has not linked their PR2 account.");
                                    web.Dispose();
                                    return;
                                }
                            }
                        }
                        try
                        {
                            pr2user = JsonConvert.DeserializeObject<PR2User>(await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + Uri.EscapeDataString(pr2name)));
                        }
                        catch (HttpRequestException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                        }
                        if (pr2user != null)
                        {
                            while (!pr2user.Success && pr2user.Error.Equals("Slow down a bit, yo."))
                            {
                                await Task.Delay(500);
                                pr2user = JsonConvert.DeserializeObject<PR2User>(await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + Uri.EscapeDataString(pr2name)));
                            }
                            if (!pr2user.Success)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {pr2user.Error}");
                            }
                            else
                            { 
                                values["mode"] = "user";
                                values["search_str"] = pr2name;
                                values["order"] = "date";
                                FormUrlEncodedContent content = new FormUrlEncodedContent(values);
                                HttpResponseMessage response = null;
                                try
                                {
                                    response = await web.PostAsync("https://pr2hub.com/search_levels.php?", content);
                                }
                                catch (HttpRequestException)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                                }
                                if (response != null)
                                {
                                    string responseString = await response.Content.ReadAsStringAsync();
                                    if (responseString.StartsWith("error="))
                                    {
                                        await context.Channel.SendMessageAsync($"{context.User.Mention} Error: {responseString.Split('=').Last()}");
                                    }
                                    else
                                    {
                                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                        {
                                            Name = $"Levels By: {Uri.UnescapeDataString(pr2user.Name)} - Page 1",
                                        };
                                        EmbedBuilder embed = new EmbedBuilder()
                                        {
                                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                            Author = author
                                        };
                                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                        {
                                            IconUrl = context.User.GetAvatarUrl(),
                                            Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                        };
                                        embed.WithFooter(footer);
                                        embed.WithCurrentTimestamp();
                                        bool moreLevels = true;
                                        int page = 1;
                                        while (moreLevels)
                                        {
                                            List<string> levels = responseString.Split('&').ToList();
                                            string ids = "", titles = "";
                                            for (int i = 0; i < 6; i++)
                                            {
                                                if (levels.Find(x => x.StartsWith("levelID" + i + "=")) == null)
                                                {
                                                    break;
                                                }
                                                PR2Level level = new PR2Level(true)
                                                {
                                                    Id = int.Parse(levels.Find(x => x.StartsWith("levelID" + i + "=")).Substring(9)),
                                                    Title = Uri.UnescapeDataString(levels.Find(x => x.StartsWith("title" + i + "=")).Substring(7).Replace("+", " ")),
                                                };
                                                ids += level.Id + "\n";
                                                titles += level.Title + "\n";
                                            }
                                            if (ids.Length > 0)
                                            {
                                                embed.AddField(y =>
                                                {
                                                    y.Name = "Title";
                                                    y.Value = titles;
                                                    y.IsInline = true;
                                                });
                                                embed.AddField(y =>
                                                {
                                                    y.Name = "ID";
                                                    y.Value = ids;
                                                    y.IsInline = true;
                                                });
                                                await context.Channel.SendMessageAsync("", false, embed.Build());
                                            }
                                            if (context.Channel is IDMChannel)
                                            {
                                                embed.Fields.Clear();
                                                page++;
                                                embed.Author.Name = $"Levels By: {search} - Page {page}";
                                                values["page"] = page.ToString();
                                                content = new FormUrlEncodedContent(values);
                                                response = await web.PostAsync("https://pr2hub.com/search_levels.php?", content);
                                                responseString = await response.Content.ReadAsStringAsync();
                                                while (responseString.Equals("error=Slow down a bit, yo."))
                                                {
                                                    await Task.Delay(250);
                                                    response = await web.PostAsync("https://pr2hub.com/search_levels.php?", content);
                                                    responseString = await response.Content.ReadAsStringAsync();
                                                }
                                                if (page > 18)
                                                {
                                                    moreLevels = false;
                                                    await context.Channel.SendMessageAsync($"{context.User.Mention} the maximum number of pages I can show is 18.");
                                                }
                                            }
                                            else
                                            {
                                                moreLevels = false;
                                            }
                                        }
                                    }
                                }
                                content.Dispose();
                            }
                        }
                    }

                    web.Dispose();
                }
            }
        }

        public async Task CampaignAsync(SocketCommandContext context, string page)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (page != null && !int.TryParse(page, out int num))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /campaign";
                        embed.Description = "**Description:** Get levels on a page of Campaign.\n**Usage:** /campaign [page]\n**Example:** /campaign 1";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}campaign";
                        embed.Description = $"**Description:** Get levels on a page of Campaign.\n**Usage:** {prefix}campaign [page]\n**Example:** {prefix}campaign 1";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {                   
                    if (page == null)
                    {
                        page = "1";
                    }
                    num = int.Parse(page);
                    if (num < 1 || num > 6)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Campaign pages are numbered 1-6.");
                    }
                    else
                    {
                        HttpClient web = new HttpClient();
                        string text = null;
                        try
                        {
                            text = await web.GetStringAsync("https://pr2hub.com/files/lists/campaign/" + page);
                        }
                        catch (HttpRequestException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                        }
                        web.Dispose();
                        if (text != null)
                        {
                            string ids = "", titles = "", usernames = "";
                            for (int i = 0; i < 9; i++)
                            {
                                string levelId = Extensions.GetBetween(text, "levelID" + i + "=", "&version" + i + "=");
                                if (levelId.Length < 1)
                                {
                                    break;
                                }
                                string title = Uri.UnescapeDataString(Extensions.GetBetween(text, "&title" + i + "=", "&rating" + i + "=").Replace("+", " "));
                                string username = Uri.UnescapeDataString(Extensions.GetBetween(text, "&userName" + i + "=", "&group" + i + "=")).Replace("+", " ");
                                ids += levelId + "\n";
                                titles += title + "\n";
                                usernames += username + "\n";
                            }
                            if (ids.Length < 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} There are no levels on that page of Campaign.");
                            }
                            else
                            {
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"Campaign - Page {page}",
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                    Author = author
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = context.User.GetAvatarUrl(),
                                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                };
                                embed.WithFooter(footer);
                                embed.WithCurrentTimestamp();
                                embed.AddField(y =>
                                {
                                    y.Name = "Title";
                                    y.Value = titles;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "Username";
                                    y.Value = usernames;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "ID";
                                    y.Value = ids;
                                    y.IsInline = true;
                                });
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                }
            }
        }

        public async Task AllTimeBestAsync(SocketCommandContext context, string page)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (page != null && !int.TryParse(page, out int num))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /alltimebest";
                        embed.Description = "**Description:** Get levels on a page of All Time Best.\n**Usage:** /alltimebest [page]\n**Example:** /alltimebest 1";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}alltimebest";
                        embed.Description = $"**Description:** Get levels on a page of All Time Best.\n**Usage:** {prefix}alltimebest [page]\n**Example:** {prefix}alltimebest 1";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (page == null)
                    {
                        page = "1";
                    }
                    num = int.Parse(page);
                    if (num < 1 || num > 9)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} All Time Best pages are numbered 1-9.");
                    }
                    else
                    {
                        HttpClient web = new HttpClient();
                        string text = null;
                        try
                        {
                            text = await web.GetStringAsync("https://pr2hub.com/files/lists/best/" + page);
                        }
                        catch (HttpRequestException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                        }
                        web.Dispose();
                        if (text != null)
                        {
                            string ids = "", titles = "", usernames = "";
                            for (int i = 0; i < 9; i++)
                            {
                                string levelId = Extensions.GetBetween(text, "levelID" + i + "=", "&version" + i + "=");
                                if (levelId.Length < 1)
                                {
                                    break;
                                }
                                string title = Uri.UnescapeDataString(Extensions.GetBetween(text, "&title" + i + "=", "&rating" + i + "=").Replace("+", " "));
                                string username = Uri.UnescapeDataString(Extensions.GetBetween(text, "&userName" + i + "=", "&group" + i + "=")).Replace("+", " ");
                                ids += levelId + "\n";
                                titles += title + "\n";
                                usernames += username + "\n";
                            }
                            if (ids.Length < 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} There are no levels on that page of All Time Best.");
                            }
                            else
                            {
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"All Time Best - Page {page}",
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                    Author = author
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = context.User.GetAvatarUrl(),
                                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                };
                                embed.WithFooter(footer);
                                embed.WithCurrentTimestamp();
                                embed.AddField(y =>
                                {
                                    y.Name = "Title";
                                    y.Value = titles;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "Username";
                                    y.Value = usernames;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "ID";
                                    y.Value = ids;
                                    y.IsInline = true;
                                });
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                }
            }
        }

        public async Task TodaysBestAsync(SocketCommandContext context, string page)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (page != null && !int.TryParse(page, out int num))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /todaysbest";
                        embed.Description = "**Description:** Get levels on a page of Today's Best.\n**Usage:** /todaysbest [page]\n**Example:** /todaysbest 1";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}todaysbest";
                        embed.Description = $"**Description:** Get levels on a page of Today's Best.\n**Usage:** {prefix}todaysbest [page]\n**Example:** {prefix}todaysbest 1";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (page == null)
                    {
                        page = "1";
                    }
                    num = int.Parse(page);
                    if (num < 1 || num > 9)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Today's Best pages are numbered 1-9.");
                    }
                    else
                    {
                        HttpClient web = new HttpClient();
                        string text = null;
                        try
                        {
                            text = await web.GetStringAsync("https://pr2hub.com/files/lists/best_today/" + page);
                        }
                        catch (HttpRequestException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                        }
                        web.Dispose();
                        if (text != null)
                        {
                            string ids = "", titles = "", usernames = "";
                            for (int i = 0; i < 9; i++)
                            {
                                string levelId = Extensions.GetBetween(text, "levelID" + i + "=", "&version" + i + "=");
                                if (levelId.Length < 1)
                                {
                                    break;
                                }
                                string title = Uri.UnescapeDataString(Extensions.GetBetween(text, "&title" + i + "=", "&rating" + i + "=").Replace("+", " "));
                                string username = Uri.UnescapeDataString(Extensions.GetBetween(text, "&userName" + i + "=", "&group" + i + "=")).Replace("+", " ");
                                ids += levelId + "\n";
                                titles += title + "\n";
                                usernames += username + "\n";
                            }
                            if (ids.Length < 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} There are no levels on that page of Today's Best.");
                            }
                            else
                            {
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"Today's Best - Page {page}",
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                    Author = author
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = context.User.GetAvatarUrl(),
                                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                };
                                embed.WithFooter(footer);
                                embed.WithCurrentTimestamp();
                                embed.AddField(y =>
                                {
                                    y.Name = "Title";
                                    y.Value = titles;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "Username";
                                    y.Value = usernames;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "ID";
                                    y.Value = ids;
                                    y.IsInline = true;
                                });
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                }
            }
        }

        public async Task NewestAsync(SocketCommandContext context, string page)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (page != null && !int.TryParse(page, out int num))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    if (context.Channel is IDMChannel)
                    {
                        embed.Title = "Command: /newest";
                        embed.Description = "**Description:** Get levels on a page of Newest.\n**Usage:** /newest [page]\n**Example:** /newest 1";
                    }
                    else
                    {
                        string prefix = Guild.Get(context.Guild).Prefix;
                        embed.Title = $"Command: {prefix}newest";
                        embed.Description = $"**Description:** Get levels on a page of Newest.\n**Usage:** {prefix}newest [page]\n**Example:** {prefix}newest 1";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (page == null)
                    {
                        page = "1";
                    }
                    num = int.Parse(page);
                    if (num < 1 || num > 9)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} Newest pages are numbered 1-9.");
                    }
                    else
                    {
                        HttpClient web = new HttpClient();
                        string text = null;
                        try
                        {
                            text = await web.GetStringAsync("https://pr2hub.com/files/lists/newest/" + page);
                        }
                        catch (HttpRequestException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                        }
                        web.Dispose();
                        if (text != null)
                        {
                            string ids = "", titles = "", usernames = "";
                            for (int i = 0; i < 9; i++)
                            {
                                string levelId = Extensions.GetBetween(text, "levelID" + i + "=", "&version" + i + "=");
                                if (levelId.Length < 1)
                                {
                                    break;
                                }
                                string title = Uri.UnescapeDataString(Extensions.GetBetween(text, "&title" + i + "=", "&rating" + i + "=").Replace("+", " "));
                                string username = Uri.UnescapeDataString(Extensions.GetBetween(text, "&userName" + i + "=", "&group" + i + "=")).Replace("+", " ");
                                ids += levelId + "\n";
                                titles += title + "\n";
                                usernames += username + "\n";
                            }
                            if (ids.Length < 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} There are no levels on that page of Newest.");
                            }
                            else
                            {
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = $"Newest - Page {page}",
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                    Author = author
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = context.User.GetAvatarUrl(),
                                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                                };
                                embed.WithFooter(footer);
                                embed.WithCurrentTimestamp();
                                embed.AddField(y =>
                                {
                                    y.Name = "Title";
                                    y.Value = titles;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "Username";
                                    y.Value = usernames;
                                    y.IsInline = true;
                                });
                                embed.AddField(y =>
                                {
                                    y.Name = "ID";
                                    y.Value = ids;
                                    y.IsInline = true;
                                });
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                            }
                        }
                    }
                }
            }
        }

        public async Task VerifyGuildAsync(SocketCommandContext context)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (!User.Exists(context.User))
                {
                    User.Add(context.User);
                }
                string pr2name = User.GetUser("user_id", context.User.Id.ToString()).PR2Name;
                if (pr2name == null)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify your PR2 account to use this command.");
                    return;
                }
                HttpClient web = new HttpClient();
                string userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name);
                string guild = Regex.Unescape(Extensions.GetBetween(userinfo, "\",\"guildName\":\"", "\",\"name\":\""));
                string id = Extensions.GetBetween(userinfo, "\",\"userId\":\"", "\",\"hatColor2\":");
                string guildinfo = await web.GetStringAsync("https://pr2hub.com/guild_info.php?getMembers=yes&name=" + guild);
                string owner = Extensions.GetBetween(guildinfo, "\",\"owner_id\":\"", "\",\"note\":\"");
                web.Dispose();
                if (id.Equals(owner))
                {
                    int memberCount = int.Parse(Extensions.GetBetween(guildinfo, "\",\"member_count\":\"", "\",\"emblem\":\""));
                    if (memberCount < 15)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you need at least 15 members in your guild.");
                        return;
                    }
                    IReadOnlyCollection<SocketRole> roles = context.Guild.Roles;
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
                        Discord.Rest.RestRole guildRole = await context.Guild.CreateRoleAsync(guild, default, default, default, default);
                        SocketRole everyoneRole = context.Guild.EveryoneRole;
                        SocketRole mutedRole = context.Guild.Roles.Where(x => x.Name.ToUpper() == "Muted".ToUpper()).First();
                        Discord.Rest.RestTextChannel guildChannel = await context.Guild.CreateTextChannelAsync(guild.Replace(" ", "-"));
                        await guildChannel.ModifyAsync(x => x.CategoryId = 528704611289399296);
                        await guildChannel.AddPermissionOverwriteAsync(guildRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Inherit));
                        await guildChannel.AddPermissionOverwriteAsync(mutedRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Deny));
                        await guildChannel.AddPermissionOverwriteAsync(everyoneRole, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny));
                        Discord.Rest.RestVoiceChannel guildVoice = await context.Guild.CreateVoiceChannelAsync(guild);
                        await guildVoice.ModifyAsync(x => x.CategoryId = 528704611289399296);
                        await guildVoice.AddPermissionOverwriteAsync(guildRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Allow));
                        await guildVoice.AddPermissionOverwriteAsync(mutedRole, OverwritePermissions.InheritAll.Modify(PermValue.Deny, PermValue.Inherit, PermValue.Inherit, PermValue.Allow, PermValue.Deny, speak: PermValue.Deny));
                        await guildVoice.AddPermissionOverwriteAsync(everyoneRole, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny));
                        IGuildUser user = context.Guild.GetUser(context.User.Id);
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = "Adding Guild Role"
                        };
                        await user.AddRoleAsync(guildRole, options);
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} your guild already has a guild channel.");
                        return;
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you are not the owner of this guild.");
                    return;
                }
            }
            else
            {
                return;
            }
        }

        public async Task JoinGuildAsync(SocketCommandContext context)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (!User.Exists(context.User))
                {
                    User.Add(context.User);
                }
                string pr2name = User.GetUser("user_id", context.User.Id.ToString()).PR2Name;
                if (pr2name == null)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to verify your PR2 account to use this command.");
                    return;
                }
                HttpClient web = new HttpClient();
                string userinfo = await web.GetStringAsync("https://pr2hub.com/get_player_info.php?name=" + pr2name);
                string guild = Regex.Unescape(Extensions.GetBetween(userinfo, "\",\"guildName\":\"", "\",\"name\":\""));
                IReadOnlyCollection<SocketRole> roles = context.Guild.Roles;
                web.Dispose();
                foreach (IRole role in roles)
                {
                    if (role.Name == guild)
                    {
                        SocketGuildUser user = context.User as SocketGuildUser;
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = "Adding Guild Role"
                        };
                        await user.AddRoleAsync(role, options);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have been added to the guild role **{Format.Sanitize(guild)}**.");
                        break;
                    }
                }
                await context.Channel.SendMessageAsync($"{context.User.Mention} your guild does not have a guild channel.");
            }
            else
            {
                return;
            }
        }

        public async Task ServersAsync(SocketCommandContext context, [Remainder] string s1)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                if (s1 != null)
                {
                    await StatsAsync(context, s1);
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
                    await context.Channel.SendMessageAsync($"{context.User.Mention} Error: Connection to PR2 Hub was not successfull.");
                }
                web.Dispose();
                if (text != null)
                {
                    PR2ServerList servers = JsonConvert.DeserializeObject<PR2ServerList>(text);
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
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    embed.Description = "";
                    foreach (PR2Server server in servers.Servers)
                    {
                        embed.Description += server.ToString() + "\n";
                    }
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        public async Task StaffAsync(SocketCommandContext context)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            if (context.Channel is SocketTextChannel)
            {
                channels = AllowedChannel.Get(context.Guild.Id);
            }
            if (context.Channel is IDMChannel || channels.Count() <= 0 || channels.Where(x => x.ChannelID.ToString() == context.Channel.Id.ToString()).Count() > 0)
            {
                HttpClient web = new HttpClient();
                string text = null;
                try
                {
                    text = await web.GetStringAsync("https://pr2hub.com/staff.php");
                }
                catch (HttpRequestException)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} connection to PR2 Hub was not successfull.");
                }
                if (text != null)
                {
                    while (text.Contains("Error: Please wait at least 10 seconds before refreshing the page again."))
                    {
                        await Task.Delay(500);
                        text = await web.GetStringAsync("https://pr2hub.com/staff.php");
                    }
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
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is no PR2 Staff online currently.");
                        return;
                    }
                    else
                    {
                        embed.Description = embed.Description.TrimEnd(new char[] { ' ', ',', ',' });
                        await context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
                web.Dispose();
            }
            else
            {
                return;
            }
        }
    }
}
