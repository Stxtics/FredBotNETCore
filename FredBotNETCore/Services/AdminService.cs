using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
                embed.Title = "Command: /temp";
                embed.Description = "**Description:** Temp mod a memeber.\n**Usage:** /temp [user] [time]\n**Example:** /temp @Jiggmin 60";
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
                embed.Title = "Command: /untemp";
                embed.Description = "**Description:** Untemp a user.\n**Usage:** /untemp [user]\n**Example:** /untemp @Jiggmin";
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
                    embed.Title = "Command: /resetpr2name";
                    embed.Description = "**Description:** Reset a users PR2 Name.\n**Usage:** /resetpr2name [user]\n**Example:** /resetpr2name Jiggmin";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else if (Extensions.UserInGuild(context.Message, context.Guild, username) != null)
                {
                    SocketGuildUser user = Extensions.UserInGuild(context.Message, context.Guild, username) as SocketGuildUser;
                    await context.Message.DeleteAsync();
                    string pr2name = Database.GetPR2Name(user);
                    Database.VerifyUser(user, "Not verified");
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
                embed.Title = "Command: /addrole";
                embed.Description = "**Description:** Add a new role, with optional color and hoist.\n**Usage:** /addrole [name] [hex color] [hoist]\n**Example:** /addrole Test #FF0000 true";
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
                embed.Title = "Command: /delrole";
                embed.Description = "**Description:** Delete a role.\n**Usage:** /delrole [role]\n**Example:** /delrole Admins";
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
                embed.Title = "Command: /mentionable";
                embed.Description = "**Description:** Toggle making a role mentionable on/off.\n**Usage:** /mentionable [role]\n**Example:** /mentionable Admins";
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
                embed.Title = "Command: /rolecolor";
                embed.Description = "**Description:** Change the color of a role.\n**Usage:** /rolecolor [role] [hex color]\n**Example:** /rolecolor Admins #FF0000";
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
            if (context.Guild.Id != 528679522707701760)
            {
                return;
            }
            if (roleName == null || description == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /addjoinablerole";
                embed.Description = "**Description:** Add a role that users can add themselves to.\n**Usage:** /addjoinablerole [role] [description]\n**Example:** /addjoinablerole HH Mentioned when there is a happy hour.";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                    string currentJoinableRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"));
                    if (currentJoinableRoles.Contains(role.Id.ToString()))
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), currentJoinableRoles);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a joinable role.");
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), currentJoinableRoles + role.Id.ToString() + " - " + description + "\n");
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} added joinable role **{Format.Sanitize(role.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
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
            if (context.Guild.Id != 528679522707701760)
            {
                return;
            }
            if (roleName == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /deljoinablerole";
                embed.Description = "**Description:** Delete a role that users can add themselves to.\n**Usage:** /deljoinablerole [role]\n**Example:** /deljoinablerole Arti";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, roleName);
                    string joinableRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"));
                    if (joinableRoles.Contains(role.Id.ToString()))
                    {
                        string description = Extensions.GetBetween(joinableRoles, role.Id.ToString(), "\n");
                        joinableRoles = joinableRoles.Replace(role.Id.ToString() + description + "\n", string.Empty);
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), joinableRoles);
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} removed joinable role **{Format.Sanitize(role.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
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
            if (context.Guild.Id != 528679522707701760)
            {
                return;
            }
            if (mod == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /addmod";
                embed.Description = "**Description:** Add a bot moderator or group of moderators.\n**Usage:** /addmod [user or role]\n**Example:** /addmod Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, mod);
                    string currentModRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
                    if (currentModRoles.Contains(role.Id.ToString()))
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), currentModRoles);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a mod role.");
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), currentModRoles + role.Id.ToString() + "\n");
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} added mod role **{Format.Sanitize(role.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                    return;
                }
                else if (Extensions.UserInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, mod);
                    string currentModUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
                    if (currentModUsers.Contains(user.Id.ToString()))
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), currentModUsers);
                        await context.Channel.SendMessageAsync($"{ context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already a mod.");
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), currentModUsers + user.Id.ToString() + "\n");
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} added mod user **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                        await log.SendMessageAsync("", false, embed.Build());
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
            if (context.Guild.Id != 528679522707701760)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(mod))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /delmod";
                embed.Description = "**Description:** Remove a bot moderator or group of moderators.\n**Usage:** /delmod [user or role]\n**Example:** /delmod Jiggmin";
                await context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(context.Message, context.Guild, mod);
                    string modRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
                    if (modRoles.Contains(role.Id.ToString()))
                    {
                        modRoles = modRoles.Replace(role.Id.ToString() + "\n", string.Empty);
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), modRoles);
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} removed mod role **{Format.Sanitize(mod)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the role **{Format.Sanitize(mod)}** is not a mod role.");
                    }
                }
                else if (Extensions.UserInGuild(context.Message, context.Guild, mod) != null)
                {
                    SocketUser user = Extensions.UserInGuild(context.Message, context.Guild, mod);
                    string modUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
                    if (modUsers.Contains(user.Id.ToString()))
                    {
                        modUsers = modUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), modUsers);
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                        await context.Channel.SendMessageAsync($"{context.User.Mention} removed mod user **{Format.Sanitize(mod)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
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
            if (context.Guild.Id != 528679522707701760)
            {
                return;
            }
            StreamReader modRoles = new StreamReader(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
            StreamReader modUsers = new StreamReader(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
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
            string modU = "", modR = "";
            string line = modUsers.ReadLine();
            while (line != null)
            {
                string user = context.Guild.GetUser(Convert.ToUInt64(line)).Username + "#" + context.Guild.GetUser(Convert.ToUInt64(line)).Discriminator;
                modU = modU + Format.Sanitize(user) + "\n";
                line = modUsers.ReadLine();
            }
            modUsers.Close();
            line = modRoles.ReadLine();
            while (line != null)
            {
                string role = context.Guild.GetRole(Convert.ToUInt64(line)).Name;
                modR = modR + Format.Sanitize(role) + "\n";
                line = modRoles.ReadLine();
            }
            modRoles.Close();
            if (modR.Length > 0)
            {
                embed.AddField(y =>
                {
                    y.Name = "Mod Roles";
                    y.Value = modR;
                    y.IsInline = false;
                });
            }
            if (modU.Length > 0)
            {
                embed.AddField(y =>
                {
                    y.Name = "Mod Users";
                    y.Value = modU;
                    y.IsInline = false;
                });
            }
            await context.Channel.SendMessageAsync("", false, embed.Build());
        }

        public async Task BlacklistWordAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklistword";
                    embed.Description = "**Description:** Blacklist a word from being said in the server.\n**Usage:** /blacklistword [word]\n**Example:** /blacklistword freak monster";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] words = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool blacklisted = false;
                    foreach (string word in words)
                    {
                        string currentBlacklistedWords = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
                        if (!currentBlacklistedWords.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords + word + "\n");
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
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Word Blacklist Add",
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
                        Extensions.BannedWords = File.ReadAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt")).Split("\n");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task UnblacklistWordAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /unblacklistword";
                    embed.Description = "**Description:** Unblacklist a word from being said in the server.\n**Usage:** /unblacklistword [word]\n**Example:** /unblacklistword freak monster";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] words = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool blacklisted = false;
                    foreach (string word in words)
                    {
                        string currentBlacklistedWords = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
                        if (currentBlacklistedWords.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentBlacklistedWords = currentBlacklistedWords.Replace(word + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords);
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
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Word Blacklist Remove",
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
                        Extensions.BannedWords = File.ReadAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt")).Split("\n");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task ListBlacklistedWordsAsync(SocketCommandContext context)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                StreamReader blacklistedWords = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
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
                string currentBlacklistedWords = "";
                string word = blacklistedWords.ReadLine();
                while (word != null)
                {
                    currentBlacklistedWords = currentBlacklistedWords + Format.Sanitize(word) + "\n";
                    word = blacklistedWords.ReadLine();
                }
                blacklistedWords.Close();
                if (currentBlacklistedWords.Length <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} there are no blacklisted words.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Words";
                        y.Value = currentBlacklistedWords;
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

        public async Task BlacklistUrlAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklisturl";
                    embed.Description = "**Description:** Blacklist a URL from being said in the server.\n**Usage:** /blacklisturl [url]\n**Example:** /blacklisturl pr2hub.com";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] urls = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool blacklisted = false;
                    foreach (string url in urls)
                    {
                        string currentBlacklistedUrls = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"));
                        if (!currentBlacklistedUrls.Contains(url, StringComparison.InvariantCultureIgnoreCase))
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"), currentBlacklistedUrls + url + "\n");
                            count++;
                        }
                        else if (urls.Count() == 1)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the URL **{Format.Sanitize(url)}** is already a blacklisted URL.");
                        }
                        else
                        {
                            blacklisted = true;
                        }
                    }
                    if (blacklisted)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more URLs are already a blacklisted URL.");
                    }
                    else if (count > 0)
                    {
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "URL Blacklist Add",
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
                        if (count == 1)
                        {
                            embed.Description = $"{context.User.Mention} blacklisted the URL **{Format.Sanitize(text)}**.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted the URL **{Format.Sanitize(text)}**.");
                        }
                        else
                        {
                            embed.Description = $"{context.User.Mention} blacklisted **{count}** URLs.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully blacklisted **{count}** URLs.");
                        }
                        Extensions.BlacklistedUrls = File.ReadAllText(Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt")).Split("\n");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task UnblacklistUrlAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /unblacklisturl";
                    embed.Description = "**Description:** Unblacklist a URL from being said in the server.\n**Usage:** /unblacklisturl [url]\n**Example:** /unblacklisturl pr2hub.com";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] urls = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool blacklisted = false;
                    foreach (string url in urls)
                    {
                        string currentBlacklistedUrls = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"));
                        if (currentBlacklistedUrls.Contains(url, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentBlacklistedUrls = currentBlacklistedUrls.Replace(url + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "Blacklistedurls.txt"), currentBlacklistedUrls);
                            count++;
                        }
                        else if (urls.Count() == 1)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the URL **{Format.Sanitize(url)}** is not a blacklisted URL.");
                        }
                        else
                        {
                            blacklisted = true;
                        }
                    }
                    if (blacklisted)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} 1 or more URLs are not a blacklisted URL.");
                    }
                    else if (count > 0)
                    {
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "URL Blacklist Remove",
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
                        if (count == 1)
                        {
                            embed.Description = $"{context.User.Mention} unblacklisted the URL **{Format.Sanitize(text)}**.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully unblacklisted the URL **{Format.Sanitize(text)}**.");
                        }
                        else
                        {
                            embed.Description = $"{context.User.Mention} unblacklisted **{count}** URLs.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully unblacklisted **{count}** URLs.");
                        }
                        Extensions.BlacklistedUrls = File.ReadAllText(Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt")).Split("\n");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task ListBlacklistedUrlsAsync(SocketCommandContext context)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                StreamReader blacklistedUrls = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = context.Guild.IconUrl,
                    Name = "List Blacklisted URLs"
                };
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(Extensions.random.Next(255), Extensions.random.Next(255), Extensions.random.Next(255)),
                    Author = auth
                };
                string currentBlacklistedUrls = "";
                string url = blacklistedUrls.ReadLine();
                while (url != null)
                {
                    currentBlacklistedUrls = currentBlacklistedUrls + Format.Sanitize(url) + "\n";
                    url = blacklistedUrls.ReadLine();
                }
                blacklistedUrls.Close();
                if (currentBlacklistedUrls.Length <= 0)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} there are no blacklisted URLs.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted URLs";
                        y.Value = currentBlacklistedUrls;
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

        public async Task AddAllowedChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /addallowedchannel";
                    embed.Description = "**Description:** Add a channel that PR2 commands can be done in.\n**Usage:** /addallowedchannel [name, id, mention]\n**Example:** /addallowedchannel pr2-discussion";
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
                                string currentAllowedChannels = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"));
                                if (!currentAllowedChannels.Contains(channel.Id.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"), currentAllowedChannels + channel.Id.ToString() + "\n");
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
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Allowed Channel Add",
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
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task RemoveAllowedChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /removeallowedchannel";
                    embed.Description = "**Description:** Remove a channel that PR2 commands can be done in.\n**Usage:** /removeallowedchannel [name, id, mention]\n**Example:** /removeallowedchannel announcements";
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
                                string currentAllowedChannels = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"));
                                if (currentAllowedChannels.Contains(channel.Id.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    currentAllowedChannels = currentAllowedChannels.Replace(channel.Id.ToString() + "\n", string.Empty);
                                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"), currentAllowedChannels);
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
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Allowed Channel Remove",
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
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        public async Task ListAllowedChannelsAsync(SocketCommandContext context)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                StreamReader allowedChannels = new StreamReader(path: Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = context.Guild.IconUrl,
                    Name = "List Allowed Channels"
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
                    await context.Channel.SendMessageAsync($"{context.User.Mention} there are no allowedChannels.");
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
                    embed.Title = "Command: /addmusicchannel";
                    embed.Description = "**Description:** Add a channel that PR2 commands can be done in.\n**Usage:** /addmusicchannel [name, id, mention]\n**Example:** /addmusicchannel bot-commands";
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
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Music Channel Add",
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
                        await log.SendMessageAsync("", false, embed.Build());
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
                    embed.Title = "Command: /removemusicchannel";
                    embed.Description = "**Description:** Remove a channel that PR2 commands can be done in.\n**Usage:** /removemusicchannel [name, id, mention]\n**Example:** /removemusicchannel announcements";
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
                        SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Music Channel Remove",
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
                        if (count == 1)
                        {
                            embed.Description = $"{context.User.Mention} disallowed the channel **{Format.Sanitize(text)}** for Music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully disallowed the channel **{Format.Sanitize(text)}** for Music commands.");
                        }
                        else
                        {
                            embed.Description = $"{context.User.Mention} disallowed **{count}** channels for Music commands.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you have successfully disallowed **{count}** channels for Music commands.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
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
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /logchannel";
                    embed.Description = "**Description:** Set the log channel for the server.\n**Usage:** /logchannel [channel]\n**Example:** /logchannel #log";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else if (Extensions.ChannelInGuild(context.Message, context.Guild, text) != null)
                {
                    SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, text);
                    if (channel is SocketTextChannel)
                    {
                        string currentLogChannel = File.ReadAllText(Path.Combine(Extensions.downloadPath, "LogChannel.txt"));
                        if (currentLogChannel != channel.Id.ToString())
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
                            embed.Description = $"{context.User.Mention} changed the log channel from **{Format.Sanitize(context.Guild.GetTextChannel(ulong.Parse(currentLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the log channel was successfully changed from **{Format.Sanitize(context.Guild.GetTextChannel(ulong.Parse(currentLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "LogChannel.txt"), channel.Id.ToString());
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
        }

        public async Task SetNotificationsChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /notificationschannel";
                    embed.Description = "**Description:** Set the channel for HH and Arti messages.\n**Usage:** /notificationschannel [channel]\n**Example:** /notificationschannel #pr2";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else if (Extensions.ChannelInGuild(context.Message, context.Guild, text) != null)
                {
                    SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, text);
                    if (channel is SocketTextChannel)
                    {
                        string currentBanLogChannel = File.ReadAllText(Path.Combine(Extensions.downloadPath, "NotificationsChannel.txt"));
                        if (currentBanLogChannel != channel.Id.ToString())
                        {
                            SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            embed.Description = $"{context.User.Mention} changed the notifications channel from **{Format.Sanitize(context.Guild.GetTextChannel(ulong.Parse(currentBanLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the notifications channel was successfully changed from **{Format.Sanitize(context.Guild.GetTextChannel(ulong.Parse(currentBanLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "NotificationsChannel.txt"), channel.Id.ToString());
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
        }

        public async Task SetBanLogChannelAsync(SocketCommandContext context, [Remainder] string text)
        {
            if (context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /banlogchannel";
                    embed.Description = "**Description:** Set the ban log channel for the server.\n**Usage:** /banlogchannel [channel]\n**Example:** /banlogchannel #ban-log";
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else if (Extensions.ChannelInGuild(context.Message, context.Guild, text) != null)
                {
                    SocketGuildChannel channel = Extensions.ChannelInGuild(context.Message, context.Guild, text);
                    if (channel is SocketTextChannel)
                    {
                        string currentBanLogChannel = File.ReadAllText(Path.Combine(Extensions.downloadPath, "BanLogChannel.txt"));
                        if (currentBanLogChannel != channel.Id.ToString())
                        {
                            SocketTextChannel log = context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            embed.Description = $"{context.User.Mention} changed the ban log channel from **{Format.Sanitize(context.Guild.GetTextChannel(ulong.Parse(currentBanLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.";
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the ban log channel was successfully changed from **{Format.Sanitize(context.Guild.GetTextChannel(ulong.Parse(currentBanLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BanLogChannel.txt"), channel.Id.ToString());
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
        }
    }
}
