using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.Support.V4.Content;
using AndroidHUD;
using Uri = Android.Net.Uri;

namespace PixelPhoto.Helpers.Utils
{
    public static class ShareFileImplementation 
    {
        public static Activity Activity { set; get; }

        /// <summary>
        /// Simply share a local file on compatible services
        /// </summary>
        /// <param name="localFilePath">path to local file</param>
        /// <param name="title">Title of popup on share (not included in message)</param>
        /// <returns>awaitable Task</returns>
        public static void ShareLocalFile(Uri localFilePath, string title = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(localFilePath.Path))
                {
                    Console.WriteLine("ShareFile: ShareLocalFile Warning: localFilePath null or empty");
                    return;
                }
                  
                var intent = new Intent();
                intent.SetFlags(ActivityFlags.ClearTop);
                intent.SetFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionSend);
                intent.SetType("*/*");
                intent.PutExtra(Intent.ExtraStream, localFilePath);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);

                var chooserIntent = Intent.CreateChooser(intent, title);
                chooserIntent.SetFlags(ActivityFlags.ClearTop);
                chooserIntent.SetFlags(ActivityFlags.NewTask);
                Activity.StartActivity(chooserIntent);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message))
                    Console.WriteLine("Exception in ShareFile: ShareLocalFile Exception: {0}", ex);
            }
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="title"></param>
        public static void ShareText(string text, string title = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("ShareFile: ShareLocalFile Warning: localFilePath null or empty");
                    return;
                }
                  
                var intent = new Intent();
                intent.SetFlags(ActivityFlags.ClearTop);
                intent.SetFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionSend);
                intent.SetType("text/plain");
                intent.PutExtra(Intent.ExtraText, text);

                var chooserIntent = Intent.CreateChooser(intent, title);
                chooserIntent.SetFlags(ActivityFlags.ClearTop);
                chooserIntent.SetFlags(ActivityFlags.NewTask);
                Activity.StartActivity(chooserIntent);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message))
                    Console.WriteLine("Exception in ShareFile: ShareLocalFile Exception: {0}", ex);
            }
        }

        /// <summary>
        /// Simply share a file from a remote resource on compatible services
        /// </summary>
        /// <param name="fileUri">uri to external file</param>
        /// <param name="fileName">name of the file</param>
        /// <param name="title">Title of popup on share (not included in message)</param>
        /// <returns>awaitable bool</returns>
        public static async Task ShareRemoteFile(string fileUri, string fileName, string title = "")
        {
            try
            {
                Download(fileUri, fileName, title);
                await Task.Delay(0);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(ex.Message))
                    Console.WriteLine("Exception in ShareFile: ShareRemoteFile Exception: {0}", ex.Message);
            }
        }

        public static void Download(string imageUrl, string fileName, string title = "")
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl) || Activity == null)
                    return;

                Uri photoUri;
                
                Activity.RunOnUiThread(() =>
                { 
                    var getImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                    if (getImage != "File Dont Exists")
                    {
                        Java.IO.File file2 = new Java.IO.File(getImage);
                        photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);
                        ShareLocalFile(photoUri, title);
                    }
                    else
                    { 
                        string filePath = Path.Combine(Methods.Path.FolderDcimImage);
                        string mediaFile = filePath + "/" + fileName;

                        if (!Directory.Exists(filePath))
                            Directory.CreateDirectory(filePath);

                        if (!File.Exists(mediaFile))
                        {
                            AndHUD.Shared.Show(Activity, Activity.GetText(Resource.String.Lbl_Loading));
                            WebClient webClient = new WebClient();
                            webClient.DownloadDataAsync(new System.Uri(imageUrl));
                            webClient.DownloadDataCompleted += (s, e) =>
                            {
                                try
                                {
                                    File.WriteAllBytes(mediaFile, e.Result);
                                      
                                    var getImagePath = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                                    if (getImagePath != "File Dont Exists")
                                    {
                                        Java.IO.File file2 = new Java.IO.File(getImagePath);

                                        photoUri = FileProvider.GetUriForFile(Activity, Activity.PackageName + ".fileprovider", file2);
                                        ShareLocalFile(photoUri, title);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception);
                                }

                                //var mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
                                //mediaScanIntent.SetData(Uri.FromFile(new Java.IO.File(mediaFile)));
                                //Activity.SendBroadcast(mediaScanIntent);

                                // Tell the media scanner about the new file so that it is
                                // immediately available to the user.
                                MediaScannerConnection.ScanFile(Application.Context, new string[] { mediaFile }, null, null);

                            };
                            AndHUD.Shared.Dismiss(Activity);
                        } 
                    }
                });
               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        } 
    }
}