using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using PixelPhoto.Activities.Search.Adapters;
using PixelPhoto.Activities.Tabbes;
using System;
using System.Collections.ObjectModel;
using Android.Graphics;
using PixelPhoto.Activities.Posts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.User;
using Fragment = Android.Support.V4.App.Fragment;
 
namespace PixelPhoto.Activities.Search
{
    public class SearchHashtagFragment : Fragment
    {
        #region Variables Basic

        public SearchHashtagAdapter MAdapter;
        private HomeActivity GlobalContext;
        private SearchFragment ContextSearch;
        private SwipeRefreshLayout SwipeRefreshLayout;
        public RecyclerView MRecycler;
        private ProgressBar ProgressBarLoader;
        private LinearLayoutManager LayoutManager;
        public ViewStub EmptyStateLayout;
        public View Inflated;
         
        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                // Use this to return your custom view for this Fragment
                View view = inflater.Inflate(Resource.Layout.SearchUsersLayout, container, false);

                //Get Value 
                InitComponent(view);
                SetRecyclerViewAdapters();

                GlobalContext = (HomeActivity)Activity ?? HomeActivity.GetInstance();
                ContextSearch = (SearchFragment)ParentFragment;
                
                return view;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
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
         
        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
                ProgressBarLoader = (ProgressBar)view.FindViewById(Resource.Id.sectionProgress);

                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.search_swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));

                ProgressBarLoader.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MAdapter = new SearchHashtagAdapter(Activity)
                {
                    HashTagsList = new ObservableCollection<SearchUsersHastagsObject.Hash>()
                };
                LayoutManager = new LinearLayoutManager(Context);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MAdapter.ItemClick += MAdapterOnItemClick;
                MRecycler.SetAdapter(MAdapter); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, SearchHashtagAdapterAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position >= 0)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        // Show All Post By Hash
                        Bundle bundle = new Bundle();
                        bundle.PutString("HashId", item.Id.ToString());
                        bundle.PutString("HashName", Methods.FunString.DecodeString(item.Tag));

                        HashTagPostFragment profileFragment = new HashTagPostFragment
                        {
                            Arguments = bundle
                        };

                        GlobalContext.OpenFragment(profileFragment);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        #endregion 
    }
}