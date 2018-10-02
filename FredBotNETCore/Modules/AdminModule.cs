﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules
{
    [Name("Admin")]
    [Summary("Module for commands of administrators of Platform Racing Group")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("resetpr2name", RunMode = RunMode.Async)]
        [Alias("resetverifiedname")]
        [Summary("Resets verified name.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ResetPR2Name(string username)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(username))
                    {
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(220, 220, 220)
                        };
                        embed.Title = "Command: /resetpr2name";
                        embed.Description = "**Description:** Reset a users PR2 Name.\n**Usage:** /resetpr2name [user]\n**Example:** /resetpr2name Jiggmin";
                        await Context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                    else if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username) as SocketGuildUser;
                        await Context.Message.DeleteAsync();
                        Database.VerifyUser(user, "Not verified");
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"PR2 Name reset by: {Context.User.Username}#{Context.User.Id}"
                        };
                        await user.RemoveRoleAsync(Context.Guild.GetRole(255513962798514177), options);
                        await user.AddRoleAsync(Context.Guild.GetRole(253265134393229312), options);
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully reset **{user.Username}#{user.Discriminator}'s** PR2 Name.");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user `{username}`.");
                    }
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user with ID: **{username}** is not in the Discord Server. Please check that the ID you used is correct and if it is then contact **Stxtics#0001**.");
                }
            }
            else
            {
                return;
            }
        }

        [Command("clearwarn", RunMode = RunMode.Async)]
        [Alias("clearwarns", "removewarnigs")]
        [Summary("Clear warnings a user")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ClearWarn([Remainder] string username = null)
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
                embed.Title = "Command: /clearwarn";
                embed.Description = "**Description:** Clear warnings a user.\n**Usage:** /clearwarn [user]\n**Example:** /clearwarn @Jiggmin";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username);
                        int warnCount = Convert.ToInt32(Database.WarnCount(user));
                        if (warnCount == 0)
                        {
                            await Context.Channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** has no warnings.");
                        }
                        else
                        {
                            Database.ClearWarn(user);
                            SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Warnings Cleared",
                                IconUrl = Context.Guild.IconUrl
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
                            if (warnCount == 1)
                            {
                                await Context.Channel.SendMessageAsync($"Cleared **{warnCount}** warning for **{user.Username}#{user.Discriminator}**.");
                                embed.Description = $"{Context.User.Mention} cleared **{warnCount}** warning for {user.Mention}.";
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"Cleared **{warnCount}** warnings for **{user.Username}#{user.Discriminator}**.");
                                embed.Description = $"{Context.User.Mention} cleared **{warnCount}** warnings for {user.Mention}.";
                            }
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{username}**.");
                    }
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
                }
            }
        }

        [Command("mentionable", RunMode = RunMode.Async)]
        [Alias("rolementionable")]
        [Summary("Toggle making a role mentionable on/off")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Mentionable([Remainder] string roleName = null)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /mentionable";
                embed.Description = "**Description:** Toggle making a role mentionable on/off.\n**Usage:** /mentionable [role]\n**Example:** /mentionable Admins";
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
                            AuditLogReason = $"Toggled Mentionable by: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        if (role.IsMentionable)
                        {
                            await role.ModifyAsync(x => x.Mentionable = false, options);
                            await Context.Channel.SendMessageAsync($"The role **{role.Name}** is no longer mentionable");
                        }
                        else
                        {
                            await role.ModifyAsync(x => x.Mentionable = true, options);
                            await Context.Channel.SendMessageAsync($"The role **{role.Name}** is now mentionable");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role **{roleName}**.");
                    }
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: `{roleName}`.");
                }
            }
        }

        [Command("setnick", RunMode = RunMode.Async)]
        [Alias("setnickname")]
        [Summary("Change the nickname of a user.")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        [RequireContext(ContextType.Guild)]
        public async Task SetNick(string username = null, [Remainder] string nickname = null)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(nickname))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /setnick";
                embed.Description = "**Description:** Change the nickname of a user.\n**Usage:** /setnick [user] [new nickname]\n**Example:** /setnick Jiggmin Jiggy";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                try
                {
                    if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                    {
                        SocketGuildUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username) as SocketGuildUser;
                        if (nickname.Length > 32)
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} a users nickname cannot be longer than 32 characters.");
                        }
                        else
                        {
                            RequestOptions options = new RequestOptions()
                            {
                                AuditLogReason = $"Changed by: {Context.User.Username}#{Context.User.Discriminator}"
                            };
                            await user.ModifyAsync(x => x.Nickname = nickname, options);
                            await Context.Channel.SendMessageAsync($"Successfully set the nickname of **{user.Username}#{user.Discriminator}** to **{nickname}**.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{username}**.");
                    }
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user with ID: **{username}**.");
                }
            }
        }

        [Command("nick", RunMode = RunMode.Async)]
        [Alias("botnick")]
        [Summary("Change the bot nickname.")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        [RequireContext(ContextType.Guild)]
        public async Task Nick([Remainder] string nickname = null)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /nick";
                embed.Description = "**Description:** Change the bot nickname.\n**Usage:** /nick [new nickname]\n**Example:** /nick Fred the Giant Cactus";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (nickname.Length > 32)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} my nickname cannot be longer than 32 characters.");
                }
                else
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Changed by: {Context.User.Mention}#{Context.User.Discriminator}"
                    };
                    await Context.Guild.GetUser(383927022583545859).ModifyAsync(x => x.Nickname = nickname, options);
                    await Context.Channel.SendMessageAsync($"Successfully set my nickname to **{nickname}**.");
                }
            }
        }

        [Command("rolecolor", RunMode = RunMode.Async)]
        [Alias("colorrole", "setcolor", "rolecolour")]
        [Summary("Change the color of a role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task RoleColor([Remainder] string roleNameAndColor = null)
        {
            if (string.IsNullOrWhiteSpace(roleNameAndColor))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /rolecolor";
                embed.Description = "**Description:** Change the color of a role.\n**Usage:** /rolecolor [role] [hex color]\n**Example:** /rolecolor Admins #FF0000";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                if (roleNameAndColor.Contains("#"))
                {
                    string[] split = roleNameAndColor.Split("#");
                    string roleName = split[0];
                    try
                    {
                        if (Extensions.RoleInGuild(Context.Message, Context.Guild, roleName) != null)
                        {
                            SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, roleName);
                            try
                            {
                                System.Drawing.Color color = System.Drawing.Color.FromArgb(int.Parse(split[1].Replace("#", ""), NumberStyles.AllowHexSpecifier));
                                RequestOptions options = new RequestOptions()
                                {
                                    AuditLogReason = $"Changed by: {Context.User.Mention}#{Context.User.Discriminator}"
                                };
                                await role.ModifyAsync(x => x.Color = new Color(color.R, color.G, color.B), options);
                                await Context.Channel.SendMessageAsync($"Successfully changed the color of **{role.Name}** to **#{split[1]}**.");
                            }
                            catch (FormatException)
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the hex color **#{split[1]}** is not a valid hex color.");
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role **{roleName}**.");
                        }
                    }
                    catch (Exception)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: `{roleName}`.");
                    }
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you need to enter a hex color to change the role to.");
                }
            }
        }

        [Command("addjoinablerole")]
        [Alias("addjoinrole", "ajr", "+jr", "+joinablerole")]
        [Summary("Adds a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddJoinableRole([Remainder] string roleName = null)
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            if (roleName == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 220, 220)
                };
                embed.Title = "Command: /addjoinablerole";
                embed.Description = "**Description:** Add a role that users can add themselves to.\n**Usage:** /addjoinablerole [role]\n**Example:** /addjoinablerole Arti";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                try
                {
                    if (Extensions.RoleInGuild(Context.Message, Context.Guild, roleName) != null)
                    {
                        SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, roleName);
                        string currentJoinableRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"));
                        if (currentJoinableRoles.Contains(role.Id.ToString()))
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), currentJoinableRoles);
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{role.Name}` is already a joinable role.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), currentJoinableRoles + role.Id.ToString() + "\n");
                            SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Added Joinable Role",
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
                            embed.Description = $"{Context.User.Mention} added {role.Mention} to the joinable roles.";
                            await Context.Channel.SendMessageAsync($"Added joinable role **{role.Name}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{roleName}` does not exist or could not be found.");
                    }
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: `{roleName}`.");
                }
            }
        }

        [Command("deljoinablerole")]
        [Alias("deljoinrole", "djr", "-jr", "-joinablerole")]
        [Summary("Removes a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DelJoinableRole([Remainder] string roleName = null)
        {
            if (Context.Guild.Id != 249657315576381450)
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
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                try
                {
                    if (Extensions.RoleInGuild(Context.Message, Context.Guild, roleName) != null)
                    {
                        SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, roleName);
                        string joinableRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"));
                        if (joinableRoles.Contains(role.Id.ToString()))
                        {
                            joinableRoles = joinableRoles.Replace(role.Id.ToString() + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), joinableRoles);
                            SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                            {
                                Name = "Removed Joinable Role",
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
                            embed.Description = $"{Context.User.Mention} removed {role.Mention} from the joinable roles.";
                            await Context.Channel.SendMessageAsync($"Removed joinable role **{role.Name}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{role.Name}` is not a joinable role.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{roleName}` does not exist or could not be found.");
                    }
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: `{roleName}`.");
                }
            }
        }

        [Command("addmod", RunMode = RunMode.Async)]
        [Alias("+mod")]
        [Summary("Add a bot moderator or group of moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddModUser([Remainder] string mod = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
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
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    try
                    {
                        if (Extensions.RoleInGuild(Context.Message, Context.Guild, mod) != null)
                        {
                            SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, mod);
                            string currentModRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
                            if (currentModRoles.Contains(role.Id.ToString()))
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), currentModRoles);
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{role.Name}` is already a mod role.");
                            }
                            else
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), currentModRoles + role.Id.ToString() + "\n");
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Added Mod Role",
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
                                embed.Description = $"{Context.User.Mention} added {role.Mention} to the mod roles.";
                                await Context.Channel.SendMessageAsync($"Added mod role **{role.Name}**.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                    try
                    {
                        if (Extensions.UserInGuild(Context.Message, Context.Guild, mod) != null)
                        {
                            SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, mod);
                            string currentModUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
                            if (currentModUsers.Contains(user.Id.ToString()))
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), currentModUsers);
                                await Context.Channel.SendMessageAsync($"{ Context.User.Mention} the user `{user.Username}` is already a mod.");
                            }
                            else
                            {
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), currentModUsers + user.Id.ToString() + "\n");
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Added Mod User",
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
                                embed.Description = $"{Context.User.Mention} added {user.Mention} to the mod users.";
                                await Context.Channel.SendMessageAsync($"Added mod **{user.Username}#{user.Discriminator}**.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role `{mod}`.");
                        }
                    }
                    catch (Exception)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role with ID: **{mod}**.");
                    }
                }
            }
            catch (Exception e)
            {
                await Extensions.ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("delmod", RunMode = RunMode.Async)]
        [Alias("-mod", "deletemod")]
        [Summary("Remove a bot moderator or group of moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DelMod([Remainder] string mod = null)
        {
            try
            {
                if (Context.Guild.Id != 249657315576381450)
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
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    try
                    {
                        if (Extensions.RoleInGuild(Context.Message, Context.Guild, mod) != null)
                        {
                            SocketRole role = (Extensions.RoleInGuild(Context.Message, Context.Guild, mod));
                            string modRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
                            if (modRoles.Contains(role.Id.ToString()))
                            {
                                modRoles = modRoles.Replace(role.Id.ToString() + "\n", string.Empty);
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), modRoles);
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Removed Mod Role",
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
                                embed.Description = $"{Context.User.Mention} removed {role.Mention} from the mod roles.";
                                await Context.Channel.SendMessageAsync($"Removed mod role **{mod}**.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role `{mod}` is not a mod role.");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                    try
                    {
                        if (Extensions.UserInGuild(Context.Message, Context.Guild, mod) != null)
                        {
                            SocketUser user = (Extensions.UserInGuild(Context.Message, Context.Guild, mod));
                            string modUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
                            if (modUsers.Contains(user.Id.ToString()))
                            {
                                modUsers = modUsers.Replace(user.Id.ToString() + "\n", string.Empty);
                                File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), modUsers);
                                SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                                EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                                {
                                    Name = "Removed Mod User",
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
                                embed.Description = $"{Context.User.Mention} removed {user.Mention} from the mod users.";
                                await Context.Channel.SendMessageAsync($"Removed mod **{mod}**.");
                                await log.SendMessageAsync("", false, embed.Build());
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user `{mod}` is not a mod.");
                            }
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role `{mod}`.");
                        }
                    }
                    catch (Exception)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role with ID: **{mod}**.");
                    }
                }
            }
            catch (Exception e)
            {
                await Extensions.ExceptionInfo(Context.Client as DiscordSocketClient, e.Message, e.StackTrace);
            }
        }

        [Command("listmods", RunMode = RunMode.Async)]
        [Alias("listmod", "showmods")]
        [Summary("List moderators")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListMods()
        {
            if (Context.Guild.Id != 249657315576381450)
            {
                return;
            }
            var modRoles = new StreamReader(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
            var modUsers = new StreamReader(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
            {
                IconUrl = Context.Guild.IconUrl,
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
                string user = (Context.Guild.GetUser(Convert.ToUInt64(line))).Username + "#" + (Context.Guild.GetUser(Convert.ToUInt64(line))).Discriminator;
                modU = modU + user + "\n";
                line = modUsers.ReadLine();
            }
            modUsers.Close();
            line = modRoles.ReadLine();
            while (line != null)
            {
                string role = (Context.Guild.GetRole(Convert.ToUInt64(line))).Name;
                modR = modR + role + "\n";
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
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("blacklistword", RunMode = RunMode.Async)]
        [Alias("wordblacklist")]
        [Summary("Blacklist a word from being said in the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistWord([Remainder] string word = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklistword";
                    embed.Description = "**Description:** Blacklist a word from being said in the server.\n**Usage:** /blacklistword [word]\n**Example:** /blacklistword freak monster";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string currentBlacklistedWords = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
                    if (currentBlacklistedWords.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords);
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the word `{word.Replace("`", string.Empty)}` is already a blacklisted word.");
                    }
                    else
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords + word + "\n");
                        SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Word Blacklist Add",
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
                        embed.Description = $"{Context.User.Mention} blacklisted the word **{word}**.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully blacklisted the word **{word}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        [Command("unblacklistword", RunMode = RunMode.Async)]
        [Alias("wordunblacklist")]
        [Summary("Unblacklist a word from being said on the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task UnblacklistWord([Remainder] string word = null)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = new Color(220, 200, 220)
                };
                embed.Title = "Command: /unblacklistword";
                embed.Description = "**Description:** Unblacklist a word from being said in the server.\n**Usage:** /unblacklistword [word]\n**Example:** /unblacklistword freak monster";
                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }
            else
            {
                string currentBlacklistedWords = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
                if (currentBlacklistedWords.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                {
                    currentBlacklistedWords = currentBlacklistedWords.Replace(word + "\n", string.Empty);
                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords);
                    SocketTextChannel log = Context.Guild.GetTextChannel(327575359765610496);
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = "Word Blacklist Remove",
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
                    embed.Description = $"{Context.User.Mention} unblacklisted the word **{word}**.";
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully unblacklisted the word **{word}**.");
                    await log.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the word `{word.Replace("`", string.Empty)}` is not a blacklisted word.");
                }
            }
        }

        [Command("listblacklistedwords", RunMode = RunMode.Async)]
        [Alias("lbw","blacklistedwords")]
        [Summary("Lists all the words that are blacklisted from being said on the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListBlacklistedWords()
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                var blacklistedWords = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.Guild.IconUrl,
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
                    currentBlacklistedWords = currentBlacklistedWords + word + "\n";
                    word = blacklistedWords.ReadLine();
                }
                blacklistedWords.Close();
                if (currentBlacklistedWords.Length <= 0)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} there are no blacklisted words.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Words";
                        y.Value = currentBlacklistedWords;
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
    }
}