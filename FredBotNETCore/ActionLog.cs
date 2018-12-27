using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FredBotNETCore
{
    public class ActionLog
    {
        public DiscordSocketClient Client { get; set; }

        public ActionLog(DiscordSocketClient client)
        {
            Client = client;
        }

        public async Task AnnounceRoleUpdated(SocketRole role, SocketRole role2)
        {
            if (role.Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = role.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                Color = new Color(0, 0, 255),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await role.Guild.GetAuditLogsAsync(5).FlattenAsync())
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
                embed.Description = $"{user.Mention} renamed the role **{Format.Sanitize(role.Name)}** to {role2.Mention}.";
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            else if (role.IsMentionable != role2.IsMentionable)
            {
                if (role.IsMentionable)
                {
                    embed.Description = $"{user.Mention} made the role {role.Mention} unmentionable.";
                }
                else
                {
                    embed.Description = $"{user.Mention} made the role {role.Mention} mentionable.";
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            else if (role.IsHoisted != role2.IsHoisted)
            {
                if (role.IsHoisted)
                {
                    embed.Description = $"{user.Mention} made the role {role.Mention} not display separately.";
                }
                else
                {
                    embed.Description = $"{user.Mention} made the role {role.Mention} display separately.";
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            else if (!role.Color.Equals(role2.Color))
            {
                embed.Description = $"{user.Mention} changed the color of {role.Mention} from **#{Extensions.HexConverter(role.Color)}** to **#{Extensions.HexConverter(role2.Color)}**.";
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
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
            SocketTextChannel log = role.Guild.GetTextChannel(Extensions.GetLogChannel());
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await role.Guild.GetAuditLogsAsync(5).FlattenAsync())
            {
                if (audit.Action == ActionType.RoleDeleted)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"{user.Mention} deleted the role **{Format.Sanitize(role.Name)}**.";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = Format.Sanitize(reason);
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
            SocketTextChannel log = role.Guild.GetTextChannel(Extensions.GetLogChannel());
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Role Created",
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await role.Guild.GetAuditLogsAsync(5).FlattenAsync())
            {
                if (audit.Action == ActionType.RoleCreated)
                {
                    user = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"{user.Mention} created the role {role.Mention}.";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = Format.Sanitize(reason);
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
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(Extensions.GetLogChannel());
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
                Color = new Color(0, 0, 255),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser user = null;
            string reason = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await (channel as SocketGuildChannel).Guild.GetAuditLogsAsync(2).FlattenAsync())
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
                    embed.Description = $"{user.Mention} renamed the text channel **{Format.Sanitize((channel as SocketTextChannel).Name)}** to {(channel2 as SocketTextChannel).Mention}.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketTextChannel).Topic != (channel2 as SocketTextChannel).Topic)
                {
                    string topic1 = Format.Sanitize((channel as SocketTextChannel).Topic);
                    string topic2 = Format.Sanitize((channel2 as SocketTextChannel).Topic);
                    if (topic1.Length > 100)
                    {
                        topic1 = (channel as SocketTextChannel).Topic.SplitInParts(100).ElementAt(0) + "...";
                    }
                    if (topic2.Length > 100)
                    {
                        topic2 = (channel2 as SocketTextChannel).Topic.SplitInParts(100).ElementAt(0) + "...";
                    }
                    embed.Description = $"{user.Mention} changed the topic of {(channel as SocketTextChannel).Mention} from **{topic1}** to **{topic2}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketTextChannel).IsNsfw != (channel2 as SocketTextChannel).IsNsfw)
                {
                    if ((channel as SocketTextChannel).IsNsfw)
                    {
                        embed.Description = $"{user.Mention} set {(channel as SocketTextChannel).Mention} as SFW.";
                    }
                    else
                    {
                        embed.Description = $"{user.Mention} set {(channel as SocketTextChannel).Mention} as NSFW.";
                    }
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
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
                    embed.Description = $"{user.Mention} renamed the voice channel **{Format.Sanitize((channel as SocketVoiceChannel).Name)}** to **{Format.Sanitize((channel2 as SocketVoiceChannel).Name)}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketVoiceChannel).Bitrate != (channel2 as SocketVoiceChannel).Bitrate)
                {
                    embed.Description = $"{user.Mention} changed **{Format.Sanitize((channel as SocketVoiceChannel).Name)}'s** birate from **{(channel as SocketVoiceChannel).Bitrate}** to **{(channel2 as SocketVoiceChannel).Bitrate}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = false;
                        });
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else if ((channel as SocketVoiceChannel).UserLimit != (channel2 as SocketVoiceChannel).UserLimit)
                {
                    if ((channel as SocketVoiceChannel).UserLimit == null)
                    {
                        embed.Description = $"{user.Mention} changed **{Format.Sanitize((channel as SocketVoiceChannel).Name)}'s** user limit from **unlimited** to **{(channel2 as SocketVoiceChannel).UserLimit}**.";
                    }
                    else if ((channel2 as SocketVoiceChannel).UserLimit == null)
                    {
                        embed.Description = $"{user.Mention} changed **{Format.Sanitize((channel as SocketVoiceChannel).Name)}'s** user limit from **{(channel as SocketVoiceChannel).UserLimit}** to **unlimited**.";
                    }
                    else
                    {
                        embed.Description = $"{user.Mention} changed **{Format.Sanitize((channel as SocketVoiceChannel).Name)}'s** user limit from **{(channel as SocketVoiceChannel).UserLimit}** to **{(channel2 as SocketVoiceChannel).UserLimit}**.";
                    }
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
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
                    embed.Description = $"{user.Mention} renamed the category channel **{Format.Sanitize((channel as SocketCategoryChannel).Name)}** to **{Format.Sanitize((channel2 as SocketCategoryChannel).Name)}**.";
                    if (reason != null)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
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
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(Extensions.GetLogChannel());
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await (channel as SocketGuildChannel).Guild.GetAuditLogsAsync(5).FlattenAsync())
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
                embed.Description = $"{user.Mention} deleted the text channel: **{Format.Sanitize((channel as SocketGuildChannel).Name)}**.";
            }
            else if (channel is SocketVoiceChannel)
            {
                embed.Description = $"{user.Mention} deleted the voice channel: **{Format.Sanitize((channel as SocketGuildChannel).Name)}**.";
            }
            else if (channel is SocketCategoryChannel)
            {
                embed.Description = $"{user.Mention} deleted the category channel: **{Format.Sanitize((channel as SocketGuildChannel).Name)}**.";
            }
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = Format.Sanitize(reason);
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
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(Extensions.GetLogChannel());
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await (channel as SocketGuildChannel).Guild.GetAuditLogsAsync(5).FlattenAsync())
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
                embed.Description = $"{user.Mention} created the text channel: {(channel as SocketTextChannel).Mention}.";
            }
            else if (channel is SocketVoiceChannel)
            {
                embed.Description = $"{user.Mention} created the voice channel: **{Format.Sanitize((channel as SocketGuildChannel).Name)}**.";
            }
            else if (channel is SocketCategoryChannel)
            {
                embed.Description = $"{user.Mention} created the category channel: **{Format.Sanitize((channel as SocketGuildChannel).Name)}**.";
            }
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = Format.Sanitize(reason);
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceGuildMemberUpdated(SocketGuildUser user, SocketGuildUser user2)
        {
            if (user.Guild.Id != 249657315576381450 || (user.Roles.Count == user2.Roles.Count && user.Nickname == user2.Nickname))
            {
                return;
            }
            SocketTextChannel log = user.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                Color = new Color(0, 0, 255),
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            IUser iUser = null;
            string reason = null;
            Discord.Rest.RestAuditLogEntry restAuditLog = null;
            foreach (Discord.Rest.RestAuditLogEntry audit in await user.Guild.GetAuditLogsAsync(2).FlattenAsync())
            {
                if (audit.Action == ActionType.MemberUpdated || audit.Action == ActionType.MemberRoleUpdated)
                {
                    restAuditLog = audit;
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (user.Nickname != user2.Nickname)
            {
                string nickname = user.Nickname;
                string nickname2 = user2.Nickname;
                if (iUser == null || iUser.Id == user.Id || restAuditLog.Action == ActionType.MemberRoleUpdated)
                {
                    if (nickname == null)
                    {
                        embed.Description = $"{user.Mention} set their nickname to **{Format.Sanitize(nickname2)}**.";
                    }
                    else if (nickname2 == null)
                    {
                        embed.Description = $"{user.Mention} removed their nickname of **{Format.Sanitize(nickname)}**.";
                    }
                    else
                    {
                        embed.Description = $"{user.Mention} has changed their nickname from **{Format.Sanitize(nickname)}** to **{Format.Sanitize(nickname2)}**.";
                    }
                }
                else
                {
                    if (nickname == null)
                    {
                        embed.Description = $"{iUser.Mention} set **{user.Mention}'s** nickname to **{Format.Sanitize(nickname2)}**.";
                    }
                    else if (nickname2 == null)
                    {
                        embed.Description = $"{iUser.Mention} removed **{user.Mention}'s** nickname of **{Format.Sanitize(nickname)}**.";
                    }
                    else
                    {
                        embed.Description = $"{iUser.Mention} changed **{user.Mention}'s** nickname from **{Format.Sanitize(nickname)}** to **{Format.Sanitize(nickname2)}**.";
                    }
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
            if (user.Roles.Count != user2.Roles.Count)
            {
                if (restAuditLog.Action == ActionType.MemberUpdated)
                {
                    foreach (Discord.Rest.RestAuditLogEntry audit in await user.Guild.GetAuditLogsAsync(2).FlattenAsync())
                    {
                        if (audit.Action == ActionType.MemberRoleUpdated)
                        {
                            restAuditLog = audit;
                            iUser = audit.User;
                            reason = audit.Reason;
                            break;
                        }
                    }
                }
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
                    embed.Description = $"{iUser.Mention} removed {user.Mention} from the {role.Mention} role.";
                }
                else
                {
                    var diff = roleList2.Except(roleList);
                    var role = diff.ElementAt(0);
                    embed.Description = $"{iUser.Mention} added {user.Mention} to the {role.Mention} role.";
                }
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = false;
                    });
                }
                await log.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task AnnounceMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (Extensions.Purging)
            {
                return;
            }
            IMessage message2 = await message.GetOrDownloadAsync();
            SocketTextChannel channel2 = channel as SocketTextChannel;
            SocketTextChannel log = Client.GetChannel(Extensions.GetLogChannel()) as SocketTextChannel;
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await channel2.Guild.GetAuditLogsAsync(1).FlattenAsync())
            {
                if (audit.Action == ActionType.MessageDeleted && DateTime.Now.ToUniversalTime().AddSeconds(-5) > audit.CreatedAt.ToUniversalTime())
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }

            if (iUser != null && iUser.Id != message2.Author.Id)
            {
                if (message2.Content.Length > 252)
                {
                    embed.Description = $"{iUser.Mention} deleted a message by {message2.Author.Mention} in {channel2.Mention}.\nContent: **{Format.Sanitize(message2.Content).SplitInParts(252).ElementAt(0)}...**";
                }
                else
                {
                    embed.Description = $"{iUser.Mention} deleted a message by {message2.Author.Mention} in {channel2.Mention}.\nContent: **{Format.Sanitize(message2.Content)}**";
                }
            }
            else
            {
                if (message2.Content.Length > 252)
                {
                    embed.Description = $"{message2.Author.Mention} deleted their message in {channel2.Mention}.\nContent: **{Format.Sanitize(message2.Content).SplitInParts(252).ElementAt(0)}...**";
                }
                else
                {
                    embed.Description = $"{message2.Author.Mention} deleted their message in {channel2.Mention}.\nContent: **{Format.Sanitize(message2.Content)}**";
                }
            }
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = Format.Sanitize(reason);
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
            SocketTextChannel log = guild.GetTextChannel(Extensions.GetLogChannel());
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await guild.GetAuditLogsAsync(5).FlattenAsync())
            {
                if (audit.Action == ActionType.Unban)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"{iUser.Mention} unbanned **{Format.Sanitize(user.Username)}#{user.Discriminator}** from the guild.";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = Format.Sanitize(reason);
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
            SocketTextChannel log = guild.GetTextChannel(Extensions.GetLogChannel());
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await guild.GetAuditLogsAsync(5).FlattenAsync())
            {
                if (audit.Action == ActionType.Ban)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            embed.Description = $"{iUser.Mention} banned **{Format.Sanitize(user.Username)}#{user.Discriminator}** from the guild.\nTotal members: **{guild.MemberCount - 1}**";
            if (reason != null)
            {
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = Format.Sanitize(reason);
                    y.IsInline = false;
                });
            }
            await log.SendMessageAsync("", false, embed.Build());
        }

        public async Task AnnounceUserJoined(SocketGuildUser user)
        {
            if (user.Guild.Id == 249657315576381450)
            {
                SocketTextChannel log = user.Guild.GetTextChannel(Extensions.GetLogChannel()), channel = user.Guild.GetTextChannel(249657315576381450);
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
                if ((DateTime.Now.ToUniversalTime() - user.CreatedAt.ToUniversalTime()).Days == 0)
                {
                    embed.Description = $"**{Format.Sanitize(user.Username)}#{user.Discriminator}** joined the guild. Account created **today**.\nTotal members: **{user.Guild.MemberCount}**";
                }
                else if ((DateTime.Now - user.CreatedAt).Days == 1)
                {
                    embed.Description = $"**{Format.Sanitize(user.Username)}#{user.Discriminator}** joined the guild. Account created **{(DateTime.Now.ToUniversalTime() - user.CreatedAt.ToUniversalTime()).Days}** day ago.\nTotal members: **{user.Guild.MemberCount}**";
                }
                else
                {
                    embed.Description = $"**{Format.Sanitize(user.Username)}#{user.Discriminator}** joined the guild. Account created **{(DateTime.Now.ToUniversalTime() - user.CreatedAt.ToUniversalTime()).Days}** days ago.\nTotal members: **{user.Guild.MemberCount}**";
                }
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
                    await user.AddRolesAsync(roles: verified, options: options);
                    if (!user.Username.Equals(pr2name))
                    {
                        options.AuditLogReason = "Setting nickname to PR2 name.";
                        await user.ModifyAsync(x => x.Nickname = pr2name, options);
                    }
                    try
                    {
                        await user.SendMessageAsync($"Hello {Format.Sanitize(user.Username)} ! Welcome back to the Platform Racing Group.\nYou have been added to the verified role as you verified yourself the last time you were here.");
                    }
                    catch(Discord.Net.HttpException)
                    {
                        //could not send message
                    }
                }
                else
                {
                    try
                    {
                        await user.SendMessageAsync($"Hello {user.Username} ! Welcome to the Platform Racing Group.\nIf you would like to be verified type /verify in DMs " +
                        $"with me or on the Server and follow the instructions.\nAnyway thank you for joining and don't forget to read {rules.Mention} and {roles.Mention}.");
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //could not send message
                    }
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
            SocketTextChannel log = user.Guild.GetTextChannel(Extensions.GetLogChannel());
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
            foreach (Discord.Rest.RestAuditLogEntry audit in await user.Guild.GetAuditLogsAsync(2).FlattenAsync())
            {
                if (audit.Action == ActionType.Kick)
                {
                    iUser = audit.User;
                    reason = audit.Reason;
                    break;
                }
            }
            if (iUser == null && (DateTime.Now.ToUniversalTime() - user.JoinedAt.Value.ToUniversalTime()).Days == 0)
            {
                embed.Description = $"**{Format.Sanitize(user.Username)}#{user.Discriminator}** left the guild. They spent less than a day in the server.\nTotal members: **{user.Guild.MemberCount}**";
            }
            else if (iUser == null && (DateTime.Now.ToUniversalTime() - user.JoinedAt.Value.ToUniversalTime()).Days == 1)
            {
                embed.Description = $"**{Format.Sanitize(user.Username)}#{user.Discriminator}** left the guild. They spent **{(DateTime.Now.ToUniversalTime() - user.JoinedAt.Value.ToUniversalTime()).Days}** day in the server.\nTotal members: **{user.Guild.MemberCount}**";
            }
            else if (iUser == null)
            {
                embed.Description = $"**{Format.Sanitize(user.Username)}#{user.Discriminator}** left the guild. They spent **{(DateTime.Now.ToUniversalTime() - user.JoinedAt.Value.ToUniversalTime()).Days}** days in the server.\nTotal members: **{user.Guild.MemberCount}**";
            }
            else
            {
                embed.Author.Name = "User Kicked";
                embed.Description = $"{iUser.Mention} kicked **{Format.Sanitize(user.Username)}#{user.Discriminator}** from the guild.\nTotal members: **{user.Guild.MemberCount}**";
                if (reason != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = false;
                    });
                }
            }
            await log.SendMessageAsync("", false, embed.Build());
        }
    }
}
