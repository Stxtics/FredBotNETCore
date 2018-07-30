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

        public static async Task RemovePermInvitesAsync()
        {
            SocketGuild guild = _client.GetGuild(249657315576381450);
            var invites = guild.GetInvitesAsync();
            RequestOptions options = new RequestOptions()
            {
                AuditLogReason = "Auto Perm Invite Remove"
            };
            foreach (IInviteMetadata invite in await invites.ToAsyncEnumerable().FlattenAsync())
            {
                if (invite.MaxAge == null)
                {
                    await invite.DeleteAsync(options);
                }
            }
        }

        public static async Task SendError(string message, string stacktrace)
        {
            IUser user = _client.GetUser(181853112045142016);
            try
            {
                await user.SendMessageAsync(message + stacktrace);
            }
            catch(Exception)
            {
                Console.WriteLine(message + stacktrace);
            }
        }

        public async Task Install(DiscordSocketClient c)
        {
            _client = c;
            _cmds = new CommandService();
            await _cmds.AddModulesAsync(Assembly.GetEntryAssembly(), Program._provider);

            _client.MessageReceived += HandleCommand;

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
                await _client.SetGameAsync($"/help | pr2hub.com", null, type: ActivityType.Watching);
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
            if (role.Name != role2.Name)
            {
                await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The role **{role.Name}** was renamed to **{role2.Name}**.");
            }
            else if (role.IsMentionable != role2.IsMentionable)
            {
                if (role.IsMentionable)
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The role **{role.Name}** is no longer mentionable.");
                }
                else
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The role **{role.Name}** is now mentionable.");
                }
            }
            else if (role.IsHoisted != role2.IsHoisted)
            {
                if (role.IsHoisted)
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The role **{role.Name}** is no longer displayed separately.");
                }
                else
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The role **{role.Name}** is now displayed separately.");
                }
            }
            else if (!role.Color.Equals(role2.Color))
            {
                await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The role **{role.Name}** color was changed from **#{PublicModule.HexConverter(role.Color)}** to **#{PublicModule.HexConverter(role2.Color)}**.");
            }
        }

        public async Task AnnounceRoleDeleted(SocketRole role)
        {
            if (role.Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = role.Guild.GetTextChannel(327575359765610496);
            await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Role deleted: **{role.Name}**.");
        }

        public async Task AnnounceRoleCreated(SocketRole role)
        {
            if (role.Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = role.Guild.GetTextChannel(327575359765610496);
            await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Role created: **{role.Name}**.");
        }

        public async Task AnnounceChannelUpdated(SocketChannel channel, SocketChannel channel2)
        {
            if ((channel as SocketGuildChannel).Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(327575359765610496);
            if (channel is SocketTextChannel)
            {
                if ((channel as SocketTextChannel).Name != (channel2 as SocketTextChannel).Name)
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The text channel **{(channel as SocketTextChannel).Name}** was renamed to **{(channel2 as SocketTextChannel).Name}**.");
                }
                else if ((channel as SocketTextChannel).Topic != (channel2 as SocketTextChannel).Topic)
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` {(channel as SocketTextChannel).Mention}'s topic was changed from **{(channel2 as SocketTextChannel).Topic}** to **{(channel2 as SocketTextChannel).Topic}**.");
                }
                else if ((channel as SocketTextChannel).IsNsfw != (channel2 as SocketTextChannel).IsNsfw)
                {
                    if ((channel as SocketTextChannel).IsNsfw)
                    {
                        await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` {(channel as SocketTextChannel).Mention} is no longer NSFW.");
                    }
                    else
                    {
                        await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` {(channel as SocketTextChannel).Mention} is now NSFW.");
                    }
                }
            }
            else if (channel is SocketVoiceChannel)
            {
                if ((channel as SocketVoiceChannel).Name != (channel2 as SocketVoiceChannel).Name)
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The text channel **{(channel as SocketVoiceChannel).Name}** was renamed to **{(channel2 as SocketVoiceChannel).Name}**.");
                }
                else if ((channel as SocketVoiceChannel).Bitrate != (channel2 as SocketVoiceChannel).Bitrate)
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}] {(channel as SocketVoiceChannel).Name}'s birate was changed from **{(channel as SocketVoiceChannel).Bitrate}** to **{(channel2 as SocketVoiceChannel).Bitrate}**.");
                }
                else if ((channel as SocketVoiceChannel).UserLimit != (channel2 as SocketVoiceChannel).UserLimit)
                {
                    if ((channel as SocketVoiceChannel).UserLimit == null)
                    {
                        await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` {(channel as SocketVoiceChannel).Name}'s user limit was changed from **unlimited** to **{(channel2 as SocketVoiceChannel).UserLimit}**.");
                    }
                    else if ((channel2 as SocketVoiceChannel).UserLimit == null)
                    {
                        await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` {(channel as SocketVoiceChannel).Name}'s user limit was changed from **{(channel as SocketVoiceChannel).UserLimit}** to **unlimited**.");
                    }
                    else
                    {
                        await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` {(channel as SocketVoiceChannel).Name}'s user limit was changed from **{(channel as SocketVoiceChannel).UserLimit}** to **{(channel2 as SocketVoiceChannel).UserLimit}**.");
                    }
                }
            }
            else if (channel is SocketCategoryChannel)
            {
                if ((channel as SocketCategoryChannel).Name != (channel2 as SocketCategoryChannel).Name)
                {
                    await log.SendMessageAsync($":tools: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` The category channel **{(channel as SocketCategoryChannel).Name}** was renamed to **{(channel2 as SocketCategoryChannel).Name}**.");
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
            if (channel is SocketTextChannel)
            {
                await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Text channel deleted: **{(channel as SocketGuildChannel).Name}**.");
            }
            else if (channel is SocketVoiceChannel)
            {
                await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Voice channel deleted: **{(channel as SocketGuildChannel).Name}**.");
            }
            else if (channel is SocketCategoryChannel)
            {
                await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Category channel deleted: **{(channel as SocketGuildChannel).Name}**.");
            }
        }

        public async Task AnnounceChannelCreated(SocketChannel channel)
        {
            if ((channel as SocketGuildChannel).Guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = (channel as SocketGuildChannel).Guild.GetTextChannel(327575359765610496);
            if (channel is SocketTextChannel)
            {
                await log.SendMessageAsync($":white_check_mark: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Text channel created: {(channel as SocketTextChannel).Mention}.");
            }
            else if (channel is SocketVoiceChannel)
            {
                await log.SendMessageAsync($":white_check_mark: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Voice channel created: **{(channel as SocketGuildChannel).Name}**.");
            }
            else if (channel is SocketCategoryChannel)
            {
                await log.SendMessageAsync($":white_check_mark: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Category channel created: **{(channel as SocketGuildChannel).Name}**.");
            }
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
            if (user.Nickname != user2.Nickname)
            {
                string nickname = user.Nickname;
                string nickname2 = user2.Nickname;
                if (nickname == null)
                {
                    await log.SendMessageAsync($":person_with_pouting_face: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** " +
                    $"set their nickname to **{nickname2}**.");
                }
                else if (nickname2 == null)
                {
                    await log.SendMessageAsync($":person_with_pouting_face: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** " +
                    $"removed their nickname of **{nickname}**.");
                }
                else
                {
                    await log.SendMessageAsync($":person_with_pouting_face: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** " +
                        $"has changed their nickname from **{nickname}** to **{nickname2}**.");
                }
            }
            if (user.Username != user2.Username)
            {
                await log.SendMessageAsync($":person_with_pouting_face: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** " +
                    $"has changed their username to **{user2.Username}#{user2.Discriminator}**.");
            }
            if (user.Discriminator != user2.Discriminator && user.Username == user2.Username)
            {
                await log.SendMessageAsync($":person_with_pouting_face: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** " +
                    $"has changed their discriminator from **#{user.Discriminator}** to **#{user2.Discriminator}**.");
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
                    await log.SendMessageAsync($":person_with_pouting_face: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** " +
                    $"was removed from the `{role}` role.");
                }
                else
                {
                    var diff = roleList2.Except(roleList);
                    var role = diff.ElementAt(0);
                    await log.SendMessageAsync($":person_with_pouting_face: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** " +
                    $"was given the `{role}` role.");
                }
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
            SocketTextChannel log = channel2.Guild.GetTextChannel(327575359765610496);
            if (channel2.Id == log.Id || channel2.Guild.Id != 249657315576381450)
            {
                return;
            }
            foreach(Discord.Rest.RestBan ban in await channel2.Guild.GetBansAsync())
            {
                if (ban.User.Id == message2.Author.Id)
                {
                    return;
                }
            }
            await log.SendMessageAsync($":pencil: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` Channel: {channel2.Mention} **{message2.Author.Username}#{message2.Author.Discriminator}'s** " +
                $"message was deleted. Content: `{message2.Content}`");
        }

        public async Task AnnounceUserUnbanned(SocketUser user, SocketGuild guild)
        {
            if (guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = guild.GetTextChannel(327575359765610496);
            await log.SendMessageAsync($":hammer: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** was unbanned from the guild.");
        }

        public async Task AnnounceUserBanned(SocketUser user, SocketGuild guild)
        {
            if (guild.Id != 249657315576381450)
            {
                return;
            }
            SocketTextChannel log = guild.GetTextChannel(327575359765610496);
            await log.SendMessageAsync($":hammer: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** was banned from the guild. " +
                $"Total members: **{guild.MemberCount - 1}**");
        }

        public async Task AnnounceUserJoined(SocketGuildUser user)
        {
            if (user.Guild.Id == 249657315576381450)
            {
                SocketTextChannel log = user.Guild.GetTextChannel(327575359765610496), channel = user.Guild.GetTextChannel(249657315576381450);
                IEnumerable<SocketRole> members = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Members".ToUpper()), verified = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Verified".ToUpper()), muted = user.Guild.Roles.Where(has => has.Name.ToUpper() == "Muted".ToUpper());
                SocketTextChannel rules = user.Guild.GetTextChannel(249682754407497728), roles = user.Guild.GetTextChannel(260272249976782848);
                await log.SendMessageAsync($":white_check_mark: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** joined the guild. " +
                    $"Total members: **{user.Guild.MemberCount}**");
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
            SocketTextChannel channel = user.Guild.GetTextChannel(249657315576381450);
            SocketTextChannel log = user.Guild.GetTextChannel(327575359765610496);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(PublicModule.rand.Next(256), PublicModule.rand.Next(256), PublicModule.rand.Next(256)),
            };

            embed.WithCurrentTimestamp();
            embed.Title = "__**User Left**__";
            embed.Description = $"**{user.Username} left the Platform Racing Group. :frowning: **";
            await channel.SendMessageAsync("", false, embed.Build());
            await log.SendMessageAsync($":x: `[{DateTime.Now.ToUniversalTime().ToString("HH:mm:ss")}]` **{user.Username}#{user.Discriminator}** left the guild, " +
                $"or was kicked. Total members: **{user.Guild.MemberCount}**");
        }

        #endregion

        public async Task HandleCommand(SocketMessage s)
        {
            SocketUserMessage msg = s as SocketUserMessage;
            if (msg == null) return;
            
            SocketCommandContext context = new SocketCommandContext(_client, msg);

            int argPos = 0;
            if (msg.HasStringPrefix("/", ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                IResult result = await _cmds.ExecuteAsync(context, argPos, Program._provider);
            }
        }
    }
}
