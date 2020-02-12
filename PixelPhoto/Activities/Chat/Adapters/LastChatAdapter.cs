using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AmulyaKhare.TextDrawableLib;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Bumptech.Glide;
using Java.Util;
using PixelPhoto.Helpers.CacheLoaders;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Messages;
using IList = System.Collections.IList;
using Object = Java.Lang.Object;


namespace PixelPhoto.Activities.Chat.Adapters
{
    public class LastChatAdapter : RecyclerView.Adapter, ListPreloader.IPreloadModelProvider
    {
        private readonly Activity ActivityContext;

        public ObservableCollection<GetChatsObject.Data> UserList = new ObservableCollection<GetChatsObject.Data>();
        public event EventHandler<LastChatAdapterClickEventArgs> ItemClick;
        public event EventHandler<LastChatAdapterClickEventArgs> ItemLongClick;

        private IOnClickListenerSelected OnClickListener;
        private readonly SparseBooleanArray SelectedItems;
        private int CurrentSelectedIdx = -1;

        public LastChatAdapter(Activity context )
        {
            try
            {
                ActivityContext = context;
                SelectedItems = new SparseBooleanArray();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetOnClickListener(IOnClickListenerSelected onClickListener)
        {
            OnClickListener = onClickListener;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is LastChatAdapterViewHolder holder)
                {
                    var item = UserList[position];
                    if (item != null)
                    {
                        Initialize(holder, item);
                          
                        holder.LytParent.Activated = SelectedItems.Get(position, false);

                        if (!holder.LytParent.HasOnClickListeners)
                        { 
                            holder.LytParent.Click += (sender, args) =>
                            {
                                try
                                {
                                    if (OnClickListener == null) return;

                                    OnClickListener.OnItemClick(holder.MainView, item, position);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            };

                            holder.LytParent.LongClick += (sender, args) =>
                            {
                                try
                                {
                                    if (OnClickListener == null) return;

                                    OnClickListener.OnItemLongClick(holder.MainView, item, position);

                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            };
                        }
                         
                        ToggleCheckedIcon(holder, position);  
                    } 
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        public void Initialize(LastChatAdapterViewHolder holder, GetChatsObject.Data item)
        {
            try
            {
                GlideImageLoader.LoadImage(ActivityContext,item.Avatar, holder.ImageAvatar, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);

                string name = Methods.FunString.DecodeString(item.UserData.Name);
                if (holder.TxtUsername.Text != name)
                {
                    holder.TxtUsername.Text = name;
                }

                string lastMessage = Methods.FunString.DecodeString(item.LastMessage);
                if (holder.TxtLastMessages.Text != lastMessage)
                {
                    holder.TxtLastMessages.Text = lastMessage;
                }

                //last seen time  
                holder.TxtTimestamp.Text = Methods.Time.ReplaceTime(item.TimeText);

                if (item.NewMessage <= 0)
                {
                    holder.ImageColor.Visibility = ViewStates.Invisible;
                }
                else
                { 
                    var drawable = TextDrawable.InvokeBuilder().BeginConfig().FontSize(25).EndConfig().BuildRound(item.NewMessage.ToString(), Color.ParseColor(AppSettings.MainColor));
                    holder.ImageColor.SetImageDrawable(drawable);
                    holder.ImageColor.Visibility = ViewStates.Visible;
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> Style_HContact_view
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.Style_LastChat_view, parent, false);
                var vh = new LastChatAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }
        }
          
         public override int ItemCount => UserList?.Count ?? 0;

        
        public void Add(GetChatsObject.Data user)
        {
            try
            {
                var check = UserList.FirstOrDefault(a => a.UserId == user.UserId);
                if (check == null)
                { 
                    UserList.Add(user);
                    NotifyItemInserted(UserList.IndexOf(UserList.Last()));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void Move(GetChatsObject.Data users)
        {
            try
            {
                var data = UserList.FirstOrDefault(a => a.UserId == users.UserId);
                if (data != null)
                {
                    var index = UserList.IndexOf(data);
                    if (index > -1 && index != 0)
                    {
                        UserList.Move(index, 0);
                        NotifyItemMoved(index, 0);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Insert(GetChatsObject.Data users)
        {
            try
            {
                UserList.Insert(0, users);
                NotifyItemInserted(UserList.IndexOf(UserList.FirstOrDefault()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Update(GetChatsObject.Data user)
        {
            try
            {
                var data = UserList.FirstOrDefault(a => a.Id == user.Id);
                if (data != null)
                {
                    data.UserId = user.UserId;
                    data.Username = user.Username;
                    data.Avatar = user.Avatar;
                    data.Time = user.Time;
                    data.Id = user.Id;
                    data.LastMessage = user.LastMessage;
                    data.NewMessage = user.NewMessage;
                    user.UserData.Name = user.UserData.Name;
                    data.TimeText = user.TimeText;

                    ActivityContext.RunOnUiThread(() =>
                    {
                        NotifyItemChanged(UserList.IndexOf(data));
                    });
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Remove(GetChatsObject.Data users)
        {
            try
            {
                var index = UserList.IndexOf(UserList.FirstOrDefault(a => a.UserId == users.UserId));
                if (index != -1)
                {
                    UserList.Remove(users);
                    NotifyItemRemoved(index);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
         
        public void BindEnd()
        {
            try
            {
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Clear()
        {
            try
            {
               
                ListUtils.ChatList.Clear();
                UserList.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Toolbar & Selected

        private void ToggleCheckedIcon(LastChatAdapterViewHolder holder, int position)
        {
            try
            {
                if (SelectedItems.Get(position, false))
                {
                    holder.LytImage.Visibility = ViewStates.Gone;
                    holder.LytChecked.Visibility = ViewStates.Visible;
                    if (CurrentSelectedIdx == position) ResetCurrentItems();
                }
                else
                {
                    holder.LytChecked.Visibility = ViewStates.Gone;
                    holder.LytImage.Visibility = ViewStates.Visible;
                    if (CurrentSelectedIdx == position) ResetCurrentItems();
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
        }

        private void ResetCurrentItems()
        {
            try
            {
                CurrentSelectedIdx = -1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public int GetSelectedItemCount()
        {
            return SelectedItems.Size();
        }

        public List<int> GetSelectedItems()
        {
            List<int> items = new List<int>(SelectedItems.Size());
            for (int i = 0; i < SelectedItems.Size(); i++)
            {
                items.Add(SelectedItems.KeyAt(i));
            }
            return items;
        }

        public void RemoveData(int position, GetChatsObject.Data users)
        {
            try
            {
                Remove(users);
                ResetCurrentItems();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ClearSelections()
        {
            try
            {
                SelectedItems.Clear();
                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ToggleSelection(int pos)
        {
            try
            {
                CurrentSelectedIdx = pos;
                if (SelectedItems.Get(pos, false))
                {
                    SelectedItems.Delete(pos);
                }
                else
                {
                    SelectedItems.Put(pos, true);
                }
                NotifyItemChanged(pos);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        #endregion

        public GetChatsObject.Data GetItem(int position)
        {
            return UserList[position];
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
         
        private void OnClick(LastChatAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        private void OnLongClick(LastChatAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }

        public IList GetPreloadItems(int p0)
        {
            try
            {
                var d = new List<string>();
                var item = UserList[p0];
                if (item == null)
                    return Collections.SingletonList(p0);

                if (item.UserData?.Avatar != "")
                {
                    d.Add(item.UserData?.Avatar);
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

    public class LastChatAdapterViewHolder : RecyclerView.ViewHolder
    {
        #region Variables Basic

        public View MainView { get; private set; }

        public RelativeLayout LytParent { get; private set; }
        public TextView TxtUsername { get; private set; }
        public TextView TxtLastMessages { get; private set; }
        public TextView TxtTimestamp { get; private set; }
        public ImageView ImageAvatar { get; private set; }  
        public ImageView ImageColor { get; private set; }

        public RelativeLayout LytChecked { get; private set; }
        public RelativeLayout LytImage { get; private set; } 

        #endregion

        public LastChatAdapterViewHolder(View itemView, Action<LastChatAdapterClickEventArgs> clickListener,Action<LastChatAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                //Get values
                LytParent = (RelativeLayout)MainView.FindViewById(Resource.Id.main);
                TxtUsername = (TextView)MainView.FindViewById(Resource.Id.Txt_Username);
                TxtLastMessages = (TextView)MainView.FindViewById(Resource.Id.Txt_LastMessages);
                TxtTimestamp = (TextView)MainView.FindViewById(Resource.Id.Txt_timestamp);
                ImageAvatar = (ImageView)MainView.FindViewById(Resource.Id.ImageAvatar);

                ImageColor = (ImageView)MainView.FindViewById(Resource.Id.image_view);

                LytChecked = (RelativeLayout)MainView.FindViewById(Resource.Id.lyt_checked);
                LytImage = (RelativeLayout)MainView.FindViewById(Resource.Id.lyt_image);

                //Create an Event
                //itemView.Click += (sender, e) => clickListener(new LastChatAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
                //itemView.LongClick += (sender, e) => longClickListener(new LastChatAdapterClickEventArgs { View = itemView, Position = AdapterPosition });

                //Dont Remove this code #####
                FontUtils.SetFont(TxtUsername,Fonts.SfRegular);
                FontUtils.SetFont(TxtLastMessages, Fonts.SfMedium);
                //##### 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        } 
    }

    public class LastChatAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    } 
}