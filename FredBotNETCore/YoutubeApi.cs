using System.Collections.Generic;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System.IO;
using System.Threading;

namespace FredBotNETCore
{
    class YoutubeApi
    {
        private static YouTubeService ytService = Auth();
        static string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "TextFiles");
        private static YouTubeService Auth()
        {
            UserCredential creds;
            using (var stream = new FileStream(Path.Combine(downloadPath, "youtube_client_secret.json"), FileMode.Open, FileAccess.Read))
            {
                creds = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeReadonly },
                    "user",
                    CancellationToken.None,
                    new FileDataStore("YouTubeAPI")
                   ).Result;
            }

            var service = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "YouTubeAPI"
            });

            return service;
        }

        //public static void GetVideoInfo2(YoutubeVideo video)
        //{
        //    var videoRequest = ytService.Videos.List("contentDetails");
        //    videoRequest.Id = video.id;

        //    var response = videoRequest.Execute();
        //    if (response.Items.Count > 0)
        //    {
        //        video.duration = response.Items[0].ContentDetails.Duration;
        //    }
        //    else
        //    {
        //        //Video ID not found
        //    }
        //}

        public static void GetVideoInfo(YoutubeVideo video)
        {
            var videoRequest = ytService.Videos.List("snippet");
            videoRequest.Id = video.id;

            var response = videoRequest.Execute();
            if (response.Items.Count > 0)
            {
                video.title = response.Items[0].Snippet.Title;
                video.description = response.Items[0].Snippet.Description;
                video.publishedDate = response.Items[0].Snippet.PublishedAt.Value;
                video.channel = response.Items[0].Snippet.ChannelTitle;
                //video.duration = response.Items[0].ContentDetails.Duration;
            }
            else
            {
                //Video ID not found
            }
        }

        internal static YoutubeVideo[] GetPlaylist(string playlistId)
        {
            var request = ytService.PlaylistItems.List("contentDetails");
            request.PlaylistId = playlistId;
            LinkedList<YoutubeVideo> videos = new LinkedList<YoutubeVideo>();

            string nextPage = "";

            while (nextPage != null)
            {
                request.PageToken = nextPage;
                var response = request.Execute();
                //int i = 0;

                foreach (var item in response.Items)
                {
                    videos.AddLast(new YoutubeVideo(item.ContentDetails.VideoId));
                }

                nextPage = response.NextPageToken;
            }

            return videos.ToArray();
        }

        public static void GetChannelInfo(YouTubeChannel channel)
        {
            var channelRequest = ytService.Channels.List("snippet");
            channelRequest.Id = channel.id;

            var response = channelRequest.Execute();
            if (response.Items.Count > 0)
            {
                channel.name = response.Items[0].Snippet.Title;
                //channel.subs = response.Items[0].Statistics.SubscriberCount.Value.ToString();
                channel.creationDate = response.Items[0].Snippet.PublishedAt.Value;
                channel.desciption = response.Items[0].Snippet.Description;
                //channel.views = response.Items[0].Statistics.ViewCount.Value.ToString();
            }
            else
            {
                //Channel ID not found
            }
        }
    }
}
