using Newtonsoft.Json;

namespace PixelPhoto.OneSignal
{
    public class OneSignalObject
    {
        public class PostDataObject
        {

            [JsonProperty("youtube", NullValueHandling = NullValueHandling.Ignore)]
            public string Youtube { get; set; }

            [JsonProperty("link", NullValueHandling = NullValueHandling.Ignore)]
            public string Link { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("registered", NullValueHandling = NullValueHandling.Ignore)]
            public string Registered { get; set; }

            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty("dailymotion", NullValueHandling = NullValueHandling.Ignore)]
            public string Dailymotion { get; set; }

            [JsonProperty("is_saved", NullValueHandling = NullValueHandling.Ignore)]
            public bool IsSaved { get; set; }

            [JsonProperty("reported", NullValueHandling = NullValueHandling.Ignore)]
            public bool Reported { get; set; }

            [JsonProperty("views", NullValueHandling = NullValueHandling.Ignore)]
            public int Views { get; set; }

            [JsonProperty("is_liked", NullValueHandling = NullValueHandling.Ignore)]
            public bool IsLiked { get; set; }

            [JsonProperty("likes", NullValueHandling = NullValueHandling.Ignore)]
            public int Likes { get; set; }

            [JsonProperty("is_should_hide", NullValueHandling = NullValueHandling.Ignore)]
            public bool IsShouldHide { get; set; }

            [JsonProperty("is_owner", NullValueHandling = NullValueHandling.Ignore)]
            public bool IsOwner { get; set; }

            [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
            public string Avatar { get; set; }

            [JsonProperty("is_verified", NullValueHandling = NullValueHandling.Ignore)]
            public int IsVerified { get; set; }

            [JsonProperty("mp4", NullValueHandling = NullValueHandling.Ignore)]
            public string Mp4 { get; set; }

            [JsonProperty("time_text", NullValueHandling = NullValueHandling.Ignore)]
            public string TimeText { get; set; }

            [JsonProperty("playtube", NullValueHandling = NullValueHandling.Ignore)]
            public string Playtube { get; set; }

            [JsonProperty("post_id", NullValueHandling = NullValueHandling.Ignore)]
            public int PostId { get; set; }

            [JsonProperty("vimeo", NullValueHandling = NullValueHandling.Ignore)]
            public string Vimeo { get; set; }

            [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
            public int UserId { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("votes", NullValueHandling = NullValueHandling.Ignore)]
            public int Votes { get; set; }

            [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
            public string Time { get; set; }

            [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
            public string Username { get; set; }
        }

        public class NotificationInfoObject
        {

            [JsonProperty("sent_push", NullValueHandling = NullValueHandling.Ignore)]
            public int SentPush { get; set; }

            [JsonProperty("notifier_id", NullValueHandling = NullValueHandling.Ignore)]
            public int NotifierId { get; set; }

            [JsonProperty("type_text", NullValueHandling = NullValueHandling.Ignore)]
            public string TypeText { get; set; }

            [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
            public int Id { get; set; }

            [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
            public string Text { get; set; }

            [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
            public string Time { get; set; }

            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public string Type { get; set; }

            [JsonProperty("seen", NullValueHandling = NullValueHandling.Ignore)]
            public string Seen { get; set; }

            [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }

            [JsonProperty("recipient_id", NullValueHandling = NullValueHandling.Ignore)]
            public int RecipientId { get; set; }
        } 
    }
}