using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PixelPhoto.Helpers.SocialLogins
{
    /// <summary>
    /// Doc: https://developers.google.com/identity/protocols/OAuth2InstalledApp
    /// </summary>
    public class GoogleServices
    {
        /// <summary>
        /// Create a new app and get new credentials: 
        /// https://console.developers.google.com/apis/
        /// </summary>
        public static readonly string RedirectUri = "";

        public async Task<AccessTokenObject> GetAccessTokenAsync(string code)
        { 
            try
            {
                var requestUrl = "https://www.googleapis.com/oauth2/v4/token"
                                + "?code=" + code
                                + "&client_id=" + AppSettings.ClientId
                                + "&client_secret=" + AppSettings.ClientSecret
                                + "&redirect_uri=" + RedirectUri
                                + "&grant_type=authorization_code";

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(requestUrl, null);
                var json = await response.Content.ReadAsStringAsync();
                var accessToken = JsonConvert.DeserializeObject<AccessTokenObject>(json);
                return accessToken;

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
        
        public async Task<GoogleProfile> GetGoogleUserProfileAsync(string accessToken)
        {

            var requestUrl = "https://www.googleapis.com/plus/v1/people/me" + "?access_token=" + accessToken;
            var httpClient = new HttpClient();
            var userJson = await httpClient.GetStringAsync(requestUrl);
            var googleProfile = JsonConvert.DeserializeObject<GoogleProfile>(userJson);
            return googleProfile;
        }
    }
}