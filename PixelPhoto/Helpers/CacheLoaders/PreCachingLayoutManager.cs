using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Java.Lang;
using Exception = System.Exception;

namespace PixelPhoto.Helpers.CacheLoaders
{
    public sealed class PreCachingLayoutManager : LinearLayoutManager
    {
        public Context Context;
        public int ExtraLayoutSpace = -1;
        public int DefaultExtraLayoutSpace = 600;
        public OrientationHelper MOrientationHelper;

        public int MAdditionalAdjacentPrefetchItemCount;
        private PreCachingLayoutManager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public PreCachingLayoutManager(Activity context) : base(context)
        {
            Context = context;
            Init();
        }

        public PreCachingLayoutManager(Activity context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Context = context;
            Init();
        }

        public PreCachingLayoutManager(Activity context, int orientation, bool reverseLayout) : base(context, orientation, reverseLayout)
        {
            Context = context;
            Init();
        }

        public void Init()
        {
            MOrientationHelper = OrientationHelper.CreateOrientationHelper(this, Orientation);
            ItemPrefetchEnabled = true;
            InitialPrefetchItemCount = 20;
        }

        public void SetPreloadItemCount(int preloadItemCount)
        {
            if (preloadItemCount < 1)
            {
                throw new IllegalArgumentException("adjacentPrefetchItemCount must not smaller than 1!");
            }
            MAdditionalAdjacentPrefetchItemCount = preloadItemCount - 1;
        }

        public override void CollectAdjacentPrefetchPositions(int dx, int dy, RecyclerView.State state, ILayoutPrefetchRegistry layoutPrefetchRegistry)
        {
            try
            {
                base.CollectAdjacentPrefetchPositions(dx, dy, state, layoutPrefetchRegistry);

                int delta = Orientation == Horizontal ? dx : dy;
                if (ChildCount == 0 || delta == 0)
                    return;

                int layoutDirection = delta > 0 ? 1 : -1;
                View child = GetChildClosest(layoutDirection);
                int currentPosition = GetPosition(child) + layoutDirection;
                int scrollingOffset;

                if (layoutDirection == 1)
                {
                    scrollingOffset = MOrientationHelper.GetDecoratedEnd(child) - MOrientationHelper.EndAfterPadding;
                    for (int i = currentPosition + 1; i < currentPosition + MAdditionalAdjacentPrefetchItemCount + 1; i++)
                    {
                        if (i >= 0 && i < state.ItemCount)
                        {
                            layoutPrefetchRegistry.AddPosition(i, Java.Lang.Math.Max(0, scrollingOffset));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private View GetChildClosest(int layoutDirection)
        {
            return GetChildAt(layoutDirection == -1 ? 0 : ChildCount - 1);
        }
    }
}