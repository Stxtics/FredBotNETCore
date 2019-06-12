using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FredBotNETCore.Services
{
    public class AdminService
    {
        public AdminService()
        {

        }

        public async Task TempAsync(SocketCommandContext context, string username, string time)
        {
            if (context.Guild.Id != 528679522707701760)
            {
                return;
            }
            if (username == null || string.IsNullOrWhiteSpace(time) || !double.TryParse(time, out double num) || Math.Round(Convert.ToDouble(time), 0) < 1)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}temp";
                embed.Description = $"**Description:** Temp mod a memeber.\n**Usage:** {prefix}temp [user] [time]\n**Example:** {prefix}temp @Jiggmin 60";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
            {
                SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                if (user.Id == context.User.Id)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you cannot temp mod yourself.");
                    return;
                }
                double minutes = Math.Round(Convert.ToDouble(time), 0);
                SocketTextChannel roles = user.Guild.Channels.Where(x => x.Name.ToUpper() == "Welcome".ToUpper()).First() as SocketTextChannel;

                IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Temp Mod".ToUpper());
                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = $"Temp Modding User | Mod: {context.User.Username}#{context.User.Discriminator}"
                };
                await user.AddRolesAsync(role, options);
                await context.Message.DeleteAsync();
                if (time.Equals("1"))
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for **{time}** minute. " +
                                $"May they reign in a minute of peace and prosperity! Read more about mods and what they do in {roles.Mention}.");
                }
                else if (minutes <= 60)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for **{time}** minutes. " +
                                $"May they reign in minutes of peace and prosperity! Read more about mods and what they do in {roles.Mention}.");
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} has promoted {user.Mention} to a temporary moderator on the discord server for **{time}** minutes. " +
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
                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
            }
        }

        public async Task UntempAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (context.Guild.Id != 528679522707701760)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}untemp";
                embed.Description = $"**Description:** Untemp a user.\n**Usage:** {prefix}untemp [user]\n**Example:** {prefix}untemp @Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
            {
                SocketGuildUser user = context.Guild.GetUser(Extensions.UserInGuild(context.Message, context.Guild, username).Id);
                if (!user.Roles.Any(e => e.Name == "Temp Mod"))
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} this user is not a temp mod.");
                }
                else
                {
                    IEnumerable<SocketRole> role = user.Guild.Roles.Where(input => input.Name.ToUpper() == "Temp Mod".ToUpper());
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Untemp Modding User | Mod: {context.User.Username}#{context.User.Discriminator}"
                    };
                    await user.RemoveRolesAsync(role, options);
                    await context.Message.DeleteAsync();
                    await context.Channel.SendMessageAsync($"{context.User.Mention} has removed temp mod from {user.Mention}");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
            }
        }

        public async Task ResetPR2NameAsync(SocketCommandContext context, [Remainder] string username)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command: {prefix}resetpr2name";
                    embed.Description = $"**Description:** Reset a users PR2 Name.\n**Usage:** {prefix}resetpr2name [user]\n**Example:** {prefix}resetpr2name Jiggmin";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = Extensions.UserInGuild(context.Message, context.Guild, username) as SocketGuildUser;
                    await context.Message.DeleteAsync();
                    string pr2name = User.GetUser("user_id", user.Id.ToString()).PR2Name;
                    User.SetValue(context.User, "pr2_name", "NULL");
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"PR2 Name reset by: {context.User.Username}#{context.User.Discriminator}"
                    };
                    await user.RemoveRolesAsync(context.Guild.Roles.Where(x => x.Name.ToUpper() == "Verified".ToUpper()), options);
                    if (user.Nickname.Equals(pr2name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        options.AuditLogReason = "Resetting nickname";
                        await user.ModifyAsync(x => x.Nickname = null, options);
                    }
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully reset **{Format.Sanitize(user.Username)}#{user.Discriminator}'s** PR2 Name.");
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**. If this user is not in the server contact Stxtics#0001.");
                }
            }
            else
            {
                return;
            }
        }

        public async Task AddRoleAsync(SocketCommandContext context, [Remainder] string settings)
        {
            if (string.IsNullOrWhiteSpace(settings))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}addrole";
                embed.Description = $"**Description:** Add a new role, with optional color and hoist.\n**Usage:** {prefix}addrole [name] [hex color] [hoist]\n**Example:** {prefix}addrole Test #FF0000 true";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = $"Created by: {context.User.Username}#{context.User.Discriminator}"
                };
                if (settings.Contains("#"))
                {
                    string[] settingsSplit = settings.Split("#");
                    if (settingsSplit[1].Contains("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string[] settingsSplit2 = settingsSplit[1].Split(" ");
                        try
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(settingsSplit2[0].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                            bool hoisted = Convert.ToBoolean(settingsSplit2[1]);
                            await context.Guild.CreateRoleAsync(settingsSplit[0], null, new Color(color.R, color.G, color.B), hoisted, options);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} created role **{Format.Sanitize(settingsSplit[0])}** with color **#{settingsSplit2[0]}**, and is displayed separately.");
                        }
                        catch (FormatException)
                        {
                            await context.Guild.CreateRoleAsync(settings, null, null, false, options);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} created role **{Format.Sanitize(settings)}**.");
                        }
                    }
                    else
                    {
                        try
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(settingsSplit[1].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                            await context.Guild.CreateRoleAsync(settingsSplit[0], null, new Color(color.R, color.G, color.B), false, options);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} created role **{Format.Sanitize(settingsSplit[0])}** with color **#{settingsSplit[1]}**.");
                        }
                        catch (FormatException)
                        {
                            await context.Guild.CreateRoleAsync(settings, null, null, false, options);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} created role **{Format.Sanitize(settings)}**.");
                        }
                    }
                }
                else
                {
                    await context.Guild.CreateRoleAsync(settings);
                    await context.Channel.SendMessageAsync($"{context.User.Mention} created role **{Format.Sanitize(settings)}**.");
                }
            }
        }

        public async Task DeleteRoleAsync(SocketCommandContext context, [Remainder] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}delrole";
                embed.Description = $"**Description:** Delete a role.\n**Usage:** {prefix}delrole [role]\n**Example:** {prefix}delrole Admins";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Deleted by: {context.User.Username}#{context.User.Discriminator}"
                    };
                    await role.DeleteAsync(options);
                    await context.Channel.SendMessageAsync($"{context.User.Mention} deleted role **{Format.Sanitize(roleName)}**");
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find role with name or ID **{Format.Sanitize(roleName)}**.");
                }
            }
        }

        public async Task MentionableAsync(SocketCommandContext context, [Remainder] string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}mentionable";
                embed.Description = $"**Description:** Toggle making a role mentionable on/off.\n**Usage:** {prefix}mentionable [role]\n**Example:** {prefix}mentionable Admins";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Toggled Mentionable by: {context.User.Username}#{context.User.Discriminator}"
                    };
                    if (role.IsMentionable)
                    {
                        await role.ModifyAsync(x => x.Mentionable = false, options);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(role.Name)}** is no longer mentionable");
                    }
                    else
                    {
                        await role.ModifyAsync(x => x.Mentionable = true, options);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(role.Name)}** is now mentionable");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find role with name or ID **{Format.Sanitize(roleName)}**.");
                }
            }
        }

        public async Task RoleColorAsync(SocketCommandContext context, [Remainder] string roleNameAndColor)
        {
            if (string.IsNullOrWhiteSpace(roleNameAndColor))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}rolecolor";
                embed.Description = $"**Description:** Change the color of a role.\n**Usage:** {prefix}rolecolor [role] [hex color]\n**Example:** {prefix}rolecolor Admins #FF0000";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (roleNameAndColor.Contains("#"))
                {
                    string[] split = roleNameAndColor.Split("#");
                    string roleName = split[0];
                    if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                    {
                        SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                        try
                        {
                            System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(split[1].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = $"Changed by: {context.User.Mention}#{context.User.Discriminator}"
                            };
                            await role.ModifyAsync(x => x.Color = new Color(color.R, color.G, color.B), options);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} successfully changed the color of **{Format.Sanitize(role.Name)}** to **#{split[1]}**.");
                        }
                        catch (FormatException)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the hex color **#{Format.Sanitize(split[1])}** is not a valid hex color.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find role with name or ID **{Format.Sanitize(roleName)}**.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to enter a hex color to change the role to.");
                }
            }
        }

        public async Task AddJoinableRoleAsync(SocketCommandContext context, string roleName, [Remainder] string description)
        {
            if (roleName == null || description == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}addjoinablerole";
                embed.Description = $"**Description:** Add a role that users can add themselves to.\n**Usage:** {prefix}addjoinablerole [role] [description]\n**Example:** {prefix}addjoinablerole HH Mentioned when there is a happy hour.";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (description.Length > 100)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the role description must be 100 characters or less.");
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                    if (JoinableRole.Get(context.Guild.Id, role.Id).Count > 0)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a joinable role.");
                    }
                    else
                    {
                        JoinableRole.Add(context.Guild.Id, role.Id, description);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} added joinable role **{Format.Sanitize(role.Name)}**.");
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Added Joinable Role",
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
                            embed.Description = $"{context.User.Mention} added {role.Mention} to the joinable roles.";
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} the role with name or ID **{Format.Sanitize(roleName)}** does not exist or could not be found.");
                }
            }
        }

        public async Task DeleteJoinableRoleAsync(SocketCommandContext context, [Remainder] string roleName)
        {
            if (roleName == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}deljoinablerole";
                embed.Description = $"**Description:** Delete a role that users can add themselves to.\n**Usage:** {prefix}deljoinablerole [role]\n**Example:** {prefix}deljoinablerole Arti";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                    if (JoinableRole.Get(context.Guild.Id, role.Id).Count > 0)
                    {
                        JoinableRole.Remove(context.Guild.Id, role.Id);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} removed joinable role **{Format.Sanitize(role.Name)}**.");
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Removed Joinable Role",
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
                            embed.Description = $"{context.User.Mention} removed {role.Mention} from the joinable roles.";
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(role.Name)}** is not a joinable role.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} the role with name or ID **{Format.Sanitize(roleName)}** does not exist or could not be found.");
                }
            }
        }

        public async Task AddModAsync(SocketCommandContext context, [Remainder] string mod)
        {
            if (mod == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}addmod";
                embed.Description = $"**Description:** Add a bot moderator or group of moderators.\n**Usage:** {prefix}addmod [user or role]\n**Example:** {prefix}addmod Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, mod);
                    if (DiscordStaff.Get(context.Guild.Id, "r-" + role.Id).Count > 0)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a mod role.");
                    }
                    else
                    {
                        DiscordStaff.Add(context.Guild.Id, "r-" + role.Id);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} added mod role **{Format.Sanitize(role.Name)}**.");
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Added Mod Role",
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
                            embed.Description = $"{context.User.Mention} added {role.Mention} to the mod roles.";
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    return;
                }
                else if (Extensions.UserInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, mod);
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0)
                    {
                        await context.Channel.SendMessageAsync($"{ context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already a mod.");
                    }
                    else
                    {
                        DiscordStaff.Add(context.Guild.Id, "u-" + user.Id);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} added mod user **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Added Mod User",
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
                            embed.Description = $"{context.User.Mention} added {user.Mention} to the mod users.";
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user or role with name or ID **{Format.Sanitize(mod)}**.");
                }
            }
        }

        public async Task DeleteModAsync(SocketCommandContext context, [Remainder] string mod)
        {
            if (string.IsNullOrWhiteSpace(mod))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}delmod";
                embed.Description = $"**Description:** Remove a bot moderator or group of moderators.\n**Usage:** {prefix}delmod [user or role]\n**Example:** {prefix}delmod Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, mod);
                    if (DiscordStaff.Get(context.Guild.Id, "r-" + role.Id).Count > 0)
                    {
                        DiscordStaff.Remove(context.Guild.Id, "r-" + role.Id);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} removed mod role **{Format.Sanitize(mod)}**.");
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Removed Mod Role",
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
                            embed.Description = $"{context.User.Mention} removed {role.Mention} from the mod roles.";
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(mod)}** is not a mod role.");
                    }
                }
                else if (Extensions.UserInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, mod);
                    if (DiscordStaff.Get(context.Guild.Id, "u-" + user.Id).Count > 0)
                    {
                        DiscordStaff.Remove(context.Guild.Id, "u-" + user.Id);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} removed mod user **{Format.Sanitize(mod)}**.");
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Removed Mod User",
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
                            embed.Description = $"{context.User.Mention} removed {user.Mention} from the mod users.";
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the user **{Format.Sanitize(mod)}** is not a mod.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find user or role with name or ID **{Format.Sanitize(mod)}**.");
                }
            }
        }

        public async Task ListModsAsync(SocketCommandContext context)
        {
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = context.Guild.IconUrl,
                Name = "List Mods"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                Author = auth
            };
            List<DiscordStaff> discordStaff = DiscordStaff.Get(context.Guild.Id);
            string roles = "", users = "";
            foreach (DiscordStaff staff in discordStaff)
            {
                if (staff.StaffID.Contains("r"))
                {
                    try
                    {
                        SocketRole role = context.Guild.GetRole(ulong.Parse(staff.StaffID.Split("-").Last()));
                        roles += Format.Sanitize(role.Name) + "\n";
                    }
                    catch (Discord.Net.HttpException)
                    {
                        DiscordStaff.Remove(context.Guild.Id, staff.StaffID);
                    }
                }
                else
                {
                    try
                    {
                        SocketGuildUser user = context.Guild.GetUser(ulong.Parse(staff.StaffID.Split("-").Last()));
                        users += Format.Sanitize(user.Username) + "#" + user.Discriminator + "\n";
                    }
                    catch (Discord.Net.HttpException)
                    {
                        DiscordStaff.Remove(context.Guild.Id, staff.StaffID);
                    }
                }
            }
            if (roles.Length > 0)
            {
                embed.AddField("Mod Roles", roles);
            }
            if (users.Length > 0)
            {
                embed.AddField("Mod Users", users);
            }
            await context.Channel.SendMessageAsync("", false, embed.Build());
        }

        public async Task BlacklistWordAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}blacklistword";
                embed.Description = $"**Description:** Blacklist a word from being said in the server.\n**Usage:** {prefix}blacklistword [word]\n**Example:** {prefix}blacklistword freak monster";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                string[] words = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                bool blacklisted = false;
                foreach (string word in words)
                {
                    if (BlacklistedWord.Get(context.Guild.Id, word).Count() < 1)
                    {
                        BlacklistedWord.Add(context.Guild.Id, word);
                        count++;
                    }
                    else if (words.Count() == 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the word **{Format.Sanitize(word)}** is already a blacklisted word.");
                    }
                    else
                    {
                        blacklisted = true;
                    }
                }
                if (blacklisted)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more words are already a blacklisted word.");
                }
                else if (count > 0)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0)
                    };
                    if (count == 1)
                    {
                        embed.Description = $"{context.User.Mention} blacklisted the word **{Format.Sanitize(text)}**.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted the word **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{context.User.Mention} blacklisted **{count}** words.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted **{count}** words.");
                    }
                    SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                    if (log != null)
                    {
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Word Blacklist Add",
                            IconUrl = context.Guild.IconUrl
                        };
                        embed.WithAuthor(author);
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {context.User.Id}",
                            IconUrl = context.User.GetAvatarUrl()
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task UnblacklistWordAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}unblacklistword";
                embed.Description = $"**Description:** Unblacklist a word from being said in the server.\n**Usage:** {prefix}unblacklistword [word]\n**Example:** {prefix}unblacklistword freak monster";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                string[] words = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                bool blacklisted = false;
                foreach (string word in words)
                {
                    if (BlacklistedWord.Get(context.Guild.Id, word).Count() > 0)
                    {
                        BlacklistedWord.Remove(context.Guild.Id, word);
                        count++;
                    }
                    else if (words.Count() == 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the word **{Format.Sanitize(word)}** is not a blacklisted word.");
                    }
                    else
                    {
                        blacklisted = true;
                    }
                }
                if (blacklisted)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more words are not blacklisted words.");
                }
                else if (count > 0)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(0, 255, 0)
                    };
                    if (count == 1)
                    {
                        embed.Description = $"{context.User.Mention} unblacklisted the word **{Format.Sanitize(text)}**.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully unblacklisted the word **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{context.User.Mention} unblacklisted **{count}** words.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully unblacklisted **{count}** words.");
                    }
                    SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                    if (log != null)
                    {
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Word Blacklist Remove",
                            IconUrl = context.Guild.IconUrl
                        };
                        embed.WithAuthor(author);
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {context.User.Id}",
                            IconUrl = context.User.GetAvatarUrl()
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task ListBlacklistedWordsAsync(SocketCommandContext context)
        {
            List<BlacklistedWord> currentBlacklistedWords = BlacklistedWord.Get(context.Guild.Id);
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = context.Guild.IconUrl,
                Name = "List Blacklisted Words"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                Author = auth
            };
            if (currentBlacklistedWords.Count <= 0)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} there are no Blacklisted Words.");
            }
            else
            {
                foreach (BlacklistedWord word in currentBlacklistedWords)
                {
                    embed.Description += Format.Sanitize(word.Word) + "\n";
                }
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task BlacklistUrlAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}blacklisturl";
                embed.Description = $"**Description:** Blacklist a url from being said in the server.\n**Usage:** {prefix}blacklisturl [url]\n**Example:** {prefix}blacklisturl pr2hub.com";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                string[] urls = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                bool blacklisted = false;
                foreach (string url in urls)
                {
                    if (BlacklistedUrl.Get(context.Guild.Id, url).Count() < 1)
                    {
                        BlacklistedUrl.Add(context.Guild.Id, url);
                        count++;
                    }
                    else if (urls.Count() == 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the url **{Format.Sanitize(url)}** is already a blacklisted url.");
                    }
                    else
                    {
                        blacklisted = true;
                    }
                }
                if (blacklisted)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more urls are already a blacklisted url.");
                }
                else if (count > 0)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0)
                    };
                    if (count == 1)
                    {
                        embed.Description = $"{context.User.Mention} blacklisted the url **{Format.Sanitize(text)}**.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted the url **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{context.User.Mention} blacklisted **{count}** urls.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted **{count}** urls.");
                    }
                    SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                    if (log != null)
                    {
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Url Blacklist Add",
                            IconUrl = context.Guild.IconUrl
                        };
                        embed.WithAuthor(author);
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {context.User.Id}",
                            IconUrl = context.User.GetAvatarUrl()
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task UnblacklistUrlAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}unblacklisturl";
                embed.Description = $"**Description:** Unblacklist a url from being said in the server.\n**Usage:** {prefix}unblacklisturl [url]\n**Example:** {prefix}unblacklisturl pr2hub.com";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                string[] urls = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                bool blacklisted = false;
                foreach (string url in urls)
                {
                    if (BlacklistedUrl.Get(context.Guild.Id, url).Count() > 0)
                    {
                        BlacklistedUrl.Remove(context.Guild.Id, url);
                        count++;
                    }
                    else if (urls.Count() == 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the url **{Format.Sanitize(url)}** is not a blacklisted url.");
                    }
                    else
                    {
                        blacklisted = true;
                    }
                }
                if (blacklisted)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more urls are not blacklisted urls.");
                }
                else if (count > 0)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0)
                    };
                    if (count == 1)
                    {
                        embed.Description = $"{context.User.Mention} unblacklisted the url **{Format.Sanitize(text)}**.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully unblacklisted the url **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{context.User.Mention} unblacklisted **{count}** urls.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully unblacklisted **{count}** urls.");
                    }
                    SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                    if (log != null)
                    {
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Url Blacklist Remove",
                            IconUrl = context.Guild.IconUrl
                        };
                        embed.WithAuthor(author);
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {context.User.Id}",
                            IconUrl = context.User.GetAvatarUrl()
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task ListBlacklistedUrlsAsync(SocketCommandContext context)
        {
            List<BlacklistedUrl> currentBlacklistedUrls = BlacklistedUrl.Get(context.Guild.Id);
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = context.Guild.IconUrl,
                Name = "List Blacklisted Urls"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                Author = auth
            };
            if (currentBlacklistedUrls.Count <= 0)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} there are no Blacklisted Urls.");
            }
            else
            {
                foreach (BlacklistedUrl url in currentBlacklistedUrls)
                {
                    embed.Description += Format.Sanitize(url.Url) + "\n";
                }
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
        }

        public async Task AddAllowedChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}addallowedchannel";
                embed.Description = $"**Description:** Add a channel that PR2 commands can be done in.\n**Usage:** {prefix}addallowedchannel [name, id, mention]\n**Example:** {prefix}addallowedchannel pr2-discussion";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                bool allowed = false, exists = false, textChannel = false;
                foreach (string channelName in channels)
                {
                    if (Extensions.ChannelInGuild(context.Message, context.Guild, channelName) != null)
                    {
                        SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, channelName);
                        if (channel is SocketTextChannel)
                        {
                            List<AllowedChannel> currentAllowedChannels = AllowedChannel.Get(context.Guild.Id);
                            if (currentAllowedChannels.Where(x => x.ChannelID == long.Parse(channel.Id.ToString())).Count() < 1)
                            {
                                AllowedChannel.Add(context.Guild.Id, channel.Id);
                                count++;
                            }
                            else if (channels.Count() == 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is already an allowed channel for PR2 commands.");
                            }
                            else
                            {
                                allowed = true;
                            }
                        }
                        else if (channels.Count() == 1)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not a text channel.");
                        }
                        else
                        {
                            textChannel = true;
                        }
                    }
                    else if (channels.Count() == 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                    }
                    else
                    {
                        exists = true;
                    }
                }
                if (allowed)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are already allowed for PR2 commands.");
                }
                else if (exists)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels do not exist or could not be found.");
                }
                else if (textChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are not text channels.");
                }
                else if (count > 0)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0)
                    };
                    if (count == 1)
                    {
                        embed.Description = $"{context.User.Mention} allowed the channel **{Format.Sanitize(text)}** for PR2 commands.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully allowed the channel **{Format.Sanitize(text)}** for PR2 commands.");
                    }
                    else
                    {
                        embed.Description = $"{context.User.Mention} allowed **{count}** channels for PR2 commands.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully allowed **{count}** channels for PR2 commands.");
                    }
                    SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                    if (log != null)
                    {
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Allowed Channel Add",
                            IconUrl = context.Guild.IconUrl
                        };
                        embed.WithAuthor(author);
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {context.User.Id}",
                            IconUrl = context.User.GetAvatarUrl()
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task RemoveAllowedChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}removeallowedchannel";
                embed.Description = $"**Description:** Remove a channel that PR2 commands can be done in.\n**Usage:** {prefix}removeallowedchannel [name, id, mention]\n**Example:** {prefix}removeallowedchannel announcements";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                int count = 0;
                bool allowed = false, exists = false, textChannel = false;
                foreach (string channelName in channels)
                {
                    if (Extensions.ChannelInGuild(context.Message, context.Guild, channelName) != null)
                    {
                        SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, channelName);
                        if (channel is SocketTextChannel)
                        {
                            List<AllowedChannel> currentAllowedChannels = AllowedChannel.Get(context.Guild.Id);
                            if (currentAllowedChannels.Where(x => x.ChannelID == long.Parse(channel.Id.ToString())).Count() > 0)
                            {
                                AllowedChannel.Remove(context.Guild.Id, channel.Id);
                                count++;
                            }
                            else if (channels.Count() == 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not an allowed channel for PR2 commands.");
                            }
                            else
                            {
                                allowed = true;
                            }
                        }
                        else if (channels.Count() == 1)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not a text channel.");
                        }
                        else
                        {
                            textChannel = true;
                        }
                    }
                    else if (channels.Count() == 1)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                    }
                    else
                    {
                        exists = true;
                    }
                }
                if (allowed)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are already not allowed for PR2 commands.");
                }
                else if (exists)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels do not exist or could not be found.");
                }
                else if (textChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are not text channels.");
                }
                else if (count > 0)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0)
                    };
                    if (count == 1)
                    {
                        embed.Description = $"{context.User.Mention} disallowed the channel **{Format.Sanitize(text)}** for PR2 commands.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully disallowed the channel **{Format.Sanitize(text)}** for PR2 commands.");
                    }
                    else
                    {
                        embed.Description = $"{context.User.Mention} disallowed **{count}** channels for PR2 commands.";
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully disallowed **{count}** channels for PR2 commands.");
                    }
                    SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                    if (log != null)
                    {
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Allowed Channel Remove",
                            IconUrl = context.Guild.IconUrl
                        };
                        embed.WithAuthor(author);
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                        {
                            Text = $"ID: {context.User.Id}",
                            IconUrl = context.User.GetAvatarUrl()
                        };
                        embed.WithFooter(footer);
                        embed.WithCurrentTimestamp();
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task ListAllowedChannelsAsync(SocketCommandContext context)
        {
            List<AllowedChannel> currentAllowedChannels = AllowedChannel.Get(context.Guild.Id);
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = context.Guild.IconUrl,
                Name = "Allowed Channels"
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                Author = auth
            };
            if (currentAllowedChannels.Count <= 0)
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} there are no Allowed Channels.");
            }
            else
            {
                bool removed = false;
                foreach (AllowedChannel channel in currentAllowedChannels)
                {
                    SocketTextChannel allowedChannel = context.Guild.GetTextChannel(ulong.Parse(channel.ChannelID.ToString()));
                    if (allowedChannel != null)
                    {
                        embed.Description += Format.Sanitize(allowedChannel.Name) + "\n";
                    }
                    else
                    {
                        AllowedChannel.Remove(context.Guild.Id, ulong.Parse(channel.ChannelID.ToString()));
                        removed = true;
                    }
                }
                if (removed)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more allowed channels were removed because they no longer exist.");
                }
                if (embed.Description == null)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} there are no Allowed Channels.");
                }
                else
                {
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        public async Task AddMusicChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command: {prefix}addmusicchannel";
                    embed.Description = $"**Description:** Add a channel that PR2 commands can be done in.\n**Usage:** {prefix}addmusicchannel [name, id, mention]\n**Example:** {prefix}addmusicchannel bot-commands";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool allowed = false, exists = false, textChannel = false;
                    foreach (string channelName in channels)
                    {
                        if (Extensions.ChannelInGuild(context.Message, context.Guild, channelName) != null)
                        {
                            SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, channelName);
                            if (channel is SocketTextChannel)
                            {
                                string currentAllowedChannels = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "MusicChannels.txt"));
                                if (!currentAllowedChannels.Contains(channel.Id.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "MusicChannels.txt"), currentAllowedChannels + channel.Id.ToString() + "\n");
                                    count++;
                                }
                                else if (channels.Count() == 1)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is already an allowed channel for Music commands.");
                                }
                                else
                                {
                                    allowed = true;
                                }
                            }
                            else if (channels.Count() == 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not a text channel.");
                            }
                            else
                            {
                                textChannel = true;
                            }
                        }
                        else if (channels.Count() == 1)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                        }
                        else
                        {
                            exists = true;
                        }
                    }
                    if (allowed)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are already allowed for Music commands.");
                    }
                    else if (exists)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels do not exist or could not be found.");
                    }
                    else if (textChannel)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are not text channels.");
                    }
                    else if (count > 0)
                    {
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(255, 0, 0)
                        };
                        if (count == 1)
                        {
                            embed.Description = $"{context.User.Mention} allowed the channel **{Format.Sanitize(text)}** for Music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully allowed the channel **{Format.Sanitize(text)}** for Music commands.");
                        }
                        else
                        {
                            embed.Description = $"{context.User.Mention} allowed **{count}** channels for Music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully allowed **{count}** channels for Music commands.");
                        }
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Music Channel Add",
                                IconUrl = context.Guild.IconUrl
                            };
                            embed.WithAuthor(author);
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"ID: {context.User.Id}",
                                IconUrl = context.User.GetAvatarUrl()
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                }
            }
        }

        public async Task RemoveMusicChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    string prefix = Guild.Get(context.Guild).Prefix;
                    embed.Title = $"Command: {prefix}removemusicchannel";
                    embed.Description = $"**Description:** Remove a channel that PR2 commands can be done in.\n**Usage:** {prefix}removemusicchannel [name, id, mention]\n**Example:** {prefix}removemusicchannel announcements";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool allowed = false, exists = false, textChannel = false;
                    foreach (string channelName in channels)
                    {
                        if (Extensions.ChannelInGuild(context.Message, context.Guild, channelName) != null)
                        {
                            SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, channelName);
                            if (channel is SocketTextChannel)
                            {
                                string currentAllowedChannels = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "MusicChannels.txt"));
                                if (currentAllowedChannels.Contains(channel.Id.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    currentAllowedChannels = currentAllowedChannels.Replace(channel.Id.ToString() + "\n", string.Empty);
                                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "MusicChannels.txt"), currentAllowedChannels);
                                    count++;
                                }
                                else if (channels.Count() == 1)
                                {
                                    await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not an allowed channel for Music commands.");
                                }
                                else
                                {
                                    allowed = true;
                                }
                            }
                            else if (channels.Count() == 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not a text channel.");
                            }
                            else
                            {
                                textChannel = true;
                            }
                        }
                        else if (channels.Count() == 1)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                        }
                        else
                        {
                            exists = true;
                        }
                    }
                    if (allowed)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are already not allowed for Music commands.");
                    }
                    else if (exists)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels do not exist or could not be found.");
                    }
                    else if (textChannel)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more channels are not text channels.");
                    }
                    else if (count > 0)
                    {
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(255, 0, 0)
                        };
                        if (count == 1)
                        {
                            embed.Description = $"{context.User.Mention} disallowed the channel **{Format.Sanitize(text)}** for Music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully disallowed the channel **{Format.Sanitize(text)}** for Music commands.");
                        }
                        else
                        {
                            embed.Description = $"{context.User.Mention} allowed **{count}** dischannels for Music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully disallowed **{count}** channels for Music commands.");
                        }
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Music Channel Remove",
                                IconUrl = context.Guild.IconUrl
                            };
                            embed.WithAuthor(author);
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"ID: {context.User.Id}",
                                IconUrl = context.User.GetAvatarUrl()
                            };
                            embed.WithFooter(footer);
                            embed.WithCurrentTimestamp();
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                }
            }
        }

        public async Task ListMusicChannelsAsync(SocketCommandContext context)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                StreamReader allowedChannels = new StreamReader(path: Path.Combine(Extensions.downloadPath, "MusicChannels.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = context.Guild.IconUrl,
                    Name = "List Music Channels"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                    Author = auth
                };
                string currentAllowedChannels = "";
                string channel = allowedChannels.ReadLine();
                while (channel != null)
                {
                    currentAllowedChannels = currentAllowedChannels + Format.Sanitize(context.Guild.GetTextChannel(ulong.Parse(channel)).Name) + "\n";
                    channel = allowedChannels.ReadLine();
                }
                allowedChannels.Close();
                if (currentAllowedChannels.Length <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} there are no Music Channels.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Allowed Channels";
                        y.Value = currentAllowedChannels;
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

        public async Task SetLogChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}logchannel";
                embed.Description = $"**Description:** Set the log channel for the server.\n**Usage:** {prefix}logchannel [channel]\n**Example:** {prefix}logchannel #log";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.ChannelInGuild(context.Message, context.Guild, text) != null)
            {
                SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, text);
                if (channel is SocketTextChannel)
                {
                    SocketTextChannel logChannel = Extensions.GetLogChannel(context.Guild);
                    if (logChannel == null || logChannel != channel)
                    {
                        SocketTextChannel log = context.Guild.GetTextChannel(channel.Id);
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Log Channel Changed",
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
                            Color = new Color(0, 0, 255),
                            Footer = footer
                        };
                        embed.WithCurrentTimestamp();
                        if (logChannel != null)
                        {
                            embed.Description = $"{context.User.Mention} changed the log channel from **{Format.Sanitize(logChannel.Name)}** to **{Format.Sanitize(channel.Name)}**.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the log channel was successfully changed from **{Format.Sanitize(logChannel.Name)}** to **{Format.Sanitize(channel.Name)}**.");
                        }
                        else
                        {
                            embed.Description = $"{context.User.Mention} set the log channel as **{Format.Sanitize(channel.Name)}**.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} set the log channel as **{Format.Sanitize(channel.Name)}**.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                        Guild.SetLogChannel(context.Guild, channel.Id);
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that channel is already the log channel.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} the log channel must be a text channel.");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
            }
        }

        public async Task SetNotificationsChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}notificationschannel";
                embed.Description = $"**Description:** Set the channel for HH and Arti messages.\n**Usage:** {prefix}notificationschannel [channel]\n**Example:** {prefix}notificationschannel #pr2";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.ChannelInGuild(context.Message, context.Guild, text) != null)
            {
                SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, text);
                if (channel is SocketTextChannel)
                {
                    SocketTextChannel notificationsChannel = Extensions.GetNotificationsChannel(context.Guild);
                    if (notificationsChannel == null || notificationsChannel != channel)
                    {
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Notifications Channel Changed",
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
                                Color = new Color(0, 0, 255),
                                Footer = footer
                            };
                            embed.WithCurrentTimestamp();
                            if (notificationsChannel != null)
                            {
                                embed.Description = $"{context.User.Mention} changed the notifications channel from **{Format.Sanitize(notificationsChannel.Name)}** to **{Format.Sanitize(channel.Name)}**.";
                            }
                            else
                            {
                                embed.Description = $"{context.User.Mention} set the notifications channel as **{Format.Sanitize(channel.Name)}**.";
                            }
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        Guild.SetNotificationsChannel(context.Guild, channel.Id);
                        if (notificationsChannel != null)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the notifications channel was successfully changed from **{Format.Sanitize(notificationsChannel.Name)}** to **{Format.Sanitize(channel.Name)}**.");
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} set the notifications channel as **{Format.Sanitize(channel.Name)}**.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that channel is already the notifications channel.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} the notifications channel must be a text channel.");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
            }
        }

        public async Task SetBanLogChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string prefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {prefix}banlogchannel";
                embed.Description = $"**Description:** Set the ban log channel for the server.\n**Usage:** {prefix}banlogchannel [channel]\n**Example:** {prefix}banlogchannel #ban-log";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else if (Extensions.ChannelInGuild(context.Message, context.Guild, text) != null)
            {
                SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, text);
                if (channel is SocketTextChannel)
                {
                    SocketTextChannel banlogChannel = Extensions.GetBanLogChannel(context.Guild);
                    if (banlogChannel == null || banlogChannel != channel)
                    {
                        SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                        if (log != null)
                        {
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Ban Log Channel Changed",
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
                                Color = new Color(0, 0, 255),
                                Footer = footer
                            };
                            embed.WithCurrentTimestamp();
                            if (banlogChannel != null)
                            {
                                embed.Description = $"{context.User.Mention} changed the ban log channel from **{Format.Sanitize(banlogChannel.Name)}** to **{Format.Sanitize(channel.Name)}**.";
                            }
                            else
                            {
                                embed.Description = $"{context.User.Mention} set the ban log channel as **{Format.Sanitize(channel.Name)}**.";
                            }
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        Guild.SetBanlogChannel(context.Guild, channel.Id);
                        if (banlogChannel != null)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel was successfully changed from **{Format.Sanitize(banlogChannel.Name)}** to **{Format.Sanitize(channel.Name)}**.");
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} set the ban log channel as **{Format.Sanitize(channel.Name)}**.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} that channel is already the ban log channel.");
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel must be a text channel.");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync($"{context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
            }
        }

        public async Task SetPrefixAsync(SocketCommandContext context, char? prefix)
        {
            if (prefix == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                string currentPrefix = Guild.Get(context.Guild).Prefix;
                embed.Title = $"Command: {currentPrefix}setprefix";
                embed.Description = $"**Description:** Set the prefix for the server.\n**Usage:** {currentPrefix}prefix [prefix]\n**Example:** {currentPrefix}prefix /";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                SocketTextChannel log = Extensions.GetLogChannel(context.Guild);
                if (log != null)
                {
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = "Prefix Changed",
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
                        Color = new Color(0, 0, 255),
                        Footer = footer
                    };
                    embed.WithCurrentTimestamp();
                    embed.Description = $"{context.User.Mention} set the prefix as **{Format.Sanitize(prefix.Value.ToString())}**.";
                    await log.SendMessageAsync("", false, embed.Build());
                }
                Guild.SetPrefix(context.Guild, prefix.Value);
                await context.Channel.SendMessageAsync($"{context.User.Mention} set the prefix as **{Format.Sanitize(prefix.Value.ToString())}**.");
            }
        }
    }
}
