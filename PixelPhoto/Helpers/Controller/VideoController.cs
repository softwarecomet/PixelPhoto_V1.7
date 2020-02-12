using Android.Content;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Ext.Ima;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Java.Lang;
using Java.Net;
using PixelPhoto.MediaPlayer;
using System;
using Com.Google.Ads.Interactivemedia.V3.Api;
using Com.Google.Android.Exoplayer2.Source.Ads;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using PixelPhoto.Activities.Posts.page;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Helpers.Controller
{
    public class VideoController : Java.Lang.Object, View.IOnClickListener 
    {
        #region Variables Basic

        private View ActivityContext { get; set; }
        private string ActivityName { get; set; }
        public SimpleExoPlayer Player { get; private set; }
        private ImaAdsLoader ImaAdsLoader;
        private PlayerEvents PlayerListener;
        private PlayerView FullScreenPlayerView;
        private PlayerView SimpleExoPlayerView;
        private FrameLayout MainVideoFrameLayout;
        private PlayerControlView ControlView;
        private ProgressBar LoadingProgressBar;
        private ImageButton VideoPlayButton;
        private ImageButton VideoResumeButton;
        private ImageView FullScreenIcon;
        private FrameLayout FullScreenButton;
        private IMediaSource VideoSource;
        private DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();
        private int ResumeWindow;
        private long ResumePosition;
        private string VideoUrL;

        #endregion Variables Basic

        public VideoController(View activity, string activityName)
        {
            try
            {
                var defaultCookieManager = new CookieManager();
                defaultCookieManager.SetCookiePolicy(CookiePolicy.AcceptOriginalServer);

                ActivityName = activityName;
                ActivityContext = activity;

                Initialize();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void Initialize()
        {
            try
            {
                PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                if (ActivityName != "FullScreen")
                {
                    SimpleExoPlayerView = ActivityContext.FindViewById<PlayerView>(Resource.Id.player_view);
                    SimpleExoPlayerView.SetControllerVisibilityListener(PlayerListener);
                    SimpleExoPlayerView.RequestFocus();

                    //Player initialize
                    ControlView = SimpleExoPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                    PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                    FullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    FullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);
                    VideoPlayButton = ControlView.FindViewById<ImageButton>(Resource.Id.exo_play);
                    VideoResumeButton = ControlView.FindViewById<ImageButton>(Resource.Id.exo_pause);

                    MainVideoFrameLayout = ActivityContext.FindViewById<FrameLayout>(Resource.Id.root);
                    MainVideoFrameLayout.SetOnClickListener(this);

                    LoadingProgressBar = ActivityContext.FindViewById<ProgressBar>(Resource.Id.progress_bar);
                     
                    if (!AppSettings.ShowFullScreenVideoPost)
                    {
                        FullScreenIcon.Visibility = ViewStates.Gone;
                        FullScreenButton.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        if (!FullScreenButton.HasOnClickListeners)
                            FullScreenButton.SetOnClickListener(this); 
                    }
                }
                else
                {
                    FullScreenPlayerView = ActivityContext.FindViewById<PlayerView>(Resource.Id.player_view2);
                    ControlView = FullScreenPlayerView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                    PlayerListener = new PlayerEvents(ActivityContext, ControlView);

                    FullScreenIcon = ControlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                    FullScreenButton = ControlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);
                    VideoPlayButton = ControlView.FindViewById<ImageButton>(Resource.Id.exo_play);
                    VideoResumeButton = ControlView.FindViewById<ImageButton>(Resource.Id.exo_pause);

                    if (!AppSettings.ShowFullScreenVideoPost)
                    {
                        FullScreenIcon.Visibility = ViewStates.Gone;
                        FullScreenButton.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        if (!FullScreenButton.HasOnClickListeners)
                            FullScreenButton.SetOnClickListener(this);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void PlayVideo(string videoUrL)
        {
            try
            {
                if (!string.IsNullOrEmpty(videoUrL))
                {
                    VideoUrL = videoUrL;

                    ReleaseVideo();

                    FullScreenIcon.SetImageDrawable(ActivityContext.Context.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));

                    LoadingProgressBar.Visibility = ViewStates.Visible;

                    Uri url = Uri.Parse(videoUrL);
                    
                    AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                    var trackSelector = new DefaultTrackSelector(trackSelectionFactory);
                    trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder().Build());

                    Player = ExoPlayerFactory.NewSimpleInstance(ActivityContext.Context, trackSelector);

                    // Produces DataSource instances through which media data is loaded.
                    var defaultSource = GetMediaSourceFromUrl(url, "normal");

                    VideoSource = null;

                    if (SimpleExoPlayerView == null)
                        Initialize();

                    //Set Interactive Media Ads 
                    if (PlayerSettings.ShowInteractiveMediaAds)
                        VideoSource = CreateMediaSourceWithAds(defaultSource, PlayerSettings.ImAdsUri);

                    //Set Cache Media Load
                    if (PlayerSettings.EnableOfflineMode)
                    {
                        VideoSource = VideoSource == null ? CreateCacheMediaSource(defaultSource, url) : CreateCacheMediaSource(VideoSource, url);
                    }

                    if (VideoSource == null)
                    {
                        VideoSource = GetMediaSourceFromUrl(url, "normal");

                        SimpleExoPlayerView.Player = Player;
                        Player.Prepare(VideoSource);
                        Player.AddListener(PlayerListener);
                        Player.PlayWhenReady = true;
                    }
                    else
                    {
                        SimpleExoPlayerView.Player = Player;
                        Player.Prepare(VideoSource);
                        Player.AddListener(PlayerListener);
                        Player.PlayWhenReady = true;
                    }
                     
                    bool haveResumePosition = ResumeWindow != C.IndexUnset;
                    if (haveResumePosition)
                        Player.SeekTo(ResumeWindow, ResumePosition);
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void ReleaseVideo()
        {
            try
            {
                if (Player != null)
                {
                    SetStopVideo();

                    Player?.Release();
                    Player = null;

                    //GC Collecter
                    GC.Collect();
                }
                 
                ReleaseAdsLoader(); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void SetStopVideo()
        {
            try
            {
                if (SimpleExoPlayerView.Player != null)
                {
                    if (SimpleExoPlayerView.Player.PlaybackState == Com.Google.Android.Exoplayer2.Player.StateReady)
                    {
                        SimpleExoPlayerView.Player.PlayWhenReady = false;
                    }

                    //GC Collecter
                    GC.Collect();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Video player
        private IMediaSource CreateCacheMediaSource(IMediaSource videoSource, Uri videoUrL)
        {
            try
            {
                if (PlayerSettings.EnableOfflineMode)
                {
                    videoSource = GetMediaSourceFromUrl(videoUrL, "normal");
                    return videoSource;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        private IMediaSource CreateMediaSourceWithAds(IMediaSource videoSource, Uri imAdsUri)
        {
            try
            {
                Player = ExoPlayerFactory.NewSimpleInstance(ActivityContext.Context);
                SimpleExoPlayerView.Player = Player;

                if (ImaAdsLoader == null)
                {
                    var imaSdkSettings = ImaSdkFactory.Instance.CreateImaSdkSettings();
                    imaSdkSettings.AutoPlayAdBreaks = true;
                    imaSdkSettings.DebugMode = true;

                    ImaAdsLoader = new ImaAdsLoader.Builder(ActivityContext.Context)
                        .SetImaSdkSettings(imaSdkSettings)
                        .SetMediaLoadTimeoutMs(30 * 1000)
                        .SetVastLoadTimeoutMs(30 * 1000)
                        .BuildForAdTag(imAdsUri); // here is url for vast xml file

                    ImaAdsLoader.SetPlayer(Player);

                    IMediaSource mediaSourceWithAds = new AdsMediaSource(
                        videoSource,
                        new AdMediaSourceFactory(this),
                        ImaAdsLoader,
                        SimpleExoPlayerView);

                    return mediaSourceWithAds;
                }

                return new AdsMediaSource(videoSource, new AdMediaSourceFactory(this), ImaAdsLoader, SimpleExoPlayerView);
            }
            catch (ClassNotFoundException e)
            {
                Console.WriteLine(e.Message);
                // IMA extension not loaded.
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private class AdMediaSourceFactory : Java.Lang.Object, AdsMediaSource.IMediaSourceFactory
        {
            private readonly VideoController Activity;

            public AdMediaSourceFactory(VideoController activity)
            {
                Activity = activity;
            }

            public IMediaSource CreateMediaSource(Uri uri)
            {
                int type = Util.InferContentType(uri);
                var dataSourceFactory = new DefaultDataSourceFactory(Activity.ActivityContext.Context, Util.GetUserAgent(Activity.ActivityContext.Context, AppSettings.ApplicationName));
                switch (type)
                {
                    case C.TypeDash:
                        return new DashMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    case C.TypeSs:
                        return new SsMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    case C.TypeHls:
                        return new HlsMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    case C.TypeOther:
                        return new ExtractorMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                    default:
                        return new ExtractorMediaSource.Factory(dataSourceFactory).SetTag("Ads").CreateMediaSource(uri);
                }
            }

            public int[] GetSupportedTypes()
            {
                return new[] { C.TypeDash, C.TypeSs, C.TypeHls, C.TypeOther };
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                var mBandwidthMeter = new DefaultBandwidthMeter();
                //DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(ActivityContext.Context, Util.GetUserAgent(ActivityContext.Context, AppSettings.ApplicationName), mBandwidthMeter);
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(ActivityContext.Context, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(ActivityContext.Context, AppSettings.ApplicationName), new DefaultBandwidthMeter()));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(ActivityContext.Context, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(ActivityContext.Context, AppSettings.ApplicationName), null));
                int type = Util.InferContentType(uri, null);
                IMediaSource src;
                switch (type)
                {
                    case C.TypeSs:
                        src = new SsMediaSource.Factory(new DefaultSsChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri);
                        break;
                    case C.TypeDash:
                        src = new DashMediaSource.Factory(new DefaultDashChunkSource.Factory(buildHttpDataSourceFactory), buildHttpDataSourceFactoryNull).SetTag(tag).CreateMediaSource(uri);
                        break;
                    case C.TypeHls:
                        src = new HlsMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                    case C.TypeOther:
                        src = new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                    default:
                        //src = Exception("Unsupported type: " + type); 
                        src = new ExtractorMediaSource.Factory(buildHttpDataSourceFactory).SetTag(tag).CreateMediaSource(uri);
                        break;
                }
                return src;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
   
        private void ReleaseAdsLoader()
        {
            try
            {
                if (ImaAdsLoader != null)
                {
                    ImaAdsLoader.Release();
                    ImaAdsLoader = null;
                    SimpleExoPlayerView.OverlayFrameLayout.RemoveAllViews();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void RestartPlayAfterShrinkScreen()
        {
            try
            {
                SimpleExoPlayerView.Player = null;
                SimpleExoPlayerView.Player = Player;
                SimpleExoPlayerView.Player.PlayWhenReady = true;
                FullScreenIcon.SetImageDrawable(
                    ActivityContext.Context.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_expand));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void PlayFullScreen()
        {
            try
            {
                if (FullScreenPlayerView != null)
                {
                    Player?.AddListener(PlayerListener);
                    FullScreenPlayerView.Player = Player;
                    if (FullScreenPlayerView.Player != null) FullScreenPlayerView.Player.PlayWhenReady = true;
                    FullScreenIcon.SetImageDrawable(ActivityContext.Context.GetDrawable(Resource.Drawable.ic_action_ic_fullscreen_skrink));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion Video player

        public void OnClick(View v)
        {
            try
            {
                if (v.Id == FullScreenIcon.Id || v.Id == FullScreenButton.Id)
                {
                    InitFullscreenDialog();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Event

        //Full Screen
        private void InitFullscreenDialog()
        {
            try
            {
                Player.PlayWhenReady = false;

                Intent intent = new Intent(ActivityContext.Context, typeof(VideoFullScreenActivity));
                intent.PutExtra("videoUrl", VideoUrL);
                //  intent.PutExtra("videoDuration", videoPlayer.Duration.ToString());
                ActivityContext.Context.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion Event
 
    }
}