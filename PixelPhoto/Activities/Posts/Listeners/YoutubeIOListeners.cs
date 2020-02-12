using System;
using Android.App;
using Android.Widget;
using Com.Google.Android.Youtube.Player;

namespace PixelPhoto.Activities.Posts.Listeners
{
    public class YoutubeIoListeners: Java.Lang.Object, IYouTubePlayerOnInitializedListener
    {
       private readonly Activity MainContext;
       private IYouTubePlayer YoutubePlayer;
        private readonly string VideoPlayid;

        public YoutubeIoListeners(Activity mainContext , string videoPlayid, IYouTubePlayer iYoutubePlayer )
        {
            MainContext = mainContext;
            VideoPlayid = videoPlayid;
            YoutubePlayer = iYoutubePlayer;
        }

        public void OnInitializationFailure(IYouTubePlayerProvider p0, YouTubeInitializationResult errorReason)
        {
            try
            {
                if (errorReason.IsUserRecoverableError)
                    errorReason.GetErrorDialog(MainContext, 1).Show();
                else
                    Toast.MakeText(MainContext, errorReason.ToString(), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnInitializationSuccess(IYouTubePlayerProvider p0, IYouTubePlayer player, bool p2)
        {
            try
            {
               
                YoutubePlayer = player;
                YoutubePlayer.SetPlayerStyle(YouTubePlayerPlayerStyle.Default);
                YoutubePlayer.LoadVideo(VideoPlayid);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}