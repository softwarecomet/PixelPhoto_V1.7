using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Posts;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Fragments;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;


namespace PixelPhoto.Activities.Comment
{ 
    public class CommentsFragment : Fragment
    {
        #region Variables Basic

        private View MainView;
        private RelativeLayout RootView;
        private EmojiconEditText EmojiconEditTextView;
        private AppCompatImageView Emojiicon;
        private CircleButton SendButton;
        private RecyclerView CommentRecyclerView;
        private SwipeRefreshLayout XSwipeRefreshLayout;
        private LinearLayoutManager MLayoutManager;
        private CommentsAdapter CommentsAdapter; 
        private ProgressBar ProgressBarLoader;
        private ViewStub EmptyStateLayout;
        private View Inflated;  
        private RecyclerViewOnScrollListener MainScrollEvent;
        private string PostId ,FragmentName;
        private TextView ViewboxText;
        private HomeActivity MainContext;

        #endregion
         
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            MainContext = (HomeActivity) Activity;
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                PostId = Arguments.GetString("postid");
                FragmentName = Arguments.GetString("PrevFragment");

                Activity.Window.SetSoftInputMode(SoftInput.AdjustResize);

                MainView = inflater.Inflate(Resource.Layout.PixCommentsFragment, container, false);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                SendButton.Click += SendButton_Click;
                XSwipeRefreshLayout.Refresh += XSwipeRefreshLayoutOnRefresh;
                CommentsAdapter.AvatarClick += CommentsAdapter_AvatarClick;

                //Get Data Api
                StartApiService();

                return MainView;
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
                RootView = MainView.FindViewById<RelativeLayout>(Resource.Id.root);
                Emojiicon = MainView.FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojiconEditTextView = MainView.FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                SendButton = MainView.FindViewById<CircleButton>(Resource.Id.sendButton);
                CommentRecyclerView = MainView.FindViewById<RecyclerView>(Resource.Id.recyler);
                XSwipeRefreshLayout = MainView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                ProgressBarLoader = MainView.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
                EmptyStateLayout = MainView.FindViewById<ViewStub>(Resource.Id.viewStub);
                ViewboxText = MainView.FindViewById<TextView>(Resource.Id.viewbox);
                ViewboxText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                XSwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                XSwipeRefreshLayout.Refreshing = false;
                XSwipeRefreshLayout.Enabled = true;
                XSwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                 
                ProgressBarLoader.Visibility = ViewStates.Visible;

                EmojiconEditTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);
                EmojiconEditTextView.SetHintTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);

                EmojIconActions emojis = new EmojIconActions(Activity, RootView, EmojiconEditTextView, Emojiicon);
                emojis.ShowEmojIcon(); 
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
                CommentsAdapter = new CommentsAdapter(Activity);
                MLayoutManager = new LinearLayoutManager(Activity);
                CommentRecyclerView.SetLayoutManager(MLayoutManager);
                CommentRecyclerView.SetAdapter(CommentsAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                CommentRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                Toolbar toolBar = MainView.FindViewById<Toolbar>(Resource.Id.toolbar);
                MainContext.SetToolBar(toolBar, " ");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion

        #region Events

        //Event Open Profile User
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

        private void XSwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                CommentsAdapter.CommentList.Clear();
                CommentsAdapter.NotifyDataSetChanged();

                StartApiService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Add New Comment 
        private async void SendButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(EmojiconEditTextView.Text) || string.IsNullOrWhiteSpace(EmojiconEditTextView.Text))
                    return;

                if (Methods.CheckConnectivity())
                {
                    CommentRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;

                    //Comment Code 
                    string time = Methods.Time.TimeAgo(DateTime.Now);

                    int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    string time2 = unixTimestamp.ToString();

                    CommentObject comment = new CommentObject
                    {
                        Text = EmojiconEditTextView.Text,
                        TimeText = time,
                        Avatar = UserDetails.Avatar,
                        Username = UserDetails.Username,
                        UserId = int.Parse(UserDetails.UserId),
                        PostId = int.Parse(PostId), 
                        Id = int.Parse(time2),
                        IsOwner = true,
                        Name = UserDetails.FullName,
                        Likes = "0",
                        Replies = 0,
                        IsLiked = false,
                    };
                    CommentsAdapter.CommentList.Add(comment);

                    var lastItem = CommentsAdapter.CommentList.IndexOf(CommentsAdapter.CommentList.Last());
                    if (lastItem > -1)
                    {
                        CommentsAdapter.NotifyItemInserted(lastItem);
                        CommentRecyclerView.ScrollToPosition(lastItem);
                    }
                     
                    //Api request  
                    var (respondCode, respondString) = await RequestsAsync.Post.AddComment(PostId, EmojiconEditTextView.Text).ConfigureAwait(false);
                    if (respondCode.Equals(200))
                    {
                        if (respondString is AddCommentObject Object)
                        {
                            var dataComment = CommentsAdapter.CommentList.FirstOrDefault(a => a.Id == int.Parse(time2));
                            if (dataComment != null)
                                dataComment.Id = Object.Id;

                            Activity.RunOnUiThread(() =>
                            {
                                try
                                {
                                    var listHome = MainContext.NewsFeedFragment?.NewsFeedAdapter?.PixelNewsFeedList;
                                    var dataPostHome = listHome?.FirstOrDefault(a => a.PostId == Convert.ToInt32(PostId));
                                    if (dataPostHome != null)
                                    {
                                        if (dataPostHome.Votes >= 0)
                                            dataPostHome.Votes++;

                                        int index = listHome.IndexOf(dataPostHome);
                                        if (index >= 0)
                                            MainContext.NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(index);
                                    }

                                    if (FragmentName == "GifPost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (!(currentFragment is GifPostFragment frm))
                                            return;

                                        frm.CommentsAdapter.CommentList = CommentsAdapter.CommentList;
                                        frm.CommentsAdapter?.NotifyDataSetChanged();
                                        frm.CommentCount.Text = CommentsAdapter.CommentList.Count + " " + GetText(Resource.String.Lbl_Comments);

                                    }
                                    else if (FragmentName == "ImagePost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (!(currentFragment is ImagePostFragment frm))
                                            return;

                                        frm.CommentsAdapter.CommentList = CommentsAdapter.CommentList;
                                        frm.CommentsAdapter?.NotifyDataSetChanged();
                                        frm.CommentCount.Text = CommentsAdapter.CommentList.Count + " " + GetText(Resource.String.Lbl_Comments);

                                    }
                                    else if (FragmentName == "MultiImagePost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (!(currentFragment is MultiImagePostFragment frm))
                                            return;

                                        frm.CommentsAdapter.CommentList = CommentsAdapter.CommentList;
                                        frm.CommentsAdapter?.NotifyDataSetChanged();
                                        frm.CommentCount.Text = CommentsAdapter.CommentList.Count + " " + GetText(Resource.String.Lbl_Comments);
                                    }
                                    else if (FragmentName == "VideoPost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (!(currentFragment is VideoPostFragment frm))
                                            return;

                                        frm.CommentsAdapter.CommentList = CommentsAdapter.CommentList;
                                        frm.CommentsAdapter?.NotifyDataSetChanged();
                                        frm.CommentCount.Text = CommentsAdapter.CommentList.Count + " " + GetText(Resource.String.Lbl_Comments);
                                    }
                                    else if (FragmentName == "YoutubePost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (!(currentFragment is YoutubePostFragment frm))
                                            return;

                                        frm.CommentsAdapter.CommentList = CommentsAdapter.CommentList;
                                        frm.CommentsAdapter?.NotifyDataSetChanged();
                                        frm.CommentCount.Text = CommentsAdapter.CommentList.Count + " " + GetText(Resource.String.Lbl_Comments);

                                    }
                                    else if (FragmentName == "HashTags")
                                    {

                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (currentFragment is HashTagPostFragment frm)
                                        {
                                            var listHash = frm.MAdapter.PixelNewsFeedList;
                                            var dataPostHash = listHash?.FirstOrDefault(a => a.PostId == Convert.ToInt32(PostId));
                                            if (dataPostHash != null)
                                            {
                                                if (dataPostHash.Votes >= 0)
                                                    dataPostHash.Votes++;

                                                int index = listHash.IndexOf(dataPostHash);
                                                if (index >= 0)
                                                    frm.MAdapter.NotifyItemChanged(index);
                                            }
                                        }
                                    }
                                    else if (FragmentName == "NewsFeedPost")
                                    {
                                        var currentFragment = MainContext.FragmentBottomNavigator.GetSelectedTabBackStackFragment();

                                        if (currentFragment is NewsFeedFragment frm)
                                        {
                                            var listHash = frm.NewsFeedAdapter.PixelNewsFeedList;
                                            var dataPostHash = listHash?.FirstOrDefault(a => a.PostId == Convert.ToInt32(PostId));
                                            if (dataPostHash != null)
                                            {
                                                if (dataPostHash.Votes >= 0)
                                                    dataPostHash.Votes++;

                                                int index = listHash.IndexOf(dataPostHash);
                                                if (index >= 0)
                                                    frm.NewsFeedAdapter.NotifyItemChanged(index);
                                            }
                                        }
                                    }
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
                    EmojiconEditTextView.Text = "";
                    EmojiconEditTextView.ClearFocus();
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
         
        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = CommentsAdapter.CommentList.LastOrDefault();
                if (item != null && CommentsAdapter.CommentList.Count > 10 && !MainScrollEvent.IsLoading)
                    StartApiService(item.Id.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
         
        #region Load Data Api 

        private void StartApiService(string offset = "")
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
                int countList = CommentsAdapter.CommentList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Post.FetchComments(PostId,offset, "24");
                if (apiStatus != 200 || !(respond is FetchCommentsObject result) || result.Data == null)
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
                            foreach (var item in from item in result.Data let check = CommentsAdapter.CommentList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                CommentsAdapter.CommentList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { CommentsAdapter.NotifyItemRangeInserted(countList - 1, CommentsAdapter.CommentList.Count - countList); });
                        }
                        else
                        {
                            CommentsAdapter.CommentList = new ObservableCollection<CommentObject>(result.Data);
                            Activity.RunOnUiThread(() => { CommentsAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (CommentsAdapter.CommentList.Count > 10 && !CommentRecyclerView.CanScrollVertically(1))
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
                XSwipeRefreshLayout.Refreshing = false;

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                if (CommentsAdapter.CommentList.Count > 0)
                {
                    CommentRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    CommentRecyclerView.Visibility = ViewStates.Gone;

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
                ProgressBarLoader.Visibility = ViewStates.Gone;
                MainScrollEvent.IsLoading = false;
                XSwipeRefreshLayout.Refreshing = false;
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