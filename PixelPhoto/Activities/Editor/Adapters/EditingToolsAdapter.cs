﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Model.Editor;

namespace PixelPhoto.Activities.Editor.Adapters
{
    public class EditingToolsAdapter : RecyclerView.Adapter
    {
        public readonly ObservableCollection<ToolModel> MToolList = new ObservableCollection<ToolModel>();

        public EditingToolsAdapter(Activity context)
        {
            try
            {
                var activityContext = context;

                string brush = activityContext.GetText(Resource.String.Lbl_brush);
                string text = activityContext.GetText(Resource.String.Lbl_text);
                string eraser = activityContext.GetText(Resource.String.Lbl_eraser);
                string effect = activityContext.GetText(Resource.String.Lbl_effect);
                string filter = activityContext.GetText(Resource.String.Lbl_filter);
                string sticker = activityContext.GetText(Resource.String.Lbl_sticker);
                string emojis = activityContext.GetText(Resource.String.Lbl_emoji);
                string image = activityContext.GetText(Resource.String.image);
                string color = activityContext.GetText(Resource.String.Lbl_Color);


                MToolList.Add(new ToolModel{ MToolIcon = "\uf1fc", MToolName = brush, MToolType = ToolType.Brush, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf035", MToolName = text, MToolType = ToolType.Text, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf12d", MToolName = eraser, MToolType = ToolType.Eraser, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf043", MToolName = effect, MToolType = ToolType.Filter, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf0d0", MToolName = filter, MToolType = ToolType.FilterColor, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf1fd", MToolName = sticker, MToolType = ToolType.Sticker, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf118", MToolName = emojis, MToolType = ToolType.Emojis, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf03e", MToolName = image, MToolType = ToolType.Image, MToolColor = "#212121" });
                MToolList.Add(new ToolModel{ MToolIcon = "\uf43c", MToolName = color, MToolType = ToolType.Color, MToolColor = "#212121" });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override int ItemCount => MToolList?.Count ?? 0;

        public event EventHandler<EditingToolsAdapterClickEventArgs> ItemClick;
        public event EventHandler<EditingToolsAdapterClickEventArgs> ItemLongClick;

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            try
            {
                //Setup your layout here >> row_editing_tools
                var itemView = LayoutInflater.From(parent.Context)
                    .Inflate(Resource.Layout.row_editing_tools, parent, false);
                var vh = new EditingToolsAdapterViewHolder(itemView, OnClick, OnLongClick);
                return vh;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is EditingToolsAdapterViewHolder holder)
                {
                    var item = MToolList[position];
                    if (item != null)
                    {
                        holder.TxtTool.Text = item.MToolName;
                        holder.TxtTool.SetTextColor(Color.ParseColor(item.MToolColor));
                        FontUtils.SetTextViewIcon(item.MToolType == ToolType.Color ? FontsIconFrameWork.FontAwesomeSolid : FontsIconFrameWork.FontAwesomeLight, holder.ImgToolIcon, item.MToolIcon);
                        holder.ImgToolIcon.SetTextColor(Color.ParseColor(item.MToolColor));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void Click_item(ToolModel item)
        {
            try
            {
                var check = MToolList.Where(a => a.MToolColor == AppSettings.MainColor).ToList();
                if (check.Count > 0)
                    foreach (var all in check)
                        all.MToolColor = "#212121";

                var click = MToolList.FirstOrDefault(a => a.MToolType == item.MToolType);
                if (click != null) click.MToolColor = AppSettings.MainColor;

                NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public ToolModel GetItem(int position)
        {
            return MToolList[position];
        }

        public override long GetItemId(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public override int GetItemViewType(int position)
        {
            try
            {
                return position;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public void OnClick(EditingToolsAdapterClickEventArgs args)
        {
            ItemClick?.Invoke(this, args);
        }

        public void OnLongClick(EditingToolsAdapterClickEventArgs args)
        {
            ItemLongClick?.Invoke(this, args);
        }
    }

    public class EditingToolsAdapterViewHolder : RecyclerView.ViewHolder
    {
        public EditingToolsAdapterViewHolder(View itemView, Action<EditingToolsAdapterClickEventArgs> clickListener,
            Action<EditingToolsAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            try
            {
                MainView = itemView;

                ImgToolIcon = MainView.FindViewById<TextView>(Resource.Id.imgToolIcon);
                TxtTool = MainView.FindViewById<TextView>(Resource.Id.txtTool);

                itemView.Click += (sender, e) => clickListener(new EditingToolsAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});
                itemView.LongClick += (sender, e) => longClickListener(new EditingToolsAdapterClickEventArgs
                    {View = itemView, Position = AdapterPosition});

                RegularTxt6 = Typeface.CreateFromAsset(itemView.Context.Assets, "SF-UI-Display-Regular.ttf");
                TxtTool.SetTypeface(RegularTxt6, TypefaceStyle.Normal);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #region Variables Basic

        public View MainView { get; }

        public TextView ImgToolIcon { get; private set; }
        public TextView TxtTool { get; private set; }

        public readonly Typeface RegularTxt6;

        #endregion
    }

    public class EditingToolsAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}