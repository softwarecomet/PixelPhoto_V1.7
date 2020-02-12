﻿using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.Ads;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.SettingsUser
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SettingDeleteAccountActivity : AppCompatActivity
    {
        private Toolbar Toolbar;
        private EditText PasswordEditText;
        private CheckBox DeleteCheckBox;
        private Button DeleteButton;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.SettingDeleteAccountLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar(); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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
                PasswordEditText = FindViewById<EditText>(Resource.Id.passwordEdit);
                DeleteCheckBox = FindViewById<CheckBox>(Resource.Id.DeleteCheckBox);
                DeleteButton = FindViewById<Button>(Resource.Id.DeleteButton);

                DeleteCheckBox.Text = GetText(Resource.String.Lbl_IWantToDelete1) + " " + UserDetails.Username + " " +
                                      GetText(Resource.String.Lbl_IWantToDelete2) + " " + AppSettings.ApplicationName +
                                      " " + GetText(Resource.String.Lbl_IWantToDelete3);

                SetColorEditText(PasswordEditText);

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
                    Toolbar.Title = GetString(Resource.String.Lbl_DeleteAccount);
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
                    DeleteButton.Click += DeleteButtonOnClick;
                }
                else
                {
                    DeleteButton.Click -= DeleteButtonOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void DeleteButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DeleteCheckBox.Checked)
                {
                    if (!Methods.CheckConnectivity())
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection),ToastLength.Short).Show();
                    }
                    else
                    {
                        var localData = ListUtils.DataUserLoginList.FirstOrDefault(a => a.UserId == UserDetails.UserId);
                        if (localData != null)
                        {
                            if (PasswordEditText.Text == localData.Password)
                            {
                                ApiRequest.Delete(this);
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Your_account_was_successfully_deleted),ToastLength.Long).Show();
                            }
                            else
                            {
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning),GetText(Resource.String.Lbl_Please_confirm_your_password),GetText(Resource.String.Lbl_Ok));
                            }
                        }
                    }
                }
                else
                {
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Warning),
                        GetText(Resource.String.Lbl_Error_Terms),GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

    }
}