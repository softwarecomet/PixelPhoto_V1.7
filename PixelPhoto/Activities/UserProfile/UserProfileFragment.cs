using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Bumptech.Glide.Integration.RecyclerView;
using Bumptech.Glide.Util;
using Com.Luseen.Autolinklibrary;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.Tabbes.Adapters;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;
using Object = Java.Lang.Object;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.UserProfile
{
    public class UserProfileFragment : Fragment, AppBarLayout.IOnOffsetChangedListener, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private ImageView UserProfileImage;
        private TextView TxtCountFollowers,TxtCountFollowing,TxtCountFav,TxtFollowers,TxtFollowing,TxtFav,Fullname,Username, FeedTextView, IconVerified, IconBusiness;
        private ImageButton Morebutton;
        private AutoLinkTextView About; 
        private LinearLayout AboutLiner,LinFollowers, LinFollowing;
        private Button WebsiteButton, SocialGoogle , SocialFacebook, SocialTwitter, SocialLinkedIn;
        private Button FollowButton, MessageButton;
        private ProgressBar ProgressBarLoader;
        private ViewStub EmptyStateLayout;
        private RecyclerView ProfileFeedRecylerView;
        private UserPostAdapter PixUserFeedAdapter;
        private AppBarLayout AppBarLayoutView;
        private HomeActivity MainContext;
        private CollapsingToolbarLayout CollapsingToolbarLayoutView;
        private RecyclerViewOnScrollListener MainScrollEvent;
        private CommentObject UserinfoComment;
        private UserDataObject UserinfoData;
        private UserDataObject UserinfoOneSignal;
        private string UserId, FullName, Avatar, Type, Url, PPrivacy = "1";
        private bool SIsFollowing;
        private string Twitter, Facebook, Google , Website;
        private TextSanitizer TextSanitizerAutoLink;
        private View Inflated;

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
                Type = Arguments.GetString("type");
                string json = Arguments.GetString("userinfo");
                UserId = Arguments.GetString("userid");
                Avatar = Arguments.GetString("avatar");
                FullName = Arguments.GetString("fullname");

                View view = inflater.Inflate(Resource.Layout.Pix_UserProfileLayout, container, false);
                  
                //Get Value And Set Toolbar
                InitComponent(view);
                InitToolbar(view);
                SetRecyclerViewAdapters();

                FeedTextView.Click += FeedTextViewOnClick;
                PixUserFeedAdapter.ItemClick += PixUserFeedAdapter_ItemClick;
                SocialGoogle.Click += BtnGoogleOnClick;
                SocialFacebook.Click += BtnFacebookOnClick;
                SocialTwitter.Click += BtnTwitterOnClick; 
                WebsiteButton.Click += WebsiteButtonOnClick;
                FollowButton.Click += FollowButtonOnClick;
                MessageButton.Click += MessageButtonOnClick;
                LinFollowers.Click += LinFollowersOnClick;
                LinFollowing.Click += LinFollowingOnClick;
                Morebutton.Click += IconMoreOnClick;

                //Get Data Api
                LoadProfile(json, Type);
                LoadExploreFeed();

                AdsGoogle.Ad_RewardedVideo(Context);

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
         
        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ProfileFeedRecylerView = (RecyclerView)view.FindViewById(Resource.Id.RecylerFeed);
                AppBarLayoutView = (AppBarLayout)view.FindViewById(Resource.Id.appBarLayout);
                CollapsingToolbarLayoutView = (CollapsingToolbarLayout)view.FindViewById(Resource.Id.collapsingToolbarLayout);
                FeedTextView = (TextView)view.FindViewById(Resource.Id.feed);
                ProgressBarLoader = (ProgressBar)view.FindViewById(Resource.Id.sectionProgress);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);

                LinFollowers = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowers);
                LinFollowing = view.FindViewById<LinearLayout>(Resource.Id.layoutFollowing);

                UserProfileImage = (ImageView)view.FindViewById(Resource.Id.user_pic);
                Fullname = (TextView)view.FindViewById(Resource.Id.fullname);
                Username = (TextView)view.FindViewById(Resource.Id.username);
                TxtCountFollowers = (TextView)view.FindViewById(Resource.Id.CountFollowers);
                TxtCountFollowing = (TextView)view.FindViewById(Resource.Id.CountFollowing);
                TxtCountFav = (TextView)view.FindViewById(Resource.Id.CountFav);
                TxtFollowers = view.FindViewById<TextView>(Resource.Id.txtFollowers);
                TxtFollowing = view.FindViewById<TextView>(Resource.Id.txtFollowing);
                TxtFav = view.FindViewById<TextView>(Resource.Id.txtFav);
                SocialGoogle = view.FindViewById<Button>(Resource.Id.social1);
                SocialFacebook = view.FindViewById<Button>(Resource.Id.social2);
                SocialTwitter = view.FindViewById<Button>(Resource.Id.social3);
                SocialLinkedIn = view.FindViewById<Button>(Resource.Id.social4);
                WebsiteButton = view.FindViewById<Button>(Resource.Id.website);
                FollowButton = view.FindViewById<Button>(Resource.Id.followButton);
                MessageButton = view.FindViewById<Button>(Resource.Id.messageButton);
                About = view.FindViewById<AutoLinkTextView>(Resource.Id.aboutdescUser);
                AboutLiner = view.FindViewById<LinearLayout>(Resource.Id.aboutliner);
                Morebutton = view.FindViewById<ImageButton>(Resource.Id.morebutton);

                IconVerified = (TextView)view.FindViewById(Resource.Id.verified);
                IconBusiness = (TextView)view.FindViewById(Resource.Id.business);

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconVerified, IonIconsFonts.CheckmarkCircled);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBusiness, IonIconsFonts.SocialUsd);

                IconVerified.Visibility = ViewStates.Gone;
                IconBusiness.Visibility = ViewStates.Gone;

                Morebutton.SetColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                //UserProfileImage.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.layout_bg_profile_friends_dark : Resource.Drawable.layout_bg_profile_friends);

                TextSanitizerAutoLink = new TextSanitizer(About, Activity);

                TextView viewboxText = view.FindViewById<TextView>(Resource.Id.Appname);
                viewboxText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                TxtCountFollowers.Text = "0";
                TxtCountFollowing.Text = "0";
                TxtCountFav.Text = "0";

                SocialLinkedIn.Visibility = ViewStates.Gone;
                AppBarLayoutView.AddOnOffsetChangedListener(this); 
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
                MainContext.SetToolBar(toolBar, " ");
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
                PixUserFeedAdapter = new UserPostAdapter(Activity);
                var mLayoutManager = new GridLayoutManager(Activity, 3);
                ProfileFeedRecylerView.SetLayoutManager(mLayoutManager);
                ProfileFeedRecylerView.AddItemDecoration(new GridSpacingItemDecoration(1, 1, true));
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<PostsObject>(Activity, PixUserFeedAdapter, sizeProvider, 8);
                ProfileFeedRecylerView.AddOnScrollListener(preLoader);
                ProfileFeedRecylerView.SetAdapter(PixUserFeedAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(mLayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += OnScroll_OnLoadMoreEvent;
                ProfileFeedRecylerView.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Load Profile
         
        private void LoadProfile(string json, string type)
        {
            try
            {
                if (!string.IsNullOrEmpty(json))
                {
                    switch (type)
                    {
                        case "comment":
                            UserinfoComment = JsonConvert.DeserializeObject<CommentObject>(json);
                            LoadUserData(UserinfoComment); 
                            break;
                        case "UserData": 
                            UserinfoData = JsonConvert.DeserializeObject<UserDataObject>(json);
                            LoadUserData(UserinfoData);
                            Url = UserinfoData.Url;
                            break; 
                        case "OneSignalNotification":
                            UserinfoOneSignal = JsonConvert.DeserializeObject<UserDataObject>(json);
                            LoadUserData(UserinfoOneSignal);
                            break;
                        default:
                            GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                            Fullname.Text = Arguments.GetString("fullname");
                            break;
                    }
                }
                else
                {
                    GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                    Fullname.Text = Arguments.GetString("fullname");  
                }

                //Add Api 
                StartApiService();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void LoadUserData(CommentObject cl)
        {
            try
            { 
                GlideImageLoader.LoadImage(Activity, cl.Avatar, UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                UserProfileImage.SetScaleType(ImageView.ScaleType.CenterCrop);
                Username.Text = "@" + cl.Username;
                Fullname.Text = Methods.FunString.DecodeString(cl.Name);  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         

        private void LoadUserData(UserDataObject cl,bool friends = true)
        {
            try
            {
                PPrivacy = cl.PPrivacy;

                GlideImageLoader.LoadImage(Activity, cl.Avatar, UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                UserProfileImage.SetScaleType(ImageView.ScaleType.CenterCrop);

                TextSanitizerAutoLink.Load(AppTools.GetAboutFinal(cl));
                AboutLiner.Visibility = ViewStates.Visible;

                Username.Text = "@" + cl.Username;
                Fullname.Text = AppTools.GetNameFinal(cl);

                IconVerified.Visibility = cl.Verified == "1" ? ViewStates.Visible : ViewStates.Gone;

                IconBusiness.Visibility = cl.BusinessAccount == "1" ? ViewStates.Visible : ViewStates.Gone;

                Typeface font = Typeface.CreateFromAsset(Application.Context.Resources.Assets, "ionicons.ttf");

                TxtCountFav.Text = Methods.FunString.FormatPriceValue(Int32.Parse(cl.PostsCount));
                TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(cl.Followers));
                TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(Convert.ToInt32(cl.Following));

                if (!string.IsNullOrEmpty(cl.Google))
                {
                    Google = cl.Google;
                    SocialGoogle.SetTypeface(font, TypefaceStyle.Normal);
                    SocialGoogle.Text = IonIconsFonts.SocialGoogle;
                    SocialGoogle.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Facebook))
                {
                    Facebook = cl.Facebook;
                    SocialFacebook.SetTypeface(font, TypefaceStyle.Normal);
                    SocialFacebook.Text = IonIconsFonts.SocialFacebook;
                    SocialFacebook.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Website))
                {
                    Website = cl.Website;
                    WebsiteButton.SetTypeface(font, TypefaceStyle.Normal);
                    WebsiteButton.Text = IonIconsFonts.AndroidGlobe;
                    WebsiteButton.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Twitter))
                {
                    Twitter = cl.Twitter;
                    SocialTwitter.SetTypeface(font, TypefaceStyle.Normal);
                    SocialTwitter.Text = IonIconsFonts.SocialTwitter;
                    SocialTwitter.Visibility = ViewStates.Visible;
                }

                if (cl.IsFollowing != null)
                {
                    SIsFollowing = cl.IsFollowing.Value;
                    if (!friends) return;

                    if (cl.IsFollowing.Value) // My Friend
                    {
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Grey_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#000000"));
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                        FollowButton.Tag = "true";
                    }
                    else
                    {
                        //Not Friend
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Gradient_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#ffffff"));
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                        FollowButton.Tag = "false";
                    }

                    MessageButton.Visibility = cl.CPrivacy == "1" || cl.CPrivacy == "2" && cl.IsFollowing.Value
                        ? ViewStates.Visible
                        : ViewStates.Invisible;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetUserProfileApi });
        }
        private async Task GetUserProfileApi()
        {
            if (Methods.CheckConnectivity())
            {
                (int respondCode, var respondString) = await RequestsAsync.User.FetchUserData(UserId);
                if (respondCode == 200)
                {
                    if (respondString is FetchUserDataObject result)
                    {
                        if (result.Data != null)
                        {
                            UserinfoData = result.Data;
                            Url = UserinfoData.Url;
                            LoadUserData(result.Data);
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity,respondString);
            }
        }

        #endregion

        #region Events

        private void IconMoreOnClick(object sender, EventArgs e)
        {
            try
            {
                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(Activity).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);

                arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Report));
                arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_Block));
                arrayAdapter.Add(Activity.GetText(Resource.String.Lbl_CopyLinkToProfile));

                dialogList.Items(arrayAdapter);
                dialogList.PositiveText(Activity.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void MessageButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(Context, typeof(MessagesBoxActivity));
                intent.PutExtra("UserId", UserId);
                intent.PutExtra("TypeChat", Type);
                if (UserinfoData != null)
                    intent.PutExtra("UserItem", JsonConvert.SerializeObject(UserinfoData));
                 
                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    Activity.StartActivity(intent);
                }
                else
                {
                    //Check to see if any permission in our group is available, if one, then all are
                    if (Context.CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                        Context.CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        Activity.StartActivity(intent); 
                    }
                    else
                        new PermissionsController(Activity).RequestPermission(100);

                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void FollowButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {

                    if (FollowButton.Tag?.ToString() == "true")
                    {
                        FollowButton.Tag = "false";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Gradient_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#ffffff"));
                    }
                    else
                    {
                        FollowButton.Tag = "true";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                        FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Grey_Btn);
                        FollowButton.SetTextColor(Color.ParseColor("#000000"));
                    }

                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollow(UserId) }); 
                }
                else
                {
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void LinFollowingOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Following");

                MyContactsFragment myContactsFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                MainContext.OpenFragment(myContactsFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            } 
        }

        private void LinFollowersOnClick(object sender, EventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();

                bundle.PutString("UserId", UserId);
                bundle.PutString("UserType", "Followers");

                MyContactsFragment profileFragment = new MyContactsFragment
                {
                    Arguments = bundle
                };

                MainContext.OpenFragment(profileFragment);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

       
        private void BtnGoogleOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenAppOnGooglePlay(Google);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BtnTwitterOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenTwitterIntent(Twitter);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void BtnFacebookOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                    new IntentController(Activity).OpenFacebookIntent(Activity, Facebook);
                else
                    Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void WebsiteButtonOnClick(object sender, EventArgs e)
        {
            if (Methods.CheckConnectivity())
                new IntentController(Activity).OpenBrowserFromPhone(Website);
            else
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
        }


        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                    ProgressBarLoader.Visibility = ViewStates.Visible;

                    StartApiServiceLoadFeedJson();
                }
                else
                {
                    EmptyStateLayout.Visibility = ViewStates.Visible;
                    Toast.MakeText(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void PixUserFeedAdapter_ItemClick(object sender, UserPostAdapterViewHolderClickEventArgs e)
        {
            try
            {
                var item = PixUserFeedAdapter.PostList[e.Position];
                if (item != null)
                {
                    MainContext.OpenNewsFeedItem(item);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void FeedTextViewOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                MainContext.FragmentNavigatorBack();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Permissions

        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                if (requestCode == 100)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        Intent intent = new Intent(Context, typeof(MessagesBoxActivity));
                        intent.PutExtra("UserId", UserId);
                        intent.PutExtra("TypeChat", Type);
                        if (UserinfoData != null)
                            intent.PutExtra("UserItem", JsonConvert.SerializeObject(UserinfoData));
                      
                        Context.StartActivity(intent);
                    }
                    else
                    {
                        Toast.MakeText(Context, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
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

        #region Load Explore Feed

        private void StartApiServiceLoadFeedJson(string offset = "0")
        {
            if (!Methods.CheckConnectivity())
                Toast.MakeText(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadFeedAsync(offset) });
        }

        private void LoadExploreFeed()
        {
            try
            {
                if (PPrivacy == "2" || PPrivacy == "1" && SIsFollowing)
                {
                    if (Methods.CheckConnectivity())
                    {
                        ProgressBarLoader.Visibility = ViewStates.Visible;

                        StartApiServiceLoadFeedJson();  
                    }
                    else
                    {
                        if (ProgressBarLoader.Visibility != ViewStates.Gone)
                            ProgressBarLoader.Visibility = ViewStates.Gone;

                        if (Inflated == null)
                            Inflated = EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += TryAgainButton_Click;
                        }
                    }
                }
                else  
                {
                    if (ProgressBarLoader.Visibility != ViewStates.Gone)
                        ProgressBarLoader.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.ProfilePrivate);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                        x.EmptyStateButton.Click += TryAgainButton_Click;
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void PopluteUserDataObjectClass(FetchUserPostsByUserIdObject cl)
        {
            try
            {
                GlideImageLoader.LoadImage(Activity, cl.data.UserData.Avatar, UserProfileImage, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
               

                if (!string.IsNullOrEmpty(cl.data.UserData.About))
                {
                    About.Text = Methods.FunString.DecodeString(cl.data.UserData.About);
                    AboutLiner.Visibility = ViewStates.Visible;
                }

                TxtCountFollowers.Text = Methods.FunString.FormatPriceValue(cl.data.UserFollowers);
                TxtCountFollowing.Text = Methods.FunString.FormatPriceValue(cl.data.UserFollowing);
                TxtCountFav.Text = Methods.FunString.FormatPriceValue(cl.data.TotalPosts);
                Username.Text = "@" + cl.data.UserData.Username;
                Fullname.Text = Methods.FunString.DecodeString(cl.data.UserData.Name);

                Typeface font = Typeface.CreateFromAsset(Application.Context.Resources.Assets, "ionicons.ttf");

                if (!string.IsNullOrEmpty(cl.data.UserData.Google))
                {
                    Google = cl.data.UserData.Google;
                    SocialGoogle.SetTypeface(font, TypefaceStyle.Normal);
                    SocialGoogle.Text = IonIconsFonts.SocialGoogle;
                    SocialGoogle.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.data.UserData.Facebook))
                {
                    Facebook = cl.data.UserData.Facebook;
                    SocialFacebook.SetTypeface(font, TypefaceStyle.Normal);
                    SocialFacebook.Text = IonIconsFonts.SocialFacebook;
                    SocialFacebook.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.data.UserData.Website))
                {
                    Website = cl.data.UserData.Website;
                    WebsiteButton.SetTypeface(font, TypefaceStyle.Normal);
                    WebsiteButton.Text = IonIconsFonts.AndroidGlobe;
                    WebsiteButton.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.data.UserData.Twitter))
                {
                    Twitter = cl.data.UserData.Twitter;
                    SocialTwitter.SetTypeface(font, TypefaceStyle.Normal);
                    SocialTwitter.Text = IonIconsFonts.SocialTwitter;
                    SocialTwitter.Visibility = ViewStates.Visible;
                }


                if (cl.data.IsFollowing) // My Friend
                {
                    FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Grey_Btn);
                    FollowButton.SetTextColor(Color.ParseColor("#000000"));
                    FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);

                    FollowButton.Tag = "true";
                }
                else 
                {
                    //Not Friend
                    FollowButton.SetBackgroundResource(Resource.Drawable.Shape_Radius_Gradient_Btn);
                    FollowButton.SetTextColor(Color.ParseColor("#ffffff"));
                    FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                    FollowButton.Tag = "false";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task LoadFeedAsync(string offset = "")
        {
            if (MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                MainScrollEvent.IsLoading = true;

                int countList = PixUserFeedAdapter.PostList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Post.FetchUserPostsById(UserId, "24", offset);
                if (apiStatus != 200 || !(respond is FetchUserPostsByUserIdObject result) || result.data == null)
                {
                    Methods.DisplayReportResult(Activity, respond);
                }
                else
                {
                    PopluteUserDataObjectClass(result);

                    var respondList = result.data.UserPosts.Count;
                    if (respondList > 0)
                    {
                        result.data.UserPosts.RemoveAll(a => a.MediaSet?.Count == 0 && a.MediaSet == null);

                        if (countList > 0)
                        {
                            foreach (var item in from item in result.data.UserPosts let check = PixUserFeedAdapter.PostList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                            {
                                item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);
                                PixUserFeedAdapter.PostList.Add(item);
                            }

                            Activity.RunOnUiThread(() => { PixUserFeedAdapter.NotifyItemRangeInserted(countList - 1, PixUserFeedAdapter.PostList.Count - countList); });
                        }
                        else
                        {
                            PixUserFeedAdapter.PostList = new ObservableCollection<PostsObject>(result.data.UserPosts);
                            Activity.RunOnUiThread(() => { PixUserFeedAdapter.NotifyDataSetChanged(); });
                        }
                    }
                    else
                    {
                        if (PixUserFeedAdapter.PostList.Count > 10 && !ProfileFeedRecylerView.CanScrollVertically(1))
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

                ProgressBarLoader.Visibility = ViewStates.Gone;

                if (PixUserFeedAdapter.PostList.Count > 0)
                {
                    ProfileFeedRecylerView.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                     
                }
                else
                {
                    ProfileFeedRecylerView.Visibility = ViewStates.Gone;

                    if (Inflated == null)
                        Inflated = EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoPost);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                    }
                    EmptyStateLayout.Visibility = ViewStates.Visible;

                    //Remove behavior from AppBarLayout
                    if (ViewCompat.IsLaidOut(AppBarLayoutView))
                    {
                        CoordinatorLayout.LayoutParams appBarLayoutParams = (CoordinatorLayout.LayoutParams)AppBarLayoutView.LayoutParameters;
                        AppBarLayout.Behavior behavior = (AppBarLayout.Behavior)appBarLayoutParams.Behavior;
                        behavior.SetDragCallback(new DragRemover());
                    } 
                }
            }
            catch (Exception e)
            {
                ProgressBarLoader.Visibility = ViewStates.Gone;
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

        #region Drag

        private class DragRemover : AppBarLayout.Behavior.DragCallback
        {
            public override bool CanDrag(Object appBarLayout)
            {
                return false;
            }
        }

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            try
            {
                if (appBarLayout.TotalScrollRange + verticalOffset == 0)
                {
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, FeedTextView, IonIconsFonts.AndroidArrowBack);
                    FeedTextView.SetTextSize(ComplexUnitType.Dip, 23f);
                }
                else
                {
                    FeedTextView.Text = " ";
                    FeedTextView.SetTextSize(ComplexUnitType.Dip, 13f);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }



        #endregion
 
        #region Scroll

        private void OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {  
                var item = PixUserFeedAdapter.PostList.LastOrDefault();
                if (item != null && PixUserFeedAdapter.PostList.Count > 10 && !MainScrollEvent.IsLoading)
                    StartApiServiceLoadFeedJson(item.PostId.ToString());
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region MaterialDialog
         
        public void OnSelection(MaterialDialog p0, View p1, int p2, ICharSequence itemString)
        {
            try
            {
                if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Report))
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Sent Api >>
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.ReportUnReportUser(UserId, "4") });
                         
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Block))
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Sent Api >>
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.BlockUnblock(UserId) });
                         
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_Blocked_successfully),ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    } 
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_CopyLinkToProfile))
                {
                    ClipboardManager clipboard = (ClipboardManager)MainContext.GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("clipboard", Url);
                    clipboard.PrimaryClip = clip;

                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short).Show();
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
    }
}