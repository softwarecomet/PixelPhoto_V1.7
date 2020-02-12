using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AT.Markushi.UI;
using Developer.SEmojis.Actions;
using Developer.SEmojis.Helper;
using Newtonsoft.Json;
using PixelPhoto.Activities.Chat.Adapters;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.Classes.User;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using ActionMode = Android.Support.V7.View.ActionMode;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using UserDataObject = PixelPhotoClient.GlobalClass.UserDataObject;


namespace PixelPhoto.Activities.Chat
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MessagesBoxActivity : AppCompatActivity, IOnClickListenerSelectedMessages
    {
        #region Variables Basic

        private AppCompatImageView ChatEmojiImage;
        private RelativeLayout RootView;
        private EmojiconEditText EmojiconEditTextView;
        private CircleButton ChatSendButton;
        private static Toolbar TopChatToolBar;
        public RecyclerView ChatBoxRecylerView;
        private LinearLayoutManager MLayoutManager;
        public static UserMessagesAdapter MAdapter;
        private string LastSeenUser = "", TypeChat = "", TaskWork = "";
        private static int UnixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        private string Time = Convert.ToString(UnixTimestamp);
        private int BeforeMessageId, FirstMessageId;
        private static int Userid;// to_id
        private static Timer Timer;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private GetChatsObject.Data DataUser;
        private UserDataObject UserInfoData;
        private CommentObject UserInfoComment;
        private ActionModeCallback ModeCallback;
        private static ActionMode ActionMode;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                SetTheme(AppSettings.SetTabDarkTheme ? Resource.Style.MyTheme_Dark_Base : Resource.Style.MyTheme_Base);

                Window.SetSoftInputMode(SoftInput.AdjustResize);
                base.OnCreate(savedInstanceState);

                if (AppSettings.SetTabDarkTheme)
                    Window.SetBackgroundDrawableResource(Resource.Drawable.chatBackground3_Dark);
                else
                    Window.SetBackgroundDrawableResource(Resource.Drawable.chatBackground3);

                // Set our view from the "MessagesBox_Layout" layout resource
                SetContentView(Resource.Layout.MessagesBox_Layout);

                var data = Intent.GetStringExtra("UserId") ?? "Data not available";
                if (data != "Data not available" && !string.IsNullOrEmpty(data)) Userid = int.Parse(data); // to_id

                try
                {
                    var type = Intent.GetStringExtra("TypeChat") ?? "Data not available";
                    if (type != "Data not available" && !string.IsNullOrEmpty(type))
                    {
                        TypeChat = type;
                        string json = Intent.GetStringExtra("UserItem");
                        dynamic item;
                        switch (type)
                        {
                            case "LastChat":
                                item = JsonConvert.DeserializeObject<GetChatsObject.Data>(json);
                                if (item != null) DataUser = item;
                                break;
                            case "comment":
                                item = JsonConvert.DeserializeObject<CommentObject>(json);
                                if (item != null) UserInfoComment = item;
                                break;
                            case "following":
                            case "followers":
                            case "suggestion":
                            case "search":
                            case "Notification":
                            case "new":
                            case "NewsFeedPost":
                            case "OneSignalNotification":
                            case "UserData":
                                item = JsonConvert.DeserializeObject<UserDataObject>(json);
                                if (item != null) UserInfoData = item;
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                var emojisIcon = new EmojIconActions(this, RootView, EmojiconEditTextView, ChatEmojiImage);
                emojisIcon.ShowEmojIcon();

                //Set Title ToolBar and data chat user
                loadData_ItemUser();
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

                if (Timer != null)
                {
                    Timer.Enabled = true;
                    Timer.Start();
                }
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

                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Functions

        private void InitComponent()
        {
            try
            {
                RootView = FindViewById<RelativeLayout>(Resource.Id.rootChatWindowView);

                ChatEmojiImage = FindViewById<AppCompatImageView>(Resource.Id.emojiicon);
                EmojiconEditTextView = FindViewById<EmojiconEditText>(Resource.Id.EmojiconEditText5);
                ChatSendButton = FindViewById<CircleButton>(Resource.Id.sendButton);
                ChatBoxRecylerView = FindViewById<RecyclerView>(Resource.Id.recyler);
                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);

                ChatSendButton.Tag = "Text";
                ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);

                ModeCallback = new ActionModeCallback(this);

                EmojiconEditTextView.SetTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);
                EmojiconEditTextView.SetHintTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Gray);

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
                TopChatToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (TopChatToolBar != null)
                {
                    TopChatToolBar.SetTitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    TopChatToolBar.SetSubtitleTextColor(AppSettings.SetTabDarkTheme ? Color.White : Color.Black);
                    TopChatToolBar.SetBackgroundResource(AppSettings.SetTabDarkTheme ? Resource.Drawable.linear_gradient_drawable_Dark : Resource.Drawable.linear_gradient_drawable);

                    SetSupportActionBar(TopChatToolBar);
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
                ChatBoxRecylerView.SetItemAnimator(null);
                MAdapter = new UserMessagesAdapter(this);
                MLayoutManager = new LinearLayoutManager(this);
                ChatBoxRecylerView.SetLayoutManager(MLayoutManager);
                ChatBoxRecylerView.SetAdapter(MAdapter);
                MAdapter.SetOnClickListener(this);
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
                    ChatSendButton.Touch += Chat_sendButton_Touch;
                }
                else
                {
                    ChatSendButton.Touch -= Chat_sendButton_Touch;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Set ToolBar and data chat user

        //Set ToolBar and data chat user
        private void loadData_ItemUser()
        {
            try
            {
                if (DataUser != null)
                {
                    SupportActionBar.Title = DataUser.UserData.Name;
                    SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(DataUser.UserData.LastSeen));
                    LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(DataUser.UserData.LastSeen));
                }
                else if (UserInfoData != null)
                {
                    SupportActionBar.Title = UserInfoData.Name;
                    SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen));
                    LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen));
                }
                else if (UserInfoComment != null)
                {
                    SupportActionBar.Title = UserInfoComment.Name;
                    SupportActionBar.Subtitle = "";
                    LastSeenUser = "";
                }

                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetUserProfileApi });

                Get_Messages();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task GetUserProfileApi()
        {
            if (Methods.CheckConnectivity())
            {
                (int respondCode, var respondString) = await RequestsAsync.User.FetchUserData(Userid.ToString());
                if (respondCode == 200)
                {
                    if (respondString is FetchUserDataObject result)
                    {
                        if (result.Data != null)
                        {
                            UserInfoData = result.Data;

                            RunOnUiThread(() =>
                            {
                                SupportActionBar.Title = UserInfoData.Name;
                                SupportActionBar.Subtitle = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen));
                                LastSeenUser = GetString(Resource.String.Lbl_Last_seen) + " " + Methods.Time.TimeAgo(int.Parse(UserInfoData.LastSeen));
                            });
                        }
                    }
                }
                else Methods.DisplayReportResult(this, respondString);
            }
        }

        private UserDataObject ConvertData()
        {
            try
            {
                UserDataObject userData = null;
                if (DataUser != null)
                {
                    userData = new UserDataObject()
                    {
                        UserId = DataUser.UserData.UserId,
                        Username = DataUser.UserData.Username,
                        Email = DataUser.UserData.Email,
                        IpAddress = DataUser.UserData.IpAddress,
                        Fname = DataUser.UserData.Fname,
                        Lname = DataUser.UserData.Lname,
                        Gender = DataUser.UserData.Gender,
                        Language = DataUser.UserData.Language,
                        Avatar = DataUser.UserData.Avatar,
                        Cover = DataUser.UserData.Cover,
                        CountryId = DataUser.UserData.CountryId,
                        About = DataUser.UserData.About,
                        Google = DataUser.UserData.Google,
                        Facebook = DataUser.UserData.Facebook,
                        Twitter = DataUser.UserData.Twitter,
                        Website = DataUser.UserData.Website,
                        Active = DataUser.UserData.Active,
                        Admin = DataUser.UserData.Admin,
                        Verified = DataUser.UserData.Verified,
                        LastSeen = DataUser.UserData.LastSeen,
                        Registered = DataUser.UserData.Registered,
                        IsPro = DataUser.UserData.IsPro,
                        Posts = DataUser.UserData.Posts,
                        PPrivacy = DataUser.UserData.PPrivacy,
                        CPrivacy = DataUser.UserData.CPrivacy,
                        NOnLike = DataUser.UserData.NOnLike,
                        NOnMention = DataUser.UserData.NOnMention,
                        NOnComment = DataUser.UserData.NOnComment,
                        NOnFollow = DataUser.UserData.NOnFollow,
                        StartupAvatar = DataUser.UserData.StartupAvatar,
                        StartupInfo = DataUser.UserData.StartupInfo,
                        StartupFollow = DataUser.UserData.StartupFollow,
                        Src = DataUser.UserData.Src,
                        SearchEngines = DataUser.UserData.SearchEngines,
                        Mode = DataUser.UserData.Mode,
                        Name = DataUser.UserData.Name,
                        Uname = DataUser.UserData.Uname,
                        Url = DataUser.UserData.Url,
                        TimeText = DataUser.UserData.TimeText,
                        IsFollowing = DataUser.UserData.IsFollowing,
                        IsBlocked = DataUser.UserData.IsBlocked,
                    };
                }
                else if (UserInfoData != null)
                {
                    userData = UserInfoData;
                }

                return userData;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }


        #endregion

        #region Get Messages

        //Get Messages Local Or Api
        private void Get_Messages()
        {
            try
            {
                BeforeMessageId = 0;
                MAdapter.Clear();

                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.GetMessagesList(int.Parse(UserDetails.UserId), Userid, BeforeMessageId);
                if (localList == "1") //Database.. Get Messages Local
                {
                    MAdapter.BindEnd();

                    //Scroll Down >> 
                    ChatBoxRecylerView.ScrollToPosition(MAdapter.MessageList.Count - 1);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                }
                else //Or server.. Get Messages Api
                {
                    SwipeRefreshLayout.Refreshing = true;
                    SwipeRefreshLayout.Enabled = true;
                    GetMessages_API();
                }

                //Set Event Scroll
                XamarinRecyclerViewOnScrollListener onScrollListener = new XamarinRecyclerViewOnScrollListener(MLayoutManager, SwipeRefreshLayout);
                onScrollListener.LoadMoreEvent += Messages_OnScroll_OnLoadMoreEvent;
                ChatBoxRecylerView.AddOnScrollListener(onScrollListener);
                TaskWork = "Working";

                //Run timer
                Timer = new Timer { Interval = AppSettings.MessageRequestSpeed, Enabled = true };
                Timer.Elapsed += TimerOnElapsed_MessageUpdater;
                Timer.Start();

                dbDatabase.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Get Messages From API 
        private async void GetMessages_API()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                    ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    BeforeMessageId = 0;

                    var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid.ToString());
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserMessagesObject result)
                        {
                            if (result.data.Messages.Count > 0)
                            {
                                MAdapter.MessageList = new ObservableCollection<GetUserMessagesObject.Message>(result.data.Messages.OrderBy(a => a.Time));
                                MAdapter.BindEnd();

                                //Insert to DataBase
                                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);
                                dbDatabase.Dispose();

                                //Scroll Down >> 
                                ChatBoxRecylerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

                                SwipeRefreshLayout.Refreshing = false;
                                SwipeRefreshLayout.Enabled = false;
                            }
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);

                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
        }

        #endregion

        //Timer Message Updater >> Get New Message
        private void TimerOnElapsed_MessageUpdater(object sender, ElapsedEventArgs e)
        {
            try
            {
                //Code get last Message id where Updater >>
                MessageUpdater();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #region Updater

        private async void MessageUpdater()
        {
            try
            {
                if (TaskWork == "Working")
                {
                    TaskWork = "Stop";

                    if (!Methods.CheckConnectivity())
                    {
                        SwipeRefreshLayout.Refreshing = false;
                        ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }
                    else
                    {
                        int countList = MAdapter.MessageList.Count;
                        string afterId = MAdapter.MessageList.LastOrDefault()?.Id.ToString() ?? "";
                        var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid.ToString(), "30", "", afterId);
                        if (apiStatus == 200)
                        {
                            if (respond is GetUserMessagesObject result)
                            {
                                int responseList = result.data.Messages.Count;
                                if (responseList > 0)
                                {
                                    if (countList > 0)
                                    {
                                        foreach (var item in from item in result.data.Messages let check = MAdapter.MessageList.FirstOrDefault(a => a.Id == item.Id) where check == null select item)
                                        {
                                            MAdapter.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        MAdapter.MessageList = new ObservableCollection<GetUserMessagesObject.Message>(result.data.Messages);
                                    }

                                    RunOnUiThread(() =>
                                    {
                                        var lastCountItem = MAdapter.ItemCount;
                                        if (countList > 0)
                                            MAdapter.NotifyItemRangeInserted(lastCountItem, MAdapter.MessageList.Count - 1);
                                        else
                                            MAdapter.NotifyDataSetChanged();

                                        //Insert to DataBase
                                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                        dbDatabase.InsertOrReplaceMessages(MAdapter.MessageList);
                                        dbDatabase.Dispose();

                                        //Scroll Down >> 
                                        ChatBoxRecylerView.ScrollToPosition(MAdapter.MessageList.Count - 1);

                                        var lastMessage = MAdapter.MessageList.LastOrDefault();
                                        if (lastMessage != null)
                                        {
                                            var dataUser = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.UserId == lastMessage?.FromId);
                                            if (dataUser != null)
                                            {
                                                dataUser.LastMessage = lastMessage.Text;

                                                LastChatActivity.MAdapter.Move(dataUser);
                                                LastChatActivity.MAdapter.Update(dataUser);
                                            }
                                            if (AppSettings.RunSoundControl)
                                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_GetMesseges.mp3");
                                        }
                                    });
                                }
                            }
                        }
                        else Methods.DisplayReportResult(this, respond);
                    }
                    TaskWork = "Working";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void UpdateOneMessage(GetUserMessagesObject.Message message)
        {
            try
            {
                MAdapter.Update(message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Load More

        private async void LoadMoreMessages()
        {
            try
            {
                //Run Load More Api 
                var local = LoadMoreMessagesDatabase();
                if (local == "1")
                {

                }
                else
                {
                    var api = await LoadMoreMessagesApi();
                    if (api == "1")
                    {

                    }
                    else
                    {
                        SwipeRefreshLayout.Refreshing = false;
                        SwipeRefreshLayout.Enabled = false;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private string LoadMoreMessagesDatabase()
        {
            try
            {
                SqLiteDatabase dbDatabase = new SqLiteDatabase();
                var localList = dbDatabase.GetMessageList(Convert.ToInt32(UserDetails.UserId), Userid, FirstMessageId);
                if (localList?.Count > 0) //Database.. Get Messages Local
                {
                    localList = new List<DataTables.MessageTb>(localList.OrderByDescending(a => a.Id));

                    foreach (var m in localList.Select(messages => new GetUserMessagesObject.Message
                    {
                        Id = messages.Id,
                        FromId = messages.FromId,
                        ToId = messages.ToId,
                        Text = messages.Text,
                        MediaFile = messages.MediaFile,
                        MediaType = messages.MediaType,
                        DeletedFs1 = messages.DeletedFs1,
                        DeletedFs2 = messages.DeletedFs2,
                        Seen = messages.Seen,
                        Time = messages.Time,
                        Extra = messages.Extra,
                        TimeText = messages.TimeText,
                        Position = messages.Position,
                    }))
                    {
                        MAdapter.Insert(m, FirstMessageId);
                    }

                    dbDatabase.Dispose();
                    return "1";
                }
                else
                {
                    dbDatabase.Dispose();
                    return "0";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "0";
            }
        }

        private async Task<string> LoadMoreMessagesApi()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    SwipeRefreshLayout.Refreshing = false;
                    ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {
                    var (apiStatus, respond) = await RequestsAsync.Messages.GetUserMessages(Userid.ToString(), "30", FirstMessageId.ToString()).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserMessagesObject result)
                        {
                            if (result.data.Messages.Count > 0)
                            {
                                var listApi = new ObservableCollection<GetUserMessagesObject.Message>();

                                foreach (var messages in result.data.Messages.OrderBy(a => a.Time))
                                {
                                    GetUserMessagesObject.Message message = new GetUserMessagesObject.Message
                                    {
                                        Id = messages.Id,
                                        FromId = messages.FromId,
                                        ToId = messages.ToId,
                                        Text = messages.Text,
                                        MediaFile = messages.MediaFile,
                                        MediaType = messages.MediaType,
                                        DeletedFs1 = messages.DeletedFs1,
                                        DeletedFs2 = messages.DeletedFs2,
                                        Seen = messages.Seen,
                                        Time = messages.Time,
                                        Extra = messages.Extra,
                                        TimeText = messages.TimeText,
                                        Position = messages.Position,
                                    };

                                    MAdapter.Insert(message, FirstMessageId);

                                    listApi.Insert(0, message);

                                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                    // Insert data user in database
                                    dbDatabase.InsertOrReplaceMessages(listApi);
                                    dbDatabase.Dispose();
                                }
                                return "1";
                            }
                            return "0";
                        }
                    }
                    else Methods.DisplayReportResult(this, respond);
                }
                return "0";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return "0";
            }
        }

        #endregion

        #region Events

        //Send Message type => "right_text"
        private void OnClick_OfSendButton()
        {
            try
            {
                UnixTimestamp = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                var time2 = UnixTimestamp.ToString();
                string timeNow = DateTime.Now.ToString("hh:mm");

                if (string.IsNullOrEmpty(EmojiconEditTextView.Text))
                {

                }
                else
                {
                    //Here on This function will send Text Messages to the user 

                    //remove \n in a string
                    string replacement = Regex.Replace(EmojiconEditTextView.Text, @"\t|\n|\r", "");

                    if (Methods.CheckConnectivity())
                    {
                        GetUserMessagesObject.Message message = new GetUserMessagesObject.Message
                        {
                            Id = UnixTimestamp,
                            FromId = int.Parse(UserDetails.UserId),
                            ToId = Userid,
                            Text = replacement,
                            MediaFile = "",
                            MediaType = "",
                            DeletedFs1 = "",
                            DeletedFs2 = "",
                            Seen = "0",
                            Time = time2,
                            Extra = "",
                            TimeText = timeNow,
                            Position = "Right",
                        };

                        MAdapter.Add(message);

                        UserDataObject userData = ConvertData();
                        MessageController.SendMessageTask(Userid, EmojiconEditTextView.Text, time2, userData).ConfigureAwait(false);
                    }
                    else
                    {
                        ToastUtils.ShowToast(GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    }

                    EmojiconEditTextView.Text = "";
                }

                ChatSendButton.Tag = "Text";
                ChatSendButton.SetImageResource(Resource.Drawable.SendLetter);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Event click send messages type text
        private void Chat_sendButton_Touch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e.Event.Action == MotionEventActions.Down)
                {
                    OnClick_OfSendButton();
                }
                e.Handled = false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        #region Scroll

        //Event Scroll #Messages
        private void Messages_OnScroll_OnLoadMoreEvent(object sender, EventArgs eventArgs)
        {
            try
            {
                //Start Loader Get from Database or API Request >>
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;

                FirstMessageId = 0;

                //Code get first Message id where LoadMore >>
                var mes = MAdapter.MessageList.FirstOrDefault();
                if (mes != null)
                {
                    FirstMessageId = mes.Id;
                }

                if (FirstMessageId > 0)
                {
                    LoadMoreMessages();
                }

                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        private class XamarinRecyclerViewOnScrollListener : RecyclerView.OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            private readonly LinearLayoutManager LayoutManager;
            private readonly SwipeRefreshLayout SwipeRefreshLayout;

            public XamarinRecyclerViewOnScrollListener(LinearLayoutManager layoutManager, SwipeRefreshLayout swipeRefreshLayout)
            {
                LayoutManager = layoutManager;
                SwipeRefreshLayout = swipeRefreshLayout;
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    var pastVisibleItems = LayoutManager.FindFirstVisibleItemPosition();
                    if (pastVisibleItems == 0 && visibleItemCount != totalItemCount)
                    {
                        //Load More  from API Request
                        if (LoadMoreEvent != null) LoadMoreEvent(this, null);
                        //Start Load More messages From Database
                    }
                    else
                    {
                        if (SwipeRefreshLayout.Refreshing)
                        {
                            SwipeRefreshLayout.Refreshing = false;
                            SwipeRefreshLayout.Enabled = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        #endregion

        #region Menu

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.MessagesBox_Menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
                case Resource.Id.menu_block:
                    OnMenuBlockClick();
                    break;
                case Resource.Id.menu_clear_chat:
                    OnMenuClearChatClick();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        //Block User
        private async void OnMenuBlockClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var (apiStatus, respond) = await RequestsAsync.User.BlockUnblock(Userid.ToString()).ConfigureAwait(false);
                    Toast.MakeText(this, GetText(Resource.String.Lbl_Blocked_successfully), ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async void OnMenuClearChatClick()
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    MAdapter.Clear();

                    var userDelete = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.UserId == Userid);
                    if (userDelete != null)
                    {
                        LastChatActivity.MAdapter.Remove(userDelete);
                    }

                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                    dbDatabase.DeleteAllMessagesUser(UserDetails.UserId, Userid.ToString());
                    dbDatabase.Dispose();

                    var (apiStatus, respond) = await RequestsAsync.Messages.ClearMessages(Userid.ToString()).ConfigureAwait(false);
                }
                else
                {
                    Toast.MakeText(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Toolbar & Selected

        private class ActionModeCallback : Java.Lang.Object, ActionMode.ICallback
        {
            private readonly MessagesBoxActivity Activity;
            public ActionModeCallback(MessagesBoxActivity activity)
            {
                Activity = activity;
            }

            public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
            {
                int id = item.ItemId;
                if (id == Resource.Id.action_delete)
                {
                    DeleteItems();
                    mode.Finish();
                    return true;
                }
                else if (id == Resource.Id.action_copy)
                {
                    CopyItems();
                    mode.Finish();
                    return true;
                }
                else if (id == Android.Resource.Id.Home)
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    MAdapter.ClearSelections();

                    TopChatToolBar.Visibility = ViewStates.Visible;
                    ActionMode.Finish();

                    return true;
                }
                return false;
            }

            public bool OnCreateActionMode(ActionMode mode, IMenu menu)
            {
                SetSystemBarColor(Activity, AppSettings.MainColor);
                mode.MenuInflater.Inflate(Resource.Menu.menuChat, menu);
                return true;
            }

            public void SetSystemBarColor(Activity act, string color)
            {
                try
                {
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                    {
                        Window window = act.Window;
                        window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
                        window.ClearFlags(WindowManagerFlags.TranslucentStatus);
                        window.SetStatusBarColor(Color.ParseColor(color));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public void OnDestroyActionMode(ActionMode mode)
            {
                try
                {
                    MAdapter.ClearSelections();
                    ActionMode = null;
                    SetSystemBarColor(Activity, AppSettings.MainColor);

                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    TopChatToolBar.Visibility = ViewStates.Visible;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
            {
                return false;
            }

            //Delete Messages 
            private void DeleteItems()
            {
                try
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    if (TopChatToolBar.Visibility != ViewStates.Visible)
                        TopChatToolBar.Visibility = ViewStates.Visible;

                    if (Methods.CheckConnectivity())
                    {
                        List<int> selectedItemPositions = MAdapter.GetSelectedItems();
                        List<int> selectedItemId = new List<int>();
                        for (int i = selectedItemPositions.Count - 1; i >= 0; i--)
                        {
                            var datItem = MAdapter.GetItem(selectedItemPositions[i]);
                            if (datItem != null)
                            {
                                selectedItemId.Add(datItem.Id);
                                MAdapter.RemoveData(selectedItemPositions[i], datItem);
                            }
                        }

                        //Send Api Delete By id
                        RequestsAsync.Messages.DeleteMessages(Userid.ToString(), selectedItemId).ConfigureAwait(false);

                        MAdapter.NotifyDataSetChanged();
                    }
                    else
                    {
                        Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            //Copy Messages
            private void CopyItems()
            {
                try
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    if (TopChatToolBar.Visibility != ViewStates.Visible)
                        TopChatToolBar.Visibility = ViewStates.Visible;

                    string allText = "";
                    List<int> selectedItemPositions = MAdapter.GetSelectedItems();
                    for (int i = selectedItemPositions.Count - 1; i >= 0; i--)
                    {
                        var datItem = MAdapter.GetItem(selectedItemPositions[i]);
                        if (datItem != null)
                        {
                            allText = allText + " \n" + datItem.Text;
                        }
                    }

                    ClipboardManager clipboard = (ClipboardManager)Activity.GetSystemService(ClipboardService);
                    ClipData clip = ClipData.NewPlainText("clipboard", allText);
                    clipboard.PrimaryClip = clip;

                    MAdapter.NotifyDataSetChanged();

                    Toast.MakeText(Activity, Activity.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short).Show();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public void OnItemClick(View view, GetUserMessagesObject.Message obj, int pos)
        {
            try
            {
                if (MAdapter.GetSelectedItemCount() > 0) // Add Select New Item 
                {
                    EnableActionMode(pos);
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    if (TopChatToolBar.Visibility != ViewStates.Visible)
                        TopChatToolBar.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnItemLongClick(View view, GetUserMessagesObject.Message obj, int pos)
        {
            EnableActionMode(pos);
        }

        private void EnableActionMode(int position)
        {
            if (ActionMode == null)
            {
                ActionMode = StartSupportActionMode(ModeCallback);
            }
            ToggleSelection(position);
        }

        private void ToggleSelection(int position)
        {
            try
            {
                MAdapter.ToggleSelection(position);
                int count = MAdapter.GetSelectedItemCount();

                if (count == 0)
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = true;
                        Timer.Start();
                    }

                    TopChatToolBar.Visibility = ViewStates.Visible;
                    ActionMode.Finish();
                }
                else
                {
                    if (Timer != null)
                    {
                        Timer.Enabled = false;
                        Timer.Stop();
                    }

                    TopChatToolBar.Visibility = ViewStates.Gone;
                    ActionMode.SetTitle(count);
                    ActionMode.Invalidate();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

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
                if (Timer != null)
                {
                    Timer.Enabled = false;
                    Timer.Stop();
                }




                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}