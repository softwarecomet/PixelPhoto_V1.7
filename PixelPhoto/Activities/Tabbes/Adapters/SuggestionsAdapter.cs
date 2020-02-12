using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.CacheLoaders;
using Refractored.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Bumptech.Glide;
using Java.Util;
using PixelPhoto.Activities.MyContacts.Adapters;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;

namespace PixelPhoto.Activities.Tabbes.Adapters
{ 
    public class SuggestionsAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        public event EventHandler<SuggestionsAdapterClickEventArgs> ItemClick;
        public event EventHandler<SuggestionsAdapterClickEventArgs> ItemLongClick;

        private readonly Activity ActivityContext;
        public ObservableCollection<UserDataObject> SuggestionsList = new ObservableCollection<UserDataObject>();
        
        public SuggestionsAdapter(Activity context)
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

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position, IList<Object> payloads)
        {
            try
            {
                var users = SuggestionsList[position];
                if (payloads.Count > 0)
                {
                    if (viewHolder is SuggestionsAdapterViewHolder holder)
                    {
                        var data = (string)payloads[0];
                        if (data == "true")
                        {
                            holder.Button.SetBackgroundResource(Resource.Xml.background_signup2);
                            holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                            holder.Button.Tag = "true";
                            users.IsFollowing = true;
                        }
                        else
                        {
                            holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                            holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                            holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Follow);
                            holder.Button.Tag = "false";
                            users.IsFollowing = false;
                        }
                    }
                }
                else
                {
                    base.OnBindViewHolder(viewHolder, position, payloads);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                base.OnBindViewHolder(viewHolder, position, payloads);
            }
        }


        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_PageCircle_view
                View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_Suggestions_view, parent, false);
                var vh = new SuggestionsAdapterViewHolder(itemView, OnClick, OnLongClick);
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
                if (!(viewHolder is SuggestionsAdapterViewHolder holder))
                    return;

                var item = SuggestionsList[position];
                if (item == null)
                    return;

                holder.Username.Text = Methods.FunString.SubStringCutOf("@" + item.Username, 15) ;
                holder.Name.Text =Methods.FunString.SubStringCutOf(item.Name,15) ;

                GlideImageLoader.LoadImage(ActivityContext, item.Avatar, holder.Image, ImageStyle.RoundedCrop, ImagePlaceholders.Color);
                 
                switch (item.IsFollowing)
                {
                    // My Friend
                    case true:
                    {
                        holder.Button.SetBackgroundResource(Resource.Xml.background_signup2);
                        holder.Button.SetTextColor(Color.ParseColor("#ffffff"));
                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Following);
                        holder.Button.Tag = "true";
                        break;
                    }
                    case false:
                    {
                        holder.Button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                        holder.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                        holder.Button.Text = ActivityContext.GetText(Resource.String.Lbl_Follow);
                        holder.Button.Tag = "false";
                        break;
                    }
                }

                if (item.UserId.ToString() == UserDetails.UserId)
                    holder.Button.Visibility = ViewStates.Gone;
                  
                if (!holder.Button.HasOnClickListeners)
                    holder.Button.Click += (sender, e) => FollowButtonClick(new FollowFollowingClickEventArgs { View = viewHolder.ItemView, UserClass = item, Position = position, ButtonFollow = holder.Button });
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        private void FollowButtonClick(FollowFollowingClickEventArgs e)
        {
            try
            {
                if (e.UserClass != null)
                {
                    NotifyItemChanged(e.Position, e.ButtonFollow.Tag.ToString() == "false" ? "true" : "false");

                    if (Methods.CheckConnectivity())
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => RequestsAsync.User.FollowUnFollow(e.UserClass.UserId.ToString()) });
                    else
                        Toast.MakeText(ActivityContext, ActivityContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public override int ItemCount => SuggestionsList?.Count ?? 0;

        public UserDataObject GetItem(int position)
        {
            return SuggestionsList[position];
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

        void OnClick(SuggestionsAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(SuggestionsAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = SuggestionsList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.Avatar != "")
                {
                    d.Add(item.Avatar);
                    return d;
                }

                return d;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Collections.SingletonList(p0);
            }
        }

        public RequestBuilder GetPreloadRequestBuilder(Object p0)
        {
            return GlideImageLoader.GetPreLoadRequestBuilder(ActivityContext, p0.ToString(), ImageStyle.CircleCrop);
        }

    }

    public class SuggestionsAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic


        public View MainView { get; private set; }
        public ImageView Image { get; private set; }
        public CircleImageView ImageOnline { get; set; }

        public TextView Name { get; private set; }
        public TextView Username { get; private set; }
        public Button Button { get; private set; }

        #endregion

        public SuggestionsAdapterViewHolder(View itemView, Action<SuggestionsAdapterClickEventArgs> clickListener,Action<SuggestionsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                Image = MainView.FindViewById<ImageView>(Resource.Id.people_profile_sos);
                Name = MainView.FindViewById<TextView>(Resource.Id.people_profile_name);
                Username = MainView.FindViewById<TextView>(Resource.Id.people_profile_username);
                Button = MainView.FindViewById<Button>(Resource.Id.btn_follow_people);
               
                //Event
                itemView.Click += (sender, e) => clickListener(new SuggestionsAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class SuggestionsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    } 
}