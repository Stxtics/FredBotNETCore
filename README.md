# FredBotNETCore
[![Build Status](https://travis-ci.org/Stxtics/FredBotNETCore.svg?branch=master)](https://travis-ci.org/Stxtics/FredBotNETCore)
[![Coverage Status](https://coveralls.io/repos/github/Stxtics/FredBotNETCore/badge.svg?branch=master)](https://coveralls.io/github/Stxtics/FredBotNETCore?branch=master)
[![Coverity Scan Build Status](https://scan.coverity.com/projects/15878/badge.svg)](https://scan.coverity.com/projects/15878)
[![Discord](https://discordapp.com/api/guilds/528679522707701760/widget.png)](https://discord.gg/kcWBBBj)
# Summary
Fred the G. Cactus Discord bot for Jiggmin's Village and other Platform Racing 2 related servers. Some of the code in this is around a year or two old. A lot of things could be improved but I haven't really got around to doing it.
If you have suggestions for ways I could improve stuff make a pull request that makes the changes, make an issue, or message me on Discord: Stxtics#0001.

If you have come across this and you would like to be in the Jiggmin's Village discord server message me on PR2 (Stxtics) or Discord: Stxtics#0001 asking for an invite to the server, or visit https://jiggmin2.com/forums

## Contributors
- Stxtics (me)

## Commands (Copied from help command of the bot)
### Admin
#### temp
Makes a user temp mod on the server.
#### untemp
Removes temp mod from user mentioned.
#### resetpr2name
Resets a users verified name.
#### addrole
Creates a new role, with optional color and hoist.
#### delrole
Deletes a role.
#### mentionable
Toggle a role being mentionable.
#### rolecolor
Change the color of a role.
#### addjoinablerole
Adds a joinable role.
#### deljoinablerole
Removes a joinable role.
#### addmod
Add a bot moderator or group of moderators.
#### delmod
Remove a bot moderator or group of moderators.
#### listmods
List bot moderators.
#### blacklistword
Blacklist a word from being said in the server.
#### unblacklistword
Unblacklist a word from being said on the server.
#### listblacklistedwords
Lists all the words that are blacklisted from being said on the server.
#### blacklisturl
Blacklist a URL from being said in the server.
#### unblacklisturl
Unblacklist a URL from being said on the server.
#### listblacklistedurls
Lists all the URLs that are blacklisted from being said on the server.
#### addallowedchannel
Add a channel that PR2 commands can be done in.
#### removeallowedchannel
Remove a channel that PR2 commands can be done in.
#### listallowedchannels
Lists all the channels that PR2 commands can be done in.
#### addmusicchannel
Add a channel that Music commands can be done in.
#### removemusicchannel
Remove a channel that Music commands can be done in.
#### listmusicchannels
Lists all the channels that Music commands can be done in.
#### logchannel
Sets the log channel of the server.
#### notificationschannel
Sets the notifications channel of the server.
#### banlogchannel
Sets the ban log channel for the server.
#### setprefix
Set the prefix for commands for the guild.
#### setwelcomemessage
Set the welcome message sent to users when they join the guild.
#### slowmode
Sets the slowmode for the channel
#### lock
Lock the current channel so members cannot message in it.
#### unlock
Unlock the current channel so members can message in it.

### Moderator
#### setnick
Change the nickname of a user.
#### nick
Change the bots nickname.
#### updatetoken
Updates token for FredtheG.CactusBot.
#### notifymacroers
Let macroers know the servers have restarted.
#### blacklistmusic
Blacklist a user from using music commands.
#### unblacklistmusic
Unblacklist a user from using music commands.
#### listblacklistedmusic
Lists blacklisted users from music commands.
#### blacklistsuggestions
Blacklist a user from using the /suggest command.
#### unblacklistsuggestions
Unblacklist a user from using the /suggest command.
#### listblacklistedsuggestions
Lists blacklisted users from suggestions.
#### channelinfo
Gets information about a channel.
#### membercount
Get the server member count.
#### uptime
Gets the bot uptime.
#### roleinfo
Get information about a role.
#### roles
Get a list of server roles and member counts.
#### warnings
Get warnings for the server or user.
#### unban
Unban a member.
#### undeafen
Undeafen a member.
#### deafen
Deafen a member.
#### softban
Softban a member (ban and immediate unban to delete user messages).
#### getcase
Get info on a case.
#### modlogs
Get a list of mod logs for a user.
#### reason
Edit a reason for a mod log.
#### warn
Warn a member.
#### endgiveaway
Ends the giveaway.
#### repick
Repicks giveaway winner.
#### giveaway
Create a giveaway.
#### promote
Announces user promoted to temp/trial/perm mod on PR2.
#### unmute
Unmutes a user.
#### ping
Returns the latency between the bot and Discord.
#### botinfo
Shows all bot info.
#### userinfo
Returns information about a user.
#### guildinfo
Information about current server.
#### purge
Deletes number of messages specified, optional user mention.
#### kick
Kick a user.
#### ban
Bans a user, needs reason.
#### mute
Mutes a user.

### Music
#### add
Adds a song to play.
#### queue
Displays song queue.
#### loop
Toggles looping of the queue.
#### pause
pauses the music
#### resume
Resumes play of music or adds another song.
#### play
Resumes play of music or adds another song.
#### qremove
Remove an item from the queue.
#### qclear
Removes all songs from the queue
#### come
Brings bot to voice channel
#### skip
Votes to skip current song
#### forceskip
Skips the current song.
#### np
Displays current song playing.
#### stop
Stops the music and makes bot leave voice channel.
#### setvolume
Sets volume of the music
#### seek
Goes to a certain point in a song.
### Public
#### pay
Pay money to another user.
#### balance
See how much money you have.
#### jackpot
See how much money is in the jackpot.
#### leaderboard
Shows users with the most money.
#### lotto
Have a go at winning the jackpot.
#### daily
Collect daily cash.
#### verify
Link your PR2 account to your Discord account.
#### verifycomplete
Let Fred know you've sent the PM.
#### suggest
Suggest something for the Discord server.
#### weather
Get weather at a location.
#### hint
Get the current artifact hint.
#### view
View information on a PR2 account.
#### viewid
View infomation on a PR2 account by its ID.
#### guild
Get details on a PR2 guild.
#### guildid
Get details on a PR2 guild by its ID.
#### exp
Get EXP needed from one PR2 rank to another.
#### role
Add or remove yourself from a joinable role.
#### joinableroles
Lists all joinable roles.
#### topguilds
Returns current top 10 guilds on pr2.
#### f@h
Get how many points someone has on Folding @ Home.
#### bans
Gets a PR2 Ban with ID from https://pr2hub.com/bans/
#### pop
Tells you the number of users on pr2. Does not include private servers.
#### stats
Gets stats of a server on PR2.
#### guildmembers
Gets members of a PR2 guild.
#### guildmembersid
Gets members of a PR2 guild by ID.
#### hh
Returns a list of servers (if any) with happy hour on them.
#### level
Gets info about a PR2 level.
#### campaign
Get levels on a page of Campaign.
#### alltimebest
Get levels on a page of All Time Best.
#### todaysbest
Get levels on a page of Today's Best.
#### newest
Get levels on a page of Newest.
#### servers
Shows all servers and population.
#### staff
Returns a list of the current online PR2 Staff.
