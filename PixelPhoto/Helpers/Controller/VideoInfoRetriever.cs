﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using YoutubeExtractor;
using IOException = System.IO.IOException;

namespace PixelPhoto.Helpers.Controller
{
    public class VideoInfoRetriever
    {
        // ReSharper disable once UnusedMember.Global
        public static string UrlYoutubeGetVideoInfo = "http://www.youtube.com/get_video_info?&video_id=";

        private static IEnumerable<YoutubeExtractor.VideoInfo> VideoInfos;
        private static YoutubeExtractor.VideoInfo VideoSelected;

        private static string VideoDownloadstring;


        public class VideoInfo
        {
            public string Quality { get; set; }
            public string Videourl { get; set; }
        }

        public static async Task<List<VideoInfo>> Get_Youtube_Video(string url)
        {
            try
            {
                List < VideoInfo > listVideos = new List<VideoInfo>();
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("https://you-link.herokuapp.com/?url="+ url);
                    
                    string json = await response.Content.ReadAsStringAsync();
                  
                    JArray videos = JArray.Parse(json);
                    if (videos.Count >= 0)
                    {
                        foreach (var key in videos)
                        {
                            var quality = key["quality"].ToString();
                            var videourl = key["url"].ToString();
                            VideoInfo vid = new VideoInfo();
                            vid.Videourl = videourl;
                            vid.Quality = quality;
                            listVideos.Add(vid);
                        }
                        
                        return listVideos;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }


        public static async Task<string> GetEmbededVideo(string url)
        {
            try
            {
                if (url.Contains("youtube") || url.Contains("youtu"))
                {
                    VideoInfos =  await DownloadUrlResolver.GetDownloadUrlsAsync(url);
                  
                    if (VideoInfos.Count() > 1)
                    {
                        YoutubeExtractor.VideoInfo video =  VideoInfos.FirstOrDefault(info => info.VideoType == VideoType.Mp4 && info.Resolution == 720);

                        if (video == null)
                            video = VideoInfos.FirstOrDefault(
                                info => info.VideoType == VideoType.Mp4 && info.Resolution == 480);

                        if (video == null)
                            video = VideoInfos.FirstOrDefault(
                                info => info.VideoType == VideoType.Mp4 && info.Resolution == 360);

                        if (video == null)
                            video = VideoInfos.FirstOrDefault(
                                info => info.VideoType == VideoType.Mp4 && info.Resolution == 240);

                        if (video == null)
                            video = VideoInfos.FirstOrDefault(
                                info => info.VideoType == VideoType.Mp4 && info.Resolution == 144);

                        if (video != null)
                        {
                            if (video.RequiresDecryption)
                                DownloadUrlResolver.DecryptDownloadUrl(video);
                            VideoSelected = video;
                            VideoDownloadstring = video.DownloadUrl;

                            return VideoDownloadstring;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        var result = await Get_Youtube_Video(url);
                        if (result != null)
                        {
                            VideoDownloadstring = result[0].Videourl;
                            return VideoDownloadstring;
                        }
                        else
                        {
                            return null;
                        } 
                    } 
                }

                else
                {
                    return null;
                }

            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
                return "Invalid";
            }
        }
    }
}