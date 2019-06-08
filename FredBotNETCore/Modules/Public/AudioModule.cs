using Discord;
using Discord.Commands;
using FredBotNETCore.Services;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules.Public
{
    [Name("Audio")]
    [Summary("Module containing all of the music commands.")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService audioService;
        public AudioModule(AudioService service)
        {
            audioService = service;
        }

        [Command("add", RunMode = RunMode.Async)]
        [Alias("addsong")]
        [Summary("Adds a song to play.")]
        [RequireContext(ContextType.Guild)]
        public async Task Add([Remainder] string url = null)
        {
            await audioService.AddAsync(Context, url);
        }

        [Command("queue", RunMode = RunMode.Async)]
        [Alias("q")]
        [Summary("Displays song queue.")]
        [RequireContext(ContextType.Guild)]
        public async Task ShowQueue()
        {
            await audioService.ShowQueueAsync(Context);
        }

        [Command("loop", RunMode = RunMode.Async)]
        [Alias("repeat", "queueloop", "loopqueue", "qloop", "loopq")]
        [Summary("Toggles looping of the queue.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task QueueLoop()
        {
            await audioService.QueueLoopsAsync(Context);
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Alias("p", "pausemusic")]
        [Summary("pauses the music")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task PauseMusic()
        {
            await audioService.PauseMusicAsync(Context);
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Summary("Resumes play of music or adds another song.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Resume()
        {
            await audioService.ResumeAsync(Context);
        }

        [Command("play", RunMode = RunMode.Async)]
        [Alias("playmusic")]
        [Summary("Resumes play of music or adds another song.")]
        [RequireContext(ContextType.Guild)]
        public async Task Play([Remainder] string url = null)
        {
            await audioService.PlayAsync(Context, url);
        }

        [Command("qremove", RunMode = RunMode.Async)]
        [Alias("queueremove")]
        [Summary("Remove an item from the queue.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task QueueRemove(string position = null)
        {
            await audioService.QueueRemoveAsync(Context, position);
        }

        [Command("qclear", RunMode = RunMode.Async)]
        [Alias("clearqueue", "clearq")]
        [Summary("Removes all songs from the queue")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task QueueClear()
        {
            await audioService.QueueClearAsync(Context);
        }

        [Command("come", RunMode = RunMode.Async)]
        [Alias("summon", "join")]
        [Summary("Brings bot to voice channel")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Come()
        {
            await audioService.ComeAsync(Context);
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Alias("skipsong")]
        [Summary("Votes to skip current song")]
        [RequireContext(ContextType.Guild)]
        public async Task SkipSong()
        {
            await audioService.SkipSongAsync(Context);
        }

        [Command("forceskip", RunMode = RunMode.Async)]
        [Alias("fskip", "forceskipsong")]
        [Summary("Skips the current song.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task ForceSkip()
        {
            await audioService.ForceSkipAsync(Context);
        }

        [Command("np", RunMode = RunMode.Async)]
        [Alias("nowplaying")]
        [Summary("Displays current song playing.")]
        [RequireContext(ContextType.Guild)]
        public async Task NP()
        {
            await audioService.NPAsync(Context);
        }

        [Command("stop", RunMode = RunMode.Async)]
        [Alias("stopmusic")]
        [Summary("Stops the music and makes bot leave voice channel.")]
        [RequireOwner]
        public async Task Stop()
        {
            await audioService.StopAsync(Context);
        }

        [Command("setvolume", RunMode = RunMode.Async)]
        [Alias("volume")]
        [Summary("Sets volume of the music")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Volume(int volume)
        {
            await audioService.VolumeAsync(Context, volume);
        }

        [Command("seek", RunMode = RunMode.Async)]
        [Alias("goto")]
        [Summary("Goes to a certain point in a song.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireContext(ContextType.Guild)]
        public async Task Seek(string seconds = null)
        {
            await audioService.SeekAsync(Context, seconds);
        }
    }
}