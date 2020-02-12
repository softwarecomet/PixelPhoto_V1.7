using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Android.Graphics;
using Android.Graphics.Drawables;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Console = System.Console;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Tabbes.Adapters
{
    public class UserPostAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<UserPostAdapterViewHolderClickEventArgs> ItemClick;
        public event EventHandler<UserPostAdapterViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<PostsObject> PostList = new ObservableCollection<PostsObject>();
        private readonly RequestBuilder FullGlideRequestBuilder;

        public UserPostAdapter(Activity context)
        {
            try
            {
                ActivityContext = context;
                FullGlideRequestBuilder = Glide.With(context).AsDrawable().SetDiskCacheStrategy(DiskCacheStrategy.Automatic).Override(500).Placeholder(new ColorDrawable(Color.ParseColor("#efefef")));
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
                //Setup your layout here >> Style_LastActivities_View
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Featured_View, parent, false);
                UserPostAdapterViewHolder vh = new UserPostAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is UserPostAdapterViewHolder holder)
                {
                    PostsObject item = PostList[position];
                    if (item != null)
                    {
                        item.Mp4 = Methods.FunString.StringNullRemover(item.Mp4);

                        NativeFeedType type = NewsFeedAdapter.GetPostType(item);


                        if (type == NativeFeedType.Video)
                        { 
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.PlayIcon, IonIconsFonts.Play);

                            if (holder.PlayIcon.Visibility != ViewStates.Visible)
                            {
                                holder.PlayIcon.Visibility = ViewStates.Visible;
                            }

                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.Videocamera);

                            if (holder.TypeIcon.Visibility != ViewStates.Visible)
                            {
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                            } 
                        }
                        else if (type == NativeFeedType.Gif)
                        {
                            holder.TypeIcon.Text = ActivityContext.GetText(Resource.String.Lbl_Gif);
                            FontUtils.SetFont(holder.TypeIcon, Fonts.SfSemibold);

                            if (holder.TypeIcon.Visibility != ViewStates.Visible)
                            {
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                            }
                        }
                        else if (type == NativeFeedType.Youtube)
                        {

                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.PlayIcon, IonIconsFonts.Play);

                            if (holder.PlayIcon.Visibility != ViewStates.Visible)
                            {
                                holder.PlayIcon.Visibility = ViewStates.Visible;
                            }

                            FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, holder.TypeIcon, IonIconsFonts.SocialYoutube);

                            if (holder.TypeIcon.Visibility != ViewStates.Visible)
                            {
                                holder.TypeIcon.Visibility = ViewStates.Visible;
                            }
                        }


                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                        {
                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;
                        }

                        if (type == NativeFeedType.Video)
                        {
                            if (!string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            {
                                FullGlideRequestBuilder.Load(item.MediaSet[0]?.Extra).Into(holder.Image);
                            }
                            else
                            {
                                var fileName = item.MediaSet[0].File.Split('/').Last();
                                var fileNameWithoutExtension = fileName.Split('.').First();

                                item.MediaSet[0].Extra = Methods.Path.FolderDcimImage + "/" + fileNameWithoutExtension + ".png";

                                var vidoePlaceHolderImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileNameWithoutExtension + ".png");
                                if (vidoePlaceHolderImage == "File Dont Exists")
                                {
                                    var bitmapImage = Methods.MultiMedia.Retrieve_VideoFrame_AsBitmap(ActivityContext, item.MediaSet[0]?.File);
                                    if (bitmapImage != null)
                                    {
                                        Methods.MultiMedia.Export_Bitmap_As_Image(bitmapImage, fileNameWithoutExtension, Methods.Path.FolderDcimImage);
                                        FullGlideRequestBuilder.Load(bitmapImage).Into(holder.Image);
                                    }
                                    else
                                    {
                                        Glide.With(ActivityContext)
                                            .AsBitmap()
                                            .Apply(new RequestOptions().Placeholder(Resource.Drawable.blackdefault).Error(Resource.Drawable.blackdefault))
                                            .Load(item.MediaSet[0]?.File) // or URI/path
                                            .Into(holder.Image); //image view to set thumbnail to 
                                    }
                                }
                                else
                                {
                                    FullGlideRequestBuilder.Load(vidoePlaceHolderImage).Into(holder.Image);
                                }
                            }
                        }
                        else
                        {
                            FullGlideRequestBuilder.Load(!string.IsNullOrEmpty(item.MediaSet[0].Extra) ? item.MediaSet[0].Extra : item.MediaSet[0].File).Into(holder.Image);
                            //GlideImageLoader.LoadImage(ActivityContext, !string.IsNullOrEmpty(item.MediaSet[0].Extra) ? item.MediaSet[0].Extra : item.MediaSet[0].File, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Color);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

       public override int ItemCount => PostList?.Count ?? 0;


        public PostsObject GetItem(int position)
        {
            return PostList[position];
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

        private void OnClick(UserPostAdapterViewHolderClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(UserPostAdapterViewHolderClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = PostList[p0];
                if (item == null)
                    return d;

                var type = NewsFeedAdapter.GetPostType(item);
                switch (type)
                {
                    case NativeFeedType.Video:
                    case NativeFeedType.Youtube:
                        d.Add(item.MediaSet[0]?.Extra);
                        break;
                    case NativeFeedType.Gif:
                    case NativeFeedType.Photo:
                        if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            item.MediaSet[0].Extra = item.MediaSet[0]?.File;

                        d.Add(item.MediaSet[0]?.Extra);
                        break;
                    case NativeFeedType.MultiPhoto:
                    {
                        foreach (var image in item.MediaSet)
                        {
                            if (string.IsNullOrEmpty(item.MediaSet[0]?.Extra))
                            {
                                item.MediaSet[0].Extra = item.MediaSet[0]?.File;
                                image.Extra = image.File;
                            }

                            d.Add(image.Extra);
                        }
                        break;
                    }
                }

                return d;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                List<string> d = new List<string>();
                return d;
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return FullGlideRequestBuilder.Load(p0.ToString());
            //return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CenterCrop);
        }
    }

    public class UserPostAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic
        public View MainView { get; private set; }
        public FrameLayout ViewFrm { get; private set; }
        public ImageView Image { get; private set; }
        public TextView PlayIcon { get; private set; }
        public TextView TypeIcon { get; private set; }
        #endregion

        public UserPostAdapterViewHolder(View itemView, Action<UserPostAdapterViewHolderClickEventArgs> clickListener, Action<UserPostAdapterViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)MainView.FindViewById(Resource.Id.Image);
                TypeIcon = (TextView)MainView.FindViewById(Resource.Id.typeicon);
                PlayIcon = (TextView)MainView.FindViewById(Resource.Id.playicon);
                ViewFrm = (FrameLayout)MainView.FindViewById(Resource.Id.viewfrm);

                //Create an Event
                ViewFrm.Click += (sender, e) => clickListener(new UserPostAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new UserPostAdapterViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class UserPostAdapterViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}