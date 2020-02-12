using System;
using System.Collections.ObjectModel;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Devs.ReadMoreOptionLib;
using Com.Luseen.Autolinklibrary;
using Newtonsoft.Json;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.Posts
{
    public class GifPostFragment : Fragment
    {
        #region Variables Basic

        private ImageView ImageDisplay;
        private ImageView UserAvatar;
        private TextView Fullname, MoreIcon, ShareIcon, LikeIcon, CommentIcon,Favicon, TypePost, LikeCount, TimeTextView, ViewCommentsButton;
        public TextView CommentCount;
        public AutoLinkTextView Description;
        private RecyclerView CommentRecyclerView;
        private View Mainview; 
        private ReadMoreOption ReadMoreOption;
        public CommentsAdapter CommentsAdapter;
        private string UserId , FullName,Avatar,Type,Json,PostId;
        private SocialIoClickListeners ClickListeners;
        private HomeActivity MainContext;
        #endregion

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainContext = (HomeActivity)Activity;
            HasOptionsMenu = true;
        }
         
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Mainview = inflater.Inflate(Resource.Layout.PixGifPost, container, false);
                 
                Type = Arguments.GetString("type");
                Json = Arguments.GetString("object");
                UserId = Arguments.GetString("userid");
                Avatar = Arguments.GetString("avatar");
                FullName = Arguments.GetString("fullname");
                PostId = Arguments.GetString("postid");

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                AddOrRemoveEvent(true);
                ReadPassedData(Type, Json);
               
                return Mainview;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
         
        #region Functions

        private void InitComponent()
        {
            try
            { 
                ImageDisplay = Mainview.FindViewById<ImageView>(Resource.Id.ImageDisplay);
                Fullname = Mainview.FindViewById<TextView>(Resource.Id.username);
                UserAvatar = Mainview.FindViewById<ImageView>(Resource.Id.userAvatar);
                MoreIcon = Mainview.FindViewById<TextView>(Resource.Id.moreicon);
                LikeIcon = Mainview.FindViewById<TextView>(Resource.Id.Like);
                CommentIcon = Mainview.FindViewById<TextView>(Resource.Id.Comment);

                Favicon = Mainview.FindViewById<TextView>(Resource.Id.fav);
                Description = Mainview.FindViewById<AutoLinkTextView>(Resource.Id.description);
                TimeTextView = Mainview.FindViewById<TextView>(Resource.Id.time_text);
                ViewCommentsButton = Mainview.FindViewById<TextView>(Resource.Id.ViewMoreComment);
                LikeCount = Mainview.FindViewById<TextView>(Resource.Id.Likecount);
                CommentCount = Mainview.FindViewById<TextView>(Resource.Id.Commentcount);
                CommentRecyclerView = Mainview.FindViewById<RecyclerView>(Resource.Id.RecylerComment);
                ShareIcon = Mainview.FindViewById<TextView>(Resource.Id.share);
                TypePost = Mainview.FindViewById<TextView>(Resource.Id.Typepost);

                TextView ViewboxText = Mainview.FindViewById<TextView>(Resource.Id.searchviewbox);
                ViewboxText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, MoreIcon, IonIconsFonts.More);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, CommentIcon, IonIconsFonts.IosChatbubbleOutline);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Favicon, IonIconsFonts.IosStarOutline);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, LikeIcon, IonIconsFonts.IosHeartOutline);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, ShareIcon, IonIconsFonts.IosUndoOutline);

                ReadMoreOption = new ReadMoreOption.Builder(MainContext)
                    .TextLength(200, ReadMoreOption.TypeCharacter)
                    .MoreLabel(MainContext.GetText(Resource.String.Lbl_ReadMore))
                    .LessLabel(MainContext.GetText(Resource.String.Lbl_ReadLess))
                    .MoreLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LessLabelColor(Color.ParseColor(AppSettings.MainColor))
                    .LabelUnderLine(true)
                    .Build(); 

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                Toolbar toolBar = Mainview.FindViewById<Toolbar>(Resource.Id.toolbar);
                MainContext.SetToolBar(toolBar, "");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                //Set Adapter Data 
                CommentsAdapter = new CommentsAdapter(Activity);
                LinearLayoutManager mLayoutManager = new LinearLayoutManager(Activity);
                CommentRecyclerView.SetLayoutManager(mLayoutManager);
                CommentRecyclerView.SetAdapter(CommentsAdapter);
                CommentRecyclerView.NestedScrollingEnabled = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    CommentsAdapter.AvatarClick += CommentsAdapter_AvatarClick;

                    if (!string.IsNullOrEmpty(Json))
                    {
                        var item = JsonConvert.DeserializeObject<PostsObject>(Json);
                        GlideImageLoader.LoadImage(Activity, item.MediaSet[0].File,ImageDisplay, ImageStyle.CenterCrop,ImagePlaceholders.Drawable );

                        ClickListeners = new SocialIoClickListeners(Activity);

                        if (!CommentCount.HasOnClickListeners)
                            CommentCount.Click += (sender, e) => ClickListeners.OnCommentPostClick(new CommentFeedClickEventArgs { View = Mainview, NewsFeedClass = item  }, "GifPost");

                        if (!LikeCount.HasOnClickListeners)
                            LikeCount.Click += (sender, e) => ClickListeners.OnLikedPostClick(new LikeNewsFeedClickEventArgs { View = Mainview, NewsFeedClass = item, LikeButton = LikeCount });
                         
                        if (!LikeIcon.HasOnClickListeners)
                            LikeIcon.Click += (sender, e) => ClickListeners.OnLikeNewsFeedClick(new LikeNewsFeedClickEventArgs { View = Mainview, NewsFeedClass = item, LikeButton = LikeIcon });

                        if (!Favicon.HasOnClickListeners)
                            Favicon.Click += (sender, e) => ClickListeners.OnFavNewsFeedClick(new FavNewsFeedClickEventArgs { NewsFeedClass = item, FavButton = Favicon });

                        if (!UserAvatar.HasOnClickListeners)
                            UserAvatar.Click += (sender, e) => ClickListeners.OnAvatarImageFeedClick(new AvatarFeedClickEventArgs { NewsFeedClass = item, Image = UserAvatar, View = Mainview }, "GifPost");

                        if (!CommentIcon.HasOnClickListeners)
                            CommentIcon.Click += (sender, e) => ClickListeners.OnCommentClick(new CommentFeedClickEventArgs { NewsFeedClass = item, View = Mainview }, "GifPost");

                        if (!ViewCommentsButton.HasOnClickListeners)
                            ViewCommentsButton.Click += (sender, e) => ClickListeners.OnCommentClick(new CommentFeedClickEventArgs { NewsFeedClass = item, View = Mainview }, "GifPost");

                        if (!MoreIcon.HasOnClickListeners)
                            MoreIcon.Click += (sender, e) => ClickListeners.OnMoreClick(new MoreFeedClickEventArgs { NewsFeedClass = item, View = Mainview, IsOwner = item.IsOwner }, false, "GifPost");

                        if (!ShareIcon.HasOnClickListeners)
                            ShareIcon.Click += (sender, e) => ClickListeners.OnShareClick(new ShareFeedClickEventArgs { NewsFeedClass = item, View = Mainview });
                    }
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Menu

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            try
            {
                base.OnCreateOptionsMenu(menu, inflater);
                MainContext.MenuInflater.Inflate(Resource.Menu.Profile_Menu, menu);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    MainContext.FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }


        #endregion

        #region Events

        private void CommentsAdapter_AvatarClick(object sender, AvatarCommentAdapterClickEventArgs e)
        {
            try
            {
                AppTools.OpenProfile(Activity, e.Class.UserId.ToString(), e.Class);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion

        #region Load Data And Get Comment 

        public void ReadPassedData(string type, string json)
        {
            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    if (type == "ExploreAdapter")
                    {
                        var item = JsonConvert.DeserializeObject<PostsObject>(json);
                        string time = Methods.Time.TimeAgo(Convert.ToInt32(item.Time));
                        DisplayData(item.Username, item.Description, item.Avatar, time, item.MediaSet[0]?.File, item.Likes, item.IsLiked, item.IsSaved, item.Votes);

                        if (item.Comments != null && item.Comments.Count > 0)
                        {
                            CommentsAdapter.CommentList = new ObservableCollection<CommentObject>(item.Comments);
                            CommentsAdapter.NotifyDataSetChanged();

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void DisplayData(string fullname, string description, string avatar, string time, string videofile, int likesCount, bool isliked, bool isSaved, int votesCount)
        {
            try
            {
                Fullname.Text = fullname;
                TimeTextView.Text = time;
                GlideImageLoader.LoadImage(Activity,avatar, UserAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                var textSanitizer = new TextSanitizer(Description, Activity);

                if (!string.IsNullOrEmpty(description))
                {
                    ReadMoreOption.AddReadMoreTo(Description, Methods.FunString.DecodeString(description));
                    textSanitizer.Load(Methods.FunString.DecodeString(description));
                }
                else
                    Description.Visibility = ViewStates.Gone;

                CommentIcon.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));

                LikeCount.Text = likesCount + " " + Context.GetText(Resource.String.Lbl_Likes);
                LikeIcon.Tag = "Like";

                if (isliked)
                {
                    LikeIcon.SetTextColor(Color.ParseColor("#ed4856"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, LikeIcon, IonIconsFonts.IosHeart);
                    LikeIcon.Tag = "Liked";
                }

                if (!isSaved)
                {
                    Favicon.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Favicon, IonIconsFonts.IosStarOutline);
                    Favicon.Tag = "Add";
                }
                else
                {
                    Favicon.SetTextColor(Color.ParseColor("#FFCE00"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, Favicon, IonIconsFonts.AndroidStar);
                    Favicon.Tag = "Added";
                }


                CommentCount.Text = votesCount + " " + Context.GetText(Resource.String.Lbl_Comments);

                if (votesCount <= 4)
                    ViewCommentsButton.Visibility = ViewStates.Gone;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion
         
    }
}