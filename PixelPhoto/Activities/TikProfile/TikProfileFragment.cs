using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Views;
using Android.Widget;
using Com.Theartofdev.Edmodo.Cropper;
using Com.Tuyenmonkey.Textdecorator;
using PixelPhoto.Activities.Favorites;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.MyProfile;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.TikProfile.Fragments;
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.TikProfile
{
    public class TikProfileFragment : Fragment
    {
        #region  Variables Basic

        private HomeActivity GlobalContext;

        public TextView TxtPostCount;
        private TextView TxtUserName, BtnSettings, TxtFullName, TxtFollowingCount, TxtFollowersCount, BtnEditProfile;
        public ImageView ImageUser;
        private LinearLayout FollowingLayout, FollowersLayout, PostLayout;
        private TabLayout TabLayout;
        private ViewPager ViewPager;
        public MyPostFragment MyPostTab;
        public ActivitiesFragment ActivitiesTab;
        private CollapsingToolbarLayout CollapsingToolbar;
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = (HomeActivity)Activity;
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TikProfileLayout, container, false);

                InitComponent(view);
                InitToolbar(view);
                AddOrRemoveEvent(true);

                LoadProfile();
                StartApiService();

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

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:

                    try
                    {
                        GlobalContext.FragmentNavigatorBack();
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

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                CollapsingToolbar = view.FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsingToolbar);
                CollapsingToolbar.Title = "";

                TxtUserName = view.FindViewById<TextView>(Resource.Id.username);
                BtnSettings = view.FindViewById<TextView>(Resource.Id.setting_btn);

                ImageUser = view.FindViewById<ImageView>(Resource.Id.user_image);
                TxtFullName = view.FindViewById<TextView>(Resource.Id.fullname);

                FollowingLayout = view.FindViewById<LinearLayout>(Resource.Id.following_layout);
                TxtFollowingCount = view.FindViewById<TextView>(Resource.Id.following_count_txt);

                FollowersLayout = view.FindViewById<LinearLayout>(Resource.Id.followers_layout);
                TxtFollowersCount = view.FindViewById<TextView>(Resource.Id.followers_count_txt);

                PostLayout = view.FindViewById<LinearLayout>(Resource.Id.post_layout);
                TxtPostCount = view.FindViewById<TextView>(Resource.Id.post_count_txt);

                BtnEditProfile = view.FindViewById<TextView>(Resource.Id.edit_profile_btn);

                ViewPager = view.FindViewById<ViewPager>(Resource.Id.pager);
                TabLayout = view.FindViewById<TabLayout>(Resource.Id.tabs);

                TxtFollowingCount.Text = "0";
                TxtFollowersCount.Text = "0";
                TxtPostCount.Text = "0";
               
                if (AppSettings.SetTabDarkTheme)
                {
                    TxtFullName.SetTextColor(Color.White);
                    BtnSettings.SetTextColor(Color.White);
                }
                
                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                TabLayout.SetupWithViewPager(ViewPager);

                TabLayout.GetTabAt(0).SetIcon(Resource.Drawable.ic_tab_more);
                TabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_action_like_1);

                // set icon color pre-selected
                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));
                TabLayout.GetTabAt(1).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));

                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor(AppSettings.MainColor), PorterDuff.Mode.SrcIn));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BtnSettings, IonIconsFonts.AndroidSettings);
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
                Toolbar toolBar = view.FindViewById<Toolbar>(Resource.Id.toolbar);
                GlobalContext.SetToolBar(toolBar, " ", false);
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
                    BtnSettings.Click += ImgSettingOnClick;
                    ImageUser.Click += ImageUserOnClick;
                    BtnEditProfile.Click += BtnEditProfileOnClick;
                    FollowersLayout.Click += LinFollowersOnClick;
                    FollowingLayout.Click += LinFollowingOnClick;
                    PostLayout.Click += LinFavoritesOnClick;
                    TabLayout.TabSelected += TabLayoutOnTabSelected;
                    TabLayout.TabUnselected += TabLayoutOnTabUnselected;
                }
                else
                {
                    BtnSettings.Click -= ImgSettingOnClick;
                    ImageUser.Click -= ImageUserOnClick;
                    BtnEditProfile.Click -= BtnEditProfileOnClick;
                    FollowersLayout.Click -= LinFollowersOnClick;
                    FollowingLayout.Click -= LinFollowingOnClick;
                    PostLayout.Click -= LinFavoritesOnClick;
                    TabLayout.TabSelected -= TabLayoutOnTabSelected;
                    TabLayout.TabUnselected -= TabLayoutOnTabUnselected;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Set Tab

        private void SetUpViewPager(ViewPager viewPager)
        {
            try
            {
                MyPostTab = new MyPostFragment();
                ActivitiesTab = new ActivitiesFragment();

                var adapter = new MainTabAdapter(ChildFragmentManager);
                adapter.AddFragment(MyPostTab, "");
                adapter.AddFragment(ActivitiesTab, "");

                viewPager.CurrentItem = 2;
                viewPager.Adapter = adapter;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TabLayoutOnTabUnselected(object sender, TabLayout.TabUnselectedEventArgs e)
        {
            try
            {
                e.Tab.Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void TabLayoutOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            try
            {
                e.Tab.Icon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor(AppSettings.MainColor), PorterDuff.Mode.SrcIn));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        #endregion Set Tab

        #region Events

        // Show Setting
        private void ImgSettingOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(Context, typeof(SettingsUserActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Change Image
        private void ImageUserOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.OpenDialogGallery("MyProfile");
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        // Show EditProfile
        private void BtnEditProfileOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivityForResult(new Intent(Context, typeof(EditProfileActivity)), 3000);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        // Show Favorites
        private void LinFavoritesOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                FavoritesFragment myFavoritesFragment = new FavoritesFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(myFavoritesFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        // Show Following
        private void LinFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("UserId", UserDetails.UserId);
                bundle.PutString("UserType", "Following");

                MyContactsFragment myContactsFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(myContactsFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        // Show Followers
        private void LinFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("UserId", UserDetails.UserId);
                bundle.PutString("UserType", "Followers");

                MyContactsFragment profileFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                GlobalContext.OpenFragment(profileFragment);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        #region Permissions && Result >> Save Image 

        //Result 
        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);

                        if (result.IsSuccessful)
                        {
                            var resultPathImage = result.Uri.Path;

                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                Java.IO.File file2 = new Java.IO.File(resultPathImage);
                                var photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);

                                var check = AppTools.CheckMimeTypesWithServer(resultPathImage);
                                if (!check)
                                {
                                    //this file not supported on the server , please select another file 
                                    Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                                    return;
                                }

                                GlideImageLoader.LoadImage(Activity, photoUri.Path, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.Avatar = resultPathImage;
                                    UserDetails.Avatar = resultPathImage;

                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                                    dbDatabase.Dispose();
                                }

                                var dataStory = GlobalContext.NewsFeedFragment.StoryAdapter?.StoryList?.FirstOrDefault(a => a.Type == "Your");
                                if (dataStory != null)
                                {
                                    dataStory.Avatar = resultPathImage;
                                    GlobalContext.NewsFeedFragment.StoryAdapter.NotifyItemChanged(0);
                                }

                                await Task.Run(async () =>
                                {
                                    (int apiStatus, var respond) = await RequestsAsync.User.SaveImageUser(resultPathImage).ConfigureAwait(false);
                                });
                            }
                            else
                            {
                                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            }
                        }
                        else
                        {
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
                else if (requestCode == 3000)
                {
                    LoadProfile();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                if (requestCode == 108)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        GlobalContext.OpenDialogGallery("MyProfile");
                    }
                    else
                    {
                        Toast.MakeText(Activity, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Load My Profile Api 

        private async void LoadProfile()
        {
            try
            {
                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(Activity);

                var data = ListUtils.MyProfileList.FirstOrDefault();
                if (data != null)
                {
                    GlideImageLoader.LoadImage(Activity, data.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                    //TxtFullName.Text = AppTools.GetNameFinal(data);
                    CollapsingToolbar.Title = AppTools.GetNameFinal(data);
                    TxtUserName.Text = "@" + data.Username;

                    TxtPostCount.Text = Methods.FunString.FormatPriceValue(Int32.Parse(data.Favourites));

                    if (data.Followers != null && int.TryParse(data.Followers, out var numberFollowers))
                        TxtFollowersCount.Text = Methods.FunString.FormatPriceValue(numberFollowers);

                    if (data.Following != null && int.TryParse(data.Following, out var numberFollowing))
                        TxtFollowingCount.Text = Methods.FunString.FormatPriceValue(numberFollowing);

                    var font = Typeface.CreateFromAsset(Application.Context.Resources.Assets, "ionicons.ttf");
                    TxtFullName.SetTypeface(font, TypefaceStyle.Normal);

                    var textHighLighter = AppTools.GetNameFinal(data);
                    var textIsPro = string.Empty;

                    if (data.Verified == "1")
                        textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                    if (data.BusinessAccount == "1")
                    {
                        textIsPro = " " + IonIconsFonts.SocialUsd;
                        textHighLighter += textIsPro;
                    }

                    var decorator = TextDecorator.Decorate(TxtFullName, textHighLighter);

                    if (data.Verified == "1")
                        decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);

                    if (data.BusinessAccount == "1")
                        decorator.SetTextColor(Resource.Color.Post_IsBusiness, IonIconsFonts.SocialUsd);

                    decorator.Build();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Load Post

        private void StartApiService(string offsetPost = "0", string offsetActivities = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPost(offsetPost), () => GetActivities(offsetActivities) });
            else
                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
        }

        public async Task GetPost(string offset = "0")
        {
            if (MyPostTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MyPostTab.MainScrollEvent.IsLoading = true;

                int countList = MyPostTab.MAdapter.PostList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Post.FetchUserPostsById(UserDetails.UserId, "24", offset);
                if (apiStatus.Equals(200))
                {
                    if (respond is FetchUserPostsByUserIdObject result)
                    {
                        Activity.RunOnUiThread(() =>
                        {
                            TxtFollowersCount.Text = Methods.FunString.FormatPriceValue(result.data.UserFollowers);
                            TxtFollowingCount.Text = Methods.FunString.FormatPriceValue(result.data.UserFollowing);
                        });

                        var respondList = result.data.UserPosts.Count;
                        if (respondList > 0)
                        {
                            result.data.UserPosts.RemoveAll(a => a.MediaSet?.Count == 0 && a.MediaSet == null);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.data.UserPosts let check = MyPostTab.MAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                                {
                                    item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);
                                    MyPostTab.MAdapter.PostList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { MyPostTab.MAdapter.NotifyItemRangeInserted(countList - 1, MyPostTab.MAdapter.PostList.Count - countList); });
                            }
                            else
                            {
                                MyPostTab.MAdapter.PostList = new ObservableCollection<PostsObject>(result.data.UserPosts);
                                Activity.RunOnUiThread(() => { MyPostTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (MyPostTab.MAdapter.PostList.Count > 10 && !MyPostTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short).Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

                Activity.RunOnUiThread(() => { ShowEmptyPage("GetPost"); });
            }
            else
            {
                MyPostTab.Inflated = MyPostTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(MyPostTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                MyPostTab.MainScrollEvent.IsLoading = false;
            }
            MyPostTab.MainScrollEvent.IsLoading = false;
        }

        public async Task GetActivities(string offset = "0")
        {
            if (ActivitiesTab.MainScrollEvent.IsLoading)
                return;

            ActivitiesTab.MainScrollEvent.IsLoading = true;
            int countList = ActivitiesTab.MAdapter.LastActivitiesList.Count;
            (int apiStatus, var respond) = await RequestsAsync.User.FetchActivities("20", offset);
            if (apiStatus.Equals(200))
            {
                if (respond is ActivitiesObject result)
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        if (countList > 0)
                        {
                            foreach (var item in from item in result.Data let check = ActivitiesTab.MAdapter.LastActivitiesList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                            {
                                ActivitiesTab.MAdapter.LastActivitiesList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { ActivitiesTab.MAdapter.NotifyItemRangeInserted(countList - 1, ActivitiesTab.MAdapter.LastActivitiesList.Count - countList); });
                        }
                        else
                        {
                            ActivitiesTab.MAdapter.LastActivitiesList = new ObservableCollection<ActivitiesObject.Activity>(result.Data);
                            Activity.RunOnUiThread(() => { ActivitiesTab.MAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (ActivitiesTab.MAdapter.LastActivitiesList.Count > 10 && !ActivitiesTab.MRecycler.CanScrollVertically(1))
                            Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short).Show();
                    }
                }
            }
            else Methods.DisplayReportResult(Activity, respond);

            Activity.RunOnUiThread(() => { ShowEmptyPage("GetActivities"); });
            ActivitiesTab.MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage(string type)
        {
            try
            { 
                if (type == "GetPost")
                {
                    MyPostTab.MainScrollEvent.IsLoading = false;
                    MyPostTab.SwipeRefreshLayout.Refreshing = false;

                    if (MyPostTab.MAdapter.PostList.Count > 0)
                    {
                        MyPostTab.MRecycler.Visibility = ViewStates.Visible;
                        MyPostTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        MyPostTab.MRecycler.Visibility = ViewStates.Gone;

                        if (MyPostTab.Inflated == null)
                            MyPostTab.Inflated = MyPostTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(MyPostTab.Inflated, EmptyStateInflater.Type.NoPost);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }
                        MyPostTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "GetActivities")
                {
                    ActivitiesTab.MainScrollEvent.IsLoading = false;
                    ActivitiesTab.SwipeRefreshLayout.Refreshing = false;

                    if (ActivitiesTab.MAdapter.LastActivitiesList.Count > 0)
                    {
                        ActivitiesTab.MRecycler.Visibility = ViewStates.Visible;
                        ActivitiesTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        ActivitiesTab.MRecycler.Visibility = ViewStates.Gone;

                        if (ActivitiesTab.Inflated == null)
                            ActivitiesTab.Inflated = ActivitiesTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(ActivitiesTab.Inflated, EmptyStateInflater.Type.NoActivities);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }
                        ActivitiesTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
            }
            catch (Exception e)
            {
                MyPostTab.MainScrollEvent.IsLoading = false;
                MyPostTab.SwipeRefreshLayout.Refreshing = false;

                ActivitiesTab.MainScrollEvent.IsLoading = false;
                ActivitiesTab.SwipeRefreshLayout.Refreshing = false;
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