using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using Com.Devs.ReadMoreOptionLib;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Activities.Funding;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Gms.Ads;
using Android.Gms.Ads.Formats;
using Android.Graphics.Drawables;
using Android.OS;
using Bumptech.Glide.Load.Engine;
using Java.Lang;
using PixelPhoto.Helpers.Ads;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Tabbes.Adapters
{
    public enum NativeFeedType
    {
        Photo, MultiPhoto, Video, Youtube, Gif, Funding, AdMob
    }

    public class NewsFeedAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider, UnifiedNativeAd.IOnUnifiedNativeAdLoadedListener
    {
        public event EventHandler<Holders.PostAdapterClickEventArgs> ItemClick;
        public event EventHandler<Holders.PostAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        private ReadMoreOption ReadMoreOption { get; set; }
        private readonly SocialIoClickListeners ClickListeners;
        public ObservableCollection<PostsObject> PixelNewsFeedList { get; set; }
        private TextSanitizer TextSanitizerAutoLink { get; set; } 
        private RecyclerView MainRecyclerView { get;  set; }
        private readonly RequestBuilder FullGlideRequestBuilder;
        public Holders.FundingViewHolder HolderFunding { get; set; }

        public NewsFeedAdapter(Activity context, PRecyclerView recyclerView)
        {
            try
            {
                ActivityContext = context;
                MainRecyclerView = recyclerView;

                PreCachingLayoutManager mLayoutManager = new PreCachingLayoutManager(ActivityContext)
                {
                    Orientation = LinearLayoutManager.Vertical
                };

                mLayoutManager.SetPreloadItemCount(5);

                MainRecyclerView.SetLayoutManager(mLayoutManager);
                MainRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                FullGlideRequestBuilder = Glide.With(context).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).SkipMemoryCache(true).Override(550).Placeholder(new ColorDrawable(Color.ParseColor("#efefef")));

                FixedPreloadSizeProvider sizeProvider = new FixedPreloadSizeProvider(10, 10);
                RecyclerViewPreloader<PostsObject> preLoader = new RecyclerViewPreloader<PostsObject>(context, this, sizeProvider, 8);
                MainRecyclerView.AddOnScrollListener(preLoader);

                MainRecyclerView.SetAdapter(this);

                PixelNewsFeedList = new ObservableCollection<PostsObject>();

                ReadMoreOption = new ReadMoreOption.Builder(ActivityContext)
                    .TextLength(200, ReadMoreOption.TypeCharacter)
                    .MoreLabel(ActivityContext.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(ActivityContext.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build();

                ClickListeners = new SocialIoClickListeners(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static NativeFeedType GetPostType(PostsObject item)
        {
            try
            {
                switch (item.Type)
                {
                    case "video":
                    case "Video":
                        return NativeFeedType.Video;
                    case "youtube":
                    case "Youtube":
                    case "EmbedVideo":
                        return NativeFeedType.Youtube;
                    case "gif":
                    case "Gif":
                        return NativeFeedType.Gif;
                    case "Funding":
                        return NativeFeedType.Funding;
                    case "AdMob":
                        return NativeFeedType.AdMob;
                    default:
                        {
                            if (item.Type == "image" || item.Type == "Image")
                            {
                                return item.MediaSet.Count > 1 ? NativeFeedType.MultiPhoto : NativeFeedType.Photo;
                            }
                            else
                            {
                                return NativeFeedType.Photo;
                            }
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return NativeFeedType.Photo;
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                View itemView;

                if (viewType == (int)NativeFeedType.Video)
                {
                    itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_VideoFeed, parent, false);
                    Holders.VideoAdapterViewHolder vh = new Holders.VideoAdapterViewHolder(itemView, OnClick, OnLongClick);
                    return vh;
                }
                else if (viewType == (int)NativeFeedType.Youtube)
                {
                    itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_YoutubeFeed, parent, false);
                    Holders.YoutubeAdapterViewHolder vh = new Holders.YoutubeAdapterViewHolder(itemView, OnClick, OnLongClick);
                    return vh;
                }
                else if (viewType == (int)NativeFeedType.Gif || viewType == (int)NativeFeedType.Photo)
                {
                    itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_PhotoFeed, parent, false);
                    Holders.PhotoAdapterViewHolder vh = new Holders.PhotoAdapterViewHolder(itemView, OnClick, OnLongClick);
                    return vh;
                }
                else if (viewType == (int)NativeFeedType.Funding)
                {
                    itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.ViewModel_HRecyclerView, parent, false);
                    Holders.FundingViewHolder vh = new Holders.FundingViewHolder(ActivityContext, itemView);
                    return vh;
                }
                else if (viewType == (int)NativeFeedType.AdMob)
                {
                    itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.PostType_AdMob, parent, false);
                    Holders.AdMobAdapterViewHolder vh = new Holders.AdMobAdapterViewHolder(itemView);
                    return vh;
                }
                else
                {
                    itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_MultiPhotoFeed, parent, false);
                    Holders.MultiPhotoAdapterViewHolder vh = new Holders.MultiPhotoAdapterViewHolder(itemView, OnClick, OnLongClick);
                    return vh;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override int ItemCount => PixelNewsFeedList?.Count ?? 0;

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {

                if (payloads.Count > 0)
                {
                    if (payloads[0].ToString() == "FundingRefresh")
                    {
                        if (viewHolder is Holders.FundingViewHolder holder)
                        {
                            holder.RefreshData();
                        }
                    }
                    else
                    {
                        base.OnBindViewHolder(viewHolder, position, payloads);
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (position >= 0)
                {
                    int itemViewType = viewHolder.ItemViewType;

                    PostsObject item = PixelNewsFeedList[position];
                    if (item == null)
                        return;

                    item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);

                    switch (itemViewType)
                    {
                        case (int)NativeFeedType.Video:
                            {
                                if (viewHolder is Holders.VideoAdapterViewHolder holder)
                                {
                                    if (item.MediaSet.Count > 0)
                                    {
                                        if (!string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                                        {
                                            FullGlideRequestBuilder.Load(item.MediaSet[0]?.Extra).Into(holder.VideoImage);
                                        }
                                        else
                                        {
                                            var fileName = item.MediaSet[0].File.Split('/').Last();
                                            var fileNameWithoutExtension = fileName.Split('.').First();

                                            item.MediaSet[0].Extra = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtension + ".png";

                                            var vidoePlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");
                                            if (vidoePlaceHolderImage == "File Dont Exists")
                                            {
                                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(ActivityContext, item.MediaSet[0]?.File);
                                                if (bitmapImage != null)
                                                {
                                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDcimImage);
                                                    FullGlideRequestBuilder.Load(bitmapImage).Into(holder.VideoImage);
                                                }
                                                else
                                                { 
                                                    Glide.With(ActivityContext)
                                                        .AsBitmap()
                                                        .Placeholder(Resource.Drawable.blackdefault)
                                                        .Error(Resource.Drawable.blackdefault)
                                                        .Load(item.MediaSet[0]?.File) // or URI/path
                                                        .Into(holder.VideoImage); //image view to set thumbnail to 
                                                }
                                            }
                                            else
                                            {
                                                FullGlideRequestBuilder.Load(vidoePlaceHolderImage).Into(holder.VideoImage);
                                            }
                                        }

                                        //GlideImageLoader.LoadImage(ActivityContext, imageUri,holder.VideoImage, ImageStyle.FitCenter, ImagePlaceholders.Color);
                                    }
                                   
                                    holder.MainView.Tag = holder;

                                    if (AppSettings.SetTabDarkTheme)
                                        holder.MainPost.SetBackgroundResource(Resource.Drawable.Shape_Radius_Black_Btn);
                                    
                                    SetDataDynamicForViewHolder(holder.ItemView, holder.Username, holder.UserAvatar, holder.Description, holder.Likeicon, holder.Favicon, holder.CommentCount,
                                    holder.LikeCount, holder.ViewMoreComment, holder.TimeText, holder.Commenticon, holder.Moreicon, holder.ShareIcon, holder.IsPromoted, item);

                                    if (!holder.PlayControl.HasOnClickListeners)
                                        holder.PlayControl.Click += (sender, args) =>
                                        {
                                            try
                                            {
                                                PRecyclerView.GetInstance()?.PlayVideo(!PRecyclerView.GetInstance().CanScrollVertically(1), holder, item);
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e);
                                            }
                                        };

                                }

                                break;
                            }
                        case (int)NativeFeedType.Youtube:
                            {
                                if (viewHolder is Holders.YoutubeAdapterViewHolder holder)
                                {
                                    if (item.MediaSet.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                                        FullGlideRequestBuilder.Load(item.MediaSet[0]?.Extra).Into(holder.VideoThumble);

                                        //GlideImageLoader.LoadImage(ActivityContext, item.MediaSet[0]?.Extra, holder.VideoThumble, ImageStyle.FitCenter, ImagePlaceholders.Color);
                                    }

                                    if (AppSettings.SetTabDarkTheme)
                                        holder.MainPost.SetBackgroundResource(Resource.Drawable.Shape_Radius_Black_Btn);

                                    SetDataDynamicForViewHolder(holder.ItemView, holder.Username, holder.UserAvatar, holder.Description, holder.Likeicon, holder.Favicon, holder.CommentCount,
                                    holder.LikeCount, holder.ViewMoreComment, holder.TimeText, holder.Commenticon, holder.Moreicon, holder.ShareIcon, holder.IsPromoted, item);


                                    if (!holder.PlayButton.HasOnClickListeners)
                                    {
                                        holder.PlayButton.Click += (sender, args) =>
                                        ClickListeners.OnPlayYoutubeButtonClicked(new YoutubeVideoClickEventArgs
                                        {
                                            Holder = holder,
                                            NewsFeedClass = item,
                                            Position = position,
                                            View = holder.MainView
                                        });
                                    }
                                }

                                break;
                            }

                        case (int)NativeFeedType.Photo:
                            {
                                if (viewHolder is Holders.PhotoAdapterViewHolder holder)
                                {
                                    if (item.MediaSet.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                                        FullGlideRequestBuilder.Load(item.MediaSet[0]?.File).Into(holder.Image);

                                        //GlideImageLoader.LoadImage(ActivityContext, item.MediaSet[0]?.File, holder.Image, ImageStyle.FitCenter, ImagePlaceholders.Color);

                                    }

                                    if (AppSettings.SetTabDarkTheme)
                                        holder.MainPost.SetBackgroundResource(Resource.Drawable.Shape_Radius_Black_Btn);

                                    SetDataDynamicForViewHolder(holder.ItemView, holder.Username, holder.UserAvatar, holder.Description, holder.Likeicon, holder.Favicon, holder.CommentCount,
                                    holder.LikeCount, holder.ViewMoreComment, holder.TimeText, holder.Commenticon, holder.Moreicon, holder.ShareIcon, holder.IsPromoted, item);
                                }

                                break;
                            }

                        case (int)NativeFeedType.Gif:
                            {
                                if (viewHolder is Holders.PhotoAdapterViewHolder holder)
                                {
                                    if (AppSettings.SetTabDarkTheme)
                                        holder.MainPost.SetBackgroundResource(Resource.Drawable.Shape_Radius_Black_Btn);

                                    if (item.MediaSet.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                                        FullGlideRequestBuilder.Load(item.MediaSet[0]?.File).Into(holder.Image);

                                        //GlideImageLoader.LoadImage(ActivityContext, item.MediaSet[0]?.File,holder.Image, ImageStyle.FitCenter, ImagePlaceholders.Color);
                                    }

                                    SetDataDynamicForViewHolder(holder.ItemView, holder.Username, holder.UserAvatar, holder.Description, holder.Likeicon, holder.Favicon, holder.CommentCount,
                                        holder.LikeCount, holder.ViewMoreComment, holder.TimeText, holder.Commenticon, holder.Moreicon, holder.ShareIcon, holder.IsPromoted, item);
                                }

                                break;
                            }

                        case (int)NativeFeedType.MultiPhoto:
                            {
                                if (viewHolder is Holders.MultiPhotoAdapterViewHolder holderMulti)
                                {
                                    List<string> list = item.MediaSet.Select(image => image.File).ToList();

                                    if (AppSettings.SetTabDarkTheme)
                                        holderMulti.MainPost.SetBackgroundResource(Resource.Drawable.Shape_Radius_Black_Btn);

                                    holderMulti.ViewPagerLayout.Adapter = new MultiImagePagerAdapter(ActivityContext, list);

                                    holderMulti.ViewPagerLayout.CurrentItem = 0;
                                    holderMulti.CircleIndicatorView.SetViewPager(holderMulti.ViewPagerLayout);

                                    SetDataDynamicForViewHolder(holderMulti.ItemView, holderMulti.Username, holderMulti.UserAvatar, holderMulti.Description, holderMulti.Likeicon, holderMulti.Favicon, holderMulti.CommentCount, holderMulti.LikeCount,
                                        holderMulti.ViewMoreComment, holderMulti.TimeText, holderMulti.Commenticon, holderMulti.Moreicon, holderMulti.ShareIcon, holderMulti.IsPromoted, item);
                                }

                                break;
                            }

                        case (int)NativeFeedType.Funding:
                            {
                                HolderFunding = viewHolder as Holders.FundingViewHolder;
                                BindFundingModel(HolderFunding, ListUtils.FundingList);
                                break;
                            }

                        case (int)NativeFeedType.AdMob:
                            {
                                Holders.AdMobAdapterViewHolder holder = viewHolder as Holders.AdMobAdapterViewHolder;
                                new Handler(Looper.MainLooper).Post(new Runnable(() =>
                                    {
                                        BindAdMob(holder);
                                    })
                                ); 
                                break;
                            }

                        default:
                            {
                                if (viewHolder is Holders.PhotoAdapterViewHolder holder)
                                {
                                    if (AppSettings.SetTabDarkTheme)
                                        holder.MainPost.SetBackgroundResource(Resource.Drawable.Shape_Radius_Black_Btn);

                                    if (item.MediaSet.Count > 0)
                                    {
                                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                                        FullGlideRequestBuilder.Load(item.MediaSet[0]?.File).Into(holder.Image);
                                        //GlideImageLoader.LoadImage(ActivityContext, item.MediaSet[0]?.File,holder.Image, ImageStyle.FitCenter, ImagePlaceholders.Color);
                                    }

                                    SetDataDynamicForViewHolder(holder.ItemView, holder.Username, holder.UserAvatar, holder.Description, holder.Likeicon, holder.Favicon, holder.CommentCount,
                                        holder.LikeCount, holder.ViewMoreComment, holder.TimeText, holder.Commenticon, holder.Moreicon, holder.ShareIcon, holder.IsPromoted, item);
                                }

                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private TemplateView Template;
        private void BindAdMob(Holders.AdMobAdapterViewHolder holder)
        {
            try
            {
                Template = holder.MianAlert;

                AdLoader.Builder builder = new AdLoader.Builder(holder.MainView.Context, AppSettings.AdAdMobNativeKey);
                builder.ForUnifiedNativeAd(this);

                VideoOptions videoOptions = new VideoOptions.Builder()
                    .SetStartMuted(true)
                    .Build();

                NativeAdOptions adOptions = new NativeAdOptions.Builder()
                    .SetVideoOptions(videoOptions)
                    .Build();

                builder.WithNativeAdOptions(adOptions);

                AdLoader adLoader = builder.WithAdListener(new AdListener()).Build();
                adLoader.LoadAd(new AdRequest.Builder().Build());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnUnifiedNativeAdLoaded(UnifiedNativeAd ad)
        {
            try
            {
                NativeTemplateStyle styles = new NativeTemplateStyle.Builder().Build();

                if (Template.GetTemplateTypeName() == TemplateView.BigTemplate)
                {
                    ActivityContext.RunOnUiThread(() =>
                    {
                        Template.PopulateUnifiedNativeAdView(ad);
                    });
                }
                else
                {
                    Template.SetStyles(styles);
                    ActivityContext.RunOnUiThread(() =>
                    {
                        Template.SetNativeAd(ad);
                    });
                }

                ActivityContext.RunOnUiThread(() =>
                {
                    try
                    {
                        Template.Visibility = ViewStates.Visible;
                   
                        if (AppSettings.SetTabDarkTheme)
                            Template.SetBackgroundResource(Resource.Drawable.Shape_Radius_Black_Btn);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                NativeFeedType type = GetPostType(PixelNewsFeedList[position]);
                return (int)type;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }
         
        private void OnClick(Holders.PostAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(Holders.PostAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public void SetDataDynamicForViewHolder(View itemView, TextView username, ImageView userAvatar, AutoLinkTextView description, TextView likeIcon, TextView favIcon, TextView commentCount, TextView likeCount, TextView viewMoreComments, TextView timeText, TextView commentIcon, TextView moreIcon, TextView shareIcon, TextView isBoostedIcon, PostsObject item)
        {
            try
            {
                if (item == null)
                {
                    return;
                }

                TextSanitizerAutoLink = new TextSanitizer(description, ActivityContext);
                 
                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, userAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                username.Text = item.Username;

                string time = Methods.Time.TimeAgo(Convert.ToInt32(item.Time));
                timeText.Text = time;

                if (!string.IsNullOrEmpty(description.Text))
                {
                    ReadMoreOption.AddReadMoreTo(description, item.Description);
                    TextSanitizerAutoLink.Load(item.Description);
                }
                else
                {
                    description.Visibility = ViewStates.Gone;
                }

                if (item.Boosted == "1")
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, isBoostedIcon, IonIconsFonts.RibbonA);
                    isBoostedIcon.Text += " " + ActivityContext.GetString(Resource.String.Lbl_Promoted);
                    isBoostedIcon.Visibility = ViewStates.Visible;
                }

                likeIcon.Tag = item.IsLiked ? "Like" : "Liked";
                ClickListeners.SetLike(likeIcon);

                favIcon.Tag = item.IsSaved ? "Add" : "Added";
                ClickListeners.SetFav(favIcon);

                commentCount.Text = item.Votes + " " + ActivityContext.GetString(Resource.String.Lbl_Comments);
                likeCount.Text = item.Likes + " " + ActivityContext.GetString(Resource.String.Lbl_Likes);

                if (item.Votes > 0)
                {
                    viewMoreComments.Visibility = ViewStates.Visible;
                    viewMoreComments.Text = ActivityContext.GetString(Resource.String.Lbl_ShowAllComments);
                }
                else
                {
                    viewMoreComments.Visibility = ViewStates.Gone;
                }

                if (!commentCount.HasOnClickListeners)
                {
                    commentCount.Click += (sender, e) => ClickListeners.OnCommentPostClick(new CommentFeedClickEventArgs { View = itemView, NewsFeedClass = item }, "NewsFeedPost");
                }

                if (!likeCount.HasOnClickListeners)
                {
                    likeCount.Click += (sender, e) => ClickListeners.OnLikedPostClick(new LikeNewsFeedClickEventArgs { View = itemView, NewsFeedClass = item, LikeButton = likeCount });
                }

                if (!likeIcon.HasOnClickListeners)
                {
                    likeIcon.Click += (sender, e) => ClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = itemView, NewsFeedClass = item, LikeButton = likeIcon });
                }

                if (!favIcon.HasOnClickListeners)
                {
                    favIcon.Click += (sender, e) => ClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { NewsFeedClass = item, FavButton = favIcon });
                }

                if (!userAvatar.HasOnClickListeners)
                {
                    userAvatar.Click += (sender, e) => ClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { NewsFeedClass = item, Image = userAvatar, View = itemView }, "NewsFeedPost");
                }

                if (!commentIcon.HasOnClickListeners)
                {
                    commentIcon.Click += (sender, e) => ClickListeners.OnCommentClick(new CommentFeedClickEventArgs { NewsFeedClass = item, View = itemView }, "NewsFeedPost");
                }

                if (!viewMoreComments.HasOnClickListeners)
                {
                    viewMoreComments.Click += (sender, e) => ClickListeners.OnCommentClick(new CommentFeedClickEventArgs { NewsFeedClass = item, View = itemView }, "NewsFeedPost");
                }

                if (!moreIcon.HasOnClickListeners)
                {
                    moreIcon.Click += (sender, e) => ClickListeners.OnMoreClick(new MoreFeedClickEventArgs { NewsFeedClass = item, View = itemView, IsOwner = item.IsOwner }, true, "NewsFeedPost");
                }

                if (!shareIcon.HasOnClickListeners)
                {
                    shareIcon.Click += (sender, e) => ClickListeners.OnShareClick(new ShareFeedClickEventArgs { NewsFeedClass = item, View = itemView });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void BindFundingModel(RecyclerView.ViewHolder viewHolder, ObservableCollection<FundingDataObject> list)
        {
            try
            {
                if (!(viewHolder is Holders.FundingViewHolder holder))
                {
                    return;
                }

                if (holder.FundingAdapters?.FundingList?.Count == 0)
                {
                    holder.FundingAdapters.FundingList = new ObservableCollection<FundingDataObject>(list);
                    holder.FundingAdapters.NotifyDataSetChanged();
                }
                else if (holder.FundingAdapters?.FundingList?.Count > 4)
                {
                    holder.AboutMore.Visibility = ViewStates.Visible;
                    holder.AboutMoreIcon.Visibility = ViewStates.Visible;
                }
                else
                {
                    holder.AboutMore.Visibility = ViewStates.Invisible;
                    holder.AboutMoreIcon.Visibility = ViewStates.Invisible;
                }

                if (!holder.AboutMore.HasOnClickListeners)
                {
                    holder.AboutMore.Click += (sender, args) => OpenAllViewer();
                    holder.AboutMoreIcon.Click += (sender, args) => OpenAllViewer();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OpenAllViewer()
        {
            try
            {
                Intent intent = new Intent(ActivityContext, typeof(FundingActivity));
                ActivityContext.StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PixelNewsFeedList[p0];
                if (item == null)
                    return d;

                var type = GetPostType(item);
                switch (type)
                {
                    case NativeFeedType.Video:
                    case NativeFeedType.Youtube:
                        d.Add(item.MediaSet[0]?.Extra);
                        break;
                    case NativeFeedType.Gif:
                    case NativeFeedType.Photo:
                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                        d.Add(item.MediaSet[0]?.Extra);
                        break;
                    case NativeFeedType.MultiPhoto:
                    {
                        foreach (var image in item.MediaSet)
                        {
                            if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            {
                                item.MediaSet[0].Extra = item.MediaSet[0]?.File;
                                image.Extra = image.File;
                            }

                            d.Add(image.Extra);
                        }
                        break;
                    }
                }

                return d;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                List<string> d = new List<string>();
                return d;
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return FullGlideRequestBuilder.Load(p0.ToString());
        }
    }
}