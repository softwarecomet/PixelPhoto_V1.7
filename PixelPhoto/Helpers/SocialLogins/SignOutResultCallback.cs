﻿using Android.Gms.Common.Apis;
using Java.Lang;
using PixelPhoto.Activities.Default;

namespace PixelPhoto.Helpers.SocialLogins
{
    public class SignOutResultCallback : Object, IResultCallback
    {
        public LoginActivity Activity { get; set; }

        public void OnResult(Object result)
        {
            //Activity.UpdateUI(false);
        }
    }
}