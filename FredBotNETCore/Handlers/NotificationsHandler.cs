using Discord;
using Discord.WebSocket;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FredBotNETCore
{
    public static class NotificationsHandler
    {
        private static DiscordSocketClient _client;
        public static async Task CheckStatus(DiscordSocketClient client)
        {
            _client = client;
            HttpClient web = new HttpClient();
            string hint = Extensions.GetBetween(await web.GetStringAsync("https://pr2hub.com/files/artifact_hint.txt"), "{\"hint\":\"", "\",\"finder_name\":\"");
            string finder = Extensions.GetBetween(await web.GetStringAsync("https://pr2hub.com/files/artifact_hint.txt"), "\",\"finder_name\":\"", "\",\"bubbles_name\":\"");
            string bubbles = Extensions.GetBetween(await web.GetStringAsync("https://pr2hub.com/files/artifact_hint.txt"), "\",\"bubbles_name\":\"", "\",\"updated_time\":");
            string time = Extensions.GetBetween(await web.GetStringAsync("https://pr2hub.com/files/artifact_hint.txt"), "\",\"updated_time\":", "}");
            bool valid = false;
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
                            happyHour = Extensions.GetBetween(server_name, "happy_hour\":\"", "\"");
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
                    string artifactHint = await web.GetStringAsync("https://pr2hub.com/files/artifact_hint.txt");
                    if (!hint.Equals(Extensions.GetBetween(artifactHint, "{\"hint\":\"", "\",\"finder_name\":\"")))
                    {
                        hint = Extensions.GetBetween(artifactHint, "{\"hint\":\"", "\",\"finder_name\":\"");
                        if (!time.Equals(Extensions.GetBetween(artifactHint, "\",\"updated_time\":", "}")))
                        {
                            time = Extensions.GetBetween(artifactHint, "\",\"updated_time\":", "}");
                            valid = true;
                        }
                        if (valid)
                        {
                            await AnnouceHintUpdatedAsync(hint, true);
                            valid = false;
                        }
                        else
                        {
                            await AnnouceHintUpdatedAsync(hint, false);
                        }
                    }
                    if (!finder.Equals(Extensions.GetBetween(artifactHint, "\",\"finder_name\":\"", "\",\"bubbles_name\":\"")))
                    {
                        finder = Extensions.GetBetween(artifactHint, "\",\"finder_name\":\"", "\",\"bubbles_name\":\"");
                        bubbles = Extensions.GetBetween(artifactHint, "\",\"bubbles_name\":\"", "\",\"updated_time\":");
                        if (finder.Length > 0 && finder == bubbles)
                        {
                            await AnnounceArtifactFoundAsync(finder, true);
                        }
                        else if (finder.Length > 0)
                        {
                            await AnnounceArtifactFoundAsync(finder);
                        }
                    }
                    if (!bubbles.Equals(Extensions.GetBetween(artifactHint, "\",\"bubbles_name\":\"", "\",\"updated_time\":")))
                    {
                        bubbles = Extensions.GetBetween(artifactHint, "\",\"bubbles_name\":\"", "\",\"updated_time\":");
                        if (bubbles.Length > 0)
                        {
                            await AnnounceBubblesAwardedAsync(bubbles);
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
                    await Extensions.LogError(client, e.Message + e.StackTrace);
                }
            }
        }

        public static string Name;

        public static string hint;

        public static string CheckHint
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

        public static bool justConnected;

        public static string DerronStatus = "";
        public static string CarinaStatus = "";
        public static string GrayanStatus = "";
        public static string FitzStatus = "";
        public static string LokiStatus = "";
        public static string PromieStatus = "";
        public static string MorganaStatus = "";
        public static string AndresStatus = "";
        public static string IsabelStatus = "";

        public static async Task CheckStatusAsync(bool isOn = false, string serverName = null)
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

        private static async Task UpdateHappyHourAsync(string Name, bool isOn = false)
        {
            if (isOn)
            {
                Process process = Process.GetCurrentProcess();
                TimeSpan time = DateTime.Now - process.StartTime;
                if (time.Minutes < 2)
                {
                    return;
                }
                SocketGuild Guild = _client.GetGuild(528679522707701760);
                SocketRole RoleM = Guild.Roles.Where(x => x.Name.ToUpper() == "HH".ToUpper()).First();
                SocketTextChannel channel = Guild.GetTextChannel(Extensions.GetNotificationsChannel());
                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = "Announcing happy hour on " + Name
                };
                await RoleM.ModifyAsync(x => x.Mentionable = true, options);
                await channel.SendMessageAsync($"{RoleM.Mention} A happy hour has just started on Server: {Name}");
                await RoleM.ModifyAsync(x => x.Mentionable = false, options);
            }
        }

        public static async Task AnnouceHintUpdatedAsync(string hint, bool newArti = false)
        {
            SocketGuild Guild = _client.GetGuild(528679522707701760);
            SocketTextChannel channel = Guild.GetTextChannel(Extensions.GetNotificationsChannel());
            RequestOptions options = new RequestOptions()
            {
                AuditLogReason = "Announcing new artifact"
            };
            if (newArti)
            {
                await Guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).First().ModifyAsync(x => x.Mentionable = true, options);
                await channel.SendMessageAsync($"{Guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).First().Mention} Hmm... I seem to have misplaced the artifact. Maybe you can help me find it?\n" +
                        $"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint))}**. Maybe I can remember more later!!");
                await Guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).First().ModifyAsync(x => x.Mentionable = false, options);
            }
            else
            {
                options.AuditLogReason = "Announcing hint update";
                await Guild.Roles.Where(x => x.Name.ToUpper() == "Arti Updates".ToUpper()).First().ModifyAsync(x => x.Mentionable = true, options);
                await channel.SendMessageAsync($"{Guild.Roles.Where(x => x.Name.ToUpper() == "Arti Updates".ToUpper()).First().Mention} Artifact hint updated. New hint: **{Format.Sanitize(Uri.UnescapeDataString(hint))}**");
                await Guild.Roles.Where(x => x.Name.ToUpper() == "Arti Updates".ToUpper()).First().ModifyAsync(x => x.Mentionable = false, options);
            }
        }

        public static async Task AnnounceArtifactFoundAsync(string finder, bool bubbles = false)
        {
            SocketGuild Guild = _client.GetGuild(528679522707701760);
            SocketTextChannel channel = Guild.GetTextChannel(Extensions.GetNotificationsChannel());
            if (bubbles)
            {
                await channel.SendMessageAsync($"**{Format.Sanitize(finder)}** has found the artifact and has received a bubble set!");
            }
            else
            {
                await channel.SendMessageAsync($"**{Format.Sanitize(finder)}** has found the artifact!");
            }
        }

        public static async Task AnnounceBubblesAwardedAsync(string bubbles = null)
        {
            SocketGuild Guild = _client.GetGuild(528679522707701760);
            SocketTextChannel channel = Guild.GetTextChannel(Extensions.GetNotificationsChannel());
            await channel.SendMessageAsync($"**{Format.Sanitize(bubbles)}** has been awarded a bubble set!");
        }
    }
}
