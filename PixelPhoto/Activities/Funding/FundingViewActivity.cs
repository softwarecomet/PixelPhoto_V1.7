using System;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient;
using PixelPhotoClient.GlobalClass;
using Plugin.Share;
using Plugin.Share.Abstractions;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PixelPhoto.Activities.Funding
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class FundingViewActivity : AppCompatActivity
    {
        #region Variables Basic

        private ImageView ImageUser, ImageFunding;
        private TextView TxtUsername, TxtTime, TxtTitle, TxtDescription, TxtFundRaise;
        private ProgressBar ProgressBar;
        private Button BtnDonate;
        private FundingDataObject DataObject;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Methods.App.FullScreenApp(this);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.FundingViewLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                GetDataFunding();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.action_share:
                    ShareEvent();
                    break;

                case Resource.Id.action_Edit:
                    EditEvent();
                    break;

                case Resource.Id.action_copy:
                    CopyLinkEvent();
                    break; 
            }

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MenuFundingShare, menu);
            ChangeMenuIconColor(menu, AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

            var item = menu.FindItem(Resource.Id.action_Edit);

            bool owner = DataObject.UserId.ToString() == UserDetails.UserId; 
            item?.SetVisible(owner);
             
            return base.OnCreateOptionsMenu(menu); 
        }

        private void ChangeMenuIconColor(IMenu menu, Color color)
        {
            for (int i = 0; i < menu.Size(); i++)
            {
                var drawable = menu.GetItem(i).Icon;
                if (drawable == null) continue;
                drawable.Mutate();
                drawable.SetColorFilter(new PorterDuffColorFilter(color, PorterDuff.Mode.SrcAtop));
            }
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                ImageUser = FindViewById<ImageView>(Resource.Id.imageAvatar);
                ImageFunding = FindViewById<ImageView>(Resource.Id.imageFunding);

                TxtUsername = FindViewById<TextView>(Resource.Id.username);
                TxtTime = FindViewById<TextView>(Resource.Id.time);
                TxtTitle = FindViewById<TextView>(Resource.Id.title);
                TxtDescription = FindViewById<TextView>(Resource.Id.description);
                TxtFundRaise = FindViewById<TextView>(Resource.Id.fund_raise);

                BtnDonate = FindViewById<Button>(Resource.Id.DonateButton);

                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = " ";
                    toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
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
                    BtnDonate.Click += BtnDonateOnClick;
                }
                else
                {
                    BtnDonate.Click -= BtnDonateOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Events

        //Event Menu >> Copy Link
        private void EditEvent()
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditFundingActivity));
                intent.PutExtra("FundingObject", JsonConvert.SerializeObject(DataObject));
                StartActivity(intent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

         //Event Menu >> Copy Link
        private void CopyLinkEvent()
        {
            try
            {
                var clipboardManager = (ClipboardManager)GetSystemService(ClipboardService);

                var clipData = ClipData.NewPlainText("text", Client.WebsiteUrl + "/funding/" + DataObject.HashedId); 
                clipboardManager.PrimaryClip = clipData;

                Toast.MakeText(this, GetText(Resource.String.Lbl_Text_copied), ToastLength.Short).Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event Menu >> Share
        private async void ShareEvent()
        {
            try
            {
                //Share Plugin same as video
                if (!CrossShare.IsSupported) return;

                await CrossShare.Current.Share(new ShareMessage
                {
                    Title = DataObject.Title,
                    Text = DataObject.Description,
                    Url = Client.WebsiteUrl + "/funding/" + DataObject.HashedId
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //open web view 
        private void BtnDonateOnClick(object sender, EventArgs e)
        {
            try
            {
                new IntentController(this).OpenBrowserFromApp(Client.WebsiteUrl + "/funding/" + DataObject.HashedId);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        #endregion

        private void GetDataFunding()
        {
            try
            {
                DataObject = JsonConvert.DeserializeObject<FundingDataObject>(Intent.GetStringExtra("ItemObject"));
                if (DataObject != null)
                {
                    GlideImageLoader.LoadImage(this, DataObject.UserData.Avatar, ImageUser, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    GlideImageLoader.LoadImage(this, DataObject.Image, ImageFunding, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                     
                    TxtUsername.Text = Methods.FunString.DecodeString(DataObject.UserData.Name);
                     
                    TxtTime.Text = Methods.Time.TimeAgo(Convert.ToInt32(DataObject.Time)) ;

                    TxtTitle.Text = Methods.FunString.DecodeString(DataObject.Title);
                    TxtDescription.Text = Methods.FunString.DecodeString(DataObject.Description);
                     
                    ProgressBar.Progress = Convert.ToInt32(DataObject.Bar);

                     //$0 Raised of $1000000
                    TxtFundRaise.Text = "$" +  DataObject.Raised.ToString(CultureInfo.InvariantCulture) + GetString(Resource.String.Lbl_RaisedOf) + " " + "$" + DataObject.Amount;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}