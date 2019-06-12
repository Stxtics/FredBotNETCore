using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FredBotNETCore.Services
{
    public class ModeratorService
    {
        public ModeratorService()
        {

        }

        public async Task SetNickAsync(SocketCommandContext context, string username, [Remainder] string nickname)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(nickname))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}setnick";
                embed.Description = $"**Description:** Change the nickname of a user.\n**Usage:** {prefix}setnick [user] [new nickname]\n**Example:** {prefix}setnick Jiggmin Jiggy";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = Extensions.UserInGuild(context.Message, context.Guild, username) as SocketGuildUser;
                    if (nickname.Length > 32)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} a users nickname cannot be longer than 32 characters.");
                    }
                    else
                    {
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"Changed by: {context.User.Username}#{context.User.Discriminator}"
                        };
                        await user.ModifyAsync(x => x.Nickname = nickname, options);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} successfully set the nickname of **{Format.Sanitize(user.Username)}#{user.Discriminator}** to **{Format.Sanitize(nickname)}**.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task NickAsync(SocketCommandContext context, string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}nick";
                embed.Description = $"**Description:** Change the bot nickname.\n**Usage:** {prefix}nick [new nickname]\n**Example:** {prefix}nick Fred";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (nickname.Length > 32)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} my nickname cannot be longer than 32 characters.");
                }
                else
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Changed by: {context.User.Mention}#{context.User.Discriminator}"
                    };
                    await context.Guild.GetUser(context.Client.CurrentUser.Id).ModifyAsync(x => x.Nickname = nickname, options);
                    await context.Channel.SendMessageAsync($"{context.User.Mention} successfully set my nickname to **{Format.Sanitize(nickname)}**.");
                }
            }
        }

        public async Task UpdateTokenAsync(SocketCommandContext context, string newToken)
        {
            if (context.Guild.Id == 356602194037964801)
            {
                string currentToken = File.ReadAllText(Path.Combine(Extensions.downloadPath, "PR2Token.txt"));

                SocketTextChannel log = Extensions.GetLogChannel(context.Client.GetGuild(528679522707701760));
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Name = "Token Changed",
                    IconUrl = context.Client.GetGuild(528679522707701760).IconUrl
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    Text = $"ID: {context.User.Id}",
                    IconUrl = context.User.GetAvatarUrl()
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Author = author,
                    Color = new Color(0, 0, 255),
                    Footer = footer
                };
                embed.WithCurrentTimestamp();
                embed.Description = $"{context.User.Mention} changed the token of FredTheG.CactusBot on PR2.";
                await context.Channel.SendMessageAsync($"{context.User.Mention} the token was successfully changed from **{Format.Sanitize(currentToken)}** to **{Format.Sanitize(newToken)}**.");
                await log.SendMessageAsync("", false, embed.Build());
                File.WriteAllText(Path.Combine(Extensions.downloadPath, "PR2Token.txt"), newToken);
            }
        }

        public async Task NotifyMacroersAsync(SocketCommandContext context)
        {
            SocketTextChannel channel = Extensions.GetNotificationsChannel(context.Guild);
            if (channel != null)
            {
                SocketRole role = context.Guild.Roles.Where(x => x.Name.ToUpper() == "Macroer".ToUpper()).FirstOrDefault();
                if (role != null)
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Notified by {context.User.Username}#{context.User.Discriminator}."
                    };
                    await context.Message.DeleteAsync();
                    await role.ModifyAsync(x => x.Mentionable = true, options);
                    await channel.SendMessageAsync($"Servers have just been restarted. Check your macros!! {role.Mention}");
                    await role.ModifyAsync(x => x.Mentionable = false, options);
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} the Macroer role does not exist or could not be found.");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the notifications channel has not been set.");
            }
        }

        public async Task BlacklistMusicAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command {prefix}blacklistmusic";
                    embed.Description = $"**Description:** Blacklist a user from using music commands.\n**Usage:** {prefix}blacklistmusic [user]\n**Example:** {prefix}blacklistmusic Jiggmin";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already blacklisted from using music commands.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers + user.Id.ToString() + "\n");
                            SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Music Blacklist Add",
                                IconUrl = context.Guild.IconUrl
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"ID: {context.User.Id}",
                                IconUrl = context.User.GetAvatarUrl()
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(255, 0, 0),
                                Footer = footer
                            };
                            embed.WithCurrentTimestamp();
                            embed.Description = $"{context.User.Mention} blacklisted {user.Mention} from using music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted **{Format.Sanitize(user.Username)}#{user.Discriminator}** from using music commands.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task UnblacklistMusicAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command {prefix}unblacklistmusic";
                    embed.Description = $"**Description:** Unblacklist a user from using music commands.\n**Usage:** {prefix}unblacklistmusic [user]\n**Example:** {prefix}unblacklistmusic Jiggmin";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            currentBlacklistedUsers = currentBlacklistedUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers);
                            SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Music Blacklist Remove",
                                IconUrl = context.Guild.IconUrl
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"ID: {context.User.Id}",
                                IconUrl = context.User.GetAvatarUrl()
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(0, 255, 0),
                                Footer = footer
                            };
                            embed.WithCurrentTimestamp();
                            embed.Description = $"{context.User.Mention} unblacklisted {user.Mention} from using music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully removed blacklisted music command user **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the user **{Format.Sanitize(user.Username)}** is not blacklisted from using music commands.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ListBlacklistedMusicAsync(SocketCommandContext context)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = context.Guild.IconUrl,
                    Name = "List Blacklisted Music"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                    Author = auth
                };
                string[] blacklisted = File.ReadAllText(Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt")).Split("\n", StringSplitOptions.RemoveEmptyEntries);
                string blacklistedUsers = "";
                foreach (string user in blacklisted)
                {
                    string currentUser = context.Guild.GetUser(Convert.ToUInt64(user)).Username + "#" + context.Guild.GetUser(Convert.ToUInt64(user)).Discriminator;
                    blacklistedUsers = blacklistedUsers + currentUser + "\n";
                }
                if (blacklistedUsers.Length <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} there are no blacklisted users from music commands.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Music Users";
                        y.Value = Format.Sanitize(blacklistedUsers);
                        y.IsInline = false;
                    });
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        public async Task BlacklistSuggestionsAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command {prefix}blacklistsuggestions";
                    embed.Description = $"**Description:** Blacklist a user from using the {prefix}suggest command.\n**Usage:** {prefix}blacklistsuggestions [user]\n**Example:** {prefix}blacklistsuggestions Jiggmin";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already blacklisted from suggestions.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers + user.Id.ToString() + "\n");
                            SocketTextChannel suggestions = context.Guild.Channels.Where(x => x.Name.ToUpper() == "suggestions".ToUpper()).First() as SocketTextChannel;
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = $"Blacklisting User | Mod: {context.User.Username}#{context.User.Discriminator}"
                            };
                            await suggestions.AddPermissionOverwriteAsync(user, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny), options);
                            SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Suggestions Blacklist Add",
                                IconUrl = context.Guild.IconUrl
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"ID: {context.User.Id}",
                                IconUrl = context.User.GetAvatarUrl()
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(255, 0, 0),
                                Footer = footer
                            };
                            embed.WithCurrentTimestamp();
                            embed.Description = $"{context.User.Mention} blacklisted {user.Mention} from the suggestions channel.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted **{Format.Sanitize(user.Username)}#{user.Discriminator}** from suggestions.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task UnblacklistSuggestionsAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command {prefix}unblacklistsuggestions";
                    embed.Description = $"**Description:** Unblacklist a user from using the {prefix}suggest command.\n**Usage:** {prefix}unblacklistsuggestions [user]\n**Example:** {prefix}unblacklistsuggestions Jiggmin";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                    {
                        IUser user = Extensions.UserInGuild(context.Message, context.Guild, username);
                        string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"));
                        if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                        {
                            currentBlacklistedUsers = currentBlacklistedUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers);
                            SocketTextChannel suggestions = context.Guild.Channels.Where(x => x.Name.ToUpper() == "suggestions".ToUpper()).First() as SocketTextChannel;
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = $"Unblacklisting User | Mod: {context.User.Username}#{context.User.Discriminator}"
                            };
                            await suggestions.RemovePermissionOverwriteAsync(user, options);
                            SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Suggestions Blacklist Remove",
                                IconUrl = context.Guild.IconUrl
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"ID: {context.User.Id}",
                                IconUrl = context.User.GetAvatarUrl()
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Author = author,
                                Color = new Color(0, 255, 0),
                                Footer = footer
                            };
                            embed.WithCurrentTimestamp();
                            embed.Description = $"{context.User.Mention} unblacklisted {user.Mention} from the suggestions channel.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully removed blacklisted suggestions user **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the user **{Format.Sanitize(user.Username)}** is not blacklisted from suggestions.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ListBlacklistedSuggestionsAsync(SocketCommandContext context)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = context.Guild.IconUrl,
                    Name = "List Blacklisted Suggestions"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                    Author = auth
                };
                string[] blacklisted = File.ReadAllText(Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt")).Split("\n", StringSplitOptions.RemoveEmptyEntries);
                string blacklistedUsers = "";
                foreach (string user in blacklisted)
                {
                    string currentUser = context.Guild.GetUser(Convert.ToUInt64(user)).Username + "#" + context.Guild.GetUser(Convert.ToUInt64(user)).Discriminator;
                    blacklistedUsers = blacklistedUsers + currentUser + "\n";
                }
                if (blacklistedUsers.Length <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} there are no blacklisted suggestions users.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Suggestions Users";
                        y.Value = Format.Sanitize(blacklistedUsers);
                        y.IsInline = false;
                    });
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        public async Task ChannelInfoAsync(SocketCommandContext context, [Remainder] string channelName)
        {
            if (string.IsNullOrWhiteSpace(channelName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}channelinfo";
                embed.Description = $"**Description:** Get information about a channel.\n**Usage:** {prefix}channelinfo [channel name]\n**Example:** {prefix}channelinfo rules";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.ChannelInGuild(context.Message, context.Guild, channelName) != null)
                {
                    SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, channelName);
                    string type = "Text";
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = "Channel Created"
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
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
                        y.Value = Format.Sanitize(channel.Name);
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Position";
                        y.Value = channel.Position;
                        y.IsInline = true;
                    });
                    if (channel is SocketVoiceChannel vChannel)
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
                            embed.AddField(y =>
                            {
                                y.Name = "Category Name";
                                y.Value = Format.Sanitize(vChannel.Category.Name);
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
                    else if (channel is SocketCategoryChannel cChannel)
                    {
                        type = "Category";
                        embed.AddField(y =>
                        {
                            y.Name = "Type";
                            y.Value = type;
                            y.IsInline = true;
                        });
                        int children = 0;
                        foreach (SocketGuildChannel gChannel in context.Guild.Channels)
                        {
                            if (gChannel is SocketTextChannel tChannel)
                            {
                                if (tChannel.CategoryId == cChannel.Id)
                                {
                                    children++;
                                }
                            }
                            else if (gChannel is SocketVoiceChannel vChannel2)
                            {
                                if (vChannel2.CategoryId == cChannel.Id)
                                {
                                    children++;
                                }
                            }
                        }
                        embed.AddField(y =>
                        {
                            y.Name = "Children";
                            y.Value = children;
                            y.IsInline = true;
                        });
                    }
                    else if (channel is SocketTextChannel tChannel)
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
                            embed.AddField(y =>
                            {
                                y.Name = "Category Name";
                                y.Value = Format.Sanitize(tChannel.Category.Name);
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
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find channel with name or ID **{Format.Sanitize(channelName)}**.");
                }
            }
        }

        public async Task MemberCountAsync(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync($"Member count: **{context.Guild.Users.Count}**.");
        }

        public async Task UptimeAsync(SocketCommandContext context)
        {
            Process process = Process.GetCurrentProcess();
            TimeSpan time = DateTime.Now - process.StartTime;
            StringBuilder sb = new StringBuilder();
            if (time.Days > 0)
            {
                sb.Append($"{time.Days}d ");
            }
            if (time.Hours > 0)
            {
                sb.Append($"{time.Hours}h ");
            }
            if (time.Minutes > 0)
            {
                sb.Append($"{time.Minutes}m ");
            }
            sb.Append($"{time.Seconds}s ");
            await context.Channel.SendMessageAsync($"Current uptime: **{sb.ToString()}**");
        }

        public async Task RoleInfoAsync(SocketCommandContext context, [Remainder] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}roleinfo";
                embed.Description = $"**Description:** Get information about a role.\n**Usage:** {prefix}roleinfo [role name]\n**Example:** {prefix}roleinfo Admins";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = "Role Created"
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = role.Color,
                        ThumbnailUrl = "https://dummyimage.com/80x80/" + Extensions.HexConverter(role.Color) + "/" + Extensions.HexConverter(role.Color),
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
                        y.Value = Format.Sanitize(role.Name);
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Color";
                        y.Value = "`#" + Extensions.HexConverter(role.Color) + "`";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Mention";
                        y.Value = $"`{role.Mention.ToString()}`";
                        y.IsInline = true;
                    });
                    int roleMembers = 0;
                    foreach (IGuildUser user in context.Guild.Users)
                    {
                        IReadOnlyCollection<ulong> roles = user.RoleIds;
                        foreach (ulong id in roles)
                        {
                            if (id == role.Id)
                            {
                                roleMembers += 1;
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
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find role with name or ID **{Format.Sanitize(roleName)}**.");
                }
            }
        }

        public async Task RolesAsync(SocketCommandContext context)
        {
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                Name = "Server Roles",
                IconUrl = context.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                IconUrl = context.User.GetAvatarUrl(),
                Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                Author = auth,
                Footer = footer
            };
            embed.WithCurrentTimestamp();
            string roleNames = "", roleMemberCounts = "";
            foreach (SocketRole role in context.Guild.Roles.OrderByDescending(x => x.Position))
            {
                if (!role.IsEveryone)
                {
                    roleNames = roleNames + role.Name + "\n";
                    roleMemberCounts = roleMemberCounts + role.Members.Count() + " members\n";
                }
            }
            embed.AddField(y =>
            {
                y.Name = "Role";
                y.Value = Format.Sanitize(roleNames);
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Member Count";
                y.Value = roleMemberCounts;
                y.IsInline = true;
            });
            await context.Channel.SendMessageAsync("", false, embed.Build());
        }

        public async Task WarningsAsync(SocketCommandContext context, [Remainder] string username)
        {
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder();
            List<Ban> warnings;
            if (string.IsNullOrWhiteSpace(username))
            {
                auth.Name = $"Warnings - {context.Guild.Name}";
                auth.IconUrl = context.Guild.IconUrl;
                warnings = Ban.Warnings(context.Guild.Id);
                if (warnings.Count <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} nobody has been warned on this server.");
                    return;
                }
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, username);
                    auth.Name = $"Warnings - {user.Username}#{user.Discriminator}";
                    auth.IconUrl = user.GetAvatarUrl();
                    warnings = Ban.Warnings(context.Guild.Id, user);
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                    return;
                }
            }
            if (warnings.Count > 0)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220),
                    Author = auth
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = context.User.GetAvatarUrl(),
                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                };
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();
                bool first = true;

                foreach (Ban warning in warnings)
                {
                    string text = $"**User:** {Format.Sanitize(warning.Username)} ({warning.UserID})\n**Moderator:** {Format.Sanitize(warning.Moderator)} ({warning.ModeratorID})\n**Reason:** {Format.Sanitize(warning.Reason)}";
                    embed.AddField("Case " + warning.Case.ToString(), text);
                    if (embed.Fields.Count == 25)
                    {
                        if (first)
                        {
                            if (warnings.Count == 1)
                            {
                                await context.Channel.SendMessageAsync($"**{warnings.Count}** warning found.", false, embed.Build());
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"**{warnings.Count}** warnings found.", false, embed.Build());
                            }
                            first = false;
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                        embed.Fields.Clear();
                    }
                }
                if (first)
                {
                    if (warnings.Count == 1)
                    {
                        await context.Channel.SendMessageAsync($"**{warnings.Count}** warning found.", false, embed.Build());
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"**{warnings.Count}** warnings found.", false, embed.Build());
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }

            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} this user has no warnings.");
            }
        }

        public async Task UnbanAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}unban";
                embed.Description = $"**Description:** Unban a member\n**Usage:** {prefix}unban [user], [optional reason]\n**Example:** {prefix}unban @Jiggmin, Appealed!";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason != null && reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                bool isBanned = false;
                IUser user = null;
                IReadOnlyCollection<Discord.Rest.RestBan> bans = await context.Guild.GetBansAsync();
                foreach (IBan ban in bans)
                {
                    if (ban.User.Username.ToUpper() == username.ToUpper() || ban.User.Id.ToString() == username)
                    {
                        user = ban.User;
                        isBanned = true;
                    }
                }
                if (isBanned)
                {
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Unban | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
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
                        y.Value = context.User.Mention;
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    if (reason == null)
                    {
                        Ban ban = new Ban()
                        {
                            GuildID = long.Parse(context.Guild.Id.ToString()),
                            Case = Ban.CaseCount(context.Guild.Id) + 1,
                            UserID = long.Parse(user.Id.ToString()),
                            Username = user.Username + "#" + user.Discriminator,
                            Type = "Unban",
                            ModeratorID = long.Parse(context.User.Id.ToString()),
                            Moderator = context.User.Username + "#" + context.User.Discriminator,
                            Reason = "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                        };
                        Ban.Add(ban);
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"No Reason Given | Mod: {context.User.Username}#{context.User.Discriminator}"
                        };
                        await context.Guild.RemoveBanAsync(user, options);
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was unbanned.");
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        Ban ban = new Ban()
                        {
                            GuildID = long.Parse(context.Guild.Id.ToString()),
                            Case = Ban.CaseCount(context.Guild.Id) + 1,
                            UserID = long.Parse(user.Id.ToString()),
                            Username = user.Username + "#" + user.Discriminator,
                            Type = "Unban",
                            ModeratorID = long.Parse(context.User.Id.ToString()),
                            Moderator = context.User.Username + "#" + context.User.Discriminator,
                            Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                        };
                        Ban.Add(ban);
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"{reason} | Mod: {context.User.Username}#{context.User.Discriminator}"
                        };
                        await context.Guild.RemoveBanAsync(user, options);
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was unbanned.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} **{Format.Sanitize(username)}** is not banned.");
                }
            }
        }

        public async Task UndeafenAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}undeafen";
                embed.Description = $"**Description:** Undeafen a member\n**Usage:** {prefix}undeafen [user], [optional reason]\n**Example:** {prefix}undeafen @Jiggmin Listen now!";
                await context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason != null && reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, username);
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0 || DiscordStaff.Get(context.Guild.Id, "r-" + (user as SocketGuildUser).Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0 || user.Id == context.Client.CurrentUser.Id)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if ((user as SocketGuildUser).Roles.OrderBy(x => x.Position).Last().Position >= context.Guild.GetUser(context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Undeafen | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 0, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
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
                        y.Value = context.User.Mention;
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    if (reason == null)
                    {
                        Ban ban = new Ban()
                        {
                            GuildID = long.Parse(context.Guild.Id.ToString()),
                            Case = Ban.CaseCount(context.Guild.Id) + 1,
                            UserID = long.Parse(user.Id.ToString()),
                            Username = user.Username + "#" + user.Discriminator,
                            Type = "Undeafen",
                            ModeratorID = long.Parse(context.User.Id.ToString()),
                            Moderator = context.User.Username + "#" + context.User.Discriminator,
                            Reason = "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                        };
                        Ban.Add(ban);
                        await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = false);
                        try
                        {
                            await user.SendMessageAsync($"You have been undeafened on **{Format.Sanitize(context.Guild.Name)}** by **{context.User.Mention}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was undeafened.");
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        Ban ban = new Ban()
                        {
                            GuildID = long.Parse(context.Guild.Id.ToString()),
                            Case = Ban.CaseCount(context.Guild.Id) + 1,
                            UserID = long.Parse(user.Id.ToString()),
                            Username = user.Username + "#" + user.Discriminator,
                            Type = "Undeafen",
                            ModeratorID = long.Parse(context.User.Id.ToString()),
                            Moderator = context.User.Username + "#" + context.User.Discriminator,
                            Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                        };
                        Ban.Add(ban);
                        await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = false);
                        try
                        {
                            await user.SendMessageAsync($"You have been undeafened on **{Format.Sanitize(context.Guild.Name)}** by **{context.User.Mention}** with reason **{Format.Sanitize(reason)}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was undeafened.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task DeafenAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}deafen";
                embed.Description = $"**Description:** Deafen a member\n**Usage:** {prefix}deafen [user], [optional reason]\n**Example:** {prefix}deafen @Jiggmin Don't listen!";
                await context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason != null && reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0 || DiscordStaff.Get(context.Guild.Id, "r-" + user.Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0 || user.Id == context.Client.CurrentUser.Id)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if (user.Roles.OrderBy(x => x.Position).Last().Position >= context.Guild.GetUser(context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Deafen | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 0, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
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
                        y.Value = context.User.Mention;
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    if (reason == null)
                    {
                        Ban ban = new Ban()
                        {
                            GuildID = long.Parse(context.Guild.Id.ToString()),
                            Case = Ban.CaseCount(context.Guild.Id) + 1,
                            UserID = long.Parse(user.Id.ToString()),
                            Username = user.Username + "#" + user.Discriminator,
                            Type = "Deafen",
                            ModeratorID = long.Parse(context.User.Id.ToString()),
                            Moderator = context.User.Username + "#" + context.User.Discriminator,
                            Reason = "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                        };
                        Ban.Add(ban);
                        await user.ModifyAsync(x => x.Deaf = true);
                        try
                        {
                            await user.SendMessageAsync($"You have been deafened on **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention}.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was deafened.");
                    }
                    else
                    {
                        embed.AddField(y =>
                        {
                            y.Name = "Reason";
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        Ban ban = new Ban()
                        {
                            GuildID = long.Parse(context.Guild.Id.ToString()),
                            Case = Ban.CaseCount(context.Guild.Id) + 1,
                            UserID = long.Parse(user.Id.ToString()),
                            Username = user.Username + "#" + user.Discriminator,
                            Type = "Deafen",
                            ModeratorID = long.Parse(context.User.Id.ToString()),
                            Moderator = context.User.Username + "#" + context.User.Discriminator,
                            Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                        };
                        Ban.Add(ban);
                        await user.ModifyAsync(x => x.Deaf = true);
                        try
                        {
                            await user.SendMessageAsync($"You have been deafened on **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was deafened.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task SoftbanAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(reason))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}softban";
                embed.Description = $"**Description:** Softban a member (ban and immediate unban to delete user messages)\n**Usage:** {prefix}softban [user] [reason]\n**Example:** {prefix}softban @Jiggmin Not making Fred admin!";
                await context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0 || DiscordStaff.Get(context.Guild.Id, "r-" + user.Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0 || user.Id == context.Client.CurrentUser.Id)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if (user.Roles.OrderBy(x => x.Position).Last().Position >= context.Guild.GetUser(context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Softban | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
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
                        y.Value = context.User.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    Ban ban = new Ban()
                    {
                        GuildID = long.Parse(context.Guild.Id.ToString()),
                        Case = Ban.CaseCount(context.Guild.Id) + 1,
                        UserID = long.Parse(user.Id.ToString()),
                        Username = user.Username + "#" + user.Discriminator,
                        Type = "Softban",
                        ModeratorID = long.Parse(context.User.Id.ToString()),
                        Moderator = context.User.Username + "#" + context.User.Discriminator,
                        Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                    };
                    Ban.Add(ban);
                    try
                    {
                        await user.SendMessageAsync($"You have been softbanned on **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //cant send message
                    }
                    await context.Guild.AddBanAsync(user, 7, $"{reason} | Mod: {context.User.Username}#{context.User.Discriminator}");
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Softban | Mod: {context.User.Username}#{context.User.Discriminator}"
                    };
                    await context.Guild.RemoveBanAsync(user, options);
                    await banlog.SendMessageAsync("", false, embed.Build());
                    await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was softbanned.");
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task GetCaseAsync(SocketCommandContext context, string caseN)
        {
            if (string.IsNullOrWhiteSpace(caseN) || !int.TryParse(caseN, out _))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}getcase";
                embed.Description = $"**Description:** Get info on a case.\n**Usage:** {prefix}getcase [case number]\n**Example:** {prefix}getcase 1";
                await context.Channel.SendMessageAsync("", false, embed.Build());
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
                    IconUrl = context.User.GetAvatarUrl(),
                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                };
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();
                Ban ban = Ban.GetCase("`case`", caseN);
                embed.Description = $"**User:** {Format.Sanitize(ban.Username)} ({ban.UserID})\n**Moderator:** {Format.Sanitize(ban.Moderator)} ({ban.ModeratorID})\n**Type:** {ban.Type}\n**Reason:** {Format.Sanitize(ban.Reason)}";
                if (embed.Description.Length <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} case could not be found.");
                }
                else
                {
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task ModlogsAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}modlogs";
                embed.Description = $"**Description:** Get a list of mod logs for a user.\n**Usage:** {prefix}modlogs [user]\n**Example:** {prefix}modlogs @Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, username);
                    List<Ban> modlogs = Ban.GetPriors(context.Guild.Id, user);
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
                        IconUrl = context.User.GetAvatarUrl(),
                        Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                    };
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    if (modlogs.Count <= 0)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} this user has no priors.");
                    }
                    else
                    {
                        bool first = true;
                        foreach (Ban modlog in modlogs)
                        {
                            string text = $"**User:** {modlog.Username} ({modlog.UserID})\n**Moderator:** {modlog.Moderator} ({modlog.ModeratorID})\n**Type:** {modlog.Type}\n**Reason:** {Format.Sanitize(modlog.Reason)}";
                            embed.AddField("Case " + modlog.Case.ToString(), text);
                            if (embed.Fields.Count == 25)
                            {
                                if (first)
                                {
                                    if (modlogs.Count == 1)
                                    {
                                        await context.Channel.SendMessageAsync($"**{modlogs.Count}** log found.", false, embed.Build());
                                    }
                                    else
                                    {
                                        await context.Channel.SendMessageAsync($"**{modlogs.Count}** logs found.", false, embed.Build());
                                    }
                                    first = false;
                                }
                                else
                                {
                                    await context.Channel.SendMessageAsync("", false, embed.Build());
                                }
                                embed.Fields.Clear();
                            }
                        }
                        if (first)
                        {
                            if (modlogs.Count == 1)
                            {
                                await context.Channel.SendMessageAsync($"**{modlogs.Count}** log found.", false, embed.Build());
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"**{modlogs.Count}** logs found.", false, embed.Build());
                            }
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task ReasonAsync(SocketCommandContext context, string caseN, [Remainder] string reason)
        {
            if (caseN == null || reason == null || !int.TryParse(caseN, out int level_))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}reason";
                embed.Description = $"**Description:** Edit a reason for a mod log.\n**Usage:** {prefix}reason [case number] [reason]\n**Example:** {prefix}reason 1 Be nice :)";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                IAsyncEnumerable<IMessage> messages = banlog.GetMessagesAsync().Flatten().Where(x => x.Embeds.Count > 0);
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
                    await context.Channel.SendMessageAsync($"{context.User.Mention} that case could not be found.");
                }
                else
                {
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
                        Text = $"{footerText}"
                    };
                    foreach (EmbedField field in msgEmbed.Fields)
                    {
                        if (field.Name.Equals("Reason"))
                        {
                            embed.AddField(y =>
                            {
                                y.Name = field.Name;
                                y.Value = Format.Sanitize(reason);
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
                    Ban.SetValue(caseN, context.Guild.Id, "reason", reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                    await context.Message.DeleteAsync();
                    await context.Channel.SendMessageAsync($"{context.User.Mention} updated case {caseN}.");
                    embed.Author.Name = "Updated Case";
                    embed.Author.IconUrl = context.Guild.IconUrl;
                    embed.Footer.Text = "ID: " + context.User.Id;
                    embed.Footer.IconUrl = context.User.GetAvatarUrl();
                    embed.Color = new Color(0, 0, 255);
                    embed.Fields.Clear();
                    embed.Description = $"{context.User.Mention} updated case **{caseN}**.";
                    if (Extensions.GetLogChannel(context.Guild) != null)
                    {
                        await Extensions.GetLogChannel(context.Guild).SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task WarnAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(reason))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}warn";
                embed.Description = $"**Description:** Warn a member.\n**Usage:** {prefix}warn [user] [reason]\n**Example:** {prefix}warn @Jiggmin No flooding";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0 || DiscordStaff.Get(context.Guild.Id, "r-" + user.Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0 || user.Id == context.Client.CurrentUser.Id)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if (user.Roles.OrderBy(x => x.Position).Last().Position >= context.Guild.GetUser(context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Warn | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
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
                        y.Value = context.User.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    Ban ban = new Ban()
                    {
                        GuildID = long.Parse(context.Guild.Id.ToString()),
                        Case = Ban.CaseCount(context.Guild.Id) + 1,
                        UserID = long.Parse(user.Id.ToString()),
                        Username = user.Username + "#" + user.Discriminator,
                        Type = "Warn",
                        ModeratorID = long.Parse(context.User.Id.ToString()),
                        Moderator = context.User.Username + "#" + context.User.Discriminator,
                        Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                    };
                    Ban.Add(ban);
                    try
                    {
                        await user.SendMessageAsync($"You have been warned on **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //cant send message
                    }
                    await banlog.SendMessageAsync("", false, embed.Build());
                    await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was warned.");
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task EndGiveawayAsync(SocketCommandContext context)
        {
            IEnumerable<IMessage> messages = await context.Channel.GetMessagesAsync(100).FlattenAsync();
            IUserMessage message = null;
            IEmbed msgEmbed = null;
            EmbedBuilder embed = new EmbedBuilder();
            int winners = 0;
            foreach (IMessage msg in messages)
            {
                if (msg.Content.Equals(":confetti_ball: **Giveaway** :confetti_ball:") && msg.Author.Id == context.Guild.CurrentUser.Id)
                {
                    message = msg as IUserMessage;
                    IReadOnlyCollection<IEmbed> msgEmbeds = msg.Embeds;
                    msgEmbed = msgEmbeds.ElementAt(0);
                    embed.Title = msgEmbed.Title;
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = msgEmbed.Footer.Value.IconUrl
                    };
                    winners = int.Parse(Extensions.GetBetween(msgEmbed.Footer.Value.Text, "Winners: ", " | Ends at"));
                    embed.WithFooter(footer);
                    break;
                }
            }
            if (message != null)
            {
                embed.Footer.Text = $"Winners: {winners} | Ended at";
                embed.WithCurrentTimestamp();
                await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");

                IEnumerable<IUser> users = await message.GetReactionUsersAsync(Emote.Parse("<:artifact:530404386229321749>"), 9999).FlattenAsync();
                if (users.Count() <= 1)
                {
                    await context.Channel.SendMessageAsync("Nobody entered the giveaway.");
                    embed.Description = "Nobody entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (users.Count() <= winners)
                {
                    await context.Channel.SendMessageAsync("Not enough users entered the giveaway.");
                    embed.Description = "Not enough users entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (winners == 1)
                {
                    IUser randomUser = users.GetRandomElement();
                    while (randomUser.Id == context.Guild.CurrentUser.Id)
                    {
                        randomUser = users.GetRandomElement();
                    }
                    embed.Description = $"Winner: {randomUser.Mention}";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await context.Channel.SendMessageAsync($"The winner of the {embed.Title} is {randomUser.Mention} !");
                }
                else
                {
                    List<IUser> userWinners = new List<IUser>();
                    for (int i = 0; i < winners; i++)
                    {
                        IUser randomUser = users.GetRandomElement();
                        while (randomUser.Id == context.Guild.CurrentUser.Id || userWinners.Contains(randomUser))
                        {
                            randomUser = users.GetRandomElement();
                        }
                        userWinners.Add(randomUser);
                    }
                    string description = "";
                    foreach (IUser userWinner in userWinners)
                    {
                        description = description + userWinner.Mention + ", ";
                    }
                    embed.Description = $"Winners: {description}";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await context.Channel.SendMessageAsync($"The winners of the {embed.Title} are {description.Substring(0, description.Length - 2)} !");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find a giveaway in this channel.");
            }
        }

        public async Task RepickAsync(SocketCommandContext context)
        {
            IEnumerable<IMessage> messages = await context.Channel.GetMessagesAsync(100).FlattenAsync();
            IUserMessage message = null;
            IEmbed msgEmbed = null;
            EmbedBuilder embed = new EmbedBuilder();
            int winners = 0;
            foreach (IMessage msg in messages)
            {
                if (msg.Content.Equals(":confetti_ball: **Giveaway Ended** :confetti_ball:") && msg.Author.Id == context.Guild.CurrentUser.Id)
                {
                    message = msg as IUserMessage;
                    msgEmbed = msg.Embeds.ElementAt(0);
                    break;
                }
            }
            if (message != null && msgEmbed != null)
            {
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    Text = msgEmbed.Footer.Value.Text,
                    IconUrl = msgEmbed.Footer.Value.IconUrl
                };
                embed.WithColor(new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)));
                embed.WithFooter(footer);
                embed.WithTitle(msgEmbed.Title);
                embed.WithTimestamp(msgEmbed.Timestamp.Value);
                IEnumerable<IUser> users = await message.GetReactionUsersAsync(Emote.Parse("<:artifact:530404386229321749>"), 9999).FlattenAsync();
                winners = int.Parse(Extensions.GetBetween(msgEmbed.Footer.Value.Text, "Winners: ", " | Ended at"));
                if (users.Count() <= 1)
                {
                    await context.Channel.SendMessageAsync("Nobody entered the giveaway.");
                    embed.Description = $"Nobody entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (users.Count() == 2 && winners == 1)
                {
                    await context.Channel.SendMessageAsync("Nobody else can win the giveaway.");
                    embed.Description = $"Nobody else can win the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (users.Count() <= winners)
                {
                    await context.Channel.SendMessageAsync("Not enough users entered the giveaway.");
                    embed.Description = "Not enough users entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (winners == 1)
                {
                    IUser randomUser = users.GetRandomElement();
                    string oldWinner = msgEmbed.Description.Substring(8, msgEmbed.Description.Length - 8);
                    while (randomUser.Id == context.Guild.CurrentUser.Id || randomUser.Mention.ToString().Equals(oldWinner))
                    {
                        randomUser = users.GetRandomElement();
                    }
                    embed.Description = $"Winner: {randomUser.Mention}";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await context.Channel.SendMessageAsync($"The new winner of the {embed.Title} is {randomUser.Mention} !");
                }
                else
                {
                    List<IUser> userWinners = new List<IUser>();
                    for (int i = 0; i < winners; i++)
                    {
                        IUser randomUser = users.GetRandomElement();
                        while (randomUser.Id == context.Guild.CurrentUser.Id || userWinners.Contains(randomUser))
                        {
                            randomUser = users.GetRandomElement();
                        }
                        userWinners.Add(randomUser);
                    }
                    string description = "";
                    foreach (IUser userWinner in userWinners)
                    {
                        description = description + userWinner.Mention + ", ";
                    }
                    embed.Description = $"Winners: {description.Substring(0, description.Length - 2)}";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await context.Channel.SendMessageAsync($"The new winners of the {embed.Title} are {description.Substring(0, description.Length - 2)} !");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find a giveaway in this channel.");
            }
        }

        public async Task GiveawayAsync(SocketCommandContext context, string channel, string time, string winnersS, [Remainder] string item)
        {
            if (channel == null || time == null || !double.TryParse(time, out double num2) || Math.Round(Convert.ToDouble(time), 0) < 0 || !int.TryParse(winnersS, out int winners) || winners < 1 || item == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}giveaway";
                embed.Description = $"**Description:** Create a giveaway.\n**Usage:** {prefix}giveaway [channel] [time] [winners] [item]\n**Example:** {prefix}giveaway pr2-discussion 60 1 Cowboy Hat";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                SocketTextChannel giveawayChannel = null;
                try
                {
                    if (Extensions.ChannelInGuild(context.Message, context.Guild, channel) != null)
                    {
                        if (Extensions.ChannelInGuild(context.Message, context.Guild, channel) is SocketTextChannel)
                        {
                            giveawayChannel = Extensions.ChannelInGuild(context.Message, context.Guild, channel) as SocketTextChannel;
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the channel `{channel}` is not a text channel so a giveaway cannot happen there.");
                            return;
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find text channel `{channel}`.");
                        return;
                    }
                }
                catch (NullReferenceException)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find a text channel with ID: `{channel}`.");
                    return;
                }
                double minutes = Math.Round(Convert.ToDouble(time), 0);
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = context.User.GetAvatarUrl(),
                    Text = $"Winners: {winners} | Ends at"
                };
                embed.WithFooter(footer);
                embed.Title = $"{item}";
                embed.WithTimestamp(DateTime.Now.AddMinutes(minutes));
                embed.Description = $"React with <:artifact:530404386229321749> to enter the giveaway.\nTime left: {minutes} minutes.";
                IUserMessage message = null;
                try
                {
                    message = await giveawayChannel.SendMessageAsync(":confetti_ball: **Giveaway** :confetti_ball:", false, embed.Build());
                }
                catch (Discord.Net.HttpException)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I do not have permission to speak in that channel.");
                    return;
                }
                await message.AddReactionAsync(Emote.Parse("<:artifact:530404386229321749>"));
                int temptime = Convert.ToInt32(minutes) * 60000, divide = Convert.ToInt32(minutes / (minutes / 10)), count = 1;
                bool ended = false;
                while (count < divide)
                {
                    await Task.Delay(temptime / divide);
                    message = await context.Channel.GetMessageAsync(message.Id) as IUserMessage;
                    if (message.Content.Equals(":confetti_ball: **Giveaway Ended** :confetti_ball:"))
                    {
                        count += divide;
                        ended = true;
                    }
                    else
                    {
                        embed.Description = $"React with <:artifact:530404386229321749> to enter the giveaway.\nTime left: {minutes - (minutes / 10 * count)} minutes.";
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                        count += 1;
                    }
                }
                if (!ended)
                {
                    IAsyncEnumerable<IReadOnlyCollection<IUser>> user = message.GetReactionUsersAsync(Emote.Parse("<:artifact:530404386229321749>"), 9999);
                    IReadOnlyCollection<IUser> users = user.ElementAt(0).Result;
                    if (users.Count <= 1)
                    {
                        await giveawayChannel.SendMessageAsync("Nobody entered the giveaway.");
                        embed.Description = $"Nobody entered the giveaway.";
                        embed.Footer.Text = $"Winners: {winners} | Ended at";
                        await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                        return;
                    }
                    if (users.Count <= winners)
                    {
                        await giveawayChannel.SendMessageAsync("Not enough users entered the Giveaway.");
                        embed.Description = $"Not enough users entered the Giveaway.";
                        embed.Footer.Text = $"Winners: {winners} | Ended at";
                        await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                        return;
                    }
                    List<IUser> userWinners = new List<IUser>();
                    for (int i = 0; i < winners; i++)
                    {
                        IUser randomUser = users.GetRandomElement();
                        while (randomUser.Id == context.Client.CurrentUser.Id || userWinners.Contains(randomUser))
                        {
                            randomUser = users.GetRandomElement();
                        }
                        userWinners.Add(randomUser);
                    }
                    await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");
                    embed.Footer.Text = $"Winners: {winners} | Ended at";
                    if (winners == 1)
                    {
                        embed.Description = $"Winner: {userWinners.ElementAt(0).Mention}";
                        await giveawayChannel.SendMessageAsync($"The winner of the {item} is {userWinners.ElementAt(0).Mention} !");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                    else
                    {
                        string description = "";
                        foreach (IUser userWinner in userWinners)
                        {
                            description = description + userWinner.Mention + ", ";
                        }
                        embed.Description = $"Winners: {description}";
                        await giveawayChannel.SendMessageAsync($"The winners of the {item} are {description.Substring(0, description.Length - 2)} !");
                        await message.ModifyAsync(x => x.Embed = embed.Build());
                    }
                }
            }
        }

        public async Task PromoteAsync(SocketCommandContext context, string type, [Remainder] string username)
        {
            if (context.User.Id == 286922861044563969 || context.User.Id == 239157630591959040 || context.User.Id == 364951508955037696)
            {
                if (type == null || username == null)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command {prefix}promote";
                    embed.Description = $"**Description:** Annonce a PR2 promotion.\n**Usage:** {prefix}promote [type] [user]\n**Example:** {prefix}promote temp @Jiggmin";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                    if (type.Equals("temp", StringComparison.InvariantCultureIgnoreCase) || type.Equals("temporary", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await context.Message.DeleteAsync();
                        await context.Channel.SendMessageAsync($"*{context.User.Mention} has promoted {user.Mention} to a temporary moderator! " +
                        $"May they reign in hours of peace and prosperity! " +
                        $"Make sure you read the moderator guidelines at https://jiggmin2.com/forums/showthread.php?tid=12*");
                    }
                    else if (type.Equals("trial", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await context.Message.DeleteAsync();
                        await context.Channel.SendMessageAsync($"*{context.User.Mention} has promoted {user.Mention} to a trial moderator! " +
                        $"May they reign in days of peace and prosperity! " +
                        $"Make sure you read the moderator guidelines at https://jiggmin2.com/forums/showthread.php?tid=12*");
                        IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "PR2 Staff Member".ToUpper());
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"New PR2 Staff Member | Mod: {context.User.Username}#{context.User.Discriminator}"
                        };
                        await user.AddRolesAsync(role, options);
                    }
                    else if (type.Equals("perm", StringComparison.InvariantCultureIgnoreCase) || type.Equals("permanent", StringComparison.InvariantCultureIgnoreCase))
                    {
                        await context.Message.DeleteAsync();
                        await context.Channel.SendMessageAsync($"*{context.User.Mention} has promoted {user.Mention} to a permanent moderator! " +
                        $"May they reign in 1,000 years of peace and prosperity! " +
                        $"Make sure you read the moderator guidelines at https://jiggmin2.com/forums/showthread.php?tid=12*");
                        IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "PR2 Staff Member".ToUpper());
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"New PR2 Staff Member | Mod: {context.User.Username}#{context.User.Discriminator}"
                        };
                        await user.AddRolesAsync(role, options);
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the promotion type **{Format.Sanitize(type)}** was not recognised.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task UnmuteAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}unmute";
                embed.Description = $"**Description:** Unmute a user.\n**Usage:** {prefix}unmute [user], [optional reason]\n**Example:** {prefix}unmute @Jiggmin, Speak now";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason != null && reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                    if (!user.Roles.Any(e => e.Name == "Muted"))
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} this user is not muted.");
                    }
                    else
                    {
                        IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Muted".ToUpper());
                        RequestOptions options = new RequestOptions();
                        EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                        {
                            Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Unmute | {user.Username}#{user.Discriminator}",
                            IconUrl = user.GetAvatarUrl(),
                        };
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(0, 255, 0),
                            Author = auth
                        };
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {user.Id}"
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
                            y.Value = context.User.Mention;
                            y.IsInline = true;
                        });
                        if (reason == null)
                        {
                            options.AuditLogReason = $"Unmuting User | Mod: {context.User.Username}#{context.User.Discriminator}";
                            Ban ban = new Ban()
                            {
                                GuildID = long.Parse(context.Guild.Id.ToString()),
                                Case = Ban.CaseCount(context.Guild.Id) + 1,
                                UserID = long.Parse(user.Id.ToString()),
                                Username = user.Username + "#" + user.Discriminator,
                                Type = "Unmute",
                                ModeratorID = long.Parse(context.User.Id.ToString()),
                                Moderator = context.User.Username + "#" + context.User.Discriminator,
                                Reason = "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                            };
                            Ban.Add(ban);
                            try
                            {
                                await user.SendMessageAsync($"{context.User.Mention} you have been unmuted in **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention}.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //can't send message
                            }
                        }
                        else
                        {
                            options.AuditLogReason = $"Unmuting User | Mod: {context.User.Username}#{context.User.Discriminator} | Reason: {reason}";
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = Format.Sanitize(reason);
                                y.IsInline = true;
                            });
                            Ban ban = new Ban()
                            {
                                GuildID = long.Parse(context.Guild.Id.ToString()),
                                Case = Ban.CaseCount(context.Guild.Id) + 1,
                                UserID = long.Parse(user.Id.ToString()),
                                Username = user.Username + "#" + user.Discriminator,
                                Type = "Unmute",
                                ModeratorID = long.Parse(context.User.Id.ToString()),
                                Moderator = context.User.Username + "#" + context.User.Discriminator,
                                Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                            };
                            Ban.Add(ban);
                            try
                            {
                                await user.SendMessageAsync($"{context.User.Mention} you have been unmuted in **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //can't send message
                            }
                        }
                        await context.Message.DeleteAsync();
                        SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                        await user.RemoveRolesAsync(role, options);
                        string mutedUsers = File.ReadAllText(Path.Combine(Extensions.downloadPath, "MutedUsers.txt")).Replace("\n" + user.Id.ToString(), string.Empty);
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "MutedUsers.txt"), mutedUsers);
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await context.Channel.SendMessageAsync($"Unmuted **{Format.Sanitize(user.Username)}#{user.Discriminator}**");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task PingAsync(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync($"**{context.Client.Latency.ToString()} ms**");
        }

        public async Task BotInfoAsync(SocketCommandContext context)
        {
            using (Process process = Process.GetCurrentProcess())
            {
                EmbedBuilder embed = new EmbedBuilder();
                Discord.Rest.RestApplication application = await context.Client.GetApplicationInfoAsync();
                embed.ImageUrl = application.IconUrl;
                embed.WithCurrentTimestamp();
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = "Bot Info"
                };
                embed.WithAuthor(auth);
                embed.WithColor(new Color(0x4900ff))
                .AddField(y =>
                {
                    y.Name = "Author.";
                    y.Value = Format.Sanitize(application.Owner.Username); application.Owner.Id.ToString();
                    y.IsInline = false;
                })
                .AddField(y =>
                {
                    y.Name = "Uptime.";
                    TimeSpan time = DateTime.Now - process.StartTime;
                    StringBuilder sb = new StringBuilder();
                    if (time.Days > 0)
                    {
                        sb.Append($"{time.Days}d ");
                    }
                    if (time.Hours > 0)
                    {
                        sb.Append($"{time.Hours}h ");
                    }
                    if (time.Minutes > 0)
                    {
                        sb.Append($"{time.Minutes}m ");
                    }
                    sb.Append($"{time.Seconds}s ");
                    y.Value = sb.ToString();
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Discord.net version";
                    y.Value = DiscordConfig.Version;
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Server Amount";
                    y.Value = context.Client.Guilds.Count.ToString();
                    y.IsInline = false;
                })
                .AddField(y =>
                {
                    y.Name = "Heap Size";
                    y.Value = Extensions.GetHeapSize();
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Number Of Users";
                    y.Value = context.Client.Guilds.Sum(g => g.Users.Count).ToString();
                    y.IsInline = true;
                })
                .AddField(y =>
                {
                    y.Name = "Channels";
                    y.Value = context.Client.Guilds.Sum(g => g.Channels.Count).ToString();
                    y.IsInline = true;
                });
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = context.User.GetAvatarUrl(),
                    Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})"
                };
                embed.WithFooter(footer);
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task UserInfoAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (username == null)
            {
                username = context.User.Username + "#" + context.User.Discriminator;
            }
            if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
            {
                SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                string createdMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(user.CreatedAt.Month);
                string createdDay = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(user.CreatedAt.DayOfWeek);
                string date = $"{createdDay}, {createdMonth} {user.CreatedAt.Day}, {user.CreatedAt.Year} {user.CreatedAt.DateTime.ToString("h:mm tt")}";
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = user.Username + "#" + user.Discriminator,
                    IconUrl = user.GetAvatarUrl(),
                };
                EmbedBuilder embed = new EmbedBuilder()

                {
                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                    Author = auth
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = context.User.GetAvatarUrl(),
                    Text = $"ID: {user.Id}"
                };
                embed.Description = $"{user.Mention}";
                string roleList = "";
                IOrderedEnumerable<SocketGuildUser> guildUsers = context.Guild.Users.OrderBy(x => x.JoinedAt);
                int position = 0;
                string joinedMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(user.JoinedAt.Value.Month);
                string joinedDay = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(user.JoinedAt.Value.DayOfWeek);
                string joined = $"{joinedDay}, {joinedMonth} {user.JoinedAt.Value.Day}, {user.JoinedAt.Value.Year} {user.JoinedAt.Value.LocalDateTime.ToString("h:mm tt")}";
                if (!User.Exists(context.User))
                {
                    User.Add(context.User);
                }
                string pr2name = User.GetUser("user_id", context.User.Id.ToString()).PR2Name;
                position = guildUsers.ToList().IndexOf(user);
                foreach (SocketRole role in user.Roles)
                {
                    if (!role.IsEveryone)
                    {
                        roleList = roleList + role.Mention + " ";
                    }
                }
                embed.AddField(y =>
                {
                    y.Name = "Status";
                    y.Value = user.Status;
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
                if (pr2name != null)
                {
                    embed.AddField(y =>
                    {
                        y.Name = "PR2 Name";
                        y.Value = Format.Sanitize(pr2name);
                        y.IsInline = true;
                    });
                }
                embed.AddField(y =>
                {
                    y.Name = "JV2 Name";
                    y.Value = "N/A";
                    y.IsInline = true;
                });
                if ((user.Roles.Count - 1) > 0)
                {
                    embed.AddField(y =>
                    {
                        y.Name = $"Roles [{user.Roles.Count - 1}]";
                        y.Value = roleList;
                        y.IsInline = false;
                    });
                }
                if (context.Guild.Id == 528679522707701760)
                {
                    embed.AddField(y =>
                    {
                        y.Name = $"Priors";
                        y.Value = Ban.GetPriors(context.Guild.Id, user).Count;
                        y.IsInline = false;
                    });
                }
                embed.ThumbnailUrl = user.GetAvatarUrl();
                embed.WithFooter(footer);
                embed.WithCurrentTimestamp();

                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
            }
        }

        public async Task ServerInfoAsync(SocketCommandContext context)
        {
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                Name = context.Guild.Name,
                IconUrl = context.Guild.IconUrl,
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                Author = auth
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                IconUrl = context.User.GetAvatarUrl(),
                Text = $"ID: {context.Guild.Id} | Server Created"
            };
            await context.Guild.DownloadUsersAsync();
            embed.AddField(y =>
            {
                y.Name = "Owner";
                y.Value = context.Guild.Owner.Mention;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Region";
                y.Value = context.Guild.VoiceRegionId;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Channel Categories";
                y.Value = context.Guild.Channels.Where(x => x is SocketCategoryChannel).Count();
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Text Channels";
                y.Value = context.Guild.Channels.Where(x => x is SocketTextChannel).Count();
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Voice Channels";
                y.Value = context.Guild.Channels.Where(x => x is SocketVoiceChannel).Count();
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Members";
                y.Value = context.Guild.MemberCount;
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Humans";
                y.Value = context.Guild.Users.Where(x => x.IsBot == false).Count();
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Bots";
                y.Value = context.Guild.Users.Where(x => x.IsBot == true).Count();
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Online";
                y.Value = context.Guild.MemberCount - context.Guild.Users.Where(x => x.Status == UserStatus.Invisible || x.Status == UserStatus.Offline).Count();
                y.IsInline = true;
            });
            embed.AddField(y =>
            {
                y.Name = "Roles";
                y.Value = context.Guild.Roles.Count;
                y.IsInline = true;
            });
            embed.WithFooter(footer);
            embed.ThumbnailUrl = context.Guild.IconUrl;
            embed.WithTimestamp(context.Guild.CreatedAt);
            await context.Channel.SendMessageAsync("", false, embed.Build());
        }

        public async Task PurgeAsync(SocketCommandContext context, string amount, [Remainder] string username)
        {
            if (string.IsNullOrWhiteSpace(amount) || !int.TryParse(amount, out int delete))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}purge";
                embed.Description = $"**Description:** Delete a number of messages from a channel.\n**Usage:** {prefix}purge [amount], [optional user]\n**Example:** {prefix}purge 10, @Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
                return;
            }
            SocketTextChannel channel = context.Channel as SocketTextChannel;
            if (username == null)
            {
                await context.Message.DeleteAsync();
                IAsyncEnumerable<IMessage> items = context.Channel.GetMessagesAsync(delete).Flatten();
                if (delete == 1)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} deleted {amount} message in {channel.Mention} .");
                    await (context.Channel as SocketTextChannel).DeleteMessagesAsync(items.ToEnumerable());
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} deleted {amount} messages in {channel.Mention} .");
                    await (context.Channel as ITextChannel).DeleteMessagesAsync(items.ToEnumerable());
                }
                return;
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                    await context.Message.DeleteAsync();
                    IAsyncEnumerable<IMessage> usermessages = context.Channel.GetMessagesAsync().Flatten().Where(x => x.Author == user).Take(delete);
                    if (delete == 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} deleted {amount} message from {user.Mention} in {channel.Mention} .");
                        await (context.Channel as ITextChannel).DeleteMessagesAsync(usermessages.ToEnumerable());
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} deleted {amount} messages from {user.Mention} in {channel.Mention} .");
                        await (context.Channel as ITextChannel).DeleteMessagesAsync(usermessages.ToEnumerable());
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task KickAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (username == null || reason == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}kick";
                embed.Description = $"**Description:** Kick a member.\n**Usage:** {prefix}kick [user] [reason]\n**Example:** {prefix}kick @Jiggmin Be nice :)";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = Extensions.UserInGuild(context.Message, context.Guild, username) as SocketGuildUser;
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0 || DiscordStaff.Get(context.Guild.Id, "r-" + user.Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0 || user.Id == context.Client.CurrentUser.Id)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if ((user as SocketGuildUser).Roles.OrderBy(x => x.Position).Last().Position >= context.Guild.GetUser(context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Kick | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
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
                        y.Value = context.User.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"{reason} | Mod: {context.User.Username}#{context.User.Discriminator}"
                    };
                    try
                    {
                        await user.SendMessageAsync($"You have been kicked from **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //cant send message
                    }
                    await user.KickAsync(null, options);
                    Ban ban = new Ban()
                    {
                        GuildID = long.Parse(context.Guild.Id.ToString()),
                        Case = Ban.CaseCount(context.Guild.Id) + 1,
                        UserID = long.Parse(user.Id.ToString()),
                        Username = user.Username + "#" + user.Discriminator,
                        Type = "Kick",
                        ModeratorID = long.Parse(context.User.Id.ToString()),
                        Moderator = context.User.Username + "#" + context.User.Discriminator,
                        Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                    };
                    Ban.Add(ban);
                    await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was kicked.");
                    await banlog.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task BanAsync(SocketCommandContext context, string username, [Remainder] string reason)
        {
            if (username == null || string.IsNullOrWhiteSpace(reason))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}ban";
                embed.Description = $"**Description:** Ban a member.\n**Usage:** {prefix}ban [user] [reason]\n**Example:** {prefix}ban @Jiggmin botting";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = Extensions.UserInGuild(context.Message, context.Guild, username) as SocketGuildUser;
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0 || DiscordStaff.Get(context.Guild.Id, "r-" + user.Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0 || user.Id == context.Client.CurrentUser.Id)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if (user.Roles.OrderBy(x => x.Position).Last().Position >= context.Guild.GetUser(context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Ban | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
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
                        y.Value = context.User.Mention;
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "Reason";
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"{reason} | Mod: {context.User.Username}#{context.User.Discriminator}"
                    };
                    try
                    {
                        await user.SendMessageAsync($"You have been banned from **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //cant send message
                    }
                    await context.Guild.AddBanAsync(user, 1, null, options);
                    Ban ban = new Ban()
                    {
                        GuildID = long.Parse(context.Guild.Id.ToString()),
                        Case = Ban.CaseCount(context.Guild.Id) + 1,
                        UserID = long.Parse(user.Id.ToString()),
                        Username = user.Username + "#" + user.Discriminator,
                        Type = "Ban",
                        ModeratorID = long.Parse(context.User.Id.ToString()),
                        Moderator = context.User.Username + "#" + context.User.Discriminator,
                        Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                    };
                    Ban.Add(ban);
                    await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was banned.");
                    await banlog.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }

        public async Task MuteAsync(SocketCommandContext context, string username, string time, [Remainder] string reason)
        {
            if (username == null || string.IsNullOrWhiteSpace(time) || string.IsNullOrWhiteSpace(reason) || !double.TryParse(time, out double minutes))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command {prefix}mute";
                embed.Description = $"**Description:** Mute a member.\n**Usage:** {prefix}mute [user] [time] [reason]\n**Example:** {prefix}mute @Jiggmin 10 Flooding";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.GetBanLogChannel(context.Guild) == null)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel has not been set.");
            }
            else if (minutes < 1)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} you must mute a user for at least 1 minute.");
            }
            else if (reason.Length > 200)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} reason must be 200 characters or less.");
            }
            else
            {
                if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0 || (user.Roles.Count - 1 > 0 && DiscordStaff.Get(context.Guild.Id, "r-" + user.Roles.Where(x => x.IsEveryone == false).OrderBy(x => x.Position).First().Id).Count > 0) || user.Id == context.Client.CurrentUser.Id)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is a mod/admin, I can't do that.");
                        return;
                    }
                    if ((user as SocketGuildUser).Roles.OrderBy(x => x.Position).Last().Position >= context.Guild.GetUser(context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that user is of higher role than me.");
                        return;
                    }
                    minutes = Math.Round(Convert.ToDouble(time), 0);
                    EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                    {
                        Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Mute | {user.Username}#{user.Discriminator}",
                        IconUrl = user.GetAvatarUrl(),
                    };
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        Text = $"ID: {user.Id}"
                    };
                    SocketTextChannel banlog = Extensions.GetBanLogChannel(context.Guild);
                    IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Muted".ToUpper());
                    if (role.Count() <= 0)
                    {
                        await context.Guild.CreateRoleAsync("Muted", GuildPermissions.None.Modify(sendMessages: false, addReactions: false));
                        role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Muted".ToUpper());
                    }
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Muting User | Reason: {reason} | Mod: {context.User.Username}#{context.User.Discriminator}"
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
                        y.Value = context.User.Mention;
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
                        y.Value = Format.Sanitize(reason);
                        y.IsInline = true;
                    });
                    await context.Message.DeleteAsync();
                    await context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was muted.");
                    await banlog.SendMessageAsync("", false, embed.Build());
                    Ban ban = new Ban()
                    {
                        GuildID = long.Parse(context.Guild.Id.ToString()),
                        Case = Ban.CaseCount(context.Guild.Id) + 1,
                        UserID = long.Parse(user.Id.ToString()),
                        Username = user.Username + "#" + user.Discriminator,
                        Type = "Mute",
                        ModeratorID = long.Parse(context.User.Id.ToString()),
                        Moderator = context.User.Username + "#" + context.User.Discriminator,
                        Reason = reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                    };
                    Ban.Add(ban);
                    try
                    {
                        if (minutes == 1)
                        {
                            await user.SendMessageAsync($"You have been muted in **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} for **{Format.Sanitize(reason)}** and for a length of **{minutes}** minute.");
                        }
                        else
                        {
                            await user.SendMessageAsync($"You have been muted in **{Format.Sanitize(context.Guild.Name)}** by {context.User.Mention} for **{Format.Sanitize(reason)}** and for a length of **{minutes}** minutes.");
                        }
                    }
                    catch (Discord.Net.HttpException)
                    {
                        //cant send message
                    }
                    MutedUser.Add(context.Guild.Id, user.Id);
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
                                Name = $"Case {Ban.CaseCount(context.Guild.Id) + 1} | Unmute | {user.Username}#{user.Discriminator}",
                                IconUrl = user.GetAvatarUrl(),
                            };
                            EmbedBuilder embed2 = new EmbedBuilder()
                            {
                                Color = new Color(0, 255, 0),
                                Author = auth2
                            };
                            EmbedFooterBuilder footer2 = new EmbedFooterBuilder()
                            {
                                Text = $"ID: {user.Id}"
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
                                y.Value = context.Client.CurrentUser.Mention;
                                y.IsInline = true;
                            });
                            embed2.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = "Auto";
                                y.IsInline = true;
                            });
                            await banlog.SendMessageAsync("", false, embed2.Build());
                            try
                            {
                                await user.SendMessageAsync($"You are now unmuted in **{Format.Sanitize(context.Guild.Name)}**.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //cant send message
                            }
                            ban.Case = Ban.CaseCount(context.Guild.Id) + 1;
                            ban.ModeratorID = long.Parse(context.Client.CurrentUser.Id.ToString());
                            ban.Moderator = context.Client.CurrentUser.Username + "#" + context.Client.CurrentUser.Discriminator;
                            ban.Type = "Unmute";
                            ban.Reason = "Auto - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString();
                            Ban.Add(ban);
                            MutedUser.Remove(context.Guild.Id, user.Id);
                        }
                        else
                        {
                            MutedUser.Remove(context.Guild.Id, user.Id);
                        }
                    });
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
                }
            }
        }
    }
}
