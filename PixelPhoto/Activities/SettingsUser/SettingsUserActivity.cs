using System;
using System.Collections.Generic;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.Lang;
using PixelPhoto.Activities.BlockedUsers;
using PixelPhoto.Activities.MyProfile;
using PixelPhoto.Activities.SettingsUser.Adapters;
using PixelPhoto.Activities.SettingsUser.Notification;
using PixelPhoto.Activities.SettingsUser.Privacy;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.SettingsUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SettingsUserActivity : AppCompatActivity, MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        #region Variables Basic

        private MoreSectionAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private AdView MAdView;
        private string TypeDialog;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                MAdView?.Resume();
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                MAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                MAdView?.Destroy();

                base.OnDestroy();
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
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            { 
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
                EmptyStateLayout.Visibility = ViewStates.Gone;

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler); 
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
                MAdapter = new MoreSectionAdapter(this);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_Settings);
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
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
 
        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                // true +=  // false -=
                if (addEvent)
                {
                    MAdapter.ItemClick += MAdapterOnItemClick; 
                }
                else
                {
                    MAdapter.ItemClick -= MAdapterOnItemClick; 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, MoreSectionAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = MAdapter.GetItem(position);
                    if (item != null)
                    {
                        if (item.Id == 1) // General
                        {
                            StartActivity(new Intent(this, typeof(SettingGeneralActivity)));
                        }
                        else if (item.Id == 2) //Profile
                        {
                            StartActivity(new Intent(this, typeof(EditProfileActivity)));
                        }
                        else if (item.Id == 3) //Change my password
                        {
                            StartActivity(new Intent(this, typeof(SettingPasswordActivity)));
                        }
                        else if (item.Id == 4)//Account privacy
                        {
                            StartActivity(new Intent(this, typeof(SettingPrivacyActivity)));
                        }
                        else if (item.Id == 5)//Notification
                        {
                            StartActivity(new Intent(this, typeof(SettingNotificationActivity)));
                        }
                        else if (item.Id == 6)//Blocked users
                        {
                            StartActivity(new Intent(this, typeof(BlockedUsersActivity)));
                        }
                        else if (item.Id == 7)//Delete account
                        {
                            StartActivity(new Intent(this, typeof(SettingDeleteAccountActivity)));
                        }
                        else if (item.Id == 8) // Logout
                        {
                            TypeDialog = "Logout";

                            var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);
                            dialog.Title(Resource.String.Lbl_Warning);
                            dialog.Content(GetText(Resource.String.Lbl_Are_you_logout));
                            dialog.PositiveText(GetText(Resource.String.Lbl_Ok)).OnPositive(this);
                            dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                            dialog.AlwaysCallSingleChoiceCallback();
                            dialog.Build().Show();
                        }
                        else if (item.Id == 9) //  NightMode
                        {
                            TypeDialog = "NightMode";

                            var arrayAdapter = new List<string>();
                            var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                            dialogList.Title(Resource.String.Lbl_Night_Mode);

                            arrayAdapter.Add(GetText(Resource.String.Lbl_Light));
                            arrayAdapter.Add(GetText(Resource.String.Lbl_Dark));

                            if ((int)Build.VERSION.SdkInt >= 29)
                                arrayAdapter.Add(GetText(Resource.String.Lbl_SetByBattery));

                            dialogList.Items(arrayAdapter);
                            dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                            dialogList.AlwaysCallSingleChoiceCallback();
                            dialogList.ItemsCallback(this).Build().Show(); 
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

        #region MaterialDialog
         
        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
             
                if (text == GetString(Resource.String.Lbl_Light))
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
                    AppSettings.SetTabDarkTheme = false;
                    MainSettings.SharedNightMode.Edit().PutString("Night_Mode_key", MainSettings.LightMode).Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(this, typeof(SplashScreenActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(intent);
                    FinishAffinity();
                }
                else if (text == GetString(Resource.String.Lbl_Dark))
                {
                    AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightYes;
                    AppSettings.SetTabDarkTheme = true;
                    MainSettings.SharedNightMode.Edit().PutString("Night_Mode_key", MainSettings.DarkMode).Commit();

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                    }

                    Intent intent = new Intent(this, typeof(SplashScreenActivity));
                    intent.AddCategory(Intent.CategoryHome);
                    intent.SetAction(Intent.ActionMain);
                    intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(intent);
                    FinishAffinity();
                }
                else if (text == GetString(Resource.String.Lbl_SetByBattery))
                {
                    MainSettings.SharedNightMode.Edit().PutString("Night_Mode_key", MainSettings.DefaultMode).Commit();

                    if ((int)Build.VERSION.SdkInt >= 29)
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightFollowSystem;

                        var currentNightMode = Resources.Configuration.UiMode & UiMode.NightMask;
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
                    }
                    else
                    {
                        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightAuto;

                        var currentNightMode = Resources.Configuration.UiMode & UiMode.NightMask;
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

                        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                        {
                            Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        }

                        Intent intent = new Intent(this, typeof(SplashScreenActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        StartActivity(intent);
                        FinishAffinity();
                    }
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
                if (TypeDialog == "Logout")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        // Check if we're running on Android 5.0 or higher
                        if ((int) Build.VERSION.SdkInt < 23)
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long)
                                .Show();
                            ApiRequest.Logout(this);
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                                CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long)
                                    .Show();
                                ApiRequest.Logout(this);
                            }
                            else
                            {
                                new PermissionsController(this).RequestPermission(100);
                            }
                        }
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
        #region Permissions && Result

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
                        Toast.MakeText(this, GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long).Show();
                        ApiRequest.Logout(this);
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

    }
}