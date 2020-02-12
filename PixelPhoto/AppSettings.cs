//###############################################################
// Author >> Elin Doughouz 
// Copyright (c) PixelPhoto 15/07/2018 All Right Reserved
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// Follow me on facebook >> https://www.facebook.com/Elindoughous
//=========================================================

//For the accuracy of the icon and logo, please use this website " http://nsimage.brosteins.com " and add images according to size in folders " mipmap " 

using PixelPhoto.Helpers.Utils;

namespace PixelPhoto
{
    public static class AppSettings
    {
        //Main Settings >>>>>
        //*********************************************************
        public static string TripleDesAppServiceProvider = "NFYnA2qriwLLUe74dlNM90fBmeMKm8QuEm2TgU8DdI2XKM6m4b+HxaxiYuUXYUUhRzPhUb5Kr/3UuiouvWr9xGsHJe/S9P8ygJcTp6lKFSyToapHBgIPMkIHXyoGK5FTv9Zouc+35yk29rZq7HOTe1ohHhxN2PC9sO3M6X+JWNwgj1Rl7d9rSc7gm6ytCNI/CNU2SIfxW7uuaD4xZIk1ToYfJHXcbfPmQB75b+DIE+bjppoIIIw+jJobF2xwm69p";

        public static string Version = "1.7";
        public static string ApplicationName = "PixelPhoto";

        //Main Colors >>
        //*********************************************************
        public static string MainColor = "#f65599";
        public static string StartColor = MainColor;
        public static string EndColor = "#4d0316";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "e06a3585-d0ac-44ef-b2df-0c24abc23682";  

        public static string YoutubeKey = "AIzaSyA-JSf9CU1cdMpgzROCCUpl4wOve9S94ZU";

        //********************************************************* 
        public static bool ImageCropping = false;

        public static bool SetApisReportMode = false; 

        //Set Theme Welcome Pages 
        //*********************************************************
        public static bool DisplayImageOnRegisterBackground = false;
        public static bool DisplayImageOnForgetPasswordBackground = false;
        public static bool DisplayImageOnLoginBackground = false;

        public static string URlImageOnFirstBackground = "FirstBackground.png";
        public static string URlImageOnLoginBackground = "loginBackground.png";
        public static string URlImageOnForgetPasswordBackground = "ForgetPasswordBackground.png";
        public static string URlImageOnRegisterBackground = "RegisterBackground.png";

        //*********************************************************

        //AdMob >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static bool ShowAdMobBanner = true; 
        public static bool ShowAdMobInterstitial = true; 
        public static bool ShowAdMobRewardVideo = true; 
        public static bool ShowAdMobNative = true; 

        public static string AdInterstitialKey = "ca-app-pub-5135691635931982/3468020887"; 
        public static string AdRewardVideoKey = "ca-app-pub-5135691635931982/6449407444";
        public static string AdAdMobNativeKey = "ca-app-pub-5135691635931982/3440100725"; 

        //Three times after entering the ad is displayed
        public static int ShowAdMobInterstitialCount = 3; 
        public static int ShowAdMobRewardedVideoCount = 3; 
        public static int ShowAdMobNativeCount = 10; 

        //Set Theme Full Screen App
        //*********************************************************
        public static bool EnableFullScreenApp = false;

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file or AndroidManifest.xml
        //Facebook >> ../values/analytic.xml .. 
        //Google >> ../Properties/AndroidManifest.xml .. line 26
        //*********************************************************
        public static bool ShowFacebookLogin = true;
        public static bool ShowGoogleLogin = true;

        public static readonly string ClientId = "428358750506-5p97f2vp91pn52oculdc4kck72hl973f.apps.googleusercontent.com";
        public static readonly string ClientSecret = "R0xLM-lNQ3nH19yhMRX5o_da";

        //########################### 

        //Last_Messages Page >>
        ///********************************************************* 
        public static bool RunSoundControl = true;
        public static int RefreshChatActivitiesSeconds = 6000; // 6 Seconds
        public static int MessageRequestSpeed = 3000; // 3 Seconds
        public static int AvatarPostSize = 60;
        public static int ImagePostSize = 300;

        //Add Post
        public static bool ShowGalleryImage = true;
        public static bool ShowGalleryVideo = true;
        public static bool ShowMention = true;
        public static bool ShowCamera = true;
        public static bool ShowGif = true;
        public static bool ShowEmbedVideo = true;

        public static bool ShowFunding = true;  
        public static bool ShowFullScreenVideoPost = true; 

        //Profile Page >>
        ///*********************************************************  
        public static ProfileTheme ProfileTheme = ProfileTheme.TikTheme; 
        public static bool ShowEmailAccount = false;


        //Settings Page >> General Account 
        public static bool ShowSettingsGeneralAccount = true;
        public static bool ShowSettingsAccountPrivacy = true;
        public static bool ShowSettingsPassword = true;
        public static bool ShowSettingsBlockedUsers = true;
        public static bool ShowSettingsNotifications = true;
        public static bool ShowSettingsDeleteAccount = true;

        //Set Theme Tab
        //*********************************************************
        public static bool SetTabColoredTheme = false;
        public static bool SetTabDarkTheme = false; //#New

        public static string TabColoredColor = MainColor;
        public static bool SetTabIsTitledWithText = false;

        //Bypass Web Errors  
        //*********************************************************
        public static bool TurnTrustFailureOnWebException = false;
        public static bool TurnSecurityProtocolType3072On = false;

        //Show custom error reporting page
        public static bool RenderPriorityFastPostLoad = true;

        //*********************************************************
        public static bool StartYoutubeAsIntent { get; set; }
    }
}
