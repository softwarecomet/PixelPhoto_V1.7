using Android.Content;
using Com.Google.Android.Exoplayer2.Source.Ads;
using Com.Google.Android.Exoplayer2.Upstream;
using Java.IO;
using Java.Lang;

namespace PixelPhoto.MediaPlayer
{
    public class AdsController : Object, IAdsLoaderEventListener
    {

        public static Context MainActivity;

        public AdsController(Context activity)
        {
            MainActivity = activity;
        }


        public void OnAdClicked()
        {

        }

        public void OnAdLoadError(AdsMediaSource.AdLoadException p0, DataSpec p1)
        {
           
        }

        public void OnAdLoadError(IOException p0)
        {

        }

        public void OnAdPlaybackState(AdPlaybackState p0)
        {

        }

        public void OnAdTapped()
        {

        }

        public void OnInternalAdLoadError(RuntimeException p0)
        {

        }
    }

}