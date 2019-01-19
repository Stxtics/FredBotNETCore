using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules
{
    [Name("Admin")]
    [Summary("Module for commands of administrators of Jiggmin's Village")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        [Command("resetpr2name", RunMode = RunMode.Async)]
        [Alias("resetverifiedname")]
        [Summary("Resets verified name.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ResetPR2Name(string username)
        {
            if (Context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 220, 220)
                    };
                    embed.Title = "Command: /resetpr2name";
                    embed.Description = "**Description:** Reset a users PR2 Name.\n**Usage:** /resetpr2name [user]\n**Example:** /resetpr2name Jiggmin";
                    await ReplyAsync("", false, embed.Build());
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
                    await user.RemoveRolesAsync(Context.Guild.Roles.Where(x => x.Name.ToUpper() == "Verified".ToUpper()), options);
                    if (user.Nickname.Equals(pr2name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        options.AuditLogReason = "Resetting nickname";
                        await user.ModifyAsync(x => x.Nickname = null, options);
                    }
                    await ReplyAsync($"{Context.User.Mention} you have successfully reset **{Format.Sanitize(user.Username)}#{user.Discriminator}'s** PR2 Name.");
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**. If this user is not in PRG contact Stxtics#0001.");
                }
            }
            else
            {
                return;
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
                await ReplyAsync("", false, embed.Build());
            }
            else
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
                        await ReplyAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is no longer mentionable");
                    }
                    else
                    {
                        await role.ModifyAsync(x => x.Mentionable = true, options);
                        await ReplyAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is now mentionable");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} I could not find role with name or ID **{Format.Sanitize(roleName)}**.");
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
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.UserInGuild(Context.Message, Context.Guild, username) != null)
                {
                    SocketGuildUser user = Extensions.UserInGuild(Context.Message, Context.Guild, username) as SocketGuildUser;
                    if (nickname.Length > 32)
                    {
                        await ReplyAsync($"{Context.User.Mention} a users nickname cannot be longer than 32 characters.");
                    }
                    else
                    {
                        RequestOptions options = new RequestOptions()
                        {
                            AuditLogReason = $"Changed by: {Context.User.Username}#{Context.User.Discriminator}"
                        };
                        await user.ModifyAsync(x => x.Nickname = nickname, options);
                        await ReplyAsync($"{Context.User.Mention} successfully set the nickname of **{Format.Sanitize(user.Username)}#{user.Discriminator}** to **{Format.Sanitize(nickname)}**.");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} I could not find user with name or ID **{Format.Sanitize(username)}**.");
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
                embed.Description = "**Description:** Change the bot nickname.\n**Usage:** /nick [new nickname]\n**Example:** /nick Fred";
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (nickname.Length > 32)
                {
                    await ReplyAsync($"{Context.User.Mention} my nickname cannot be longer than 32 characters.");
                }
                else
                {
                    RequestOptions options = new RequestOptions()
                    {
                        AuditLogReason = $"Changed by: {Context.User.Mention}#{Context.User.Discriminator}"
                    };
                    await Context.Guild.GetUser(Context.Client.CurrentUser.Id).ModifyAsync(x => x.Nickname = nickname, options);
                    await ReplyAsync($"{Context.User.Mention} successfully set my nickname to **{Format.Sanitize(nickname)}**.");
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
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (roleNameAndColor.Contains("#"))
                {
                    string[] split = roleNameAndColor.Split("#");
                    string roleName = split[0];
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
                            await ReplyAsync($"{Context.User.Mention} successfully changed the color of **{Format.Sanitize(role.Name)}** to **#{split[1]}**.");
                        }
                        catch (FormatException)
                        {
                            await ReplyAsync($"{Context.User.Mention} the hex color **#{Format.Sanitize(split[1])}** is not a valid hex color.");
                        }
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} I could not find role with name or ID **{Format.Sanitize(roleName)}**.");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} you need to enter a hex color to change the role to.");
                }
            }
        }

        [Command("addjoinablerole", RunMode = RunMode.Async)]
        [Alias("addjoinrole", "ajr", "+jr", "+joinablerole")]
        [Summary("Adds a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddJoinableRole([Remainder] string roleName = null)
        {
            if (Context.Guild.Id != 528679522707701760)
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
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(Context.Message, Context.Guild, roleName) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, roleName);
                    string currentJoinableRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"));
                    if (currentJoinableRoles.Contains(role.Id.ToString()))
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "JoinableRoles.txt"), currentJoinableRoles);
                        await ReplyAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a joinable role.");
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
                        await ReplyAsync($"{Context.User.Mention} added joinable role **{Format.Sanitize(role.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} the role with name or ID **{Format.Sanitize(roleName)}** does not exist or could not be found.");
                }
            }
        }

        [Command("deljoinablerole", RunMode = RunMode.Async)]
        [Alias("deljoinrole", "djr", "-jr", "-joinablerole", "removejoinablerole", "rjr")]
        [Summary("Removes a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DelJoinableRole([Remainder] string roleName = null)
        {
            if (Context.Guild.Id != 528679522707701760)
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
                await ReplyAsync("", false, embed.Build());
            }
            else
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
                        await ReplyAsync($"{Context.User.Mention} removed joinable role **{Format.Sanitize(role.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is not a joinable role.");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} the role with name or ID **{Format.Sanitize(roleName)}** does not exist or could not be found.");
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
            if (Context.Guild.Id != 528679522707701760)
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
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(Context.Message, Context.Guild, mod) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, mod);
                    string currentModRoles = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
                    if (currentModRoles.Contains(role.Id.ToString()))
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"), currentModRoles);
                        await ReplyAsync($"{Context.User.Mention} the role **{Format.Sanitize(role.Name)}** is already a mod role.");
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
                        await ReplyAsync($"{Context.User.Mention} added mod role **{Format.Sanitize(role.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                    return;
                }
                else if (Extensions.UserInGuild(Context.Message, Context.Guild, mod) != null)
                {
                    SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, mod);
                    string currentModUsers = File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
                    if (currentModUsers.Contains(user.Id.ToString()))
                    {
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"), currentModUsers);
                        await ReplyAsync($"{ Context.User.Mention} the user **{Format.Sanitize(user.Username)}** is already a mod.");
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
                        await ReplyAsync($"{Context.User.Mention} added mod user **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} I could not find user or role with name or ID **{Format.Sanitize(mod)}**.");
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
            if (Context.Guild.Id != 528679522707701760)
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
                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                if (Extensions.RoleInGuild(Context.Message, Context.Guild, mod) != null)
                {
                    SocketRole role = Extensions.RoleInGuild(Context.Message, Context.Guild, mod);
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
                        await ReplyAsync($"{Context.User.Mention} removed mod role **{Format.Sanitize(mod)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the role **{Format.Sanitize(mod)}** is not a mod role.");
                    }
                }
                else if (Extensions.UserInGuild(Context.Message, Context.Guild, mod) != null)
                {
                    SocketUser user = Extensions.UserInGuild(Context.Message, Context.Guild, mod);
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
                        await ReplyAsync($"{Context.User.Mention} removed mod user **{Format.Sanitize(mod)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the user **{Format.Sanitize(mod)}** is not a mod.");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} I could not find user or role with name or ID **{Format.Sanitize(mod)}**.");
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
            if (Context.Guild.Id != 528679522707701760)
            {
                return;
            }
            StreamReader modRoles = new StreamReader(path: Path.Combine(Extensions.downloadPath, "DiscordStaffRoles.txt"));
            StreamReader modUsers = new StreamReader(path: Path.Combine(Extensions.downloadPath, "DiscordStaff.txt"));
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
                string user = Context.Guild.GetUser(Convert.ToUInt64(line)).Username + "#" + Context.Guild.GetUser(Convert.ToUInt64(line)).Discriminator;
                modU = modU + Format.Sanitize(user) + "\n";
                line = modUsers.ReadLine();
            }
            modUsers.Close();
            line = modRoles.ReadLine();
            while (line != null)
            {
                string role = Context.Guild.GetRole(Convert.ToUInt64(line)).Name;
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
            await ReplyAsync("", false, embed.Build());
        }

        [Command("blacklistword", RunMode = RunMode.Async)]
        [Alias("wordblacklist", "addblacklistedword")]
        [Summary("Blacklist a word from being said in the server")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistWord([Remainder] string text = null)
        {
            if (Context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklistword";
                    embed.Description = "**Description:** Blacklist a word from being said in the server.\n**Usage:** /blacklistword [word]\n**Example:** /blacklistword freak monster";
                    await ReplyAsync("", false, embed.Build());
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
                            await ReplyAsync($"{Context.User.Mention} the word **{Format.Sanitize(word)}** is already a blacklisted word.");
                        }
                        else
                        {
                            blacklisted = true;
                        }
                    }
                    if (blacklisted)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more words are already a blacklisted word.");
                    }
                    else if (count > 0)
                    {
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
                            await ReplyAsync($"{Context.User.Mention} you have successfully blacklisted the word **{Format.Sanitize(text)}**.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} blacklisted **{count}** words.";
                            await ReplyAsync($"{Context.User.Mention} you have successfully blacklisted **{count}** words.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                    }
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
            if (Context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /unblacklistword";
                    embed.Description = "**Description:** Unblacklist a word from being said in the server.\n**Usage:** /unblacklistword [word]\n**Example:** /unblacklistword freak monster";
                    await ReplyAsync("", false, embed.Build());
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
                            await ReplyAsync($"{Context.User.Mention} the word **{Format.Sanitize(word)}** is not a blacklisted word.");
                        }
                        else
                        {
                            blacklisted = true;
                        }
                    }
                    if (blacklisted)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more words are not blacklisted words.");
                    }
                    else if (count > 0)
                    {
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
                            await ReplyAsync($"{Context.User.Mention} you have successfully unblacklisted the word **{Format.Sanitize(text)}**.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} unblacklisted **{count}** words.";
                            await ReplyAsync($"{Context.User.Mention} you have successfully unblacklisted **{count}** words.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        [Command("listblacklistedwords", RunMode = RunMode.Async)]
        [Alias("lbw", "blacklistedwords")]
        [Summary("Lists all the words that are blacklisted from being said on the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListBlacklistedWords()
        {
            if (Context.Guild.Id == 528679522707701760)
            {
                StreamReader blacklistedWords = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedWords.txt"));
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
                    await ReplyAsync($"{Context.User.Mention} there are no blacklisted words.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted Words";
                        y.Value = currentBlacklistedWords;
                        y.IsInline = false;
                    });
                    await ReplyAsync("", false, embed.Build());
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
            if (Context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /blacklisturl";
                    embed.Description = "**Description:** Blacklist a URL from being said in the server.\n**Usage:** /blacklisturl [url]\n**Example:** /blacklisturl pr2hub.com";
                    await ReplyAsync("", false, embed.Build());
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
                            await ReplyAsync($"{Context.User.Mention} the URL **{Format.Sanitize(url)}** is already a blacklisted URL.");
                        }
                        else
                        {
                            blacklisted = true;
                        }
                    }
                    if (blacklisted)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more URLs are already a blacklisted URL.");
                    }
                    else if (count > 0)
                    {
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
                            await ReplyAsync($"{Context.User.Mention} you have successfully blacklisted the URL **{Format.Sanitize(text)}**.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} blacklisted **{count}** URLs.";
                            await ReplyAsync($"{Context.User.Mention} you have successfully blacklisted **{count}** URLs.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                    }
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
            if (Context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /unblacklisturl";
                    embed.Description = "**Description:** Unblacklist a URL from being said in the server.\n**Usage:** /unblacklisturl [url]\n**Example:** /unblacklisturl pr2hub.com";
                    await ReplyAsync("", false, embed.Build());
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
                            await ReplyAsync($"{Context.User.Mention} the URL **{Format.Sanitize(url)}** is not a blacklisted URL.");
                        }
                        else
                        {
                            blacklisted = true;
                        }
                    }
                    if (blacklisted)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more URLs are not a blacklisted URL.");
                    }
                    else if (count > 0)
                    {
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
                            await ReplyAsync($"{Context.User.Mention} you have successfully unblacklisted the URL **{Format.Sanitize(text)}**.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} unblacklisted **{count}** URLs.";
                            await ReplyAsync($"{Context.User.Mention} you have successfully unblacklisted **{count}** URLs.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                    }
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
            if (Context.Guild.Id == 528679522707701760)
            {
                StreamReader blacklistedUrls = new StreamReader(path: Path.Combine(Extensions.downloadPath, "BlacklistedUrls.txt"));
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
                    await ReplyAsync($"{Context.User.Mention} there are no blacklisted URLs.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Blacklisted URLs";
                        y.Value = currentBlacklistedUrls;
                        y.IsInline = false;
                    });
                    await ReplyAsync("", false, embed.Build());
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
            if (Context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /addallowedchannel";
                    embed.Description = "**Description:** Add a channel that PR2 commands can be done in.\n**Usage:** /addallowedchannel [name, id, mention]\n**Example:** /addallowedchannel pr2-discussion";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool allowed = false, exists = false, textChannel = false;
                    foreach (string channelName in channels)
                    {
                        if (Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName) != null)
                        {
                            SocketGuildChannel channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName);
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
                                    await ReplyAsync($"{Context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is already an allowed channel for PR2 commands.");
                                }
                                else
                                {
                                    allowed = true;
                                }                            
                            }
                            else if (channels.Count() == 1)
                            {
                                await ReplyAsync($"{Context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not a text channel.");
                            }
                            else
                            {
                                textChannel = true;
                            }
                        }
                        else if (channels.Count() == 1)
                        {
                            await ReplyAsync($"{Context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                        }
                        else
                        {
                            exists = true;
                        }
                    }
                    if (allowed)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more channels are already allowed for PR2 commands.");
                    }
                    else if (exists)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more channels do not exist or could not be found.");
                    }
                    else if (textChannel)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more channels are not text channels.");
                    }
                    else if (count > 0)
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
                            await ReplyAsync($"{Context.User.Mention} you have successfully allowed the channel **{Format.Sanitize(text)}** for PR2 commands.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} allowed **{count}** channels for PR2 commands.";
                            await ReplyAsync($"{Context.User.Mention} you have successfully allowed **{count}** channels for PR2 commands.");
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
            if (Context.Guild.Id == 528679522707701760)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Color = new Color(220, 200, 220)
                    };
                    embed.Title = "Command: /removeallowedchannel";
                    embed.Description = "**Description:** Remove a channel that PR2 commands can be done in.\n**Usage:** /removeallowedchannel [name, id, mention]\n**Example:** /removeallowedchannel announcements";
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    string[] channels = text.Split(", ", StringSplitOptions.RemoveEmptyEntries);
                    int count = 0;
                    bool allowed = false, exists = false, textChannel = false;
                    foreach (string channelName in channels)
                    {
                        if (Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName) != null)
                        {
                            SocketGuildChannel channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, channelName);
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
                                    await ReplyAsync($"{Context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not an allowed channel for PR2 commands.");
                                }
                                else
                                {
                                    allowed = true;
                                }
                            }
                            else if (channels.Count() == 1)
                            {
                                await ReplyAsync($"{Context.User.Mention} the channel **{Format.Sanitize(channel.Name)}** is not a text channel.");
                            }
                            else
                            {
                                textChannel = true;
                            }
                        }
                        else if (channels.Count() == 1)
                        {
                            await ReplyAsync($"{Context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                        }
                        else
                        {
                            exists = true;
                        }
                    }
                    if (allowed)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more channels are already not allowed for PR2 commands.");
                    }
                    else if (exists)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more channels do not exist or could not be found.");
                    }
                    else if (textChannel)
                    {
                        await ReplyAsync($"{Context.User.Mention} 1 or more channels are not text channels.");
                    }
                    else if (count > 0)
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
                            await ReplyAsync($"{Context.User.Mention} you have successfully disallowed the channel **{Format.Sanitize(text)}** for PR2 commands.");
                        }
                        else
                        {
                            embed.Description = $"{Context.User.Mention} disallowed **{count}** channels for PR2 commands.";
                            await ReplyAsync($"{Context.User.Mention} you have successfully disallowed **{count}** channels for PR2 commands.");
                        }
                        await log.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
        }

        [Command("listallowedchannels", RunMode = RunMode.Async)]
        [Alias("allowedchannelslist", "listpr2channels", "allowedchannels")]
        [Summary("Lists all the channels that PR2 commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListAllowedChannels()
        {
            if (Context.Guild.Id == 528679522707701760)
            {
                StreamReader allowedChannels = new StreamReader(path: Path.Combine(Extensions.downloadPath, "AllowedChannels.txt"));
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
                    await ReplyAsync($"{Context.User.Mention} there are no allowedChannels.");
                }
                else
                {
                    embed.AddField(y =>
                    {
                        y.Name = "Allowed Channels";
                        y.Value = currentAllowedChannels;
                        y.IsInline = false;
                    });
                    await ReplyAsync("", false, embed.Build());
                }
            }
            else
            {
                return;
            }
        }

        [Command("logchannel", RunMode = RunMode.Async)]
        [Alias("updatelogchannel", "setlogchannel")]
        [Summary("Sets the log channel for JV")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetLogChannel([Remainder] string text = null)
        {
            if (Context.Guild.Id == 528679522707701760)
            {
                if (Extensions.ChannelInGuild(Context.Message, Context.Guild, text) != null)
                {
                    SocketGuildChannel channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, text);
                    if (channel is SocketTextChannel)
                    {
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
                        await ReplyAsync($"{Context.User.Mention} the log channel was successfully changed from **{Format.Sanitize(Context.Guild.GetTextChannel(ulong.Parse(currentLogChannel)).Name)}** to **{Format.Sanitize(channel.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "LogChannel.txt"), channel.Id.ToString());
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the log channel must be a text channel.");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                }
            }
        }

        [Command("notificationschannel", RunMode = RunMode.Async)]
        [Alias("updatenotifactionschannel", "setnotificationschannel")]
        [Summary("Sets the log channel for PRG")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetNotifcationsChannel([Remainder] string text = null)
        {
            if (Context.Guild.Id == 528679522707701760)
            {
                if (Extensions.ChannelInGuild(Context.Message, Context.Guild, text) != null)
                {
                    SocketGuildChannel channel = Extensions.ChannelInGuild(Context.Message, Context.Guild, text);
                    if (channel is SocketTextChannel)
                    {
                        string currentNotificationsChannel = File.ReadAllText(Path.Combine(Extensions.downloadPath, "NotificationsChannel.txt"));
                        SocketTextChannel log = Context.Guild.GetTextChannel(Extensions.GetLogChannel());
                        EmbedAuthorBuilder author = new EmbedAuthorBuilder()
                        {
                            Name = "Notifications Channel Changed",
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
                        embed.Description = $"{Context.User.Mention} changed the notifications channel from **{Format.Sanitize(currentNotificationsChannel)}** to **{Format.Sanitize(channel.Name)}**.";
                        await ReplyAsync($"{Context.User.Mention} the notifications channel was successfully changed from **{Format.Sanitize(currentNotificationsChannel)}** to **{Format.Sanitize(channel.Name)}**.");
                        await log.SendMessageAsync("", false, embed.Build());
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "NotificationsChannel.txt"), channel.Name);
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the notifications channel must be a text channel.");
                    }
                }
                else
                {
                    await ReplyAsync($"{Context.User.Mention} the channel with name or ID **{Format.Sanitize(text)}** does not exist or could not be found.");
                }
            }
        }
    }
}
