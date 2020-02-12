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
using Exception = System.Exception;
using ListPreference = Android.Support.V7.Preferences.ListPreference;
using Preference = Android.Support.V7.Preferences.Preference;

namespace PixelPhoto.Activities.SettingsUser.Privacy
{
    public class SettingPrivacyPrefcFragment : PreferenceFragmentCompat, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        #region Variables Basic

        private ListPreference PrivacyCanFollowPref, PrivacyCanMessagePref;
        //private SwitchPreferenceCompat PrivacyShareShowYourProfilePref;
        private string CanFollowPref = "0", CanMessagePref = "0";
        private readonly Activity ActivityContext;

        #endregion

        public SettingPrivacyPrefcFragment(Activity context)
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
                AddPreferencesFromResource(Resource.Xml.SettingsPrefs_Privacy);

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

                PrivacyCanFollowPref = (ListPreference)FindPreference("CanViewProfile_key");
                PrivacyCanMessagePref = (ListPreference)FindPreference("CanDirectMessage_key");
                //PrivacyShareShowYourProfilePref = (SwitchPreferenceCompat)FindPreference("ShowYourProfile_key");
                //PrivacyShareShowYourProfilePref.IconSpaceReserved = false;
                //Update Preferences data on Load
                OnSharedPreferenceChanged(MainSettings.SharedData, "CanViewProfile_key");
                OnSharedPreferenceChanged(MainSettings.SharedData, "CanDirectMessage_key");
                //OnSharedPreferenceChanged(MainSettings.SharedData, "ShowYourProfile_key");
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
                    PrivacyCanFollowPref.PreferenceChange += PrivacyCanFollowPrefOnPreferenceChange;
                    PrivacyCanMessagePref.PreferenceChange += PrivacyCanMessagePrefOnPreferenceChange;
                    //PrivacyShareShowYourProfilePref.PreferenceChange += PrivacyShareShowYourProfilePrefOnPreferenceChange;

                }
                else
                {
                    PrivacyCanFollowPref.PreferenceChange -= PrivacyCanFollowPrefOnPreferenceChange;
                    PrivacyCanMessagePref.PreferenceChange -= PrivacyCanMessagePrefOnPreferenceChange;
                    //PrivacyShareShowYourProfilePref.PreferenceChange -= PrivacyShareShowYourProfilePrefOnPreferenceChange;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events
         
        /*private void PrivacyShareShowYourProfilePrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
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
                        if (dataUser != null) dataUser.SearchEngines = "1";
                        ShowYourProfilePref = "1";
                    }
                    else
                    {
                        if (dataUser != null) dataUser.SearchEngines = "0";
                        ShowYourProfilePref = "0";
                    }

                    if (dataUser != null) dataUser.SearchEngines = ShowYourProfilePref;

                    if (Methods.CheckConnectivity())
                    {
                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"search_engines", ShowYourProfilePref}
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
        }*/

        private void PrivacyCanMessagePrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var etp = (ListPreference)sender;
                    var value = eventArgs.NewValue.ToString();
                    //var valueAsText = etp.GetEntries()[int.Parse(value)];

                    if (value == "1")
                    {
                        CanMessagePref = "1";
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Everyone);
                    }
                    else if (value == "2")
                    {
                        CanMessagePref = "2";
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_People_i_Follow);
                    }
                    
                    if (Methods.CheckConnectivity())
                    {
                        var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                        if (dataUser != null) dataUser.CPrivacy = CanMessagePref;

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                        dbDatabase.Dispose();

                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"c_privacy", CanMessagePref}
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

        private void PrivacyCanFollowPrefOnPreferenceChange(object sender, Preference.PreferenceChangeEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.Handled)
                {
                    var etp = (ListPreference)sender;
                    var value = eventArgs.NewValue.ToString();
                    var valueAsText = etp.GetEntries()[int.Parse(value)];
                    
                    if (value == "0")
                    {
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_No_body);
                        CanFollowPref = "0";
                    }
                    else if (value == "1")
                    {
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Followers);
                        CanFollowPref = "1";
                    }
                    else
                    {
                        etp.Summary = ActivityContext.GetString(Resource.String.Lbl_Everyone);
                        CanFollowPref = "2";
                    }
                     
                    if (Methods.CheckConnectivity())
                    {
                        var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                        if (dataUser != null) dataUser.PPrivacy = CanFollowPref;

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.InsertOrUpdateToMyProfileTable(dataUser);
                        dbDatabase.Dispose();

                        var dataPrivacy = new Dictionary<string, string>
                        {
                            {"p_privacy", CanFollowPref}
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
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                if (key.Equals("CanViewProfile_key"))
                {
                    var valueAsText = PrivacyCanFollowPref.Entry;
                    if (!string.IsNullOrEmpty(valueAsText))
                    {
                        if (dataUser != null)
                        {
                            MainSettings.SharedData.Edit().PutString("CanViewProfile_key", dataUser.PPrivacy).Commit();
                            PrivacyCanFollowPref.SetValueIndex(int.Parse(dataUser.PPrivacy));

                            CanFollowPref = dataUser.PPrivacy;
                            if (CanFollowPref == "0")
                                PrivacyCanFollowPref.Summary = ActivityContext.GetString(Resource.String.Lbl_No_body);
                            else if (CanFollowPref == "1")
                                PrivacyCanFollowPref.Summary = ActivityContext.GetString(Resource.String.Lbl_Followers);
                            else
                                PrivacyCanFollowPref.Summary = ActivityContext.GetString(Resource.String.Lbl_Everyone);
                        }
                    }
                    else
                    {
                        CanFollowPref = PrivacyCanFollowPref.Value;
                        PrivacyCanFollowPref.Summary = CanFollowPref;
                        PrivacyCanFollowPref.SetValueIndex(dataUser != null ? int.Parse(dataUser?.PPrivacy) : 0);
                    }
                }
                else if (key.Equals("CanDirectMessage_key"))
                {
                    var valueAsText = PrivacyCanMessagePref.Entry;
                    if (!string.IsNullOrEmpty(valueAsText))
                    {
                        if (dataUser != null)
                        {
                            MainSettings.SharedData.Edit().PutString("CanDirectMessage_key", dataUser.CPrivacy).Commit();
                             
                            CanMessagePref = dataUser.CPrivacy;
                            if (CanMessagePref == "1")
                            {
                                PrivacyCanMessagePref.SetValueIndex(int.Parse("0"));
                                PrivacyCanMessagePref.Summary = ActivityContext.GetString(Resource.String.Lbl_Everyone);
                            }
                            else if (CanMessagePref == "2")
                            {
                                PrivacyCanMessagePref.SetValueIndex(int.Parse("1"));
                                PrivacyCanMessagePref.Summary = ActivityContext.GetString(Resource.String.Lbl_People_i_Follow);
                            }
                            else
                            {
                                PrivacyCanMessagePref.SetValueIndex(int.Parse(dataUser.CPrivacy));
                            }
                        }
                    }
                    else
                    {
                        CanMessagePref = PrivacyCanMessagePref.Value;
                        PrivacyCanMessagePref.Summary = CanMessagePref;
                        PrivacyCanMessagePref.SetValueIndex(dataUser != null ? int.Parse(dataUser?.CPrivacy) : 0);
                    }
                }
                /*else if (key.Equals("ShowYourProfile_key"))
                {
                    var getValue = MainSettings.SharedData.GetBoolean("ShowYourProfile_key", true);
                    //PrivacyShareShowYourProfilePref.Checked = getValue;
                }*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}