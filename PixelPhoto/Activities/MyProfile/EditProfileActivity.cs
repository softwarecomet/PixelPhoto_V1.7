using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.RestCalls;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.MyProfile
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class EditProfileActivity : AppCompatActivity
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private TextView SaveTextView;
        private EditText FirstNameEditText, LastNameEditText, AboutEditText, FacebookEditText, GoogleEditText, TwitterEditText;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.EditProfileLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                LoadMyData();

                AdsGoogle.Ad_Interstitial(this);

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
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
         
        #region Functions

        private void InitComponent()
        {
            try
            {
                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
                FirstNameEditText = FindViewById<EditText>(Resource.Id.firstNameText);
                LastNameEditText = FindViewById<EditText>(Resource.Id.lasttNameText);
                AboutEditText = FindViewById<EditText>(Resource.Id.aboutText);
                FacebookEditText = FindViewById<EditText>(Resource.Id.facebookText);
                GoogleEditText = FindViewById<EditText>(Resource.Id.googleText);
                TwitterEditText = FindViewById<EditText>(Resource.Id.twitterText);
                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);


                SetColorEditText(FirstNameEditText);
                SetColorEditText(LastNameEditText);
                SetColorEditText(AboutEditText);
                SetColorEditText(FacebookEditText);
                SetColorEditText(GoogleEditText);
                SetColorEditText(TwitterEditText); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetColorEditText(EditText edit)
        {
            try
            {
                edit.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.EditTextStyleOne_Dark : Resource.Drawable.pixEditTextStyleOne);
                edit.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                edit.SetHintTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = GetString(Resource.String.Lbl_EditProfile);
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    Toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    SetSupportActionBar(Toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
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
                    SaveTextView.Click += SaveDataUserOnClick;
                }
                else
                {
                    SaveTextView.Click -= SaveDataUserOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events
         
        //Event Save data 
        private async void SaveDataUserOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();

                    if (!string.IsNullOrEmpty(FirstNameEditText.Text))
                        dictionary.Add("fname", FirstNameEditText.Text);

                    if (!string.IsNullOrEmpty(LastNameEditText.Text))
                        dictionary.Add("lname", LastNameEditText.Text);

                    if (!string.IsNullOrEmpty(AboutEditText.Text))
                        dictionary.Add("about", AboutEditText.Text);

                    if (!string.IsNullOrEmpty(FacebookEditText.Text) && !FacebookEditText.Text.Contains("https://www.facebook.com/"))
                        dictionary.Add("facebook", "https://www.facebook.com/" + FacebookEditText.Text);
                    else if (!string.IsNullOrEmpty(FacebookEditText.Text))
                        dictionary.Add("facebook", FacebookEditText.Text);

                    if (!string.IsNullOrEmpty(GoogleEditText.Text) && !GoogleEditText.Text.Contains("https://plus.google.com/u/0/"))
                        dictionary.Add("google", "https://plus.google.com/u/0/" + GoogleEditText.Text);
                    else if (!string.IsNullOrEmpty(GoogleEditText.Text))
                        dictionary.Add("google", GoogleEditText.Text);
                     
                    if (!string.IsNullOrEmpty(TwitterEditText.Text) && !TwitterEditText.Text.Contains("https://twitter.com/"))
                        dictionary.Add("twitter", "https://twitter.com/" + TwitterEditText.Text);
                    else if (!string.IsNullOrEmpty(TwitterEditText.Text))
                        dictionary.Add("twitter", TwitterEditText.Text);
                     
                    if (dictionary.Count > 0)
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        //Send Api
                        (int respondCode, var respond) = await RequestsAsync.User.SaveSettings(dictionary);
                        if (respondCode == 200)
                        { 
                            var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                            if (dataUser != null)
                            {
                                dataUser.Fname = FirstNameEditText.Text;
                                dataUser.Lname = LastNameEditText.Text;
                                dataUser.About = AboutEditText.Text;
                                dataUser.Facebook = FacebookEditText.Text;
                                dataUser.Google = GoogleEditText.Text;
                                dataUser.Twitter = TwitterEditText.Text;
                                dataUser.Name = FirstNameEditText.Text + " " + LastNameEditText.Text;

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                                dbDatabase.Dispose();
                            }
                             
                            AndHUD.Shared.ShowSuccess(this, GetText(Resource.String.Lbl_Done), MaskType.Clear, TimeSpan.FromSeconds(2));
                            Intent resultIntent = new Intent();
                            SetResult(Result.Ok, resultIntent);
                            Finish();
                        }
                        else Methods.DisplayReportResult(this, respond);

                        AndHUD.Shared.Dismiss(this);
                    } 
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }
         
        #endregion

        #region LoadMyData

        public async void LoadMyData()
        {
            try
            {
                var dataUser = new SqLiteDatabase().GetMyProfile();

                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(this);

                var myData = ListUtils.MyProfileList.FirstOrDefault();
                if (myData != null)
                {
                    FirstNameEditText.Text = myData.Fname;
                    LastNameEditText.Text = myData.Lname;
                    AboutEditText.Text = Methods.FunString.DecodeString(myData.About);
                    FacebookEditText.Text = myData.Facebook;
                    GoogleEditText.Text = myData.Google;
                    TwitterEditText.Text = myData.Twitter;
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
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion
         
    }
}