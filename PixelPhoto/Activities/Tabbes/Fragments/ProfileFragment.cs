using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using Com.Luseen.Autolinklibrary;
using Com.Theartofdev.Edmodo.Cropper;
using Liaoinstan.SpringViewLib.Widgets;
using PixelPhoto.Activities.Favorites;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.MyProfile;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.PullSwipeStyles;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Console = System.Console;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;

namespace PixelPhoto.Activities.Tabbes.Fragments
{
    public class ProfileFragment : Fragment, SpringView.IOnFreshListener
    {
        #region Variables Basic

        public ImageView UserProfileImage;
        private ImageView ImgSetting;
        private TextView ImgChange;
        public TextView TxtCountFav;
        private TextView TxtCountFollowers, TxtCountFollowing, TxtFollowers, TxtFollowing, TxtFav, IconVerified, IconBusiness;
        private TextView TxtName, TxtUsername;
        private AutoLinkTextView TxtAbout;
        private Button BtnEditProfile;
        private LinearLayout LinFollowers, LinFollowing, LinFavorites;
        private RecyclerView ProfileFeedRecyclerView;
        public UserPostAdapter UserPostAdapter;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private TextSanitizer TextSanitizerAutoLink;
        private RelativeLayout Layoutfriends;
        private HomeActivity GlobalContext;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private SpringView SwipeRefreshLayout;
        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = (HomeActivity)Activity;

                UserPostAdapter = new UserPostAdapter(Activity);

                HasOptionsMenu = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.Pix_ProfileLayout, container, false);


                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters(view);

                AddOrRemoveEvent(true);

                LoadProfile();
                StartApiService();

                AdsGoogle.Ad_Interstitial(Activity);

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
                UserProfileImage = (ImageView)view.FindViewById(Resource.Id.user_pic);
                ImgSetting = (ImageView)view.FindViewById(Resource.Id.settingsbutton);
                ImgChange = (TextView)view.FindViewById(Resource.Id.IconAdd);

                ImgSetting.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                SwipeRefreshLayout = (SpringView)view.FindViewById(Resource.Id.material_style_ptr_frame);

                TxtName = (TextView)view.FindViewById(Resource.Id.card_name);
                TxtUsername = (TextView)view.FindViewById(Resource.Id.card_dist);
                BtnEditProfile = (Button)view.FindViewById(Resource.Id.cont);

                TxtCountFollowers = (TextView)view.FindViewById(Resource.Id.CountFollowers);
                TxtCountFollowing = (TextView)view.FindViewById(Resource.Id.CountFollowing);
                TxtCountFav = (TextView)view.FindViewById(Resource.Id.CountFav);

                TxtFollowers = view.FindViewById<TextView>(Resource.Id.txtFollowers);
                TxtFollowing = view.FindViewById<TextView>(Resource.Id.txtFollowing);
                TxtFav = view.FindViewById<TextView>(Resource.Id.txtFav);

