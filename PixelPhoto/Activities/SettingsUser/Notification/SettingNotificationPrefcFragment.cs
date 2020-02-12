using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Preferences;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.RestCalls;
using Preference = Android.Support.V7.Preferences.Preference;

namespace PixelPhoto.Activities.SettingsUser.Notification
{
    public class SettingNotificationPrefcFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region Variables Basic

        private SwitchPreferenceCompat NotificationLikedMyPostPref;
        private SwitchPreferenceCompat NotificationCommentedMyPostPref;
        private SwitchPreferenceCompat NotificationFollowedMePref;
        private SwitchPreferenceCompat NotificationMentionedMePref;
        private string LikedMyPostPref = "0";
        private string CommentedMyPostPref = "0";
        private string FollowedMePref = "0";
        private string MentionedMePref = "0";
        private readonly Activity ActivityContext;

        #endregion

        public SettingNotificationPrefcFragment(Activity context)
        {
            try
            {
                ActivityContext = context;
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
                // create ContextThemeWrapper from the original Activity Context with the custom theme
                Context contextThemeWrapper = AppSettings.SetTabDarkTheme ? new ContextThemeWrapper(Activity, Resource.Style.SettingsThemeDark) : new ContextThemeWrapper(Activity, Resource.Style.SettingsTheme);

                // clone the inflater using the ContextThemeWrapper
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);

                View view = base.OnCreateView(localInflater, container, savedInstanceState);

                return view;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            { 
                // Create your fragment here
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_Notification);

                MainSettings.SharedData = PreferenceManager.SharedPreferences;

                InitComponent(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                //Add OnChange event to Preferences
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                PreferenceScreen.SharedPreferences.UnregisterOnSharedPreferenceChangeListener(this);

                //Close OnChange event to Preferences
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Functions

        private void InitComponent()
        {
            try
            {
                MainSettings.SharedData = PreferenceManager.SharedPreferences;
                PreferenceManager.SharedPreferences.RegisterOnSharedPreferenceChangeListener(this);

                NotificationLikedMyPostPref = (SwitchPreferenceCompat)FindPreference("LikedMyPost_key");
                NotificationCommentedMyPostPref = (SwitchPreferenceCompat)FindPreference("CommentedMyPost_key");
                NotificationFollowedMePref = (SwitchPreferenceCompat)FindPreference("FollowedMe_key");
                NotificationMentionedMePref = (SwitchPreferenceCompat)FindPreference("MentionedMe_key");

                NotificationLikedMyPostPref.IconSpaceReserved = false;
                NotificationCommentedMyPostPref.IconSpaceReserved = false;
                NotificationFollowedMePref.IconSpaceReserved = false;
                NotificationMentionedMePref.IconSpaceReserved = false;
                
                //Update Preferences data on Load
                OnSharedPreferenceChanged(MainSettings.SharedData, "LikedMyPost_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "CommentedMyPost_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "FollowedMe_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "MentionedMe_key");
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
                    NotificationLikedMyPostPref.PreferenceChange += NotificationLikedMyPostPrefOnPreferenceChange;
                    NotificationCommentedMyPostPref.PreferenceChange += NotificationCommentedMyPostPrefOnPreferenceChange;
                    NotificationFollowedMePref.PreferenceChange += NotificationFollowedMePrefOnPreferenceChange;
                    NotificationMentionedMePref.PreferenceChange += NotificationMentionedMePrefOnPreferenceChange;
                }
                else
                {
                    NotificationLikedMyPostPref.PreferenceChange -= NotificationLikedMyPostPrefOnPreferenceChange;
                    NotificationCommentedMyPostPref.PreferenceChange -= NotificationCommentedMyPostPrefOnPreferenceChange;
                    NotificationFollowedMePref.PreferenceChange -= NotificationFollowedMePrefOnPreferenceChange;
                    NotificationMentionedMePref.PreferenceChange -= NotificationMentionedMePrefOnPreferenceChange;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events
         
        private void NotificationMentionedMePrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        if (dataUser != null) dataUser.NOnMention = "1";
                        MentionedMePref = "1";
                    }
                    else
                    {
                        if (dataUser != null) dataUser.NOnMention = "0";
                        MentionedMePref = "0";
                    }

                    if (dataUser != null) dataUser.NOnMention = MentionedMePref;

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                    dbDatabase.Dispose();

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"n_on_mention", MentionedMePref}
                        };
                        RequestsAsync.User.SaveSettings(dataPrivacy).ConfigureAwait(false);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            } 
        }

        private void NotificationFollowedMePrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        if (dataUser != null) dataUser.NOnFollow = "1";
                        FollowedMePref = "1";
                    }
                    else
                    {
                        if (dataUser != null) dataUser.NOnFollow = "0";
                        FollowedMePref = "0";
                    }

                    if (dataUser != null) dataUser.NOnFollow = FollowedMePref;

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                    dbDatabase.Dispose();

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"n_on_follow", FollowedMePref}
                        };
                        RequestsAsync.User.SaveSettings(dataPrivacy).ConfigureAwait(false);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void NotificationCommentedMyPostPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs) 
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        if (dataUser != null) dataUser.NOnComment = "1";
                        CommentedMyPostPref = "1";
                    }
                    else
                    {
                        if (dataUser != null) dataUser.NOnComment = "0";
                        CommentedMyPostPref = "0";
                    }

                    if (dataUser != null) dataUser.NOnComment = CommentedMyPostPref;

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                    dbDatabase.Dispose();

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"n_on_comment", CommentedMyPostPref}
                        };
                        RequestsAsync.User.SaveSettings(dataPrivacy).ConfigureAwait(false);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            } 
        }
         
        private void NotificationLikedMyPostPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                    var etp = (SwitchPreferenceCompat)sender;
                    var value = eventArgs.NewValue.ToString();
                    etp.Checked = bool.Parse(value);
                    if (etp.Checked)
                    {
                        if (dataUser != null) dataUser.NOnLike = "1";
                        LikedMyPostPref = "1";
                    }
                    else
                    {
                        if (dataUser != null) dataUser.NOnLike = "0";
                        LikedMyPostPref = "0";
                    }

                    if (dataUser != null) dataUser.NOnLike = LikedMyPostPref;

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                    dbDatabase.Dispose();

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"n_on_like", LikedMyPostPref}
                        };
                        RequestsAsync.User.SaveSettings(dataPrivacy).ConfigureAwait(false);
                    }
                    else
                    {
                        Toast.MakeText(ActivityContext, ActivityContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            } 
        }

        #endregion

        //On Change 
        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            try
            {
                if (key.Equals("LikedMyPost_key"))
                {
                    var getValue = MainSettings.SharedData.GetBoolean("LikedMyPost_key", true);
                    NotificationLikedMyPostPref.Checked = getValue;
                }
                else if (key.Equals("CommentedMyPost_key"))
                {
                    var getValue = MainSettings.SharedData.GetBoolean("CommentedMyPost_key", true);
                    NotificationCommentedMyPostPref.Checked = getValue;
                }
                else if (key.Equals("FollowedMe_key"))
                {
                    var getValue = MainSettings.SharedData.GetBoolean("FollowedMe_key", true);
                    NotificationFollowedMePref.Checked = getValue;
                }
                else if (key.Equals("MentionedMe_key"))
                {
                    var getValue = MainSettings.SharedData.GetBoolean("MentionedMe_key", true);
                    NotificationMentionedMePref.Checked = getValue;
                }  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}