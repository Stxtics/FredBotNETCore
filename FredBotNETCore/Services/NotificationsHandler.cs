﻿using Discord;
using Discord.WebSocket;
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

                    #region Check Members Downloaded

                    //foreach (var guild in _client.Guilds)
                    //{
                    //    if (!guild.HasAllMembers)
                    //    {
                    //        await guild.DownloadUsersAsync();
                    //    }
                    //}

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
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = "Announcing happy hour on " + Name
                            };
                            await role.ModifyAsync(x => x.Mentionable = true, options);
                            await channel.SendMessageAsync($"{role.Mention} A happy hour has just started on Server: {Name}");
                            await role.ModifyAsync(x => x.Mentionable = false, options);
                        }
                    }
                }
            }
        }

        public async Task AnnouceHintUpdatedAsync(string hint, bool newArti = false)
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
                    SocketRole arti = guild.Roles.Where(x => x.Name.ToUpper() == "Arti".ToUpper()).FirstOrDefault();
                    SocketRole updates = guild.Roles.Where(x => x.Name.ToUpper() == "ArtiUpdates".ToUpper()).FirstOrDefault();
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = "Announcing new artifact"
                    };
                    if (newArti)
                    {
                        if (arti != null)
                        {
                            await arti.ModifyAsync(x => x.Mentionable = true, options);
                            await channel.SendMessageAsync($"{arti.Mention} Hmm... I seem to have misplaced the artifact. Maybe you can help me find it?\n" +
                                    $"Here's what I remember: **{Format.Sanitize(Uri.UnescapeDataString(hint))}**. Maybe I can remember more later!!");
                            await arti.ModifyAsync(x => x.Mentionable = false, options);
                        }
                    }
                    else
                    {
                        if (updates != null)
                        {
                            options.AuditLogReason = "Announcing hint update";
                            await updates.ModifyAsync(x => x.Mentionable = true, options);
                            await channel.SendMessageAsync($"{updates.Mention} Artifact hint updated. New hint: **{Format.Sanitize(Uri.UnescapeDataString(hint))}**");
                            await updates.ModifyAsync(x => x.Mentionable = false, options);
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
                        await channel.SendMessageAsync($"**{Format.Sanitize(finder)}** has found the artifact and has received a bubble set!");
                    }
                    else
                    {
                        await channel.SendMessageAsync($"**{Format.Sanitize(finder)}** has found the artifact!");
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
    }
}
