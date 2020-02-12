﻿using System;
using System.Collections.Generic;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using Com.Sothree.Slidinguppanel;
using Java.IO;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.AddPost.Adapters;
using PixelPhoto.Activities.Editor;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.Post;
using PixelPhotoClient.GlobalClass;
using Console = System.Console;
using Exception = System.Exception;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Activities.AddPost
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class AddPostActivity : AppCompatActivity, SlidingUpPanelLayout.IPanelSlideListener, MaterialDialog.ISingleButtonCallback,MaterialDialog.IInputCallback, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private Toolbar Toolbar;
        private EditText DataEditText;
        private string TypePost = "", TypeDialog = "", EmbedVideoLink = "", MentionText = "", GifFile = "";
        private SlidingUpPanelLayout SlidingUpPanel;
        private ImageView PostSectionImage;
        private RecyclerView PostTypeRecyclerView, AttachmentsRecyclerView;
        private TextView UserNameTextView , SaveTextView;
        private ImageView IconImage, IconHappy, IconTag;
        private AutoLinkTextView ExtendTextView;
        private TextView PostTypeButton;
        private MainPostAdapter MainPostAdapter;
        private AttachmentsAdapter AttachmentsAdapter; 
        private Classes.TypePostEnum TypePostFinal;
        private LinearLayout MainLayout;
        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                // Create your application here
                SetContentView(Resource.Layout.AddPostLayout);

                Methods.Path.Chack_MyFolder();
                 
                TypePost = Intent.GetStringExtra("TypePost");

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                LoadDataUser();
                  
                if (TypePost == "Image") // image
                {
                    OpenTypePost(Classes.TypePostEnum.Image);
                }
                else if (TypePost == "Video") // video
                {
                    OpenTypePost(Classes.TypePostEnum.Video);
                }
                else if(TypePost == "Gif") // gif
                {
                    OpenTypePost(Classes.TypePostEnum.Gif);
                }
                else if (TypePost == "EmbedVideo") // broken_link
                {
                    OpenTypePost(Classes.TypePostEnum.EmbedVideo);
                } 
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

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    var resultIntent = new Intent();
                    SetResult(Result.Canceled, resultIntent);
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
                SaveTextView = FindViewById<TextView>(Resource.Id.toolbar_title);
                DataEditText = FindViewById<EditText>(Resource.Id.editTxtEmail);

                SaveTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);

                MainLayout = FindViewById<LinearLayout>(Resource.Id.main);
                MainLayout.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.layout_bg_profile_friends_dark : Resource.Drawable.layout_bg_profile_friends);

                SlidingUpPanel = FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
                PostSectionImage = FindViewById<ImageView>(Resource.Id.postsectionimage);
                PostTypeRecyclerView = FindViewById<RecyclerView>(Resource.Id.Recyler);
                AttachmentsRecyclerView = FindViewById<RecyclerView>(Resource.Id.AttachementRecyler);
                UserNameTextView = FindViewById<TextView>(Resource.Id.card_name);
                IconImage = FindViewById<ImageView>(Resource.Id.ImageIcon);
                IconHappy = FindViewById<ImageView>(Resource.Id.Activtyicon);
                IconTag = FindViewById<ImageView>(Resource.Id.TagIcon);
                IconTag.Tag = "Close";

                ExtendTextView = FindViewById<AutoLinkTextView>(Resource.Id.MentionTextview);
                PostTypeButton = FindViewById<TextView>(Resource.Id.cont);

                DataEditText.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);
                DataEditText.SetHintTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);
                DataEditText.ClearFocus();
                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                SlidingUpPanel.AddPanelSlideListener(this);

                if (AppSettings.SetTabDarkTheme) return;
                SlidingUpPanel.SetBackgroundResource(Resource.Color.white);
                PostTypeRecyclerView.SetBackgroundResource(Resource.Color.white);
                AttachmentsRecyclerView.SetBackgroundResource(Resource.Color.white);
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
                    Toolbar.Title = GetText(Resource.String.Lbl_AddPost);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                PostTypeRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
                MainPostAdapter = new MainPostAdapter(this);
                PostTypeRecyclerView.SetAdapter(MainPostAdapter);

                AttachmentsAdapter = new AttachmentsAdapter(this);
                AttachmentsRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                AttachmentsRecyclerView.SetAdapter(AttachmentsAdapter);
                AttachmentsRecyclerView.NestedScrollingEnabled = false;
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
                    AttachmentsAdapter.DeleteItemClick += AttachmentsAdapterOnDeleteItemClick;
                    MainPostAdapter.ItemClick += MainPostAdapterOnItemClick;
                    AttachmentsAdapter.ItemEditClick += AttachmentsAdapterOnItemEditClick;
                    SaveTextView.Click += SaveTextViewOnClick;
                }
                else
                {
                    AttachmentsAdapter.DeleteItemClick -= AttachmentsAdapterOnDeleteItemClick;
                    MainPostAdapter.ItemClick -= MainPostAdapterOnItemClick;
                    AttachmentsAdapter.ItemEditClick -= AttachmentsAdapterOnItemEditClick;
                    SaveTextView.Click -= SaveTextViewOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AttachmentsAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0)
                {
                    var item = AttachmentsAdapter.GetItem(position);
                    if (item != null)
                    {
                        AttachmentsAdapter.Remove(item);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Load Data User
 
        private async void LoadDataUser()
        {
            try
            {
                 new SqLiteDatabase().GetMyProfile();

                if (ListUtils.MyProfileList.Count == 0)
                    await ApiRequest.GetProfile_Api(this);

                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                if (dataUser != null)
                {
                    GlideImageLoader.LoadImage(this, dataUser.Avatar, PostSectionImage, ImageStyle.CircleCrop,ImagePlaceholders.Drawable);
                    UserNameTextView.Text = dataUser.Name;
                    PostTypeButton.Text = TypePost;
                }
                else
                {
                    GlideImageLoader.LoadImage(this, UserDetails.Avatar, PostSectionImage,  ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                    UserNameTextView.Text = UserDetails.Username;
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Event

        //Open NiceArt Editor
        private void AttachmentsAdapterOnItemEditClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position >= 0)
                {
                    var item = AttachmentsAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        Intent intent = new Intent(this, typeof(EditImageActivity));
                        intent.PutExtra("PathImage", item.FileUrl);
                        intent.PutExtra("IdImage", item.Id.ToString());
                        StartActivityForResult(intent, 2000);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Open Type page post
        private void MainPostAdapterOnItemClick(object sender, MainPostAdapterClickEventArgs e)
        {
            try
            {
                if (MainPostAdapter.PostTypeList[e.Position] != null)
                {
                    if (MainPostAdapter.PostTypeList[e.Position].Id == 1) //Image Gallery
                    {
                        OpenTypePost(Classes.TypePostEnum.Image);
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 2) //video Gallery
                    {
                        OpenTypePost(Classes.TypePostEnum.Video);
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 3) // Mention
                    {
                        OpenTypePost(Classes.TypePostEnum.Mention);
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 4) // Camera
                    {
                        OpenTypePost(Classes.TypePostEnum.Camera);
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 5) // Gif
                    {
                        OpenTypePost(Classes.TypePostEnum.Gif);
                    }
                    else if (MainPostAdapter.PostTypeList[e.Position].Id == 6) // EmbedVideo
                    {
                        OpenTypePost(Classes.TypePostEnum.EmbedVideo);
                    } 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Save Post
        private void SaveTextViewOnClick(object sender, EventArgs e)
        {
            try
            { 
                if (TypePostFinal == Classes.TypePostEnum.EmbedVideo)
                {
                    if (Methods.FunString.IsUrlValid(EmbedVideoLink) && !string.IsNullOrEmpty(EmbedVideoLink))
                    {
                        AddPostApi(EmbedVideoLink);
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_EmbedVideoUrlError), ToastLength.Short).Show();
                    }
                }
                else
                {
                    AddPostApi();
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region MaterialDialog
  
        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (TypeDialog == "PostBack")
                {
                    if (p1 == DialogAction.Positive)
                    {
                        p0.Dismiss();

                        var resultIntent = new Intent(); 
                        SetResult(Result.Canceled, resultIntent);
                        Finish(); 
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else if (TypeDialog == "EmbedVideo")
                {
                    if (p1 == DialogAction.Positive)
                    {
                       
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {

                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }

                if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnInput(MaterialDialog p0, ICharSequence p1)
        {
            try
            {
                if (p1.Length() > 0)
                {
                    var strName = p1.ToString();
                    EmbedVideoLink = strName;

                    if (!string.IsNullOrEmpty(EmbedVideoLink))
                    {
                        var attach = new Attachments
                        {
                            Id = AttachmentsAdapter.AttachmentsList.Count + 1,
                            TypeAttachment = "EmbedVideo",
                            FileSimple = "EmbedVideo_File",
                            FileUrl = "EmbedVideo_File"
                        };

                        AttachmentsAdapter.Add(attach);
                    }
                    else
                    {
                        Toast.MakeText(this, GetString(Resource.String.Lbl_EmbedVideoUrlError), ToastLength.Short).Show();
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                if (TypeDialog == "PostVideos")
                {
                    if (itemString.ToString() == GetText(Resource.String.Lbl_VideoGallery))
                    {
                        TypePostFinal = Classes.TypePostEnum.Video;
                        // Check if we're running on Android 5.0 or higher
                        if ((int) Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 501 => video Gallery
                            new IntentController(this).OpenIntentVideoGallery();
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 501 => video Gallery
                                new IntentController(this).OpenIntentVideoGallery();
                            }
                            else
                            {
                                RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                }, 501);
                            }
                        }
                    }
                    else if (itemString.ToString() == GetText(Resource.String.Lbl_RecordVideoFromCamera))
                    {
                        TypePostFinal = Classes.TypePostEnum.Video;
                        // Check if we're running on Android 5.0 or higher
                        if ((int) Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 513 => video Camera
                            new IntentController(this).OpenIntentVideoCamera();
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 513 => video Camera
                                new IntentController(this).OpenIntentVideoCamera();
                            }
                            else
                            {
                                RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                }, 513);
                            }
                        }
                    }
                }
                else
                {
                    if (itemString.ToString() == GetText(Resource.String.Lbl_ImageGallery))
                    {
                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 500 => Image Gallery
                            new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 500 => Image Gallery
                                new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                            }
                            else
                            {
                                RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                }, 500);
                            }
                        }
                    }
                    else
                    {
                        // Check if we're running on Android 5.0 or higher
                        if ((int)Build.VERSION.SdkInt < 23)
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                        }
                        else
                        {
                            if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                                && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                            {
                                //requestCode >> 503 => Camera
                                new IntentController(this).OpenIntentCamera();

                            }
                            else
                            {
                                RequestPermissions(new[]
                                {
                                    Manifest.Permission.Camera,
                                    Manifest.Permission.ReadExternalStorage,
                                    Manifest.Permission.WriteExternalStorage,
                                }, 503);
                            }
                        }
                    } 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region SlidingUpPanelLayout
         
        public void OnPanelStateChanged(View p0, SlidingUpPanelLayout.PanelState p1, SlidingUpPanelLayout.PanelState p2)
        {
            try
            {
                if (p1 == SlidingUpPanelLayout.PanelState.Expanded && p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (IconTag.Tag.ToString() == "Open")
                    {
                        IconTag.SetImageResource(Resource.Drawable.ic__Attach_tag);
                        IconTag.Tag = "Close";
                        IconImage.Visibility = ViewStates.Visible;
                        IconHappy.Visibility = ViewStates.Visible;
                    }
                }
                else if (p1 == SlidingUpPanelLayout.PanelState.Collapsed &&
                         p2 == SlidingUpPanelLayout.PanelState.Dragging)
                {
                    if (IconTag.Tag.ToString() == "Close")
                    {
                        IconTag.SetImageResource(Resource.Drawable.ic_action_arrow_down_sign);
                        IconTag.Tag = "Open";
                        IconImage.Visibility = ViewStates.Invisible;
                        IconHappy.Visibility = ViewStates.Invisible;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void SlidingUpPanelLayout.IPanelSlideListener.OnPanelSlide(View p0, float p1)
        {

        }

        #endregion

        #region Fun Post

        private string LoadPostStrings()
        {
            try
            {
                var newMentionText = string.Empty;

                if (!string.IsNullOrEmpty(MentionText))
                    newMentionText += " " + GetText(Resource.String.Lbl_With) + " " + MentionText.Remove(MentionText.Length - 1,1);

                var mainString = newMentionText;
                return mainString;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }

        private void OpenTypePost(Classes.TypePostEnum type)
        {
            try
            { 
                if (type == Classes.TypePostEnum.Image)
                {
                    TypePostFinal = Classes.TypePostEnum.Image;

                    TypeDialog = "PostImages";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_ImageGallery));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_Camera));

                    dialogList.Title(GetText(Resource.String.Lbl_SelectImageFrom));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show(); 
                }
                else if (type == Classes.TypePostEnum.Video)
                {
                    TypePostFinal = Classes.TypePostEnum.Video;

                    TypeDialog = "PostVideos";

                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    arrayAdapter.Add(GetText(Resource.String.Lbl_VideoGallery));
                    arrayAdapter.Add(GetText(Resource.String.Lbl_RecordVideoFromCamera));

                    dialogList.Title(GetText(Resource.String.Lbl_SelectVideoFrom));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show();
                }
                else if (type == Classes.TypePostEnum.Mention)
                { 
                    StartActivityForResult(new Intent(this, typeof(MentionActivity)), 3);
                }
                else if (type == Classes.TypePostEnum.Camera)
                {
                    TypePostFinal = Classes.TypePostEnum.Image;
                    // Check if we're running on Android 5.0 or higher
                    if ((int)Build.VERSION.SdkInt < 23)
                    {
                        //requestCode >> 503 => Camera
                        new IntentController(this).OpenIntentCamera();
                    }
                    else
                    {
                        if (CheckSelfPermission(Manifest.Permission.Camera) == Permission.Granted
                            && CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted
                            && CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                        {
                            //requestCode >> 503 => Camera
                            new IntentController(this).OpenIntentCamera();
                          
                        }
                        else
                        {
                            RequestPermissions(new[]
                            {
                                Manifest.Permission.Camera,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.WriteExternalStorage,
                            }, 503);
                        }
                    }
                }
                else if (type == Classes.TypePostEnum.Gif)
                {
                    TypePostFinal = Classes.TypePostEnum.Gif;
                    StartActivityForResult(new Intent(this, typeof(GifActivity)), 5);
                }
                else if (type == Classes.TypePostEnum.EmbedVideo)
                {
                    TypePostFinal = Classes.TypePostEnum.EmbedVideo;
                    TypeDialog = "EmbedVideo";

                    AttachmentsAdapter.RemoveAll();

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    dialog.Title(Resource.String.Lbl_EmbedVideo);
                    dialog.Input(Resource.String.Lbl_EmbedVideoUrl, 0, false, this);
                    dialog.InputType(InputTypes.TextFlagImeMultiLine);
                    dialog.PositiveText(GetText(Resource.String.Lbl_Submit)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_Cancel)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }

                TypePost = TypePostFinal.ToString();
                PostTypeButton.Text = TypePostFinal.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void AddPostApi(string url = "")
        {
            try
            {
                var content = DataEditText.Text + " " + ExtendTextView.Text;

                string time = Methods.Time.TimeAgo(DateTime.Now);
                int unixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                string time2 = unixTimestamp.ToString();
                 
                List<MediaSet> listFile = new List<MediaSet>(); 
                if (AttachmentsAdapter.AttachmentsList.Count > 0)
                {
                    listFile.AddRange(AttachmentsAdapter.AttachmentsList.Select(mediaSet => new MediaSet()
                    {
                        Id = mediaSet.Id,
                        UserId = Convert.ToInt32(UserDetails.UserId),
                        File = mediaSet.FileUrl,
                        Extra = mediaSet.FileSimple,
                        PostId = unixTimestamp,
                    }));
                }

                var userData = ListUtils.MyProfileList.FirstOrDefault();

                PostsObject postsObject = new PostsObject()
                {
                    Time = time2,
                    Name = AppTools.GetNameFinal(userData) ?? UserDetails.Username,
                    Avatar = userData?.Avatar ?? UserDetails.Avatar,
                    Comments = new List<CommentObject>(),
                    Dailymotion = "",
                    Description = content,
                    IsLiked = false,
                    IsOwner = true,
                    IsSaved = false,
                    IsShouldHide = false,
                    IsVerified = 0,
                    Likes = 0,
                    MediaSet = listFile,
                    PostId = unixTimestamp,
                    Type = TypePost,
                    UserId = Convert.ToInt32(userData?.UserId),
                    Username = userData?.Username ?? UserDetails.Username,
                    TimeText = time,
                    Votes = 0,
                    Link = url,
                    Mp4 = "",
                    Playtube = "",
                    Registered = "",
                    Reported = false,
                    Views = 0,
                    Vimeo = "",
                    Youtube = url.Split('/').Last(),
                };

                // put the String to pass back into an Intent and close this activity
                var resultIntent = new Intent();
                resultIntent.PutExtra("PostUrl",url);
                resultIntent.PutExtra("PostData", JsonConvert.SerializeObject(postsObject));
                SetResult(Result.Ok, resultIntent);
                Finish();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            { 
                base.OnActivityResult(requestCode, resultCode, data);
                SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);

                if (requestCode == 500 && resultCode == Result.Ok) // => Image Gallery
                {
                    if (data.ClipData != null)
                    {
                        var mClipData = data.ClipData;
                        var mArrayUri = new List<Uri>();
                        for (var i = 0; i < mClipData.ItemCount; i++)
                        {
                            var item = mClipData.GetItemAt(i);
                            var uri = item.Uri;
                             
                            var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                            if (filepath != null)
                            {
                                var check = AppTools.CheckMimeTypesWithServer(filepath);
                                if (!check)
                                {
                                    //this file not supported on the server , please select another file 
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                                    return;
                                }

                                mArrayUri.Add(Uri.Parse(filepath));
                            } 
                        }

                        if (mArrayUri.Count > 0)
                        {
                            var videoAttach = AttachmentsAdapter.AttachmentsList.Where(a => !a.TypeAttachment.Contains("images")).ToList();

                            if (videoAttach.Count > 0)
                                foreach (var video in videoAttach)
                                    AttachmentsAdapter.Remove(video);

                            foreach (var attach in mArrayUri.Select(item => new Attachments
                            {
                                Id = AttachmentsAdapter.AttachmentsList.Count + 1,
                                TypeAttachment = "images[]",
                                FileSimple = item.Path,
                                FileUrl = item.Path
                            }))
                            {
                                AttachmentsAdapter.Add(attach);
                            }
                        }
                    }
                    else
                    {
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                        if (filepath != null)
                        {
                            var check = AppTools.CheckMimeTypesWithServer(filepath);
                            if (!check)
                            {
                                //this file not supported on the server , please select another file 
                                Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                                return;
                            }

                            var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                            if (type == "Image")
                            { 
                                var attach = new Attachments
                                {
                                    Id = AttachmentsAdapter.AttachmentsList.Count + 1,
                                    TypeAttachment = "images[]",
                                    FileSimple = filepath,
                                    FileUrl = filepath
                                };

                                AttachmentsAdapter.Add(attach);
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                        }
                    }
                }
                else if (requestCode == 501 && resultCode == Result.Ok) // => video Gallery
                {
                    AttachmentsAdapter.RemoveAll();

                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = AppTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                            return;
                        }


                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                if (bitmapImage != null)
                                    Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }
                              
                            var attach = new Attachments
                            {
                                Id = AttachmentsAdapter.AttachmentsList.Count + 1,
                                TypeAttachment = "video",
                                FileSimple = fullPathFile.AbsolutePath,
                                Thumb = new Attachments.VideoThumb()
                                {
                                    FileUrl = fullPathFile.AbsolutePath
                                },
                                FileUrl = filepath
                            };

                            AttachmentsAdapter.Add(attach);
                        }
                    } 
                }
                else if (requestCode == 513 && resultCode == Result.Ok) // Add video Camera 
                {
                    AttachmentsAdapter.RemoveAll();

                    var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                    if (filepath != null)
                    {
                        var check = AppTools.CheckMimeTypesWithServer(filepath);
                        if (!check)
                        {
                            //this file not supported on the server , please select another file 
                            Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                            return;
                        }
                         
                        var type = Methods.AttachmentFiles.Check_FileExtension(filepath);
                        if (type == "Video")
                        {
                            var fileName = filepath.Split('/').Last();
                            var fileNameWithoutExtension = fileName.Split('.').First();
                            var pathWithoutFilename = Methods.Path.FolderDcimImage;
                            var fullPathFile = new File(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");

                            var videoPlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(pathWithoutFilename, fileNameWithoutExtension + ".png");
                            if (videoPlaceHolderImage == "File Dont Exists")
                            {
                                var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(this, data.Data.ToString());
                                Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, pathWithoutFilename);
                            }

                            var attach = new Attachments
                            {
                                Id = AttachmentsAdapter.AttachmentsList.Count + 1,
                                TypeAttachment = "video",
                                FileSimple = fullPathFile.AbsolutePath,
                                Thumb = new Attachments.VideoThumb()
                                {
                                    FileUrl = fullPathFile.AbsolutePath
                                },
                                FileUrl = filepath
                            };

                            AttachmentsAdapter.Add(attach);
                        }
                    }
                } 
                else if (requestCode == 503 && resultCode == Result.Ok) // => Camera
                {
                    try
                    {
                        //remove file the type
                        var videoAttach = AttachmentsAdapter.AttachmentsList.Where(a => !a.TypeAttachment.Contains("images")).ToList();
                        if (videoAttach.Count > 0)
                            foreach (var video in videoAttach)
                                AttachmentsAdapter.Remove(video);
                         
                        if (string.IsNullOrEmpty(IntentController.CurrentPhotoPath))
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                        }
                        else
                        {
                            if (Methods.MultiMedia.CheckFileIfExits(IntentController.CurrentPhotoPath) != "File Dont Exists")
                            {
                                var check = AppTools.CheckMimeTypesWithServer(IntentController.CurrentPhotoPath);
                                if (!check)
                                {
                                    //this file not supported on the server , please select another file 
                                    Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                                    return;
                                }
                                 
                                var attach = new Attachments
                                {
                                   Id = AttachmentsAdapter.AttachmentsList.Count + 1,
                                   TypeAttachment = "images[]", 
                                   FileSimple = IntentController.CurrentPhotoPath,
                                   FileUrl = IntentController.CurrentPhotoPath
                                };

                                AttachmentsAdapter.Add(attach);
                            }
                            else
                            {
                                Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                            }
                        } 
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Failed_to_load), ToastLength.Short).Show();
                    }
                }
                else if (requestCode == 3 && resultCode == Result.Ok) // => Mention
                {
                    try
                    {
                        var dataUser = MentionActivity.MAdapter.MentionList.Where(a => a.Selected).ToList();
                        if (dataUser.Count > 0)
                        {
                            var textSanitizer = new TextSanitizer(ExtendTextView, this);

                            foreach (var item in dataUser) MentionText += " @" + item.Username + " ,";

                            textSanitizer.Load(LoadPostStrings());
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                else if (requestCode == 5 && resultCode == Result.Ok) // => Gif
                {
                    // G_fixed_height_small_url, // UrlGif - view  
                    // G_fixed_height_small_mp4, //MediaGif - sent  

                    var url = data.GetStringExtra("MediaGif") ?? "Data not available";
                    Console.WriteLine(url);
                    var gifMp4 = data.GetStringExtra("UrlGif") ?? "Data not available";
                    if (gifMp4 != "Data not available" && !string.IsNullOrEmpty(gifMp4))
                    {
                        GifFile = gifMp4;

                        //remove file the type
                        AttachmentsAdapter.RemoveAll();

                        var attach = new Attachments
                        {
                            Id = AttachmentsAdapter.AttachmentsList.Count + 1,
                            TypeAttachment = "gif_url",
                            FileSimple = GifFile,
                            FileUrl = GifFile
                        };

                        AttachmentsAdapter.Add(attach);
                    }
                }
                else if (requestCode == 2000 && resultCode == Result.Ok) // => NiceArtEditor
                {
                    var imageId = data.GetStringExtra("ImageId") ?? "0";
                    var imagePath = data.GetStringExtra("ImagePath") ?? "Data not available";
                    if (imagePath != "Data not available" && !string.IsNullOrEmpty(imagePath))
                    {
                        try
                        {
                            var check = AppTools.CheckMimeTypesWithServer(imagePath);
                            if (!check)
                            {
                                //this file not supported on the server , please select another file 
                                Toast.MakeText(this, GetString(Resource.String.Lbl_ErrorFileNotSupported), ToastLength.Short).Show();
                                return;
                            }
                             
                            var change = AttachmentsAdapter.AttachmentsList.FirstOrDefault(q => q.Id == Convert.ToInt32(imageId));
                            if (change != null)
                            {
                                change.FileUrl = imagePath;
                                change.FileSimple = imagePath;

                                AttachmentsAdapter.NotifyItemChanged(AttachmentsAdapter.AttachmentsList.IndexOf(change));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }

                if (SlidingUpPanel != null && SlidingUpPanel.GetPanelState() != SlidingUpPanelLayout.PanelState.Collapsed)
                {
                    SlidingUpPanel.SetPanelState(SlidingUpPanelLayout.PanelState.Collapsed);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        //Permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                switch (requestCode)
                {
                    case 500 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //requestCode >> 500 => Image Gallery
                        new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.Lbl_SelectPictures));
                        break;
                    case 500:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        break;
                    case 501 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //requestCode >> 501 => VideoGallery
                        new IntentController(this).OpenIntentVideoGallery();
                        break;
                    case 501:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        break;
                    case 513 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //requestCode >> 513 => VideoCamera
                        new IntentController(this).OpenIntentVideoCamera();
                        break;
                    case 513:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        break;
                    case 503 when grantResults.Length > 0 && grantResults[0] == Permission.Granted:
                        //requestCode >> 503 => Camera
                        new IntentController(this).OpenIntentCamera();
                        break;
                    case 503:
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Event Back
        public override void OnBackPressed()
        {
            try
            {
                if (!string.IsNullOrEmpty(DataEditText.Text) || !string.IsNullOrEmpty(MentionText) ||
                    AttachmentsAdapter.AttachmentsList.Count > 0)
                {
                    TypeDialog = "PostBack";

                    var dialog = new MaterialDialog.Builder(this).Theme(AppSettings.SetTabDarkTheme ? AFollestad.MaterialDialogs.Theme.Dark : AFollestad.MaterialDialogs.Theme.Light);

                    dialog.Title(GetText(Resource.String.Lbl_Title_Back));
                    dialog.Content(GetText(Resource.String.Lbl_Content_Back));
                    dialog.PositiveText(GetText(Resource.String.Lbl_PositiveText_Back)).OnPositive(this);
                    dialog.NegativeText(GetText(Resource.String.Lbl_NegativeText_Back)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.Build().Show();
                }
                else
                {
                    var resultIntent = new Intent();
                    SetResult(Result.Canceled, resultIntent);
                    Finish();
                    base.OnBackPressed();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion
         
    }
}