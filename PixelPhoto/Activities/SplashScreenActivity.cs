using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;
using Android.Support.V7.App;
using PixelPhoto.Activities.Default;
using PixelPhoto.Activities.Tabbes;
using Android.Widget;
using Java.Lang;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using Exception = System.Exception;


namespace PixelPhoto.Activities
{
    [Activity(MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/SplashScreenTheme", NoHistory = true, ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class SplashScreenActivity : AppCompatActivity
    {
        #region Variables Basic

        private SqLiteDatabase DbDatabase;

        #endregion
         
        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                DbDatabase = new SqLiteDatabase();
                DbDatabase.CheckTablesStatus();

                new Handler(Looper.MainLooper).Post(new Runnable(FirstRunExcite)); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void FirstRunExcite()
        {
            try
            {
                DbDatabase = new SqLiteDatabase();
                DbDatabase.CheckTablesStatus();

                if (!string.IsNullOrEmpty(AppSettings.Lang))
                {
                    LangController.SetApplicationLang(this, AppSettings.Lang);
                }
                else
                {
                    UserDetails.LangName = Resources.Configuration.Locale.Language.ToLower();
                    LangController.SetApplicationLang(this, UserDetails.LangName);
                }

                LoadConfigSettings();
                var result = DbDatabase.Get_data_Login_Credentials();
                if (result != null)
                {
                    Current.AccessToken = result.AccessToken;
                    switch (result.Status)
                    {
                        case "Active":
                        case "Pending":
                            StartActivity(new Intent(Application.Context, typeof(HomeActivity)));
                            break;
                        default:
                            StartActivity(new Intent(Application.Context, typeof(FirstActivity)));
                            break;
                    }
                }
                else
                {
                    StartActivity(new Intent(Application.Context, typeof(FirstActivity)));
                }

                DbDatabase.Dispose();

                if (AppSettings.ShowAdMobBanner || AppSettings.ShowAdMobInterstitial || AppSettings.ShowAdMobRewardVideo || AppSettings.ShowAdMobNative)
                    MobileAds.Initialize(this, GetString(Resource.String.admob_app_id));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                Toast.MakeText(this, exception.Message, ToastLength.Short).Show();
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
    }
}