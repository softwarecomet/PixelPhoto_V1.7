using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Fragment = Android.Support.V4.App.Fragment;
using SearchView = Android.Support.V7.Widget.SearchView;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.Search
{
    public class SearchFragment : Fragment
    {
        #region Variables Basic

        private AppBarLayout AppBarLayout;
        private SearchUserFragment UserTab;
        private SearchHashtagFragment HashTagTab;
        private TabLayout TabLayout;
        private ViewPager ViewPager;
        private SearchView SearchView;
        private string SearchText = "a";
        private HomeActivity MainContext;

        #endregion

        #region General

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
                View view = inflater.Inflate(Resource.Layout.Search_Tabbed_Layout, container, false);

                SearchText = Arguments?.GetString("Key") ?? "a";

                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                AddOrRemoveEvent(true);

                AdsGoogle.Ad_Interstitial(Context);

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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                SearchView = view.FindViewById<SearchView>(Resource.Id.searchviewbox);
                SearchView.SetQuery("", false);
                SearchView.ClearFocus();
                SearchView.OnActionViewExpanded();
                SearchView.SetIconifiedByDefault(true);
                SearchView.OnActionViewExpanded();

                //Change text colors
                var editText = (EditText) SearchView.FindViewById(Resource.Id.search_src_text);
                editText.SetHintTextColor(Color.Gray);
                editText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                TabLayout = view.FindViewById<TabLayout>(Resource.Id.Searchtabs);
                ViewPager = view.FindViewById<ViewPager>(Resource.Id.Searchviewpager);

                AppBarLayout = view.FindViewById<AppBarLayout>(Resource.Id.mainAppBarLayout);
                AppBarLayout.SetExpanded(true);

                //Set Tab
                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                TabLayout.SetupWithViewPager(ViewPager);
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
                var toolBar = view.FindViewById<Toolbar>(Resource.Id.Searchtoolbar);
                MainContext.SetToolBar(toolBar, "");

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
                    SearchView.QueryTextChange += SearchView_QueryTextChange;
                    SearchView.QueryTextSubmit += SearchViewOnQueryTextSubmit;

                }
                else
                {
                    SearchView.QueryTextChange -= SearchView_QueryTextChange;
                    SearchView.QueryTextSubmit -= SearchViewOnQueryTextSubmit;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void SearchView_QueryTextChange(object sender, SearchView.QueryTextChangeEventArgs e)
        {
            SearchText = e.NewText;
        }

        private void SearchViewOnQueryTextSubmit(object sender, SearchView.QueryTextSubmitEventArgs e)
        {
            try
            {
                SearchView.ClearFocus();

                UserTab.MAdapter.UserList.Clear();
                UserTab.MAdapter.NotifyDataSetChanged();

                HashTagTab.MAdapter.HashTagsList.Clear();
                HashTagTab.MAdapter.NotifyDataSetChanged();

                if (Methods.CheckConnectivity())
                {
                    if (UserTab.MAdapter.UserList.Count > 0)
                    {
                        UserTab.MAdapter.UserList.Clear();
                        UserTab.MAdapter.NotifyDataSetChanged();
                    }

                    if (HashTagTab.MAdapter.HashTagsList.Count > 0)
                    {
                        HashTagTab.MAdapter.HashTagsList.Clear();
                        HashTagTab.MAdapter.NotifyDataSetChanged();
                    }

                    UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
                }
                else
                {
                    if (UserTab.Inflated == null)
                        UserTab.Inflated = UserTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoConnection);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                SearchView.ClearFocus();

                UserTab.MAdapter.UserList.Clear();
                UserTab.MAdapter.NotifyDataSetChanged();

                HashTagTab.MAdapter.HashTagsList.Clear();
                HashTagTab.MAdapter.NotifyDataSetChanged();

                if (string.IsNullOrEmpty(SearchText) || string.IsNullOrWhiteSpace(SearchText))
                {
                    SearchText = "a";
                }

                ViewPager.SetCurrentItem(0, true);

                if (Methods.CheckConnectivity())
                {
                    if (UserTab.MAdapter.UserList.Count > 0)
                    {
                        UserTab.MAdapter.UserList.Clear();
                        UserTab.MAdapter.NotifyDataSetChanged();
                    }

                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });

                }
                else
                {
                    if (UserTab.Inflated == null)
                        UserTab.Inflated = UserTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                UserTab = new SearchUserFragment();
                HashTagTab = new SearchHashtagFragment();

                MainTabAdapter adapter = new MainTabAdapter(ChildFragmentManager);
                adapter.AddFragment(UserTab, GetText(Resource.String.Lbl_Users));
                adapter.AddFragment(HashTagTab, GetText(Resource.String.Lbl_HashTags));
                viewPager.Adapter = adapter;

                TabLayout.SetTabTextColors(AppSettings.SetTabDarkTheme ? Color.White : Color.Black,
                    Color.ParseColor(AppSettings.MainColor));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

        #region Load Data Search 

        public void Search()
        {
            try
            {
                if (!string.IsNullOrEmpty(SearchText))
                {
                    if (Methods.CheckConnectivity())
                    {
                        UserTab.MAdapter.UserList.Clear();
                        UserTab.MAdapter.NotifyDataSetChanged();

                        HashTagTab.MAdapter.HashTagsList.Clear();
                        HashTagTab.MAdapter.NotifyDataSetChanged();

                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { StartSearchRequest });
                    }
                }
                else
                {
                    if (UserTab.Inflated == null)
                        UserTab.Inflated = UserTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task StartSearchRequest()
        {
            if (UserTab.MainScrollEvent.IsLoading)
                return;

            UserTab.MainScrollEvent.IsLoading = true;

            int countSongsList = UserTab.MAdapter.UserList.Count;
            int countHashTagsList = HashTagTab.MAdapter.HashTagsList.Count;

            (int apiStatus, var respond) = await RequestsAsync.User.SearchUsersHashtags(SearchText, UserTab.Offset);
            if (apiStatus == 200)
            {
                if (respond is SearchUsersHastagsObject result)
                {
                    // User
                    var respondSongsList = result.DataSearch?.Users?.Count;
                    if (respondSongsList > 0)
                    {
                        if (countSongsList > 0)
                        {
                            foreach (var item in from item in result.DataSearch?.Users let check = UserTab.MAdapter.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                UserTab.MAdapter.UserList.Add(item);
                            }

                            //Activity.RunOnUiThread(() => { UserTab.MAdapter.NotifyItemRangeInserted(countSongsList - 1, UserTab.MAdapter.UserList.Count - countSongsList); });
                        }
                        else
                        {
                            UserTab.MAdapter.UserList = new ObservableCollection<UserDataObject>(result.DataSearch?.Users);
                        }
                        Activity.RunOnUiThread(() => { UserTab.MAdapter.NotifyDataSetChanged(); });

                    }
                    else
                    {
                        if (UserTab.MAdapter.UserList.Count > 10 && !UserTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreUsers), ToastLength.Short).Show();
                    }

                    var respondHashTagsList = result.DataSearch?.Hash?.Count;
                    if (respondHashTagsList > 0)
                    {
                        if (countHashTagsList > 0)
                        {
                            foreach (var item in from item in result.DataSearch?.Hash let check = HashTagTab.MAdapter.HashTagsList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                HashTagTab.MAdapter.HashTagsList.Add(item);
                            }

                            //Activity.RunOnUiThread(() => { HashTagTab.MAdapter.NotifyItemRangeInserted(countHashTagsList - 1, HashTagTab.MAdapter.HashTagsList.Count - countHashTagsList); });
                        }
                        else
                        {
                            HashTagTab.MAdapter.HashTagsList = new ObservableCollection<SearchUsersHastagsObject.Hash>(result.DataSearch?.Hash);
                        }
                        Activity.RunOnUiThread(() => { HashTagTab.MAdapter.NotifyDataSetChanged(); });
                    }
                    else
                    {
                        if (HashTagTab.MAdapter.HashTagsList.Count > 10 && !HashTagTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Context, Context.GetText(Resource.String.Lbl_NoMoreHash), ToastLength.Short).Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);
             
            Activity.RunOnUiThread(ShowEmptyPage); 
        }

        private void ShowEmptyPage()
        {
            try
            {
                UserTab.MainScrollEvent.IsLoading = false;
                UserTab.ProgressBarLoader.Visibility = ViewStates.Gone;

                if (UserTab.MAdapter.UserList.Count > 0)
                {
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (UserTab.Inflated == null)
                        UserTab.Inflated = UserTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(UserTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    UserTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                }


                if (HashTagTab.MAdapter.HashTagsList.Count > 0)
                {
                    HashTagTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    if (HashTagTab.Inflated == null)
                        HashTagTab.Inflated = HashTagTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(HashTagTab.Inflated, EmptyStateInflater.Type.NoSearchResult);
                    if (x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click -= EmptyStateButtonOnClick;
                        x.EmptyStateButton.Click -= TryAgainButton_Click;
                    }

                    x.EmptyStateButton.Click += TryAgainButton_Click;
                    HashTagTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception e)
            {
                UserTab.MainScrollEvent.IsLoading = false;
                Console.WriteLine(e);
            }
        }

        //No Internet Connection 
        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchText = "a";

                ViewPager.SetCurrentItem(0, true);

                UserTab.ProgressBarLoader.Visibility = ViewStates.Visible;
                UserTab.EmptyStateLayout.Visibility = ViewStates.Gone;

                Search();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion 
    }
}