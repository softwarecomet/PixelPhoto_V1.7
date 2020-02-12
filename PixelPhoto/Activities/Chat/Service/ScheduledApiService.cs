﻿using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Content;
using Android.OS;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Helpers.Utils;
using PixelPhoto.SQLite;
using PixelPhotoClient.Classes.Messages;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;

namespace PixelPhoto.Activities.Chat.Service
{
    [Service]
    public class ScheduledApiService : Android.App.Service
    {
        private readonly Handler MainHandler = new Handler();
        private ResultReceiver ResultSender;

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            try
            {
                base.OnCreate();

                MainHandler.PostDelayed(new ApiPostUpdaterHelper(Application.Context, MainHandler, ResultSender), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            try
            {
                var rec = intent.GetParcelableExtra("receiverTag");
                ResultSender = (ResultReceiver)rec;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            MainHandler.PostDelayed(new ApiPostUpdaterHelper(Application.Context, MainHandler, ResultSender), AppSettings.RefreshChatActivitiesSeconds);

            return base.OnStartCommand(intent, flags, startId);
        }
    }

    public class ApiPostUpdaterHelper : Java.Lang.Object, IRunnable
    {
        private readonly Handler MainHandler;
        private readonly Context Activity;
        private readonly ResultReceiver ResultSender;

        public ApiPostUpdaterHelper(Context activity, Handler mainHandler, ResultReceiver resultSender)
        {
            MainHandler = mainHandler;
            Activity = activity;
            ResultSender = resultSender;
        }

        public async void Run()
        {
            try
            {
                //Toast.MakeText(Application.Context, "Started", ToastLength.Short).Show(); 
                if (ResultSender == null)
                {
                    try
                    {
                        SqLiteDatabase dbDatabase = new SqLiteDatabase();
                        dbDatabase.Get_data_Login_Credentials();

                        (int apiStatus, var respond) = await RequestsAsync.Messages.GetChats("15", "0"); 
                        if (apiStatus != 200 || !(respond is GetChatsObject result))
                        {
                            // Methods.DisplayReportResult(Activity, respond);
                        }
                        else
                        {
                            //Toast.MakeText(Application.Context, "ResultSender 1 \n" + data, ToastLength.Short).Show();
                             
                            if (result.data.Count > 0)
                            {
                                ListUtils.ChatList = new ObservableCollection<GetChatsObject.Data>(result.data);
                                //Insert All data users to database
                                dbDatabase.InsertOrReplaceLastChatTable(ListUtils.ChatList);
                            }
                        }
                        dbDatabase.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        // Toast.MakeText(Application.Context, "Exception  " + e, ToastLength.Short).Show();
                    }
                }
                else
                {
                    (int apiStatus, var respond) = await RequestsAsync.Messages.GetChats("15", "0");
                    if (apiStatus != 200 || !(respond is GetChatsObject result))
                    {
                       // Methods.DisplayReportResult(Activity, respond);
                    }
                    else
                    {
                        var b = new Bundle();
                        b.PutString("Json", JsonConvert.SerializeObject(result));
                        ResultSender.Send(0, b);

                        //Toast.MakeText(Application.Context, "ResultSender 2 \n" + data, ToastLength.Short).Show();

                        Console.WriteLine("Allen Post + started");
                    }
                }

                MainHandler.PostDelayed(new ApiPostUpdaterHelper(Activity, MainHandler, ResultSender), AppSettings.RefreshChatActivitiesSeconds);
            }
            catch (Exception e)
            {
                //Toast.MakeText(Application.Context, "ResultSender failed", ToastLength.Short).Show();
                MainHandler.PostDelayed(new ApiPostUpdaterHelper(Activity, MainHandler, ResultSender), AppSettings.RefreshChatActivitiesSeconds);
                Console.WriteLine(e);
                Console.WriteLine("Allen Post + failed");
            }
        }
    }
} 