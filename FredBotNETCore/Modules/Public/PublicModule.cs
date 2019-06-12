using Discord.Commands;
using FredBotNETCore.Services;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules.Public
{
    [Name("Public")]
    [Summary("Module of commands that anyone can use.")]
    public class PublicModule : ModuleBase<SocketCommandContext>
    {
        private readonly PublicService publicService = new PublicService();

        #region Everyone

        [Command("pay", RunMode = RunMode.Async)]
        [Alias("give")]
        [Summary("Pay money to another user.")]
        [RequireContext(ContextType.DM)]
        public async Task Pay(string amount = null, [Remainder] string username = null)
        {
            await publicService.PayAsync(Context, amount, username);
        }

        [Command("balance", RunMode = RunMode.Async)]
        [Alias("bal")]
        [Summary("See how much money you have.")]
        [RequireContext(ContextType.DM)]
        public async Task Balance([Remainder] string username = null)
        {
            await publicService.BalanceAsync(Context, username);
        }

        [Command("jackpot", RunMode = RunMode.Async)]
        [Alias("lottobal")]
        [Summary("See how much money is in the jackpot.")]
        [RequireContext(ContextType.DM)]
        public async Task Jackpot()
        {
            await publicService.JackpotAsync(Context);
        }

        [Command("leaderboard", RunMode = RunMode.Async)]
        [Alias("lb")]
        [Summary("Shows users with the most money.")]
        [RequireContext(ContextType.DM)]
        public async Task Leaderboard()
        {
            await publicService.LeaderboardAsync(Context);
        }

        [Command("lotto", RunMode = RunMode.Async)]
        [Alias("lottery")]
        [Summary("Have a go at winning the jackpot.")]
        [RequireContext(ContextType.DM)]
        public async Task Lotto(string ticketsS = null)
        {
            await publicService.LottoAsync(Context, ticketsS);
        }

        [Command("daily", RunMode = RunMode.Async)]
        [Alias("work")]
        [Summary("Collect daily cash.")]
        [RequireContext(ContextType.DM)]
        public async Task Daily()
        {
            await publicService.DailyAsync(Context);
        }

        [Command("verify", RunMode = RunMode.Async)]
        [Alias("verifyme")]
        [Summary("Link your PR2 account to your Discord account.")]
        public async Task Verify()
        {
            await publicService.VerifyAsync(Context);
        }

        [Command("verifycomplete", RunMode = RunMode.Async)]
        [Alias("verifydone", "verified")]
        [Summary("Let Fred know you've sent the PM.")]
        [RequireContext(ContextType.DM)]
        public async Task Verified([Remainder] string username)
        {
            await publicService.VerifiedAsync(Context, username);
        }

        [Command("suggest", RunMode = RunMode.Async)]
        [Alias("suggestion")]
        [Summary("Suggest something for the Discord server.")]
        [RequireContext(ContextType.Guild)]
        public async Task Suggest([Remainder] string suggestion = null)
        {
            await publicService.SuggestAsync(Context, suggestion);
        }

        [Command("weather", RunMode = RunMode.Async)]
        [Alias("weath")]
        [Summary("Get weather at a location.")]
        [RequireContext(ContextType.DM)]
        public async Task Weather([Remainder] string city = null)
        {
            await publicService.WeatherAsync(Context, city);
        }

        #endregion

        #region PR2 Commands

        [Command("hint", RunMode = RunMode.Async)]
        [Alias("arti", "artifact")]
        [Summary("Get the current artifact hint.")]
        public async Task Hint()
        {
            await publicService.HintAsync(Context);
        }

        [Command("view", RunMode = RunMode.Async)]
        [Summary("View information on a PR2 account.")]
        public async Task View([Remainder] string pr2name = null)
        {
            await publicService.ViewAsync(Context, pr2name);
        }

        [Command("viewid", RunMode = RunMode.Async)]
        [Alias("vid")]
        [Summary("View infomation on a PR2 account by its ID.")]
        public async Task ViewID(string id = null)
        {
            await publicService.ViewIDAsync(Context, id);
        }

        [Command("guild", RunMode = RunMode.Async)]
        [Summary("Get details on a PR2 guild.")]
        public async Task Guild([Remainder] string guildname = null)
        {
            await publicService.GuildAsync(Context, guildname);
        }

        [Command("guildid", RunMode = RunMode.Async)]
        [Alias("gid")]
        [Summary("Get details on a PR2 guild by its ID.")]
        public async Task GuildID(string id = null)
        {
            await publicService.GuildIDAsync(Context, id);
        }

        [Command("exp", RunMode = RunMode.Async)]
        [Alias("experience")]
        [Summary("Get EXP needed from one PR2 rank to another.")]
        public async Task EXP([Remainder] string lvl)
        {
            await publicService.EXPAsync(Context, lvl);
        }

        [Command("role", RunMode = RunMode.Async)]
        [Alias("joinrole", "leaverole")]
        [Summary("Add or remove yourself from a joinable role.")]
        [RequireContext(ContextType.Guild)]
        public async Task Role([Remainder] string roleName = null)
        {
            await publicService.RoleAsync(Context, roleName);
        }

        [Command("joinableroles")]
        [Alias("ljr", "listjroles", "listjoinableroles")]
        [Summary("Lists all joinable roles.")]
        [RequireContext(ContextType.Guild)]
        public async Task ListJoinableRoles()
        {
            await publicService.ListJoinableRolesAsync(Context);
        }

        [Command("topguilds", RunMode = RunMode.Async)]
        [Alias("guildstop", "topg", "gtop")]
        [Summary("Returns current top 10 guilds on pr2.")]
        public async Task TopGuilds()
        {
            await publicService.TopGuildsAsync(Context);
        }

        [Command("f@h", RunMode = RunMode.Async)]
        [Alias("fah", "fold")]
        [Summary("Get how many points someone has on Folding @ Home.")]
        public async Task Fah([Remainder] string fahuser = null)
        {
            await publicService.FahAsync(Context, fahuser);
        }

        [Command("bans", RunMode = RunMode.Async)]
        [Alias("pr2ban")]
        [Summary("Gets a PR2 Ban with ID from https://pr2hub.com/bans/")]
        public async Task Bans(string id = null)
        {
            await publicService.BansAsync(Context, id);
        }

        [Command("Pop", RunMode = RunMode.Async)]
        [Alias("population")]
        [Summary("Tells you the number of users on pr2. Does not include private servers.")]
        public async Task Pop([Remainder] string s = null)
        {
            await publicService.PopAsync(Context, s);
        }

        [Command("stats", RunMode = RunMode.Async)]
        [Alias("s")]
        [Summary("Gets stats of a server on PR2.")]
        public async Task Stats([Remainder] string server = null)
        {
            await publicService.StatsAsync(Context, server);
        }

        [Command("guildmembers", RunMode = RunMode.Async)]
        [Alias("gmembers", "membersguild", "membersg")]
        [Summary("Gets members of a PR2 guild.")]
        public async Task GuildMembers([Remainder] string guildname = null)
        {
            await publicService.GuildMembersAsync(Context, guildname);
        }

        [Command("guildmembersid", RunMode = RunMode.Async)]
        [Alias("gmembersid", "membersguildid", "membersgid")]
        [Summary("Gets members of a PR2 guild by ID.")]
        public async Task GuildMembersID(string id = null)
        {
            await publicService.GuildMembersIDAsync(Context, id);
        }

        [Command("hh", RunMode = RunMode.Async)]
        [Alias("happyhour")]
        [Summary("Returns a list of servers (if any) with happy hour on them.")]
        public async Task HH()
        {
            await publicService.HHAsync(Context);
        }

        [Command("level", RunMode = RunMode.Async)]
        [Alias("levelinfo", "li")]
        [Summary("Gets info about a PR2 level.")]
        public async Task Level([Remainder] string level)
        {
            await publicService.LevelAsync(Context, level);
        }

        //[Command("verifyguild", RunMode = RunMode.Async)]
        //[Alias("addguild")]
        //[Summary("Creates a channel for the users guild.")]
        //public async Task VerifyGuild()
        //{
        //    await publicService.VerifyGuildAsync(Context);
        //}

        //[Command("joinguild", RunMode = RunMode.Async)]
        //[Alias("jguild", "guildjoin", "guildj")]
        //[Summary("Adds user to guild role of their guild if it exists.")]
        //public async Task JoinGuild()
        //{
        //    await publicService.JoinGuildAsync(Context);
        //}

        [Command("servers", RunMode = RunMode.Async)]
        [Alias("sl", "status", "serverstatus", "ss")]
        [Summary("Shows all servers and population.")]
        public async Task Servers([Remainder] string s1 = null)
        {
            await publicService.ServersAsync(Context, s1);
        }

        [Command("staff")]
        [Alias("staffonline", "so")]
        [Summary("Returns a list of the current online PR2 Staff.")]
        public async Task Staff()
        {
            await publicService.StaffAsync(Context);
        }

        #endregion
    }
}