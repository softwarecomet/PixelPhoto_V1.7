using System;
using Android.App;
using Android.Graphics;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using Com.Luseen.Autolinklibrary;
using Me.Relex;
using PixelPhoto.Activities.Funding.Adapters;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Fonts;

namespace PixelPhoto.Activities.Posts.Adapters
{
    public class Holders
    {
        public class PostAdapterClickEventArgs : EventArgs
        {
            public View View { get;  set; }
            public int Position { get;  set; }
        }


        #region ViewHolder
         
        public class VideoAdapterViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public View MainView { get; private set; }
            public LinearLayout MainPost { get; private set; }
            public ImageView UserAvatar { get;private set; }
            public TextView Username { get;private set; }
            public TextView Moreicon { get;private set; }
            public TextView Likeicon { get;private set; }
            public TextView Commenticon { get;private set; }
            public TextView Favicon { get;private set; }
            public AutoLinkTextView Description { get;private set; }
            public TextView PlayButton { get;private set; }
            public TextView TimeText { get;private set; }
            public TextView CommentCount { get;private set; }
            public TextView ViewMoreComment { get;private set; }
            public TextView LikeCount { get;private set; }
            public TextView TypePost { get;private set; }
            public TextView ShareIcon { get;private set; }
            public TextView IsPromoted { get;private set; }
            

            public ImageView VideoImage { get;private set; }
            public FrameLayout MediaContainerLayout { get;private set; }
            public ProgressBar VideoProgressBar { get;private set; }
            public ImageView PlayControl { get;private set; }

          

            #endregion Variables Basic

            public VideoAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Video";

                    MainPost = MainView.FindViewById<LinearLayout>(Resource.Id.mainpost);
                    //MainPost.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.Shape_Radius_Black_Btn : Resource.Drawable.Shape_Radius_White_Btn);

                    
                    TimeText = MainView.FindViewById<TextView>(Resource.Id.time_text);
                    CommentCount = MainView.FindViewById<TextView>(Resource.Id.Commentcount);
                    ViewMoreComment = MainView.FindViewById<TextView>(Resource.Id.ViewMoreComment);
                    LikeCount = MainView.FindViewById<TextView>(Resource.Id.Likecount);

                    Username = MainView.FindViewById<TextView>(Resource.Id.username);
                    UserAvatar = MainView.FindViewById<ImageView>(Resource.Id.userAvatar);
                
                    Moreicon = MainView.FindViewById<TextView>(Resource.Id.moreicon);
                    Likeicon = MainView.FindViewById<TextView>(Resource.Id.Like);
                    Commenticon = MainView.FindViewById<TextView>(Resource.Id.Comment);
                    PlayButton = MainView.FindViewById<TextView>(Resource.Id.playbutton);
                    Favicon = MainView.FindViewById<TextView>(Resource.Id.fav);
                    Description = MainView.FindViewById<AutoLinkTextView>(Resource.Id.description);
                    IsPromoted = MainView.FindViewById<TextView>(Resource.Id.promoted);

