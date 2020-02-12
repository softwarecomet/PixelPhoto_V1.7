using System;
using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Gms.Ads.Formats;
using Android.Gms.Ads.Reward;
using Android.Support.V7.Widget;
using Android.Views;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using Exception = System.Exception;
using Object = Java.Lang.Object;

namespace PixelPhoto.Helpers.Ads
{
    public static class AdsGoogle
    {
        private static int CountInterstitial;
        private static int CountRewarded;

        #region Interstitial

        private class AdmobInterstitial
        {
            private InterstitialAd Ad;

            public void ShowAd(Context context)
            {
                try
                {
                    Ad = new InterstitialAd(context);
                    Ad.AdUnitId = AppSettings.AdInterstitialKey;

                    var intlistener = new InterstitialAdListener(Ad);
                    intlistener.OnAdLoaded();
                    Ad.AdListener = intlistener;

                    var requestbuilder = new AdRequest.Builder();
                    requestbuilder.AddTestDevice(UserDetails.AndroidId);
                    Ad.LoadAd(requestbuilder.Build());
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }
        }

        private class InterstitialAdListener : AdListener
        {
            private readonly InterstitialAd Ad;

            public InterstitialAdListener(InterstitialAd ad)
            {
                Ad = ad;
            }

            public override void OnAdLoaded()
            {
                base.OnAdLoaded();

                if (Ad.IsLoaded)
                    Ad.Show();
            }
        }


