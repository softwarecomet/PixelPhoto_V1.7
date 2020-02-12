using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Media;
using Android.OS;
using Newtonsoft.Json;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Activities.UserProfile;
using PixelPhoto.Helpers.Model;
using PixelPhotoClient;
using PixelPhotoClient.GlobalClass;

namespace PixelPhoto.Helpers.Utils
{
    public enum ProfileTheme
    {
        DefaultTheme,
        TikTheme,
    }
     
    public static class AppTools
    {
        public static string GetNameFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser.Name) && !string.IsNullOrWhiteSpace(dataUser.Name))
                    return Methods.FunString.DecodeString(dataUser.Name);

                if (!string.IsNullOrEmpty(dataUser.Username) && !string.IsNullOrWhiteSpace(dataUser.Username))
                    return Methods.FunString.DecodeString(dataUser.Username);

                return UserDetails.Username;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return UserDetails.Username;
            }
        }

        public static string GetAboutFinal(UserDataObject dataUser)
        {
            try
            {
                if (!string.IsNullOrEmpty(dataUser.About) && !string.IsNullOrWhiteSpace(dataUser.About))
                    return Methods.FunString.DecodeString(dataUser.About);

                return Application.Context.Resources.GetString(Resource.String.Lbl_DefaultAbout) + " " + AppSettings.ApplicationName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Application.Context.Resources.GetString(Resource.String.Lbl_DefaultAbout) + " " + AppSettings.ApplicationName;
            }
        }
         
        public static void OpenProfile(Activity activity, string userId, UserDataObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("userinfo", JsonConvert.SerializeObject(item));
                    bundle.PutString("type", "UserData");
                    bundle.PutString("userid", userId);
                    bundle.PutString("avatar", item.Avatar);
                    bundle.PutString("fullname", item.Username);
                    if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                    {
                        UserProfileFragment profileFragment = new UserProfileFragment
                        {
                            Arguments = bundle
                        };

                        HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                    }
                    else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                    {
                        TikUserProfileFragment profileFragment = new TikUserProfileFragment
                        {
                            Arguments = bundle
                        };
                        HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                    } 
                }
                else
                {
                    HomeActivity.GetInstance()?.NavigationTabBar.SetModelIndex(4, true);
                }  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void OpenProfile(Activity activity, string userId, CommentObject item)
        {
            try
            {
                if (userId != UserDetails.UserId)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("userinfo", JsonConvert.SerializeObject(item));
                    bundle.PutString("type", "comment");
                    bundle.PutString("userid", userId);
                    bundle.PutString("avatar", item.Avatar);
                    bundle.PutString("fullname", item.Username);
                    if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                    {
                        UserProfileFragment profileFragment = new UserProfileFragment
                        {
                            Arguments = bundle
                        };

                        HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                    }
                    else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                    {
                        TikUserProfileFragment profileFragment = new TikUserProfileFragment
                        {
                            Arguments = bundle
                        };
                        HomeActivity.GetInstance()?.OpenFragment(profileFragment);
                    } 
                }
                else
                {
                    HomeActivity.GetInstance()?.NavigationTabBar.SetModelIndex(4, true);
                }  
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static bool GetStatusOnline(int lastSeen, string isShowOnline)
        {
            try
            {
                string time = Methods.Time.TimeAgo(lastSeen);
                bool status = isShowOnline == "on" && time == Methods.Time.LblJustNow ? true : false;
                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool CheckAllowedUploadInServer(string type)
        {
            try
            {
                var settings = ListUtils.SettingsSiteList;
                if (settings == null || string.IsNullOrEmpty(settings.UploadImages) || string.IsNullOrEmpty(settings.UploadVideos))
                    return false;
                 
                switch (type)
                {
                    case "Image":
                    {
                        var check = settings.UploadImages;
                        if (check == "on")
                        {
                            // Allowed
                            return true;
                        }
                        break;
                    }
                    case "Video":
                    {
                        var check = settings.UploadVideos;
                        if (check == "on")
                        {
                            // Allowed
                            return true;
                        }
                        break;
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool CheckAllowedFileSharingInServer(string path)
        {
            try
            {
                var settings = ListUtils.SettingsSiteList;
                if (settings == null || string.IsNullOrEmpty(settings.UploadImages) || string.IsNullOrEmpty(settings.UploadVideos))
                    return false;

                if (!string.IsNullOrEmpty(path))
                {
                    var type = Methods.AttachmentFiles.Check_FileExtension(path);
                    var check = CheckAllowedUploadInServer(type);
                    if (check)
                    {
                        // Allowed
                        return true;
                    } 
                } 
                
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public static bool CheckMimeTypesWithServer(string path)
        {
            try
            {
                var allowedExtenstionStatic = "jpg,png,jpeg,gif,mp4,webm";
                var mimeTypes = "video/mp4,video/mov,video/mpeg,video/flv,video/avi,video/webm,audio/wav,audio/mpeg,video/quicktime,audio/mp3,image/png,image/jpeg,image/gif,application/pdf,application/msword,application/zip,application/x-rar-compressed,text/pdf,application/x-pointplus,text/css,text/plain,application/x-zip-compressed"; //video/mp4,video/mov,video/mpeg,video/flv,video/avi,video/webm,audio/wav,audio/mpeg,video/quicktime,audio/mp3,image/png,image/jpeg,image/gif,application/pdf,application/msword,application/zip,application/x-rar-compressed,text/pdf,application/x-pointplus,text/css

                var fileName = path.Split('/').Last();
                var fileNameWithExtension = fileName.Split('.').Last();

                var getMimeType = MimeTypeMap.GetMimeType(fileNameWithExtension);

                if (allowedExtenstionStatic.Contains(fileNameWithExtension) && mimeTypes.Contains(getMimeType))
                {
                    var check = CheckAllowedFileSharingInServer(path);
                    if (check)  // Allowed
                        return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
         
        // Functions Save Images
        private static void SaveFile(string id, string folder, string fileName, string url)
        {
            try
            {
                if (url.Contains("http"))
                {
                    string folderDestination = folder + id + "/";

                    string filePath = Path.Combine(folderDestination);
                    string mediaFile = filePath + "/" + fileName;

                    var fileNameWithoutExtension = fileName.Split('.').First();

                    if (!File.Exists(mediaFile))
                    {
                        if (!File.Exists(mediaFile))
                        {
                            WebClient webClient = new WebClient();

                            webClient.DownloadDataAsync(new Uri(url));
                            webClient.DownloadDataCompleted += (s, e) =>
                            {
                                try
                                {
                                    File.WriteAllBytes(mediaFile, e.Result);
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Functions file from folder
        public static string GetFile(string id, string folder, string filename, string url)
        {
            try
            {
                string folderDestination = folder + id + "/";

                if (!Directory.Exists(folderDestination))
                {
                    Directory.Delete(folder, true); 
                    Directory.CreateDirectory(folderDestination);
                }
                  
                string imageFile = Methods.MultiMedia.GetMediaFrom_Gallery(folderDestination, filename);
                if (imageFile == "File Dont Exists")
                {
                    //This code runs on a new thread, control is returned to the caller on the UI thread.
                    Task.Run(() => { SaveFile(id, folder, filename, url); });
                    return url;
                }
                else
                {
                    return imageFile;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return url;
            }
        }

        public static string GetDuration(string mediaFile)
        {
            try
            {
                string duration;
                MediaMetadataRetriever retriever;
                if (mediaFile.Contains("http"))
                {
                    retriever = new MediaMetadataRetriever();
                    if ((int)Build.VERSION.SdkInt >= 14)
                        retriever.SetDataSource(mediaFile, new Dictionary<string, string>());
                    else
                        retriever.SetDataSource(mediaFile);

                    duration = retriever.ExtractMetadata(MetadataKey.Duration); //time In Millisec 
                    retriever.Release();
                }
                else
                { 
                    var file = Android.Net.Uri.FromFile(new Java.IO.File(mediaFile));
                    retriever = new MediaMetadataRetriever();
                    //if ((int)Build.VERSION.SdkInt >= 14)
                    //    retriever.SetDataSource(file.Path, new Dictionary<string, string>());
                    //else
                    retriever.SetDataSource(file.Path);

                    duration = retriever.ExtractMetadata(MetadataKey.Duration); //time In Millisec 
                    retriever.Release();
                }

                return duration;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "0";
            }
        }

    }
}