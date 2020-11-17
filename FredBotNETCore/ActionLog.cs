using Discord;
using Discord.WebSocket;
using FredBotNETCore.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FredBotNETCore
{
    public class ActionLog
    {
        public ActionLog()
        {

        }

        public async Task OnGuildJoin(SocketGuild guild)
        {
            if (!Guild.Exists(guild))
            {
                Guild.Add(guild);
            }
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = guild.Name,
                IconUrl = guild.IconUrl
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(0, 255, 0),
            };
            embed.Description = $"Hi I'm Fred the G. Cactus. Thank you for adding me to this server.\n" +
                $"For a list of commands type /help.\n" +
                $"**Note:** Keep in mind the music commands and some of the moderation commands can only be done in the offical server for the bot which is https://discord.gg/kcWBBBj \n" +
                $"Also the PR2 commands are based off a game which can be played at https://jiggmin2.com/games/platform-racing-2/ and the forums are located at https://jiggmin2.com/forums/ \n" +
                $"Thanks again for adding me to this server.";
            try
            {
                await guild.DefaultChannel.SendMessageAsync("", false, embed.Build());
            }
            catch (Discord.Net.HttpException)
            {
                //ignore
            }
        }

        public async Task AnnounceRoleUpdated(SocketRole role, SocketRole role2)
        {
            SocketTextChannel log = Extensions.GetLogChannel(role.Guild);
            if (log != null)
            {
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
        }

        public async Task AnnounceRoleDeleted(SocketRole role)
        {
            SocketTextChannel log = Extensions.GetLogChannel(role.Guild);
            if (log != null)
            {
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
        }

        public async Task AnnounceRoleCreated(SocketRole role)
        {
            SocketTextChannel log = Extensions.GetLogChannel(role.Guild);
            if (log != null)
            {
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
        }

        public async Task AnnounceChannelUpdated(SocketChannel channel, SocketChannel channel2)
        {
            if (channel is IDMChannel)
            {
                return;
            }
            SocketTextChannel log = Extensions.GetLogChannel((channel as SocketGuildChannel).Guild);
            if (log != null)
            {
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
                if (channel is SocketTextChannel textChannel && channel2 is SocketTextChannel textChannel2)
                {
                    if (textChannel.Name != textChannel2.Name)
                    {
                        embed.Description = $"{user.Mention} renamed the text channel **{Format.Sanitize(textChannel.Name)}** to {textChannel2.Mention}.";
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
                    else if (textChannel.Topic != textChannel2.Topic)
                    {
                        string topic1 = Format.Sanitize(textChannel.Topic);
                        string topic2 = Format.Sanitize(textChannel2.Topic);
                        if (topic1.Length > 100)
                        {
                            topic1 = textChannel.Topic.SplitInParts(100).ElementAt(0) + "...";
                        }
                        if (topic2.Length > 100)
                        {
                            topic2 = textChannel2.Topic.SplitInParts(100).ElementAt(0) + "...";
                        }
                        embed.Description = $"{user.Mention} changed the topic of {textChannel.Mention} from **{topic1}** to **{topic2}**.";
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
                    else if (textChannel.IsNsfw != textChannel2.IsNsfw)
                    {
                        if (textChannel.IsNsfw)
                        {
                            embed.Description = $"{user.Mention} set {textChannel.Mention} as SFW.";
                        }
                        else
                        {
                            embed.Description = $"{user.Mention} set {textChannel.Mention} as NSFW.";
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
                    else if (textChannel.SlowModeInterval != textChannel2.SlowModeInterval)
                    {
                        if (textChannel2.SlowModeInterval == 0)
                        {
                            embed.Description = $"{user.Mention} removed the slowmode in {textChannel.Mention}.";
                        }
                        else if (textChannel2.SlowModeInterval == 1)
                        {
                            embed.Description = $"{user.Mention} set {textChannel.Mention}'s slowmode as {textChannel2.SlowModeInterval} second.";
                        }
                        else
                        {
                            embed.Description = $"{user.Mention} set {textChannel.Mention}'s slowmode as {textChannel2.SlowModeInterval} seconds.";
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
        }

        public async Task AnnounceChannelDestroyed(SocketChannel channel)
        {
            if (channel is IDMChannel)
            {
                return;
            }
            SocketTextChannel log = Extensions.GetLogChannel((channel as SocketGuildChannel).Guild);
            if (log != null)
            {
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
        }

        public async Task AnnounceChannelCreated(SocketChannel channel)
        {
            if (channel is IDMChannel)
            {
                return;
            }
            SocketTextChannel log = Extensions.GetLogChannel((channel as SocketGuildChannel).Guild);
            if (log != null)
            {
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
        }

        public async Task AnnounceGuildMemberUpdated(SocketGuildUser user, SocketGuildUser user2)
        {
            if (user.Roles.Count == user2.Roles.Count && user.Nickname == user2.Nickname)
            {
                return;
            }
            SocketTextChannel log = Extensions.GetLogChannel(user.Guild);
            if (log != null)
            {
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
                    IOrderedEnumerable<SocketRole> roles = user.Roles.OrderBy(x => x.Name);
                    IOrderedEnumerable<SocketRole> roles2 = user2.Roles.OrderBy(x => x.Name);
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
                        IEnumerable<SocketRole> diff = roleList.Except(roleList2);
                        SocketRole role = diff.ElementAt(0);
                        embed.Description = $"{iUser.Mention} removed {user.Mention} from the {role.Mention} role.";
                    }
                    else
                    {
                        IEnumerable<SocketRole> diff = roleList2.Except(roleList);
                        SocketRole role = diff.ElementAt(0);
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
        }

        public async Task AnnounceBulkDelete(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, ISocketMessageChannel channel)
        {
            if (channel is IDMChannel)
            {
                return;
            }
            SocketTextChannel channel2 = channel as SocketTextChannel;
            SocketTextChannel log = Extensions.GetLogChannel((channel as SocketGuildChannel).Guild);
            if (log != null)
            {
                if (channel2.Id == log.Id)
                {
                    return;
                }
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Name = "Bulk Delete",
                    IconUrl = channel2.Guild.IconUrl
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    Text = $"ID: {channel2.Id}"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Author = author,
                    Color = new Color(255, 0, 0),
                    Footer = footer
                };
                embed.WithCurrentTimestamp();
                embed.Description = $"**{messages.Count}** messages were deleted in {channel2.Mention}.";
                await log.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task AnnounceMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (channel is IDMChannel)
            {
                return;
            }
            IMessage message2 = await message.GetOrDownloadAsync();
            SocketTextChannel channel2 = channel as SocketTextChannel;
            SocketTextChannel log = Extensions.GetLogChannel((channel as SocketGuildChannel).Guild);
            if (log != null && message2 != null)
            {
                if (channel2.Id == log.Id)
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

                if (message2.Content.Length > 252)
                {
                    embed.Description = $"{message2.Author.Mention} deleted their message in {channel2.Mention}.\nContent: **{Format.Sanitize(message2.Content).SplitInParts(252).ElementAt(0)}...**";
                }
                else
                {
                    embed.Description = $"{message2.Author.Mention} deleted their message in {channel2.Mention}.\nContent: **{Format.Sanitize(message2.Content)}**";
                }

                await log.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task AnnounceUserUnbanned(SocketUser user, SocketGuild guild)
        {
            SocketTextChannel log = Extensions.GetLogChannel(guild);
            if (log != null)
            {
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
        }

        public async Task AnnounceUserBanned(SocketUser user, SocketGuild guild)
        {
            SocketTextChannel log = Extensions.GetLogChannel(guild);
            if (log != null)
            {
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
        }

        public async Task AnnounceUserJoined(SocketGuildUser user)
        {
            SocketTextChannel log = Extensions.GetLogChannel(user.Guild);
            if (log != null)
            {
                IEnumerable<SocketRole> verified = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Verified".ToUpper()), muted = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Muted".ToUpper());
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
                if (!User.Exists(user))
                {
                    User.Add(user);
                }
                string welcomeMessage = Guild.Get(user.Guild).WelcomeMessage;
                bool isVerified = User.IsVerified(user);
                if (muted.Count() > 0 && MutedUser.Get(user.Guild.Id, user.Id).Count > 0)
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = "Auto Mute - Attempted Mute Evade."
                    };
                    await user.AddRolesAsync(roles: muted, options: options);
                }
                if (verified.Count() > 0 && isVerified)
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = "Auto Verify."
                    };
                    string pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                    await user.AddRolesAsync(roles: verified, options: options);
                    if (!user.Username.Equals(pr2name))
                    {
                        options.AuditLogReason = "Setting nickname to PR2 name.";
                        await user.ModifyAsync(x => x.Nickname = pr2name, options);
                    }
                    try
                    {
                        await user.SendMessageAsync($"Hello {Format.Sanitize(user.Username)} ! Welcome back to **{Format.Sanitize(user.Guild.Name)}**.\nYou have been added to the verified role as you have verified your PR2 account with me.");
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //could not send message
                    }
                }
                if (welcomeMessage != null)
                {
                    try
                    {
                        await user.SendMessageAsync(welcomeMessage);
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
            SocketTextChannel log = Extensions.GetLogChannel(user.Guild);
            if (log != null)
            {
                foreach (Discord.Rest.RestBan ban in await user.Guild.GetBansAsync())
                {
                    if (ban.User.Id == user.Id)
                    {
                        return;
                    }
                }
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
}
