using System;
using System.Collections.Generic;
using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Com.Gigamole.Navigationtabbar.Ntb;
using Newtonsoft.Json;
using PixelPhoto.Activities.Comment;
using PixelPhoto.Activities.Posts;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.Res;
using Android.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Com.Theartofdev.Edmodo.Cropper;
using Java.IO;
using PixelPhoto.Activities.AddPost;
using PixelPhoto.Activities.Editor;
using PixelPhoto.Activities.Funding;
using PixelPhoto.Activities.Funding.Adapters;
using PixelPhoto.Activities.Story;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Activities.Tabbes.Fragments;
using PixelPhoto.Activities.UserProfile;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Q.Rorbin.Badgeview;
using Console = System.Console;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;
using Com.Hitomi.Cmlibrary;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.Chat.Service;
using PixelPhoto.Activities.TikProfile;
using PixelPhotoClient.Classes.Messages;

namespace PixelPhoto.Activities.Tabbes
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode | ConfigChanges.Locale)]
    public class HomeActivity : AppCompatActivity, IOnMenuSelectedListener, IOnMenuStatusChangeListener, ServiceResultReceiver.IReceiver
    {
        #region Variables Basic

        public NewsFeedFragment NewsFeedFragment;
        private ExploreFragment ExploreFragment;
        private NotificationsFragment NotificationsFragment;
        public ProfileFragment ProfileFragment;
        public TikProfileFragment TikProfileFragment;
        public CircleMenu CircleMenu;
        public NavigationTabBar NavigationTabBar;
        public FragmentBottomNavigationView FragmentBottomNavigator;
        private static HomeActivity Instance;
        private ServiceResultReceiver Receiver;
        private readonly Handler ExitHandler = new Handler();
        private bool RecentlyBackPressed;
        public string TypeOpen;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                Window.SetSoftInputMode(SoftInput.AdjustNothing);

                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.Tabbed_Main_Layout);

                Instance = this;

                //Get Value  
                SetupBottomNavigationView(); 

                GetGeneralAppData();  
                SetupAddPostView();

                string type = Intent.GetStringExtra("TypeNotification") ?? "Don't have type";
                if (!string.IsNullOrEmpty(type) && type != "Don't have type")
                {
                    if (type == "followed_u")
                    {
                        Bundle bundle = new Bundle();
                        bundle.PutString("userinfo", JsonConvert.SerializeObject(OneSignalNotification.UserData));
                        bundle.PutString("type", "OneSignalNotification");
                        bundle.PutString("userid", OneSignalNotification.Userid);
                        bundle.PutString("avatar", OneSignalNotification.UserData.Avatar);
                        bundle.PutString("fullname", OneSignalNotification.UserData.Username);

                        if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                        {
                            UserProfileFragment profileFragment = new UserProfileFragment
                            {
                                Arguments = bundle
                            };
                            OpenNewsFeedItem(profileFragment);
                        }
                        else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                        {
                            TikUserProfileFragment profileFragment = new TikUserProfileFragment
                            {
                                Arguments = bundle
                            };
                            OpenNewsFeedItem(profileFragment);
                        }
                    }
                    else if (type == "liked_ur_post" || type == "commented_ur_post" || type == "mentioned_u_in_comment" || type == "mentioned_u_in_post")
                    {
                        OpenNewsFeedItem(OneSignalNotification.PostData);
                    }
                }

                SetService();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public static HomeActivity GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

        protected override void OnDestroy()
        {
            try
            {
                NewsFeedFragment?.RecyclerFeed?.ReleasePlayer();

                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            try
            {
                base.OnConfigurationChanged(newConfig);

                var currentNightMode = newConfig.UiMode & UiMode.NightMask;
                switch (currentNightMode)
                {
                    case UiMode.NightNo:
                        // Night mode is not active, we're using the light theme
                        AppSettings.SetTabDarkTheme = false;
                        break;
                    case UiMode.NightYes:
                        // Night mode is active, we're using dark theme
                        AppSettings.SetTabDarkTheme = true;
                        break;
                }

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                if (AppSettings.SetTabColoredTheme)
                {
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_News_Feed)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Explore)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Notifications)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_More)).Color = Color.ParseColor(AppSettings.TabColoredColor);

                    NavigationTabBar.BgColor = Color.ParseColor(AppSettings.MainColor);
                    NavigationTabBar.ActiveColor = Color.White;
                    NavigationTabBar.InactiveColor = Color.White;
                }
                else if (AppSettings.SetTabDarkTheme)
                {
                    //FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_News_Feed)).Color = Color.ParseColor("#444444");
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_News_Feed)).Color = Color.ParseColor(AppSettings.MainColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Explore)).Color = Color.ParseColor(AppSettings.MainColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor(AppSettings.MainColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Notifications)).Color = Color.ParseColor(AppSettings.MainColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_More)).Color = Color.ParseColor(AppSettings.MainColor);
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor(AppSettings.MainColor);

                    NavigationTabBar.BgColor = Color.ParseColor("#000000");
                    NavigationTabBar.ActiveColor = Color.White;
                    NavigationTabBar.InactiveColor = Color.White;
                }
                else
                {
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_News_Feed)).Color = Color.ParseColor("#ffffff");
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Explore)).Color = Color.ParseColor("#ffffff");
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor("#ffffff");
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Notifications)).Color = Color.ParseColor("#ffffff");
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_More)).Color = Color.ParseColor("#ffffff");
                    FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor("#ffffff");
                     
                    NavigationTabBar.BgColor = Color.White;
                    NavigationTabBar.ActiveColor = Color.ParseColor(AppSettings.MainColor);
                    NavigationTabBar.InactiveColor = Color.ParseColor("#bfbfbf");
                }



            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Timer

        private void GetGeneralAppData()
        {
            try
            {
                var sqlEntity = new SqLiteDatabase();

                var data = ListUtils.DataUserLoginList.FirstOrDefault();
                if (data != null && data.Status != "Active")
                {
                    data.Status = "Active";
                    UserDetails.Status = "Active";
                    sqlEntity.InsertOrUpdateLogin_Credentials(data);
                }

                sqlEntity.GetMyProfile();
                Glide.With(this).Load(UserDetails.Avatar).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CircleCrop()).Preload();

                if (ListUtils.MyProfileList?.Count == 0)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetProfile_Api(this) });

                sqlEntity.Dispose();

                LoadConfigSettings();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadConfigSettings()
        {
            try
            {
                var dbDatabase = new SqLiteDatabase();
                var settingsData = dbDatabase.GetSettings();
                if (settingsData != null)
                    ListUtils.SettingsSiteList = settingsData;
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });

                dbDatabase.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async void Get_Notifications()
        {
            try
            {
                if (FragmentBottomNavigator.Models != null)
                {
                    var (countNotifications, countMessages) = await ApiRequest.GetCountNotifications().ConfigureAwait(false);
                    var tabNotifications = FragmentBottomNavigator.Models.First(a => a.Title == GetText(Resource.String.Lbl_Notifications));
                    if (tabNotifications != null && countNotifications != 0)
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                tabNotifications.BadgeTitle = countNotifications.ToString();
                                tabNotifications.UpdateBadgeTitle(countNotifications.ToString());
                                tabNotifications.ShowBadge();
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        });
                    }

                    if (countMessages != 0)
                    {
                        ShowOrHideBadgeViewMessenger(countMessages, true);
                    }
                    else
                    {
                        ShowOrHideBadgeViewMessenger();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ShowOrHideBadgeViewMessenger(int countMessages = 0, bool show = false)
        {
            try
            {
                RunOnUiThread(() =>
                {
                    try
                    {
                        if (show)
                        {
                            if (NewsFeedFragment?.ImageChat != null)
                            {
                                int gravity = (int)(GravityFlags.End | GravityFlags.Bottom);
                                QBadgeView badge = new QBadgeView(this);
                                badge.BindTarget(NewsFeedFragment?.ImageChat);
                                badge.SetBadgeNumber(countMessages);
                                badge.SetBadgeGravity(gravity);
                                badge.SetBadgeBackgroundColor(Color.ParseColor(AppSettings.MainColor));
                                badge.SetGravityOffset(10, true);
                            }
                        }
                        else
                        {
                            new QBadgeView(this).BindTarget(NewsFeedFragment?.ImageChat).Hide(true);
                        }
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

        #endregion

        #region Functions

        public void SetToolBar(Toolbar toolbar, string title, bool showIconBack = true)
        {
            try
            {
                if (toolbar != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        toolbar.Title = title;

                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(showIconBack);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);

                    if (AppSettings.SetTabDarkTheme)
                        toolbar.SetBackgroundResource(Resource.Drawable.linear_gradient_drawable_Dark);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Set Navigation And Show Fragment

        private void SetupBottomNavigationView()
        {
            try
            {
                NavigationTabBar = FindViewById<NavigationTabBar>(Resource.Id.ntb_horizontal);
                FragmentBottomNavigator = new FragmentBottomNavigationView(this);

                NewsFeedFragment = new NewsFeedFragment();
                ExploreFragment = new ExploreFragment();
                NotificationsFragment = new NotificationsFragment();

                FragmentBottomNavigator.FragmentListTab0.Add(NewsFeedFragment);
                FragmentBottomNavigator.FragmentListTab1.Add(ExploreFragment);
                FragmentBottomNavigator.FragmentListTab3.Add(NotificationsFragment);

                switch (AppSettings.ProfileTheme)
                {
                    case ProfileTheme.DefaultTheme:
                        ProfileFragment = new ProfileFragment();
                        FragmentBottomNavigator.FragmentListTab4.Add(ProfileFragment);
                        break;
                    case ProfileTheme.TikTheme:
                        TikProfileFragment = new TikProfileFragment();
                        FragmentBottomNavigator.FragmentListTab4.Add(TikProfileFragment);
                        break;
                }

                FragmentBottomNavigator.SetupNavigation(NavigationTabBar);
                NavigationTabBar.SetModelIndex(0, true);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Back Pressed 

        public override void OnBackPressed()
        {
            try
            {
                if (CircleMenu.IsOpened)
                {
                    CircleMenu.CloseMenu();
                    CircleMenu.Visibility = ViewStates.Gone;
                    return;
                }

                if (FragmentBottomNavigator.GetCountFragment() > 0)
                {
                    FragmentNavigatorBack();
                }
                else
                {
                    if (RecentlyBackPressed)
                    {
                        ExitHandler.RemoveCallbacks(() => { RecentlyBackPressed = false; });
                        RecentlyBackPressed = false;
                        MoveTaskToBack(true);
                    }
                    else
                    {
                        RecentlyBackPressed = true;
                        Toast.MakeText(this, GetString(Resource.String.press_again_exit), ToastLength.Long).Show();
                        ExitHandler.PostDelayed(() => { RecentlyBackPressed = false; }, 2000L);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void FragmentNavigatorBack()
        {
            try
            {
                NewsFeedFragment?.RecyclerFeed?.ReleasePlayer();

                FragmentBottomNavigator.OnBackStackClickFragment();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events Open Fragment

        private void SetupAddPostView()
        {
            try
            {
                CircleMenu = FindViewById<CircleMenu>(Resource.Id.circle_menu);
                CircleMenu.Visibility = ViewStates.Gone;
                //CircleMenu.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Color.CircleMenu_colorDark : Resource.Color.CircleMenu_color);
                CircleMenu.SetMainMenu(Color.ParseColor("#444444"), Resource.Drawable.pix_add_icon, Resource.Drawable.ic_action_close);

                CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.pix_action_image);
                CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.ic_action_video_icon);
                CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.ic_action_gif_icon);
                CircleMenu.AddSubMenu(Color.ParseColor("#444444"), Resource.Drawable.ic_action_broken_link);

                CircleMenu.SetOnMenuSelectedListener(this);
                CircleMenu.SetOnMenuStatusChangeListener(this);
                CircleMenu.CloseMenu();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OpenFragment(Fragment frg)
        {
            try
            {
                FragmentBottomNavigator.DisplayFragment(frg);
                NewsFeedFragment?.RecyclerFeed?.ReleasePlayer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OpenNewsFeedItem(dynamic item)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("object", JsonConvert.SerializeObject(item));
                bundle.PutString("type", "ExploreAdapter");
                bundle.PutString("userid", item.UserId.ToString());
                bundle.PutString("avatar", item.Avatar);
                bundle.PutString("fullname", item.Username);
                bundle.PutString("postid", item.PostId.ToString());

                var type = NewsFeedAdapter.GetPostType(item);
                if (type == NativeFeedType.Video)
                {
                    VideoPostFragment videoPostFragment = new VideoPostFragment { Arguments = bundle };
                    OpenFragment(videoPostFragment);
                }
                else if (type == NativeFeedType.Photo || type == NativeFeedType.Gif)
                {
                    ImagePostFragment imagePostFragment = new ImagePostFragment { Arguments = bundle };
                    OpenFragment(imagePostFragment);
                }
                else if (type == NativeFeedType.MultiPhoto)
                {
                    MultiImagePostFragment multiImagePostFragment = new MultiImagePostFragment { Arguments = bundle };
                    OpenFragment(multiImagePostFragment);
                }
                else if (type == NativeFeedType.Youtube)
                {
                    bundle.PutString("videoplayid", item.Youtube.ToString());
                    YoutubePostFragment youtubePostFragment = new YoutubePostFragment() { Arguments = bundle };
                    OpenFragment(youtubePostFragment);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OpenCommentFragment(ObservableCollection<CommentObject> commentobject, string postid, string nameFragment)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("postid", postid);
                bundle.PutString("PrevFragment", nameFragment);
                bundle.PutString("json", commentobject != null ? JsonConvert.SerializeObject(commentobject) : "null");

                var commentsFragment = new CommentsFragment()
                {
                    Arguments = bundle
                };

                OpenFragment(commentsFragment);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Circle Menu

        private int PType;
        public void OnMenuSelected(int p0)
        {
            try
            {
                PType = p0; 
                 
                Intent intPost = new Intent(this, typeof(AddPostActivity));
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (p0 == 0) // image
                    {
                        intPost.PutExtra("TypePost", "Image");
                    }
                    else if (p0 == 1) // video
                    {
                        intPost.PutExtra("TypePost", "Video");
                    }
                    else if (p0 == 2) // gif
                    {
                        intPost.PutExtra("TypePost", "Gif");
                    }
                    else if (p0 == 3) // broken_link
                    {
                        intPost.PutExtra("TypePost", "EmbedVideo");
                    }

                    StartActivityForResult(intPost, 2500);
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        if (p0 == 0) // image
                        {
                            intPost.PutExtra("TypePost", "Image");
                        }
                        else if (p0 == 1) // video
                        {
                            intPost.PutExtra("TypePost", "Video");
                        }
                        else if (p0 == 2) // gif
                        {
                            intPost.PutExtra("TypePost", "Gif");
                        }
                        else if (p0 == 3) // broken_link
                        {
                            intPost.PutExtra("TypePost", "EmbedVideo");
                        }

                        StartActivityForResult(intPost, 2500);

                    }
                    else
                    {
                        RequestPermissions(new[]
                        {
                            Manifest.Permission.Camera,
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.WriteExternalStorage,
                        }, 555);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnMenuClosed()
        {
            try
            {
                CircleMenu.Visibility = ViewStates.Gone; 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnMenuOpened()
        {

        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 2500 && resultCode == Result.Ok) //Add Post
                {
                    RunOnUiThread(() =>
                    {
                        if (FragmentBottomNavigator.PageNumber != 0)
                            NavigationTabBar.SetModelIndex(0, true);

                        string url = data.GetStringExtra("PostUrl") ?? "";
                        string json = data.GetStringExtra("PostData") ?? "";

                        PostsObject dataPost = JsonConvert.DeserializeObject<PostsObject>(json);
                        if (dataPost != null)
                        {
                            NewsFeedFragment?.NewsFeedAdapter?.PixelNewsFeedList.Insert(0, dataPost);
                            NewsFeedFragment?.NewsFeedAdapter?.NotifyItemInserted(0);
                            NewsFeedFragment?.RecyclerFeed?.ScrollToPosition(0); // >> go to post

                            AddPostMediaApi(dataPost, url);

                            if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                            {
                                var list = ProfileFragment?.UserPostAdapter?.PostList;
                                if (list != null)
                                {
                                    var dataPostUser = list.FirstOrDefault(a => a.PostId == dataPost.PostId);
                                    if (dataPostUser == null)
                                    {
                                        PostsObject userPost = new PostsObject()
                                        {
                                            Time = dataPost.Time,
                                            Name = dataPost.Name,
                                            Avatar = dataPost.Avatar,
                                            Comments = dataPost.Comments,
                                            Dailymotion = dataPost.Dailymotion,
                                            Description = dataPost.Description,
                                            IsLiked = dataPost.IsLiked,
                                            IsOwner = dataPost.IsOwner,
                                            IsSaved = dataPost.IsSaved,
                                            IsShouldHide = dataPost.IsShouldHide,
                                            IsVerified = dataPost.IsVerified,
                                            Likes = dataPost.Likes,
                                            MediaSet = dataPost.MediaSet,
                                            PostId = dataPost.PostId,
                                            Type = dataPost.Type,
                                            UserId = dataPost.UserId,
                                            Username = dataPost.Username,
                                            TimeText = dataPost.TimeText,
                                            Votes = dataPost.Votes,
                                            Link = dataPost.Link,
                                            Mp4 = dataPost.Mp4,
                                            Playtube = dataPost.Playtube,
                                            Registered = dataPost.Registered,
                                            Reported = dataPost.Reported,
                                            Views = dataPost.Views,
                                            Vimeo = dataPost.Vimeo,
                                            Youtube = dataPost.Youtube,
                                        };

                                        list.Insert(0, userPost);
                                        ProfileFragment?.UserPostAdapter?.NotifyItemInserted(list.IndexOf(list.FirstOrDefault()));
                                    }
                                }
                            }
                            else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                            {
                                var list = TikProfileFragment?.MyPostTab?.MAdapter?.PostList;
                                if (list != null)
                                {
                                    var dataPostUser = list.FirstOrDefault(a => a.PostId == dataPost.PostId);
                                    if (dataPostUser == null)
                                    {
                                        PostsObject userPost = new PostsObject()
                                        {
                                            Time = dataPost.Time,
                                            Name = dataPost.Name,
                                            Avatar = dataPost.Avatar,
                                            Comments = dataPost.Comments,
                                            Dailymotion = dataPost.Dailymotion,
                                            Description = dataPost.Description,
                                            IsLiked = dataPost.IsLiked,
                                            IsOwner = dataPost.IsOwner,
                                            IsSaved = dataPost.IsSaved,
                                            IsShouldHide = dataPost.IsShouldHide,
                                            IsVerified = dataPost.IsVerified,
                                            Likes = dataPost.Likes,
                                            MediaSet = dataPost.MediaSet,
                                            PostId = dataPost.PostId,
                                            Type = dataPost.Type,
                                            UserId = dataPost.UserId,
                                            Username = dataPost.Username,
                                            TimeText = dataPost.TimeText,
                                            Votes = dataPost.Votes,
                                            Link = dataPost.Link,
                                            Mp4 = dataPost.Mp4,
                                            Playtube = dataPost.Playtube,
                                            Registered = dataPost.Registered,
                                            Reported = dataPost.Reported,
                                            Views = dataPost.Views,
                                            Vimeo = dataPost.Vimeo,
                                            Youtube = dataPost.Youtube,
                                        };

                                        list.Insert(0, userPost);
                                        TikProfileFragment?.MyPostTab?.MAdapter?.NotifyItemInserted(list.IndexOf(list.FirstOrDefault()));
                                    }
                                }
                            }


                        }
                    });
                }
                else if (requestCode == 2250 && resultCode == Result.Ok) //Edit Post
                {
                    var id = Convert.ToInt32(data.GetStringExtra("PostId") ?? "0");
                    var text = data.GetStringExtra("NewTextPost") ?? " ";

                    RunOnUiThread(() =>
                    {
                        var dataPost = NewsFeedFragment.NewsFeedAdapter?.PixelNewsFeedList?.FirstOrDefault(a => a.PostId == id);
                        if (dataPost != null)
                        {
                            dataPost.Description = text;
                            NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(NewsFeedFragment.NewsFeedAdapter.PixelNewsFeedList.IndexOf(dataPost));
                        }

                        if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                        {
                            var dataPostUser = ProfileFragment.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == id);
                            if (dataPostUser != null)
                            {
                                dataPostUser.Description = text;
                                ProfileFragment.UserPostAdapter.NotifyItemChanged(ProfileFragment.UserPostAdapter.PostList.IndexOf(dataPostUser));
                            }
                        }
                        else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                        {
                            var dataPostUser = TikProfileFragment?.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == id);
                            if (dataPostUser != null)
                            {
                                dataPostUser.Description = text;
                                TikProfileFragment.MyPostTab?.MAdapter.NotifyItemChanged(TikProfileFragment.MyPostTab.MAdapter.PostList.IndexOf(dataPostUser));
                            }
                        }

                        var currentFragment = FragmentBottomNavigator.GetSelectedTabLastStackFragment();
                        if (currentFragment is ImagePostFragment frmImage)
                        {
                            frmImage.Description.SetAutoLinkText(text);
                            frmImage.Description.Text = text;
                        }
                        else if (currentFragment is GifPostFragment frmGif)
                        {
                            frmGif.Description.SetAutoLinkText(text);
                            frmGif.Description.Text = text;
                        }
                        else if (currentFragment is MultiImagePostFragment frmMulti)
                        {
                            frmMulti.Description.SetAutoLinkText(text);
                            frmMulti.Description.Text = text;
                        }
                        else if (currentFragment is VideoPostFragment frmVideo)
                        {
                            frmVideo.Description.SetAutoLinkText(text);
                            frmVideo.Description.Text = text;
                        }
                        else if (currentFragment is YoutubePostFragment frmYoutube)
                        {
                            frmYoutube.Description.SetAutoLinkText(text);
                            frmYoutube.Description.Text = text;
                        }
                        else if (currentFragment is HashTagPostFragment frmHashTag)
                        {
                            var dataHash = frmHashTag.MAdapter?.PixelNewsFeedList?.FirstOrDefault(a => a.PostId == id);
                            if (dataHash != null)
                            {
                                dataHash.Description = text;
                                frmHashTag.MAdapter.NotifyItemChanged(frmHashTag.MAdapter.PixelNewsFeedList.IndexOf(dataHash));
                            }
                        }
                    });
                }
                else if (requestCode == 503 && resultCode == Result.Ok) // Add story using camera
                {
                    try
                    {
                        if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                        }
                        else
                        {
                            if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                            {
                                var check = AppTools.CheckMimeTypesWithServer(IntentController.CurrentPhotoPath);
                                if (!check)
                                {
                                    //this file not supported on the server , please select another file 
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                                    return;
                                }
                                 
                                //Do something with your Uri
                                Intent intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", IntentController.CurrentPhotoPath);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                    } 
                }
                else if (requestCode == 501 && resultCode == Result.Ok) // Add video story
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = AppTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            Intent intent = new Intent(this, typeof(AddStoryActivity));
                            intent.PutExtra("Uri", filepath);
                            intent.PutExtra("Type", "video");
                            StartActivity(intent);
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                    }
                }
                else if (requestCode == 500 && resultCode == Result.Ok) // Add image story
                {
                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = AppTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                            return;
                        }

                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Image")
                        {
                            if (!string.IsNullOrEmpty(filepath))
                            {
                                //Do something with your Uri
                                Intent intent = new Intent(this, typeof(AddStoryActivity));
                                intent.PutExtra("Uri", filepath);
                                intent.PutExtra("Type", "image");
                                StartActivity(intent);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            }
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                    }
                }
                else if (requestCode == 108 || requestCode == CropImage.CropImageActivityRequestCode)
                {
                    if (Methods.CheckConnectivity())
                    {
                        var result = CropImage.GetActivityResult(data);

                        if (result.IsSuccessful)
                        {
                            var resultPathImage = result.Uri.Path;
                            if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                            {
                                if (ProfileFragment.UserProfileImage == null)
                                    return;

                                if (!string.IsNullOrEmpty(resultPathImage))
                                    GlideImageLoader.LoadImage(this, resultPathImage, ProfileFragment.UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Drawable);
                            }
                            else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                            {
                                if (TikProfileFragment.ImageUser == null)
                                    return;
                                if (!string.IsNullOrEmpty(resultPathImage))
                                    GlideImageLoader.LoadImage(this, resultPathImage, TikProfileFragment.ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                            }

                            if (!string.IsNullOrEmpty(resultPathImage))
                            {
                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.Avatar = resultPathImage;

                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                                    dbDatabase.Dispose();
                                }

                                var dataStory = NewsFeedFragment.StoryAdapter?.StoryList?.FirstOrDefault(a => a.Type == "Your");
                                if (dataStory != null)
                                {
                                    dataStory.Avatar = resultPathImage;
                                    NewsFeedFragment.StoryAdapter.NotifyItemChanged(0);
                                }

                                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.SaveImageUser(resultPathImage) });
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long).Show();
                            }
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
                else if (requestCode == 2200 && resultCode == Result.Ok) // => NiceArtEditor add story text
                {
                    RunOnUiThread(() =>
                    {
                        var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                        if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                        {
                            //Do something with your Uri
                            Intent intent = new Intent(this, typeof(AddStoryActivity));
                            intent.PutExtra("Uri", imagePath);
                            intent.PutExtra("Type", "image");
                            StartActivity(intent);
                        }
                    });
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
                    if (TypeOpen == "StoryImage")
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        {
                            if (!AppSettings.ImageCropping)
                                //requestCode >> 500 => Image Gallery
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                            else
                                OpenDialogGallery("StoryImage");
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        }
                    }
                    else if (TypeOpen == "MyProfile")
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        {
                            OpenDialogGallery("MyProfile");
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        }
                    }
                    else if (TypeOpen == "StoryVideo")
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        }
                    }
                    else if (TypeOpen == "StoryCamera")
                    {
                        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                        {
                            //requestCode >> 503 => Camera Gallery
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        }
                    }
                }
                else if (requestCode == 555) // => Open AddPostActivity
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Intent intPost = new Intent(this, typeof(AddPostActivity));
                        if (PType == 0) // image
                        {
                            intPost.PutExtra("TypePost", "Image");
                        }
                        else if (PType == 1) // video
                        {
                            intPost.PutExtra("TypePost", "Video");
                        }
                        else if (PType == 2) // gif
                        {
                            intPost.PutExtra("TypePost", "Gif");
                        }
                        else if (PType == 3) // broken_link
                        {
                            intPost.PutExtra("TypePost", "EmbedVideo");
                        }

                        StartActivityForResult(intPost, 2500);
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Add Post 

        private async Task AddPostUrlVideoApi(PostsObject dataPosts, string url)
        {
            if (Methods.CheckConnectivity())
            {
                (int apiStatus, var respond) = await RequestsAsync.Post.PostVideoFrom(url, dataPosts.Description);
                if (apiStatus == 200)
                {
                    if (respond is MessageIdObject messageIdObject)
                    {
                        RunOnUiThread(() =>
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Post_Added), ToastLength.Long).Show();

                            int id = messageIdObject.Id;
                            var dataPost = NewsFeedFragment.NewsFeedAdapter?.PixelNewsFeedList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                            if (dataPost != null)
                            {
                                dataPost.PostId = id;
                            }

                            if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                            {
                                var dataPostUser = ProfileFragment.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                if (dataPostUser != null)
                                {
                                    dataPostUser.PostId = id;
                                }
                            }
                            else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                            {
                                var dataPostUser = TikProfileFragment?.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                if (dataPostUser != null)
                                {
                                    dataPostUser.PostId = id;
                                }
                            }

                        });
                    }
                }
                else Methods.DisplayReportResult(this, respond);
            }
            else
            {
                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
        }

        private async void AddPostMediaApi(PostsObject dataPosts, string url)
        {
            try
            {
                if (dataPosts != null)
                {
                    var content = dataPosts.Description;
                    string typePost = dataPosts.Type;

                    if (typePost == Classes.TypePostEnum.Image.ToString()) // image
                    {
                        typePost = "images[]";
                    }
                    else if (typePost == Classes.TypePostEnum.Video.ToString()) // video
                    {
                        typePost = "video";
                    }
                    else if (typePost == Classes.TypePostEnum.Gif.ToString()) // gif
                    {
                        typePost = "gif_url";
                    }
                    else if (typePost == Classes.TypePostEnum.EmbedVideo.ToString()) // EmbedVideo
                    {
                        typePost = "youtube";
                    }

                    if (typePost == "youtube")
                    {
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => AddPostUrlVideoApi(dataPosts, url) });
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(content) && dataPosts.MediaSet.Count == 0)
                        {
                            Toast.MakeText(this, GetString(Resource.String.Lbl_YouCannot_PostanEmptyPost), ToastLength.Long).Show();
                        }
                        else
                        {
                            if (Methods.CheckConnectivity())
                            {
                                List<Attachments> list = new List<Attachments>();
                                if (typePost != "gif_url")
                                {
                                    list.AddRange(dataPosts.MediaSet.Select(item => new Attachments()
                                    {
                                        FileUrl = item.File,
                                        TypeAttachment = typePost,
                                        Id = item.Id,
                                        FileSimple = item.File,
                                        Thumb = new Attachments.VideoThumb() {FileUrl = item.Extra,},
                                    }));
                                }
                                else
                                {
                                    url = dataPosts.MediaSet[0].File;
                                }

                                (int apiStatus, var respond) = await RequestsAsync.Post.PostMedia(dataPosts.Type, list, content, url).ConfigureAwait(false);
                                if (apiStatus == 200)
                                {
                                    if (respond is MessageIdObject messageIdObject)
                                    {
                                        RunOnUiThread(() =>
                                        {
                                            try
                                            {
                                                Toast.MakeText(this, GetText(Resource.String.Lbl_Post_Added), ToastLength.Long).Show();

                                                // put the String to pass back into an Intent and close this activity 
                                                int id = messageIdObject.Id;
                                                var dataPost = NewsFeedFragment.NewsFeedAdapter?.PixelNewsFeedList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                                if (dataPost != null)
                                                {
                                                    dataPost.PostId = id;
                                                }

                                                if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                                                {
                                                    var dataPostUser = ProfileFragment.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                                    if (dataPostUser != null)
                                                    {
                                                        dataPostUser.PostId = id;
                                                    }
                                                }
                                                else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                                                {
                                                    var dataPostUser = TikProfileFragment.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                                    if (dataPostUser != null)
                                                    {
                                                        dataPostUser.PostId = id;
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine(e);
                                            }
                                        });
                                    }
                                }
                                else
                                {
                                    Methods.DisplayReportResult(this, respond);

                                    RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            Toast.MakeText(this, GetText(Resource.String.Lbl_PostFailedUpload), ToastLength.Short).Show();

                                            var dataPost = NewsFeedFragment.NewsFeedAdapter?.PixelNewsFeedList?.FirstOrDefault(a => a.PostId == dataPosts.PostId);
                                            if (dataPost != null)
                                            {
                                                NewsFeedFragment.NewsFeedAdapter?.PixelNewsFeedList.Remove(dataPost);
                                                int index = NewsFeedFragment.NewsFeedAdapter.PixelNewsFeedList.IndexOf(dataPosts);
                                                NewsFeedFragment.NewsFeedAdapter?.NotifyItemRemoved(index);
                                                NewsFeedFragment.NewsFeedAdapter?.NotifyDataSetChanged();
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    });
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        public void OpenDialogGallery(string type)
        {
            try
            {
                TypeOpen = type;
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Methods.Path.Chack_MyFolder();

                    //Open Image 
                    var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                    CropImage.Builder()
                        .SetInitialCropWindowPaddingRatio(0)
                        .SetAutoZoomEnabled(true)
                        .SetMaxZoom(4)
                        .SetGuidelines(CropImageView.Guidelines.On)
                        .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Done))
                        .SetOutputUri(myUri).Start(this);
                }
                else
                {
                    if (!CropImage.IsExplicitCameraPermissionRequired(this) && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted && CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted)
                    {
                        Methods.Path.Chack_MyFolder();

                        //Open Image 
                        var myUri = Uri.FromFile(new File(Methods.Path.FolderDcimImage, Methods.GetTimestamp(DateTime.Now) + ".jpeg"));
                        CropImage.Builder()
                            .SetInitialCropWindowPaddingRatio(0)
                            .SetAutoZoomEnabled(true)
                            .SetMaxZoom(4)
                            .SetGuidelines(CropImageView.Guidelines.On)
                            .SetCropMenuCropButtonTitle(GetText(Resource.String.Lbl_Done))
                            .SetOutputUri(myUri).Start(this);
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(108);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OpenEditColor()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditColorActivity));
                StartActivityForResult(intent, 2200);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void FundingAdaptersOnItemClick(object sender, FundingAdaptersViewHolderClickEventArgs e)
        {
            try
            {
                var item = NewsFeedFragment.NewsFeedAdapter?.HolderFunding?.FundingAdapters.GetItem(e.Position);
                if (item != null)
                {
                    Intent intent = new Intent(this, typeof(FundingViewActivity));
                    intent.PutExtra("ItemObject", JsonConvert.SerializeObject(item));
                    StartActivity(intent);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Service Chat

        public void SetService(bool run = true)
        {
            try
            {
                if (run)
                {
                    try
                    {
                        Receiver = new ServiceResultReceiver(new Handler());
                        Receiver.SetReceiver(this);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    var intent = new Intent(this, typeof(ScheduledApiService));
                    intent.PutExtra("receiverTag", Receiver);
                    StartService(intent);
                }
                else
                {
                    var intentService = new Intent(this, typeof(ScheduledApiService));
                    StopService(intentService);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<GetChatsObject>(resultData.GetString("Json"));
                if (result != null)
                {
                    LastChatActivity.GetInstance()?.LoadDataJsonLastChat(result);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                // Toast.MakeText(Application.Context, "Exception  " + e, ToastLength.Short).Show();
            }
        }

        #endregion

    }
}