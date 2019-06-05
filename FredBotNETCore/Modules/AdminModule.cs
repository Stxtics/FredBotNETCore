using Discord;
using Discord.Commands;
using FredBotNETCore.Services;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules
{
    [Name("Admin")]
    [Summary("Module of commands for Administrators of servers.")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly AdminService adminService = new AdminService();

        [Command("temp", RunMode = RunMode.Async)]
        [Alias("tempmod")]
        [Summary("Makes a user temp mod on the server.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Temp(string username = null, string time = null)
        {
            await adminService.TempAsync(Context, username, time);
        }

        [Command("untemp", RunMode = RunMode.Async)]
        [Alias("removetemp")]
        [Summary("Removes temp mod from user mentioned.")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Untemp([Remainder] string username = null)
        {
            await adminService.UntempAsync(Context, username);
        }

        [Command("resetpr2name", RunMode = RunMode.Async)]
        [Alias("resetverifiedname")]
        [Summary("Resets a users verified name.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ResetPR2Name(string username)
        {
            await adminService.ResetPR2NameAsync(Context, username);
        }

        [Command("addrole", RunMode = RunMode.Async)]
        [Alias("+role", "createrole")]
        [Summary("Creates a new role, with optional color and hoist.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task AddRole([Remainder] string settings = null)
        {
            await adminService.AddRoleAsync(Context, settings);
        }

        [Command("delrole", RunMode = RunMode.Async)]
        [Alias("-role", "deleterole")]
        [Summary("Deletes a role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteRole([Remainder] string roleName = null)
        {
            await adminService.DeleteRoleAsync(Context, roleName);
        }

        [Command("mentionable", RunMode = RunMode.Async)]
        [Alias("rolementionable")]
        [Summary("Toggle a role being mentionable.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task Mentionable([Remainder] string roleName = null)
        {
            await adminService.MentionableAsync(Context, roleName);
        }

        [Command("rolecolor", RunMode = RunMode.Async)]
        [Alias("colorrole", "setcolor", "rolecolour")]
        [Summary("Change the color of a role.")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireContext(ContextType.Guild)]
        public async Task RoleColor([Remainder] string roleNameAndColor = null)
        {
            await adminService.RoleColorAsync(Context, roleNameAndColor);
        }

        [Command("addjoinablerole", RunMode = RunMode.Async)]
        [Alias("addjoinrole", "ajr", "+jr", "+joinablerole")]
        [Summary("Adds a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddJoinableRole(string roleName = null, [Remainder] string description = null)
        {
            await adminService.AddJoinableRoleAsync(Context, roleName, description);
        }

        [Command("deljoinablerole", RunMode = RunMode.Async)]
        [Alias("deljoinrole", "djr", "-jr", "-joinablerole", "removejoinablerole", "rjr")]
        [Summary("Removes a joinable role.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteJoinableRole([Remainder] string roleName = null)
        {
            await adminService.DeleteJoinableRoleAsync(Context, roleName);
        }

        [Command("addmod", RunMode = RunMode.Async)]
        [Alias("+mod")]
        [Summary("Add a bot moderator or group of moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddMod([Remainder] string mod = null)
        {
            await adminService.AddModAsync(Context, mod);
        }

        [Command("delmod", RunMode = RunMode.Async)]
        [Alias("-mod", "deletemod")]
        [Summary("Remove a bot moderator or group of moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteMod([Remainder] string mod = null)
        {
            await adminService.DeleteModAsync(Context, mod);
        }

        [Command("listmods", RunMode = RunMode.Async)]
        [Alias("listmod", "showmods")]
        [Summary("List bot moderators.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListMods()
        {
            await adminService.ListModsAsync(Context);
        }

        [Command("blacklistword", RunMode = RunMode.Async)]
        [Alias("wordblacklist", "addblacklistedword")]
        [Summary("Blacklist a word from being said in the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistWord([Remainder] string text = null)
        {
            await adminService.BlacklistWordAsync(Context, text);
        }

        [Command("unblacklistword", RunMode = RunMode.Async)]
        [Alias("wordunblacklist", "removeblacklistedword")]
        [Summary("Unblacklist a word from being said on the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task UnblacklistWord([Remainder] string text = null)
        {
            await adminService.UnblacklistWordAsync(Context, text);
        }

        [Command("listblacklistedwords", RunMode = RunMode.Async)]
        [Alias("lbw", "blacklistedwords")]
        [Summary("Lists all the words that are blacklisted from being said on the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListBlacklistedWords()
        {
            await adminService.ListBlacklistedWordsAsync(Context);
        }

        [Command("blacklisturl", RunMode = RunMode.Async)]
        [Alias("urlblacklist", "addblacklistedurl")]
        [Summary("Blacklist a URL from being said in the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task BlacklistUrl([Remainder] string url = null)
        {
            await adminService.BlacklistUrlAsync(Context, url);
        }

        [Command("unblacklisturl", RunMode = RunMode.Async)]
        [Alias("urlunblacklist", "removeblacklistedurl")]
        [Summary("Unblacklist a URL from being said on the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task UnblacklistUrl([Remainder] string url = null)
        {
            await adminService.UnblacklistUrlAsync(Context, url);
        }

        [Command("listblacklistedurls", RunMode = RunMode.Async)]
        [Alias("lbw", "blacklistedurls")]
        [Summary("Lists all the URLs that are blacklisted from being said on the server.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListBlacklistedUrls()
        {
            await adminService.ListBlacklistedUrlsAsync(Context);
        }

        [Command("addallowedchannel", RunMode = RunMode.Async)]
        [Alias("allowedchanneladd", "addpr2channel")]
        [Summary("Add a channel that PR2 commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddAllowedChannel([Remainder] string channel = null)
        {
            await adminService.AddAllowedChannelAsync(Context, channel);
        }

        [Command("removeallowedchannel", RunMode = RunMode.Async)]
        [Alias("delallowedchannel", "allowedchanneldel", "allowedchannelremove", "removepr2channel")]
        [Summary("Remove a channel that PR2 commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveAllowedChannel([Remainder] string channel = null)
        {
            await adminService.RemoveAllowedChannelAsync(Context, channel);
        }

        [Command("listallowedchannels", RunMode = RunMode.Async)]
        [Alias("allowedchannelslist", "listpr2channels", "allowedchannels")]
        [Summary("Lists all the channels that PR2 commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListAllowedChannels()
        {
            await adminService.ListAllowedChannelsAsync(Context);
        }

        [Command("addmusicchannel", RunMode = RunMode.Async)]
        [Alias("musicchanneladd", "addaudiochannel")]
        [Summary("Add a channel that Music commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task AddMusicChannel([Remainder] string channel = null)
        {
            await adminService.AddMusicChannelAsync(Context, channel);
        }

        [Command("removemusicchannel", RunMode = RunMode.Async)]
        [Alias("delmusicchannel", "musicchanneldel", "musicchannelremove", "removemusicchannel")]
        [Summary("Remove a channel that Music commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveMusicChannel([Remainder] string channel = null)
        {
            await adminService.RemoveMusicChannelAsync(Context, channel);
        }

        [Command("listmusicchannels", RunMode = RunMode.Async)]
        [Alias("musicchannelslist", "listaudiochannels", "musicchannels")]
        [Summary("Lists all the channels that Music commands can be done in.")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [RequireContext(ContextType.Guild)]
        public async Task ListMusicChannels()
        {
            await adminService.ListMusicChannelsAsync(Context);
        }

        [Command("logchannel", RunMode = RunMode.Async)]
        [Alias("updatelogchannel", "setlogchannel")]
        [Summary("Sets the log channel of the server.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetLogChannel([Remainder] string channel = null)
        {
            await adminService.SetLogChannelAsync(Context, channel);
        }

        [Command("notificationschannel", RunMode = RunMode.Async)]
        [Alias("updatenotificationschannel", "setnotificationschannel")]
        [Summary("Sets the notifications channel of the server.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetNotificationsChannel([Remainder] string channel = null)
        {
            await adminService.SetNotificationsChannelAsync(Context, channel);
        }

        [Command("banlogchannel", RunMode = RunMode.Async)]
        [Alias("updatebanlogchannel", "setbanlogchannel")]
        [Summary("Sets the ban log channel for the server.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task SetBanLogChannel([Remainder] string channel = null)
        {
            await adminService.SetBanLogChannelAsync(Context, channel);
        }
    }
}
