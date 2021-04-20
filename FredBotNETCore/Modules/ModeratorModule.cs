using Discord;
using Discord.Commands;
using FredBotNETCore.Services;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules
{
    [Name("Moderator")]
    [Summary("Module of commands for Moderators and up of servers.")]
    [RequireContext(ContextType.Guild)]
    public class ModeratorModule : ModuleBase<SocketCommandContext>
    {
        private readonly ModeratorService modService = new ModeratorService();

        [Command("setnick", RunMode = RunMode.Async)]
        [Alias("setnickname")]
        [Summary("Change the nickname of a user.")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ManageNicknames)]
        public async Task SetNick(string username = null, [Remainder] string nickname = null)
        {
            await modService.SetNickAsync(Context, username, nickname);
        }

        [Command("nick", RunMode = RunMode.Async)]
        [Alias("botnick")]
        [Summary("Change the bots nickname.")]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        [RequireBotPermission(GuildPermission.ChangeNickname)]
        public async Task Nick([Remainder] string nickname = null)
        {
            await modService.NickAsync(Context, nickname);
        }

        [Command("updatetoken", RunMode = RunMode.Async)]
        [Alias("utoken", "changetoken")]
        [Summary("Updates token for FredtheG.CactusBot.")]
        public async Task UpdateToken(string newToken)
        {
            await modService.UpdateTokenAsync(Context, newToken);
        }

        [Command("notifymacroers", RunMode = RunMode.Async)]
        [Alias("pingmacroers")]
        [Summary("Let macroers know the servers have restarted.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task NotifyMacroers()
        {
            await modService.NotifyMacroersAsync(Context);
        }

        [Command("blacklistmusic", RunMode = RunMode.Async)]
        [Alias("musicblacklist")]
        [Summary("Blacklist a user from using music commands.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task BlacklistMusic([Remainder] string username = null)
        {
            await modService.BlacklistMusicAsync(Context, username);
        }

        [Command("unblacklistmusic", RunMode = RunMode.Async)]
        [Alias("musicunblacklist")]
        [Summary("Unblacklist a user from using music commands.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task UnblacklistMusic([Remainder] string username = null)
        {
            await modService.UnblacklistMusicAsync(Context, username);
        }

        [Command("listblacklistedmusic", RunMode = RunMode.Async)]
        [Alias("lbm", "blacklistedmusic")]
        [Summary("Lists blacklisted users from music commands.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ListBlacklistedMusic()
        {
            await modService.ListBlacklistedMusicAsync(Context);
        }

        [Command("blacklistsuggestions", RunMode = RunMode.Async)]
        [Alias("blacklistsuggestion", "suggestionblacklist", "suggestionsblacklist")]
        [Summary("Blacklist a user from using the /suggest command.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task BlacklistSuggestions([Remainder] string username = null)
        {
            await modService.BlacklistSuggestionsAsync(Context, username);
        }

        [Command("unblacklistsuggestions", RunMode = RunMode.Async)]
        [Alias("unblacklistsuggestion", "suggestionunblacklist", "suggestionsunblacklist")]
        [Summary("Unblacklist a user from using the /suggest command.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task UnblacklistSuggestions([Remainder] string username = null)
        {
            await modService.UnblacklistSuggestionsAsync(Context, username);
        }

        [Command("listblacklistedsuggestions", RunMode = RunMode.Async)]
        [Alias("lbs", "listblacklistedsuggestion", "blacklistedsuggestions")]
        [Summary("Lists blacklisted users from suggestions.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ListBlacklistedSuggestions()
        {
            await modService.ListBlacklistedSuggestionsAsync(Context);
        }

        [Command("channelinfo", RunMode = RunMode.Async)]
        [Alias("infochannel", "channel", "ci")]
        [Summary("Gets information about a channel.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ChannelInfo([Remainder] string channelName)
        {
            await modService.ChannelInfoAsync(Context, channelName);
        }

        [Command("membercount", RunMode = RunMode.Async)]
        [Alias("mcount", "usercount", "ucount")]
        [Summary("Get the server member count.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task MemberCount()
        {
            await modService.MemberCountAsync(Context);
        }

        [Command("uptime", RunMode = RunMode.Async)]
        [Alias("utime", "ut")]
        [Summary("Gets the bot uptime.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Uptime()
        {
            await modService.UptimeAsync(Context);
        }

        [Command("roleinfo", RunMode = RunMode.Async)]
        [Alias("rinfo")]
        [Summary("Get information about a role.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task RoleInfo([Remainder] string roleName = null)
        {
            await modService.RoleInfoAsync(Context, roleName);
        }

        [Command("roles", RunMode = RunMode.Async)]
        [Alias("rolelist")]
        [Summary("Get a list of server roles and member counts.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Roles()
        {
            await modService.RolesAsync(Context);
        }

        [Command("warnings", RunMode = RunMode.Async)]
        [Alias("warns")]
        [Summary("Get warnings for the server or user.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Warnings([Remainder] string username = null)
        {
            await modService.WarningsAsync(Context, username);
        }

        [Command("unban", RunMode = RunMode.Async)]
        [Alias("removeban")]
        [Summary("Unban a member.")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Unban(string username = null, [Remainder] string reason = null)
        {
            await modService.UnbanAsync(Context, username, reason);
        }

        [Command("undeafen", RunMode = RunMode.Async)]
        [Alias("undeaf")]
        [Summary("Undeafen a member.")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.DeafenMembers)]
        public async Task Undeafen(string username = null, [Remainder] string reason = null)
        {
            await modService.UndeafenAsync(Context, username, reason);
        }

        [Command("deafen", RunMode = RunMode.Async)]
        [Alias("deaf")]
        [Summary("Deafen a member.")]
        [RequireUserPermission(GuildPermission.DeafenMembers)]
        [RequireBotPermission(GuildPermission.DeafenMembers)]
        public async Task Deafen(string username = null, [Remainder] string reason = null)
        {
            await modService.DeafenAsync(Context, username, reason);
        }

        [Command("softban", RunMode = RunMode.Async)]
        [Alias("sb")]
        [Summary("Softban a member (ban and immediate unban to delete user messages).")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task Softban(string username = null, [Remainder] string reason = null)
        {
            await modService.SoftbanAsync(Context, username, reason);
        }

        [Command("getcase", RunMode = RunMode.Async)]
        [Alias("getprior", "case")]
        [Summary("Get info on a case.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task GetCase(string caseN = null)
        {
            await modService.GetCaseAsync(Context, caseN);
        }

        [Command("modlogs", RunMode = RunMode.Async)]
        [Alias("priors")]
        [Summary("Get a list of mod logs for a user.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Modlogs([Remainder] string username)
        {
            await modService.ModlogsAsync(Context, username);
        }

        [Command("reason", RunMode = RunMode.Async)]
        [Alias("edit", "editcase")]
        [Summary("Edit a reason for a mod log.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Reason(string caseN = null, [Remainder] string reason = null)
        {
            await modService.ReasonAsync(Context, caseN, reason);
        }

        [Command("warn", RunMode = RunMode.Async)]
        [Alias("w")]
        [Summary("Warn a member.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Warn(string username = null, [Remainder] string reason = null)
        {
            await modService.WarnAsync(Context, username, reason);
        }

        [Command("endgiveaway", RunMode = RunMode.Async)]
        [Alias("endg", "gend")]
        [Summary("Ends the giveaway.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task EndGiveaway()
        {
            await modService.EndGiveawayAsync(Context);
        }

        [Command("repick", RunMode = RunMode.Async)]
        [Alias("reroll", "redo")]
        [Summary("Repicks giveaway winner.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Repick()
        {
            await modService.RepickAsync(Context);
        }

        [Command("giveaway", RunMode = RunMode.Async)]
        [Alias("give")]
        [Summary("Create a giveaway.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.AddReactions)]
        public async Task Giveaway(string channel = null, string time = null, string winnersS = null, [Remainder] string item = null)
        {
            await modService.GiveawayAsync(Context, channel, time, winnersS, item);
        }

        [Command("promote", RunMode = RunMode.Async)]
        [Alias("prom")]
        [Summary("Announces user promoted to temp/trial/perm mod on PR2.")]
        public async Task Promote(string type = null, [Remainder] string username = null)
        {
            await modService.PromoteAsync(Context, type, username);
        }

        [Command("unmute", RunMode = RunMode.Async)]
        [Alias("removemute")]
        [Summary("Unmutes a user.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public async Task Unmute(string username = null, [Remainder] string reason = null)
        {
            await modService.UnmuteAsync(Context, username, reason);
        }

        [Command("ping", RunMode = RunMode.Async)]
        [Alias("latency")]
        [Summary("Returns the latency between the bot and Discord.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Ping()
        {
            await modService.PingAsync(Context);
        }

        [Command("botinfo", RunMode = RunMode.Async)]
        [Alias("fredinfo")]
        [Summary("Shows all bot info.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task BotInfo()
        {
            await modService.BotInfoAsync(Context);
        }

        [Command("userinfo", RunMode = RunMode.Async)]
        [Alias("uinfo", "jvwhois")]
        [Summary("Returns information about a user.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task UserInfo([Remainder] string username = null)
        {
            await modService.UserInfoAsync(Context, username);
        }

        [Command("guildinfo", RunMode = RunMode.Async)]
        [Alias("ginfo", "serverinfo")]
        [Summary("Information about current server.")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task ServerInfo()
        {
            await modService.ServerInfoAsync(Context);
        }

        [Command("purge", RunMode = RunMode.Async)]
        [Alias("delete")]
        [Summary("Deletes number of messages specified, optional user mention.")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(string amount = null, [Remainder] string username = null)
        {
            await modService.PurgeAsync(Context, amount, username);
        }

        [Command("kick", RunMode = RunMode.Async)]
        [Alias("kick")]
        [Summary("Kick a user.")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Kick(string username = null, [Remainder] string reason = null)
        {
            await modService.KickAsync(Context, username, reason);
        }

        [Command("ban", RunMode = RunMode.Async)]
        [Alias("Ban")]
        [Summary("Bans a user, needs reason.")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Ban(string username = null, [Remainder] string reason = null)
        {
            await modService.BanAsync(Context, username, reason);
        }

        [Command("mute", RunMode = RunMode.Async)]
        [Alias("Mute")]
        [Summary("Mutes a user.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task Mute(string username = null, string time = null, [Remainder] string reason = null)
        {
            await modService.MuteAsync(Context, username, time, reason);
        }
    }
}
