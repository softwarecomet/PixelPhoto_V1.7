using System;
using System.Collections.ObjectModel;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Refractored.Controls;
using UserDataObject = PixelPhotoClient.GlobalClass.UserDataObject;

namespace PixelPhoto.Activities.Comment.Adapters
{
    public class ReplyAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AvatarReplyAdapterClickEventArgs> AvatarClick;
        public event EventHandler<ReplyAdapterClickEventArgs> ItemClick;
        public event EventHandler<ReplyAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<ReplyObject> ReplyList = new ObservableCollection<ReplyObject>();
        private SocialIoClickListeners ClickListeners;

        public ReplyAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                ClickListeners = new SocialIoClickListeners(context);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Comment, parent, false);
                var vh = new ReplyAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is ReplyAdapterViewHolder holder)
                {
                    var item = ReplyList[position];
                    if (item != null)
                    {
                        TextSanitizer changer = new TextSanitizer(holder.CommentText, ActivityContext);
                        changer.Load(Methods.FunString.DecodeString(item.Text));

                        holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time));

                        GlideImageLoader.LoadImage(ActivityContext, item.UserData.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                        holder.ReplyButton.Text = ActivityContext.GetString(Resource.String.Lbl_Reply);

                        holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                         
                        if (!holder.Image.HasOnClickListeners)
                            holder.Image.Click += (sender, e) => OnAvatarClick(new AvatarReplyAdapterClickEventArgs { Class = item.UserData, Position = position, View = holder.MainView });

                        holder.LikeIcon.SetImageResource(item.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);

                        if (!holder.LikeIcon.HasOnClickListeners)
                        {
                            MyBounceInterpolator interpolator = new MyBounceInterpolator(0.2, 20);

                            var animationScale = AnimationUtils.LoadAnimation(ActivityContext, Resource.Animation.scale);
                            animationScale.Interpolator = interpolator;
                            holder.LikeIcon.Click += (sender, args) =>
                            {
                                try
                                {
                                    if (!Methods.CheckConnectivity())
                                        Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    else
                                    {
                                        item.IsLiked = item.IsLiked == 0 ? 1 : 0;

                                        holder.LikeIcon.SetImageResource(item.IsLiked == 1 ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);
                                        holder.LikeIcon.StartAnimation(animationScale);

                                        if (item.IsLiked == 1)
                                        {
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                                            item.Likes++;
                                            holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                                        }
                                        else
                                        {
                                            if (item.Likes > 0)
                                                item.Likes--;
                                            else
                                                item.Likes = 0;

                                            holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                                        }

                                        RequestsAsync.Post.LikeReply(item.Id.ToString()).ConfigureAwait(false);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            };
                        }

                        if (!holder.ReplyButton.HasOnClickListeners)
                            holder.ReplyButton.Click += (sender, args) =>
                            {
                                try
                                {
                                    //ActivityContext.TxtComment.Text = "";
                                    //ActivityContext.TxtComment.Text = "@" + item.Publisher.Username + " ";
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            };

                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public class MyBounceInterpolator : Java.Lang.Object, IInterpolator
        {
            private readonly double MAmplitude = 1;
            private readonly double MFrequency = 10;

            public MyBounceInterpolator(double amplitude, double frequency)
            {
                MAmplitude = amplitude;
                MFrequency = frequency;
            }
            float IInterpolator.GetInterpolation(float time)
            {
                return (float)(-1 * Math.Pow(Math.E, -time / MAmplitude) *
                               Math.Cos(MFrequency * time) + 1);
            }

            public float GetInterpolation(float input)
            {
                return 0;
            }
        }

        
        public override int ItemCount => ReplyList?.Count ?? 0;

        public ReplyObject GetItem(int position)
        {
            return ReplyList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return 0;
            }
        }

        void OnAvatarClick(AvatarReplyAdapterClickEventArgs args) => AvatarClick?.Invoke(this, args);
        void OnClick(ReplyAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(ReplyAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class ReplyAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic


        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public ImageView LikeIcon { get; private set; } 
        public AutoLinkTextView CommentText { get; private set; } 
        public TextView TimeTextView { get; private set; }
        public TextView LikeCount { get; private set; }
        public TextView ReplyButton { get; private set; }
         
        #endregion

        public ReplyAdapterViewHolder(View itemView, Action<ReplyAdapterClickEventArgs> clickListener, Action<ReplyAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<CircleImageView>(Resource.Id.card_pro_pic);
                CommentText = MainView.FindViewById<AutoLinkTextView>(Resource.Id.active);
                LikeIcon = MainView.FindViewById<ImageView>(Resource.Id.likeIcon);
                TimeTextView = MainView.FindViewById<TextView>(Resource.Id.time);
                LikeCount = MainView.FindViewById<TextView>(Resource.Id.Like);
                ReplyButton = MainView.FindViewById<TextView>(Resource.Id.reply);

                ReplyButton.Text = MainView.Context.GetString(Resource.String.Lbl_Reply);
                ReplyButton.Visibility = ViewStates.Gone;

                if (AppSettings.SetTabDarkTheme)
                    LikeIcon.SetBackgroundResource(Resource.Drawable.Shape_Circle_Black);

                //Event
                itemView.Click += (sender, e) => clickListener(new ReplyAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class ReplyAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public class AvatarReplyAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public UserDataObject Class { get; set; }
    } 
}