using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Com.OneSignal.Abstractions;
using Com.OneSignal.Android;
using Newtonsoft.Json;
using Org.Json;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Model;
using PixelPhotoClient.GlobalClass;
using OSNotification = Com.OneSignal.Abstractions.OSNotification;
using OSNotificationPayload = Com.OneSignal.Abstractions.OSNotificationPayload;

namespace PixelPhoto.OneSignal
{
    public class OneSignalNotification
    { 
        public static string Userid;
        public static OneSignalObject.NotificationInfoObject NotificationInfo;
        public static UserDataObject UserData;
        public static OneSignalObject.PostDataObject PostData;

        public static void RegisterNotificationDevice()
        {
            try
            {
                if (UserDetails.NotificationPopup)
                {
                    if (AppSettings.OneSignalAppId != "")
                    {
                        Com.OneSignal.OneSignal.Current.StartInit(AppSettings.OneSignalAppId)
                            .InFocusDisplaying(OSInFocusDisplayOption.Notification)
                            .HandleNotificationReceived(HandleNotificationReceived)
                            .HandleNotificationOpened(HandleNotificationOpened)
                            .EndInit();
                        Com.OneSignal.OneSignal.Current.IdsAvailable(IdsAvailable);
                        Com.OneSignal.OneSignal.Current.RegisterForPushNotifications();

                        AppSettings.ShowNotification = true;
                    }
                }
                else
                {
                    UnRegisterNotificationDevice();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void UnRegisterNotificationDevice()
        {
            try
            {
                Com.OneSignal.OneSignal.Current.SetSubscription(false);
                AppSettings.ShowNotification = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void IdsAvailable(string userId, string pushToken)
        {
            try
            {
                UserDetails.DeviceId = userId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void HandleNotificationReceived(OSNotification notification)
        {
            try
            {
                OSNotificationPayload payload = notification.payload;
                Dictionary<string, object> additionalData = payload.additionalData;
                Console.WriteLine(additionalData);
                //string message = payload.body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            try
            {
                OSNotificationPayload payload = result.notification.payload;
                Dictionary<string, object> additionalData = payload.additionalData;
                string message = payload.body;
                Console.WriteLine(message);
                string actionId = result.action.actionID;

                if (additionalData != null)
                {
                    foreach (var item in additionalData)
                    {
                        if (item.Key == "user_id")
                        {
                            Userid = item.Value.ToString();
                        }

                        if (item.Key == "post_data")
                        {
                            PostData = JsonConvert.DeserializeObject<OneSignalObject.PostDataObject>(item.Value.ToString());
                        }

                        if (item.Key == "notification_info")
                        {
                            NotificationInfo = JsonConvert.DeserializeObject<OneSignalObject.NotificationInfoObject>(item.Value.ToString());
                        }

                        if (item.Key == "user_data")
                        {
                            UserData = JsonConvert.DeserializeObject<UserDataObject>(item.Value.ToString());
                        }

                        if (item.Key == "url")
                        {
                            string url = item.Value.ToString();
                        }
                    }

                    //to : do
                    //go to activity or fragment depending on data
                    Intent intent = new Intent(Application.Context, typeof(HomeActivity));
                    intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    intent.AddFlags(ActivityFlags.SingleTop);
                    intent.SetAction(Intent.ActionView);
                    intent.PutExtra("TypeNotification", NotificationInfo.TypeText);
                    Application.Context.StartActivity(intent);

                    if (additionalData.ContainsKey("discount"))
                    {
                        // Take user to your store..
                    }
                }

                if (actionId != null)
                {
                    // actionSelected equals the id on the button the user pressed.
                    // actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present. 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public class NotificationExtenderServiceHandeler : NotificationExtenderService, NotificationCompat.IExtender
        {
            protected override void OnHandleIntent(Intent intent)
            {

            }

            protected override bool OnNotificationProcessing(OSNotificationReceivedResult p0)
            {
                OverrideSettings overrideSettings = new OverrideSettings();
                overrideSettings.Extender = new NotificationCompat.CarExtender();

                Com.OneSignal.Android.OSNotificationPayload payload = p0.Payload;
                JSONObject additionalData = payload.AdditionalData;

                if (additionalData.Has("room_name"))
                {
                    string roomName = additionalData.Get("room_name").ToString();
                    string callType = additionalData.Get("call_type").ToString();
                    string callId = additionalData.Get("call_id").ToString();
                    string fromId = additionalData.Get("from_id").ToString();
                    string toId = additionalData.Get("to_id").ToString();

                    return false;
                }
                else
                {
                    return true;
                }
            }

            public NotificationCompat.Builder Extend(NotificationCompat.Builder builder)
            {
                return builder;
            }
        }
    }
}