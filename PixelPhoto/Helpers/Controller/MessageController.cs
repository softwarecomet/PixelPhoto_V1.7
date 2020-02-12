using Android.Widget;
using System;
using System.Linq;
using System.Threading.Tasks;
using PixelPhoto.Activities.Chat;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;


namespace PixelPhoto.Helpers.Controller
{
    public class MessageController
    {
        //############# DON'T MODIFY HERE #############
        //========================= Functions =========================

        public static async Task SendMessageTask(int userId ,string text , string hashId , UserDataObject userData)
        {
            try
            {
                var (apiStatus, respond) = await RequestsAsync.Messages.SendMessage(userId.ToString(), text, hashId);
                if (apiStatus == 200)
                {
                    if (respond is SendMessageObject result)
                    {
                        if (result.data != null)
                        {
                            UpdateLastIdMessage(result.data , userData); 
                        }
                    } 
                }
                else if (apiStatus == 400)
                {
                    if (respond is ErrorObject error)
                    {
                        var errorText = error.errors.ErrorText;
                        ToastUtils.ShowToast(errorText, ToastLength.Short);
                    }
                }
                else if (apiStatus == 404)
                {
                    var error = respond.ToString();
                    ToastUtils.ShowToast(error, ToastLength.Short);
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void UpdateLastIdMessage(SendMessageObject.Data messages , UserDataObject userData)
        {
            try
            {
                var checker = MessagesBoxActivity.MAdapter.MessageList.FirstOrDefault(a => a.Id == Convert.ToInt32(messages.HashId));
                if (checker != null)
                {
                    checker.Id = messages.Id;
                    checker.FromId = messages.FromId;
                    checker.ToId = messages.ToId;
                    checker.Text = messages.Text;
                    checker.MediaFile = messages.MediaFile;
                    checker.MediaType = messages.MediaType;
                    checker.DeletedFs1 = messages.DeletedFs1;
                    checker.DeletedFs2 = messages.DeletedFs2;
                    checker.Seen = messages.Seen;
                    checker.Time = messages.Time;
                    checker.Extra = messages.Extra;
                    checker.TimeText = messages.TimeText;
                    checker.Position = "Right";
                     
                    var dataUser = LastChatActivity.MAdapter.UserList?.FirstOrDefault(a => a.UserId == messages.ToId);
                    if (dataUser != null)
                    { 
                        dataUser.LastMessage = messages.Text;

                        var index = LastChatActivity.MAdapter.UserList?.IndexOf(LastChatActivity.MAdapter.UserList?.FirstOrDefault(x => x.UserId == messages.ToId));
                        if (index > -1)
                        {
                            LastChatActivity.MAdapter.Move(dataUser);
                            LastChatActivity.MAdapter.Update(dataUser);
                        } 
                    }
                    else
                    {
                        if (userData != null)
                        {
                            LastChatActivity.MAdapter.Insert(new GetChatsObject.Data()
                            {
                                UserId = checker.ToId,
                                Username = userData.Username,
                                Avatar = userData.Avatar,
                                Time = userData.LastSeen,
                                LastMessage = messages.Text,
                                NewMessage = 0,
                                TimeText = checker.TimeText,
                                UserData = userData,
                            });
                        } 
                    }
 
                    SqLiteDatabase dbDatabase = new SqLiteDatabase();
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
                        Position = "Right",
                    };
                    //Update All data users to database
                    dbDatabase.InsertOrUpdateToOneMessages(message);
                    dbDatabase.Dispose();

                    MessagesBoxActivity.UpdateOneMessage(message);

                    if (AppSettings.RunSoundControl)
                        Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("Popup_SendMesseges.mp3");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}