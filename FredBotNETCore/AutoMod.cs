using Discord;
using Discord.WebSocket;
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

        readonly Regex rx = new Regex(File.ReadAllText(Path.Combine(Extensions.downloadPath, "UnicodeRegex.txt")));

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
                await channel.SendMessageAsync($"**{user.Username}#{user.Discriminator}** was muted.");
                await banlog.SendMessageAsync("", false, embed.Build());
                Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Mute", Client.CurrentUser.Username + "#" + Client.CurrentUser.Discriminator, "Auto - Mass mention - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                try
                {
                    await user.SendMessageAsync($"You have been muted in **{Format.Sanitize(channel.Guild.Name)}** by {Client.CurrentUser.Mention} for **mass mentioning** and for a length of **10** minutes.");
                }
                catch (Discord.Net.HttpException)
                {
                    //cant send message
                }
                string mutedUsers = File.ReadAllText(Path.Combine(Extensions.downloadPath, "MutedUsers.txt"));
                File.WriteAllText(Path.Combine(Extensions.downloadPath, "MutedUsers.txt"), mutedUsers + user.Id.ToString() + "\n");
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
                        Database.AddPrior(user, user.Username + "#" + user.Discriminator, "Unmute", moderator: "Fred the G. Cactus#1000", reason: "Auto - " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToUniversalTime().ToShortTimeString());
                        mutedUsers = File.ReadAllText(Path.Combine(Extensions.downloadPath, "MutedUsers.txt")).Replace("\n" + user.Id.ToString(), string.Empty);
                        File.WriteAllText(Path.Combine(Extensions.downloadPath, "MutedUsers.txt"), mutedUsers);
                    }
                    else
                    {
                        return;
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
            if (msg.Tags.Count(x => x.Type == TagType.Emoji) + rx.Matches(msg.Content).Count >= 5)
            {
                return true;
            }
            return false;
        }

        public bool BlacklistedUrl(SocketUserMessage msg)
        {
            string[] blacklistedUrls = Extensions.BlacklistedUrls;
            foreach (string blacklistedUrl in blacklistedUrls)
            {
                if (blacklistedUrl.Length > 0 && msg.Content.Contains(blacklistedUrl, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool BlacklistedWord(SocketUserMessage msg)
        {
            string[] bannedWords = Extensions.BannedWords;
            foreach (string bannedWord in bannedWords)
            {
                if (bannedWord.Length > 0 && msg.Content.Contains(bannedWord, StringComparison.InvariantCultureIgnoreCase))
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
            SocketTextChannel log = channel.Guild.GetTextChannel(Extensions.GetLogChannel());
            if (msg.Content.Length > 252)
            {
                embed.Description = $"Message sent by {msg.Author.Mention} deleted in {channel.Mention}\nContent: **{msg.Content.Replace("`", string.Empty).SplitInParts(252).ElementAt(0)}...**";
            }
            else
            {
                embed.Description = $"Message sent by {msg.Author.Mention} deleted in {channel.Mention}\nContent: **{msg.Content.Replace("`", string.Empty)}**";
            }
            bool badMessage = BlacklistedWord(msg);
            Discord.Rest.RestUserMessage message;
            if (badMessage)
            {
                Extensions.Purging = true;
                await msg.DeleteAsync();
                embed.Fields.ElementAt(0).Value = "Blacklisted word";
                await log.SendMessageAsync("", false, embed.Build());
                Extensions.Purging = false;
                message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} watch your language.");
                await Task.Delay(5000);
                Extensions.Purging = true;
                await message.DeleteAsync();
                await Task.Delay(100);
                Extensions.Purging = false;
                return true;
            }
            badMessage = await MassMention(msg, channel);
            if (badMessage)
            {
                Extensions.Purging = true;
                await msg.DeleteAsync();
                embed.Fields.ElementAt(0).Value = "Mass mention";
                await log.SendMessageAsync("", false, embed.Build());
                Extensions.Purging = false;
                message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no mention spamming.");
                await Task.Delay(5000);
                Extensions.Purging = true;
                await message.DeleteAsync();
                await Task.Delay(100);
                Extensions.Purging = false;
                return true;
            }
            badMessage = BlacklistedUrl(msg);
            if (badMessage)
            {
                Extensions.Purging = true;
                await msg.DeleteAsync();
                embed.Fields.ElementAt(0).Value = "Blacklisted URL";
                await log.SendMessageAsync("", false, embed.Build());
                Extensions.Purging = false;
                message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no inappropriate links.");
                await Task.Delay(5000);
                Extensions.Purging = true;
                await message.DeleteAsync();
                await Task.Delay(100);
                Extensions.Purging = false;
                return true;
            }
            badMessage = EmojiSpam(msg);
            if (badMessage)
            {
                Extensions.Purging = true;
                await msg.DeleteAsync();
                embed.Fields.ElementAt(0).Value = "Too many emojis";
                await log.SendMessageAsync("", false, embed.Build());
                Extensions.Purging = false;
                message = await msg.Channel.SendMessageAsync($"{msg.Author.Mention} no emoji spamming.");
                await Task.Delay(5000);
                Extensions.Purging = true;
                await message.DeleteAsync();
                await Task.Delay(100);
                Extensions.Purging = false;
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
            return false;
        }
    }
}
