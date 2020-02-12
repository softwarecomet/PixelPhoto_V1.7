using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Com.Luseen.Autolinklibrary;
using PixelPhoto.Helpers.CacheLoaders;
using System;
using System.Collections.ObjectModel;
using Android.Views.Animations;
using PixelPhoto.Activities.Posts.Listeners;
using PixelPhoto.Helpers.Controller;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Refractored.Controls;

namespace PixelPhoto.Activities.Comment.Adapters
{ 
    public class CommentsAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AvatarCommentAdapterClickEventArgs> AvatarClick;
        public event EventHandler<CommentAdapterClickEventArgs> ItemClick;
        public event EventHandler<CommentAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<CommentObject> CommentList = new ObservableCollection<CommentObject>();
        private readonly SocialIoClickListeners ClickListeners;
        public CommentsAdapter(Activity context)
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
                var vh = new CommentAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is CommentAdapterViewHolder holder)
                {
                    var item = CommentList[position];
                    if (item != null)
                    { 
                        TextSanitizer changer = new TextSanitizer(holder.CommentText, ActivityContext);
                        changer.Load(Methods.FunString.DecodeString(item.Text));

                        holder.TimeTextView.Text = Methods.Time.TimeAgo(Convert.ToInt32(item.Time));

                        GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.CircleCrop, ImagePlaceholders.Color);

                        if (item.Replies != 0)
                            holder.ReplyButton.Text = ActivityContext.GetString(Resource.String.Lbl_Reply) + " " + "(" + item.Replies + ")"; 
                        else
                            holder.ReplyButton.Text = ActivityContext.GetString(Resource.String.Lbl_Reply);

                        holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                         
                        if (!holder.Image.HasOnClickListeners)
                            holder.Image.Click+= (sender, e) => OnAvatarClick(new AvatarCommentAdapterClickEventArgs{Class = item,Position = position, View = holder.MainView });

                        holder.LikeIcon.SetImageResource(item.IsLiked ? Resource.Drawable.ic_action_like_2 : Resource.Drawable.ic_action_like_1);

                        if (!holder.LikeIcon.HasOnClickListeners)
                        {
                            MyBounceInterpolator interpolator = new MyBounceInterpolator(0.2, 20);
                          
                            var animationScale = AnimationUtils.LoadAnimation(ActivityContext, Resource.Animation.scale);
                            animationScale.Interpolator =interpolator;
                            holder.LikeIcon.Click += (sender, args) =>
                            {
                                try
                                {
                                    if (!Methods.CheckConnectivity())
                                        Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long).Show();
                                    else
                                    { 
                                        item.IsLiked = !item.IsLiked;
                                        holder.LikeIcon.SetImageResource(item.IsLiked? Resource.Drawable.ic_action_like_2: Resource.Drawable.ic_action_like_1);
                                        holder.LikeIcon.StartAnimation(animationScale);

                                        if (item.IsLiked)
                                        {
                                            Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                                            int x = int.Parse(item.Likes) + 1; 
                                            holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + x.ToString() + ")";
                                        }
                                        else
                                        {
                                            int x = int.Parse(item.Likes);

                                            if (x > 0)
                                                x--;
                                            else
                                                x = 0;

                                            item.Likes = x.ToString();

                                            holder.LikeCount.Text = ActivityContext.GetString(Resource.String.Lbl_Likes) + " " + "(" + item.Likes + ")";
                                        }
 
                                        RequestsAsync.Post.LikeComment(item.Id.ToString()).ConfigureAwait(false);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }; 
                        }

                        if (!holder.ReplyButton.HasOnClickListeners)
                            holder.ReplyButton.Click += (sender, args) => ClickListeners.CommentReplyPostClick(new CommentReplyClickEventArgs { CommentObject = item, Position = position, View = holder.MainView });

                    }

                    if (AppSettings.SetTabDarkTheme)
                        holder.LikeIcon.SetBackgroundResource(Resource.Drawable.Shape_Circle_Black);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

       public class MyBounceInterpolator :Java.Lang.Object ,IInterpolator
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
         
        public override int ItemCount => CommentList?.Count ?? 0;

        public CommentObject GetItem(int position)
        {
            return CommentList[position];
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

        void OnAvatarClick(AvatarCommentAdapterClickEventArgs args) => AvatarClick?.Invoke(this, args);
        void OnClick(CommentAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(CommentAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class CommentAdapterViewHolder : RecyclerView.ViewHolder
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

        public CommentAdapterViewHolder(View itemView, Action<CommentAdapterClickEventArgs> clickListener,Action<CommentAdapterClickEventArgs> longClickListener) : base(itemView)
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
                 
                //Event
                itemView.Click += (sender, e) => clickListener(new CommentAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class CommentAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }

    public class AvatarCommentAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public CommentObject Class { get; set; }
    }


}