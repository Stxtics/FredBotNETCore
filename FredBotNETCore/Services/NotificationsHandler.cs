using Discord;
using Discord.WebSocket;
using FredBotNETCore.Models;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FredBotNETCore
{
    public class NotificationsHandler
    {
        private readonly DiscordSocketClient _client;

        public NotificationsHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task CheckStatus()
        {
            HttpClient web = new HttpClient();
            PR2ArtifactHint previousHint = JsonConvert.DeserializeObject<PR2ArtifactHint>(await web.GetStringAsync("https://pr2hub.com/files/level_of_the_week.json"));

#if !DEBUG

            while (true)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
#region HH
                    string status = await web.GetStringAsync("https://pr2hub.com/files/server_status_2.txt");
                    string[] servers = status.Split('}');
                    string happyHour = "", guildId = "";

                    foreach (string server_name in servers)
                    {
                        guildId = Extensions.GetBetween(server_name, "guild_id\":\"", "\"");
                        if (guildId.Equals("0"))
                        {
                            happyHour = Extensions.GetBetween(server_name + "}", "happy_hour\":", "}");
                            string serverName = Extensions.GetBetween(server_name, "server_name\":\"", "\"");
                            if (!serverName.Equals("Tournament"))
                            {
                                if (happyHour.Equals("1"))
                                {
                                    await CheckStatusAsync(true, serverName);
                                }
                                else
                                {
                                    await CheckStatusAsync(false, serverName);
                                }
                            }
                        }
                    }

#endregion

#region Arti
                    PR2ArtifactHint currentHint = JsonConvert.DeserializeObject<PR2ArtifactHint>(await web.GetStringAsync("https://pr2hub.com/files/level_of_the_week.json"));
                    if (previousHint.Current.Level.ToString() != currentHint.Current.Level.ToString())
                    {
                        if (previousHint.Current.SetTime != currentHint.Current.SetTime)
                        {
                            await AnnouceCurrentArtiAsync(currentHint.Current.Level.ToString());
                        }
                    }
                    if (previousHint.Current.FirstFinder?.Name != currentHint.Current.FirstFinder?.Name && currentHint.Current.FirstFinder != null)
                    {
                        if (currentHint.Current.FirstFinder.Name == currentHint.Current.BubblesWinner?.Name)
                        {
                            await AnnounceArtifactFoundAsync(currentHint.Current.FirstFinder.Name, true);
                        }
                        else
                        {
                            await AnnounceArtifactFoundAsync(currentHint.Current.FirstFinder.Name);
                        }
                    }
                    if (previousHint.Current.BubblesWinner?.Name != currentHint.Current.BubblesWinner?.Name && currentHint.Current.BubblesWinner?.Name.Length > 0)
                    {
                        await AnnounceBubblesAwardedAsync(currentHint.Current.BubblesWinner.Name);
                    }
                    if (previousHint.Scheduled?.UpdatedTime != currentHint.Scheduled?.UpdatedTime && currentHint.Scheduled != null)
                    {
                        await AnnounceScheduledArtiAsync(currentHint.Scheduled);
                    }
                    previousHint = currentHint;
#endregion

#region Check Members Downloaded

                    foreach (var guild in _client.Guilds)
                    {
                        if (!guild.HasAllMembers)
                        {
                            await guild.DownloadUsersAsync();
                        }
                    }

#endregion
                }
                catch (HttpRequestException)
                {
                    //failed to connect
                }
                catch (Exception e)
                {
                    await Extensions.LogError(_client, e.Message + e.StackTrace);
                }
            }
#endif
        }

        public string Name;

        public string hint;

        public string CheckHint
        {
            get => hint;
            set
            {
                if (value == hint)
                {
                    return;
                }

                if (value.Contains("\"happy_hour\":\"1\""))
                {
                    hint = value;
                }
            }
        }

        public bool justConnected;

        public string DerronStatus = "";
        public string CarinaStatus = "";
        public string GrayanStatus = "";
        public string FitzStatus = "";
        public string LokiStatus = "";
        public string PromieStatus = "";
        public string MorganaStatus = "";
        public string AndresStatus = "";
        public string IsabelStatus = "";

        public async Task CheckStatusAsync(bool isOn = false, string serverName = null)
        {
            string compare = isOn + serverName;
            switch (serverName)
            {
                case "Derron":
                    if (DerronStatus == compare)
                    {
                        return;
                    }

                    DerronStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Carina":
                    if (CarinaStatus == compare)
                    {
                        return;
                    }

                    CarinaStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Grayan":
                    if (GrayanStatus == compare)
                    {
                        return;
                    }

                    GrayanStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Fitz":
                    if (FitzStatus == compare)
                    {
                        return;
                    }

                    FitzStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Loki":
                    if (LokiStatus == compare)
                    {
                        return;
                    }

                    LokiStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Promie":
                    if (PromieStatus == compare)
                    {
                        return;
                    }

                    PromieStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Morgana":
                    if (MorganaStatus == compare)
                    {
                        return;
                    }

                    MorganaStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Andres":
                    if (AndresStatus == compare)
                    {
                        return;
                    }

                    AndresStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;

                case "Isabel":
                    if (IsabelStatus == compare)
                    {
                        return;
                    }

                    IsabelStatus = compare;
                    if (justConnected == true)
                    {
                        justConnected = false;
                        return;
                    }
                    await UpdateHappyHourAsync(serverName, isOn);
                    break;
            }
        }

        private async Task UpdateHappyHourAsync(string Name, bool isOn = false)
        {
            if (isOn)
            {
                Process process = Process.GetCurrentProcess();
                TimeSpan time = DateTime.Now - process.StartTime;
                if (time.Minutes < 2)
                {
                    return;
                }
                foreach (SocketGuild guild in _client.Guilds)
                {
                    SocketRole role = guild.Roles.Where(x => x.Name.ToUpper() == "HH".ToUpper()).FirstOrDefault();
                    if (role != null)
                    {
                        SocketTextChannel channel = Extensions.GetNotificationsChannel(guild);
                        if (channel != null)
                        {
                            if (channel.Guild.CurrentUser.GetPermissions(channel).MentionEveryone == false)
                            {
                                await channel.SendMessageAsync($"I am missing permission: Mention all roles.");
                            }
                            else
                            {
                                await channel.SendMessageAsync($"{role.Mention} A happy hour has just started on Server: {Name}");
                            }
                        }
                    }
                }
            }
        }

        public async Task AnnouceCurrentArtiAsync(string hint)
        {
            if (hint.Length < 1)
            {
                return;
            }
            foreach (SocketGuild guild in _client.Guilds)
            {
                SocketTextChannel channel = Extensions.GetNotificationsChannel(guild);
                if (channel != null)
                {
                    if (channel.Guild.CurrentUser.GetPermissions(channel).MentionEveryone == false)
                    {
                        await channel.SendMessageAsync($"I am missing permission: Mention all roles.");
                    }
                    else
                    {
                        SocketRole arti = guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).FirstOrDefault();
                        if (arti != null)
                        {
                            await channel.SendMessageAsync($"{arti.Mention} The current level of the week has just been updated.\nIt is now at **{Format.Sanitize(Uri.UnescapeDataString(hint))}**.");
                        }
                    }
                }
            }
        }

        public async Task AnnounceArtifactFoundAsync(string finder, bool bubbles = false)
        {
            foreach (SocketGuild guild in _client.Guilds)
            {
                SocketTextChannel channel = Extensions.GetNotificationsChannel(guild);
                if (channel != null)
                {
                    if (bubbles)
                    {
                        await channel.SendMessageAsync($"**{Format.Sanitize(finder)}** has found the hidden artifact and has received a bubble set!");
                    }
                    else
                    {
                        await channel.SendMessageAsync($"**{Format.Sanitize(finder)}** has found the hidden artifact!");
                    }
                }
            }
        }

        public async Task AnnounceBubblesAwardedAsync(string bubbles = null)
        {
            foreach (SocketGuild guild in _client.Guilds)
            {
                SocketTextChannel channel = Extensions.GetNotificationsChannel(guild);
                if (channel != null)
                {
                    await channel.SendMessageAsync($"**{Format.Sanitize(bubbles)}** has been awarded a bubble set!");
                }
            }
        }

        public async Task AnnounceScheduledArtiAsync(Scheduled scheduled)
        {
            foreach (SocketGuild guild in _client.Guilds)
            {
                SocketTextChannel channel = Extensions.GetNotificationsChannel(guild);
                SocketRole artiUpdates = guild.Roles.Where(x => x.Name.ToUpper() == "ArtiUpdates".ToUpper()).FirstOrDefault();
                if (channel != null && artiUpdates != null)
                {
                    if (channel.Guild.CurrentUser.GetPermissions(channel).MentionEveryone == false)
                    {
                        await channel.SendMessageAsync($"I am missing permission: Mention all roles.");
                    }
                    else
                    {
                        DateTime time = DateTimeOffset.FromUnixTimeSeconds(scheduled.SetTime).DateTime;
                        await channel.SendMessageAsync($"{artiUpdates.Mention} The next level of the week will be **{Format.Sanitize(scheduled.Level.ToString())}**, " +
                            $"which will take effect on **{time:MMMM} {time.Day}, {time.Year} at {time.ToLongTimeString()} UTC**.");
                    }
                }
            }
        }
    }
}
