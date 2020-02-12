using SQLite;


namespace PixelPhoto.SQLite
{
    public class DataTables
    {
        public class LoginTb
        {
            [PrimaryKey, AutoIncrement] public int AutoId { get; set; }

            public string UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string AccessToken { get; set; }
            public string Cookie { get; set; }
            public string Email { get; set; }
            public string Status { get; set; }
            public string Lang { get; set; }
            public string DeviceId { get; set; }
        }

        public class SettingsTb 
        {
            [PrimaryKey, AutoIncrement] public int AutoId { get; set; }

            public string SiteUrl { get; set; }
            public string SiteName { get; set; }
            public string Theme { get; set; }
            public string Validation { get; set; }
            public string FfmpegSys { get; set; }
            public string FfmpegBinary { get; set; }
            public string MaxVideoDuration { get; set; }
            public string YtApi { get; set; }
            public string GiphyApi { get; set; }
            public string UploadImages { get; set; }
            public string UploadVideos { get; set; }
            public string ImportVideos { get; set; }
            public string ImportImages { get; set; }
            public string StorySystem { get; set; }
            public string SignupSystem { get; set; }
            public string DeleteAccount { get; set; }
            public string Recaptcha { get; set; }
            public string RecaptchaKey { get; set; }
            public string SiteDesc { get; set; }
            public string SiteEmail { get; set; }
            public string MetaKeywords { get; set; }
            public string Obscene { get; set; }
            public string MaxUpload { get; set; }
            public string CaptionLen { get; set; }
            public string CommentLen { get; set; }
            public string Language { get; set; }
            public string SmtpOrMail { get; set; }
            public string SmtpHost { get; set; }
            public string SmtpUsername { get; set; }
            public string SmtpPassword { get; set; }
            public string SmtpPort { get; set; }
            public string SmtpEncryption { get; set; }
            public string FbLogin { get; set; }
            public string TwLogin { get; set; }
            public string GlLogin { get; set; }
            public string FacebookAppId { get; set; }
            public string FacebookAppKey { get; set; }
            public string TwitterAppId { get; set; }
            public string TwitterAppKey { get; set; }
            public string GoogleAppId { get; set; }
            public string GoogleAppKey { get; set; }
            public string SiteDocs { get; set; }
            public string LastCreatedSitemap { get; set; }
            public string LastBackup { get; set; }
            public string StoryDuration { get; set; }
            public string LastCleanDb { get; set; }
            public string EmailValidation { get; set; }
            public string AmazoneS3 { get; set; }
            public string BucketName { get; set; }
            public string AmazoneS3Key { get; set; }
            public string AmazoneS3SKey { get; set; }
            public string Region { get; set; }
            public string Ad1 { get; set; }
            public string Ad2 { get; set; }
            public string Ad3 { get; set; }
            public string GoogleAnalytics { get; set; }
            public string FtpUpload { get; set; }
            public string FtpHost { get; set; }
            public string FtpUsername { get; set; }
            public string FtpPassword { get; set; }
            public string FtpPort { get; set; }
            public string FtpEndpoint { get; set; }
            public string AppApiId { get; set; }
            public string AppApiKey { get; set; }
            public string WowonderAppId { get; set; }
            public string WowonderAppKey { get; set; }
            public string WowonderDomainUri { get; set; }
            public string WowonderLogin { get; set; }
            public string LastRun { get; set; }
            public string ConfigRun { get; set; }
            public string WowonderDomainIcon { get; set; }
            public string ServerKey { get; set; }
            public string PlaytubeUrl { get; set; }
            public string RecaptchaSiteKey { get; set; }
            public string RecaptchaSecretKey { get; set; }
            public string Watermark { get; set; }
            public string WatermarkLink { get; set; }
            public string Mp4Links { get; set; }
            public string PlaytubeLinks { get; set; }
            public string FaceFilter { get; set; }
            public string Push { get; set; }
            public string PushId { get; set; }
            public string PushKey { get; set; }
            public string AffiliateSystem { get; set; }
            public string AffiliateType { get; set; }
            public string AmountRef { get; set; }
            public string AmountPercentRef { get; set; }
            public string Currency { get; set; }
            public string CreditCard { get; set; }
            public string StripeSecret { get; set; }
            public string StripeId { get; set; }
            public string PaypalMode { get; set; }
            public string PaypalId { get; set; }
            public string PaypalSecret { get; set; }
            public string ProPrice { get; set; }
            public string BankPayment { get; set; }
            public string BankTransferNote { get; set; }
            public string ProSystem { get; set; }
            public string BoostedPosts { get; set; }
            public string AdCPrice { get; set; }
            public string AdVPrice { get; set; }
            public string GoogleMap { get; set; }
            public string GoogleMapApi { get; set; }
            public string UserAds { get; set; }
            public string BusinessAccount { get; set; }
            public string WithdrawSystem { get; set; }
            public string RaiseMoney { get; set; }
            public string RaiseMoneyType { get; set; }
            public string Version { get; set; }
            public string BankDescription { get; set; }
            public string DonatePercentage { get; set; }
            public string LogoExtension { get; set; }
            public string FaviconExtension { get; set; }
            public string ClickableUrl { get; set; }
            public string BlogSystem { get; set; }
            public string ImageSellSystem { get; set; }
            public string MinImageHeight { get; set; }
            public string MinImageWidth { get; set; }
            public string WatermarkAnchor { get; set; }
            public string WatermarkOpacity { get; set; }
            public string Header { get; set; }
            public string Footer { get; set; }

        }
 