        public static void Ad_Interstitial(Context context)
        {
            try
            {
                if (AppSettings.ShowAdMobInterstitial)
                {
                    if (CountInterstitial == AppSettings.ShowAdMobInterstitialCount)
                    {
                        CountInterstitial = 0;
                        AdmobInterstitial ads = new AdmobInterstitial();
                        ads.ShowAd(context);
                    }

                    CountInterstitial++;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Native

        private class AdmobNative : Object, UnifiedNativeAd.IOnUnifiedNativeAdLoadedListener
        {
            private TemplateView Template;
            private Activity Context;
            public void ShowAd(Activity context)
            {
                try
                {
                    Context = context;
                    Template = Context.FindViewById<TemplateView>(Resource.Id.my_template);
                    Template.Visibility = ViewStates.Gone;

                    if (AppSettings.ShowAdMobNative)
                    {
                        AdLoader.Builder builder = new AdLoader.Builder(Context, AppSettings.AdAdMobNativeKey);
                        builder.ForUnifiedNativeAd(this);
                        VideoOptions videoOptions = new VideoOptions.Builder()
                            .SetStartMuted(true)
                            .Build();
                        NativeAdOptions adOptions = new NativeAdOptions.Builder()
                            .SetVideoOptions(videoOptions)
                            .Build();

                        builder.WithNativeAdOptions(adOptions);

                        AdLoader adLoader = builder.WithAdListener(new AdListener()).Build();
                        adLoader.LoadAd(new AdRequest.Builder().Build());
                    }
                    else
                    {
                        Template.Visibility = ViewStates.Gone;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnUnifiedNativeAdLoaded(UnifiedNativeAd ad)
            {
                try
                {
                    NativeTemplateStyle styles = new NativeTemplateStyle.Builder().Build();

                    if (Template.GetTemplateTypeName() == TemplateView.BigTemplate)
                    {
                        Template.PopulateUnifiedNativeAdView(ad);
                    }
                    else
                    {
                        Template.SetStyles(styles);
                        Template.SetNativeAd(ad);
                    }

                    Template.Visibility = ViewStates.Visible;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void Ad_AdmobNative(Activity context)
        {
            try
            {
                if (AppSettings.ShowAdMobNative)
                {
                    AdmobNative ads = new AdmobNative();
                    ads.ShowAd(context);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        //Rewarded Video >>
        //===================================================

        #region Rewarded

        private class AdmobRewardedVideo : AdListener, IRewardedVideoAdListener
        {
            private IRewardedVideoAd Rad;

            public void ShowAd(Context context)
            {
                try
                {
                    // Use an activity context to get the rewarded video instance.
                    Rad = MobileAds.GetRewardedVideoAdInstance(context);
                    Rad.RewardedVideoAdListener = this;

                    OnRewardedVideoAdLoaded();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }

            public override void OnAdLoaded()
            {
                try
                {
                    base.OnAdLoaded();

                    OnRewardedVideoAdLoaded();

                    if (Rad.IsLoaded)
                        Rad.Show();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                }
            }

            public void OnRewarded(IRewardItem reward)
            {
                //Toast.MakeText(Application.Context, "onRewarded! currency: " + reward.Type + "  amount: " + reward.Amount , ToastLength.Short).Show();

                if (Rad.IsLoaded)
                    Rad.Show();
            }


            public void OnRewardedVideoAdClosed()
            {

            }

            public void OnRewardedVideoAdFailedToLoad(int errorCode)
            {
                //Toast.MakeText(Application.Context, "No ads currently available", ToastLength.Short).Show();
            }

            public void OnRewardedVideoAdLeftApplication()
            {

            }

            public void OnRewardedVideoAdLoaded()
            {
                try
                {
                    if (!Rad.IsLoaded)
                    {
                        Rad.LoadAd(AppSettings.AdRewardVideoKey, new AdRequest.Builder().Build());
                    }


                    //Bundle extras = new Bundle();
                    //extras.PutBoolean("_noRefresh", true);

                    //var requestBuilder = new AdRequest.Builder();
                    //requestBuilder.AddTestDevice(UserDetails.AndroidId);
                    //requestBuilder.AddNetworkExtrasBundle(new AdMobAdapter().Class, extras);
                    //Rad.UserId = Application.Context.GetString(Resource.String.admob_app_id);
                    //Rad.LoadAd(AppSettings.AdRewardVideoKey, requestBuilder.Build());
                    //Rad.Show();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnRewardedVideoAdOpened()
            {

            }

            public void OnRewardedVideoCompleted()
            {

            }

            public void OnRewardedVideoStarted()
            {

            }
        }

        public static void Ad_RewardedVideo(Context context)
        {
            try
            {
                if (AppSettings.ShowAdMobRewardVideo)
                {
                    if (CountRewarded == AppSettings.ShowAdMobRewardedVideoCount)
                    {
                        CountRewarded = 0;
                        AdmobRewardedVideo ads = new AdmobRewardedVideo();
                        ads.ShowAd(context);
                    }

                    CountRewarded++;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        public static void InitAdView(AdView mAdView, RecyclerView mRecycler)
        {
            try
            {
                if (AppSettings.ShowAdMobBanner)
                {
                    mAdView.Visibility = ViewStates.Visible;
                    var adRequest = new AdRequest.Builder();
                    adRequest.AddTestDevice(UserDetails.AndroidId);
                    mAdView.LoadAd(adRequest.Build());
                    mAdView.AdListener = new MyAdListener(mAdView, mRecycler);
                }
                else
                {
                    mAdView.Pause();
                    mAdView.Visibility = ViewStates.Gone;
                    if (mRecycler != null) Methods.SetMargin(mRecycler, 10, 0, 0, 0);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private class MyAdListener : AdListener
        {
            private readonly AdView MAdView;
            private readonly RecyclerView MRecycler;
            public MyAdListener(AdView mAdView, RecyclerView mRecycler)
            {
                MAdView = mAdView;
                MRecycler = mRecycler;
            }

            public override void OnAdFailedToLoad(int p0)
            {
                try
                {
                    MAdView.Visibility = ViewStates.Gone;
                    if (MRecycler != null) Methods.SetMargin(MRecycler, 10, 0, 0, 0);
                    base.OnAdFailedToLoad(p0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnAdLoaded()
            {
                try
                {
                    MAdView.Visibility = ViewStates.Visible;
                    base.OnAdLoaded();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}