using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AT.Markushi.UI;
using JP.ShTs.StoriesProgressView;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.Story;
using PixelPhotoClient.RestCalls;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using File = Java.IO.File;
using Bumptech.Glide;
using Bumptech.Glide.Request;
using PixelPhoto.Activities.Editor;

namespace PixelPhoto.Activities.Story
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AddStoryActivity : AppCompatActivity
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private ImageView StoryImageView;
        private VideoView StoryVideoView;
        private CircleButton PlayIconVideo, AddStoryButton;
        private EditText EmojisIconEditText;
        private string PathStory = "", Type = "";
        private StoriesProgressView StoriesProgress;
        private long Duration;
        private HomeActivity GlobalContext;
        private TextView TxtEdit;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                Window.SetSoftInputMode(SoftInput.AdjustResize);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.AddStoryLayout);
                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                GlobalContext = HomeActivity.GetInstance();

                var dataUri = Intent.GetStringExtra("Uri") ?? "Data not available";
                if (dataUri != "Data not available" && !string.IsNullOrEmpty(dataUri)) PathStory = dataUri; // Uri file 
                var dataType = Intent.GetStringExtra("Type") ?? "Data not available";
                if (dataType != "Data not available" && !string.IsNullOrEmpty(dataType)) Type = dataType; // Type file  
                 
                if (Type == "image")
                    SetImageStory(PathStory);
                else
                    SetVideoStory(PathStory);

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
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                // Very important !
                StoriesProgress.Destroy();
                 
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                TxtEdit = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtEdit.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                TxtEdit.Visibility = ViewStates.Gone;

                StoryImageView = FindViewById<ImageView>(Resource.Id.imagstoryDisplay);
                StoryVideoView = FindViewById<VideoView>(Resource.Id.VideoView);
                PlayIconVideo = FindViewById<CircleButton>(Resource.Id.Videoicon_button);
                EmojisIconEditText = FindViewById<EditText>(Resource.Id.captionText);
                AddStoryButton = FindViewById<CircleButton>(Resource.Id.sendButton);

                StoriesProgress = FindViewById<StoriesProgressView>(Resource.Id.stories);
                StoriesProgress.Visibility = ViewStates.Gone;

                EmojisIconEditText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);
                EmojisIconEditText.SetHintTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);

                PlayIconVideo.Visibility = ViewStates.Gone;
                PlayIconVideo.Tag = "Play"; 
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
                Toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (Toolbar != null)
                {
                    Toolbar.Title = GetString(Resource.String.Lbl_AddStory);
                    Toolbar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    Toolbar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    SetSupportActionBar(Toolbar);
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
                    TxtEdit.Click += ToolbarTitleOnClick;  
                    AddStoryButton.Click += AddStoryButtonOnClick;
                    StoryVideoView.Completion += StoryVideoViewOnCompletion;
                    PlayIconVideo.Click += PlayIconVideoOnClick;
                }
                else
                {
                    TxtEdit.Click -= ToolbarTitleOnClick;
                    AddStoryButton.Click -= AddStoryButtonOnClick;
                    StoryVideoView.Completion -= StoryVideoViewOnCompletion;
                    PlayIconVideo.Click -= PlayIconVideoOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetImageStory(string url)
        {
            try
            {

                TxtEdit.Visibility = ViewStates.Visible;

                if (StoryImageView.Visibility == ViewStates.Gone)
                    StoryImageView.Visibility = ViewStates.Visible;

                var file = Android.Net.Uri.FromFile(new File(url));

                Glide.With(this).Load(file.Path).Apply(new RequestOptions()).Into(StoryImageView);

                //GlideImageLoader.LoadImage(this,file.Path, StoryImageView, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);
                
                if (StoryVideoView.Visibility == ViewStates.Visible)
                    StoryVideoView.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetVideoStory(string url)
        {
            try
            {    
                 TxtEdit.Visibility = ViewStates.Gone;

                if (StoryImageView.Visibility == ViewStates.Visible)
                    StoryImageView.Visibility = ViewStates.Gone;

                if (StoryVideoView.Visibility == ViewStates.Gone)
                    StoryVideoView.Visibility = ViewStates.Visible;

                PlayIconVideo.Visibility = ViewStates.Visible;
                PlayIconVideo.Tag = "Play";
                PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);

                if (StoryVideoView.IsPlaying)
                    StoryVideoView.Suspend();

                if (url.Contains("http"))
                {
                    StoryVideoView.SetVideoURI(Android.Net.Uri.Parse(url));
                }
                else
                {
                    var file = Android.Net.Uri.FromFile(new File(url));
                    StoryVideoView.SetVideoPath(file.Path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events
        private void ToolbarTitleOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent intent = new Intent(this, typeof(EditImageActivity));
                intent.PutExtra("PathImage", PathStory);
                intent.PutExtra("IdImage", "");
                StartActivityForResult(intent, 2000);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private void PlayIconVideoOnClick(object sender, EventArgs e)
        {
            try
            {
                if (PlayIconVideo.Tag.ToString() == "Play")
                { 
                    if (PathStory.Contains("http"))
                    {
                        StoryVideoView.SetVideoURI(Android.Net.Uri.Parse(PathStory));
                    }
                    else
                    {
                        var file = Android.Net.Uri.FromFile(new File(PathStory));
                        StoryVideoView.SetVideoPath(file.Path);
                    }
                    StoryVideoView.Start();

                    //MediaMetadataRetriever retriever = new MediaMetadataRetriever();
                    //if ((int)Build.VERSION.SdkInt >= 14)
                    //    retriever.SetDataSource(PathStory, new Dictionary<string, string>());
                    //else
                    //    retriever.SetDataSource(PathStory);

                    //Duration = Long.ParseLong(retriever.ExtractMetadata(MetadataKey.Duration));
                    //retriever.Release();

                    //StoriesProgress.Visibility = ViewStates.Gone;
                    //StoriesProgress.SetStoriesCount(1); // <- set stories
                    //StoriesProgress.SetStoryDuration(Duration); // <- set a story duration
                    //StoriesProgress.StartStories(); // <- start progress

                    PlayIconVideo.Tag = "Stop";
                    PlayIconVideo.SetImageResource(Resource.Drawable.ic_stop_white_24dp);
                }
                else
                {
                    //StoriesProgress.Visibility = ViewStates.Gone;
                    //StoriesProgress.Pause();

                    StoryVideoView.Pause();

                    PlayIconVideo.Tag = "Play";
                    PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void StoryVideoViewOnCompletion(object sender, EventArgs e)
        {
            try
            {
                //StoriesProgress.Visibility = ViewStates.Gone;
                //StoriesProgress.Pause();
                StoryVideoView.Pause();

                PlayIconVideo.Tag = "Play";
                PlayIconVideo.SetImageResource(Resource.Drawable.ic_play_arrow);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private async void AddStoryButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    string time = Methods.Time.TimeAgo(DateTime.Now);
                    int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                    string time2 = unixTimestamp.ToString();
                     
                    var attach = new Attachments
                    {
                        Id = 0,
                        TypeAttachment = "file",
                        FileSimple = PathStory,
                        FileUrl = PathStory,
                    };

                    (int respondCode, var respondString) = await RequestsAsync.Story.CreateStory(attach);
                    if (respondCode == 200)
                    {
                        if (respondString is CreateStoryObject storyObject)
                        {
                            AndHUD.Shared.ShowSuccess(this, GetText(Resource.String.Lbl_Done), MaskType.Clear, TimeSpan.FromSeconds(3));

                            var check = GlobalContext?.NewsFeedFragment?.StoryAdapter?.StoryList?.FirstOrDefault(a => a.UserId == Convert.ToInt32(UserDetails.UserId));
                            if (check != null)
                            {
                                check.Stories.Add(new FetchStoriesObject.Story()
                                {
                                    UserId = Convert.ToInt32(UserDetails.UserId),
                                    Id = storyObject.Id,
                                    Caption = EmojisIconEditText.Text,
                                    Owner = true,
                                    TimeText = time,
                                    Time = time2,
                                    Type = Type,
                                    MediaFile = PathStory
                                });
                            }
                            else
                            {
                                var userData = ListUtils.MyProfileList.FirstOrDefault();

                                List<FetchStoriesObject.Story> storiesList = new List<FetchStoriesObject.Story>
                                {
                                    new FetchStoriesObject.Story()
                                    {
                                        UserId = Convert.ToInt32(UserDetails.UserId),
                                        Id = storyObject.Id,
                                        Caption = EmojisIconEditText.Text,
                                        Owner = true,
                                        TimeText = time,
                                        Time = time2,
                                        Type = Type,
                                        MediaFile = PathStory,
                                        Duration = Duration.ToString(),
                                    }
                                };

                                GlobalContext?.NewsFeedFragment?.StoryAdapter?.StoryList?.Add(new FetchStoriesObject.Data()
                                {
                                    Id = storyObject.Id,
                                    Avatar = userData?.Avatar ?? UserDetails.Avatar,
                                    Type = "",
                                    Username = UserDetails.FullName,
                                    Owner = true,
                                    UserId = Convert.ToInt32(UserDetails.UserId),
                                    Stories = new List<FetchStoriesObject.Story>(storiesList),
                                    Name = AppTools.GetNameFinal(userData) ?? UserDetails.Username,
                                    Time = time2,
                                    Caption = EmojisIconEditText.Text,
                                    MediaFile = PathStory,
                                    Duration = Duration.ToString(),
                                });
                            }

                            GlobalContext?.NewsFeedFragment?.StoryAdapter?.NotifyDataSetChanged();

                            Finish();
                        } 
                    }
                    else Methods.DisplayReportResult(this, respondString);

                    AndHUD.Shared.Dismiss(this);
                }
                else
                {
                    Toast.MakeText(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                AndHUD.Shared.Dismiss(this);
            }
        }

        #endregion

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 2000 && resultCode == Result.Ok) // => NiceArtEditor
                {
                    //var imageId = data.GetStringExtra("ImageId") ?? "0";
                    var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                    if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                    {
                        try
                        {
                            PathStory = imagePath;
                            SetImageStory(imagePath); 
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


    }
}