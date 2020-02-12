using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Widget;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat.Service;
using PixelPhoto.Activities.Default;
using PixelPhoto.Activities.Funding;
using PixelPhoto.Activities.SettingsUser;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.OneSignal;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Console = System.Console;
using Exception = System.Exception;
using File = Java.IO.File;


namespace PixelPhoto.Helpers.Controller
{
    public static class ApiRequest
    {
        private static readonly string ApiGetSearchGif = "https://api.giphy.com/v1/gifs/search?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g&q=";
        private static readonly string ApiGeTrendingGif = "https://api.giphy.com/v1/gifs/trending?api_key=b9427ca5441b4f599efa901f195c9f58&limit=45&rating=g";

        public static async Task GetSettings_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                (int apiStatus, var respond) = await Current.GetSettings();
                if (apiStatus == 200)
                {
                    if (respond is GetSettingsObject result)
                    {
                        if (result.Data != null)
                        {
                            ListUtils.SettingsSiteList = result.Data;

                            AppSettings.OneSignalAppId = result.Data.PushId;
                            OneSignalNotification.RegisterNotificationDevice();

                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrReplaceSettingsAsync(result.Data);
                            dbDatabase.Dispose();
                        }
                    }
                }
                else Methods.DisplayReportResult(context, respond);
            }
        }

        public static async Task<(int, int)> GetCountNotifications()
        {
            var (respondCode, respondString) = await RequestsAsync.User.FetchNotifications("0", "15").ConfigureAwait(false);
            if (respondCode.Equals(200))
            {
                if (respondString is FetchNotificationsObject fetch)
                {
                    return (fetch.NewNotifications , fetch.NewMessages);
                }
            }
            return (0, 0);
        }
         
        public static async Task GetProfile_Api(Activity context)
        {
            if (Methods.CheckConnectivity())
            {
                (int apiStatus, var respond) = await RequestsAsync.User.FetchUserData(UserDetails.UserId);
                if (apiStatus == 200)
                {
                    if (respond is FetchUserDataObject result)
                    {
                        if (result.Data != null)
                        {
                            SqLiteDatabase dbDatabase = new SqLiteDatabase();
                            dbDatabase.InsertOrUpdateToMyProfileTable(result.Data);
                            dbDatabase.Dispose();

                            var dataStory = HomeActivity.GetInstance().NewsFeedFragment.StoryAdapter?.StoryList?.FirstOrDefault(a => a.Type == "Your");
                            if (dataStory != null)
                            {
                                dataStory.Avatar = result.Data.Avatar;
                                HomeActivity.GetInstance().NewsFeedFragment.StoryAdapter.NotifyItemChanged(0);
                            }
                        }
                    }
                }
                else Methods.DisplayReportResult(context, respond);
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> SearchGif(string searchKey, string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return null;
                }
                else
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGetSearchGif + searchKey + "&offset=" + offset);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (data.meta.Status == 200)
                        {
                            return new ObservableCollection<GifGiphyClass.Datum>(data.Data);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public static async Task<ObservableCollection<GifGiphyClass.Datum>> TrendingGif(string offset)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    Toast.MakeText(Application.Context, Application.Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    return null;
                }
                else
                {
                    var client = new HttpClient();
                    var response = await client.GetAsync(ApiGeTrendingGif + "&offset=" + offset);
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<GifGiphyClass>(json);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        if (data.meta.Status == 200)
                        {
                            return new ObservableCollection<GifGiphyClass.Datum>(data.Data);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }


        /////////////////////////////////////////////////////////////////
        private static bool RunLogout;

        public static async void Delete(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Delete");

                    context.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_MyFolderDisk();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();

                        Runtime.GetRuntime().RunFinalization();
                        Runtime.GetRuntime().Gc();
                        TrimCache(context);

                        dbDatabase.ClearAll();
                        dbDatabase.DropAll();

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.CheckTablesStatus();
                        dbDatabase.Dispose();
                         
                        var intentService = new Intent(context, typeof(ScheduledApiService));
                        context.StopService(intentService);

                        MainSettings.SharedData.Edit().Clear().Commit();

                        Intent intent = new Intent(context, typeof(FirstActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish();
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async void Logout(Activity context)
        {
            try
            {
                if (RunLogout == false)
                {
                    RunLogout = true;

                    await RemoveData("Logout");

                    context.RunOnUiThread(() =>
                    {
                        Methods.Path.DeleteAll_MyFolderDisk();

                        SqLiteDatabase dbDatabase = new SqLiteDatabase();

                        Runtime.GetRuntime().RunFinalization();
                        Runtime.GetRuntime().Gc();
                        TrimCache(context);

                        dbDatabase.ClearAll();
                        dbDatabase.DropAll();

                        ListUtils.ClearAllList();

                        UserDetails.ClearAllValueUserDetails();

                        dbDatabase.CheckTablesStatus();
                        dbDatabase.Dispose();

                        var intentService = new Intent(context, typeof(ScheduledApiService));
                        context.StopService(intentService);

                        HomeActivity.GetInstance()?.NewsFeedFragment?.MainHandler?.RemoveCallbacks(HomeActivity.GetInstance().NewsFeedFragment.Runnable);
                        HomeActivity.GetInstance().NewsFeedFragment.MainHandler = null;
                         
                        MainSettings.SharedData.Edit().Clear().Commit();

                        Intent intent = new Intent(context, typeof(FirstActivity));
                        intent.AddCategory(Intent.CategoryHome);
                        intent.SetAction(Intent.ActionMain);
                        intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask | ActivityFlags.ClearTask);
                        context.StartActivity(intent);
                        context.FinishAffinity();
                        context.Finish();
                    });

                    RunLogout = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void TrimCache(Activity context)
        {
            try
            {
                File dir = context.CacheDir;
                if (dir != null && dir.IsDirectory)
                {
                    DeleteDir(dir);
                }

                context.DeleteDatabase("PixelPhotoSocial.db");
                context.DeleteDatabase(SqLiteDatabase.PathCombine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static bool DeleteDir(File dir)
        {
            try
            {
                if (dir == null || !dir.IsDirectory) return dir != null && dir.Delete();
                string[] children = dir.List();
                if (children.Select(child => DeleteDir(new File(dir, child))).Any(success => !success))
                {
                    return false;
                }

                // The directory is now empty so delete it
                return dir.Delete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        private static async Task RemoveData(string type)
        {
            try
            {
                if (type == "Logout")
                {
                    if (Methods.CheckConnectivity())
                    {
                        await RequestsAsync.Auth.Logout();
                    }
                }
                else if (type == "Delete")
                {
                    Methods.Path.DeleteAll_MyFolder();

                    if (Methods.CheckConnectivity())
                    {
                        await RequestsAsync.Auth.DeleteAccount(UserDetails.Password);
                    }
                }

                try
                {
                    if (AppSettings.ShowGoogleLogin && LoginActivity.MGoogleApiClient != null)
                        if (Auth.GoogleSignInApi != null)
                        {
                            Auth.GoogleSignInApi.SignOut(LoginActivity.MGoogleApiClient);
                            LoginActivity.MGoogleApiClient = null;
                        }
                     
                    if (AppSettings.ShowFacebookLogin)
                    {
                        var accessToken = AccessToken.CurrentAccessToken;
                        var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                        if (isLoggedIn && Profile.CurrentProfile != null)
                        {
                            LoginManager.Instance.LogOut();
                        }
                    }

                    OneSignalNotification.UnRegisterNotificationDevice();

                    UserDetails.ClearAllValueUserDetails();

                    if (FundingActivity.MAdapter != null)
                    {
                        FundingActivity.MAdapter.FundingList = new ObservableCollection<FundingDataObject>();
                    }
                     
                    GC.Collect();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }

            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
} 