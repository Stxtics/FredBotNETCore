﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FredBotNETCore
{
    public static class Extensions
    {
        public static string GetHeapSize()
        {
            return Math.Round(GC.GetTotalMemory(false) / (1024.0 * 1024.0), 2).ToString();
        }

        public static string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "TextFiles");
        public static Random random = new Random();

        public static T GetRandomElement<T>(this IEnumerable<T> list)
        {
            if (list.Count() == 0)
            {
                return default;
            }

            return list.ElementAt(random.Next(list.Count()));
        }

        public static IEnumerable<string> SplitInParts(this string s, int partLength)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (partLength <= 0)
            {
                throw new ArgumentException("Part length has to be positive.", "partLength");
            }

            for (int i = 0; i < s.Length; i += partLength)
            {
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
            }
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        public static string HexConverter(Color c)
        {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static SocketUser UserInGuild(SocketUserMessage message, SocketGuild guild, string username)
        {
            SocketUser user = null;
            if (message != null)
            {
                if (message.MentionedUsers.Count > 0)
                {
                    int argPos = 0;
                    if (message.HasStringPrefix(Guild.Get(guild).Prefix, ref argPos))
                    {
                        user = message.MentionedUsers.First();
                    }
                    else if (message.HasMentionPrefix(guild.CurrentUser, ref argPos) && message.MentionedUsers.Count > 1)
                    {
                        user = message.MentionedUsers.ElementAt(1);
                    }
                }
            }
            if (user == null)
            {
                if (ulong.TryParse(username, out ulong userId))
                {
                    if (guild.Users.Where(x => x.Id == userId).Count() > 0)
                    {
                        user = guild.GetUser(userId);
                    }
                }
            }
            if (user == null)
            {
                string discriminator = null;
                if (username.Contains("#"))
                {
                    discriminator = username.Split("#").Last();
                    username = username.Split("#").First();
                }
                if (discriminator == null)
                {
                    if (guild.Users.Where(x => x.Username.ToUpper() == username.ToUpper()).Count() > 0)
                    {
                        user = guild.Users.Where(x => x.Username.ToUpper() == username.ToUpper()).First();
                    }
                }
                else
                {
                    if (guild.Users.Where(x => x.Username.ToUpper() == username.ToUpper()).Where(x => x.Discriminator == discriminator).Count() > 0)
                    {
                        user = guild.Users.Where(x => x.Username.ToUpper() == username.ToUpper()).Where(x => x.Discriminator == discriminator).First();
                    }
                }
            }
            return user;
        }

        public static SocketRole RoleInGuild(SocketUserMessage message, SocketGuild guild, string roleName)
        {
            SocketRole role = null;
            if (message != null)
            {
                if (message.MentionedRoles.Count > 0)
                {
                    role = message.MentionedRoles.First();
                }
            }
            if (role == null)
            {
                if (ulong.TryParse(roleName, out ulong roleId))
                {
                    if (guild.Roles.Where(x => x.Id == roleId).Count() > 0)
                    {
                        role = guild.GetRole(roleId);
                    }
                }
            }
            if (role == null)
            {
                if (guild.Roles.Where(x => x.Name.ToUpper() == roleName.ToUpper()).Count() > 0)
                {
                    role = guild.Roles.Where(x => x.Name.ToUpper() == roleName.ToUpper()).First();
                }
            }
            return role;
        }

        public static SocketGuildChannel ChannelInGuild(SocketUserMessage message, SocketGuild guild, string channelName)
        {
            SocketGuildChannel channel = null;
            if (message != null)
            {
                if (message.MentionedChannels.Count > 0)
                {
                    channel = message.MentionedChannels.First();
                    return channel;
                }
            }
            if (channel == null)
            {
                if (ulong.TryParse(channelName, out ulong channelId))
                {
                    if (guild.Channels.Where(x => x.Id == channelId).Count() > 0)
                    {
                        channel = guild.GetChannel(channelId);
                    }
                }
            }
            if (channel == null)
            {
                if (guild.Channels.Where(x => x.Name.ToUpper() == channelName.ToUpper()).Count() > 0)
                {
                    channel = guild.Channels.Where(x => x.Name.ToUpper() == channelName.ToUpper()).First();
                }
            }
            return channel;
        }

        public static readonly string appid = new StreamReader(path: Path.Combine(downloadPath, "WeatherAppID.txt")).ReadLine();
        public static async Task<string> GetWeatherAsync(string city)
        {
            HttpClient httpClient = new HttpClient();
            string URL = "http://api.openweathermap.org/data/2.5/weather?q=" + city + "&appid=" + appid;
            HttpResponseMessage response = await httpClient.GetAsync(URL);
            string result = await response.Content.ReadAsStringAsync();
            return result;
        }

        public static SocketTextChannel GetLogChannel(SocketGuild guild)
        {
            SocketTextChannel channel = null;
            long? logChannel = Guild.Get(guild).LogChannel;
            if (logChannel != null)
            {
                try
                {
                    channel = guild.GetTextChannel(ulong.Parse(logChannel.ToString()));
                }
                catch (Discord.Net.HttpException)
                {

                }
            }
            return channel;
        }

        public static SocketTextChannel GetNotificationsChannel(SocketGuild guild)
        {
            SocketTextChannel channel = null;
            long? notificationsChannel = Guild.Get(guild).NotificationsChannel;
            if (notificationsChannel != null)
            {
                try
                {
                    channel = guild.GetTextChannel(ulong.Parse(notificationsChannel.ToString()));
                }
                catch (Discord.Net.HttpException)
                {

                }
            }
            return channel;
        }

        public static SocketTextChannel GetBanLogChannel(SocketGuild guild)
        {
            SocketTextChannel channel = null;
            long? banlogChannel = Guild.Get(guild).BanlogChannel;
            if (banlogChannel != null)
            {
                try
                {
                    channel = guild.GetTextChannel(ulong.Parse(banlogChannel.ToString()));
                }
                catch (Discord.Net.HttpException)
                {

                }
            }
            return channel;
        }

        public static List<ulong> MusicChannels()
        {
            List<ulong> channels = new List<ulong>();
            StreamReader allowedChannels = new StreamReader(path: Path.Combine(downloadPath, "MusicChannels.txt"));
            string channel = allowedChannels.ReadLine();
            while (channel != null)
            {
                channels.Add(ulong.Parse(channel));
                channel = allowedChannels.ReadLine();
            }
            allowedChannels.Close();
            return channels;
        }

        public static async Task LogError(DiscordSocketClient client, string error)
        {
            SocketUser user = client.GetUser(181853112045142016);
            IEnumerable<string> parts = error.SplitInParts(1990);
            foreach (string part in parts)
            {
                await user.SendMessageAsync("```" + part + "```");
            }
        }
    }
}
