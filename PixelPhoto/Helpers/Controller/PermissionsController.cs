using Android;
using Android.App;
using Android.OS;
using System;

namespace PixelPhoto.Helpers.Controller
{
    public class PermissionsController
    {
        private readonly Activity Context;

        public PermissionsController(Activity activity)
        {
            try
            {
                Context = activity;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Handle Permission Request
        /// </summary>
        /// <param name="idPermissions">
        /// 100 >> Storage
        /// 101 >> ReadContacts && ReadPhoneNumbers
        /// 102 >> RecordAudio
        /// 103 >> Camera
        /// 104 >> SendSms
        /// 105 >> Location
        /// 106 >> GetAccounts && UseCredentials >> Social Logins
        /// 107 >> AccessWifiState && Internet
        /// </param>
        public void RequestPermission(int idPermissions)
        {
            // Check if we're running on Android 5.0 or higher
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                switch (idPermissions)
                {
                    case 100:
                        Context.RequestPermissions(new string[]
                        {
                            Manifest.Permission.ReadExternalStorage,
                            Manifest.Permission.AccessMediaLocation,
                            Manifest.Permission.WriteExternalStorage,
                        }, 100);
                        break;

                    case 101:
                        Context.RequestPermissions(new string[]
                        {
                            Manifest.Permission.ReadContacts,
                            Manifest.Permission.ReadPhoneNumbers,
                        }, 101);
                        break;

                    case 102:
                        Context.RequestPermissions(new string[]
                        {
                            Manifest.Permission.RecordAudio,
                            Manifest.Permission.ModifyAudioSettings,
                        }, 102);
                        break;

                    case 103:
                        Context.RequestPermissions(new string[]
                        {
                            Manifest.Permission.Camera,
                        }, 103);
                        break;

                    case 104:
                        Context.RequestPermissions(new string[]
                        {
                            Manifest.Permission.SendSms,
                            Manifest.Permission.BroadcastSms,
                        }, 104);
                        break;

                    case 105:
                        Context.RequestPermissions(new string[]
                        {
                            Manifest.Permission.AccessFineLocation,
                            Manifest.Permission.AccessCoarseLocation
                        }, 105);
                        break;

                    case 106:
                        Context.RequestPermissions(new[]
                        {
                            Manifest.Permission.GetAccounts,
                            Manifest.Permission.UseCredentials
                        }, 106);
                        break;

                    case 107:
                        Context.RequestPermissions(new[]
                        {
                            Manifest.Permission.AccessWifiState,
                            Manifest.Permission.Internet,
                        }, 107);
                        break;
                    case 108:
                        Context.RequestPermissions(new[]
                        {
                            Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                        }, 108);
                        break;
                }
            }
            else
            {
                return;
            }
        }
    }
}