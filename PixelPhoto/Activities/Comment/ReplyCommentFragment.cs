using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Com.Luseen.Autolinklibrary;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Newtonsoft.Json;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Refractored.Controls;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.Comment
{
    public class ReplyCommentFragment : Fragment
    {
        #region  Variables Basic

        private HomeActivity MainContext; 
        private ImageView Image, LikeIcon;
        private AutoLinkTextView CommentText;
        private TextView TimeTextView, LikeCount, ReplyButton; 
        private RelativeLayout RootView;
        private EmojiconEditText EmojIconEditTextView;
        private AppCompatImageView EmojiIcon;
        private CircleButton SendButton;
        private RecyclerView MRecycler;
        private ReplyAdapter MAdapter;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private string CommentId;
        private CommentObject UserinfoComment;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private TextView ViewboxText;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            MainContext = (HomeActivity)Activity;
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.ReplyCommentLayout, container, false);
                 
                UserinfoComment = JsonConvert.DeserializeObject<CommentObject>(Arguments.GetString("CommentObject")); 
                CommentId = Arguments.GetString("CommentId");

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                SendButton.Click += SendButtonOnClick;
                MAdapter.AvatarClick += MAdapterOnAvatarClick; 
                LikeIcon.Click += LikeIconOnClick;

                GetReply();

                return view;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }
         
        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                Image = view.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = view.FindViewById<AutoLinkTextView>(Resource.Id.active);
                LikeIcon = view.FindViewById<ImageView>(Resource.Id.likeIcon);
                TimeTextView = view.FindViewById<TextView>(Resource.Id.time);
                LikeCount = view.FindViewById<TextView>(Resource.Id.Like);
                ReplyButton = view.FindViewById<TextView>(Resource.Id.reply);
                 
                RootView = view.FindViewById<RelativeLayout>(Resource.Id.root);
                EmojiIcon = view.FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojIconEditTextView = view.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                SendButton = view.FindViewById<CircleButton>(Resource.Id.sendButton);
                ViewboxText = view.FindViewById<TextView>(Resource.Id.viewbox);
                ViewboxText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                EmojIconEditTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);
                EmojIconEditTextView.SetHintTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);

                EmojIconActions emojis = new EmojIconActions(Activity, RootView, EmojIconEditTextView, EmojiIcon);
                emojis.ShowEmojIcon();
                  
                if (AppSettings.SetTabDarkTheme)
                    LikeIcon.SetBackgroundResource(Resource.Drawable.Shape_Circle_Black);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar(View view)
        {
            try
            {
                var toolbar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                    MainContext.SetToolBar(toolbar, " ");
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
                MAdapter = new ReplyAdapter(Activity) { ReplyList =  new ObservableCollection<ReplyObject>()};
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    MainContext?.FragmentNavigatorBack();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Events

        private async void SendButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EmojIconEditTextView.Text) || string.IsNullOrWhiteSpace(EmojIconEditTextView.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    //Comment Code 
                    string time = Methods.Time.TimeAgo(DateTime.Now);

                    int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    string time2 = unixTimestamp.ToString();

                    ReplyObject comment = new ReplyObject
                    {
                        Id = unixTimestamp,
                        Text = EmojIconEditTextView.Text, 
                        Avatar = UserDetails.Avatar,
                        Username = UserDetails.Username,
                        UserId = int.Parse(UserDetails.UserId),   
                        IsOwner = true,
                        CommentId = int.Parse(CommentId),
                        IsLiked = 0,
                        Likes = 0,
                        TextTime = time,
                        Time = unixTimestamp.ToString(),
                        UserData = ListUtils.MyProfileList.FirstOrDefault()
                    };
                    MAdapter.ReplyList.Add(comment);
                  
                    var lastItem = MAdapter.ReplyList.IndexOf(MAdapter.ReplyList.Last());
                    if (lastItem > -1)
                    {
                        MAdapter.NotifyItemInserted(lastItem);
                        MRecycler.ScrollToPosition(lastItem);
                    }
                   
                    //Api request  
                    var (respondCode, respondString) = await RequestsAsync.Post.AddReplyComment(CommentId, comment.Text).ConfigureAwait(false);
                    if (respondCode.Equals(200))
                    {
                        if (respondString is  AddReplyObject Object)
                        {
                            var dataComment = MAdapter.ReplyList.FirstOrDefault(a => a.Id == int.Parse(time2));
                            if (dataComment != null)
                            {
                                dataComment = Object.Data;
                                dataComment.Id = Object.Data.Id;
                            }
                                
                            Activity.RunOnUiThread(() =>
                            {
                                try
                                {
                                    MAdapter.NotifyItemChanged(MAdapter.ReplyList.IndexOf(dataComment));
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }
                            });
                        }
                    }
                    else Methods.DisplayReportResult(Activity, respondString);

                    //Hide keyboard
                    EmojIconEditTextView.Text = "";
                    EmojIconEditTextView.ClearFocus();
                }
                else
                {
                    Toast.MakeText(Activity, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void MAdapterOnAvatarClick(object sender, AvatarReplyAdapterClickEventArgs e)
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

        private void LikeIconOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                else
                { 
                    UserinfoComment.IsLiked = !UserinfoComment.IsLiked;

                    LikeIcon.SetImageResource(UserinfoComment.IsLiked  ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);
                    //LikeIcon.StartAnimation(animationScale);

                    RequestsAsync.Post.LikeComment(UserinfoComment.Id.ToString()).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
         
        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = MAdapter.ReplyList.LastOrDefault();
                if (item != null && MAdapter.ReplyList.Count > 10 && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Load Data Api 

        private void GetReply()
        {
            try
            {
                if (UserinfoComment != null)
                {
                    TextSanitizer changer = new TextSanitizer(CommentText, Activity);
                    changer.Load(Methods.FunString.DecodeString(UserinfoComment.Text));

                    TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(UserinfoComment.Time));

                    GlideImageLoader.LoadImage(Activity, UserinfoComment.Avatar, Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                    ReplyButton.Visibility = ViewStates.Invisible;

                    LikeCount.Text = Activity.GetString(Resource.String.Lbl_Likes) + " " + "(" + UserinfoComment.Likes + ")";
                    LikeIcon.SetImageResource(UserinfoComment.IsLiked ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);
                }

                StartApiService();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataAsync(offset) });
        }

        private async Task LoadDataAsync(string offset = "")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                int countList = MAdapter.ReplyList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Post.FetchReplyComment(CommentId, "24", offset);
                if (apiStatus != 200 || !(respond is FetchReplyObject result) || result.Data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = MAdapter.ReplyList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                MAdapter.ReplyList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList - 1, MAdapter.ReplyList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.ReplyList = new ObservableCollection<ReplyObject>(result.Data);
                            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.ReplyList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreComment), ToastLength.Short).Show();
                    }
                }

                Activity.RunOnUiThread(ShowEmptyPage);
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                MainScrollEvent.IsLoading = false;
            }
            MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage()
        {
            try
            {
                MainScrollEvent.IsLoading = false;
                if (MAdapter.ReplyList.Count > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    MRecycler.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoComments);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                MainScrollEvent.IsLoading = false;
                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        #endregion
    }
}