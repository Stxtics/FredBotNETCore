using System.Threading.Tasks;
using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Linq;
using System;
using FredBotNETCore.Modules.Public;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FredBotNETCore
{
    public class CommandHandler
    {
        private CommandService _cmds;
        public static DiscordSocketClient _client;

        public static string Name;

        public static string hint;

        public static string CheckHint
        {
            get{return hint;}
            set
            {
                if (value == hint)
                    return;

                if (value.Contains("\"happy_hour\":\"1\""))
                    hint = value;
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
            string compare = "";
            compare = isOn + serverName;

            switch (serverName)
            {
                case "Derron":
                    if (DerronStatus == compare)
                        return;

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
                        return;

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
                        return;

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
                        return;

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
                        return;

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
                        return;

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
                        return;

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
                        return;

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
                        return;

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

        private static async Task UpdateHappyHourAsync(string Name = null, bool isOn = false)
        {
            if (isOn)
            {
                var process = Process.GetCurrentProcess();
                var time = DateTime.Now - process.StartTime;
                if (time.Minutes < 2)
                {
                    return;
                }
                SocketGuild Guild = _client.GetGuild(249657315576381450);
                SocketRole RoleM = Guild.GetRole(307631922094407682);
                SocketTextChannel channel = Guild.GetTextChannel(249678944956055562);
                await RoleM.ModifyAsync(x => x.Mentionable = true);
                await channel.SendMessageAsync($"{RoleM.Mention} A happy hour has just started on Server: {Name}");
                await RoleM.ModifyAsync(x => x.Mentionable = false);
            }
        }

        public static async Task AnnouceHintUpdatedAsync(string hint = null, bool newArti = false)
        {
            SocketGuild Guild = _client.GetGuild(249657315576381450);
            SocketTextChannel channel = Guild.GetTextChannel(249678944956055562);
            if (newArti)
            {
                await Guild.GetRole(347312071618330626).ModifyAsync(x => x.Mentionable = true);
                await channel.SendMessageAsync($"{Guild.GetRole(347312071618330626).Mention} Hmm... I seem to have misplaced the artifact. Maybe you can help me find it?\n" +
                        $"Here's what I remember: `{hint}`. Maybe I can remember more later!!");
                await Guild.GetRole(347312071618330626).ModifyAsync(x => x.Mentionable = false);
            }
            else
            {
                await channel.SendMessageAsync($"Artifact hint updated. New hint: `{hint}`");
            }
        }

        public static async Task AnnounceArtifactFoundAsync(string finder = null)
        {
            SocketTextChannel channel = _client.GetChannel(249678944956055562) as SocketTextChannel;
            await channel.SendMessageAsync($"{finder} has found the artifact!");
        }

        public async Task Install(DiscordSocketClient c)
        {
            _client = c;
            _cmds = new CommandService();
            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly(), Program._provider);

            _client.MessageReceived += OnMessageReceived;

            _client.UserJoined += AnnounceUserJoined;
            _client.UserLeft += AnnounceUserLeft;
            _client.UserBanned += AnnounceUserBanned;
            _client.UserUnbanned += AnnounceUserUnbanned;
            _client.MessageDeleted += AnnounceMessageDeleted;
            _client.GuildMemberUpdated += AnnounceGuildMemberUpdated;
            _client.ChannelCreated += AnnounceChannelCreated;
            _client.ChannelDestroyed += AnnounceChannelDestroyed;
            _client.ChannelUpdated += AnnounceChannelUpdated;
            _client.RoleCreated += AnnounceRoleCreated;
            _client.RoleDeleted += AnnounceRoleDeleted;
            _client.RoleUpdated += AnnounceRoleUpdated;
            _client.Ready += async () =>
            {
                int users = 0;
                foreach(SocketGuild guild in _client.Guilds)
                {
                    users = users + guild.MemberCount;
                }
                await _client.SetGameAsync($"/help with {users} users", null, type: ActivityType.Listening);
            };
        }

        #region Log

        public async Task AnnounceRoleUpdated(SocketRole role, SocketRole role2)
        {
            if (role.Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = role.Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Role Updated",
                IconUrl = role.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {role.Id}"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(0, 255, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach(Discord.Rest.RestAuditLogEntry audit in await role.Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.RoleUpdated)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (role.Name != role2.Name)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** renamed the role **{role.Name}** to **{role2.Name}**.";
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            else if (role.IsMentionable != role2.IsMentionable)
            {
                if (role.IsMentionable)
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** made the role **{role.Name}** unmentionable.";
                }
                else
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** made the role **{role.Name}** mentionable.";
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            else if (role.IsHoisted != role2.IsHoisted)
            {
                if (role.IsHoisted)
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** made the role **{role.Name}** not display separately.";
                }
                else
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** made the role **{role.Name}** display separately.";
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            else if (!role.Color.Equals(role2.Color))
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** changed the color of **{role.Name}** from **#{PublicModule.HexConverter(role.Color)}** to **#{PublicModule.HexConverter(role2.Color)}**.";
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task AnnounceRoleDeleted(SocketRole role)
        {
            if (role.Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = role.Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Role Deleted",
                IconUrl = role.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {role.Id}"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(255, 0, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await role.Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.RoleDeleted)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"**{user.Username}#{user.Discriminator}** deleted the role {role.Name}.";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = reason;
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceRoleCreated(SocketRole role)
        {
            if (role.Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = role.Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Role Deleted",
                IconUrl = role.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {role.Id}"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(0, 255, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await role.Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.RoleCreated)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"**{user.Username}#{user.Discriminator}** created the role {role.Name}.";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = reason;
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceChannelUpdated(SocketChannel channel, SocketChannel channel2)
        {
            if ((channel as SocketGuildChannel).Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Channel Updated",
                IconUrl = (channel as SocketGuildChannel).Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {channel.Id}"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(0, 255, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await (channel as SocketGuildChannel).Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.ChannelUpdated)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (channel is SocketTextChannel)
            {
                if ((channel as SocketTextChannel).Name != (channel2 as SocketTextChannel).Name)
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** renamed the text channel **{(channel as SocketTextChannel).Name}** to **{(channel2 as SocketTextChannel).Name}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketTextChannel).Topic != (channel2 as SocketTextChannel).Topic)
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** changed the topic of {(channel as SocketTextChannel).Mention} from **{(channel2 as SocketTextChannel).Topic}** to **{(channel2 as SocketTextChannel).Topic}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketTextChannel).IsNsfw != (channel2 as SocketTextChannel).IsNsfw)
                {
                    if ((channel as SocketTextChannel).IsNsfw)
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** set {(channel as SocketTextChannel).Mention} as SFW.";
                    }
                    else
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** set {(channel as SocketTextChannel).Mention} as NSFW.";
                    }
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
            }
            else if (channel is SocketVoiceChannel)
            {
                if ((channel as SocketVoiceChannel).Name != (channel2 as SocketVoiceChannel).Name)
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** renamed the voice channel **{(channel as SocketVoiceChannel).Name}** to **{(channel2 as SocketVoiceChannel).Name}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketVoiceChannel).Bitrate != (channel2 as SocketVoiceChannel).Bitrate)
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** changed {(channel as SocketVoiceChannel).Name}'s birate from **{(channel as SocketVoiceChannel).Bitrate}** to **{(channel2 as SocketVoiceChannel).Bitrate}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketVoiceChannel).UserLimit != (channel2 as SocketVoiceChannel).UserLimit)
                {
                    if ((channel as SocketVoiceChannel).UserLimit == null)
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** changed {(channel as SocketVoiceChannel).Name}'s user limit from **unlimited** to **{(channel2 as SocketVoiceChannel).UserLimit}**.";
                    }
                    else if ((channel2 as SocketVoiceChannel).UserLimit == null)
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** changed {(channel as SocketVoiceChannel).Name}'s user limit from **{(channel as SocketVoiceChannel).UserLimit}** to **unlimited**.";
                    }
                    else
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** changed {(channel as SocketVoiceChannel).Name}'s user limit from **{(channel as SocketVoiceChannel).UserLimit}** to **{(channel2 as SocketVoiceChannel).UserLimit}**.";
                    }
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
            }
            else if (channel is SocketCategoryChannel)
            {
                if ((channel as SocketCategoryChannel).Name != (channel2 as SocketCategoryChannel).Name)
                {
                    embed.Description = $"**{user.Username}#{user.Discriminator}** renamed the category channel **{(channel as SocketCategoryChannel).Name}** to **{(channel2 as SocketCategoryChannel).Name}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = reason;
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task AnnounceChannelDestroyed(SocketChannel channel)
        {
            if ((channel as SocketGuildChannel).Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Channel Deleted",
                IconUrl = (channel as SocketGuildChannel).Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {channel.Id}"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(255, 0, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await (channel as SocketGuildChannel).Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.ChannelDeleted)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (channel is SocketTextChannel)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** deleted the text channel: **{(channel as SocketGuildChannel).Name}**.";
            }
            else if (channel is SocketVoiceChannel)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** deleted the voice channel: **{(channel as SocketGuildChannel).Name}**.";
            }
            else if (channel is SocketCategoryChannel)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** deleted the category channel: **{(channel as SocketGuildChannel).Name}**.";
            }
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = reason;
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceChannelCreated(SocketChannel channel)
        {
            if ((channel as SocketGuildChannel).Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Channel Created",
                IconUrl = (channel as SocketGuildChannel).Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {channel.Id}"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(0, 255, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await (channel as SocketGuildChannel).Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.ChannelCreated)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (channel is SocketTextChannel)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** created the text channel: {(channel as SocketTextChannel).Mention}.";
            }
            else if (channel is SocketVoiceChannel)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** created the voice channel: **{(channel as SocketGuildChannel).Name}**.";
            }
            else if (channel is SocketCategoryChannel)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** created the category channel: **{(channel as SocketGuildChannel).Name}**.";
            }
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = reason;
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceGuildMemberUpdated(SocketGuildUser user, SocketGuildUser user2)
        {
            if (user.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (user.Status != user2.Status)
            {
                return;
            }
            SocketTextChannel log = user.Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Member Updated",
                IconUrl = user.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {user.Id}",
                IconUrl = user.GetAvatarUrl()
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(0, 255, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser iUser = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await user.Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.MemberUpdated)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (user.Nickname != user2.Nickname)
            {
                string nickname = user.Nickname;
                string nickname2 = user2.Nickname;
                if (iUser.Id == user.Id)
                {
                    if (nickname == null)
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** set their nickname to **{nickname2}**.";
                    }
                    else if (nickname2 == null)
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** removed their nickname of **{nickname}**.";
                    }
                    else
                    {
                        embed.Description = $"**{user.Username}#{user.Discriminator}** has changed their nickname from **{nickname}** to **{nickname2}**.";
                    }
                }
                else
                {
                    if (nickname == null)
                    {
                        embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** set **{user.Username}#{user.Discriminator}'s** nickname to **{nickname2}**.";
                    }
                    else if (nickname2 == null)
                    {
                        embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** removed **{user.Username}#{user.Discriminator}'s** nickname of **{nickname}**.";
                    }
                    else
                    {
                        embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** changed **{user.Username}#{user.Discriminator}'s** nickname from **{nickname}** to **{nickname2}**.";
                    }
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            if (user.Roles.Count != user2.Roles.Count)
            {
                var roles = user.Roles.OrderBy(x => x.Name);
                var roles2 = user2.Roles.OrderBy(x => x.Name);
                List<SocketRole> roleList = new List<SocketRole>(), roleList2 = new List<SocketRole>();
                foreach (SocketRole role in roles)
                {
                    if (!role.IsEveryone)
                    {
                        roleList.Add(role);
                    }
                }
                foreach (SocketRole role in roles2)
                {
                    if (!role.IsEveryone)
                    {
                        roleList2.Add(role);
                    }
                }
                if (roleList.Count > roleList2.Count)
                {
                    var diff = roleList.Except(roleList2);
                    var role = diff.ElementAt(0);
                    embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** removed **{user.Username}#{user.Discriminator}** from the **{role}** role.";
                }
                else
                {
                    var diff = roleList2.Except(roleList);
                    var role = diff.ElementAt(0);
                    embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** added **{user.Username}#{user.Discriminator}** to the **{role}** role.";
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task AnnounceMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (PublicModule.Purging)
            {
                return;
            }
            IMessage message2 = await message.GetOrDownloadAsync();
            SocketTextChannel channel2 = channel as SocketTextChannel;
            SocketTextChannel log = _client.GetChannel(327575359765610496) as SocketTextChannel;
            if (channel2.Id == log.Id || channel2.Guild.Id != 249657315576381450)
            {
                return;
            }
            foreach (Discord.Rest.RestBan ban in await channel2.Guild.GetBansAsync())
            {
                if (ban.User.Id == message2.Author.Id)
                {
                    return;
                }
            }
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Message Deleted",
                IconUrl = channel2.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {message2.Author.Id}",
                IconUrl = message2.Author.GetAvatarUrl()
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(255, 0, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser iUser = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await channel2.Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.MessageDeleted)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }

            if (iUser != null)
            {
                if (message2.Content.Length > 252)
                {
                    embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** deleted a message by **{message2.Author.Username}#{message2.Author.Discriminator}** in {channel2.Mention}.\nContent: **{message2.Content.SplitInParts(252).ElementAt(0)}...**";
                }
                else
                {
                    embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** deleted a message by **{message2.Author.Username}#{message2.Author.Discriminator}** in {channel2.Mention}.\nContent: **{message2.Content}**";
                }
            }
            else
            {
                if (message2.Content.Length > 252)
                {
                    embed.Description = $"Message deleted from **{message2.Author.Username}#{message2.Author.Discriminator}** in {channel2.Mention}.\nContent: **{message2.Content.SplitInParts(252).ElementAt(0)}...**";
                }
                else
                {
                    embed.Description = $"Message deleted from **{message2.Author.Username}#{message2.Author.Discriminator}** in {channel2.Mention}.\nContent: **{message2.Content}**";
                }
            }
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = reason;
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceUserUnbanned(SocketUser user, SocketGuild guild)
        {
            if (guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "User Unbanned",
                IconUrl = guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {user.Id}",
                IconUrl = user.GetAvatarUrl()
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(0, 255, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser iUser = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.Unban)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** unbanned **{user.Username}#{user.Discriminator}** from the guild.";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = reason;
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceUserBanned(SocketUser user, SocketGuild guild)
        {
            if (guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "User Banned",
                IconUrl = guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {user.Id}",
                IconUrl = user.GetAvatarUrl()
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(255, 0, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser iUser = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.Ban)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** banned **{user.Username}#{user.Discriminator}** from the guild.\nTotal members: **{guild.MemberCount - 1}**";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = reason;
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceUserJoined(SocketGuildUser user)
        {
            if (user.Guild.Id == 249657315576381450)
            {
                SocketTextChannel log = user.Guild.GetTextChannel(327575359765610496), channel = user.Guild.GetTextChannel(249657315576381450);
                IEnumerable<SocketRole> members = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Members".ToUpper()), verified = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Verified".ToUpper()), muted = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Muted".ToUpper());
                SocketTextChannel rules = user.Guild.GetTextChannel(249682754407497728), roles = user.Guild.GetTextChannel(260272249976782848);
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Name = "User Joined",
                    IconUrl = user.Guild.IconUrl
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    Text = $"ID: {user.Id}",
                    IconUrl = user.GetAvatarUrl()
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Author = author,
                    Color = new Color(0, 255, 0),
                    Footer = footer
                };
                embed.WithCurrentTimestamp();
                embed.Description = $"**{user.Username}#{user.Discriminator}** joined the guild.\nTotal members: **{user.Guild.MemberCount}**";
                await log.SendMessageAsync("", false, embed.Build());
                var result = Database.CheckExistingUser(user);
                if (result.Count() <= 0)
                {
                    Database.EnterUser(user);
                }
                result = Database.CheckForVerified(user, "Not verified");
                bool isMuted = Database.CheckForMuted(user);
                if (isMuted)
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = "Auto Mute - Attempted Mute Evade."
                    };
                    await user.AddRolesAsync(roles: muted, options: options);
                }
                else if ((result.Count() <= 0))
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = "Auto Verify."
                    };
                    string pr2name = Database.GetPR2Name(user);
                    if (!user.Username.Equals(pr2name))
                    {
                        await user.ModifyAsync(x => x.Nickname = pr2name);
                    }
                    await user.AddRolesAsync(roles: verified, options: options);
                    await user.SendMessageAsync($"Hello {user.Username} ! Welcome back to the Platform Racing Group.\nYou have been added to the verified role as you verified yourself the last time you were here.");
                }
                else
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = "Auto Role."
                    };
                    await user.AddRolesAsync(roles: members, options: options);
                    await user.SendMessageAsync($"Hello {user.Username} ! Welcome to the Platform Racing Group.\nIf you would like to be verified type /verify in DMs " +
                    $"with me or on the Server and follow the instructions.\nAnyway thank you for joining and don't forget to read {rules.Mention} and {roles.Mention}.");
                }
            }
            else
            {
                return;
            }
        }

        public async Task AnnounceUserLeft(SocketGuildUser user)
        {
            if (user.Guild.Id != 249657315576381450)
            {
                return;
            }
            foreach (Discord.Rest.RestBan ban in await user.Guild.GetBansAsync())
            {
                if (ban.User.Id == user.Id)
                {
                    return;
                }
            }
            SocketTextChannel log = user.Guild.GetTextChannel(327575359765610496);
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "User Left",
                IconUrl = user.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {user.Id}",
                IconUrl = user.GetAvatarUrl()
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(255, 0, 0),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser iUser = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await user.Guild.GetAuditLogsAsync(20).FlattenAsync())
            {
                if (audit.Action == ActionType.Kick)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (iUser == null)
            {
                embed.Description = $"**{user.Username}#{user.Discriminator}** left the guild. Total members: **{user.Guild.MemberCount}**";
            }
            else
            {
                embed.Author.Name = "User Kicked";
                embed.Description = $"**{iUser.Username}#{iUser.Discriminator}** kicked **{user.Username}#{user.Discriminator}** from the guild.\nTotal members: **{user.Guild.MemberCount}**";
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = reason;
                        y.IsInline = false;
                    });
                }
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        #endregion

        public async Task OnMessageReceived(SocketMessage m)
        {
            try
            {
                SocketUserMessage msg = m as SocketUserMessage;
                if (msg == null) return;
                if (msg.Channel is SocketGuildChannel && msg.Channel is SocketTextChannel channel)
                {
                    if (channel.Guild.Id == 249657315576381450 && channel.Id != 327575359765610496)
                    {
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = msg.Author.Username + "#" + msg.Author.Discriminator,
                            IconUrl = msg.Author.GetAvatarUrl()
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {msg.Author.Id}"
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Author = author,
                            Color = new Color(255, 0, 0),
                            Footer = footer,
                            Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Reason",
                                Value = "Bad words",
                                IsInline = false
                            }
                        }
                        };
                        embed.WithCurrentTimestamp();
                        var log = channel.Guild.GetTextChannel(327575359765610496);
                        Discord.Rest.RestUserMessage message = null;
                        //var usermessages = channel.GetMessagesAsync().Flatten().Where(x => x.Author == msg.Author).Take(4).ToEnumerable();
                        //if (((usermessages.ElementAt(0) as SocketUserMessage).CreatedAt - (usermessages.ElementAt(3) as SocketUserMessage).CreatedAt).Seconds < 5)
                        //{
                        //    embed.Fields.ElementAt(0).Value = "Sent 4 messages in less than 5 seconds";
                        //    embed.Description = $"**Messages sent by {msg.Author.Mention} deleted in {channel.Mention}**\n{msg.Content}";
                        //    PublicModule.Purging = true;
                        //    await channel.DeleteMessagesAsync(usermessages);
                        //    await log.SendMessageAsync("", false, embed.Build());
                        //    PublicModule.Purging = false;
                        //    message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no spamming!");
                        //    await Task.Delay(5000);
                        //    PublicModule.Purging = true;
                        //    await message.DeleteAsync();
                        //    PublicModule.Purging = false;
                        //}
                        var bannedwords = File.ReadAllText(Path.Combine(PublicModule.downloadPath, "BlacklistedWords.txt")).Split("\r\n");
                        foreach (string bannedword in bannedwords)
                        {
                            if (msg.Content.Contains(bannedword, StringComparison.OrdinalIgnoreCase))
                            {
                                PublicModule.Purging = true;
                                await msg.DeleteAsync();
                                PublicModule.Purging = false;
                                embed.Fields.ElementAt(0).Value = "Bad words";
                                if (msg.Content.Length > 252)
                                {
                                    embed.Description = $"**Message sent by {msg.Author.Mention} deleted in {channel.Mention}**\n{msg.Content.SplitInParts(252).ElementAt(0)}...";
                                }
                                else
                                {
                                    embed.Description = $"**Message sent by {msg.Author.Mention} deleted in {channel.Mention}**\n{msg.Content}";
                                }
                                await log.SendMessageAsync("", false, embed.Build());
                                message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} watch your language.");
                                await Task.Delay(5000);
                                PublicModule.Purging = true;
                                await message.DeleteAsync();
                                PublicModule.Purging = false;
                                return;
                            }
                        }

                    }
                }
                await HandleCommand(msg);
            }
            catch (Exception e)
            {
                await PublicModule.ExceptionInfo(_client, e.Message, e.StackTrace);
            }
        }

        public async Task HandleCommand(SocketUserMessage msg)
        {
            SocketCommandContext context = new SocketCommandContext(_client, msg);
            int argPos = 0;
            if (msg.HasStringPrefix("/", ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                IResult result = await _cmds.ExecuteAsync(context, argPos, Program._provider);
            }
        }
    }
}