        public class MyProfileTb
        {
            [PrimaryKey, AutoIncrement]
            public int AutoIdMyProfileTb { get; set; }

            public string UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string IpAddress { get; set; }
            public string Fname { get; set; }
            public string Lname { get; set; }
            public string Gender { get; set; }
            public string Language { get; set; }
            public string Avatar { get; set; }
            public string Cover { get; set; }
            public string CountryId { get; set; }
            public string About { get; set; }
            public string Google { get; set; }
            public string Facebook { get; set; }
            public string Twitter { get; set; }
            public string Website { get; set; }
            public string Active { get; set; }
            public string Admin { get; set; }
            public string Verified { get; set; }
            public string LastSeen { get; set; }
            public string Registered { get; set; }
            public string IsPro { get; set; }
            public string Posts { get; set; }
            public string PPrivacy { get; set; }
            public string CPrivacy { get; set; }
            public string NOnLike { get; set; }
            public string NOnMention { get; set; }
            public string NOnComment { get; set; }
            public string NOnFollow { get; set; }
            public string NOnCommentLike { get; set; }
            public string NOnCommentReply { get; set; }
            public string StartupAvatar { get; set; }
            public string StartupInfo { get; set; }
            public string StartupFollow { get; set; }
            public string Src { get; set; }
            public string SearchEngines { get; set; }
            public string Mode { get; set; }
            public string DeviceId { get; set; }
            public string Balance { get; set; }
            public string Wallet { get; set; }
            public string Referrer { get; set; }
            public string Profile { get; set; }
            public string BusinessAccount { get; set; }
            public string PaypalEmail { get; set; }
            public string BName { get; set; }
            public string BEmail { get; set; }
            public string BPhone { get; set; }
            public string BSite { get; set; }
            public string BSiteAction { get; set; }
            public string Name { get; set; }
            public string Uname { get; set; }
            public string Url { get; set; }
            public string Followers { get; set; }
            public string Following { get; set; }
            public string Favourites { get; set; }
            public string PostsCount { get; set; }
            public string TimeText { get; set; }
            public string IsFollowing { get; set; }
            public string IsBlocked { get; set; }
        }

        public class LastChatTb
        {
            [PrimaryKey, AutoIncrement] public int AutoIdLastChat { get; set; }

            public int UserId { get; set; }
            public string Username { get; set; }
            public string Avatar { get; set; }
            public string Time { get; set; }
            public int Id { get; set; }
            public string LastMessage { get; set; }
            public int NewMessage { get; set; }
            public string TimeText { get; set; }
            public string UserDataJson { get; set; }
        }

        public class MessageTb
        {
            [PrimaryKey, AutoIncrement] public int AutoIdMessage { get; set; }

            public int Id { get; set; }
            public int FromId { get; set; }
            public int ToId { get; set; }
            public string Text { get; set; }
            public string MediaFile { get; set; }
            public string MediaType { get; set; }
            public string DeletedFs1 { get; set; }
            public string DeletedFs2 { get; set; }
            public string Seen { get; set; }
            public string Time { get; set; }
            public string Extra { get; set; }
            public string TimeText { get; set; }
            public string Position { get; set; }
        }
    }
}