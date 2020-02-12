﻿using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Activities;
using PixelPhoto.Activities.Posts;
using PixelPhoto.Activities.Search;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Utils;

namespace PixelPhoto.Helpers.Controller
{
    public class TextSanitizer
    {
        public AutoLinkTextView AutoLinkTextView;
        public Activity Activity;

        public TextSanitizer(AutoLinkTextView linkTextView , Activity activity )
        {
            try
            {
                AutoLinkTextView = linkTextView;
                Activity = activity;
                AutoLinkTextView.AutoLinkOnClick += AutoLinkTextViewOnAutoLinkOnClick;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
        
        public void Load(string autoLinktext, string position = "Left")
        {
            try
            {
                AutoLinkTextView.AddAutoLinkMode(AutoLinkMode.ModePhone, AutoLinkMode.ModeEmail, AutoLinkMode.ModeHashtag, AutoLinkMode.ModeUrl, AutoLinkMode.ModeMention, AutoLinkMode.ModeCustom);

                if (position == "Right" || position == "right")
                {
                    AutoLinkTextView.SetPhoneModeColor(ContextCompat.GetColor(Activity, Resource.Color.right_ModePhone_color));
                    AutoLinkTextView.SetEmailModeColor(ContextCompat.GetColor(Activity, Resource.Color.right_ModeEmail_color));
                    AutoLinkTextView.SetHashtagModeColor(ContextCompat.GetColor(Activity, Resource.Color.right_ModeHashtag_color));
                    AutoLinkTextView.SetUrlModeColor(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color));
                    AutoLinkTextView.SetMentionModeColor(ContextCompat.GetColor(Activity, Resource.Color.right_ModeMention_color));
                    AutoLinkTextView.SetCustomModeColor(ContextCompat.GetColor(Activity, Resource.Color.right_ModeUrl_color));
                }
                else
                {
                    AutoLinkTextView.SetPhoneModeColor(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModePhone_color));
                    AutoLinkTextView.SetEmailModeColor(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeEmail_color));
                    AutoLinkTextView.SetHashtagModeColor(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeHashtag_color));
                    AutoLinkTextView.SetUrlModeColor(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color));
                    AutoLinkTextView.SetMentionModeColor(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeMention_color));
                    AutoLinkTextView.SetCustomModeColor(ContextCompat.GetColor(Activity, Resource.Color.AutoLinkText_ModeUrl_color));
                }
                var text = autoLinktext.Split('/');
                if (text.Count() > 1)
                { 
                    AutoLinkTextView.SetCustomRegex(@"\b("+ text.LastOrDefault() + @")\b");
                }

               string lastString = autoLinktext.Replace(" /", " ");
                if (!string.IsNullOrEmpty(lastString))
                   AutoLinkTextView.SetAutoLinkText(lastString);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void AutoLinkTextViewOnAutoLinkOnClick(object sender, AutoLinkOnClickEventArgs autoLinkOnClickEventArgs)
        {
            try
            {
                var typetext = Methods.FunString.Check_Regex(autoLinkOnClickEventArgs.P1);
                if (typetext == "Email")
                {
                    Methods.App.SendEmail(Activity, autoLinkOnClickEventArgs.P1);
                    return;
                }
                else if (typetext == "Website")
                {
                    String url = autoLinkOnClickEventArgs.P1;
                    if (!autoLinkOnClickEventArgs.P1.Contains("http"))
                    {
                        url = "http://" + autoLinkOnClickEventArgs.P1;
                    }
                   
                    var intent = new Intent(Application.Context, typeof(LocalWebViewActivity));
                    intent.PutExtra("URL", url);
                    intent.PutExtra("Type", url);
                    Activity.StartActivity(intent);
                    return;  
                }
                else if (typetext == "Hashtag")
                {
                    // Show All Post By Hash
                    Bundle bundle = new Bundle();
                    bundle.PutString("HashId", "");
                    bundle.PutString("HashName", Methods.FunString.DecodeString(autoLinkOnClickEventArgs.P1));

                    HashTagPostFragment profileFragment = new HashTagPostFragment
                    {
                        Arguments = bundle
                    };

                    ((HomeActivity)Activity).OpenFragment(profileFragment);

                    return;
                }
                else if (typetext == "Mention")
                { 
                    Bundle bundle = new Bundle();
                    bundle.PutString("Key", Methods.FunString.DecodeString(autoLinkOnClickEventArgs.P1));

                    SearchFragment searchFragment = new SearchFragment() 
                    {
                        Arguments = bundle
                    };

                    ((HomeActivity)Activity).OpenFragment(searchFragment);

                    return;
                }
                else if (typetext == "Number")
                {
                   // IMethods.App.SaveContacts(_activity, autoLinkOnClickEventArgs.P1, "", "2");
                    return;
                }
                else
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}