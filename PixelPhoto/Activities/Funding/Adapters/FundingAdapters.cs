using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Android.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;

namespace PixelPhoto.Activities.Funding.Adapters
{
    public class FundingAdapters : RecyclerView.Adapter
    {
        public event EventHandler<FundingAdaptersViewHolderClickEventArgs> ItemClick;
        public event EventHandler<FundingAdaptersViewHolderClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;

        public ObservableCollection<FundingDataObject> FundingList = new ObservableCollection<FundingDataObject>();

        public FundingAdapters(Activity context)
        {
            try
            {
                ActivityContext = context;  
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
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_FundingView, parent, false);
                var vh = new FundingAdaptersViewHolder(itemView, OnClick, OnLongClick);
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
                if (viewHolder is FundingAdaptersViewHolder holder)
                {
                    var item = FundingList[position];
                    if (item != null)
                    {  
                        GlideImageLoader.LoadImage(ActivityContext, item.Image, holder.Image, ImageStyle.CenterCrop, ImagePlaceholders.Drawable);

                        holder.Title.Text = Methods.FunString.DecodeString(item.Title);
                        //$0 Raised of $1000000
                        holder.Description.Text = "$" + item.Raised.ToString(CultureInfo.InvariantCulture) + " " + ActivityContext.GetString(Resource.String.Lbl_RaisedOf) + " " + "$" + item.Amount;
                        holder.Progress.Progress = Convert.ToInt32(item.Bar);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override int ItemCount => FundingList?.Count ?? 0;
        
        public FundingDataObject GetItem(int position)
        {
            return FundingList[position];
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

        void OnClick(FundingAdaptersViewHolderClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(FundingAdaptersViewHolderClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class FundingAdaptersViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public ImageView Image { get; private set; }
        public TextView Title { get; private set; }
        public TextView Description { get; private set; }
        public ProgressBar Progress { get; private set; }

        #endregion

        public FundingAdaptersViewHolder(View itemView, Action<FundingAdaptersViewHolderClickEventArgs> clickListener, Action<FundingAdaptersViewHolderClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;
                Image = (ImageView)MainView.FindViewById(Resource.Id.Image);
                Title = (TextView)MainView.FindViewById(Resource.Id.Title);
                Description = (TextView)MainView.FindViewById(Resource.Id.description);
                Progress = (ProgressBar)MainView.FindViewById(Resource.Id.progressBar);

                //Create an Event
                itemView.Click += (sender, e) => clickListener(new FundingAdaptersViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
                itemView.LongClick += (sender, e) => longClickListener(new FundingAdaptersViewHolderClickEventArgs { View = itemView, Position = AdapterPosition });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class FundingAdaptersViewHolderClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}