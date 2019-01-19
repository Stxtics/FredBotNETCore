using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace FredBotNETCore.Modules.Public
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService commandService;
        private readonly IServiceProvider serviceProvider;

        public HelpModule(CommandService commands, IServiceProvider provider)
        {
            commandService = commands;
            serviceProvider = provider;
        }

        [Command("help", RunMode = RunMode.Async)]
        [Alias("commands")]
        [Summary("List of commands for the bot.")]
        public async Task Help()
        {
            Discord.Rest.RestUserMessage msg = null;
            if (!(Context.Channel is IDMChannel))
            {
                msg = await Context.Channel.SendMessageAsync($"{Context.User.Mention} I've just sent my commands to your DMs. :grinning:");
            }
            EmbedBuilder embed = new EmbedBuilder()
            {
                Color = new Color(Extensions.random.Next(256), Extensions.random.Next(256), Extensions.random.Next(256)),
            };
            string help = "**Manager**\n" +
                "/notificationschannel - Set the channel for HH and Arti updates.\n" +
                "/logchannel - Set the log channel for the server.\n" +
                "/addallowedchannel - Add an allowed channel for PR2 commands.\n" +
                "/removeallowedchannel - Removes an allowed channel for PR2 commands.\n" +
                "/listallowedchannels - Lists allowed channels for PR2 commands.\n" +
                "/resetpr2name - Used to reset a users PR2 Name.\n" +
                "/addmod - Add a server mod or role.\n" +
                "/delmod - Delete a server mod or role.\n" +
                "/listmods - List server mod roles and users.\n" +
                "/blacklistword - Blacklists a word from being said.\n" +
                "/unblacklistword - Unblacklists a word from being said.\n" +
                "/listblacklistedwords - Lists blacklisted words.\n" +
                "/blacklisturl - Blacklists a URL from being said.\n" +
                "/unblacklisturl - Unblacklists a URL from being said.\n" +
                "/listblacklistedurls - Lists blacklisted URLs.\n" +
                "/addjoinablerole - Add a joinable role.\n" +
                "/deljoinablerole - Remove a joinable role.\n" +
                "**Moderator**\n" +
                "/notifymacroers - Mentions macroers in pr2-dicussion about server restart.\n" +
                "/blacklistmusic - Blacklist a user from using music commands.\n" +
                "/unblacklistmusic - Unblacklist a user from using music commands.\n" +
                "/listblacklistedmusic - List blacklisted users from music commands.\n" +
                "/blacklistsuggestions - Blacklist a user from using /suggest.\n" +
                "/unblacklistsuggestions - Unblacklist a user from using /suggest.\n" +
                "/listblacklistedsuggestions - List blacklisted users from suggestions.\n" +
                "/channelinfo - Get info about a channel.\n" +
                "/rolecolor - Change color of a role.\n" +
                "/nick - Set bot nickname.\n" +
                "/setnick - Set nickname of a user.\n" +
                "/mentionable - Toggle making a role mentionable on/off.\n" +
                "/delrole - Delete a role.\n" +
                "/addrole - Create a role, with optional color and hoist.\n" +
                "/membercount - Get server member count.\n" +
                "/uptime - Get bot uptime.\n" +
                "/roleinfo - Get information about a role.\n" +
                "/roles - Get a list of server roles and member counts.\n" +
                "/warnings - Get warnings for the server or user.\n" +
                "/unban - Unban a user, optional reason.\n" +
                "/undeafen - Undeafen a user, optional reason.\n" +
                "/deafen - Deafen a user, optional reason.\n" +
                "/softban - Softban a member (ban and immediate unban to delete user messages).\n" +
                "/reason - Edits reason for case provided. Usage: case, reason.\n" +
                "/modlogs - Gets all priors for user mentioned.\n" +
                "/getcase - Gets info on a case number.\n" +
                "/endgiveaway - Ends the most recent giveaway.\n" +
                "/giveaway - Creates a giveaway. Usage: channel, time, winners, item\n" +
                "/repick - Repicks a winner for most recent giveaway in channel\n" +
                "/userinfo - Returns info of a user -mention needed.\n" +
                "/guildinfo - Returns info about the discord server.\n" +
                "/ping - Gets ping for the bot.\n" +
                "/botinfo - Gets some info about Fred.\n" +
                "/purge - Deleted the number of messages specified.Optional user mention.\n" +
                "/temp - Adds Temp Mod role to a user.Usage: user, time.\n" +
                "/untemp - Removes Temp mod role from a user.\n" +
                "/warn - Warns the user mentioned with reason given.\n" +
                "/mute - Adds Muted role to a user.Usage: user, time, reason.\n" +
                "/unmute - Unmuted the user mentioned.\n" +
                "/kick - Kicks the user mentioned.Reason needed.\n" +
                "/ban - Bans the user mentioned.Reason needed.\n" +
                "**PR2 Staff Member Only**\n" +
                "/promote - Says about a PR2 Promotion. PR2 Admins only.\n" +
                "/updatetoken - Updates token of FredtheG.CactusBot.\n" +
                "**PR2 Discussion Only**\n" +
                "/hint - Tell you the current hint for the artifact location.\n" +
                "/view - Gives you info of a PR2 user, or multiple if each name seperated by | .\n" +
                "/viewid - Same as /view but with user ID instead of username.\n" +
                "/guild - Tells you guild info of the guild named.\n" +
                "/guildid - Tells you guild info for the guild ID given.\n" +
                "/exp - Gives EXP needed to next rank or exp from one rank to another.\n" +
                "/role - Adds/removes one of the joinable roles.Usage: /role <role>\n" +
                "/joinableroles - Lists all joinable roles.\n" +
                "/bans - Gives you info on a PR2 ban, type ban id after command.\n" +
                "/fah - Gives you info on a fah user from Team Jiggmin.\n" +
                "/pop - Tells you total number of users on PR2.\n" +
                "/stats - Gives info about any PR2 server.\n" +
                "/guildmembers - Gets members for the PR2 Guild specified.\n" +
                "/guildmembersid - Gets members for the PR2 guild ID specified.\n" +
                "/hh - Tells you current servers with happy hour on them.\n" +
                "/level - Tells you info about a level.\n" +
                "/verifyguild - Creates a server for your guild if you are owner.\n" +
                "/joinguild - Adds you to the role of the guild you are in if it exists.\n" +
                "/servers - Lists all servers how you see them on PR2.\n" +
                "/staff - Returns all PR2 Staff online is there is any.\n" +
                "**Music Moderator**\n" +
                "/play - Resumes music.\n" +
                "/pause - Pauses music.\n" +
                "/loop - Loops music queue.\n" +
                "/qclear - Clears queue.\n" +
                "/qremove - Remove song from queue.\n" +
                "/come - Brings bot to voice channel.\n" +
                "/forceskip - Skips the current song.\n" +
                "/volume - Set volume of the music.\n" +
                "**Music**\n" +
                "/add - Adds a song to the queue.\n" +
                "/skip - Vote to skip current song.\n" +
                "/np - Displays current playing song.\n" +
                "/queue - Displays song queue.\n" +
                "**DMS Only**\n" +
                "/balance - Get how much money you have or someone else has.\n" +
                "/jackpot - Tells you how much money is in the jackpot.\n" +
                "/leaderboard - Gets users with the highest money.\n" +
                "/lotto - Have a go at winning the jackpot.\n" +
                "/daily - Collect money the day.\n" +
                "/pay - Pay a user money.\n" +
                "/weather - Get weather for a city.\n" +
                "**Everyone**\n" +
                "/help - Tells you commands that you can use for me.\n" +
                "/suggest - Lets you add a suggestion for the suggestions channel.\n" +
                "/verify - Gives you instructions on how to get verified(if you are not).\n";
            embed.Title = "Fred the G. Cactus Commands";
            System.Collections.Generic.IEnumerable<string> parts = help.SplitInParts(2000);
            try
            {
                foreach (string part in parts)
                {
                    embed.Description = part;
                    await Context.User.SendMessageAsync("", false, embed.Build());
                    embed.Title = "";
                }
            }
            catch (Discord.Net.HttpException)
            {
                if (msg != null)
                {
                    await msg.ModifyAsync(x => x.Content = $"{Context.User.Mention} I was unable to send you my commands. To fix this goto Settings --> Privacy & Safety --> And allow direct messages from server members.");
                }
            }
        }
    }
}
