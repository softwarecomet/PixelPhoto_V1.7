using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Com.Gigamole.Navigationtabbar.Ntb;
using PixelPhoto.Activities.Tabbes;
using PixelPhoto.Helpers.Ads;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace PixelPhoto.Helpers.Utils
{
    public class FragmentBottomNavigationView
    {
        public HomeActivity Context;

        public JavaList<NavigationTabBar.Model> Models;
        public List<Fragment> FragmentListTab0 = new List<Fragment>();
        public List<Fragment> FragmentListTab1 = new List<Fragment>();
        public List<Fragment> FragmentListTab3 = new List<Fragment>();
        public List<Fragment> FragmentListTab4 = new List<Fragment>();

        public int PageNumber;

        public FragmentBottomNavigationView(Activity context)
        {
            Context = (HomeActivity)context;
        }

        public void SetupNavigation(NavigationTabBar navigationTabBar)
        {
            try
            {
                Models = new JavaList<NavigationTabBar.Model>
                {
                    new NavigationTabBar.Model.Builder(ContextCompat.GetDrawable(Context, Resource.Drawable.ic_tab_home),Color.ParseColor("#ffffff")).Title(Context.GetText(Resource.String.Lbl_News_Feed)).Build(),
                    new NavigationTabBar.Model.Builder(ContextCompat.GetDrawable(Context, Resource.Drawable.ic_explore_tool),Color.ParseColor("#ffffff")).Title(Context.GetString(Resource.String.Lbl_Explore)).Build(),
                    new NavigationTabBar.Model.Builder(ContextCompat.GetDrawable(Context, Resource.Drawable.ic_add),Color.ParseColor("#ffffff")).Title(Context.GetString(Resource.String.Lbl_Add_Post)).Build(),
                    new NavigationTabBar.Model.Builder(ContextCompat.GetDrawable(Context, Resource.Drawable.ic_action_notification),Color.ParseColor("#ffffff")).Title(Context.GetText(Resource.String.Lbl_Notifications)).BadgeTitle("0").Build(),
                    new NavigationTabBar.Model.Builder(ContextCompat.GetDrawable(Context, Resource.Drawable.ic_tab_user_profile),Color.ParseColor("#ffffff")).Title(Context.GetText(Resource.String.Lbl_More)).Build()
                };
                 
                var eee = NavigationTabBar.BadgeGravity.Top;
                navigationTabBar.SetBadgeGravity(eee);
                navigationTabBar.BadgeBgColor = Color.ParseColor(AppSettings.MainColor);
                navigationTabBar.BadgeTitleColor = Color.White;
             
                if (AppSettings.SetTabColoredTheme)
                {
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_News_Feed)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Explore)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Notifications)).Color = Color.ParseColor(AppSettings.TabColoredColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_More)).Color = Color.ParseColor(AppSettings.TabColoredColor);

                    navigationTabBar.BgColor = Color.ParseColor(AppSettings.MainColor);
                    navigationTabBar.ActiveColor = Color.White;
                    navigationTabBar.InactiveColor = Color.White;
                }
                else if (AppSettings.SetTabDarkTheme)
                {
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_News_Feed)).Color = Color.ParseColor(AppSettings.MainColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Explore)).Color = Color.ParseColor(AppSettings.MainColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor(AppSettings.MainColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Notifications)).Color = Color.ParseColor(AppSettings.MainColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_More)).Color = Color.ParseColor(AppSettings.MainColor);
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor(AppSettings.MainColor);

                    navigationTabBar.BgColor = Color.ParseColor("#000000");
                    //navigationTabBar.BgColor = Color.ParseColor("#282828");
                    navigationTabBar.ActiveColor = Color.White;
                    navigationTabBar.InactiveColor = Color.White;
                }
                else
                {
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_News_Feed)).Color = Color.ParseColor("#ffffff");
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Explore)).Color = Color.ParseColor("#ffffff");
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor("#ffffff");
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Notifications)).Color = Color.ParseColor("#ffffff");
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_More)).Color = Color.ParseColor("#ffffff");
                    Models.First(a => a.Title == Context.GetText(Resource.String.Lbl_Add_Post)).Color = Color.ParseColor("#ffffff");


                    navigationTabBar.BgColor = Color.White;
                    navigationTabBar.ActiveColor = Color.ParseColor(AppSettings.MainColor);
                    navigationTabBar.InactiveColor = Color.ParseColor("#bfbfbf");
                }
                navigationTabBar.Models = Models;
                

                navigationTabBar.IsScaled = false;
                navigationTabBar.IconSizeFraction = (float)0.450;
                navigationTabBar.BehaviorEnabled = false;
                //navigationTabBar.SetBadgePosition(NavigationTabBar.BadgePosition.Center);
                if (AppSettings.SetTabIsTitledWithText)
                {
                    navigationTabBar.SetTitleMode(NavigationTabBar.TitleMode.All);
                    navigationTabBar.IsTitled = true;
                }

                navigationTabBar.StartTabSelected += NavigationTabBarOnStartTabSelected;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void NavigationTabBarOnStartTabSelected(object sender, NavigationTabBar.StartTabSelectedEventArgs e)
        {
            try
            {
                switch (e.P1)
                {
                    case 0:
                        PageNumber = 0;
                        ShowFragment0();
                        AdsGoogle.Ad_Interstitial(Context);
                        break; 
                    case 1:
                        PageNumber = 1;
                        ShowFragment1();
                        AdsGoogle.Ad_Interstitial(Context);
                        break; 
                    case 2:
                        PageNumber = 2;
                        Context.RunOnUiThread(() =>
                        {
                            try
                            {
                                if (!Context.CircleMenu.IsOpened)
                                {
                                    Context.CircleMenu.Visibility = ViewStates.Visible;
                                    Context.CircleMenu.OpenMenu();
                                }
                                else
                                {
                                    Context.CircleMenu.CloseMenu();
                                    Context.CircleMenu.Visibility = ViewStates.Gone;
                                }
                            }
                            catch (Exception exception)
                            {
                                Console.WriteLine(exception);
                            }
                        }); 
                        break; 
                    case 3:
                        PageNumber = 3; 
                        var dataTab = Models.FirstOrDefault(a => a.Title == Context.GetText(Resource.String.Lbl_Notifications));
                        dataTab?.HideBadge();
                        ShowFragment3();
                        AdsGoogle.Ad_RewardedVideo(Context);
                        break;  
                    case 4:
                        PageNumber = 4;
                        ShowFragment4();
                        AdsGoogle.Ad_RewardedVideo(Context);
                        break; 
                    default:
                        PageNumber = 0;
                        ShowFragment0(); 
                        break;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception); 
            } 
        }

        public int GetCountFragment()
        {
            try
            {
                switch (PageNumber)
                {
                    case 0:
                        return FragmentListTab0.Count > 1 ? FragmentListTab0.Count : 0;
                    case 1:
                        return FragmentListTab1.Count > 1 ? FragmentListTab1.Count : 0;
                    case 3:
                        return FragmentListTab3.Count > 1 ? FragmentListTab4.Count : 0;
                    case 4:
                        return FragmentListTab4.Count > 1 ? FragmentListTab4.Count : 0;
                    default:
                        return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        }

        public static void HideFragmentFromList(List<Fragment> fragmentList, FragmentTransaction ft)
        {
            try
            {
                if (fragmentList.Count > 0)
                {
                    foreach (var fra in fragmentList.Where(fra => fra.IsAdded && fra.IsVisible))
                    {
                        ft.Hide(fra);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e); 
            } 
        }

        public Fragment GetSelectedTabBackStackFragment()
        {
            switch (PageNumber)
            {
                case 0:
                    {
                        var currentFragment = FragmentListTab0[FragmentListTab0.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                case 1:
                    {
                        var currentFragment = FragmentListTab1[FragmentListTab1.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                case 3:
                    {
                        var currentFragment = FragmentListTab3[FragmentListTab3.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }
                case 4:
                    {
                        var currentFragment = FragmentListTab4[FragmentListTab4.Count - 2];
                        if (currentFragment != null)
                            return currentFragment;
                        break;
                    }

                default:
                        return null;
                   
            }

            return null;
        }

        public Fragment GetSelectedTabLastStackFragment()
        {
            switch (PageNumber)
            {
                case 0:
                {
                    var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                    if (currentFragment != null)
                        return currentFragment;
                    break;
                }
                case 1:
                {
                    var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                    if (currentFragment != null)
                        return currentFragment;
                    break;
                }
                case 3:
                {
                    var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                    if (currentFragment != null)
                        return currentFragment;
                    break;
                }
                case 4:
                {
                    var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                    if (currentFragment != null)
                        return currentFragment;
                    break;
                }

                default:
                    return null;

            }

            return null;
        }


        public void DisplayFragment(Fragment newFragment)
        {
            try
            {
                FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();
            
                HideFragmentFromList(FragmentListTab0, ft);
                HideFragmentFromList(FragmentListTab1, ft);
                HideFragmentFromList(FragmentListTab3, ft);
                HideFragmentFromList(FragmentListTab4, ft);

                if (PageNumber == 0)
                    if (!FragmentListTab0.Contains(newFragment))
                        FragmentListTab0.Add(newFragment);

                if (PageNumber == 1)
                    if (!FragmentListTab1.Contains(newFragment))
                        FragmentListTab1.Add(newFragment);

                if (PageNumber == 3)
                    if (!FragmentListTab3.Contains(newFragment))
                        FragmentListTab3.Add(newFragment);

                if (PageNumber == 4)
                    if (!FragmentListTab4.Contains(newFragment))
                        FragmentListTab4.Add(newFragment);

                if (!newFragment.IsAdded)
                    ft.Add(Resource.Id.content, newFragment, PageNumber + newFragment.Id.ToString());
                else
                    ft.Show(newFragment);

                ft.CommitNow();
                Context.SupportFragmentManager.ExecutePendingTransactions();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
               
            }
            
           
           
        }


        public void RemoveFragment(Fragment oldFragment)
        {
            FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

            if (PageNumber == 0)
                if (FragmentListTab0.Contains(oldFragment))
                    FragmentListTab0.Remove(oldFragment);

            if (PageNumber == 1)
                if (FragmentListTab1.Contains(oldFragment))
                    FragmentListTab1.Remove(oldFragment);

            if (PageNumber == 3)
                if (FragmentListTab3.Contains(oldFragment))
                    FragmentListTab3.Remove(oldFragment);

            if (PageNumber == 4)
                if (FragmentListTab4.Contains(oldFragment))
                    FragmentListTab4.Remove(oldFragment);


            HideFragmentFromList(FragmentListTab0, ft);
            HideFragmentFromList(FragmentListTab1, ft);
            HideFragmentFromList(FragmentListTab3, ft);
            HideFragmentFromList(FragmentListTab4, ft);

            if (oldFragment.IsAdded)
                ft.Remove(oldFragment);

            if (PageNumber == 0)
            {
                var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                ft.Show(currentFragment).Commit();
            }
            else if (PageNumber == 1)
            {
                var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                ft.Show(currentFragment).Commit();
            }
            else if (PageNumber == 3)
            {
                var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                ft.Show(currentFragment).Commit();
            }
            else if (PageNumber == 4)
            {
                var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                ft.Show(currentFragment).Commit();
            }
        }

        public void OnBackStackClickFragment()
        {
            if (PageNumber == 0)
            {
                if (FragmentListTab0.Count > 1)
                {
                    var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                    if (currentFragment != null)
                        RemoveFragment(currentFragment);
                }
                else
                {
                    Context.Finish();
                }
            }
            else if (PageNumber == 1)
            {
                if (FragmentListTab1.Count > 1)
                {
                    var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
                    if (currentFragment != null)
                        RemoveFragment(currentFragment);
                }
                else
                {
                    Context.Finish();
                }

            }
            else if (PageNumber == 3)
            {
                if (FragmentListTab3.Count > 1)
                {
                    var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
                    if (currentFragment != null)
                        RemoveFragment(currentFragment);
                }
                else
                {
                    Context.Finish();
                }
            }
            else if (PageNumber == 4)
            {

                if (FragmentListTab4.Count > 1)
                {
                    var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
                    if (currentFragment != null)
                        RemoveFragment(currentFragment);
                }
                else
                {
                    Context.Finish();
                }
            }

        }

        public void ShowFragment0()
        {
            try
            {
                if (FragmentListTab0.Count < 0) return;

                // If user presses it while still on that tab it removes all fragments from the list
                if (FragmentListTab0.Count > 1)
                {
                    FragmentTransaction ft = Context.SupportFragmentManager.BeginTransaction();

                    for (var index = FragmentListTab0.Count - 1 ; index > 0; index--)
                    {
                        var oldFragment = FragmentListTab0[index];
                        if (FragmentListTab0.Contains(oldFragment))
                            FragmentListTab0.Remove(oldFragment);

                        if (oldFragment.IsAdded)
                            ft.Remove(oldFragment);

                        HideFragmentFromList(FragmentListTab0, ft);
                    }
                     
                    var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                    ft.Show(currentFragment).Commit();

                    Context.NewsFeedFragment.OnRefresh();

                }
                else
                {
                    var currentFragment = FragmentListTab0[FragmentListTab0.Count - 1];
                    if (currentFragment != null)
                        DisplayFragment(currentFragment);
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void ShowFragment1()
        {
            if (FragmentListTab1.Count < 0) return;
            var currentFragment = FragmentListTab1[FragmentListTab1.Count - 1];
            if (currentFragment != null)
                DisplayFragment(currentFragment);
        }

        public void ShowFragment3()
        {
            if (FragmentListTab3.Count < 0) return;
            var currentFragment = FragmentListTab3[FragmentListTab3.Count - 1];
            if (currentFragment != null)
                DisplayFragment(currentFragment);
        }

        public void ShowFragment4()
        {
            if (FragmentListTab4.Count < 0) return;
            var currentFragment = FragmentListTab4[FragmentListTab4.Count - 1];
            if (currentFragment != null)
                DisplayFragment(currentFragment);
        }


    }
}