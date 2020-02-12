using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PixelPhoto.Properties;

namespace PixelPhoto.Helpers.SocialLogins
{
    public class GoogleApi : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private GoogleProfile  Profile;
        private readonly GoogleServices GoogleServices;

        public GoogleProfile GoogleProfile
        {
            set
            {
                Profile = value;
                OnPropertyChanged();
            }
        }

        public GoogleApi()
        {
            try
            {
                GoogleServices = new GoogleServices();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task<string> GetAccessTokenAsync(string code)
        {
            try
            {
                var dataGoogle = await GoogleServices.GetAccessTokenAsync(code); 
                return dataGoogle.AccessToken;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task SetGoogleUserProfileAsync(string accessToken)
        {
            try
            {
                GoogleProfile = await GoogleServices.GetGoogleUserProfileAsync(accessToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}