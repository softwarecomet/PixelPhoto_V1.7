using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Org.Json;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.SocialLogins;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient;
using PixelPhotoClient.Classes.Auth;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.RestCalls;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Facebook.Login.Widget;
using GoogleApi = PixelPhoto.Helpers.SocialLogins.GoogleApi;
using Object = Java.Lang.Object;
using Toolbar = Android.Support.V7.Widget.Toolbar;


namespace PixelPhoto.Activities.Default
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class LoginActivity : AppCompatActivity, IFacebookCallback, GraphRequest.IGraphJSONObjectCallback,
        GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener, IResultCallback
    {
        #region Variables Basic

        private ImageView LogoImage;
        private TextView ForgetPasswordTextView;
        private EditText EmailEditText , PasswordEditText;
        private Button LoginButton;
        private ProgressBar ProgressBar;
        private ImageView BackgroundImage;
        private Toolbar Toolbar;
        private LoginButton FbLoginButton;
        private SignInButton GoogleSignInButton;
        private LinearLayout RegisterLayout;
         
        private ICallbackManager MFbCallManager;
        private FbMyProfileTracker MprofileTracker;
        public static GoogleApiClient MGoogleApiClient;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                //Set Full screen 
                View mContentView = Window.DecorView;
                var uiOptions = (int)mContentView.SystemUiVisibility;
                var newUiOptions = uiOptions;

                newUiOptions |= (int)SystemUiFlags.Fullscreen;
                newUiOptions |= (int)SystemUiFlags.LayoutStable;
                mContentView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;

               // Window.AddFlags(WindowManagerFlags.Fullscreen);

                // Create your application here
                SetContentView(Resource.Layout.Login_Layout);

                Client client = new Client(AppSettings.TripleDesAppServiceProvider);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                InitSocialLogins();

                //Check and Get Settings
                GetSettingsSite();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
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

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                if (AppSettings.ShowGoogleLogin && MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
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
                BackgroundImage = FindViewById<ImageView>(Resource.Id.backgroundimage);
                LogoImage = FindViewById<ImageView>(Resource.Id.logoimage);
                ForgetPasswordTextView = FindViewById<TextView>(Resource.Id.txt_forgot_pass);
                EmailEditText = FindViewById<EditText>(Resource.Id.edt_email);
                PasswordEditText = FindViewById<EditText>(Resource.Id.edt_password);
                LoginButton = FindViewById<Button>(Resource.Id.SignInButton);
                ProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
                FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                RegisterLayout = FindViewById<LinearLayout>(Resource.Id.tvRegister);
                  
                if (AppSettings.DisplayImageOnLoginBackground)
                    GlideImageLoader.LoadImage(this,AppSettings.URlImageOnLoginBackground, BackgroundImage, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                ProgressBar.Visibility = ViewStates.Invisible;
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
                    Toolbar.Title = GetString(Resource.String.Lbl_SignIn);
                                        
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

        private void InitSocialLogins()
        {
            try
            {
                //#Facebook
                if (AppSettings.ShowFacebookLogin)
                {
                    //FacebookSdk.SdkInitialize(this);

                    MprofileTracker = new FbMyProfileTracker();
                    MprofileTracker.MOnProfileChanged += MprofileTrackerOnM_OnProfileChanged;
                    MprofileTracker.StartTracking();

                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Visible;
                    FbLoginButton.SetPermissions(new List<string>
                    {
                        "email",
                        "public_profile"
                    });

                    MFbCallManager = CallbackManagerFactory.Create();
                    FbLoginButton.RegisterCallback(MFbCallManager, this);

                    //FB accessToken
                    var accessToken = AccessToken.CurrentAccessToken;
                    var isLoggedIn = accessToken != null && !accessToken.IsExpired;
                    if (isLoggedIn && Profile.CurrentProfile != null)
                    {
                        LoginManager.Instance.LogOut();
                    }

                    string hashId = Methods.App.GetKeyHashesConfigured(this);
                    Console.WriteLine(hashId);
                }
                else
                {
                    FbLoginButton = FindViewById<LoginButton>(Resource.Id.fblogin_button);
                    FbLoginButton.Visibility = ViewStates.Gone;
                }

                //#Google
                if (AppSettings.ShowGoogleLogin)
                {
                    // Configure sign-in to request the user's ID, email address, and basic profile. ID and basic profile are included in DEFAULT_SIGN_IN.
                    var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                        .RequestIdToken(AppSettings.ClientId)
                        .RequestScopes(new Scope(Scopes.Profile))
                        .RequestScopes(new Scope(Scopes.PlusMe))
                        .RequestScopes(new Scope(Scopes.DriveAppfolder))
                        .RequestServerAuthCode(AppSettings.ClientId)
                        .RequestProfile().RequestEmail().Build();

                    // Build a GoogleApiClient with access to the Google Sign-In API and the options specified by gso.
                    MGoogleApiClient = new GoogleApiClient.Builder(this, this, this)
                        .EnableAutoManage(this, this)
                        .AddApi(Auth.GOOGLE_SIGN_IN_API, gso) 
                        .Build();

                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Click += MGsignBtnOnClick; 
                }
                else
                {
                    GoogleSignInButton = FindViewById<SignInButton>(Resource.Id.Googlelogin_button);
                    GoogleSignInButton.Visibility = ViewStates.Gone;
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
                    ForgetPasswordTextView.Click += ForgetPasswordButton_Click;
                    LoginButton.Click += SignInButtonOnClick;
                    RegisterLayout.Click += RegisterLayoutOnClick;
                }
                else
                {
                    ForgetPasswordTextView.Click -= ForgetPasswordButton_Click;
                    LoginButton.Click -= SignInButtonOnClick;
                    RegisterLayout.Click -= RegisterLayoutOnClick;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void GetSettingsSite()
        {
            try
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetSettings_Api(this) });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        //Event Click open Register Activity
        private void RegisterLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(RegisterActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Event Click Login
        private async void SignInButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                { 
                    if (!string.IsNullOrEmpty(EmailEditText.Text.Replace(" ", "")) || !string.IsNullOrEmpty(PasswordEditText.Text))
                    {
                        ProgressBar.Visibility = ViewStates.Visible;
                        LoginButton.Visibility = ViewStates.Gone;

                        (int apiStatus, var respond) = await RequestsAsync.Auth.Login(EmailEditText.Text.Replace(" ",""), PasswordEditText.Text, UserDetails.DeviceId);
                        if (apiStatus == 200)
                        {
                            if (respond is LoginObject auth)
                            {
                                if (auth.data != null)
                                { 
                                    Current.AccessToken = auth.data.AccessToken;

                                    UserDetails.Username = EmailEditText.Text;
                                    UserDetails.FullName = EmailEditText.Text;
                                    UserDetails.Password = PasswordEditText.Text;
                                    UserDetails.AccessToken = auth.data.AccessToken;
                                    UserDetails.UserId = auth.data.UserId.ToString();
                                    UserDetails.Status = "Active";
                                    UserDetails.Cookie = auth.data.AccessToken;
                                    UserDetails.Email = EmailEditText.Text;

                                    //Insert user data to database
                                    var user = new DataTables.LoginTb
                                    {
                                        UserId = UserDetails.UserId,
                                        AccessToken = UserDetails.AccessToken,
                                        Cookie = UserDetails.Cookie,
                                        Username = EmailEditText.Text,
                                        Password = PasswordEditText.Text,
                                        Status = "Active",
                                        Lang = "",
                                        DeviceId = UserDetails.DeviceId,
                                    };
                                    ListUtils.DataUserLoginList.Add(user);

                                    var dbDatabase = new SqLiteDatabase();
                                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                    dbDatabase.Dispose();
                                     
                                   PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => ApiRequest.GetProfile_Api(this) });

                                    StartActivity(new Intent(this, typeof(HomeActivity)));

                                    FinishAffinity();
                                }
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                var errorText = error.errors.ErrorText;
                                var errorId = error.errors.ErrorId;
                                if (errorId == "2")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_2), GetText(Resource.String.Lbl_Ok));
                                else if (errorId == "4")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                                else if (errorId == "19")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                                else
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            LoginButton.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            LoginButton.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security),respond.toString(), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        LoginButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Please_enter_your_data), GetText(Resource.String.Lbl_Ok));
                    }
                }
                else
                {
                    ProgressBar.Visibility = ViewStates.Gone;
                    LoginButton.Visibility = ViewStates.Visible;
                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_CheckYourInternetConnection), GetText(Resource.String.Lbl_Ok));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                ProgressBar.Visibility = ViewStates.Gone;
                LoginButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
            }
        }

        //Event Click open ForgetPassword Activity
        private void ForgetPasswordButton_Click(object sender, EventArgs e)
        {
            try
            {
                StartActivity(new Intent(this, typeof(ForgetPasswordActivity)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion
         
        #region Social Logins

        public string FbFirstName, FbLastName, FbName, FbEmail, FbAccessToken, FbProfileId;
        public string GFirstName, GLastName, GProfileId;
        public string GAccountName, GAccountType, GDisplayName, GEmail, GImg, GIdtoken, GAccessToken, GServerCode;

        #region Facebook

        public void OnCancel()
        {
            try
            {
                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void OnError(FacebookException error)
        {
            try
            {

                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;

                // Handle exception
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), error.Message, GetText(Resource.String.Lbl_Ok));

                SetResult(Result.Canceled);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void OnSuccess(Object result)
        {
            try
            {
                var loginResult = result as LoginResult;
                var id = AccessToken.CurrentAccessToken.UserId;

                ProgressBar.Visibility = ViewStates.Visible;
                GoogleSignInButton.Visibility = ViewStates.Gone;

                SetResult(Result.Ok);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public async void OnCompleted(JSONObject json, GraphResponse response)
        {
            try
            {
                var data = json.ToString();
                var result = JsonConvert.DeserializeObject<FacebookResult>(data);
                FbEmail = result.Email;

                ProgressBar.Visibility = ViewStates.Visible;
                LoginButton.Visibility = ViewStates.Gone;

                var accessToken = AccessToken.CurrentAccessToken;
                if (accessToken != null)
                {
                    FbAccessToken = accessToken.Token;

                    //Login Api 
                    (int apiStatus, var respond) = await RequestsAsync.Auth.SocialLogin(FbAccessToken, "facebook");
                    if (apiStatus == 200)
                    {
                        if (respond is LoginObject auth)
                        {
                            if (auth.data != null)
                            {
                                Current.AccessToken = auth.data.AccessToken;

                                UserDetails.Username = EmailEditText.Text;
                                UserDetails.FullName = EmailEditText.Text;
                                UserDetails.Password = PasswordEditText.Text;
                                UserDetails.AccessToken = auth.data.AccessToken;
                                UserDetails.UserId = auth.data.UserId.ToString();
                                UserDetails.Status = "Active";
                                UserDetails.Cookie = auth.data.AccessToken;
                                UserDetails.Email = EmailEditText.Text;

                                //Insert user data to database
                                var user = new DataTables.LoginTb
                                {
                                    UserId = UserDetails.UserId,
                                    AccessToken = UserDetails.AccessToken,
                                    Cookie = UserDetails.Cookie,
                                    Username = EmailEditText.Text,
                                    Password = PasswordEditText.Text,
                                    Status = "Active",
                                    Lang = "",
                                    DeviceId = UserDetails.DeviceId,
                                };
                                ListUtils.DataUserLoginList.Add(user);

                                var dbDatabase = new SqLiteDatabase();
                                dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                dbDatabase.Dispose();

                                StartActivity(new Intent(this, typeof(HomeActivity)));
                                Finish();
                            }
                        }
                    }
                    else if (apiStatus == 400)
                    {
                        if (respond is ErrorObject error)
                        {
                            var errorText = error.errors.ErrorText;
                            var errorId = error.errors.ErrorId;
                            if (errorId == "2")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_2), GetText(Resource.String.Lbl_Ok));
                            else if (errorId == "4")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                            else if (errorId == "19")
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                            else
                                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errorText, GetText(Resource.String.Lbl_Ok));
                        }

                        ProgressBar.Visibility = ViewStates.Gone;
                        LoginButton.Visibility = ViewStates.Visible;
                    }
                    else if (apiStatus == 404)
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        LoginButton.Visibility = ViewStates.Visible;
                        Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Login), GetText(Resource.String.Lbl_Ok));
                    } 
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Console.WriteLine(exception);
            }
        }

        private void MprofileTrackerOnM_OnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            try
            {
                if (e.MProfile != null)
                    try
                    {
                        FbFirstName = e.MProfile.FirstName;
                        FbLastName = e.MProfile.LastName;
                        FbName = e.MProfile.Name;
                        FbProfileId = e.MProfile.Id;

                        var request = GraphRequest.NewMeRequest(AccessToken.CurrentAccessToken, this);
                        var parameters = new Bundle();
                        parameters.PutString("fields", "id,name,age_range,email");
                        request.Parameters = parameters;
                        request.ExecuteAsync();
                    }
                    catch (Java.Lang.Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                else
                    Toast.MakeText(this, GetString(Resource.String.Lbl_Null_Data_User), ToastLength.Short).Show();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion

        //======================================================

        #region Google

        //Event Click login using google
        private void MGsignBtnOnClick(object sender, EventArgs e)
        {
            try
            {
                MGoogleApiClient.Connect();
                var opr = Auth.GoogleSignInApi.SilentSignIn(MGoogleApiClient);
                if (opr.IsDone)
                {
                    // If the user's cached credentials are valid, the OptionalPendingResult will be "done"
                    // and the GoogleSignInResult will be available instantly.
                    Log.Debug("Login_Activity", "Got cached sign-in");
                    var result = opr.Get() as GoogleSignInResult;
                    HandleSignInResult(result);

                    //Auth.GoogleSignInApi.SignOut(mGoogleApiClient).SetResultCallback(this);
                }
                else
                {
                    // If the user has not previously signed in on this device or the sign-in has expired,
                    // this asynchronous branch will attempt to sign in the user silently.  Cross-device
                    // single sign-on will occur in this branch.
                    opr.SetResultCallback(new SignInResultCallback { Activity = this });
                }

                // Check if we're running on Android 5.0 or higher
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    if (!MGoogleApiClient.IsConnecting)
                        ResolveSignInError();
                    else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                }
                else
                {
                    if (CheckSelfPermission(Manifest.Permission.GetAccounts) == Permission.Granted && CheckSelfPermission(Manifest.Permission.UseCredentials) == Permission.Granted)
                    {
                        if (!MGoogleApiClient.IsConnecting)
                            ResolveSignInError();
                        else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                    }
                    else
                    {
                        new PermissionsController(this).RequestPermission(106); 
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

     
        public void HandleSignInResult(GoogleSignInResult result)
        {
            try
            {
                Log.Debug("Login_Activity", "handleSignInResult:" + result.IsSuccess);
                if (result.IsSuccess)
                {
                    // Signed in successfully, show authenticated UI.
                    var acct = result.SignInAccount;
                    SetContentGoogle(acct);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ResolveSignInError()
        {
            try
            {
                if (MGoogleApiClient.IsConnecting) return;

                var signInIntent = Auth.GoogleSignInApi.GetSignInIntent(MGoogleApiClient);
                StartActivityForResult(signInIntent, 0);
            }
            catch (IntentSender.SendIntentException io)
            {
                //The intent was cancelled before it was sent. Return to the default
                //state and attempt to connect to get an updated ConnectionResult
                Console.WriteLine(io);
                MGoogleApiClient.Connect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnConnected(Bundle connectionHint)
        {
            try
            { 
                var opr = Auth.GoogleSignInApi.SilentSignIn(MGoogleApiClient);
                if (opr.IsDone)
                {
                    // If the user's cached credentials are valid, the OptionalPendingResult will be "done"
                    // and the GoogleSignInResult will be available instantly.
                    Log.Debug("Login_Activity", "Got cached sign-in");
                    var result = opr.Get() as GoogleSignInResult;
                    HandleSignInResult(result);
                }
                else
                {
                    // If the user has not previously signed in on this device or the sign-in has expired,
                    // this asynchronous branch will attempt to sign in the user silently.  Cross-device
                    // single sign-on will occur in this branch.

                    opr.SetResultCallback(new SignInResultCallback { Activity = this });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async void SetContentGoogle(GoogleSignInAccount acct)
        {
            try
            {
                //Successful log in hooray!!
                if (acct != null)
                {
                    ProgressBar.Visibility = ViewStates.Visible;
                    LoginButton.Visibility = ViewStates.Gone;

                    //GAccountName = acct.Account.Name;
                    //GAccountType = acct.Account.Type;
                    //GDisplayName = acct.DisplayName;
                    //GFirstName = acct.GivenName;
                    //GLastName = acct.FamilyName;
                    //GProfileId = acct.Id;
                    //GEmail = acct.Email;
                    //GImg = acct.PhotoUrl.Path;
                    //GIdtoken = acct.IdToken;
                    GServerCode = acct.ServerAuthCode;

                    var api = new GoogleApi();
                    GAccessToken = await api.GetAccessTokenAsync(GServerCode);

                    if (!string.IsNullOrEmpty(GAccessToken))
                    {
                        //Login Api 
                        string key = Methods.App.GetValueFromManifest(this, "com.google.android.geo.API_KEY");
                        (int apiStatus, var respond) = await RequestsAsync.Auth.SocialLogin(GAccessToken, "google", key);
                        if (apiStatus == 200)
                        {
                            if (respond is LoginObject auth)
                            {
                                if (auth.data != null)
                                {
                                    Current.AccessToken = auth.data.AccessToken;

                                    UserDetails.Username = EmailEditText.Text;
                                    UserDetails.FullName = EmailEditText.Text;
                                    UserDetails.Password = PasswordEditText.Text;
                                    UserDetails.AccessToken = auth.data.AccessToken;
                                    UserDetails.UserId = auth.data.UserId.ToString();
                                    UserDetails.Status = "Active";
                                    UserDetails.Cookie = auth.data.AccessToken;
                                    UserDetails.Email = EmailEditText.Text;

                                    //Insert user data to database
                                    var user = new DataTables.LoginTb
                                    {
                                        UserId = UserDetails.UserId,
                                        AccessToken = UserDetails.AccessToken,
                                        Cookie = UserDetails.Cookie,
                                        Username = EmailEditText.Text,
                                        Password = PasswordEditText.Text,
                                        Status = "Active",
                                        Lang = "",
                                        DeviceId = UserDetails.DeviceId,
                                    };
                                    ListUtils.DataUserLoginList.Add(user);

                                    var dbDatabase = new SqLiteDatabase();
                                    dbDatabase.InsertOrUpdateLogin_Credentials(user);
                                    dbDatabase.Dispose();

                                    StartActivity(new Intent(this, typeof(HomeActivity)));
                                    Finish();
                                }
                            }
                        }
                        else if (apiStatus == 400)
                        {
                            if (respond is ErrorObject error)
                            {
                                var errortext = error.errors.ErrorText;
                                var errorid = error.errors.ErrorId;
                                if (errorid == "2")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_2), GetText(Resource.String.Lbl_Ok));
                                else if (errorid == "4")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_ErrorLogin_4), GetText(Resource.String.Lbl_Ok));
                                else if (errorid == "19")
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_19), GetText(Resource.String.Lbl_Ok));
                                else
                                    Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), errortext, GetText(Resource.String.Lbl_Ok));
                            }

                            ProgressBar.Visibility = ViewStates.Gone;
                            LoginButton.Visibility = ViewStates.Visible;
                        }
                        else if (apiStatus == 404)
                        {
                            ProgressBar.Visibility = ViewStates.Gone;
                            LoginButton.Visibility = ViewStates.Visible;
                            Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), GetText(Resource.String.Lbl_Error_Login), GetText(Resource.String.Lbl_Ok));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressBar.Visibility = ViewStates.Gone;
                GoogleSignInButton.Visibility = ViewStates.Visible;
                Methods.DialogPopup.InvokeAndShowDialog(this, GetText(Resource.String.Lbl_Security), exception.Message, GetText(Resource.String.Lbl_Ok));
                Console.WriteLine(exception);
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            try
            {
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            try
            {
                // An unresolvable error has occurred and Google APIs (including Sign-In) will not
                // be available.
                Log.Debug("Login_Activity", "onConnectionFailed:" + result);

                //The user has already clicked 'sign-in' so we attempt to resolve all
                //errors until the user is signed in, or the cancel
                ResolveSignInError();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnResult(Object result)
        {
            try
            {

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        //======================================================

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);

                Log.Debug("Login_Activity", "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);
                if (requestCode == 0)
                {
                    var result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                    HandleSignInResult(result);
                }
                else
                {
                    // Logins Facebook
                    MFbCallManager.OnActivityResult(requestCode, (int)resultCode, data);
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

                if (requestCode == 106)
                {
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        if (!MGoogleApiClient.IsConnecting)
                            ResolveSignInError();
                        else if (MGoogleApiClient.IsConnected) MGoogleApiClient.Disconnect();
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_Permission_is_denailed), ToastLength.Long).Show();
                    }
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