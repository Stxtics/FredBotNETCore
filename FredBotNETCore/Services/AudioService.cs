using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Victoria;
using Victoria.Entities;
using Victoria.Entities.Enums;

namespace FredBotNETCore.Services
{
    public class AudioService
    {
        private static Lavalink _lavalink;
        private static bool QueueLoop { get; set; } = false;
        private static List<SocketUser> SkippedUsers { get; set; } = new List<SocketUser>();
        private static List<SocketUser> UserQueue { get; set; } = new List<SocketUser>();
        private static SocketUser NowPlayingUser { get; set; } = null;

        private readonly YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = File.ReadAllText(Path.Combine(Extensions.downloadPath, "YoutubeApiKey.txt")),
            ApplicationName = "Fred bot"
        });

        public AudioService(Lavalink lavalink)
        {
            _lavalink = lavalink;
        }

        public async Task Connect(IVoiceChannel voiceChannel, IMessageChannel messageChannel)
        {
            await _lavalink.DefaultNode.ConnectAsync(voiceChannel, messageChannel);
        }

        public static async Task Disconnect(ulong guildId)
        {
            await _lavalink.DefaultNode.DisconnectAsync(guildId);
        }

        public static async Task OnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
        {
            if (reason is TrackReason.LoadFailed || reason is TrackReason.Cleanup || reason is TrackReason.Replaced || reason is TrackReason.Stopped)
            {
                return;
            }
            player.Queue.TryDequeue(out LavaTrack nextTrack);
            if (nextTrack is null && !QueueLoop)
            {
                await player.TextChannel.SendMessageAsync("Queue finished.");
                await Disconnect(player.VoiceChannel.Guild.Id);
                SkippedUsers.Clear();
                UserQueue.Clear();
                NowPlayingUser = null;
            }
            else if (QueueLoop)
            {
                SkippedUsers.Clear();
                if (nextTrack is null)
                {
                    await player.PlayAsync(track);
                    await player.TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(track.Title)}** ({track.Length.Minutes}:{track.Length.Seconds.ToString("D2")}) requested by **{NowPlayingUser.Username}#{NowPlayingUser.Discriminator}**.");
                }
                else
                {
                    player.Queue.Enqueue(track);
                    UserQueue.Add(NowPlayingUser);
                    SocketUser nextUser = UserQueue.FirstOrDefault();
                    UserQueue.RemoveAt(0);
                    NowPlayingUser = nextUser;
                    await player.PlayAsync(nextTrack);
                    await player.TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(nextTrack.Title)}** ({nextTrack.Length.Minutes}:{nextTrack.Length.Seconds.ToString("D2")}) requested by **{nextUser.Username}#{nextUser.Discriminator}**.");
                }
            }
            else
            {
                SkippedUsers.Clear();
                SocketUser nextUser = UserQueue.FirstOrDefault();
                UserQueue.RemoveAt(0);
                NowPlayingUser = nextUser;
                await player.PlayAsync(nextTrack);
                await player.TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(nextTrack.Title)}** ({nextTrack.Length.Minutes}:{nextTrack.Length.Seconds.ToString("D2")}) requested by **{nextUser.Username}#{nextUser.Discriminator}**.");
            }
        }

        public async Task Play(ulong guildId, LavaTrack track)
        {
            LavaPlayer player = _lavalink.DefaultNode.GetPlayer(guildId);
            await player.PlayAsync(track);
        }

        public async Task<LavaTrack> GetTrackFromYoutube(string searchQuery)
        {
            LavaResult search = await _lavalink.DefaultNode.SearchYouTubeAsync(searchQuery);
            if (search.LoadResultType == LoadResultType.NoMatches)
            {
                return null;
            }
            return search.Tracks.FirstOrDefault();
        }

        public async Task Pause(ulong guildId)
        {
            await _lavalink.DefaultNode.GetPlayer(guildId).PauseAsync();
        }

        public async Task Resume(ulong guildId)
        {
            await _lavalink.DefaultNode.GetPlayer(guildId).PauseAsync();
        }

        public async Task SetVolume(ulong guildId, int volume)
        {
            await _lavalink.DefaultNode.GetPlayer(guildId).SetVolumeAsync(volume);
        }

        public async Task Skip(ulong guildId)
        {
            if (_lavalink.DefaultNode.GetPlayer(guildId).Queue.Count == 0)
            {
                await _lavalink.DefaultNode.GetPlayer(guildId).TextChannel.SendMessageAsync("Queue finished.");
                await Stop(guildId);
            }
            else
            {
                LavaTrack nextTrack = _lavalink.DefaultNode.GetPlayer(guildId).Queue.Peek();
                await _lavalink.DefaultNode.GetPlayer(guildId).SkipAsync();
                SocketUser user = UserQueue.FirstOrDefault();
                UserQueue.RemoveAt(0);
                SkippedUsers.Clear();
                if (UserQueue.Count == 0)
                {
                    NowPlayingUser = user;
                }
                else
                {
                    SocketUser nextUser = UserQueue.FirstOrDefault();
                    NowPlayingUser = nextUser;
                }
                await _lavalink.DefaultNode.GetPlayer(guildId).TextChannel.SendMessageAsync($"Now playing: **{Format.Sanitize(nextTrack.Title)}** ({nextTrack.Length.Minutes}:{nextTrack.Length.Seconds.ToString("D2")}) requested by **{NowPlayingUser.Username}#{NowPlayingUser.Discriminator}**.");
            }
        }

        public List<SocketUser> GetSkippedUsers()
        {
            return SkippedUsers;
        }

        public void AddSkippedUser(SocketUser user)
        {
            SkippedUsers.Add(user);
        }

        public LavaTrack NowPlaying(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).CurrentTrack;
        }

        public async Task Stop(ulong guildId)
        {
            LavaPlayer player = _lavalink.DefaultNode.GetPlayer(guildId);
            if (player.IsPlaying)
            {
                await player.StopAsync();
            }
            await Disconnect(guildId);
            UserQueue.Clear();
            SkippedUsers.Clear();
            NowPlayingUser = null;
        }

        public bool Paused(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).IsPaused;
        }

        public bool Playing(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).IsPlaying;
        }

        public int GetVolume(ulong guildId)
        {
            return _lavalink.DefaultNode.GetPlayer(guildId).Volume;
        }

        public Tuple<LavaQueue<LavaTrack>, List<SocketUser>> Queue(ulong guildId)
        {
            if (_lavalink.DefaultNode.GetPlayer(guildId).Queue != null)
            {
                return Tuple.Create(_lavalink.DefaultNode.GetPlayer(guildId).Queue, UserQueue);
            }
            return null;
        }

        public void QueueAdd(ulong guildId, LavaTrack track, SocketUser user)
        {
            _lavalink.DefaultNode.GetPlayer(guildId).Queue.Enqueue(track);
            UserQueue.Add(user);
        }

        public Tuple<LavaTrack, SocketUser> QueueRemove(ulong guildId, int index)
        {
            SocketUser user = UserQueue[index];
            UserQueue.RemoveAt(index);
            return Tuple.Create(_lavalink.DefaultNode.GetPlayer(guildId).Queue.RemoveAt(index), user);
        }

        public void QueueClear(ulong guildId)
        {
            _lavalink.DefaultNode.GetPlayer(guildId).Queue.Clear();
            UserQueue.Clear();
        }

        public void SetNowPlayingUser(SocketUser user)
        {
            NowPlayingUser = user;
        }

        public SocketUser GetNowPlayingUser()
        {
            return NowPlayingUser;
        }

        public bool Loop()
        {
            if (QueueLoop)
            {
                QueueLoop = false;
            }
            else
            {
                QueueLoop = true;
            }
            return QueueLoop;
        }

        public async Task Seek(ulong guildId, TimeSpan time)
        {
            await _lavalink.DefaultNode.GetPlayer(guildId).SeekAsync(time);
        }

        private static bool Blacklisted(SocketUser user)
        {
            if (File.ReadAllText(path: Path.Combine(Extensions.downloadPath, "BlacklistedMusic.txt")).Contains(user.Id.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task AddAsync(SocketCommandContext context, [Remainder] string url)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(url))
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Title = "Command: /add",
                        Description = "**Description:** Add a song to the music queue.\n**Usage:** /add [url]\n**Example:** /add https://www.youtube.com/watch?v=ifFNeqzB5os",
                        Color = new Color(220, 220, 220)
                    };
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                    if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                    }
                    else
                    {
                        bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                                  && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
                        if (context.Guild.CurrentUser.VoiceChannel == null)
                        {
                            await Connect((context.User as SocketGuildUser).VoiceChannel, context.Channel);
                        }
                        if (Queue(context.Guild.Id) != null && Queue(context.Guild.Id).Item1.Items.Count() >= 20)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the queue is full.");
                        }
                        else if (result && (url.Contains("youtube.com/") || url.Contains("youtu.be/")))
                        {
                            WebClient myDownloader = new WebClient
                            {
                                Encoding = System.Text.Encoding.UTF8
                            };
                            string id = Extensions.GetBetween(url, "youtube.com/watch?v=", "&");
                            if (id == null || id.Length <= 0)
                            {
                                if (url.Contains("?"))
                                {
                                    id = Extensions.GetBetween(url, "youtu.be/", "?");
                                }
                                else
                                {
                                    try
                                    {
                                        id = url.Split(".be/").Last();
                                    }
                                    catch (Exception)
                                    {
                                        //no id in url
                                    }
                                }
                            }
                            if (id == null || id.Length <= 0)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find a video ID in that URL.");
                                return;
                            }
                            string jsonResponse = myDownloader.DownloadString(
                            "https://www.googleapis.com/youtube/v3/videos?part=snippet&id=" + id + "&key="
                            + File.ReadAllText(Path.Combine(Extensions.downloadPath, "YoutubeApiKey.txt")));
                            string title = Extensions.GetBetween(jsonResponse, "\"title\": \"", "\",");
                            LavaTrack track = await GetTrackFromYoutube(title);
                            if (track == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find a song with that URL.");
                            }
                            else if (track.Length.Minutes > 10 || (track.Length.Minutes == 10 && track.Length.Seconds > 0))
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} the maximum song length is 10 minutes.");
                            }
                            else if (Queue(context.Guild.Id).Item1.Items.Any(x => x.Uri.Equals(track.Uri)))
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} that video is already in the queue.");
                            }
                            else
                            {
                                SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
                                searchListRequest.Q = title;
                                searchListRequest.MaxResults = 1;
                                searchListRequest.Type = "video";
                                SearchListResponse searchListResponse = await searchListRequest.ExecuteAsync();
                                string channel = "";
                                ThumbnailDetails thumbnails = null;
                                foreach (Google.Apis.YouTube.v3.Data.SearchResult searchResult in searchListResponse.Items)
                                {
                                    switch (searchResult.Id.Kind)
                                    {
                                        case "youtube#video":
                                            channel = searchResult.Snippet.ChannelTitle;
                                            thumbnails = searchResult.Snippet.Thumbnails;
                                            break;
                                    }
                                }
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                    Author = new EmbedAuthorBuilder()
                                    {
                                        Name = "Add song",
                                        Url = track.Uri.ToString()
                                    },
                                    Fields = new List<EmbedFieldBuilder>
                                        {
                                            new EmbedFieldBuilder
                                            {
                                                Name = "Song",
                                                Value = Format.Sanitize(title),
                                                IsInline = false
                                            },
                                            new EmbedFieldBuilder
                                            {
                                                Name = "Duration",
                                                Value = track.Length.Minutes + ":" + track.Length.Seconds.ToString("D2"),
                                                IsInline = false
                                            },
                                            new EmbedFieldBuilder
                                            {
                                                Name = "Channel",
                                                Value = Format.Sanitize(channel),
                                                IsInline = false
                                            }
                                        },
                                    Footer = new EmbedFooterBuilder()
                                    {
                                        IconUrl = context.User.GetAvatarUrl(),
                                        Text = $"Queued by {context.User.Username}#{context.User.Discriminator}"
                                    },
                                    ThumbnailUrl = thumbnails.High.Url
                                };
                                embed.WithCurrentTimestamp();
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                                if (NowPlaying(context.Guild.Id) != null && Playing(context.Guild.Id) || Paused(context.Guild.Id))
                                {
                                    QueueAdd(context.Guild.Id, track, context.User);
                                }
                                else
                                {
                                    await Play(context.Guild.Id, track);
                                    SetNowPlayingUser(context.User);
                                    await context.Channel.SendMessageAsync($"Now playing: **{Format.Sanitize(track.Title)}** ({track.Length.Minutes}:{track.Length.Seconds.ToString("D2")}) requested by **{Format.Sanitize(context.User.Username)}#{context.User.Discriminator}**.");
                                }
                            }
                        }
                        else if (result)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} that URL is not supported. Use a youtube URL or search for the song.");
                        }
                        else
                        {
                            SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
                            searchListRequest.Q = url;
                            searchListRequest.MaxResults = 1;
                            searchListRequest.Type = "video";
                            SearchListResponse searchListResponse = await searchListRequest.ExecuteAsync();
                            string channel = "";
                            ThumbnailDetails thumbnails = null;
                            foreach (Google.Apis.YouTube.v3.Data.SearchResult searchResult in searchListResponse.Items)
                            {
                                switch (searchResult.Id.Kind)
                                {
                                    case "youtube#video":
                                        channel = searchResult.Snippet.ChannelTitle;
                                        thumbnails = searchResult.Snippet.Thumbnails;
                                        break;
                                }
                            }
                            LavaTrack track = await GetTrackFromYoutube(url);
                            if (track == null)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find a song with that title.");
                            }
                            else if (track.Length.Minutes > 10 || (track.Length.Minutes == 10 && track.Length.Seconds > 0))
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} the maximum song length is 10 minutes.");
                            }
                            else if (Queue(context.Guild.Id).Item1.Items.Any(x => x.Uri.Equals(track.Uri)))
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} that video is already in the queue.");
                            }
                            else
                            {
                                EmbedBuilder embed = new EmbedBuilder()
                                {
                                    Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                    Author = new EmbedAuthorBuilder()
                                    {
                                        Name = "Add song",
                                        Url = track.Uri.ToString()
                                    },
                                    Fields = new List<EmbedFieldBuilder>
                                        {
                                            new EmbedFieldBuilder
                                            {
                                                Name = "Song",
                                                Value = Format.Sanitize(track.Title),
                                                IsInline = false
                                            },
                                            new EmbedFieldBuilder
                                            {
                                                Name = "Duration",
                                                Value = track.Length.Minutes + ":" + track.Length.Seconds.ToString("D2"),
                                                IsInline = false
                                            },
                                            new EmbedFieldBuilder
                                            {
                                                Name = "Channel",
                                                Value = Format.Sanitize(channel),
                                                IsInline = false
                                            }
                                        },
                                    Footer = new EmbedFooterBuilder()
                                    {
                                        IconUrl = context.User.GetAvatarUrl(),
                                        Text = $"Queued by {context.User.Username}#{context.User.Discriminator}"
                                    },
                                    ThumbnailUrl = thumbnails.High.Url
                                };
                                embed.WithCurrentTimestamp();
                                await context.Channel.SendMessageAsync("", false, embed.Build());
                                if (NowPlaying(context.Guild.Id) != null && Playing(context.Guild.Id) || Paused(context.Guild.Id))
                                {
                                    QueueAdd(context.Guild.Id, track, context.User);
                                }
                                else
                                {
                                    await Play(context.Guild.Id, track);
                                    SetNowPlayingUser(context.User);
                                    await context.Channel.SendMessageAsync($"Now playing: **{Format.Sanitize(track.Title)}** ({track.Length.Minutes}:{track.Length.Seconds.ToString("D2")}) requested by **{Format.Sanitize(context.User.Username)}#{context.User.Discriminator}**.");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ShowQueueAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel != null)
                    {
                        Tuple<LavaQueue<LavaTrack>, List<SocketUser>> queue = Queue(context.Guild.Id);
                        if (queue.Item1.Items.Count() <= 0)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the queue is currently empty.");
                        }
                        else
                        {
                            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                            {
                                IconUrl = context.Guild.IconUrl,
                                Name = "Queue"
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"{context.User.Username}#{context.User.Discriminator}({context.User.Id})",
                                IconUrl = context.User.GetAvatarUrl()
                            };
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                Author = auth,
                                Footer = footer
                            };
                            embed.WithCurrentTimestamp();
                            int count = 1;
                            int index = 0;
                            foreach (LavaTrack track in queue.Item1.Items)
                            {
                                embed.Description += $"{count.ToString()}. **{Format.Sanitize(track.Title)}** ({track.Length.Minutes}:{track.Length.Seconds.ToString("D2")}) queued by **{Format.Sanitize(queue.Item2[index].Username)}#{queue.Item2[index].Discriminator}**.\n";
                                count++;
                                index++;
                            }
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} nothing is playing right now. Use /play to play a song.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task QueueLoopsAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Loop())
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the queue will now loop.");
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the queue will no longer loop.");
                    }
                }
            }
        }

        public async Task PauseMusicAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel != null && NowPlaying(context.Guild.Id) != null)
                    {
                        if (Paused(context.Guild.Id))
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the music is already paused.");
                        }
                        else
                        {
                            await Pause(context.Guild.Id);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} paused the music.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing to pause.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ResumeAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel != null && NowPlaying(context.Guild.Id) != null)
                    {
                        if (Paused(context.Guild.Id))
                        {
                            await Resume(context.Guild.Id);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} resumed the music.");
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the music is already playing.");
                        }
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing to resume.");
                    }
                }
            }
        }

        public async Task PlayAsync(SocketCommandContext context, [Remainder] string url)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                if (url == null)
                {
                    SocketGuildUser user = context.User as SocketGuildUser;
                    if (user.Roles.Any(e => e.Name.ToUpperInvariant() == "Discord Staff".ToUpperInvariant()) && Paused(context.Guild.Id))
                    {
                        await Resume(context.Guild.Id);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} resumed the music.");
                    }
                    else
                    {
                        await AddAsync(context, url);
                    }
                }
                else
                {
                    await AddAsync(context, url);
                }
            }
            else
            {
                return;
            }
        }

        public async Task QueueRemoveAsync(SocketCommandContext context, string position)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                if (string.IsNullOrWhiteSpace(position) || !int.TryParse(position, out int pos) || pos < 1)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                    {
                        Title = "Command: /qremove",
                        Description = "**Description:** Remove a song from the queue.\n**Usage:** /qremove [position]\n**Example:** /qremove 1",
                        Color = new Color(220, 220, 220)
                    };
                    await context.Channel.SendMessageAsync("", false, embed.Build());
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || Queue(context.Guild.Id).Item1.Items.Count() <= 0)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing in the queue.");
                    }
                    else if (Queue(context.Guild.Id).Item1.Items.Count() < pos)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is not that many items in the queue.");
                    }
                    else
                    {
                        Tuple<LavaTrack, SocketUser> removedTrack = QueueRemove(context.Guild.Id, pos - 1);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} removed **{Format.Sanitize(removedTrack.Item1.Title)}** queued by **{Format.Sanitize(removedTrack.Item2.Username)}#{removedTrack.Item2.Discriminator}** from the queue.");

                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task QueueClearAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || Queue(context.Guild.Id).Item1.Items.Count() <= 0)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the queue is already empty.");
                    }
                    else
                    {
                        QueueClear(context.Guild.Id);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} cleared the queue.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ComeAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != null && context.Guild.CurrentUser.VoiceChannel.Id == 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I am already in the Music voice channel.");
                }
                else
                {
                    await Connect(_voiceChannel, context.Channel);
                    await context.Channel.SendMessageAsync($"Joined voice channel **{Format.Sanitize(_voiceChannel.Name)}**.");
                }
            }
            else
            {
                return;
            }
        }

        public async Task SkipSongAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild.Id) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing to skip.");
                    }
                    else if (GetSkippedUsers().Contains(context.User))
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have already voted to skip this song.");
                    }
                    else
                    {
                        int users = _voiceChannel.Users.Count();
                        int votesNeeded = Convert.ToInt32(Math.Ceiling((double)users / 3));
                        if (votesNeeded < 1)
                        {
                            votesNeeded = 1;
                        }
                        AddSkippedUser(context.User);
                        if (votesNeeded - GetSkippedUsers().Count == 0)
                        {
                            SocketUser user = GetNowPlayingUser();
                            LavaTrack nowPlaying = NowPlaying(context.Guild.Id);
                            await context.Channel.SendMessageAsync($"**{Format.Sanitize(nowPlaying.Title)}** requested by **{Format.Sanitize(user.Username)}#{user.Discriminator}** was skipped.");
                            await Skip(context.Guild.Id);
                        }
                        else
                        {
                            if (votesNeeded - GetSkippedUsers().Count == 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} voted to skip **{Format.Sanitize(NowPlaying(context.Guild.Id).Title)}**. {votesNeeded - GetSkippedUsers().Count} more vote needed to skip.");
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} voted to skip **{Format.Sanitize(NowPlaying(context.Guild.Id).Title)}**. {votesNeeded - GetSkippedUsers().Count} more votes needed to skip.");
                            }
                        }
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task ForceSkipAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild.Id) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing to skip.");
                    }
                    else
                    {
                        LavaTrack nowPlaying = NowPlaying(context.Guild.Id);
                        SocketUser user = GetNowPlayingUser();
                        await context.Channel.SendMessageAsync($"{context.User.Mention} force skipped **{Format.Sanitize(nowPlaying.Title)}** requested by **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                        await Skip(context.Guild.Id);
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task NPAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild.Id) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} nothing is playing right now.");
                    }
                    else
                    {
                        LavaTrack nowPlaying = NowPlaying(context.Guild.Id);
                        SocketUser playingUser = GetNowPlayingUser();
                        SearchResource.ListRequest searchListRequest = youtubeService.Search.List("snippet");
                        searchListRequest.Q = nowPlaying.Title;
                        searchListRequest.MaxResults = 1;
                        searchListRequest.Type = "video";
                        SearchListResponse searchListResponse = await searchListRequest.ExecuteAsync();
                        string channel = "";
                        ThumbnailDetails thumbnails = null;
                        foreach (Google.Apis.YouTube.v3.Data.SearchResult searchResult in searchListResponse.Items)
                        {
                            switch (searchResult.Id.Kind)
                            {
                                case "youtube#video":
                                    channel = searchResult.Snippet.ChannelTitle;
                                    thumbnails = searchResult.Snippet.Thumbnails;
                                    break;
                            }
                        }
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            Author = new EmbedAuthorBuilder()
                            {
                                Name = "Now playing",
                                Url = nowPlaying.Uri.ToString()
                            },
                            Fields = new List<EmbedFieldBuilder>
                                {
                                new EmbedFieldBuilder
                                {
                                    Name = "Song",
                                    Value = Format.Sanitize(nowPlaying.Title),
                                    IsInline = false
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Duration",
                                    Value = $"{nowPlaying.Length.Minutes}:{nowPlaying.Length.Seconds.ToString("D2")}",
                                    IsInline = false
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Channel",
                                    Value = Format.Sanitize(channel),
                                    IsInline = false
                                }
                                },
                            Footer = new EmbedFooterBuilder()
                            {
                                IconUrl = context.User.GetAvatarUrl(),
                                Text = $"Queued by {playingUser.Username}#{playingUser.Discriminator}"
                            },
                            ThumbnailUrl = thumbnails.High.Url
                        };
                        embed.WithCurrentTimestamp();
                        await context.Channel.SendMessageAsync("", false, embed.Build());
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task StopAsync(SocketCommandContext context)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild.Id) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is no music playing.");
                    }
                    else
                    {
                        await Stop(context.Guild.Id);
                        await context.Channel.SendMessageAsync($"The music was successfully stopped by {context.User.Mention} .");
                    }
                }
            }
            else
            {
                return;
            }
        }

        public async Task VolumeAsync(SocketCommandContext context, int volume)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild.Id) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is no music to set the volume of.");
                    }
                    else
                    {
                        if (volume >= 150 || volume <= 0)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} volume must be between 0 and 150.");
                        }
                        else
                        {
                            int currentVolume = GetVolume(context.Guild.Id);
                            await SetVolume(context.Guild.Id, volume);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} set the volume from **{currentVolume}** to **{volume}**.");
                        }
                    }
                }
            }
        }

        public async Task SeekAsync(SocketCommandContext context, string seconds)
        {
            if (context.Channel.Id == 528696379325808655 || context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild.Id) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} nothing is playing right now.");
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(seconds) || !int.TryParse(seconds, out int time) || time < 0)
                        {
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Title = "Command: /seek",
                                Description = "**Description:** Goto point in song.\n**Usage:** /seek [seconds]\n**Example:** /seek 60",
                                Color = new Color(220, 220, 220)
                            };
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                        }
                        else
                        {
                            LavaTrack nowPlaying = NowPlaying(context.Guild.Id);
                            if (nowPlaying.Length.TotalSeconds < time)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} that point exceeds the songs length.");
                            }
                            else
                            {
                                TimeSpan timeSpan = new TimeSpan(0, 0, time);
                                await Seek(context.Guild.Id, timeSpan);
                                await context.Channel.SendMessageAsync($"{context.User.Mention} successfully went to **{timeSpan.Minutes}:{timeSpan.Seconds.ToString("D2")}** in the current song.");
                            }
                        }
                    }
                }
            }
        }
    }
}
