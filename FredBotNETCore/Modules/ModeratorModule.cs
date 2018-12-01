using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules
{
    [Name("Moderator")]
    [Summary("Module for commands of moderators and up of Platform Racing Group")]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        [Command("updatetoken", RunMode = RunMode.Async)]
        [Alias("utoken", "changetoken")]
        [Summary("Updates token used in some commands")]
        [RequireContext(ContextType.Guild)]
        public async Task UpdateToken(string newToken)
        {
            if (Context.Guild.Id == 356602194037964801)
            {
                string currentToken = File.ReadAllText(Path.Combine(Extensions.downloadPath, "PR2Token.txt"));

                SocketTextChannel log = Context.Client.GetGuild(249657315576381450).GetTextChannel(327575359765610496);
                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                {
                    Name = "Token Changed",
                    IconUrl = Context.Client.GetGuild(249657315576381450).IconUrl
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    Text = $"ID: {Context.User.Id}",
                    IconUrl = Context.User.GetAvatarUrl()
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Author = author,
                    Color = new Color(0, 0, 255),
                    Footer = footer
                };
                embed.WithCurrentTimestamp();
                embed.Description = $"{Context.User.Mention} changed the token of FredTheG.CactusBot on PR2.";
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the token was successfully changed from **{Format.Sanitize(currentToken)}** to **{Format.Sanitize(newToken)}**.");
                await log.SendMessageAsync("", false, embed.Build());
                File.WriteAllText(Path.Combine(Extensions.downloadPath, "PR2Token.txt"), newToken);
            }
        }

        [Command("notifymacroers", RunMode = RunMode.Async)]
        [Alias("pingmacroers")]
        [Summary("Mention macroers role with message specified.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task NotifyMacroers()
        {
            var channel = Context.Guild.GetTextChannel(249678944956055562);
            var role = Context.Guild.GetRole(497409284615962834);
            RequestOptions options = new RequestOptions()
            {
                AuditLogReason = $"Notified by {Context.User.Username}#{Context.User.Discriminator}."
            };
            Extensions.Purging = true;
            await Context.Message.DeleteAsync();
            await role.ModifyAsync(x => x.Mentionable = true, options);
            await channel.SendMessageAsync($"Servers have just been restarted. Check your macros!! {role.Mention}");
            Extensions.Purging = false;
            await role.ModifyAsync(x => x.Mentionable = false, options);
        }

        [Command("addrole", RunMode = RunMode.Async)]
        [Alias("+role", "createrole")]
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
                            await Context.Channel.SendMessageAsync($"Created role **{Format.Sanitize(settingsSplit[0])}** with color **#{settingsSplit2[0]}**, and is displayed separately.");
                        }
                        catch (FormatException)
                        {
                            await Context.Guild.CreateRoleAsync(settings, null, null, false, options);
                            await Context.Channel.SendMessageAsync($"Created role **{Format.Sanitize(settings)}**.");
                        }
                    }
                    else
                    {
                        try
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(settingsSplit[1].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                            await Context.Guild.CreateRoleAsync(settingsSplit[0], null, new Color(color.R, color.G, color.B), false, options);
                            await Context.Channel.SendMessageAsync($"Created role **{Format.Sanitize(settingsSplit[0])}** with color **#{settingsSplit[1]}**.");
                        }
                        catch (FormatException)
                        {
                            await Context.Guild.CreateRoleAsync(settings, null, null, false, options);
                            await Context.Channel.SendMessageAsync($"Created role **{Format.Sanitize(settings)}**.");
                        }
                    }
                }
                else
                {
                    await Context.Guild.CreateRoleAsync(settings);
                    await Context.Channel.SendMessageAsync($"Created role **{Format.Sanitize(settings)}**.");
                }
            }
        }

        [Command("delrole", RunMode = RunMode.Async)]
        [Alias("-role", "deleterole")]
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
                try
                {
                    if (Extensions.RoleInGuild(Context.Message, Context.Guild, roleName) != null)
                    {
                        SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, roleName);
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"Deleted by: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await role.DeleteAsync(options);
                        await Context.Channel.SendMessageAsync($"Deleted role **{Format.Sanitize(roleName)}**");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role **{Format.Sanitize(roleName)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: **{roleName}**.");
                }
            }
        }

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
                    try
                    {
                        if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                        {
                            SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                            string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"));
                            if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers);
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already blacklisted from using music commands.");
                            }
                            else
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers + user.Id.ToString() + "\n");
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Music Blacklist Add",
                                    IconUrl = Context.Guild.IconUrl
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    Text = $"ID: {Context.User.Id}",
                                    IconUrl = Context.User.GetAvatarUrl()
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Author = author,
                                    Color = new Color(255, 0, 0),
                                    Footer = footer
                                };
                                embed.WithCurrentTimestamp();
                                embed.Description = $"{Context.User.Mention} blacklisted {user.Mention} from using music commands.";
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully blacklisted **{Format.Sanitize(user.Username)}#{user.Discriminator}** from using music commands.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                    try
                    {
                        if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                        {
                            SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                            string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"));
                            if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                            {
                                currentBlacklistedUsers = currentBlacklistedUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"), currentBlacklistedUsers);
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Music Blacklist Remove",
                                    IconUrl = Context.Guild.IconUrl
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    Text = $"ID: {Context.User.Id}",
                                    IconUrl = Context.User.GetAvatarUrl()
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Author = author,
                                    Color = new Color(0, 255, 0),
                                    Footer = footer
                                };
                                embed.WithCurrentTimestamp();
                                embed.Description = $"{Context.User.Mention} unblacklisted {user.Mention} from using music commands.";
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully removed blacklisted music command user **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user **{Format.Sanitize(user.Username)}** is not blacklisted from using music commands.");
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                var blacklistedMusic = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = "List Blacklisted Music"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
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
                        y.Value = Format.Sanitize(blacklistedUsers);
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
        [Alias("blacklistsuggestion", "suggestionblacklist", "suggestionsblacklist")]
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
                    try
                    {
                        if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                        {
                            SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                            string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"));
                            if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers);
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already blacklisted from suggestions.");
                            }
                            else
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers + user.Id.ToString() + "\n");
                                SocketTextChannel suggestions = Context.Guild.GetTextChannel(249684395454234624);
                                RequestOptions options = new RequestOptions()
                                {
                                    AuditLogReason = $"Blacklisting User | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                                };
                                await suggestions.AddPermissionOverwriteAsync(user, OverwritePermissions.InheritAll.Modify(PermValue.Inherit, PermValue.Inherit, PermValue.Inherit, PermValue.Deny), options);
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Suggestions Blacklist Add",
                                    IconUrl = Context.Guild.IconUrl
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    Text = $"ID: {Context.User.Id}",
                                    IconUrl = Context.User.GetAvatarUrl()
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Author = author,
                                    Color = new Color(255, 0, 0),
                                    Footer = footer
                                };
                                embed.WithCurrentTimestamp();
                                embed.Description = $"{Context.User.Mention} blacklisted {user.Mention} from the suggestions channel.";
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully blacklisted **{Format.Sanitize(user.Username)}#{user.Discriminator}** from suggestions.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                    try
                    {
                        if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                        {
                            IUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                            string currentBlacklistedUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"));
                            if (currentBlacklistedUsers.Contains(user.Id.ToString()))
                            {
                                currentBlacklistedUsers = currentBlacklistedUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"), currentBlacklistedUsers);
                                SocketTextChannel suggestions = Context.Guild.GetTextChannel(249684395454234624);
                                RequestOptions options = new RequestOptions()
                                {
                                    AuditLogReason = $"Unblacklisting User | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                                };
                                await suggestions.RemovePermissionOverwriteAsync(user, options);
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Suggestions Blacklist Remove",
                                    IconUrl = Context.Guild.IconUrl
                                };
                                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                {
                                    Text = $"ID: {Context.User.Id}",
                                    IconUrl = Context.User.GetAvatarUrl()
                                };
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Author = author,
                                    Color = new Color(0, 255, 0),
                                    Footer = footer
                                };
                                embed.WithCurrentTimestamp();
                                embed.Description = $"{Context.User.Mention} unblacklisted {user.Mention} from the suggestions channel.";
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully removed blacklisted suggestions user **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user **{Format.Sanitize(user.Username)}** is not blacklisted from suggestions.");
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                        }
                    }
                    catch (NullReferenceException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                var blacklistedsuggestions = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedSuggestions.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.Guild.IconUrl,
                    Name = "List Blacklisted Suggestions"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
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
                        y.Value = Format.Sanitize(blacklistedUsers);
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
                try
                {
                    if (Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName) != null)
                    {
                        SocketGuildChannel channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName);
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
                            foreach (SocketGuildChannel gChannel in Context.Guild.Channels)
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
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find channel **{Format.Sanitize(channelName)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the channel with ID: **{channelName}** does not exist or could not be found.");
                }
            }
        }

        [Command("membercount", RunMode = RunMode.Async)]
        [Alias("mcount", "usercount", "ucount")]
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
                try
                {
                    if (Extensions.RoleInGuild(Context.Message, Context.Guild, roleName) != null)
                    {
                        SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, roleName);
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
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role **{Format.Sanitize(roleName)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: **{roleName}**.");
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
                Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
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
                y.Value = Format.Sanitize(roleNames);
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
            List<string> warnings = null;
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
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
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                        return;
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                embed.Description = embed.Description + Format.Sanitize(warn) + "\n";
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
                foreach (IBan ban in bans)
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
                    await Context.Message.DeleteAsync();
                    if (reason == null)
                    {
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unban", Context.User.Username + "#" + Context.User.Discriminator, "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was unbanned.");
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
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unban", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was unbanned.");
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await Context.Guild.RemoveBanAsync(user, options);
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} **{Format.Sanitize(username)}** is not banned.");
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                        if (Extensions.CheckStaff(user.Id.ToString(), (user as SocketGuildUser).Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if ((user as SocketGuildUser).Roles.OrderBy(x => x.Position).Last().Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                            return;
                        }
                        SocketTextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
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
                        await Context.Message.DeleteAsync();
                        if (reason == null)
                        {
                            Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Undeafen", Context.User.Username + "#" + Context.User.Discriminator, "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                            await banlog.SendMessageAsync("", false, embed.Build());
                            await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was undeafened.");
                            await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = false);
                            try
                            {
                                await user.SendMessageAsync($"You have been undeafened on **{Format.Sanitize(Context.Guild.Name)}** by **{Context.User.Mention}**.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //cant send message
                            }
                        }
                        else
                        {
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = Format.Sanitize(reason);
                                y.IsInline = true;
                            });
                            Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Undeafen", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                            await banlog.SendMessageAsync("", false, embed.Build());
                            await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was undeafened.");
                            await (user as SocketGuildUser).ModifyAsync(x => x.Deaf = false);
                            try
                            {
                                await user.SendMessageAsync($"You have been undeafened on **{Format.Sanitize(Context.Guild.Name)}** by **{Context.User.Mention}** with reason **{Format.Sanitize(reason)}**.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //cant send message
                            }
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        if (Extensions.CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if (user.Roles.OrderBy(x => x.Position).Last().Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
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
                        await Context.Message.DeleteAsync();
                        if (reason == null)
                        {
                            Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Deafen", Context.User.Username + "#" + Context.User.Discriminator, "No reason given - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                            await banlog.SendMessageAsync("", false, embed.Build());
                            await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was deafened.");
                            await user.ModifyAsync(x => x.Deaf = true);
                            try
                            {
                                await user.SendMessageAsync($"You have been deafened on **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention}.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //cant send message
                            }
                        }
                        else
                        {
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = Format.Sanitize(reason);
                                y.IsInline = true;
                            });
                            Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Deafen", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                            await banlog.SendMessageAsync("", false, embed.Build());
                            await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was deafened.");
                            await user.ModifyAsync(x => x.Deaf = true);
                            try
                            {
                                await user.SendMessageAsync($"You have been deafened on **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //cant send message
                            }
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        if (Extensions.CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if (user.Roles.OrderBy(x => x.Position).Last().Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
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
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        await Context.Message.DeleteAsync();
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Softban", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was softbanned.");
                        try
                        {
                            await user.SendMessageAsync($"You have been softbanned on **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        await Context.Guild.AddBanAsync(user, 7, $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}");
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"Softban | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await Context.Guild.RemoveBanAsync(user, options);
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
                }
            }
        }


        [Command("getcase", RunMode = RunMode.Async)]
        [Alias("getprior", "case")]
        [Summary("Get info on a case")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task GetCase([Remainder] string caseN = null)
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

        [Command("modlogs", RunMode = RunMode.Async)]
        [Alias("priors")]
        [Summary("Get a list of mod logs for a user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Modlogs([Remainder] string username)
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
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
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
                }
            }
        }

        [Command("reason", RunMode = RunMode.Async)]
        [Alias("edit", "editcase")]
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
                Database.UpdateReason(caseN, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                await Context.Message.DeleteAsync();
                await Context.Channel.SendMessageAsync($"Updated case {caseN}.");
                embed.Author.Name = "Updated Case";
                embed.Author.IconUrl = Context.Guild.IconUrl;
                embed.Footer.Text = "ID: " + Context.User.Id;
                embed.Footer.IconUrl = Context.User.GetAvatarUrl();
                embed.Color = new Color(0, 0, 255);
                embed.Fields.Clear();
                embed.Description = $"{Context.User.Mention} updated case **{caseN}**.";
                await Context.Guild.GetTextChannel(327575359765610496).SendMessageAsync("", false, embed.Build());
            }
        }

        [Command("warn", RunMode = RunMode.Async)]
        [Alias("w")]
        [Summary("Warn a member")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Warn(string username = null, [Remainder] string reason = null)
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        if (Extensions.CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if (user.Roles.OrderBy(x => x.Position).Last().Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is of higher role than me.");
                            return;
                        }
                        SocketTextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
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
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        await Context.Message.DeleteAsync();
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Warn", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was warned.");
                        try
                        {
                            await user.SendMessageAsync($"You have been warned on **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
                }
            }
        }

        [Command("endgiveaway", RunMode = RunMode.Async)]
        [Alias("endg", "gend")]
        [Summary("Ends the giveaway")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task EndGiveaway()
        {
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
            IUserMessage message = null;
            IEmbed msgEmbed = null;
            EmbedBuilder embed = new EmbedBuilder();
            string item = null;
            int winners = 0;
            foreach (var msg in messages)
            {
                if (msg.Content.Equals(":confetti_ball: **Giveaway** :confetti_ball:") && msg.Author.Id == Context.Guild.CurrentUser.Id)
                {
                    message = msg as IUserMessage;
                    var msgEmbeds = msg.Embeds;
                    msgEmbed = msgEmbeds.ElementAt(0);
                    embed.Title = msgEmbed.Title;
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = msgEmbed.Footer.Value.IconUrl
                    };
                    winners = int.Parse(Extensions.GetBetween(msgEmbed.Footer.Value.Text, "Winners: ", " | Ends at"));
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();
                    break;
                }
            }
            if (message != null)
            {
                embed.Footer.Text = $"Winners: {winners} | Ended at";
                await message.ModifyAsync(x => x.Content = $":confetti_ball: **Giveaway Ended** :confetti_ball:");

                var users = await message.GetReactionUsersAsync(Emote.Parse("<:artifact:260898610734956574>"), 9999).FlattenAsync();
                if (users.Count() <= 1)
                {
                    await Context.Channel.SendMessageAsync("Nobody entered the giveaway.");
                    embed.Description = "Nobody entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (users.Count() <= winners)
                {
                    await Context.Channel.SendMessageAsync("Not enough users entered the giveaway.");
                    embed.Description = "Not enough users entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (winners == 1)
                {
                    IUser randomUser = users.GetRandomElement();
                    while (randomUser.Id == Context.Guild.CurrentUser.Id)
                    {
                        randomUser = users.GetRandomElement();
                    }
                    embed.Description = $"Winner: {randomUser.Mention}";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await Context.Channel.SendMessageAsync($"The winner of the {item} is {randomUser.Mention} !");
                }
                else
                {
                    List<IUser> userWinners = new List<IUser>();
                    for (int i = 0; i < winners; i++)
                    {
                        IUser randomUser = users.GetRandomElement();
                        while (randomUser.Id == Context.Guild.CurrentUser.Id || userWinners.Contains(randomUser))
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
                    await Context.Channel.SendMessageAsync($"The winners of the {item} are {description.Substring(0, description.Length - 2)} !");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find a giveaway in this channel.");
            }
        }

        [Command("repick", RunMode = RunMode.Async)]
        [Alias("reroll", "redo")]
        [Summary("Repicks giveaway winner")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Repick()
        {
            var messages = await Context.Channel.GetMessagesAsync(100).FlattenAsync();
            IUserMessage message = null;
            IEmbed msgEmbed = null;
            EmbedBuilder embed = new EmbedBuilder();
            int winners = 0;
            foreach (var msg in messages)
            {
                if (msg.Content.Equals(":confetti_ball: **Giveaway Ended** :confetti_ball:") && msg.Author.Id == Context.Guild.CurrentUser.Id)
                {
                    message = msg as IUserMessage;
                    msgEmbed = msg.Embeds.ElementAt(0);
                    break;
                }
            }
            if (message != null)
            {
                var users = await message.GetReactionUsersAsync(Emote.Parse("<:artifact:260898610734956574>"), 9999).FlattenAsync();
                embed = msgEmbed as EmbedBuilder;
                winners = int.Parse(Extensions.GetBetween(embed.Footer.Text, "Winners: ", " | Ends at"));
                if (users.Count() <= 1)
                {
                    await Context.Channel.SendMessageAsync("Nobody entered the giveaway.");
                    embed.Description = $"Nobody entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (users.Count() == 2 && winners == 1)
                {
                    await Context.Channel.SendMessageAsync("Nobody else can win the giveaway.");
                    embed.Description = $"Nobody else can win the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (users.Count() <= winners)
                {
                    await Context.Channel.SendMessageAsync("Not enough users entered the giveaway.");
                    embed.Description = "Not enough users entered the giveaway.";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                }
                else if (winners == 1)
                {
                    IUser randomUser = users.GetRandomElement();
                    string oldWinner = embed.Description.Substring(8, embed.Description.Length);
                    while (randomUser.Id == Context.Guild.CurrentUser.Id || randomUser.Mention.ToString().Equals(oldWinner))
                    {
                        randomUser = users.GetRandomElement();
                    }
                    embed.Description = $"Winner: {randomUser.Mention}";
                    await message.ModifyAsync(x => x.Embed = embed.Build());
                    await Context.Channel.SendMessageAsync($"The new winner of the {embed.Title} is {randomUser.Mention} !");
                }
                else
                {
                    List<IUser> userWinners = new List<IUser>();
                    for (int i = 0; i < winners; i++)
                    {
                        IUser randomUser = users.GetRandomElement();
                        while (randomUser.Id == Context.Guild.CurrentUser.Id || userWinners.Contains(randomUser))
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
                    await Context.Channel.SendMessageAsync($"The new winners of the {embed.Title} are {description.Substring(0, description.Length - 2)} !");
                }
            }
            else
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
        public async Task Giveaway(string channel = null, string time = null, string winnersS = null, [Remainder] string item = null)
        {
            if (channel == null || time == null || !double.TryParse(time, out double num2) || Math.Round(Convert.ToDouble(time), 0) < 0 || !int.TryParse(winnersS, out int winners) || winners < 1 || item == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /giveaway";
                embed.Description = "**Description:** Create a giveaway.\n**Usage:** /giveaway [channel] [time] [winners] [item]\n**Example:** /giveaway pr2-discussion 60 1 Cowboy Hat";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                SocketTextChannel giveawayChannel = null;
                try
                {
                    if (Extensions.ChannelInGuild(Context.Message, Context.Guild, channel) != null)
                    {
                        if (Extensions.ChannelInGuild(Context.Message, Context.Guild, channel) is SocketTextChannel)
                        {
                            giveawayChannel = Extensions.ChannelInGuild(Context.Message, Context.Guild, channel) as SocketTextChannel;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the channel `{channel}` is not a text channel so a giveaway cannot happen there.");
                            return;
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find text channel `{channel}`.");
                        return;
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find a text channel with ID: `{channel}`.");
                    return;
                }
                double minutes = Math.Round(Convert.ToDouble(time), 0);
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                };
                EmbedFooterBuilder footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl(),
                    Text = ($"Winners: {winners} | Ends at")
                };
                embed.WithFooter(footer);
                embed.Title = $"{item}";
                embed.WithTimestamp((DateTime.Now).AddMinutes(minutes));
                embed.Description = $"React with <:artifact:260898610734956574> to enter the giveaway.\nTime left: {minutes} minutes.";
                IUserMessage message = null;
                try
                {
                    message = await giveawayChannel.SendMessageAsync(":confetti_ball: **Giveaway** :confetti_ball:", false, embed.Build());
                }
                catch (Discord.Net.HttpException)
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
                        while (randomUser.Id == 383927022583545859 || userWinners.Contains(randomUser))
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        if (type.Equals("temp", StringComparison.InvariantCultureIgnoreCase) || type.Equals("temporary", StringComparison.InvariantCultureIgnoreCase))
                        {
                            await Context.Message.DeleteAsync();
                            await Context.Channel.SendMessageAsync($"*{Context.User.Mention} has promoted {user.Mention} to a temporary moderator! " +
                            $"May they reign in hours of peace and prosperity! " +
                            $"Make sure you read the moderator guidelines at https://jiggmin2.com/forums/showthread.php?tid=12*");
                        }
                        else if (type.Equals("trial", StringComparison.InvariantCultureIgnoreCase))
                        {
                            await Context.Message.DeleteAsync();
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
                            await Context.Message.DeleteAsync();
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
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the promotion type **{Format.Sanitize(type)}** was not recognised.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
            try
            {
                if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                {
                    SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
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
                    await Context.Message.DeleteAsync();
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} has removed temp mod from {user.Mention}");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                }
            }
            catch (NullReferenceException)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
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
                            try
                            {
                                await user.SendMessageAsync($"{Context.User.Mention} you have been unmuted in **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention}.");
                            }
                            catch(Discord.Net.HttpException)
                            {
                                //can't send message
                            }
                        }
                        else
                        {
                            options.AuditLogReason = $"Unmuting User | Mod: {Context.User.Username}#{Context.User.Discriminator} | Reason: {reason}";
                            embed.AddField(y =>
                            {
                                y.Name = "Reason";
                                y.Value = Format.Sanitize(reason);
                                y.IsInline = true;
                            });
                            Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unmute", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                            try
                            {
                                await user.SendMessageAsync($"{Context.User.Mention} you have been unmuted in **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                            }
                            catch (Discord.Net.HttpException)
                            {
                                //can't send message
                            }
                        }
                        await Context.Message.DeleteAsync();
                        ITextChannel banlog = Context.Guild.GetTextChannel(263474494327226388);
                        await banlog.SendMessageAsync("", false, embed.Build());
                        await user.RemoveRolesAsync(role, options);
                        await Context.Channel.SendMessageAsync($"Unmuted **{Format.Sanitize(user.Username)}#{user.Discriminator}**");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
            await Context.Channel.SendMessageAsync($"**{client.Latency.ToString()} ms**");
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
                    y.Value = Format.Sanitize(application.Owner.Username); application.Owner.Id.ToString();  /*Pulls the owner's name*/
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
                    y.Value = Context.Client.Guilds.Count.ToString();  /*Numbers of servers the bot is in*/
                    y.IsInline = false;
                })

                .AddField(y =>
                {
                    y.Name = "Heap Size";
                    y.Value = Extensions.GetHeapSize();   /*pulls ram usage of modules/heaps*/
                    y.IsInline = true;
                })

                .AddField(y =>
                {
                    y.Name = "Number Of Users";
                    y.Value = Context.Client.Guilds.Sum(g => g.Users.Count).ToString();  /*Counts users*/
                    y.IsInline = true;
                })

                .AddField(y =>
                {
                    y.Name = "Channels";
                    y.Value = Context.Client.Guilds.Sum(g => g.Channels.Count).ToString();  /*Gets Number of channels*/
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
        [Alias("uinfo", "whois")]
        [Summary("Returns information about the mentioned user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task UserInfo([Remainder] string username = null)
        {
            if (username == null)
            {
                username = Context.User.Username;
            }
            try
            {
                if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                {
                    SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
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
                        Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                        Author = auth
                    };
                    EmbedFooterBuilder footer = new EmbedFooterBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = ($"ID: {user.Id}")
                    };
                    embed.Description = $"{user.Mention}";
                    string roleList = "";
                    var guildusers = Context.Guild.Users.OrderBy(x => x.JoinedAt);
                    int position = 0;
                    string joinedMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(user.JoinedAt.Value.Month);
                    string joinedDay = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(user.JoinedAt.Value.DayOfWeek);
                    string joined = $"{joinedDay}, {joinedMonth} {user.JoinedAt.Value.Day}, {user.JoinedAt.Value.Year} {user.JoinedAt.Value.LocalDateTime.ToString("h:mm tt")}";
                    string pr2name = Database.GetPR2Name(user);
                    if (pr2name == null || pr2name.Length <= 0)
                    {
                        pr2name = "N/A";
                    }
                    foreach (SocketGuildUser member in guildusers)
                    {
                        if (member.Id == user.Id)
                        {
                            position = guildusers.ToList().IndexOf(member) + 1;
                        }
                    }
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
                    embed.AddField(y =>
                    {
                        y.Name = "PR2 Name";
                        y.Value = Format.Sanitize(pr2name);
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = "JV2 Name";
                        y.Value = "N/A";
                        y.IsInline = true;
                    });
                    embed.AddField(y =>
                    {
                        y.Name = $"Roles [{user.Roles.Count - 1}]";
                        y.Value = roleList;
                        y.IsInline = false;
                    });
                    if (Context.Guild.Id == 249657315576381450)
                    {
                        embed.AddField(y =>
                        {
                            y.Name = $"Priors";
                            y.Value = Database.Modlogs(user).Count;
                            y.IsInline = false;
                        });
                    }
                    embed.ThumbnailUrl = user.GetAvatarUrl();
                    embed.WithFooter(footer);
                    embed.WithCurrentTimestamp();

                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                }
            }
            catch (NullReferenceException)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
            }
        }

        [Command("guildinfo", RunMode = RunMode.Async)]
        [Alias("ginfo", "serverinfo")]
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
                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
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
                y.Value = Format.Sanitize(guildName);
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
            if (string.IsNullOrWhiteSpace(amount) || !int.TryParse(amount, out int delete))
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
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Purge Messages",
                IconUrl = Context.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {Context.User.Id}"
            };
            EmbedBuilder embed2 = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(255, 0, 0),
                Footer = footer
            };
            embed2.WithCurrentTimestamp();
            if (username == null)
            {
                await Context.Message.DeleteAsync();
                var items = Context.Channel.GetMessagesAsync(delete).Flatten();
                if (delete == 1)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} message in {channel.Mention} .");
                    Extensions.Purging = true;
                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(items.ToEnumerable());
                    embed2.Description = $"{Context.User.Mention} purged **{amount}** message in {channel.Mention}.";
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} messages in {channel.Mention} .");
                    Extensions.Purging = true;
                    await (Context.Channel as ITextChannel).DeleteMessagesAsync(items.ToEnumerable());
                    embed2.Description = $"{Context.User.Mention} purged **{amount}** messages in {channel.Mention}.";
                }
                await log.SendMessageAsync("", false, embed2.Build());
                Extensions.Purging = false;
                return;
            }
            else
            {
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        await Context.Message.DeleteAsync();
                        var usermessages = (Context.Channel.GetMessagesAsync().Flatten()).Where(x => x.Author == user).Take(delete);
                        if (delete == 1)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} message from {user.Mention} in {channel.Mention} .");
                            Extensions.Purging = true;
                            await (Context.Channel as ITextChannel).DeleteMessagesAsync(usermessages.ToEnumerable());
                            embed2.Description = $"{Context.User.Mention} purged **{amount}** message in {channel.Mention} from {user.Mention}.";
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} deleted {amount} messages from {user.Mention} in {channel.Mention} .");
                            Extensions.Purging = true;
                            await (Context.Channel as ITextChannel).DeleteMessagesAsync(usermessages.ToEnumerable());
                            embed2.Description = $"{Context.User.Mention} purged **{amount}** messages in {channel.Mention} from {user.Mention}.";
                        }
                        await log.SendMessageAsync("", false, embed2.Build());
                        Extensions.Purging = false;
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        if (Extensions.CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if ((user as SocketGuildUser).Roles.OrderBy(x => x.Position).Last().Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
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
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        await Context.Message.DeleteAsync();
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        try
                        {
                            await user.SendMessageAsync($"You have been kicked from **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        await user.KickAsync(null, options);
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Kick", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was kicked.");
                        await banlog.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
                }
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        if (Extensions.CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if (user.Roles.OrderBy(x => x.Position).Last().Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
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
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        await Context.Message.DeleteAsync();
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"{reason} | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        try
                        {
                            await user.SendMessageAsync($"You have been banned from **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} with reason **{Format.Sanitize(reason)}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        await Context.Guild.AddBanAsync(user, 1, null, options);
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Ban", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was banned.");
                        await banlog.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
                }
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
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                        if (Extensions.CheckStaff(user.Id.ToString(), user.Roles.ElementAt(1).Id.ToString()) || user.Id == 383927022583545859)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} that user is a mod/admin, I can't do that.");
                            return;
                        }
                        if ((user as SocketGuildUser).Roles.OrderBy(x => x.Position).Last().Position >= Context.Guild.GetUser(Context.Client.CurrentUser.Id).Roles.OrderBy(x => x.Position).Last().Position)
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
                            y.Value = Format.Sanitize(reason);
                            y.IsInline = true;
                        });
                        await Context.Message.DeleteAsync();
                        await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** was muted.");
                        await banlog.SendMessageAsync("", false, embed.Build());
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Mute", Context.User.Username + "#" + Context.User.Discriminator, reason + " - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        try
                        {
                            if (minutes == 1)
                            {
                                await user.SendMessageAsync($"You have been muted in **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} for **{Format.Sanitize(reason)}** and for a length of **{minutes}** minute.");
                            }
                            else
                            {
                                await user.SendMessageAsync($"You have been muted in **{Format.Sanitize(Context.Guild.Name)}** by {Context.User.Mention} for **{Format.Sanitize(reason)}** and for a length of **{minutes}** minutes.");
                            }
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
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
                                    y.Value = Context.Client.CurrentUser.Mention;
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
                                    await user.SendMessageAsync($"You are now unmuted in **{Format.Sanitize(Context.Guild.Name)}**.");
                                }
                                catch (Discord.Net.HttpException)
                                {
                                    //cant send message
                                }
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
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
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
            try
            {
                if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                {
                    SocketGuildUser user = Context.Guild.GetUser(Extensions.UserInGuild(Context.Message, Context.Guild, username).Id);
                    if (user.Id == Context.User.Id)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you cannot temp mod yourself.");
                        return;
                    }
                    double minutes = Math.Round(Convert.ToDouble(time), 0);
                    ITextChannel roles = user.Guild.GetChannel(260272249976782848) as ITextChannel;

                    IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Temp Mod".ToUpper());
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Temp Modding User | Mod: {Context.User.Username}#{Context.User.Discriminator}"
                    };
                    await user.AddRolesAsync(role, options);
                    await Context.Message.DeleteAsync();
                    if (time.Equals("1"))
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for **{time}** minute. " +
                                    $"May they reign in a minute of peace and prosperity! Read more about mods and what they do in {roles.Mention}.");
                    }
                    else if (minutes <= 60)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for **{time}** minutes. " +
                                    $"May they reign in minutes of peace and prosperity! Read more about mods and what they do in {roles.Mention}.");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for **{time}** minutes. " +
                                    $"May they reign in hours of peace and prosperity! Read more about mods and what they do in {roles.Mention}.");
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
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                }
            }
            catch (NullReferenceException)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
            }
        }
    }
}
