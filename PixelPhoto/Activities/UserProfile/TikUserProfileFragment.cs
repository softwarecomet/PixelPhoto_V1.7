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
using Android.Views;
using Android.Widget;
using Com.Tuyenmonkey.Textdecorator;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Activities.MyContacts;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.UserProfile.Fragments;
using PixelPhoto.Adapters;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using Fragment = Android.Support.V4.App.Fragment;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.UserProfile
{
    public class TikUserProfileFragment : Fragment, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region  Variables Basic

        private HomeActivity GlobalContext;
        private TextView IconBack, TxtUserName, BtnMore, TxtFullName, TxtFollowingCount, TxtFollowersCount, TxtPostCount, BtnMessage;
        private Button FollowButton;
        private ImageView ImageUser;
        private LinearLayout FollowingLayout, FollowersLayout, PostLayout;
        private TabLayout TabLayout;
        private ViewPager ViewPager;
        private UserPostFragment PostTab;
        private AboutFragment AboutTab;
        private CollapsingToolbarLayout CollapsingToolbar;
        public string UserId, Json, Type, Url, PPrivacy = "1";
        private bool SIsFollowing;
        private CommentObject UserinfoComment;
        public UserDataObject UserinfoData;
        private UserDataObject UserinfoOneSignal;

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
                View view = inflater.Inflate(Resource.Layout.TikUserProfileLayout, container, false);

                Type = Arguments.GetString("type");
                Json = Arguments.GetString("userinfo");
                UserId = Arguments.GetString("userid");

                InitComponent(view);
                InitToolbar(view);
                AddOrRemoveEvent(true);

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

                IconBack = view.FindViewById<TextView>(Resource.Id.back_btn);
                TxtUserName = view.FindViewById<TextView>(Resource.Id.username);
                BtnMore = view.FindViewById<TextView>(Resource.Id.more_btn);

                ImageUser = view.FindViewById<ImageView>(Resource.Id.user_image);
                TxtFullName = view.FindViewById<TextView>(Resource.Id.fullname);

                FollowingLayout = view.FindViewById<LinearLayout>(Resource.Id.following_layout);
                TxtFollowingCount = view.FindViewById<TextView>(Resource.Id.following_count_txt);

                FollowersLayout = view.FindViewById<LinearLayout>(Resource.Id.followers_layout);
                TxtFollowersCount = view.FindViewById<TextView>(Resource.Id.followers_count_txt);

                PostLayout = view.FindViewById<LinearLayout>(Resource.Id.post_layout);
                TxtPostCount = view.FindViewById<TextView>(Resource.Id.post_count_txt);

                BtnMessage = view.FindViewById<TextView>(Resource.Id.message_btn);
                FollowButton = view.FindViewById<Button>(Resource.Id.add_btn);

                ViewPager = view.FindViewById<ViewPager>(Resource.Id.pager);
                TabLayout = view.FindViewById<TabLayout>(Resource.Id.tabs);

                TxtFollowingCount.Text = "0";
                TxtFollowersCount.Text = "0";
                TxtPostCount.Text = "0";
                  
                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                TabLayout.SetupWithViewPager(ViewPager);

                TabLayout.GetTabAt(0).SetIcon(Resource.Drawable.ic_tab_more);
                TabLayout.GetTabAt(1).SetIcon(Resource.Drawable.ic_tab_user_profile);

                // set icon color pre-selected
                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));
                TabLayout.GetTabAt(1).Icon.SetColorFilter(new PorterDuffColorFilter(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray, PorterDuff.Mode.SrcIn));

                TabLayout.GetTabAt(0).Icon.SetColorFilter(new PorterDuffColorFilter(Color.ParseColor(AppSettings.MainColor), PorterDuff.Mode.SrcIn));

                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.AndroidArrowBack);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, BtnMore, IonIconsFonts.AndroidMoreVertical);
                FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, BtnMessage, FontAwesomeIcon.PaperPlane);

                TxtUserName.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                BtnMore.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                TxtFullName.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                if (AppSettings.FlowDirectionRightToLeft)
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, IconBack, IonIconsFonts.AndroidArrowForward);

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
                    IconBack.Click += IconBackOnClick;
                    BtnMore.Click += BtnMoreOnClick;
                    BtnMessage.Click += BtnMessageOnClick;
                    FollowButton.Click += BtnAddOnClick;
                    FollowersLayout.Click += LinFollowersOnClick;
                    FollowingLayout.Click += LinFollowingOnClick;
                    TabLayout.TabSelected += TabLayoutOnTabSelected;
                    TabLayout.TabUnselected += TabLayoutOnTabUnselected;
                }
                else
                {
                    IconBack.Click -= IconBackOnClick;
                    BtnMore.Click -= BtnMoreOnClick;
                    BtnMessage.Click -= BtnMessageOnClick;
                    FollowButton.Click -= BtnAddOnClick;
                    FollowersLayout.Click -= LinFollowersOnClick;
                    FollowingLayout.Click -= LinFollowingOnClick;
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
                PostTab = new UserPostFragment();
                AboutTab = new AboutFragment();

                var adapter = new MainTabAdapter(ChildFragmentManager);
                adapter.AddFragment(PostTab, "");
                adapter.AddFragment(AboutTab, "");

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

        //Back
        private void IconBackOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext.FragmentNavigatorBack();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        // More
        private void BtnMoreOnClick(object sender, EventArgs e)
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

        // Show Message 
        private void BtnMessageOnClick(object sender, EventArgs e)
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

        private void BtnAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    if (FollowButton.Tag?.ToString() == "true")
                    {
                        FollowButton.Tag = "false";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Follow);
                        FollowButton.SetBackgroundResource(Resource.Drawable.buttonFlatNormal);
                        FollowButton.SetTextColor(Color.ParseColor("#ffffff"));
                    }
                    else
                    {
                        FollowButton.Tag = "true";
                        FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                        FollowButton.SetBackgroundColor(Color.ParseColor("#efefef"));
                        FollowButton.SetTextColor(Color.ParseColor("#444444"));
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

        // Show Following
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

                bundle.PutString("UserId", UserId);
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

        #region Load Profile Api 

        public void LoadProfile(string json, string type)
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
                            GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                            TxtFullName.Text = Arguments.GetString("fullname");
                            break;
                    } 
                }
                else
                {
                    GlideImageLoader.LoadImage(Activity, Arguments.GetString("avatar"), ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);
                    TxtFullName.Text = Arguments.GetString("fullname");
                }

                //Add Api 
                LoadExploreFeed();
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
                GlideImageLoader.LoadImage(Activity, cl.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                TxtUserName.Text = "@" + cl.Username;
                TxtFullName.Text = Methods.FunString.DecodeString(cl.Name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void LoadUserData(UserDataObject cl, bool friends = true)
        {
            try
            {
                PPrivacy = cl.PPrivacy;

                GlideImageLoader.LoadImage(Activity, cl.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                AboutTab.TextSanitizerAutoLink.Load(AppTools.GetAboutFinal(cl));
                AboutTab.TxtGender.Text = cl.Gender;
                AboutTab.TxtEmail.Text = cl.Email;
                if (string.IsNullOrEmpty(cl.Website))
                {
                    AboutTab.WebsiteLinearLayout.Visibility = ViewStates.Gone;
                }
                else
                {
                    AboutTab.TxtWebsite.Text = cl.Website;
                    AboutTab.WebsiteLinearLayout.Visibility = ViewStates.Visible;
                }
                 
                TxtUserName.Text = "@" + cl.Username;

                var font = Typeface.CreateFromAsset(Application.Context.Resources.Assets, "ionicons.ttf");
                TxtFullName.SetTypeface(font, TypefaceStyle.Normal);

                var textHighLighter = AppTools.GetNameFinal(cl);

                if (cl.Verified == "1")
                    textHighLighter += " " + IonIconsFonts.CheckmarkCircled;

                if (cl.BusinessAccount == "1")
                {
                    textHighLighter += " " + IonIconsFonts.SocialUsd;
                }

                var decorator = TextDecorator.Decorate(TxtFullName, textHighLighter);

                if (cl.Verified == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsVerified, IonIconsFonts.CheckmarkCircled);

                if (cl.BusinessAccount == "1")
                    decorator.SetTextColor(Resource.Color.Post_IsBusiness, IonIconsFonts.SocialUsd);

                decorator.Build();

                TxtPostCount.Text = Methods.FunString.FormatPriceValue(Int32.Parse(cl.PostsCount));

                if (cl.Followers != null && int.TryParse(cl.Followers, out var numberFollowers))
                    TxtFollowersCount.Text = Methods.FunString.FormatPriceValue(numberFollowers);

                if (cl.Following != null && int.TryParse(cl.Following, out var numberFollowing))
                    TxtFollowingCount.Text = Methods.FunString.FormatPriceValue(numberFollowing);

                if (!string.IsNullOrEmpty(cl.Google))
                {
                    AboutTab.Google = cl.Google;
                    AboutTab.SocialGoogle.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.SocialGoogle.Text = IonIconsFonts.SocialGoogle;
                    AboutTab.SocialGoogle.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Facebook))
                {
                    AboutTab.Facebook = cl.Facebook;
                    AboutTab.SocialFacebook.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.SocialFacebook.Text = IonIconsFonts.SocialFacebook;
                    AboutTab.SocialFacebook.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }

                if (!string.IsNullOrEmpty(cl.Website))
                {
                    AboutTab.Website = cl.Website;
                    AboutTab.WebsiteButton.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.WebsiteButton.Text = IonIconsFonts.AndroidGlobe;
                    AboutTab.WebsiteButton.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }


                if (!string.IsNullOrEmpty(cl.Twitter))
                {
                    AboutTab.Twitter = cl.Twitter;
                    AboutTab.SocialTwitter.SetTypeface(font, TypefaceStyle.Normal);
                    AboutTab.SocialTwitter.Text = IonIconsFonts.SocialTwitter;
                    AboutTab.SocialTwitter.Visibility = ViewStates.Visible;
                    AboutTab.SocialLinksLinear.Visibility = ViewStates.Visible;
                }

                BtnMessage.Visibility = cl.IsFollowing != null && (cl.CPrivacy == "1" || cl.CPrivacy == "2" && cl.IsFollowing.Value) ? ViewStates.Visible : ViewStates.Invisible;

                if (cl.IsFollowing != null)
                    SIsFollowing = cl.IsFollowing.Value;

                if (!friends) return;
                if (cl.IsFollowing != null && cl.IsFollowing.Value) // My Friend
                {
                    FollowButton.SetBackgroundColor(Color.ParseColor("#efefef"));
                    FollowButton.SetTextColor(Color.ParseColor("#444444"));
                    FollowButton.Text = Context.GetText(Resource.String.Lbl_Following);
                    FollowButton.Tag = "true";
                }
                else
                {
                    //Not Friend
                    FollowButton.SetBackgroundResource(Resource.Drawable.buttonFlatNormal);
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


        #endregion

        #region Load Post

        public void LoadExploreFeed()
        {
            try
            {
                if (PPrivacy == "2" || PPrivacy == "1" && SIsFollowing)
                {
                    if (Methods.CheckConnectivity())
                    {
                        StartApiService();
                    }
                    else
                    {
                        if (PostTab.Inflated == null)
                            PostTab.Inflated = PostTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.NoConnection);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                            x.EmptyStateButton.Click += TryAgainButton_Click;
                        }
                    }
                }
                else
                {
                    ShowEmptyPage("ProfilePrivate");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StartApiService(string offsetPost = "0")
        {
            if (Methods.CheckConnectivity())
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetPost(offsetPost) });
            else
                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
        }

        public async Task GetPost(string offset = "0")
        {
            if (PostTab.MainScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                PostTab.MainScrollEvent.IsLoading = true;
                int countList = PostTab.MAdapter.PostList.Count;
                (int apiStatus, var respond) = await RequestsAsync.Post.FetchUserPostsById(UserId, "24", offset);
                if (apiStatus.Equals(200))
                {
                    if (respond is FetchUserPostsByUserIdObject result)
                    {
                        var respondList = result.data.UserPosts.Count;
                        if (respondList > 0)
                        {
                            result.data.UserPosts.RemoveAll(a => a.MediaSet?.Count == 0 && a.MediaSet == null);

                            if (countList > 0)
                            {
                                foreach (var item in from item in result.data.UserPosts let check = PostTab.MAdapter.PostList.FirstOrDefault(a => a.PostId == item.PostId) where check == null select item)
                                {
                                    item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);
                                    PostTab.MAdapter.PostList.Add(item);
                                }

                                Activity.RunOnUiThread(() => { PostTab.MAdapter.NotifyItemRangeInserted(countList - 1, PostTab.MAdapter.PostList.Count - countList); });
                            }
                            else
                            {
                                PostTab.MAdapter.PostList = new ObservableCollection<PostsObject>(result.data.UserPosts);
                                Activity.RunOnUiThread(() => { PostTab.MAdapter.NotifyDataSetChanged(); });
                            }
                        }
                        else
                        {
                            if (PostTab.MAdapter.PostList.Count > 10 && !PostTab.MRecycler.CanScrollVertically(1))
                                Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_NoMorePost), ToastLength.Short).Show();
                        }
                    }
                }
                else Methods.DisplayReportResult(Activity, respond);

                Activity.RunOnUiThread(() => { ShowEmptyPage("GetPost"); });
            }
            else
            {
                PostTab.Inflated = PostTab.EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                Toast.MakeText(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                PostTab.MainScrollEvent.IsLoading = false;
            }
            PostTab.MainScrollEvent.IsLoading = false;
        }

        private void ShowEmptyPage(string type)
        {
            try
            {
                if (PostTab.SwipeRefreshLayout != null) PostTab.SwipeRefreshLayout.Refreshing = false;
                PostTab.MainScrollEvent.IsLoading = false;

                if (type == "GetPost")
                { 
                    if (PostTab.MAdapter.PostList.Count > 0)
                    {
                        PostTab.MRecycler.Visibility = ViewStates.Visible;
                        PostTab.EmptyStateLayout.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        PostTab.MRecycler.Visibility = ViewStates.Gone;

                        if (PostTab.Inflated == null)
                            PostTab.Inflated = PostTab.EmptyStateLayout.Inflate();

                        EmptyStateInflater x = new EmptyStateInflater();
                        x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.NoPost);
                        if (!x.EmptyStateButton.HasOnClickListeners)
                        {
                            x.EmptyStateButton.Click += null;
                        }
                        PostTab.EmptyStateLayout.Visibility = ViewStates.Visible;
                    }
                }
                else if (type == "ProfilePrivate")
                {
                    PostTab.MRecycler.Visibility = ViewStates.Gone;

                    if (PostTab.Inflated == null)
                        PostTab.Inflated = PostTab.EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(PostTab.Inflated, EmptyStateInflater.Type.ProfilePrivate);
                    if (!x.EmptyStateButton.HasOnClickListeners)
                    {
                        x.EmptyStateButton.Click += null;
                        x.EmptyStateButton.Click += TryAgainButton_Click;
                    }
                }
            }
            catch (Exception e)
            {
                if (PostTab.SwipeRefreshLayout != null) PostTab.SwipeRefreshLayout.Refreshing = false;
                PostTab.MainScrollEvent.IsLoading = false;
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

        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            StartApiService();
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

                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_Block))
                {
                    if (Methods.CheckConnectivity())
                    {
                        //Sent Api >>
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.BlockUnblock(UserId) });

                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
                else if (itemString.ToString() == Activity.GetText(Resource.String.Lbl_CopyLinkToProfile))
                {
                    ClipboardManager clipboard = (ClipboardManager)Activity.GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("clipboard", Url);
                    clipboard.PrimaryClip = clip;

                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short).Show();
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