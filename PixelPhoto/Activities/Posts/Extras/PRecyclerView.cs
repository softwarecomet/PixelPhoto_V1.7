using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Dash;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Source.Smoothstreaming;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.RestCalls;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Java.Util.Concurrent.Atomic;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.page;
using PixelPhoto.Activities.Tabbes;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Activities.Posts.Extras
{
    public class PRecyclerView : RecyclerView
    {
        private static PRecyclerView Instance;

        private enum VolumeState
        {
            On,
            Off
        };

        private FrameLayout MediaContainerLayout;
        private ImageView Thumbnail, PlayControl;
        private PlayerView VideoSurfaceView;
        public SimpleExoPlayer VideoPlayer;

        private View ViewHolderParent;
        private int VideoSurfaceDefaultHeight;
        private int ScreenDefaultHeight;
        private Context MainContext;
        private int PlayPosition = -1;
        private bool IsVideoViewAdded;

        private RecyclerScrollListener MainScrollEvent;
        private NewsFeedAdapter NativeFeedAdapter;

        //private PopupBubble PopupBubbleView;
       
        private VolumeState VolumeStateProvider;

        private CacheDataSourceFactory CacheDataSourceFactory;
        private static SimpleCache Cache;
        private static readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();
        //private ExctractorMediaListener EventLogger;
        private DefaultDataSourceFactory DefaultDataSourceFac;
         
        protected PRecyclerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public PRecyclerView(Context context) : base(context)
        {
            Init(context);
        }

        public PRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public PRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            try
            {
                MainContext = context.ApplicationContext;

                Instance = this;
                 
                HasFixedSize = true;
                SetItemViewCacheSize(20);
                ClearAnimation();


                var display = Context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>().DefaultDisplay;

                var point = new Point();
                display.GetSize(point);
                VideoSurfaceDefaultHeight = point.X;
                ScreenDefaultHeight = point.Y;

                VideoSurfaceView = new PlayerView(MainContext)
                {
                    ResizeMode = AspectRatioFrameLayout.ResizeModeFixedWidth
                };

                //===================== Exo Player ========================
                SetPlayer();

                //=============================================

                MainScrollEvent = new RecyclerScrollListener(MainContext, this);
                AddOnScrollListener(MainScrollEvent);
                AddOnChildAttachStateChangeListener(new ChildAttachStateChangeListener(this));
                MainScrollEvent.LoadMoreEvent += MainScrollEvent_LoadMoreEvent;
                MainScrollEvent.IsLoading = false; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static PRecyclerView GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private bool RunApi;
        public void SetXAdapter(NewsFeedAdapter adapter , bool runApi)
        {
            RunApi = runApi;
            NativeFeedAdapter = adapter;
        }
        //wael
        //public void SetXPopupBubble(PopupBubble popupBubble)
        //{
        //    PopupBubbleView = popupBubble;
        //    PopupBubbleView.SetRecyclerView(this);
        //    PopupBubbleView.SetPopupBubbleListener(new PopupBubbleClickEvent(this));
        //}

        public async Task FetchNewsFeedApiPosts(string typeRun, string offset , string limit )
        {
            try
            {
                if (!RunApi)
                    return;

                var countList = NativeFeedAdapter.PixelNewsFeedList.Count;

                var (respondCode, respondString) = await RequestsAsync.Post.FetchHomePosts(offset, limit);

                if (respondCode != 200 || !(respondString is FetchHomePostsObject Object))
                     Methods.DisplayReportResult(HomeActivity.GetInstance(), respondString);
                else
                {
                    if (respondCode.Equals(200))
                    { 
                        var respondList = Object.PostList.Count;
                        if (respondList > 0)
                        { 
                            Object.PostList.RemoveAll(a => a.MediaSet?.Count == 0 || a.MediaSet == null);
                               
                            foreach (var item in from item in Object.PostList let check = NativeFeedAdapter.PixelNewsFeedList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);

                                var checkType = NativeFeedAdapter.PixelNewsFeedList.LastOrDefault();
                                 
                                if (checkType?.Type == "video" || checkType?.Type == "Video")
                                    CacheVideosFiles(Uri.Parse(item.MediaSet[0].File));

                                if (NativeFeedAdapter.PixelNewsFeedList.Count > 0 && NativeFeedAdapter.PixelNewsFeedList.Count % AppSettings.ShowAdMobNativeCount == 0)
                                {
                                    if (AppSettings.ShowAdMobNative)
                                    { 
                                       
                                        if (checkType?.Type != "AdMob")
                                        {
                                            var adMobBox = new PostsObject
                                            {
                                                Type = "AdMob",
                                            };

                                            NativeFeedAdapter.PixelNewsFeedList.Add(adMobBox);
                                        }  
                                    }
                                }
                                 
                                if (AppSettings.ShowFunding && NativeFeedAdapter.PixelNewsFeedList.Count == 5)
                                {
                                    if (ListUtils.FundingList.Count > 0)
                                    {
                                        NativeFeedAdapter.PixelNewsFeedList.Add(new PostsObject() { Type = "Funding" });
                                    } 
                                }
                                 
                                if (typeRun == "Add")
                                    NativeFeedAdapter.PixelNewsFeedList.Add(item);
                                else
                                    NativeFeedAdapter.PixelNewsFeedList.Insert(0, item);
                            }
                             
                            if (countList > 0)
                            {
                                NativeFeedAdapter.NotifyItemRangeInserted(countList - 1, NativeFeedAdapter.PixelNewsFeedList.Count - countList);
                            }
                            else
                            {
                                NativeFeedAdapter.NotifyDataSetChanged();
                            }
                        }
                        else
                        {
                            if (NativeFeedAdapter.PixelNewsFeedList.Count > 10 && !CanScrollVertically(1))
                                Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short).Show();
                        } 
                    }

                    MainScrollEvent.IsLoading = false;

                    var newsFeedFragment = HomeActivity.GetInstance()?.NewsFeedFragment;
                    if (newsFeedFragment != null)
                        HomeActivity.GetInstance()?.RunOnUiThread(newsFeedFragment.ShowEmptyPage);
                } 
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                Console.WriteLine(e);
            }
        }

        public void PlayVideo(bool isEndOfList, Holders.VideoAdapterViewHolder holderVideoPlayer = null, PostsObject item = null)
        {
            try
            {
                if (VideoPlayer == null)
                    SetPlayer();
                 
                int targetPosition;
                if (!isEndOfList)
                {
                    var startPosition = ((LinearLayoutManager)GetLayoutManager()).FindFirstVisibleItemPosition();
                    var endPosition = ((LinearLayoutManager)GetLayoutManager()).FindLastVisibleItemPosition();

                    if (endPosition - startPosition > 1)
                        endPosition = startPosition + 1;

                    if (startPosition < 0 || endPosition < 0)
                        return;

                    if (startPosition != endPosition)
                    {
                        var startPositionVideoHeight = GetVisibleVideoSurfaceHeight(startPosition);
                        var endPositionVideoHeight = GetVisibleVideoSurfaceHeight(endPosition);
                        targetPosition = startPositionVideoHeight > endPositionVideoHeight ? startPosition : endPosition;
                    }
                    else
                        targetPosition = startPosition;
                }
                else
                    targetPosition = GetAdapter().ItemCount - 1;


                if (targetPosition == PlayPosition)
                    return;

                // set the position of the list-item that is to be played
                PlayPosition = targetPosition;
                if (VideoSurfaceView == null)
                    return;

                VideoSurfaceView.Visibility = ViewStates.Invisible;
                RemoveVideoView(VideoSurfaceView);

                var currentPosition = targetPosition - ((LinearLayoutManager)GetLayoutManager()).FindFirstVisibleItemPosition();

                var child = GetChildAt(currentPosition);
                if (child == null)
                    return;

                dynamic holder;
                if (holderVideoPlayer != null)
                {
                    holder = holderVideoPlayer;
                    targetPosition = holderVideoPlayer.LayoutPosition;
                }
                else
                {
                    Holders.VideoAdapterViewHolder holderChild = (Holders.VideoAdapterViewHolder)child.Tag;
                    if (holderChild == null)
                    {
                        PlayPosition = -1;
                        return;
                    }
                    else
                    {
                        targetPosition = holderChild.LayoutPosition;
                        holder = holderChild;
                    }
                }

                if (!(holder is Holders.VideoAdapterViewHolder holderVideo)) return;
                MediaContainerLayout = holderVideo.MediaContainerLayout;
                Thumbnail = holderVideo.VideoImage;
                

                ViewHolderParent = holderVideo.ItemView;
                PlayControl = holderVideo.PlayControl;
                 
                if (!IsVideoViewAdded)
                    AddVideoView();
                holderVideo.VideoProgressBar.Visibility = ViewStates.Visible;
                VideoSurfaceView.Player = VideoPlayer;

                var controlView = VideoSurfaceView.FindViewById<PlayerControlView>(Resource.Id.exo_controller);
                Uri videoUrl = Uri.Parse(item != null ? item.MediaSet[0].File : NativeFeedAdapter.PixelNewsFeedList[targetPosition].MediaSet[0].File);
                
                //>> Old Code 
                //===================== Exo Player ======================== 
                var lis = new ExoPlayerRecyclerEvent(controlView, this, holderVideo);

                IMediaSource videoSource = GetMediaSourceFromUrl(videoUrl, "normal");

                var dataSpec = new DataSpec(videoUrl); //0, 1000 * 1024, null

                if (Cache == null)
                    CacheVideosFiles(videoUrl);

                //Cache = new SimpleCache(new Java.IO.File(MainContext.FilesDir, "media"), new NoOpCacheEvictor());

                if (CacheDataSourceFactory == null)
                    CacheDataSourceFactory = new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                var counters = new CacheUtil.CachingCounters();

                CacheUtil.GetCached(dataSpec, Cache, counters);
                if (counters.ContentLength == counters.TotalCachedBytes())
                {
                    videoSource = new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(videoUrl);
                }
                else if (counters.TotalCachedBytes() == 0)
                {
                    videoSource = new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(videoUrl);
                    // not cached at all
                    Task.Run(() =>
                    {
                        try
                        {
                            var cacheDataSource = new CacheDataSource(Cache, CacheDataSourceFactory.CreateDataSource());
                            CacheUtil.Cache(dataSpec, Cache, cacheDataSource, counters, new AtomicBoolean());
                            double downloadPercentage = counters.TotalCachedBytes() * 100d / counters.ContentLength;
                            Console.WriteLine(downloadPercentage);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
                else
                {
                    // partially cached
                    videoSource = new ExtractorMediaSource.Factory(CacheDataSourceFactory).CreateMediaSource(videoUrl);
                }

                lis.mFullScreenButton.SetOnClickListener(new NewClicker(lis.mFullScreenButton, videoUrl.ToString(), this));

                VideoPlayer.Prepare(videoSource);
                VideoPlayer.AddListener(lis);
                VideoPlayer.PlayWhenReady = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private class NewClicker : Java.Lang.Object, IOnClickListener
        {
            private string VideoUrl { get; set; }
            private FrameLayout FullScreenButton { get; set; }
            private PRecyclerView WRecyclerViewController { get; set; }
            public NewClicker(FrameLayout fullScreenButton, string videoUrl, PRecyclerView wRecyclerViewController)
            {
                WRecyclerViewController = wRecyclerViewController;
                FullScreenButton = fullScreenButton;
                VideoUrl = videoUrl;
            }
            public void OnClick(View v)
            {
                if (v.Id == FullScreenButton.Id)
                {
                    try
                    { 
                        WRecyclerViewController.VideoPlayer.PlayWhenReady = false;

                        Intent intent = new Intent(WRecyclerViewController.MainContext, typeof(VideoFullScreenActivity));
                        intent.PutExtra("videoUrl", VideoUrl);
                        //  intent.PutExtra("videoDuration", videoPlayer.Duration.ToString());
                        MainApplication.GetInstance().Activity.StartActivity(intent);
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }
         
        private void MainScrollEvent_LoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                MainScrollEvent.IsLoading = true;
                 
                if (NativeFeedAdapter.PixelNewsFeedList.Count <= 5)
                    return;

                var item = NativeFeedAdapter.PixelNewsFeedList.LastOrDefault();
                if (item != null)
                {
                    var lastItem = NativeFeedAdapter.PixelNewsFeedList.IndexOf(item);

                    item = NativeFeedAdapter.PixelNewsFeedList[lastItem];

                    if (item.Type == "AdMob" || item.Type == "Funding")
                    {
                        item = NativeFeedAdapter.PixelNewsFeedList[lastItem - 2];
                    }
                }

                var offset = "0";

                if (item != null)
                    offset = item.PostId.ToString();

                if (!Methods.CheckConnectivity())
                    Toast.MakeText(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short).Show();
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> {() => FetchNewsFeedApiPosts("Add",offset,"25")});
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        
        public class RecyclerScrollListener : OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            public bool IsLoading { get; set; }

            private LinearLayoutManager LayoutManager;
            private readonly PRecyclerView XRecyclerView;

            public RecyclerScrollListener(Context ctx, PRecyclerView recyclerView)
            {
                XRecyclerView = recyclerView;
                IsLoading = false;
            }

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {
                base.OnScrollStateChanged(recyclerView, newState);

                try
                {
                    if (newState == (int) Android.Widget.ScrollState.Idle)
                    {
                        //if (XRecyclerView.Thumbnail != null)
                        //    XRecyclerView.Thumbnail.Visibility = ViewStates.Visible;

                        //if (XRecyclerView.PlayControl != null)
                        //    XRecyclerView.PlayControl.Visibility = ViewStates.Visible;

                        XRecyclerView.PlayVideo(!recyclerView.CanScrollVertically(1));
                    }
                    else
                    {
                        var startPosition = LayoutManager.FindFirstVisibleItemPosition();
                        var endPosition = LayoutManager.FindLastVisibleItemPosition();

                        var startPositionVideoHeight = XRecyclerView.GetVisibleVideoSurfaceHeight(startPosition);
                        var endPositionVideoHeight = XRecyclerView.GetVisibleVideoSurfaceHeight(endPosition);
                        var targetPosition = startPositionVideoHeight > endPositionVideoHeight;
                         
                        if (endPositionVideoHeight - 230 > startPositionVideoHeight)
                            if (XRecyclerView.VideoPlayer.PlayWhenReady)
                                XRecyclerView.VideoPlayer.PlayWhenReady = false;
                        if (startPositionVideoHeight <= 60)
                        {
                            if (XRecyclerView.Thumbnail != null)
                                XRecyclerView.Thumbnail.Visibility = ViewStates.Visible;

                            if (XRecyclerView.PlayControl != null)
                                XRecyclerView.PlayControl.Visibility = ViewStates.Visible;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {

                try
                {
                    if (LayoutManager == null)
                        LayoutManager = (LinearLayoutManager)recyclerView.GetLayoutManager();

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    var pastItems = LayoutManager.FindFirstVisibleItemPosition();

                    if (visibleItemCount + pastItems + 10 < totalItemCount)
                        return;

                    if (IsLoading)
                        return;

                    LoadMoreEvent?.Invoke(this, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #region Listeners
         
        public class ChildAttachStateChangeListener : Java.Lang.Object, IOnChildAttachStateChangeListener
        {
            public PRecyclerView XRecyclerView;

            public ChildAttachStateChangeListener(PRecyclerView recyclerView)
            {
                XRecyclerView = recyclerView;
            }

            public void OnChildViewAttachedToWindow(View view)
            {
                try
                {
                    var mainHolder = XRecyclerView.GetChildViewHolder(view);

                    if (XRecyclerView.ViewHolderParent != null && XRecyclerView.ViewHolderParent.Equals(view))
                    {
                        if (!(mainHolder is Holders.VideoAdapterViewHolder holder))
                            return;

                        if (!XRecyclerView.IsVideoViewAdded)
                            return;

                        XRecyclerView.RemoveVideoView(XRecyclerView.VideoSurfaceView);
                        XRecyclerView.PlayPosition = -1;
                        XRecyclerView.VideoSurfaceView.Visibility = ViewStates.Invisible;
                        holder.VideoImage.Visibility = ViewStates.Visible;
                        holder.PlayControl.Visibility = ViewStates.Visible;
                        holder.VideoProgressBar.Visibility = ViewStates.Gone;

                        XRecyclerView.VideoPlayer?.Stop();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnChildViewDetachedFromWindow(View view)
            {
            }
        }

        private class ExoPlayerRecyclerEvent : Java.Lang.Object, IPlayerEventListener, PlayerControlView.IVisibilityListener
        {  
            private readonly ProgressBar LoadingProgressBar;
            private readonly ImageButton VideoPlayButton, VideoResumeButton;
            public ImageView VolumeControl, mFullScreenIcon;
            public FrameLayout mFullScreenButton;
            private readonly PRecyclerView XRecyclerView;
             
            public ExoPlayerRecyclerEvent(PlayerControlView controlView, PRecyclerView recyclerView,Holders.VideoAdapterViewHolder holder)
            { 
                XRecyclerView = recyclerView;
                if (controlView == null)
                    return;

                mFullScreenIcon = controlView.FindViewById<ImageView>(Resource.Id.exo_fullscreen_icon);
                mFullScreenButton = controlView.FindViewById<FrameLayout>(Resource.Id.exo_fullscreen_button);

                VideoPlayButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_play);
                VideoResumeButton = controlView.FindViewById<ImageButton>(Resource.Id.exo_pause);
                VolumeControl = controlView.FindViewById<ImageView>(Resource.Id.exo_volume_icon);

                if (!AppSettings.ShowFullScreenVideoPost)
                {
                    mFullScreenIcon.Visibility = ViewStates.Gone;
                    mFullScreenButton.Visibility = ViewStates.Gone;
                }

                if (holder.VideoProgressBar != null) LoadingProgressBar = holder.VideoProgressBar;

                SetVolumeControl(XRecyclerView.VolumeStateProvider == VolumeState.On? VolumeState.On: VolumeState.Off);

                if (!VolumeControl.HasOnClickListeners)
                {
                    VolumeControl.Click += (sender, args) => { ToggleVolume(); };
                } 
            }

            private void ToggleVolume()
            {
                try
                {
                    if (XRecyclerView.VideoPlayer == null)
                        return;

                    switch (XRecyclerView.VolumeStateProvider)
                    {
                        case VolumeState.Off:
                            SetVolumeControl(VolumeState.On);
                            break;
                        case VolumeState.On:
                            SetVolumeControl(VolumeState.Off);
                            break;
                        default:
                            SetVolumeControl(VolumeState.Off);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            private void SetVolumeControl(VolumeState state)
            {
                try
                {
                    XRecyclerView.VolumeStateProvider = state;
                    switch (state)
                    {
                        case VolumeState.Off:
                            XRecyclerView.VolumeStateProvider = VolumeState.Off;
                            XRecyclerView.VideoPlayer.Volume = 0f;
                            AnimateVolumeControl();
                            break;
                        case VolumeState.On:
                            XRecyclerView.VolumeStateProvider = VolumeState.On;
                            XRecyclerView.VideoPlayer.Volume = 1f;
                            AnimateVolumeControl();
                            break;
                        default:
                            XRecyclerView.VideoPlayer.Volume = 1f;
                            AnimateVolumeControl();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            private void AnimateVolumeControl()
            { 
                try
                {
                    if (VolumeControl == null)
                        return;

                    VolumeControl.BringToFront();
                    switch (XRecyclerView.VolumeStateProvider)
                    {
                        case VolumeState.Off:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);

                            break;
                        case VolumeState.On:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_up_grey_24dp);
                            break;
                        default:
                            VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                            break;
                    }
                    //VolumeControl.Animate().Cancel();

                    //VolumeControl.Alpha = (1f);

                    //VolumeControl.Animate().Alpha(0f).SetDuration(600).SetStartDelay(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnLoadingChanged(bool p0)
            {
            }

            public void OnPlaybackParametersChanged(PlaybackParameters p0)
            {
            }

            public void OnPlayerError(ExoPlaybackException p0)
            {
            }

            public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
            {
                try
                {
                    if (VideoResumeButton == null || VideoPlayButton == null || LoadingProgressBar == null)
                        return;

                    if (playbackState == Player.StateEnded)
                    {
                        if (playWhenReady == false)
                            VideoResumeButton.Visibility = ViewStates.Visible;
                        else
                        {
                            VideoResumeButton.Visibility = ViewStates.Gone;
                            VideoPlayButton.Visibility = ViewStates.Visible;
                        }

                        LoadingProgressBar.Visibility = ViewStates.Invisible;
                        XRecyclerView.VideoPlayer.SeekTo(0);
                    }
                    else if (playbackState == Player.StateReady)
                    {

                        if (playWhenReady == false)
                        {
                            VideoResumeButton.Visibility = ViewStates.Gone;
                            VideoPlayButton.Visibility = ViewStates.Visible;
                        }
                        else
                        {
                            VideoResumeButton.Visibility = ViewStates.Visible;
                        }


                        LoadingProgressBar.Visibility = ViewStates.Invisible;

                        if (!XRecyclerView.IsVideoViewAdded)
                            XRecyclerView.AddVideoView();
                    }
                    else if (playbackState == Player.StateBuffering)
                    {
                        VideoPlayButton.Visibility = ViewStates.Invisible;
                        LoadingProgressBar.Visibility = ViewStates.Visible;
                        VideoResumeButton.Visibility = ViewStates.Invisible;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }

            public void OnPositionDiscontinuity(int p0)
            {
            }

            public void OnRepeatModeChanged(int p0)
            {
            }

            public void OnSeekProcessed()
            {
            }

            public void OnShuffleModeEnabledChanged(bool p0)
            {
            }

            public void OnTimelineChanged(Timeline p0, Java.Lang.Object p1, int p2)
            {

            }



            public void OnTracksChanged(TrackGroupArray p0, TrackSelectionArray p1)
            {
            }

            public void OnVisibilityChange(int p0)
            {
            }
        }

        #endregion
            
        #region VideoObject player

        private void SetPlayer()
        {
            try
            {
                AdaptiveTrackSelection.Factory trackSelectionFactory = new AdaptiveTrackSelection.Factory();
                var trackSelector = new DefaultTrackSelector(trackSelectionFactory);
                trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder().Build());

                VideoPlayer = ExoPlayerFactory.NewSimpleInstance(MainContext, trackSelector);

                DefaultDataSourceFac = new DefaultDataSourceFactory(MainContext, Util.GetUserAgent(MainContext, AppSettings.ApplicationName), BandwidthMeter);
                VideoSurfaceView.UseController = true;
                VideoSurfaceView.Player = VideoPlayer;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private IMediaSource GetMediaSourceFromUrl(Uri uri, string tag)
        {
            try
            {
                var mBandwidthMeter = new DefaultBandwidthMeter();
                //DefaultDataSourceFactory dataSourceFactory = new DefaultDataSourceFactory(MainContext, Util.GetUserAgent(MainContext, AppSettings.ApplicationName), mBandwidthMeter);
                var buildHttpDataSourceFactory = new DefaultDataSourceFactory(MainContext, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(MainContext, AppSettings.ApplicationName), new DefaultBandwidthMeter()));
                var buildHttpDataSourceFactoryNull = new DefaultDataSourceFactory(MainContext, mBandwidthMeter, new DefaultHttpDataSourceFactory(Util.GetUserAgent(MainContext, AppSettings.ApplicationName), null));
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

        private void CacheVideosFiles(Uri videoUrl)
        {
            try
            {
                if (Cache == null)
                    Cache = new SimpleCache(MainContext.CacheDir, new NoOpCacheEvictor());

                if (CacheDataSourceFactory == null)
                    CacheDataSourceFactory = new CacheDataSourceFactory(Cache, DefaultDataSourceFac);

                var dataSpec = new DataSpec(videoUrl, 0, 3000 * 1024, null); //0, 1000 * 1024, null
                var counters = new CacheUtil.CachingCounters();

                CacheUtil.GetCached(dataSpec, Cache, counters);

                if (counters.ContentLength == counters.TotalCachedBytes())
                {

                }
                else if (counters.TotalCachedBytes() == 0)
                {
                    // not cached at all
                    Task.Run(() =>
                    {
                        try
                        {
                            var cacheDataSource = new CacheDataSource(Cache, CacheDataSourceFactory.CreateDataSource());
                            CacheUtil.Cache(dataSpec, Cache, cacheDataSource, counters, new AtomicBoolean());
                            double downloadPercentage = counters.TotalCachedBytes() * 100d / counters.ContentLength;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    });
                }
                else
                {
                    // just few mb cached
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private int GetVisibleVideoSurfaceHeight(int playPosition)
        {
            try
            {
                var d = (LinearLayoutManager)GetLayoutManager();
                var at = playPosition - d.FindFirstVisibleItemPosition();

                var child = GetChildAt(at);
                if (child == null)
                    return 0;

                int[] location = new int[2];
                child.GetLocationInWindow(location);

                if (location[1] < 0)
                    return location[1] + VideoSurfaceDefaultHeight;

                return ScreenDefaultHeight - location[1];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        private void AddVideoView()
        {
            try
            {
                //var d = MediaContainerLayout.FindViewById<PlayerView>(VideoSurfaceView.Id);
                //if (d == null)
                //{
                MediaContainerLayout.AddView(VideoSurfaceView);
                IsVideoViewAdded = true;
                VideoSurfaceView.RequestFocus();
                VideoSurfaceView.Visibility = ViewStates.Visible;

                //}

                Thumbnail.Visibility = ViewStates.Gone;
                PlayControl.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void RemoveVideoView(PlayerView videoView)
        {
            try
            {
                var parent = (ViewGroup)videoView.Parent;
                if (parent == null)
                    return;

                var index = parent.IndexOfChild(videoView);
                if (index < 0)
                    return;

                parent.RemoveViewAt(index);
                IsVideoViewAdded = false;
                PlayControl.SetOnClickListener(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public void StopVideo()
        {
            try
            {
                if (VideoSurfaceView.Player == null) return;
                if (VideoSurfaceView.Player.PlaybackState == Player.StateReady)
                {
                    VideoSurfaceView.Player.PlayWhenReady = false;
                }

                //GC Collect
                GC.Collect();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void ReleasePlayer()
        {
            try
            {
                if (VideoPlayer != null)
                {
                    VideoPlayer.Release();
                    VideoPlayer = null;
                }

                ViewHolderParent = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion


    }
}