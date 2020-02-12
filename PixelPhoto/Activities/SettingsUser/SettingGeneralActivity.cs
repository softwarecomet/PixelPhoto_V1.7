﻿using System;
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
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.SettingsUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SettingGeneralActivity : AppCompatActivity 
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private TextView SaveTextView;
        private EditText UsernameEditText, EmailEditText;
        private RadioButton RadioMale, RadioFemale;
        private string GenderStatus = "";

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.SettingGeneralLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                LoadMyData();
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
                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
                UsernameEditText = FindViewById<EditText>(Resource.Id.usernameText);
                EmailEditText = FindViewById<EditText>(Resource.Id.emailText);
                RadioMale = FindViewById<RadioButton>(Resource.Id.radioMale);
                RadioFemale = FindViewById<RadioButton>(Resource.Id.radioFemale);
                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                SetColorEditText(UsernameEditText);
                SetColorEditText(EmailEditText);

                AdsGoogle.Ad_AdmobNative(this);
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
                    Toolbar.Title = GetText(Resource.String.Lbl_General);
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
                    SaveTextView.Click += SaveTextViewOnClick;
                    RadioMale.CheckedChange += RadioMaleOnCheckedChange;
                    RadioFemale.CheckedChange += RadioFemaleOnCheckedChange;
                }
                else
                {
                    SaveTextView.Click -= SaveTextViewOnClick;
                    RadioMale.CheckedChange -= RadioMaleOnCheckedChange;
                    RadioFemale.CheckedChange -= RadioFemaleOnCheckedChange;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events


        //Set Value Gender >> Female
        private void RadioFemaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RadioFemale.Checked;
                if (isChecked)
                {
                    RadioMale.Checked = false;
                    GenderStatus = "female";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Set Value Gender >> Male
        private void RadioMaleOnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                var isChecked = RadioMale.Checked;
                if (isChecked)
                {
                    RadioFemale.Checked = false;
                    GenderStatus = "female";
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private async void SaveTextViewOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();

                    if (!string.IsNullOrEmpty(UsernameEditText.Text))
                        dictionary.Add("username", UsernameEditText.Text);

                    if (!string.IsNullOrEmpty(EmailEditText.Text))
                        dictionary.Add("email", EmailEditText.Text);
                     
                    if (!string.IsNullOrEmpty(GenderStatus))
                        dictionary.Add("gender", GenderStatus);
                     
                    if (dictionary.Count > 0)
                    {
                        //Show a progress
                        AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                        //Send Api
                        (int respondCode, var respondString) = await RequestsAsync.User.SaveSettings(dictionary);
                        if (respondCode == 200)
                        {
                            var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                            if (dataUser != null)
                            {
                                dataUser.Username = UsernameEditText.Text;
                                dataUser.Email = EmailEditText.Text;
                                dataUser.Gender = GenderStatus;

                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                                dbDatabase.Dispose();
                            }
                             
                            AndHUD.Shared.ShowSuccess(this, GetText(Resource.String.Lbl_Done), MaskType.Clear, TimeSpan.FromSeconds(2));
                            Finish();
                        }
                        else Methods.DisplayReportResult(this, respondString);

                        AndHUD.Shared.Dismiss(this);
                    }
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short).Show();
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
                    UsernameEditText.Text = myData.Username;
                    EmailEditText.Text = myData.Email;
                   
                    if (myData.Gender == "male" || myData.Gender == "Male")
                    {
                        RadioMale.Checked = true;
                        RadioFemale.Checked = false;
                        GenderStatus = "male";
                    }
                    else
                    {
                        RadioMale.Checked = false;
                        RadioFemale.Checked = true;
                        GenderStatus = "female";
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