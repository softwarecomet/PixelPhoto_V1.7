using System;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using PixelPhoto.Activities.Editor.Adapters;
using PixelPhoto.NiceArt;
using static Android.Support.Design.Widget.BottomSheetBehavior;

namespace PixelPhoto.Activities.Editor.Tools
{
    public class EmojisFragment : BottomSheetDialogFragment
    {
        private readonly BottomSheetCallback MBottomSheetBehaviorCallback = new MyBottomSheetCallBack();
        private readonly NiceArtEditor NiceArtEditor;
        private EmojisAdapter EmojiAdapter;

        public EmojisFragment(NiceArtEditor mNiceArtEditor)
        {
            // Required empty public constructor
            NiceArtEditor = mNiceArtEditor;
        }

        public override void SetupDialog(Dialog dialog, int style)
        {
            try
            {
                base.SetupDialog(dialog, style);
                var contentView = View.Inflate(Activity, Resource.Layout.sticker_emoji_dialog, null);
                dialog.SetContentView(contentView);
                var Params = (CoordinatorLayout.LayoutParams) ((View) contentView.Parent).LayoutParameters;
                var behavior = Params.Behavior;

                if (behavior != null && behavior.GetType() == typeof(BottomSheetBehavior))
                    ((BottomSheetBehavior) behavior).SetBottomSheetCallback(MBottomSheetBehaviorCallback);

                //((View)contentView.Parent).SetBackgroundColor(Resources.GetColor(Color.Transparent));
                var rvEmoji = contentView.FindViewById<RecyclerView>(Resource.Id.rvEmoji);

                var gridLayoutManager = new GridLayoutManager(Context, 5);
                rvEmoji.SetLayoutManager(gridLayoutManager);
                EmojiAdapter = new EmojisAdapter(Activity);
                EmojiAdapter.ItemClick += EmojisAdapterOnItemClick;
                rvEmoji.SetAdapter(EmojiAdapter);
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

        private void EmojisAdapterOnItemClick(object sender, EmojisAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                if (position >= 0)
                {
                    var item = EmojiAdapter.GetItem(position);
                    if (item != null)
                    {
                        NiceArtEditor.AddEmojis(item);
                        Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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