﻿using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using PixelPhoto.Activities.Editor.Adapters;
using PixelPhoto.Helpers.Model.Editor;
using PixelPhoto.NiceArt;
using static Android.Support.Design.Widget.BottomSheetBehavior;

namespace PixelPhoto.Activities.Editor.Tools
{
    public class ColorFragment : BottomSheetDialogFragment
    {
        private readonly BottomSheetCallback MBottomSheetBehaviorCallback = new MyBottomSheetCallBack();
        private readonly NiceArtEditor NiceArtEditor;

        private ColorPickerAdapter PickerAdapter;
        private readonly EditColorActivity ColorActivity;

        public ColorFragment(NiceArtEditor mNiceArtEditor , EditColorActivity colorActivity)
        {
            // Required empty public constructor
            NiceArtEditor = mNiceArtEditor;
            ColorActivity = colorActivity;
        }

        public override void SetupDialog(Dialog dialog, int style)
        {
            try
            {
                base.SetupDialog(dialog, style);
                var contentView = View.Inflate(Context, Resource.Layout.sticker_emoji_dialog, null);
                dialog.SetContentView(contentView);
                var Params = (CoordinatorLayout.LayoutParams)((View)contentView.Parent).LayoutParameters;
                var behavior = Params.Behavior;

                if (behavior != null && behavior.GetType() == typeof(BottomSheetBehavior))
                    ((BottomSheetBehavior)behavior).SetBottomSheetCallback(MBottomSheetBehaviorCallback);

                var rvEmoji = contentView.FindViewById<RecyclerView>(Resource.Id.rvEmoji);

                var gridLayoutManager = new GridLayoutManager(Activity, 4);
                rvEmoji.SetLayoutManager(gridLayoutManager);
                PickerAdapter = new ColorPickerAdapter(Activity,ColorType.ColorNormal);
                PickerAdapter.ItemClick += PickerAdapterOnItemClick;
                rvEmoji.SetAdapter(PickerAdapter);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void OnStart()
        {
            try
            {
                base.OnStart();
                var dialog = Dialog;
                //Make dialog full screen with transparent background
                if (dialog != null)
                {
                    var width = ViewGroup.LayoutParams.MatchParent;
                    var height = ViewGroup.LayoutParams.MatchParent;
                    dialog.Window.SetLayout(width, height);
                    dialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Select Color Text
        private void PickerAdapterOnItemClick(object sender, ColorPickerAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position > -1)
                {
                    var item = PickerAdapter.GetItem(position);
                    if (item != null)
                    {
                        ColorActivity.MColorCode = item.ColorFirst;
                        ColorActivity.MAutoResizeEditText.SetTextColor(Color.ParseColor(item.ColorFirst));
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public class MyBottomSheetCallBack : BottomSheetCallback
        {
            public override void OnSlide(View bottomSheet, float slideOffset)
            {
                try
                {
                    //Sliding
                    if (slideOffset == StateHidden) Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            public override void OnStateChanged(View bottomSheet, int newState)
            {
                //State changed
            }
        }
    }
}