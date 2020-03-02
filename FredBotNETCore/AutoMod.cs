using Discord;
using Discord.WebSocket;
using FredBotNETCore.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FredBotNETCore
{
    public class AutoMod
    {
        public DiscordSocketClient Client { get; set; }

        private readonly Regex rx = new Regex(File.ReadAllText(Path.Combine(Extensions.downloadPath, "UnicodeRegex.txt")));

        public AutoMod(DiscordSocketClient client)
        {
            Client = client;
        }

        public bool Spam(SocketUserMessage msg, SocketTextChannel channel)
        {
            IEnumerable<IMessage> usermessages = channel.GetMessagesAsync().Flatten().Where(x => x.Author == msg.Author).Take(4).ToEnumerable();
            if (((usermessages.ElementAt(0) as SocketUserMessage).CreatedAt - (usermessages.ElementAt(3) as SocketUserMessage).CreatedAt).Seconds < 5)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> MassMention(SocketUserMessage msg, SocketTextChannel channel)
        {
            if (msg.MentionedUsers.Distinct().Where(x => x != msg.Author).Where(x => x.IsBot == false).Count() + msg.MentionedRoles.Distinct().Where(x => x.IsMentionable == true).Count() >= 10)
            {
                SocketGuildUser user = channel.Guild.GetUser(msg.Author.Id);
                EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                {
                    Name = $"Case {Ban.CaseCount(channel.Guild.Id) + 1} | Mute | {user.Username}#{user.Discriminator}",
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
                SocketTextChannel banlog = user.Guild.GetTextChannel(263474494327226388);
                SocketRole role = user.Guild.GetRole(308331455602229268);

                RequestOptions options = new RequestOptions()
                {
                    AuditLogReason = $"Muting User | Reason: Auto - Mass mention | Mod: {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}"
                };
                await user.AddRoleAsync(role, options);
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
                    y.Value = Client.CurrentUser.Mention;
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Length";
                    y.Value = $"10 minutes";
                    y.IsInline = true;
                });
                embed.AddField(y =>
                {
                    y.Name = "Reason";
                    y.Value = "Auto - Mass mention";
                    y.IsInline = true;
                });
                string content = Format.Sanitize(msg.Content).Length > 252
                    ? Format.Sanitize(msg.Content).SplitInParts(252).ElementAt(0) + "..."
                    : Format.Sanitize(msg.Content);
                embed.AddField(y =>
                {
                    y.Name = "Message";
                    y.Value = $"**{content}**";
                    y.IsInline = true;
                });
                Ban ban = new Ban()
                {
                    GuildID = long.Parse(channel.Guild.Id.ToString()),
                    Case = Ban.CaseCount(channel.Guild.Id) + 1,
                    UserID = long.Parse(user.Id.ToString()),
                    Username = user.Username + "#" + user.Discriminator,
                    Type = "Mute",
                    ModeratorID = long.Parse(Client.CurrentUser.Id.ToString()),
                    Moderator = Client.CurrentUser.Username + "#" + Client.CurrentUser.Discriminator,
                    Reason = "Auto - Mass mention - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString()
                };
                Ban.Add(ban);
                try
                {
                    await user.SendMessageAsync($"You have been muted in **{Format.Sanitize(channel.Guild.Name)}** by {Client.CurrentUser.Mention} for **mass mentioning** and for a length of **10** minutes.");
                }
                catch (Discord.Net.HttpException)
                {
                    //cant send message
                }
                MutedUser.Add(channel.Guild.Id, user.Id);
                await channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was muted.");
                await banlog.SendMessageAsync("", false, embed.Build());
                Task task = Task.Run(async () =>
                {
                    await Task.Delay(600000);
                    SocketGuildUser usr = user as SocketGuildUser;
                    if (usr.Roles.Any(e => e.Name.ToUpperInvariant() == "Muted".ToUpperInvariant()))
                    {
                        options.AuditLogReason = "Unmuting User | Reason: Mute expired";
                        await user.RemoveRoleAsync(role, options);
                        EmbedAuthorBuilder auth2 = new EmbedAuthorBuilder()
                        {
                            Name = $"Case {Ban.CaseCount(channel.Guild.Id) + 1} | Unmute | {user.Username}#{user.Discriminator}",
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
                            y.Value = Client.CurrentUser.Mention;
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
                            await user.SendMessageAsync($"You are now unmuted in **{Format.Sanitize(channel.Guild.Name)}**.");
                        }
                        catch (Discord.Net.HttpException)
                        {
                            //cant send message
                        }
                        ban.Type = "Unmute";
                        ban.Reason = "Auto - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString();
                        Ban.Add(ban);
                        MutedUser.Remove(channel.Guild.Id, user.Id);
                    }
                    else
                    {
                        MutedUser.Remove(channel.Guild.Id, user.Id);
                    }
                });
                return true;
            }
            else if (msg.MentionedUsers.Distinct().Where(x => x != msg.Author).Where(x => x.IsBot == false).Count() + msg.MentionedRoles.Distinct().Where(x => x.IsMentionable == true).Count() >= 5)
            {
                return true;
            }
            return false;
        }

        public bool EmojiSpam(SocketUserMessage msg)
        {
            if (msg.Tags.Count(x => x.Type == TagType.Emoji) + rx.Matches(msg.Content).Count >= 6)
            {
                return true;
            }
            return false;
        }

        public bool HasBlacklistedUrl(SocketUserMessage msg, SocketTextChannel channel)
        {
            List<BlacklistedUrl> blacklistedUrls = BlacklistedUrl.Get(channel.Guild.Id);
            foreach (BlacklistedUrl blacklistedUrl in blacklistedUrls)
            {
                if (msg.Content.Contains(blacklistedUrl.Url, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasBlacklistedWord(SocketUserMessage msg, SocketTextChannel channel)
        {
            List<BlacklistedWord> blacklistedWords = BlacklistedWord.Get(channel.Guild.Id);
            foreach (BlacklistedWord blacklistedWord in blacklistedWords)
            {
                if (msg.Content.Contains(blacklistedWord.Word, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public async Task<bool> FilterMessage(SocketUserMessage msg, SocketTextChannel channel)
        {
            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
            {
                Name = "Message Deleted",
                IconUrl = channel.Guild.IconUrl
            };
            EmbedFooterBuilder footer = new EmbedFooterBuilder()
            {
                Text = $"ID: {msg.Author.Id}",
                IconUrl = msg.Author.GetAvatarUrl()
            };
            EmbedBuilder embed = new EmbedBuilder()
            {
                Author = author,
                Color = new Color(255, 0, 0),
                Footer = footer,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Reason",
                        Value = "Blacklisted word",
                        IsInline = false
                    }
                }
            };
            embed.WithCurrentTimestamp();
            SocketTextChannel log = Extensions.GetLogChannel(channel.Guild);
            if (log != null)
            {
                if (msg.Content.Length > 252)
                {
                    embed.Description = $"Message sent by {msg.Author.Mention} deleted in {channel.Mention}\nContent: **{msg.Content.Replace("`", string.Empty).SplitInParts(252).ElementAt(0)}...**";
                }
                else
                {
                    embed.Description = $"Message sent by {msg.Author.Mention} deleted in {channel.Mention}\nContent: **{msg.Content.Replace("`", string.Empty)}**";
                }
                bool badMessage = HasBlacklistedWord(msg, channel);
                Discord.Rest.RestUserMessage message;
                if (badMessage)
                {
                    await msg.DeleteAsync();
                    embed.Fields.ElementAt(0).Value = "Blacklisted word";
                    await log.SendMessageAsync("", false, embed.Build());
                    message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} watch your language.");
                    await Task.Delay(5000);
                    await message.DeleteAsync();
                    await Task.Delay(100);
                    return true;
                }
                badMessage = await MassMention(msg, channel);
                if (badMessage)
                {
                    await msg.DeleteAsync();
                    embed.Fields.ElementAt(0).Value = "Mass mention";
                    await log.SendMessageAsync("", false, embed.Build());
                    message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no mention spamming.");
                    await Task.Delay(5000);
                    await message.DeleteAsync();
                    await Task.Delay(100);
                    return true;
                }
                badMessage = HasBlacklistedUrl(msg, channel);
                if (badMessage)
                {
                    await msg.DeleteAsync();
                    embed.Fields.ElementAt(0).Value = "Blacklisted URL";
                    await log.SendMessageAsync("", false, embed.Build());
                    message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no inappropriate links.");
                    await Task.Delay(5000);
                    await message.DeleteAsync();
                    await Task.Delay(100);
                    return true;
                }
                badMessage = EmojiSpam(msg);
                if (badMessage)
                {
                    await msg.DeleteAsync();
                    embed.Fields.ElementAt(0).Value = "Too many emojis";
                    await log.SendMessageAsync("", false, embed.Build());
                    message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no emoji spamming.");
                    await Task.Delay(5000);
                    await message.DeleteAsync();
                    await Task.Delay(100);
                    return true;
                }
                //badMessage = Spam(msg, channel);
                //if (badMessage)
                //{
                //    embed.Fields.ElementAt(0).Value = "Sent 4 messages in less than 5 seconds";
                //    Extensions.Purging = true;
                //    await channel.DeleteMessagesAsync(usermessages);
                //    await log.SendMessageAsync("", false, embed.Build());
                //    Extensions.Purging = false;
                //    message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no spamming!");
                //    await Task.Delay(5000);
                //    Extensions.Purging = true;
                //    await message.DeleteAsync();
                //    Extensions.Purging = false;
                //}
            }
            return false;
        }
    }
}
