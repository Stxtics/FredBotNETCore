using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Services;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Victoria.Entities;

namespace FredBotNETCore.Modules.Public
{
    [Name("Audio")]
    [Summary("Module containing all of the music commands.")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApiKey = new StreamReader(path: Path.Combine(Extensions.downloadPath, "YoutubeApiKey.txt")).ReadLine(),
            ApplicationName = "Fred bot"
        });
        private readonly AudioService audioService = new AudioService(CommandHandler._lavaLink);

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

        [Command("add", RunMode = RunMode.Async)]
        [Alias("addsong")]
        [Summary("Adds a song to play.")]
        [RequireContext(ContextType.Guild)]
        public async Task Add([Remainder] string url = null)
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
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
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                    if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                    {
                        await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                    }
                    else
                    {
                        bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                                  && (uriResult.Scheme == "http" || uriResult.Scheme == "https");
                        if (Context.Guild.CurrentUser.VoiceChannel == null)
                        {
                            await audioService.Connect((Context.User as SocketGuildUser).VoiceChannel, Context.Channel);
                        }
                        if (audioService.Queue(Context.Guild.Id) != null && audioService.Queue(Context.Guild.Id).Item1.Items.Count() >= 20)
                        {
                            await ReplyAsync($"{Context.User.Mention} the queue is full.");
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
                                await ReplyAsync($"{Context.User.Mention} I could not find a video ID in that URL.");
                                return;
                            }
                            string jsonResponse = myDownloader.DownloadString(
                            "https://www.googleapis.com/youtube/v3/videos?part=snippet&id=" + id + "&key="
                            + File.ReadAllText(Path.Combine(Extensions.downloadPath, "YoutubeApiKey.txt")));
                            string title = Extensions.GetBetween(jsonResponse, "\"title\": \"", "\",");
                            LavaTrack track = await audioService.GetTrackFromYoutube(title);
                            if (track == null)
                            {
                                await ReplyAsync($"{Context.User.Mention} I could not find a song with that URL.");
                            }
                            else if (track.Length.Minutes > 10 || (track.Length.Minutes == 10 && track.Length.Seconds > 0))
                            {
                                await ReplyAsync($"{Context.User.Mention} the maximum song length is 10 minutes.");
                            }
                            else if (audioService.Queue(Context.Guild.Id).Item1.Items.Any(x => x.Uri.Equals(track.Uri)))
                            {
                                await ReplyAsync($"{Context.User.Mention} that video is already in the queue.");
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
                                        IconUrl = Context.User.GetAvatarUrl(),
                                        Text = $"Queued by {Context.User.Username}#{Context.User.Discriminator}"
                                    },
                                    ThumbnailUrl = thumbnails.High.Url
                                };
                                embed.WithCurrentTimestamp();
                                await ReplyAsync("", false, embed.Build());
                                if (audioService.NowPlaying(Context.Guild.Id) != null && audioService.Playing(Context.Guild.Id) || audioService.Paused(Context.Guild.Id))
                                {
                                    audioService.QueueAdd(Context.Guild.Id, track, Context.User);
                                }
                                else
                                {
                                    await audioService.Play(Context.Guild.Id, track);
                                    audioService.SetNowPlayingUser(Context.User);
                                    await ReplyAsync($"Now playing: **{Format.Sanitize(track.Title)}** ({track.Length.Minutes}:{track.Length.Seconds.ToString("D2")}) requested by **{Format.Sanitize(Context.User.Username)}#{Context.User.Discriminator}**.");
                                }
                            }
                        }
                        else if (result)
                        {
                            await ReplyAsync($"{Context.User.Mention} that URL is not supported. Use a youtube URL or search for the song.");
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
                            LavaTrack track = await audioService.GetTrackFromYoutube(url);
                            if (track == null)
                            {
                                await ReplyAsync($"{Context.User.Mention} I could not find a song with that title.");
                            }
                            else if (track.Length.Minutes > 10 || (track.Length.Minutes == 10 && track.Length.Seconds > 0))
                            {
                                await ReplyAsync($"{Context.User.Mention} the maximum song length is 10 minutes.");
                            }
                            else if (audioService.Queue(Context.Guild.Id).Item1.Items.Any(x => x.Uri.Equals(track.Uri)))
                            {
                                await ReplyAsync($"{Context.User.Mention} that video is already in the queue.");
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
                                        IconUrl = Context.User.GetAvatarUrl(),
                                        Text = $"Queued by {Context.User.Username}#{Context.User.Discriminator}"
                                    },
                                    ThumbnailUrl = thumbnails.High.Url
                                };
                                embed.WithCurrentTimestamp();
                                await ReplyAsync("", false, embed.Build());
                                if (audioService.NowPlaying(Context.Guild.Id) != null && audioService.Playing(Context.Guild.Id) || audioService.Paused(Context.Guild.Id))
                                {
                                    audioService.QueueAdd(Context.Guild.Id, track, Context.User);
                                }
                                else
                                {
                                    await audioService.Play(Context.Guild.Id, track);
                                    audioService.SetNowPlayingUser(Context.User);
                                    await ReplyAsync($"Now playing: **{Format.Sanitize(track.Title)}** ({track.Length.Minutes}:{track.Length.Seconds.ToString("D2")}) requested by **{Format.Sanitize(Context.User.Username)}#{Context.User.Discriminator}**.");
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

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("Displays song queue.")]
        [RequireContext(ContextType.Guild)]
        public async Task ShowQueue()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel != null)
                    {
                        Tuple<LavaQueue<LavaTrack>, List<SocketUser>> queue = audioService.Queue(Context.Guild.Id);
                        if (queue.Item1.Items.Count() <= 0)
                        {
                            await ReplyAsync($"{Context.User.Mention} the queue is currently empty.");
                        }
                        else
                        {
                            EmbedAuthorBuilder auth = new EmbedAuthorBuilder()
                            {
                                IconUrl = Context.Guild.IconUrl,
                                Name = "Queue"
                            };
                            EmbedFooterBuilder footer = new EmbedFooterBuilder()
                            {
                                Text = $"{Context.User.Username}#{Context.User.Discriminator}({Context.User.Id})",
                                IconUrl = Context.User.GetAvatarUrl()
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
                            await ReplyAsync("", false, embed.Build());
                        }
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} nothing is playing right now. Use /play to play a song.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("loop", RunMode = RunMode.Async)]
        [Alias("repeat", "queueloop", "loopqueue", "qloop", "loopq")]
        [Summary("Toggles looping of the queue.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task QueueLoop()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (audioService.Loop())
                    {
                        await ReplyAsync($"{Context.User.Mention} the queue will now loop.");
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} the queue will no longer loop.");
                    }
                }
            }
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Alias("p", "pausemusic")]
        [Summary("pauses the music")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task PauseMusic()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel != null && audioService.NowPlaying(Context.Guild.Id) != null)
                    {
                        if (audioService.Paused(Context.Guild.Id))
                        {
                            await ReplyAsync($"{Context.User.Mention} the music is already paused.");
                        }
                        else
                        {
                            await audioService.Pause(Context.Guild.Id);
                            await ReplyAsync($"{Context.User.Mention} paused the music.");
                        }
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} there is nothing to pause.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Summary("Resumes play of music or adds another song.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Resume()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel != null && audioService.NowPlaying(Context.Guild.Id) != null)
                    {
                        if (audioService.Paused(Context.Guild.Id))
                        {
                            await audioService.Resume(Context.Guild.Id);
                            await ReplyAsync($"{Context.User.Mention} resumed the music.");
                        }
                        else
                        {
                            await ReplyAsync($"{Context.User.Mention} the music is already playing.");
                        }
                    }
                    else
                    {
                        await ReplyAsync($"{Context.User.Mention} there is nothing to resume.");
                    }
                }
            }
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("playmusic")]
        [Summary("Resumes play of music or adds another song.")]
        [RequireContext(ContextType.Guild)]
        public async Task Play([Remainder] string url = null)
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                if (url == null)
                {
                    SocketGuildUser user = Context.User as SocketGuildUser;
                    if (user.Roles.Any(e => e.Name.ToUpperInvariant() == "Discord Staff".ToUpperInvariant()) && audioService.Paused(Context.Guild.Id))
                    {
                        await audioService.Resume(Context.Guild.Id);
                        await ReplyAsync($"{Context.User.Mention} resumed the music.");
                    }
                    else
                    {
                        await Add(url);
                    }
                }
                else
                {
                    await Add(url);
                }
            }
            else
            {
                return;
            }
        }

        [Command("qremove", RunMode = RunMode.Async)]
        [Alias("queueremove")]
        [Summary("Remove an item from the queue.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task QueueRemove(string position = null)
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
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
                    await ReplyAsync("", false, embed.Build());
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.Queue(Context.Guild.Id).Item1.Items.Count() <= 0)
                    {
                        await ReplyAsync($"{Context.User.Mention} there is nothing in the queue.");
                    }
                    else if (audioService.Queue(Context.Guild.Id).Item1.Items.Count() < pos)
                    {
                        await ReplyAsync($"{Context.User.Mention} there is not that many items in the queue.");
                    }
                    else
                    {
                        Tuple<LavaTrack, SocketUser> removedTrack = audioService.QueueRemove(Context.Guild.Id, pos - 1);
                        await ReplyAsync($"{Context.User.Mention} removed **{Format.Sanitize(removedTrack.Item1.Title)}** queued by **{Format.Sanitize(removedTrack.Item2.Username)}#{removedTrack.Item2.Discriminator}** from the queue.");

                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("qclear", RunMode = RunMode.Async)]
        [Alias("clearqueue", "clearq")]
        [Summary("Removes all songs from the queue")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task QueueClear()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.Queue(Context.Guild.Id).Item1.Items.Count() <= 0)
                    {
                        await ReplyAsync($"{Context.User.Mention} the queue is already empty.");
                    }
                    else
                    {
                        audioService.QueueClear(Context.Guild.Id);
                        await ReplyAsync($"{Context.User.Mention} cleared the queue.");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("come", RunMode = RunMode.Async)]
        [Alias("summon", "join")]
        [Summary("Brings bot to voice channel")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Come()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (Context.Guild.CurrentUser.VoiceChannel != null && Context.Guild.CurrentUser.VoiceChannel.Id == 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} I am already in the Music voice channel.");
                }
                else
                {
                    await audioService.Connect(_voiceChannel, Context.Channel);
                    await ReplyAsync($"Joined voice channel **{Format.Sanitize(_voiceChannel.Name)}**.");
                }
            }
            else
            {
                return;
            }
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Alias("skipsong")]
        [Summary("Votes to skip current song")]
        [RequireContext(ContextType.Guild)]
        public async Task SkipSong()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.NowPlaying(Context.Guild.Id) == null)
                    {
                        await ReplyAsync($"{Context.User.Mention} there is nothing to skip.");
                    }
                    else if (audioService.GetSkippedUsers().Contains(Context.User))
                    {
                        await ReplyAsync($"{Context.User.Mention} you have already voted to skip this song.");
                    }
                    else
                    {
                        int users = _voiceChannel.Users.Count();
                        int votesNeeded = Convert.ToInt32(Math.Ceiling((double)users / 3));
                        if (votesNeeded < 1)
                        {
                            votesNeeded = 1;
                        }
                        audioService.AddSkippedUser(Context.User);
                        if (votesNeeded - audioService.GetSkippedUsers().Count == 0)
                        {
                            SocketUser user = audioService.GetNowPlayingUser();
                            LavaTrack nowPlaying = audioService.NowPlaying(Context.Guild.Id);
                            await ReplyAsync($"**{Format.Sanitize(nowPlaying.Title)}** requested by **{Format.Sanitize(user.Username)}#{user.Discriminator}** was skipped.");
                            await audioService.Skip(Context.Guild.Id);
                        }
                        else
                        {
                            if (votesNeeded - audioService.GetSkippedUsers().Count == 1)
                            {
                                await ReplyAsync($"{Context.User.Mention} voted to skip **{Format.Sanitize(audioService.NowPlaying(Context.Guild.Id).Title)}**. {votesNeeded - audioService.GetSkippedUsers().Count} more vote needed to skip.");
                            }
                            else
                            {
                                await ReplyAsync($"{Context.User.Mention} voted to skip **{Format.Sanitize(audioService.NowPlaying(Context.Guild.Id).Title)}**. {votesNeeded - audioService.GetSkippedUsers().Count} more votes needed to skip.");
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

        [Command("forceskip", RunMode = RunMode.Async)]
        [Alias("fskip", "forceskipsong")]
        [Summary("Skips the current song.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ForceSkip()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.NowPlaying(Context.Guild.Id) == null)
                    {
                        await ReplyAsync($"{Context.User.Mention} there is nothing to skip.");
                    }
                    else
                    {
                        LavaTrack nowPlaying = audioService.NowPlaying(Context.Guild.Id);
                        SocketUser user = audioService.GetNowPlayingUser();
                        await ReplyAsync($"{Context.User.Mention} force skipped **{Format.Sanitize(nowPlaying.Title)}** requested by **{Format.Sanitize(user.Username)}#{user.Discriminator}**.");
                        await audioService.Skip(Context.Guild.Id);
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("np", RunMode = RunMode.Async)]
        [Alias("nowplaying")]
        [Summary("Displays current song playing.")]
        [RequireContext(ContextType.Guild)]
        public async Task NP()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.NowPlaying(Context.Guild.Id) == null)
                    {
                        await ReplyAsync($"{Context.User.Mention} nothing is playing right now.");
                    }
                    else
                    {
                        LavaTrack nowPlaying = audioService.NowPlaying(Context.Guild.Id);
                        SocketUser playingUser = audioService.GetNowPlayingUser();
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
                                IconUrl = Context.User.GetAvatarUrl(),
                                Text = $"Queued by {playingUser.Username}#{playingUser.Discriminator}"
                            },
                            ThumbnailUrl = thumbnails.High.Url
                        };
                        embed.WithCurrentTimestamp();
                        await ReplyAsync("", false, embed.Build());
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Alias("stopmusic")]
        [Summary("Stops the music and makes bot leave voice channel.")]
        [RequireOwner]
        public async Task Stop()
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.NowPlaying(Context.Guild.Id) == null)
                    {
                        await ReplyAsync($"{Context.User.Mention} there is no music playing.");
                    }
                    else
                    {
                        await audioService.Stop(Context.Guild.Id);
                        await ReplyAsync($"The music was successfully stopped by {Context.User.Mention} .");
                    }
                }
            }
            else
            {
                return;
            }
        }

        [Command("setvolume", RunMode = RunMode.Async)]
        [Alias("volume")]
        [Summary("Sets volume of the music")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Volume(int volume)
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.NowPlaying(Context.Guild.Id) == null)
                    {
                        await ReplyAsync($"{Context.User.Mention} there is no music to set the volume of.");
                    }
                    else
                    {
                        if (volume >= 150 || volume <= 0)
                        {
                            await ReplyAsync($"{Context.User.Mention} volume must be between 0 and 150.");
                        }
                        else
                        {
                            int currentVolume = audioService.GetVolume(Context.Guild.Id);
                            await audioService.SetVolume(Context.Guild.Id, volume);
                            await ReplyAsync($"{Context.User.Mention} set the volume from **{currentVolume}** to **{volume}**.");
                        }
                    }
                }
            }
        }

        [Command("seek", RunMode = RunMode.Async)]
        [Alias("goto")]
        [Summary("Goes to a certain point in a song.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Seek(string seconds = null)
        {
            if (Context.Channel.Id == 528696379325808655 || Context.Channel.Id == 528692074917134346)
            {
                if (Blacklisted(Context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (Context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null || _voiceChannel.Id != 528688237812908057)
                {
                    await ReplyAsync($"{Context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (Context.Guild.CurrentUser.VoiceChannel == null || audioService.NowPlaying(Context.Guild.Id) == null)
                    {
                        await ReplyAsync($"{Context.User.Mention} nothing is playing right now.");
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
                            await ReplyAsync("", false, embed.Build());
                        }
                        else
                        {
                            LavaTrack nowPlaying = audioService.NowPlaying(Context.Guild.Id);
                            if (nowPlaying.Length.TotalSeconds < time)
                            {
                                await ReplyAsync($"{Context.User.Mention} that point exceeds the songs length.");
                            }
                            else
                            {
                                TimeSpan timeSpan = new TimeSpan(0, 0, time);
                                await audioService.Seek(Context.Guild.Id, timeSpan);
                                await ReplyAsync($"{Context.User.Mention} successfully went to **{timeSpan.Minutes}:{timeSpan.Seconds.ToString("D2")}** in the current song.");
                            }
                        }
                    }
                }
            }
        }
    }
}