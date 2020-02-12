using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android; 
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Bumptech.Glide.Util;
using Java.Lang;
using Liaoinstan.SpringViewLib.Widgets;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Story;
using PixelPhoto.Activities.Story.Adapters;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Story;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Qintong.Library;
using Console = System.Console;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace PixelPhoto.Activities.Tabbes.Fragments
{
    public class NewsFeedFragment : Fragment, SpringView.IOnFreshListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private HomeActivity MainContext;
        private AppBarLayout AppBarLayout;
        private RecyclerView StoryRecycler;
        private TextView TxtAppName;
        public ImageView ImageChat;
        private SpringView SwipeRefreshLayout;
        public PRecyclerView RecyclerFeed;
        private ProgressBar ProgressBar;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        public StoryAdapter StoryAdapter;
        public NewsFeedAdapter NewsFeedAdapter;
        public Handler MainHandler = new Handler();
        public IRunnable Runnable;
        private const int PostUpdaterInterval = 20000;

        #endregion

        #region General

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
                View view = inflater.Inflate(Resource.Layout.Pix_Tab_NewsFeed, container, false);

                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                StartApiService("Add");

                //Start Updating the news feed every few minus 
                MainHandler.PostDelayed(new ApiPostUpdaterHelper(MainContext, RecyclerFeed, MainHandler), PostUpdaterInterval);

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
                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.appBarLayout);
                AppBarLayout.SetExpanded(false);

                StoryRecycler = view.FindViewById<RecyclerView>(Resource.Id.StoryRecyler);

                TxtAppName = view.FindViewById<TextView>(Resource.Id.Appname);
                TxtAppName.Text = AppSettings.ApplicationName;
                TxtAppName.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                ImageChat = view.FindViewById<ImageView>(Resource.Id.chatbutton);
                ImageChat.Click += ImageChatOnClick;

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new PixelDefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new PixelDefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.SetListener(this);
                SwipeRefreshLayout.OnFinishFreshAndLoad();//check this

                RecyclerFeed = view.FindViewById<PRecyclerView>(Resource.Id.RecylerFeed);

                ProgressBar = view.FindViewById<ProgressBar>(Resource.Id.sectionProgress);
                ProgressBar.Visibility = ViewStates.Visible;

                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                EmptyStateLayout.Visibility = ViewStates.Gone;
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
                var toolbar = view.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
                MainContext.SetToolBar(toolbar, " ", false);
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
                //Pro Recycler View 
                StoryAdapter = new StoryAdapter(Activity);
                StoryRecycler.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                StoryRecycler.SetItemViewCacheSize(20);
                StoryRecycler.HasFixedSize = true;
                StoryRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProviderPro = new FixedPreloadSizeProvider(10, 10);
                var preLoaderPro = new RecyclerViewPreloader<FetchStoriesObject.Data>(Activity, StoryAdapter, sizeProviderPro, 10);
                StoryRecycler.AddOnScrollListener(preLoaderPro);
                StoryAdapter.ItemClick += StoryAdapterOnItemClick;
                StoryRecycler.SetAdapter(StoryAdapter);

                NewsFeedAdapter = new NewsFeedAdapter(Activity, RecyclerFeed);
                RecyclerFeed.SetXAdapter(NewsFeedAdapter, true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void ImageChatOnClick(object sender, EventArgs e)
        {
            try
            {
                //Convert to fragment 
                Context.StartActivity(new Intent(Context, typeof(LastChatActivity)));
                MainContext.ShowOrHideBadgeViewMessenger();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void StoryAdapterOnItemClick(object sender, StoryAdapterClickEventArgs e)
        {
            try
            {
                var circleIndicator = e.View.FindViewById<InsLoadingView>(Resource.Id.profile_indicator);
                circleIndicator.Status = InsLoadingView.Statuses.Clicked;

                //Open View Story Or Create New Story
                var item = StoryAdapter.GetItem(e.Position);
                if (item != null)
                {
                    if (item.Type == "Your")
                    {
                        var arrayAdapter = new List<string>();
                        var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                        arrayAdapter.Add(Activity.GetText(Resource.String.text));
                        arrayAdapter.Add(Activity.GetText(Resource.String.image));
                        arrayAdapter.Add(Activity.GetText(Resource.String.video));
                        arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Camera));

                        dialogList.Title(Activity.GetText(Resource.String.Lbl_AddStory));
                        dialogList.Items(arrayAdapter);
                        dialogList.PositiveText(Activity.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                        dialogList.AlwaysCallSingleChoiceCallback();
                        dialogList.ItemsCallback(this).Build().Show();
                    }
                    else
                    {
                        Intent intent = new Intent(Activity, typeof(ViewStoryActivity));
                        intent.PutExtra("UserId", item.UserId.ToString());
                        intent.PutExtra("DataItem", JsonConvert.SerializeObject(item));
                        Activity.StartActivity(intent);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                NewsFeedAdapter.PixelNewsFeedList.Clear();
                NewsFeedAdapter.NotifyDataSetChanged();

                StoryAdapter.StoryList.Clear();
                StoryAdapter.NotifyDataSetChanged();

                ListUtils.FundingList.Clear();

                StartApiService("Add");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnLoadMore()
        {
            try
            {
                var item = NewsFeedAdapter.PixelNewsFeedList.LastOrDefault();
                if (item == null)
                    return;

                StartApiService(item.PostId.ToString());

                SwipeRefreshLayout.OnFinishFreshAndLoad();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Activity.GetText(Resource.String.image))
                {
                    MainContext.TypeOpen = "StoryImage";
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        if (!AppSettings.ImageCropping)
                            //requestCode >> 500 => Image Gallery
                            new IntentController(Activity).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                        else
                            ((HomeActivity)Activity).OpenDialogGallery("StoryImage");
                    }
                    else
                    {
                        if (Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            if (!AppSettings.ImageCropping)
                                //requestCode >> 500 => Image Gallery
                                new IntentController(Activity).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                            else
                                ((HomeActivity)Activity).OpenDialogGallery("StoryImage");
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(108);
                        }
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.video))
                {
                    MainContext.TypeOpen = "StoryVideo";
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 501 => video Gallery
                        new IntentController(Activity).OpenIntentVideoGallery();
                    }
                    else
                    {
                        if (Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(Activity).OpenIntentVideoGallery();
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(108);
                        }
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Camera))
                {
                    MainContext.TypeOpen = "StoryCamera";
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 503 => Camera
                        new IntentController(Activity).OpenIntentCamera();
                    }
                    else
                    {
                        if (Activity.CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                            && Activity.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(Activity).OpenIntentCamera();
                        }
                        else
                        {
                            new PermissionsController(Activity).RequestPermission(108);
                        }
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.text))
                {
                    ((HomeActivity)Activity).OpenEditColor();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (p1 == DialogAction.Positive)
                {
                }
                else if (p1 == DialogAction.Negative)
                {
                    p0.Dismiss();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion

        #region Load Data Api

        private void StartApiService(string typeRun, string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                // check if the app in background
                Toast.MakeText(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
            {
                if (AppSettings.ShowFunding)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadFunding });

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadStory, () => RecyclerFeed.FetchNewsFeedApiPosts(typeRun, offset, "25") });
            }
        }

        private async Task LoadStory()
        {
            try
            {
                Activity.RunOnUiThread(() =>
                {
                    try
                    {
                        var dataOwner = StoryAdapter.StoryList.FirstOrDefault(a => a.Type == "Your");
                        if (dataOwner == null)
                        {
                            StoryAdapter.StoryList.Insert(0, new FetchStoriesObject.Data()
                            {
                                Avatar = UserDetails.Avatar,
                                Type = "Your",
                                Username = Context.GetText(Resource.String.Lbl_YourStory),
                                Owner = true,
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
                 
                if (Methods.CheckConnectivity())
                {
                    int countList = StoryAdapter.StoryList.Count;
                    (int apiStatus, var respond) = await RequestsAsync.Story.FetchStories();
                    if (apiStatus != 200 || !(respond is FetchStoriesObject result) || result.data == null)
                    {
                        Methods.DisplayReportResult(Activity, respond);
                    }
                    else
                    {
                        var respondList = result.data.Count;
                        if (respondList > 0)
                        {  
                            foreach (var item in result.data)
                            {
                                var check = StoryAdapter.StoryList.FirstOrDefault(a => a.UserId == item.UserId);
                                if (check == null)
                                {
                                    foreach (var item1 in item.Stories)
                                    {
                                        if (item.DurationsList == null)
                                            item.DurationsList = new List<long>();

                                        var type1 = Methods.AttachmentFiles.Check_FileExtension(item1.MediaFile);
                                        if (type1 != "Video")
                                        {
                                            Glide.With(Context).Load(item1.MediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                            item.DurationsList.Add(10000L);
                                        }
                                        else
                                        { 
                                            var fileName = item1.MediaFile.Split('/').Last();
                                            item1.MediaFile = AppTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, item1.MediaFile);

                                            if (Long.ParseLong(item1.Duration) == 0)
                                            {
                                                item1.Duration = AppTools.GetDuration(item1.MediaFile);
                                                item.DurationsList.Add(Long.ParseLong(item1.Duration));
                                            }
                                            else
                                            {
                                                item.DurationsList.Add(Long.ParseLong(item1.Duration));
                                            }
                                        }
                                    }
                                     
                                    StoryAdapter.StoryList.Add(item);
                                }
                                else
                                { 
                                    foreach (var item2 in item.Stories)
                                    {
                                        if (item.DurationsList == null)
                                            item.DurationsList = new List<long>();

                                        var type = Methods.AttachmentFiles.Check_FileExtension(item2.MediaFile);
                                        if (type != "Video")
                                        {
                                            Glide.With(Context).Load(item2.MediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                            item.DurationsList.Add(10000L);
                                        }
                                        else
                                        {
                                            var fileName = item2.MediaFile.Split('/').Last();
                                            item2.MediaFile = AppTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, item2.MediaFile);

                                            if (Long.ParseLong(item2.Duration) == 0)
                                            {
                                                item2.Duration = AppTools.GetDuration(item2.MediaFile);
                                                item.DurationsList.Add(Long.ParseLong(item2.Duration));
                                            }
                                            else
                                            {
                                                item.DurationsList.Add(Long.ParseLong(item2.Duration));
                                            }
                                        }
                                    }

                                    check.Stories = item.Stories; 
                                }
                            }

                            if (countList > 0)
                            { 
                                Activity.RunOnUiThread(() => { StoryAdapter.NotifyItemRangeInserted(countList - 1, StoryAdapter.StoryList.Count - countList); });
                            }
                            else
                            {
                                Activity.RunOnUiThread(() => { StoryAdapter.NotifyDataSetChanged(); });
                            }
                        }
                    }
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
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                
            }
        }

        public void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBar.Visibility == ViewStates.Visible)
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    AppBarLayout.SetExpanded(true);
                }

                if (NewsFeedAdapter.PixelNewsFeedList.Count > 0)
                {
                    RecyclerFeed.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    RecyclerFeed.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPost);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                SwipeRefreshLayout.OnFinishFreshAndLoad();
                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                StartApiService("Add");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async Task LoadFunding()
        {
            if (Methods.CheckConnectivity())
            {
                var countList = ListUtils.FundingList.Count;

                var (respondCode, respondString) = await RequestsAsync.User.FetchFunding("10", "0");
                if (respondCode.Equals(200))
                {
                    if (respondString is FetchFundingObject respond)
                    {
                        var respondList = respond.Data.Count;
                        if (respondList > 0)
                        {
                            if (countList == 0)
                            {
                                ListUtils.FundingList = new ObservableCollection<FundingDataObject>(respond.Data);
                            }
                            else
                            {
                                foreach (var item in from item in respond.Data let check = ListUtils.FundingList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                {
                                    ListUtils.FundingList.Add(item);
                                }
                            }
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respondString);
            }
        }

        #endregion

        private class ApiPostUpdaterHelper : Java.Lang.Object, IRunnable
        {
            private readonly PRecyclerView MainRecyclerView;
            private readonly Handler MainHandler;
            private readonly HomeActivity Activity;

            public ApiPostUpdaterHelper(HomeActivity activity, PRecyclerView mainRecyclerView, Handler mainHandler)
            {
                MainRecyclerView = mainRecyclerView;
                MainHandler = mainHandler;
                Activity = activity;
            }

            public async void Run()
            {
                try
                {
                    Activity.NewsFeedFragment.StartApiService(Activity.NewsFeedFragment.NewsFeedAdapter.PixelNewsFeedList.Count > 0 ? "Insert" : "Add");
                    Activity.Get_Notifications();
                    await ApiRequest.GetProfile_Api(Activity).ConfigureAwait(false);
                    MainHandler.PostDelayed(new ApiPostUpdaterHelper(Activity, MainRecyclerView, MainHandler), 20000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}