using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Liaoinstan.SpringViewLib.Widgets;
using PixelPhoto.Activities.Posts.Extras;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace PixelPhoto.Activities.Posts
{
    public class HashTagPostFragment : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        public NewsFeedAdapter MAdapter;
        private PRecyclerView MRecycler;
        private ProgressBar ProgressBarLoader;
        private ViewStub EmptyStateLayout;
        private LinearLayoutManager MLayoutManager;
        private HomeActivity GlobalContext;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private View Inflated;
        private string HashId , HashName;
        private HomeActivity MainContext;
        private SpringView SwipeRefreshLayout;

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
                View view = inflater.Inflate(Resource.Layout.HashtagPostLayout, container, false);
                GlobalContext = (HomeActivity)Activity;

                HashId = Arguments.GetString("HashId");
                HashName = Arguments.GetString("HashName");
                 
                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                StartApiService();

                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        public override void OnDestroy()
        {
            try
            {
                MRecycler?.ReleasePlayer();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = view.FindViewById<PRecyclerView>(Resource.Id.HashtagRecyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                ProgressBarLoader = view.FindViewById<ProgressBar>(Resource.Id.sectionProgress);

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);
                SwipeRefreshLayout.SetType(SpringView.Type.Overlap);
                SwipeRefreshLayout.Header = new PixelDefaultHeader(Activity);
                SwipeRefreshLayout.Footer = new PixelDefaultFooter(Activity);
                SwipeRefreshLayout.Enable = true;
                SwipeRefreshLayout.SetListener(this);
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
                var toolBar = view.FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

                if (!HashName.Contains("#"))
                    HashName = "#" + HashName;

                GlobalContext.SetToolBar(toolBar, HashName);
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
                MAdapter = new NewsFeedAdapter(Activity, MRecycler);
                MLayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(MLayoutManager);
                MRecycler.SetXAdapter(MAdapter,false);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(MLayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
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
                    try
                    {
                        MRecycler?.ReleasePlayer(); 
                        MainContext.FragmentNavigatorBack();
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Scroll

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = MAdapter.PixelNewsFeedList.LastOrDefault();
                if (item != null && MAdapter.PixelNewsFeedList.Count > 10 && !MainScrollEvent.IsLoading)
                    StartApiService(item.PostId.ToString());
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

                int countList = MAdapter.PixelNewsFeedList.Count;
                string decodeHash = HashName.Replace("#", "");
                var (apiStatus, respond) = await RequestsAsync.Post.FetchPostsByHashtag(decodeHash, offset, "25");
                if (apiStatus != 200 || !(respond is FetchPostsByHashTagObject result) || result.PostList == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    var respondList = result.PostList.Count;
                    if (respondList > 0)
                    {
                        result.PostList.RemoveAll(a => a.MediaSet?.Count == 0 && a.MediaSet == null);

                        if (countList > 0)
                        {
                            foreach (var item in from item in result.PostList let check = MAdapter.PixelNewsFeedList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);
                                MAdapter.PixelNewsFeedList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList - 1, MAdapter.PixelNewsFeedList.Count - countList); });
                        }
                        else
                        {
                            MAdapter.PixelNewsFeedList = new ObservableCollection<PostsObject>(result.PostList);
                            Activity.RunOnUiThread(() => { MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (MAdapter.PixelNewsFeedList.Count > 10 && !MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short).Show();
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

                SwipeRefreshLayout.OnFinishFreshAndLoad();

                if (ProgressBarLoader.Visibility == ViewStates.Visible)
                    ProgressBarLoader.Visibility = ViewStates.Gone;

                if (MAdapter.PixelNewsFeedList.Count > 0)
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
                MainScrollEvent.IsLoading = false;
                ProgressBarLoader.Visibility = ViewStates.Gone;
                SwipeRefreshLayout.OnFinishFreshAndLoad();
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

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                MAdapter.PixelNewsFeedList.Clear();
                MAdapter.NotifyDataSetChanged();

                StartApiService();
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
                var item = MAdapter.PixelNewsFeedList.LastOrDefault();
                if (item != null && MAdapter.PixelNewsFeedList.Count > 10)
                    StartApiService(item.PostId.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion

    }
}