                LinFollowers = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowers);
                LinFollowing = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowing);
                LinFavorites = view.FindViewById<LinearLayout>(Resource.Id.layoutFavorites);

                IconVerified = (TextView)view.FindViewById(Resource.Id.verified);
                IconBusiness = (TextView)view.FindViewById(Resource.Id.business);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconVerified, IonIconsFonts.CheckmarkCircled);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBusiness, IonIconsFonts.SocialUsd);

                IconVerified.Visibility = ViewStates.Gone;
                IconBusiness.Visibility = ViewStates.Gone;

                Layoutfriends = (RelativeLayout)view.FindViewById(Resource.Id.layoutfriends);
                //Layoutfriends.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.layout_bg_profile_friends_dark : Resource.Drawable.layout_bg_profile_friends);

                TxtAbout = (AutoLinkTextView)view.FindViewById(Resource.Id.description);
                TextSanitizerAutoLink = new TextSanitizer(TxtAbout, Activity);

                TxtCountFollowers.Text = "0";
                TxtCountFollowing.Text = "0";
                TxtCountFav.Text = "0";

                EmptyStateLayout = (ViewStub)view.FindViewById(Resource.Id.viewStub);

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

        private void SetRecyclerViewAdapters(View view)
        {
            try
            {
                ProfileFeedRecyclerView = (RecyclerView)view.FindViewById(Resource.Id.RecylerFeed);

                var mLayoutManager = new GridLayoutManager(Activity, 3);
                ProfileFeedRecyclerView.SetLayoutManager(mLayoutManager);
                ProfileFeedRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(1, 1, true));
                ProfileFeedRecyclerView.HasFixedSize = true;
                ProfileFeedRecyclerView.SetItemViewCacheSize(10);
                ProfileFeedRecyclerView.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, UserPostAdapter, sizeProvider, 8);
                ProfileFeedRecyclerView.AddOnScrollListener(preLoader);
                ProfileFeedRecyclerView.SetAdapter(UserPostAdapter);

                EmptyStateLayout.Visibility = ViewStates.Gone;
                ProfileFeedRecyclerView.Visibility = ViewStates.Visible;

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(mLayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                ProfileFeedRecyclerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
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
                GlobalContext.SetToolBar(toolbar, " ", false);
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
                    ImgSetting.Click += ImgSettingOnClick;
                    ImgChange.Click += ImgChangeOnClick;
                    UserProfileImage.Click += ImgChangeOnClick;
                    BtnEditProfile.Click += BtnEditProfileOnClick;
                    LinFollowers.Click += LinFollowersOnClick;
                    LinFollowing.Click += LinFollowingOnClick;
                    LinFavorites.Click += LinFavoritesOnClick;
                    UserPostAdapter.ItemClick += UserPostAdapterOnItemClick;
                }
                else
                {
                    ImgSetting.Click -= ImgSettingOnClick;
                    ImgChange.Click -= ImgChangeOnClick;
                    UserProfileImage.Click -= ImgChangeOnClick;
                    BtnEditProfile.Click -= BtnEditProfileOnClick;
                    LinFollowers.Click -= LinFollowersOnClick;
                    LinFollowing.Click -= LinFollowingOnClick;
                    LinFavorites.Click -= LinFavoritesOnClick;
                    UserPostAdapter.ItemClick -= UserPostAdapterOnItemClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
            {
                EmptyStateLayout.Visibility = ViewStates.Visible;
                ProfileFeedRecyclerView.Visibility = ViewStates.Gone;

                if (Inflated == null)
                    Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButton_Click;
                }
            }
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadFeedAsync(offset) });
        }
        #endregion

        #region Events

        //Load Explore Feed
        private void EmptyStateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    StartApiService();
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

        //Show post by id
        private void UserPostAdapterOnItemClick(object sender, UserPostAdapterViewHolderClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = UserPostAdapter.PostList[e.Position];
                    if (item != null)
                    {
                        GlobalContext.OpenNewsFeedItem(item);
                    }
                }
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

        // Show Img Change
        private void ImgChangeOnClick(object sender, EventArgs e)
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

        // Show Setting
        private void ImgSettingOnClick(object sender, EventArgs e)
        {
            StartActivity(new Intent(Context, typeof(SettingsUserActivity)));
        }

        #endregion

        #region Load My Profile Api 

        private async void LoadProfile()
        {
            try
            {
                //var dataUser = new SqLiteDatabase().GetMyProfile();

                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(Activity);

                var data = ListUtils.MyProfileList.FirstOrDefault();
                if (data != null)
                {
                    GlideImageLoader.LoadImage(Activity, data.Avatar, UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);

                    TxtName.Text = AppTools.GetNameFinal(data);
                    TxtUsername.Text = data.Username;

                    TextSanitizerAutoLink.Load(AppTools.GetAboutFinal(data));

                    TxtCountFav.Text = Methods.FunString.FormatPriceValue(Int32.Parse(data.Favourites));

                    IconVerified.Visibility = data.Verified == "1" ? ViewStates.Visible : ViewStates.Gone;

                    IconBusiness.Visibility = data.BusinessAccount == "1" ? ViewStates.Visible : ViewStates.Gone;

                    if (data.Followers != null && int.TryParse(data.Followers, out var numberFollowers))
                        TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(numberFollowers);

                    if (data.Following != null && int.TryParse(data.Following, out var numberFollowing))
                        TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(numberFollowing);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Load Explore Feed

        private async Task LoadFeedAsync(string offset = "")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;
                int countList = UserPostAdapter.PostList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Post.FetchUserPostsById(UserDetails.UserId, "24", offset);
                if (apiStatus != 200 || !(respond is FetchUserPostsByUserIdObject result) || result.data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    Activity.RunOnUiThread(() =>
                    {
                        TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(result.data.UserFollowers);
                        TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(result.data.UserFollowing);
                    });

                    var respondList = result.data.UserPosts.Count;
                    if (respondList > 0)
                    {
                        result.data.UserPosts.RemoveAll(a => a.MediaSet?.Count == 0 && a.MediaSet == null);

                        if (countList > 0)
                        {
                            foreach (var item in from item in result.data.UserPosts let check = UserPostAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                            {
                                item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);
                                UserPostAdapter.PostList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { UserPostAdapter.NotifyItemRangeInserted(countList - 1, UserPostAdapter.PostList.Count - countList); });
                        }
                        else
                        {
                            UserPostAdapter.PostList = new ObservableCollection<PostsObject>(result.data.UserPosts);
                            Activity.RunOnUiThread(() => { UserPostAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (UserPostAdapter.PostList.Count > 10 && !ProfileFeedRecyclerView.CanScrollVertically(1))
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

                if (UserPostAdapter.PostList.Count > 0)
                {
                    ProfileFeedRecyclerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    ProfileFeedRecyclerView.Visibility = ViewStates.Gone;

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

        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                var item = UserPostAdapter.PostList.LastOrDefault();
                if (item != null && UserPostAdapter.PostList.Count > 10 && !MainScrollEvent.IsLoading)
                    StartApiService(item.PostId.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

                                GlideImageLoader.LoadImage(Activity, photoUri.Path, UserProfileImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

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

        #region Refresh

        public void OnRefresh()
        {
            try
            {
                LoadProfile();

                UserPostAdapter.PostList.Clear();
                UserPostAdapter.NotifyDataSetChanged();
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
                var item = UserPostAdapter.PostList.LastOrDefault();
                if (item != null && UserPostAdapter.PostList.Count > 10)
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