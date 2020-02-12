using System;
using Android.App;

namespace PixelPhoto.Helpers.Model
{
    public static class UserDetails
    {
        public static string AccessToken = "";
        public static string UserId = "";
        public static string Username = "";
        public static string FullName = "";
        public static string Password = "";
        public static string Email = "";
        public static string Cookie = "";
        public static string Status = "";
        public static string Avatar = "";
        public static string Cover = "";
        public static string DeviceId = "";
        public static string Lang = "";
        public static string Lat = "";
        public static string Lng = "";
        public static string LangName = "";
        public static bool NotificationPopup { get; set; } = true;
         
        public static int UnixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        public static string Time = UnixTimestamp.ToString();

        public static string AndroidId = Android.Provider.Settings.Secure.GetString(Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);
         
        public static void ClearAllValueUserDetails()
        {
            try
            {
                AccessToken = string.Empty;
                UserId = string.Empty;
                Username = string.Empty;
                FullName = string.Empty;
                Password = string.Empty;
                Email = string.Empty;
                Cookie = string.Empty;
                Status = string.Empty;
                Avatar = string.Empty;
                Cover = string.Empty;
                DeviceId = string.Empty;
                Lang = string.Empty;
                Lat = string.Empty;
                Lng = string.Empty;
                LangName = string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
}