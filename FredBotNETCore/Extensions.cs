using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
        public static bool Purging { get; set; } = false;

        public static T GetRandomElement<T>(this IEnumerable<T> list)
        {
            if (list.Count() == 0)
            {
                return default(T);
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

        public static bool IsMuted(ulong userID)
        {
            if (File.ReadAllText(Path.Combine(downloadPath, "MutedUsers.txt")).Contains(userID.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CheckStaff(string userID, IEnumerable<SocketRole> roles)
        {
            string roleID = null;
            if (roles.Count() > 0)
            {
                roleID = roles.First().Id.ToString();
            }
            StreamReader staff = new StreamReader(path: Path.Combine(downloadPath, "DiscordStaff.txt"));
            StreamReader staffRoles = new StreamReader(path: Path.Combine(downloadPath, "DiscordStaffRoles.txt"));
            string line = staff.ReadLine();
            bool isStaff = false;
            while (line != null)
            {
                if (line.Equals(userID))
                {
                    isStaff = true;
                    break;
                }
                line = staff.ReadLine();
            }
            if (!isStaff && roleID != null)
            {
                line = staffRoles.ReadLine();
                while (line != null)
                {
                    if (line.Equals(roleID))
                    {
                        isStaff = true;
                        break;
                    }
                    line = staffRoles.ReadLine();
                }
            }
            staff.Close();
            staffRoles.Close();
            return isStaff;
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
                    if (message.HasStringPrefix("/", ref argPos))
                    {
                        user = message.MentionedUsers.First();
                    }
                    else if (message.HasMentionPrefix(guild.CurrentUser, ref argPos) && message.MentionedUsers.Count > 1)
                    {
                        user = message.MentionedUsers.ElementAt(1);
                    }
                    return user;
                }
            }
            if (ulong.TryParse(username, out ulong userid))
            {
                username = guild.GetUser(userid).Username;
            }
            string discriminator = null;
            if (username.Contains("#"))
            {
                discriminator = username.Split("#").Last();
                username = username.Split("#").First();
            }
            foreach (SocketGuildUser gUser in guild.Users)
            {
                if (discriminator == null)
                {
                    if (gUser.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                    {
                        user = gUser;
                        break;
                    }
                }
                else
                {
                    if (gUser.Username.Equals(username, StringComparison.InvariantCultureIgnoreCase) && gUser.Discriminator.Equals(discriminator))
                    {
                        user = gUser;
                        break;
                    }
                }
            }
            return user;
        }

        public static SocketRole RoleInGuild(SocketUserMessage message, SocketGuild guild, string roleName)
        {
            SocketRole role = null;
            if (message.MentionedRoles.Count > 0)
            {
                role = message.MentionedRoles.First();
                return role;
            }
            if (ulong.TryParse(roleName, out ulong roleId))
            {
                roleName = guild.GetRole(roleId).Name;
            }
            foreach (SocketRole gRole in guild.Roles)
            {
                if (gRole.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase))
                {
                    role = guild.GetRole(gRole.Id);
                    break;
                }
            }
            return role;
        }

        public static SocketGuildChannel ChannelInGuild(SocketUserMessage message, SocketGuild guild, string channelName)
        {
            SocketGuildChannel channel = null;
            if (message.MentionedChannels.Count > 0)
            {
                channel = message.MentionedChannels.First();
                return channel;
            }
            if (ulong.TryParse(channelName, out ulong channelId))
            {
                channelName = guild.GetChannel(channelId).Name;
            }
            foreach (SocketGuildChannel gChannel in guild.Channels)
            {
                if (gChannel.Name.Equals(channelName, StringComparison.InvariantCultureIgnoreCase))
                {
                    channel = guild.GetChannel(gChannel.Id);
                    break;
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

        public static ulong GetLogChannel()
        {
            return ulong.Parse(File.ReadAllText(Path.Combine(downloadPath, "LogChannel.txt")));
        }

        public static List<ulong> AllowedChannels()
        {
            List<ulong> channels = new List<ulong>();
            StreamReader allowedChannels = new StreamReader(path: Path.Combine(downloadPath, "AllowedChannels.txt"));
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