                    ShareIcon = MainView.FindViewById<TextView>(Resource.Id.share);
                    TypePost = MainView.FindViewById<TextView>(Resource.Id.Typepost);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TypePost, IonIconsFonts.IosVideocam);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, PlayButton, IonIconsFonts.Play);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Moreicon, IonIconsFonts.More);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Commenticon, IonIconsFonts.IosChatbubbleOutline);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Likeicon, IonIconsFonts.IosHeartOutline);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Favicon, IonIconsFonts.IosStarOutline);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIcon, IonIconsFonts.IosUndoOutline);

                    Commenticon.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));

                    Favicon.Tag = "Add";

                    MediaContainerLayout = MainView.FindViewById<FrameLayout>(Resource.Id.media_container);
                    VideoImage = MainView.FindViewById<ImageView>(Resource.Id.image);
                    VideoProgressBar = MainView.FindViewById<ProgressBar>(Resource.Id.progressBar);
                    PlayControl = MainView.FindViewById<ImageView>(Resource.Id.Play_control);

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public class YoutubeAdapterViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public View MainView { get; private set; }


            public LinearLayout MainPost { get; private set; }
            public ImageView UserAvatar { get;private set; }
            public TextView PlayButton { get;private set; }
            public ImageView VideoThumble { get;private set; }
            public TextView Username { get;private set; }
            public TextView Moreicon { get;private set; }
            public TextView Likeicon { get;private set; }
            public TextView Commenticon { get;private set; }
            public TextView Favicon { get;private set; }
            public AutoLinkTextView Description { get;private set; }
            public FrameLayout InflaterYoutube { get;private set; }
            public TextView TimeText { get;private set; }
            public TextView CommentCount { get;private set; }
            public TextView ViewMoreComment { get;private set; }
            public TextView LikeCount { get;private set; }
            public TextView IsPromoted { get;private set; }

            public TextView TypePost { get;private set; }
            public TextView ShareIcon { get;private set; }

            public YouTubePlayerSupportFragment SupportFragment { get;private set; }
            public IYouTubePlayer YoutubePlayer { get;private set; }

            #endregion Variables Basic

            public YoutubeAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Video";

                    MainPost = MainView.FindViewById<LinearLayout>(Resource.Id.mainpost);
                    //MainPost.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.Shape_Radius_Black_Btn : Resource.Drawable.Shape_Radius_White_Btn);


                    TimeText = MainView.FindViewById<TextView>(Resource.Id.time_text);
                    CommentCount = MainView.FindViewById<TextView>(Resource.Id.Commentcount);
                    ViewMoreComment = MainView.FindViewById<TextView>(Resource.Id.ViewMoreComment);
                    LikeCount = MainView.FindViewById<TextView>(Resource.Id.Likecount);
                    InflaterYoutube = MainView.FindViewById<FrameLayout>(Resource.Id.viewStub);
                    Username = MainView.FindViewById<TextView>(Resource.Id.username);
                    UserAvatar = MainView.FindViewById<ImageView>(Resource.Id.userAvatar);
                    PlayButton = MainView.FindViewById<TextView>(Resource.Id.playbutton);
                    VideoThumble = MainView.FindViewById<ImageView>(Resource.Id.videoimage);
                    Moreicon = MainView.FindViewById<TextView>(Resource.Id.moreicon);
                    Likeicon = MainView.FindViewById<TextView>(Resource.Id.Like);
                    Commenticon = MainView.FindViewById<TextView>(Resource.Id.Comment);
                    Favicon = MainView.FindViewById<TextView>(Resource.Id.fav);
                    Description = MainView.FindViewById<AutoLinkTextView>(Resource.Id.description);
                    ShareIcon = MainView.FindViewById<TextView>(Resource.Id.share);
                    TypePost = MainView.FindViewById<TextView>(Resource.Id.Typepost);
                    IsPromoted = MainView.FindViewById<TextView>(Resource.Id.promoted);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TypePost, IonIconsFonts.SocialYoutube);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, PlayButton, IonIconsFonts.Play);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Moreicon, IonIconsFonts.More);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Commenticon, IonIconsFonts.IosChatbubbleOutline);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Likeicon, IonIconsFonts.IosHeartOutline);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Favicon, IonIconsFonts.IosStarOutline);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIcon, IonIconsFonts.IosUndoOutline);

                    Commenticon.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));

                    Favicon.Tag = "Add";
                     
                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public class PhotoAdapterViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public View MainView { get; private set; }


            public LinearLayout MainPost { get; private set; }
            public ImageView UserAvatar { get;private set; }
            public ImageView Image { get;private set; }

            public TextView Username { get;private set; }
            public TextView Moreicon { get;private set; }
            public TextView Likeicon { get;private set; }
            public TextView Commenticon { get;private set; }
            public TextView Favicon { get;private set; }
            public AutoLinkTextView Description { get;private set; }

            public TextView TimeText { get;private set; }
            public TextView CommentCount { get;private set; }
            public TextView ViewMoreComment { get;private set; }
            public TextView LikeCount { get;private set; }
            public TextView IsPromoted { get;private set; }
            public TextView TypePost { get;private set; }
            public TextView ShareIcon { get;private set; }
            #endregion Variables Basic

            public PhotoAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Image";

                    MainPost = MainView.FindViewById<LinearLayout>(Resource.Id.mainpost);
                   // MainPost.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.Shape_Radius_Black_Btn : Resource.Drawable.Shape_Radius_White_Btn);


                    TimeText = MainView.FindViewById<TextView>(Resource.Id.time_text);
                    CommentCount = MainView.FindViewById<TextView>(Resource.Id.Commentcount);
                    ViewMoreComment = MainView.FindViewById<TextView>(Resource.Id.ViewMoreComment);
                    LikeCount = MainView.FindViewById<TextView>(Resource.Id.Likecount);


                    Username = MainView.FindViewById<TextView>(Resource.Id.username);
                    UserAvatar = MainView.FindViewById<ImageView>(Resource.Id.userAvatar);
                    Image = MainView.FindViewById<ImageView>(Resource.Id.image);
                    Moreicon = MainView.FindViewById<TextView>(Resource.Id.moreicon);

                    Likeicon = MainView.FindViewById<TextView>(Resource.Id.Like);
                    Commenticon = MainView.FindViewById<TextView>(Resource.Id.Comment);
                    Favicon = MainView.FindViewById<TextView>(Resource.Id.fav);

                    Description = MainView.FindViewById<AutoLinkTextView>(Resource.Id.description);
                    IsPromoted = MainView.FindViewById<TextView>(Resource.Id.promoted);
                    ShareIcon = MainView.FindViewById<TextView>(Resource.Id.share);
                    TypePost = MainView.FindViewById<TextView>(Resource.Id.Typepost);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TypePost, IonIconsFonts.Image);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIcon, IonIconsFonts.IosUndoOutline);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Moreicon, IonIconsFonts.More);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Likeicon, IonIconsFonts.IosHeartOutline);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Commenticon, IonIconsFonts.IosChatbubbleOutline);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Favicon, IonIconsFonts.IosStarOutline);

                    Commenticon.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));

                    Favicon.Tag = "Add";

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public class MultiPhotoAdapterViewHolder : RecyclerView.ViewHolder
        {
            #region Variables Basic

            public View MainView { get; private set; }


            public LinearLayout MainPost { get; private set; }
            public ImageView UserAvatar { get;private set; }
            public ViewPager ViewPagerLayout { get;private set; }
            public CircleIndicator CircleIndicatorView { get;private set; }
            public TextView Username { get;private set; }
            public TextView Moreicon { get;private set; }
            public TextView Likeicon { get;private set; }
            public TextView Commenticon { get;private set; }
            public TextView Favicon { get;private set; }
            public AutoLinkTextView Description { get;private set; }

            public TextView TimeText { get;private set; }
            public TextView CommentCount { get;private set; }
            public TextView ViewMoreComment { get;private set; }
            public TextView LikeCount { get;private set; }
            public TextView IsPromoted { get;private set; }
            public TextView TypePost { get;private set; }
            public TextView ShareIcon { get;private set; }
            #endregion Variables Basic

            public MultiPhotoAdapterViewHolder(View itemView, Action<PostAdapterClickEventArgs> clickListener, Action<PostAdapterClickEventArgs> longClickListener) : base(itemView)
            {
                try
                {
                    MainView = itemView;
                    itemView.Tag = "Image";

                    MainPost = MainView.FindViewById<LinearLayout>(Resource.Id.mainpost);
                   // MainPost.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.Shape_Radius_Black_Btn : Resource.Drawable.Shape_Radius_White_Btn);


                    TimeText = MainView.FindViewById<TextView>(Resource.Id.time_text);
                    CommentCount = MainView.FindViewById<TextView>(Resource.Id.Commentcount);
                    ViewMoreComment = MainView.FindViewById<TextView>(Resource.Id.ViewMoreComment);
                    LikeCount = MainView.FindViewById<TextView>(Resource.Id.Likecount);


                    Username = MainView.FindViewById<TextView>(Resource.Id.username);
                    UserAvatar = MainView.FindViewById<ImageView>(Resource.Id.userAvatar);
                    ViewPagerLayout = MainView.FindViewById<ViewPager>(Resource.Id.pager);
                    CircleIndicatorView = MainView.FindViewById<CircleIndicator>(Resource.Id.indicator);
                    Moreicon = MainView.FindViewById<TextView>(Resource.Id.moreicon);

                    Likeicon = MainView.FindViewById<TextView>(Resource.Id.Like);
                    Commenticon = MainView.FindViewById<TextView>(Resource.Id.Comment);
                    Favicon = MainView.FindViewById<TextView>(Resource.Id.fav);

                    Description = MainView.FindViewById<AutoLinkTextView>(Resource.Id.description);
                    IsPromoted = MainView.FindViewById<TextView>(Resource.Id.promoted);
                    ShareIcon = MainView.FindViewById<TextView>(Resource.Id.share);
                    TypePost = MainView.FindViewById<TextView>(Resource.Id.Typepost);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, TypePost, IonIconsFonts.Images);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIcon, IonIconsFonts.IosUndoOutline);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Moreicon, IonIconsFonts.More);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Likeicon, IonIconsFonts.IosHeartOutline);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Commenticon, IonIconsFonts.IosChatbubbleOutline);
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Favicon, IonIconsFonts.IosStarOutline);

                    Commenticon.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));

                    Favicon.Tag = "Add";

                    //Create an Event
                    itemView.Click += (sender, e) => clickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                    itemView.LongClick += (sender, e) => longClickListener(new PostAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public class FundingViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public RecyclerView StoryRecyclerView { get; private set; }
            public FundingAdapters  FundingAdapters { get; private set; }
            public TextView AboutHead { get; private set; }
            public TextView AboutMore { get; private set; }
            public TextView AboutMoreIcon { get; private set; }

            public RelativeLayout MainLinear { get; private set; }

            public FundingViewHolder(Activity activity, View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    MainLinear = (RelativeLayout)MainView.FindViewById(Resource.Id.mainLinear);

                    StoryRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.Recyler);
                    AboutHead = MainView.FindViewById<TextView>(Resource.Id.headText);
                    AboutMore = MainView.FindViewById<TextView>(Resource.Id.moreText);
                    AboutMoreIcon = MainView.FindViewById<TextView>(Resource.Id.icon);

                    if (FundingAdapters != null)
                        return;

                    //MainLinear.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, AboutMoreIcon, IonIconsFonts.ChevronRight);

                    StoryRecyclerView.SetLayoutManager(new LinearLayoutManager(itemView.Context, LinearLayoutManager.Horizontal, false));
                    FundingAdapters = new FundingAdapters(activity);
                    StoryRecyclerView.SetAdapter(FundingAdapters);
                    FundingAdapters.ItemClick += HomeActivity.GetInstance().FundingAdaptersOnItemClick;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
             
            public void RefreshData()
            {
                FundingAdapters.NotifyDataSetChanged();
            }
        }
         
        public class AdMobAdapterViewHolder : RecyclerView.ViewHolder
        {
            public View MainView { get; private set; }
            public TemplateView MianAlert { get; private set; }

            public AdMobAdapterViewHolder(View itemView) : base(itemView)
            {
                try
                {
                    MainView = itemView;

                    MianAlert = MainView.FindViewById<TemplateView>(Resource.Id.my_template); 
                    MianAlert.Visibility = ViewStates.Gone;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #endregion
    }
}