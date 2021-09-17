using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FredBotNETCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace FredBotNETCore.Services
{
    public class AudioService
    {
        private readonly LavaNode<CustomLavaPlayer> _lavaNode;
        private readonly DiscordSocketClient _client;

        public AudioService(LavaNode<CustomLavaPlayer> lavaNode, DiscordSocketClient client)
        {
            _lavaNode = lavaNode;
            _client = client;

            // subscribe to events
            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnTrackException += OnTrackException;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
        }

        public async Task Connect(IVoiceChannel voiceChannel, ITextChannel messageChannel)
        {
            await _lavaNode.JoinAsync(voiceChannel, messageChannel);
        }

        public async Task Disconnect(IVoiceChannel voiceChannel)
        {
            await _lavaNode.LeaveAsync(voiceChannel);
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason is TrackEndReason.LoadFailed || args.Reason is TrackEndReason.Cleanup
                || args.Reason is TrackEndReason.Replaced || args.Reason is TrackEndReason.Stopped)
            {
                return;
            }
            CustomLavaPlayer player = (CustomLavaPlayer)args.Player;
            player.SkippedUsers.Clear();
            player.Queue.TryDequeue(out _);

            if (!player.CustomQueue.TryDequeue(out CustomLavaTrack nextTrack) && !player.QueueLoop)
            {
                await player.TextChannel.SendMessageAsync("Queue finished.");
                await Disconnect(player.VoiceChannel);
            }
            else if ((player.VoiceChannel as SocketVoiceChannel).Users.Count < 2)
            {
                await player.TextChannel.SendMessageAsync("Nobody is listening. Stopping music.");
                await Stop(player.VoiceChannel.Guild);
            }
            else if (player.QueueLoop)
            {
                if (nextTrack is null) // no songs in queue, play the current track again
                {
                    await player.PlayAsync(args.Track);

                    // HACK
                    // sometimes track end is called multiple times when looping
                    // so this check makes sure the message is only sent once
                    if ((DateTime.Now - player.LastFinishTime).TotalSeconds > 5)
                    {
                        await player.TextChannel.SendMessageAsync(string.Format("Now playing: **{0}** ({1}:{2}) requested by **{3}#{4}**.",
                            Format.Sanitize(player.Track.Title), player.Track.Duration.Minutes, player.Track.Duration.Seconds,
                            Format.Sanitize(player.User.Username), player.User.Discriminator));
                    }
                }
                else
                {
                    //requeue the track that just finished
                    player.Queue.Enqueue(args.Track);
                    player.CustomQueue.Enqueue(new CustomLavaTrack(args.Track, player.User));

                    //play the next track
                    await player.PlayAsync(nextTrack);

                    // HACK
                    // sometimes track end is called multiple times when looping
                    // so this check makes sure the message is only sent once
                    if ((DateTime.Now - player.LastFinishTime).TotalSeconds > 5)
                    {
                        await player.TextChannel.SendMessageAsync(string.Format("Now playing: **{0}** ({1}:{2}) requested by **{3}#{4}**.",
                            Format.Sanitize(player.Track.Title), player.Track.Duration.Minutes, player.Track.Duration.Seconds,
                            Format.Sanitize(player.User.Username), player.User.Discriminator));
                    }
                }
            }
            else
            {
                // play the next track
                await player.PlayAsync(nextTrack);
                await player.TextChannel.SendMessageAsync(string.Format("Now playing: **{0}** ({1}:{2}) requested by **{3}#{4}**.",
                    Format.Sanitize(player.Track.Title), player.Track.Duration.Minutes, player.Track.Duration.Seconds,
                    Format.Sanitize(player.User.Username), player.User.Discriminator));
            }
            player.LastFinishTime = DateTime.Now;
        }

        private async Task OnTrackException(TrackExceptionEventArgs args)
        {
            await args.Player.TextChannel.SendMessageAsync($"Oh no! An error occurred while trying to play **{args.Track.Title}**. Details have been sent to my owner.");
            await Stop(args.Player.VoiceChannel.Guild);
            SocketUser user = _client.GetUser(181853112045142016);
            await user.SendMessageAsync("Music Playback Error: " + args.ErrorMessage);
        }

        private async Task OnWebSocketClosed(WebSocketClosedEventArgs args)
        {
            IGuild guild = _client.GetGuild(args.GuildId);
            if (HasPlayer(guild))
            {
                CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
                IGuildUser user = await guild.GetCurrentUserAsync();
                if (user.VoiceChannel == null)
                {
                    // handle voice channel disconnect
                    await player.TextChannel.SendMessageAsync($"Disconnected: Ending playback.");
                    await Stop(guild);
                }
                else
                {
                    // handle voice channel move
                    await Pause(guild);
                    await Task.Delay(100);
                    await Connect(user.VoiceChannel, player.TextChannel);
                    await Task.Delay(500);
                    await Resume(guild);
                }
            }
        }

        private bool HasPlayer(IGuild guild)
        {
            return _lavaNode.HasPlayer(guild);
        }

        private async Task Play(IGuild guild, CustomLavaTrack track)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            await player.PlayAsync(track);
        }

        private async Task<LavaTrack> GetTrackFromYoutube(string searchQuery)
        {
            SearchResponse search = await _lavaNode.SearchYouTubeAsync(searchQuery);
            if (search.Status == SearchStatus.NoMatches || search.Status == SearchStatus.LoadFailed)
            {
                return null;
            }
            return search.Tracks.FirstOrDefault();
        }

        private async Task<LavaTrack> GetTrackFromYoutubeUrl(string url)
        {
            SearchResponse search = await _lavaNode.SearchAsync(SearchType.Direct, url);
            if (search.Status == SearchStatus.NoMatches || search.Status == SearchStatus.LoadFailed)
            {
                return null;
            }
            return search.Tracks.FirstOrDefault();
        }

        private async Task Pause(IGuild guild)
        {
            await _lavaNode.GetPlayer(guild).PauseAsync();
        }

        private async Task Resume(IGuild guild)
        {
            await _lavaNode.GetPlayer(guild).ResumeAsync();
        }

        private async Task SetVolume(IGuild guild, ushort volume)
        {
            await _lavaNode.GetPlayer(guild).UpdateVolumeAsync(volume);
        }

        private async Task Skip(IGuild guild)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            if (player.Queue.Count == 0)
            {
                await player.TextChannel.SendMessageAsync("Queue finished.");
                await Stop(guild);
            }
            else
            {
                await player.SkipAsync();

                await player.TextChannel.SendMessageAsync($"Now playing: " +
                    $"**{Format.Sanitize(player.Track.Title)}** ({player.Track.Duration.Minutes}:{player.Track.Duration.Seconds:D2}) " +
                    $"requested by **{player.User.Username}#{player.User.Discriminator}**.");
            }
        }

        private List<SocketUser> GetSkippedUsers(IGuild guild)
        {
            return _lavaNode.GetPlayer(guild).SkippedUsers;
        }

        private void AddSkippedUser(IGuild guild, SocketUser user)
        {
            _lavaNode.GetPlayer(guild).SkippedUsers.Add(user);
        }

        private CustomLavaTrack NowPlaying(IGuild guild)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);

            return player.Track != null ? new CustomLavaTrack(player.Track, player.User) : null;
        }

        private async Task Stop(IGuild guild)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                await player.StopAsync();
            }
            await Disconnect(player.VoiceChannel);
            player.SkippedUsers.Clear();
        }

        private bool Paused(IGuild guild)
        {
            return _lavaNode.GetPlayer(guild).PlayerState == PlayerState.Paused;
        }

        private bool Playing(IGuild guild)
        {
            return _lavaNode.GetPlayer(guild).PlayerState == PlayerState.Playing;
        }

        private int GetVolume(IGuild guild)
        {
            return _lavaNode.GetPlayer(guild).Volume;
        }

        private void QueueAdd(IGuild guild, LavaTrack track, SocketUser user)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            player.Queue.Enqueue(track);
            player.CustomQueue.Enqueue(new CustomLavaTrack(track, user));
        }

        private CustomLavaTrack QueueRemove(IGuild guild, int index)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            player.Queue.RemoveAt(index);
            return player.CustomQueue.RemoveAt(index);
        }

        private void QueueClear(IGuild guild)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            player.Queue.Clear();
            player.CustomQueue.Clear();
        }

        private bool Loop(IGuild guild)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            player.QueueLoop = !player.QueueLoop;
            return player.QueueLoop;
        }

        private async Task Seek(IGuild guild, TimeSpan time)
        {
            await _lavaNode.GetPlayer(guild).SeekAsync(time);
        }

        private DefaultQueue<CustomLavaTrack> Queue(IGuild guild)
        {
            return _lavaNode.GetPlayer(guild).CustomQueue;
        }

        private CustomLavaTrack Track(IGuild guild)
        {
            CustomLavaPlayer player = _lavaNode.GetPlayer(guild);
            return new CustomLavaTrack(player.Track, player.User);
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
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
                    if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                    }
                    else
                    {
                        if (context.Guild.CurrentUser.VoiceChannel == null || !HasPlayer(context.Guild))
                        {
                            await Connect((context.User as SocketGuildUser).VoiceChannel, context.Channel as ITextChannel);
                        }

                        LavaTrack track = null;
                        bool result = Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult)
                                  && (uriResult.Scheme == "http" || uriResult.Scheme == "https");

                        if (result && (uriResult.Host == "youtube.com" || uriResult.Host == "youtu.be" || uriResult.Host == "www.youtube.com"))
                        {
                            track = await GetTrackFromYoutubeUrl(url);
                        }
                        else if (result)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} that URL is not supported. Use a youtube URL or search for the song.");
                        }
                        else
                        {
                            track = await GetTrackFromYoutube(url);
                        }

                        if (track == null)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} I could not find a song with that URL.");
                        }
                        else if (track.Duration.TotalSeconds < 5)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the minimum song length is 5 seconds.");
                        }
                        else if (track.Duration.TotalMinutes > 10)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the maximum song length is 10 minutes.");
                        }
                        else if (Queue(context.Guild).Any(x => x.Url.Equals(track.Url)) || (Playing(context.Guild) && Track(context.Guild).Url == track.Url))
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} that song is already in the queue or currently playing.");
                        }
                        else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                        }
                        else
                        {
                            EmbedBuilder embed = new EmbedBuilder()
                            {
                                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                                Author = new EmbedAuthorBuilder()
                                {
                                    Name = "Add song",
                                    Url = track.Url
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
                                                Value = track.Duration.Minutes + ":" + track.Duration.Seconds.ToString("D2"),
                                                IsInline = false
                                            },
                                            new EmbedFieldBuilder
                                            {
                                                Name = "Channel",
                                                Value = Format.Sanitize(track.Author),
                                                IsInline = false
                                            }
                                        },
                                Footer = new EmbedFooterBuilder()
                                {
                                    IconUrl = context.User.GetAvatarUrl(),
                                    Text = $"Queued by {context.User.Username}#{context.User.Discriminator}"
                                },
                                ThumbnailUrl = await track.FetchArtworkAsync()
                            };
                            embed.WithCurrentTimestamp();
                            await context.Channel.SendMessageAsync("", false, embed.Build());
                            if (NowPlaying(context.Guild) != null && Playing(context.Guild) || Paused(context.Guild))
                            {
                                QueueAdd(context.Guild, track, context.User);
                            }
                            else
                            {
                                await Play(context.Guild, new CustomLavaTrack(track, context.User));
                                await context.Channel.SendMessageAsync(string.Format("Now playing: **{0}** ({1}:{2}) requested by **{3}#{4}**.",
                                    Format.Sanitize(track.Title), track.Duration.Minutes, track.Duration.Seconds, Format.Sanitize(context.User.Username), context.User.Discriminator));
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel != null)
                    {
                        if (Queue(context.Guild).Count <= 0)
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
                            foreach (CustomLavaTrack track in Queue(context.Guild))
                            {
                                embed.Description += $"{count}. **{Format.Sanitize(track.Title)}** ({track.Duration.Minutes}:{track.Duration.Seconds:D2}) queued by **{Format.Sanitize(track.User.Username)}#{track.User.Discriminator}**.\n";
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (Loop(context.Guild))
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel != null && NowPlaying(context.Guild) != null)
                    {
                        if (Paused(context.Guild))
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} the music is already paused.");
                        }
                        else
                        {
                            await Pause(context.Guild);
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel != null && NowPlaying(context.Guild) != null)
                    {
                        if (Paused(context.Guild))
                        {
                            await Resume(context.Guild);
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else
                {
                    if (url == null)
                    {
                        SocketGuildUser user = context.User as SocketGuildUser;
                        if (user.Roles.Any(e => e.Name.ToUpperInvariant() == "Discord Staff".ToUpperInvariant()) && Paused(context.Guild))
                        {
                            if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                            }
                            else
                            {
                                await Resume(context.Guild);
                                await context.Channel.SendMessageAsync($"{context.User.Mention} resumed the music.");
                            }
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
            }
            else
            {
                return;
            }
        }

        public async Task QueueRemoveAsync(SocketCommandContext context, string position)
        {
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
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
                        if (context.Guild.CurrentUser.VoiceChannel == null || Queue(context.Guild).Count <= 0)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing in the queue.");
                        }
                        else if (Queue(context.Guild).Count < pos)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} there is not that many items in the queue.");
                        }
                        else
                        {
                            CustomLavaTrack removedTrack = QueueRemove(context.Guild, pos - 1);
                            await context.Channel.SendMessageAsync($"{context.User.Mention} removed **{Format.Sanitize(removedTrack.Title)}** queued by **{Format.Sanitize(removedTrack.User.Username)}#{removedTrack.User.Discriminator}** from the queue.");
                        }
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || Queue(context.Guild).Count <= 0)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} the queue is already empty.");
                    }
                    else
                    {
                        QueueClear(context.Guild);
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != null)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} I am already in a voice channel.");
                }
                else
                {
                    await Connect(_voiceChannel, context.Channel as ITextChannel);
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing to skip.");
                    }
                    else if (GetSkippedUsers(context.Guild).Contains(context.User))
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} you have already voted to skip this song.");
                    }
                    else
                    {
                        int users = _voiceChannel.Users.Count;
                        int votesNeeded = Convert.ToInt32(Math.Ceiling((double)users / 3));
                        if (votesNeeded < 1)
                        {
                            votesNeeded = 1;
                        }
                        AddSkippedUser(context.Guild, context.User);
                        if (votesNeeded - GetSkippedUsers(context.Guild).Count == 0)
                        {
                            CustomLavaTrack track = Track(context.Guild);
                            await Skip(context.Guild);
                            await context.Channel.SendMessageAsync($"**{Format.Sanitize(track.Title)}** requested by **{Format.Sanitize(track.User.Username)}#{track.User.Discriminator}** was skipped.");
                        }
                        else
                        {
                            if (votesNeeded - GetSkippedUsers(context.Guild).Count == 1)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} voted to skip **{Format.Sanitize(NowPlaying(context.Guild).Title)}**. {votesNeeded - GetSkippedUsers(context.Guild).Count} more vote needed to skip.");
                            }
                            else
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} voted to skip **{Format.Sanitize(NowPlaying(context.Guild).Title)}**. {votesNeeded - GetSkippedUsers(context.Guild).Count} more votes needed to skip.");
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is nothing to skip.");
                    }
                    else
                    {
                        CustomLavaTrack track = Track(context.Guild);
                        await Skip(context.Guild);
                        await context.Channel.SendMessageAsync($"{context.User.Mention} force skipped " +
                            $"**{Format.Sanitize(track.Title)}** requested by **{Format.Sanitize(track.User.Username)}#{track.User.Discriminator}**.");
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} nothing is playing right now.");
                    }
                    else
                    {
                        CustomLavaTrack nowPlaying = Track(context.Guild);
                        EmbedBuilder embed = new EmbedBuilder()
                        {
                            Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
                            Author = new EmbedAuthorBuilder()
                            {
                                Name = "Now playing",
                                Url = nowPlaying.Url
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
                                    Name = "Progress",
                                    Value = string.Format("{0}:{1}/{2}:{3} ({4}%)", nowPlaying.Position.Minutes, nowPlaying.Position.Seconds.ToString("D2"),
                                        nowPlaying.Duration.Minutes, nowPlaying.Duration.Seconds.ToString("D2"),
                                        Math.Round(nowPlaying.Position.TotalSeconds / nowPlaying.Duration.TotalSeconds * 100)),
                                    IsInline = false
                                },
                                new EmbedFieldBuilder
                                {
                                    Name = "Channel",
                                    Value = Format.Sanitize(nowPlaying.Author),
                                    IsInline = false
                                }
                                },
                            Footer = new EmbedFooterBuilder()
                            {
                                IconUrl = context.User.GetAvatarUrl(),
                                Text = $"Queued by {nowPlaying.User.Username}#{nowPlaying.User.Discriminator}"
                            },
                            ThumbnailUrl = await nowPlaying.FetchArtworkAsync()
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is no music playing.");
                    }
                    else
                    {
                        await Stop(context.Guild);
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
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild) == null)
                    {
                        await context.Channel.SendMessageAsync($"{context.User.Mention} there is no music to set the volume of.");
                    }
                    else
                    {
                        if (volume > 200 || volume < 0)
                        {
                            await context.Channel.SendMessageAsync($"{context.User.Mention} volume must be from 0 to 200.");
                        }
                        else
                        {
                            int currentVolume = GetVolume(context.Guild);
                            await SetVolume(context.Guild, ushort.Parse(volume.ToString()));
                            await context.Channel.SendMessageAsync($"{context.User.Mention} set the volume from **{currentVolume}** to **{volume}**.");
                        }
                    }
                }
            }
        }

        public async Task SeekAsync(SocketCommandContext context, string seconds)
        {
            if (Extensions.MusicChannels().Contains(context.Channel.Id))
            {
                if (Blacklisted(context.User))
                {
                    return;
                }
                SocketVoiceChannel _voiceChannel = (context.User as SocketGuildUser).VoiceChannel;
                if (_voiceChannel == null)// || _voiceChannel.Id != 528688237812908057)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the Music voice channel to use this command.");
                }
                else if (context.Guild.CurrentUser.VoiceChannel != _voiceChannel)
                {
                    await context.Channel.SendMessageAsync($"{context.User.Mention} you need to be in the same voice channel as me to use this command.");
                }
                else
                {
                    if (context.Guild.CurrentUser.VoiceChannel == null || NowPlaying(context.Guild) == null)
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
                            LavaTrack nowPlaying = NowPlaying(context.Guild);
                            if (nowPlaying.Duration.TotalSeconds < time)
                            {
                                await context.Channel.SendMessageAsync($"{context.User.Mention} that point exceeds the songs length.");
                            }
                            else
                            {
                                TimeSpan timeSpan = new TimeSpan(0, 0, time);
                                await Seek(context.Guild, timeSpan);
                                await context.Channel.SendMessageAsync($"{context.User.Mention} successfully went to **{timeSpan.Minutes}:{timeSpan.Seconds:D2}** in the current song.");
                            }
                        }
                    }
                }
            }
        }
    }
}
