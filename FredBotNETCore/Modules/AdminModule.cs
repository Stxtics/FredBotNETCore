using Discord;
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
                        string pr2name = Database.GetPR2Name(user);
                        Database.VerifyUser(user, "Not verified");
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"PR2 Name reset by: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await user.RemoveRoleAsync(Context.Guild.GetRole(255513962798514177), options);
                        await user.AddRoleAsync(Context.Guild.GetRole(253265134393229312), options);
                        if (user.Nickname.Equals(pr2name, StringComparison.InvariantCultureIgnoreCase))
                        {
                            options.AuditLogReason = "Resetting nickname";
                            await user.ModifyAsync(x => x.Nickname = null, options);
                        }
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully reset **{Format.Sanitize(user.Username)}#{user.Discriminator}'s** PR2 Name.");
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user **{Format.Sanitize(username)}**.");
                    }
                }
                catch (NullReferenceException)
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
                            await Context.Channel.SendMessageAsync($"**{Format.Sanitize(user.Username)}#{user.Discriminator}** has no warnings.");
                        }
                        else
                        {
                            Database.ClearWarn(user);
                            SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                                await Context.Channel.SendMessageAsync($"Cleared **{warnCount}** warning for **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                                embed.Description = $"{Context.User.Mention} cleared **{warnCount}** warning for {user.Mention}.";
                            }
                            else
                            {
                                await Context.Channel.SendMessageAsync($"Cleared **{warnCount}** warnings for **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                                embed.Description = $"{Context.User.Mention} cleared **{warnCount}** warnings for {user.Mention}.";
                            }
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
                            await Context.Channel.SendMessageAsync($"The role **{Format.Sanitize(role.Name)}** is no longer mentionable");
                        }
                        else
                        {
                            await role.ModifyAsync(x => x.Mentionable = true, options);
                            await Context.Channel.SendMessageAsync($"The role **{Format.Sanitize(role.Name)}** is now mentionable");
                        }
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
                            await Context.Channel.SendMessageAsync($"Successfully set the nickname of **{Format.Sanitize(user.Username)}#{user.Discriminator}** to **{Format.Sanitize(nickname)}**.");
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
                    await Context.Channel.SendMessageAsync($"Successfully set my nickname to **{Format.Sanitize(nickname)}**.");
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
                                await Context.Channel.SendMessageAsync($"Successfully changed the color of **{Format.Sanitize(role.Name)}** to **#{split[1]}**.");
                            }
                            catch (FormatException)
                            {
                                await Context.Channel.SendMessageAsync($"{Context.User.Mention} the hex color **#{Format.Sanitize(split[1])}** is not a valid hex color.");
                            }
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
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a joinable role.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), currentJoinableRoles + role.Id.ToString() + "\n");
                            SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            await Context.Channel.SendMessageAsync($"Added joinable role **{Format.Sanitize(role.Name)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role **{Format.Sanitize(roleName)}** does not exist or could not be found.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: **{roleName}**.");
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
                            SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            await Context.Channel.SendMessageAsync($"Removed joinable role **{Format.Sanitize(role.Name)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is not a joinable role.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role **{Format.Sanitize(roleName)}** does not exist or could not be found.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find role with ID: **{roleName}**.");
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
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a mod role.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), currentModRoles + role.Id.ToString() + "\n");
                            SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            await Context.Channel.SendMessageAsync($"Added mod role **{Format.Sanitize(role.Name)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        return;
                    }
                }
                catch (NullReferenceException)
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
                            await Context.Channel.SendMessageAsync($"{ Context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already a mod.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), currentModUsers + user.Id.ToString() + "\n");
                            SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            await Context.Channel.SendMessageAsync($"Added mod **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role **{Format.Sanitize(mod)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role with ID: **{mod}**.");
                }
            }
        }

        [Command("delmod", RunMode = RunMode.Async)]
        [Alias("-mod", "deletemod")]
        [Summary("Remove a bot moderator or group of moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DelMod([Remainder] string mod = null)
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
                            SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            await Context.Channel.SendMessageAsync($"Removed mod role **{Format.Sanitize(mod)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the role **{Format.Sanitize(mod)}** is not a mod role.");
                        }
                    }
                }
                catch (NullReferenceException)
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
                            SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                            await Context.Channel.SendMessageAsync($"Removed mod **{Format.Sanitize(mod)}**.");
                            await log.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the user **{Format.Sanitize(mod)}** is not a mod.");
                        }
                    }
                    else
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role **{Format.Sanitize(mod)}**.");
                    }
                }
                catch (NullReferenceException)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find user or role with ID: **{mod}**.");
                }
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
                modU = modU + Format.Sanitize(user) + "\n";
                line = modUsers.ReadLine();
            }
            modUsers.Close();
            line = modRoles.ReadLine();
            while (line != null)
            {
                string role = (Context.Guild.GetRole(Convert.ToUInt64(line))).Name;
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
            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("blacklistword", RunMode = RunMode.Async)]
        [Alias("wordblacklist", "addblacklistedword")]
        [Summary("Blacklist a word from being said in the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistWord([Remainder] string text = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(text))
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
                    string[] words = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    foreach (string word in words)
                    {
                        string currentBlacklistedWords = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
                        if (currentBlacklistedWords.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords);
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the word **{Format.Sanitize(word)}** is already a blacklisted word.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords + word + "\n");
                            count++;
                        }
                    }
                    SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                    if (count == 1)
                    {
                        embed.Description = $"{Context.User.Mention} blacklisted the word **{Format.Sanitize(text)}**.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully blacklisted the word **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{Context.User.Mention} blacklisted **{count}** words.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully blacklisted **{count}** words.");
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        [Command("unblacklistword", RunMode = RunMode.Async)]
        [Alias("wordunblacklist", "removeblacklistedword")]
        [Summary("Unblacklist a word from being said on the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task UnblacklistWord([Remainder] string text = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(text))
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
                    string[] words = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    foreach (string word in words)
                    {
                        string currentBlacklistedWords = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
                        if (currentBlacklistedWords.Contains(word, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentBlacklistedWords = currentBlacklistedWords.Replace(word + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"), currentBlacklistedWords);
                            count++;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the word **{Format.Sanitize(word)}** is not a blacklisted word.");
                        }
                    }
                    SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
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
                    if (count == 1)
                    {
                        embed.Description = $"{Context.User.Mention} unblacklisted the word **{Format.Sanitize(text)}**.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully unblacklisted the word **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{Context.User.Mention} unblacklisted **{count}** words.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully unblacklisted **{count}** words.");
                    }
                    await log.SendMessageAsync("", false, embed.Build());
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
                    currentBlacklistedWords = currentBlacklistedWords + Format.Sanitize(word) + "\n";
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

        [Command("blacklisturl", RunMode = RunMode.Async)]
        [Alias("urlblacklist", "addblacklistedurl")]
        [Summary("Blacklist a URL from being said in the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistUrl([Remainder] string text = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklisturl";
                    embed.Description = "**Description:** Blacklist a URL from being said in the server.\n**Usage:** /blacklisturl [url]\n**Example:** /blacklisturl pr2hub.com";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] urls = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    foreach (string url in urls)
                    {
                        string currentBlacklistedUrls = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"));
                        if (currentBlacklistedUrls.Contains(url, StringComparison.InvariantCultureIgnoreCase))
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"), currentBlacklistedUrls);
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the URL **{Format.Sanitize(url)}** is already a blacklisted URL.");
                        }
                        else
                        {
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"), currentBlacklistedUrls + url + "\n");
                            count++;
                        }
                    }
                    SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = "URL Blacklist Add",
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
                    if (count == 1)
                    {
                        embed.Description = $"{Context.User.Mention} blacklisted the URL **{Format.Sanitize(text)}**.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully blacklisted the URL **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{Context.User.Mention} blacklisted **{count}** URLs.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully blacklisted **{count}** URLs.");
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        [Command("unblacklisturl", RunMode = RunMode.Async)]
        [Alias("urlunblacklist", "removeblacklistedurl")]
        [Summary("Unblacklist a URL from being said on the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task UnblacklistUrl([Remainder] string text = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /unblacklisturl";
                    embed.Description = "**Description:** Unblacklist a URL from being said in the server.\n**Usage:** /unblacklisturl [url]\n**Example:** /unblacklisturl pr2hub.com";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] urls = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    foreach (string url in urls)
                    {
                        string currentBlacklistedUrls = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"));
                        if (currentBlacklistedUrls.Contains(url, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentBlacklistedUrls = currentBlacklistedUrls.Replace(url + "\n", string.Empty);
                            File.WriteAllText(Path.Combine(Extensions.downloadPath, "Blacklistedurls.txt"), currentBlacklistedUrls);
                            count++;
                        }
                        else
                        {
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} the url **{Format.Sanitize(url)}** is not a blacklisted URL.");
                        }
                    }
                    SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
                    EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                    {
                        Name = "URL Blacklist Remove",
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
                    if (count == 1)
                    {
                        embed.Description = $"{Context.User.Mention} unblacklisted the URL **{Format.Sanitize(text)}**.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully unblacklisted the URL **{Format.Sanitize(text)}**.");
                    }
                    else
                    {
                        embed.Description = $"{Context.User.Mention} unblacklisted **{count}** URLs.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully unblacklisted **{count}** URLs.");
                    }
                    await log.SendMessageAsync("", false, embed.Build());
                }
            }
        }

        [Command("listblacklistedurls", RunMode = RunMode.Async)]
        [Alias("lbw", "blacklistedurls")]
        [Summary("Lists all the URLs that are blacklisted from being said on the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListBlacklistedUrls()
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                var blacklistedUrls = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.Guild.IconUrl,
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
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} there are no blacklisted urls.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Urls";
                        y.Value = currentBlacklistedUrls;
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

        [Command("addallowedchannel", RunMode = RunMode.Async)]
        [Alias("allowedchanneladd", "addpr2channel")]
        [Summary("Add a channel that PR2 commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddAllowedChannel([Remainder] string text = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /addallowedchannel";
                    embed.Description = "**Description:** Add a channel that PR2 commands can be done in.\n**Usage:** /addallowedchannel [name, id, mention]\n**Example:** /addallowedchannel pr2-discussion";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    try
                    {
                        foreach (string channelName in channels)
                        {
                            if (Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName) != null)
                            {
                                var channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName);
                                string currentAllowedChannels = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"));
                                if (currentAllowedChannels.Contains(channel.Id.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"), currentAllowedChannels);
                                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is already an allowed channel.");
                                }
                                else
                                {
                                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"), currentAllowedChannels + channel.Id.ToString() + "\n");
                                    count++;
                                }
                            }
                        }
                    }
                    catch (NullReferenceException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find 1 or more channels with an ID you entered.");
                    }
                    if (count > 0)
                    {
                        SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Allowed Channel Add",
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
                        if (count == 1)
                        {
                            embed.Description = $"{Context.User.Mention} allowed the channel **{Format.Sanitize(text)}** for PR2 commands.";
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully allowed the channel **{Format.Sanitize(text)}** for PR2 commands.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} allowed **{count}** channels for PR2 commands.";
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully allowed **{count}** channels for PR2 commands.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        [Command("removeallowedchannel", RunMode = RunMode.Async)]
        [Alias("delallowedchannel", "allowedchanneldel", "allowedchannelremove", "removepr2channel")]
        [Summary("Unblacklist a URL from being said on the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveAllowedChannel([Remainder] string text = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /removeallowedchannel";
                    embed.Description = "**Description:** Remove a channel that PR2 commands can be done in.\n**Usage:** /removeallowedchannel [name, id, mention]\n**Example:** /removeallowedchannel announcements";
                    await Context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    try
                    {
                        foreach (string channelName in channels)
                        {
                            if (Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName) != null)
                            {
                                var channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName);
                                string currentAllowedChannels = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"));
                                if (currentAllowedChannels.Contains(channel.Id.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                {
                                    currentAllowedChannels = currentAllowedChannels.Replace(channel.Id.ToString() + "\n", string.Empty);
                                    File.WriteAllText(Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"), currentAllowedChannels);
                                    count++;
                                }
                                else
                                {
                                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not an allowed channel for PR2 commands.");
                                }
                            }
                        }
                    }
                    catch (NullReferenceException)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} I could not find 1 or more channels with an ID you entered.");
                    }
                    if (count > 0)
                    {
                        SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Allowed Channel Remove",
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
                        if (count == 1)
                        {
                            embed.Description = $"{Context.User.Mention} disallowed the channel **{Format.Sanitize(text)}** for PR2 commands.";
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully disallowed the channel **{Format.Sanitize(text)}** for PR2 commands.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} disallowed **{count}** channels for PR2 commands.";
                            await Context.Channel.SendMessageAsync($"{Context.User.Mention} you have successfully disallowed **{count}** channels for PR2 commands.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        [Command("listallowedchannels", RunMode = RunMode.Async)]
        [Alias("allowedchannelslist", "listpr2channels")]
        [Summary("Lists all the channels that PR2 commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListAllowedChannels()
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                var allowedChannels = new StreamReader(path: Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"));
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.Guild.IconUrl,
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
                    currentAllowedChannels = currentAllowedChannels + Format.Sanitize(Context.Guild.GetTextChannel(ulong.Parse(channel)).Name) + "\n";
                    channel = allowedChannels.ReadLine();
                }
                allowedChannels.Close();
                if (currentAllowedChannels.Length <= 0)
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} there are no allowedChannels.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Allowed Channels";
                        y.Value = currentAllowedChannels;
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

        [Command("logchannel", RunMode = RunMode.Async)]
        [Alias("updatelogchannel", "setlogchannel")]
        [Summary("Sets the log channel for PRG")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetLogChannel([Remainder] string text = null)
        {
            if (Context.Guild.Id == 249657315576381450)
            {
                try
                {
                    if (Extensions.ChannelInGuild(Context.Message, Context.Guild, text) != null)
                    {
                        var channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, text);
                        string currentLogChannel = File.ReadAllText(Path.Combine(Extensions.downloadPath, "LogChannel.txt"));

                        SocketTextChannel log = Context.Guild.GetTextChannel(channel.Id);
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Log Channel Changed",
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
                            Color = new Color(0, 0, 255),
                            Footer = footer
                        };
                        embed.WithCurrentTimestamp();
                        embed.Description = $"{Context.User.Mention} changed the log channel from **{Format.Sanitize(Context.Guild.GetTextChannel(ulong.Parse(currentLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.";
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} the log channel was successfully changed from **{Format.Sanitize(Context.Guild.GetTextChannel(ulong.Parse(currentLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "LogChannel.txt"), channel.Id.ToString());
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the channel **{Format.Sanitize(text)}** does not exist or could not be found.");
                    }
                }
                catch(NullReferenceException)
                {
                    await ReplyAsync($"{Context.User.Mention} the channel with ID: **{Format.Sanitize(text)}** does not exist or could not be found.");
                }
            }
        }
    }
}
