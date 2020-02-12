using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Youtube.Player;
using PixelPhoto.Activities.Tabbes;
using Plugin.Share;
using Plugin.Share.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AFollestad.MaterialDialogs;
using Android.Views.Animations;
using Java.Lang;
using Newtonsoft.Json;
using PixelPhoto.Activities.AddPost;
using PixelPhoto.Activities.Comment;
using PixelPhoto.Activities.Comment.Adapters;
using PixelPhoto.Activities.Posts.Adapters;
using PixelPhoto.Helpers.Fonts;
using PixelPhoto.Helpers.Utils;
using PixelPhotoClient.Classes.Global;
using PixelPhotoClient.GlobalClass;
using PixelPhotoClient.RestCalls;
using Exception = System.Exception;


namespace PixelPhoto.Activities.Posts.Listeners
{
    public class SocialIoClickListeners: Java.Lang.Object , MaterialDialog.IListCallback, MaterialDialog.ISingleButtonCallback
    {
        private readonly Activity MainContext;
        private string TypeDialog, NamePage;
        private MoreFeedClickEventArgs MoreFeedArgs;
         
        public SocialIoClickListeners(Activity context)
        {
            MainContext = context;
            TypeDialog = string.Empty;
        }

        //Add Like Or Remove 
        public void OnLikeNewsFeedClick(LikeNewsFeedClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("reaction.mp3");

                    var refs = SetLike(e.LikeButton);
                    e.NewsFeedClass.IsLiked = refs;
                    
                    var likeCount = e.View.FindViewById<TextView>(Resource.Id.Likecount);
                    if (likeCount != null)
                    {
                        string likes = MainContext.GetText(Resource.String.Lbl_Likes);
                        int count = 0;
                        if (!refs && e.NewsFeedClass.Likes == 0)
                        {
                            e.NewsFeedClass.Likes = 0;
                            likeCount.Text = "0" + " " + likes;
                        }
                        else if (!refs && e.NewsFeedClass.Likes > 0)
                        {
                            count = e.NewsFeedClass.Likes - 1;
                            likeCount.Text = count + " " + likes;
                            e.NewsFeedClass.Likes = count;
                        }
                        else if (refs)
                        {
                            count = e.NewsFeedClass.Likes + 1;
                            likeCount.Text = count + " " + likes;
                            e.NewsFeedClass.Likes = count;
                        }

                        var list = ((HomeActivity)MainContext).NewsFeedFragment?.NewsFeedAdapter?.PixelNewsFeedList;
                        var dataPost = list?.FirstOrDefault(a => a.PostId == e.NewsFeedClass.PostId);
                        if (dataPost != null)
                        {
                            dataPost.Likes = count;
                            dataPost.IsLiked = refs;
                            int index = list.IndexOf(dataPost);
                            //((HomeActivity)MainContext).NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(index,"like");
                        } 
                    }
                     
                    //Sent Api
                    RequestsAsync.Post.AddLikeOrRemove(e.NewsFeedClass.PostId.ToString()).ConfigureAwait(false);
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Add Remove From Favorite
        public void OnFavNewsFeedClick(FavNewsFeedClickEventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    var refs = SetFav(e.FavButton);
                    e.NewsFeedClass.IsSaved = refs;

                    if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                    {
                        string textFav = ((HomeActivity)MainContext).ProfileFragment?.TxtCountFav?.Text;
                        if (textFav != null)
                        {
                            if (!textFav.Contains("K") || !textFav.Contains("M"))
                            {
                                int count = Convert.ToInt32(textFav);
                                if (!refs)
                                {
                                    if (count > 0)
                                        count--;
                                    else
                                        count = 0;
                                }
                                else
                                    count++;

                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.Favourites = count.ToString();
                                    ((HomeActivity)MainContext).ProfileFragment.TxtCountFav.Text = count.ToString();
                                }
                            }
                        }
                    }
                    else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                    {
                        string textFav = ((HomeActivity)MainContext).TikProfileFragment?.TxtPostCount?.Text;
                        if (textFav != null)
                        {
                            if (!textFav.Contains("K") || !textFav.Contains("M"))
                            {
                                int count = Convert.ToInt32(textFav);
                                if (!refs)
                                {
                                    if (count > 0)
                                        count--;
                                    else
                                        count = 0;
                                }
                                else
                                    count++;

                                var dataUser = ListUtils.MyProfileList.FirstOrDefault();
                                if (dataUser != null)
                                {
                                    dataUser.Favourites = count.ToString();
                                    ((HomeActivity)MainContext).TikProfileFragment.TxtPostCount.Text = count.ToString();
                                }
                            }
                        }
                    }

                    

                    var list = ((HomeActivity) MainContext).NewsFeedFragment?.NewsFeedAdapter?.PixelNewsFeedList;
                    var dataPost = list?.FirstOrDefault(a => a.PostId == e.NewsFeedClass.PostId);
                    if (dataPost != null)
                    { 
                        dataPost.IsSaved = refs;
                        int index = list.IndexOf(dataPost);
                        //((HomeActivity)MainContext).NewsFeedFragment.NewsFeedAdapter.NotifyItemChanged(index, "favorite");
                    }
                     
                    //Sent Api
                    RequestsAsync.Post.AddRemoveFromFavorite(e.NewsFeedClass.PostId.ToString()).ConfigureAwait(false);
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Share
        public async void OnShareClick(ShareFeedClickEventArgs args)
        {
            try
            {
                if (!CrossShare.IsSupported)
                    return;
                 
                if (args.NewsFeedClass.Type == "image" || args.NewsFeedClass.Type == "video" || args.NewsFeedClass.Type == "gif")
                {
                    if (args.NewsFeedClass?.MediaSet.Count >= 1)
                    { 
                        string urlImage = args.NewsFeedClass.MediaSet[0].File;
                        ShareFileImplementation.Activity = MainContext;
                        await ShareFileImplementation.ShareRemoteFile(urlImage, urlImage.Split('/').Last(), MainContext.GetText(Resource.String.Lbl_Send_to)); 
                    } 
                } 
                else
                {
                    await CrossShare.Current.Share(new ShareMessage
                    {
                        Title = args.NewsFeedClass.Username,
                        Text = args.NewsFeedClass.Description,
                        Url = args.NewsFeedClass.Link
                    }); 
                } 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Open Comment Fragment
        public void OnCommentClick(CommentFeedClickEventArgs e, string nameFragment)
        {
            try
            {
                ((HomeActivity)e.View.Context).OpenCommentFragment(new ObservableCollection<CommentObject>(e.NewsFeedClass.Comments), e.NewsFeedClass.PostId.ToString(), nameFragment);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open Profile user 
        public void OnAvatarImageFeedClick(AvatarFeedClickEventArgs e , string namePage)
        {
            try
            {
                AppTools.OpenProfile((HomeActivity)e.View.Context, e.NewsFeedClass.UserData.UserId.ToString(), e.NewsFeedClass.UserData);  
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open liked Post user
        public void OnLikedPostClick(LikeNewsFeedClickEventArgs e)
        {
            try
            {
                if (e.NewsFeedClass.Likes > 0)
                {
                    Bundle bundle = new Bundle();
                    bundle.PutString("userinfo", JsonConvert.SerializeObject(e.NewsFeedClass));
                    bundle.PutString("PostId", e.NewsFeedClass.PostId.ToString());

                    LikesPostFragment fragment = new LikesPostFragment
                    {
                        Arguments = bundle
                    };

                    ((HomeActivity)e.View.Context).OpenFragment(fragment);
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_No_likes_yet), ToastLength.Short).Show();
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        //Open Comment Post user
        public void OnCommentPostClick(CommentFeedClickEventArgs e , string namePage)
        {
            try
            {
                if (e.NewsFeedClass.Comments.Count > 0)
                {
                    ((HomeActivity)e.View.Context).OpenCommentFragment(new ObservableCollection<CommentObject>(e.NewsFeedClass.Comments), e.NewsFeedClass.PostId.ToString(), namePage);
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_NoComments_TitleText), ToastLength.Short).Show();
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }
         
        //Event Show More :  DeletePost , EditPost , GoPost , Copy Link  , Report .. 
        public void OnMoreClick(MoreFeedClickEventArgs args, bool isShow = false, string namePage = "")
        {
            try
            {
                var dataUser = ListUtils.MyProfileList.FirstOrDefault();

                NamePage = namePage; 
                MoreFeedArgs = args;
                TextView moreIcon = args.View.FindViewById<TextView>(Resource.Id.moreicon);
                if (moreIcon != null)
                { 
                    var arrayAdapter = new List<string>();
                    var dialogList = new MaterialDialog.Builder(MainContext);
                    if (args.IsOwner)
                    {
                        arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_DeletePost));
                        arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_EditPost));
                    }

                    if (args.NewsFeedClass?.Boosted == "0" && dataUser?.IsPro == "1")
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_BoostPost));
                    else if (args.NewsFeedClass?.Boosted == "1" && dataUser?.IsPro == "1")
                        arrayAdapter.Add(MainContext.GetString(Resource.String.Lbl_UnBoostPost));
                     
                    if (isShow)
                        arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_GoToPost));

                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_ReportThisPost));
                    arrayAdapter.Add(MainContext.GetText(Resource.String.Lbl_Copy));

                    dialogList.Title(MainContext.GetText(Resource.String.Lbl_Post));
                    dialogList.Items(arrayAdapter);
                    dialogList.PositiveText(MainContext.GetText(Resource.String.Lbl_Close)).OnPositive(this);
                    dialogList.AlwaysCallSingleChoiceCallback();
                    dialogList.ItemsCallback(this).Build().Show(); 
                } 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void OnSelection(MaterialDialog p0, View p1, int itemId, ICharSequence itemString)
        {
            try
            {
                string text = itemString.ToString();
                if (text == MainContext.GetText(Resource.String.Lbl_DeletePost))
                {
                    OnMenuDeletePostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_EditPost))
                {
                    OnMenuEditPostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_GoToPost))
                {
                    OnMenuGoPostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_ReportThisPost))
                {
                    OnMenuReportPostOnClick(MoreFeedArgs);
                }
                else if (text == MainContext.GetText(Resource.String.Lbl_Copy))
                {
                    OnMenuCopyOnClick(MoreFeedArgs.NewsFeedClass);
                }
                else if (text == MainContext.GetString(Resource.String.Lbl_BoostPost) || text == MainContext.GetString(Resource.String.Lbl_UnBoostPost))
                {
                    BoostPostEvent(MoreFeedArgs.NewsFeedClass);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnClick(MaterialDialog p0, DialogAction p1)
        {
            try
            {
                if (TypeDialog == "DeletePost" )
                {
                    var home = (HomeActivity) MainContext;
                    if (p1 == DialogAction.Positive)
                    {
                        MainContext.RunOnUiThread(() =>
                        {
                            try
                            {
                                var list = home.NewsFeedFragment?.NewsFeedAdapter?.PixelNewsFeedList;
                                var dataPost = list?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId);
                                if (dataPost != null)
                                {
                                    int index = list.IndexOf(dataPost);
                                    if (index >= 0)
                                    {
                                        home.NewsFeedFragment.NewsFeedAdapter?.PixelNewsFeedList?.Remove(dataPost);
                                        home.NewsFeedFragment.NewsFeedAdapter?.NotifyItemRemoved(index);
                                    }
                                }

                                if (AppSettings.ProfileTheme == ProfileTheme.DefaultTheme)
                                {
                                    var dataPostProfile = home.ProfileFragment?.UserPostAdapter?.PostList?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId);
                                    if (dataPostProfile != null)
                                    {
                                        int index = home.ProfileFragment.UserPostAdapter.PostList.IndexOf(dataPostProfile);
                                        if (index >= 0)
                                        {
                                            home.ProfileFragment?.UserPostAdapter?.PostList.Remove(dataPostProfile);
                                            home.ProfileFragment?.UserPostAdapter?.NotifyItemRemoved(index);
                                        }
                                    }
                                }
                                else if (AppSettings.ProfileTheme == ProfileTheme.TikTheme)
                                {
                                    var dataPostProfile = home.TikProfileFragment?.MyPostTab?.MAdapter?.PostList?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId);
                                    if (dataPostProfile != null)
                                    {
                                        int index = home.TikProfileFragment.MyPostTab.MAdapter.PostList.IndexOf(dataPostProfile);
                                        if (index >= 0)
                                        {
                                            home.TikProfileFragment?.MyPostTab?.MAdapter?.PostList.Remove(dataPostProfile);
                                            home.TikProfileFragment?.MyPostTab?.MAdapter?.NotifyItemRemoved(index);
                                        }
                                    }
                                }

                                //Delete post from list
                                if (NamePage == "HashTags")
                                {
                                    //Delete post from list //TODO Wael
                                    //var dataPostHashTags = HashTagPostFragment.HashTagAdapter?.PixelNewsFeedList?.FirstOrDefault(a => a.PostId == MoreFeedArgs.NewsFeedClass.PostId);
                                    //if (dataPostHashTags != null)
                                    //{
                                    //    HashTagPostFragment.HashTagAdapter?.Remove(dataPostHashTags);
                                    //}
                                }
                                else if (NamePage == "GifPost" || NamePage == "ImagePost" || NamePage == "MultiImagePost" || NamePage == "VideoPost" || NamePage == "YoutubePost")
                                {
                                   home.FragmentNavigatorBack();
                                    home.NewsFeedFragment.NewsFeedAdapter?.NotifyDataSetChanged();
                                }
                                else if (NamePage == "NewsFeedPost")
                                {

                                }

                                //SqLiteDatabase dbDatabase = new SqLiteDatabase(); 
                                //dbDatabase.RemoveOneNewsFeedPost(MoreFeedArgs.NewsFeedClass.PostId);
                                //dbDatabase.Dispose();

                                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyDeleted), ToastLength.Short).Show();

                                //Sent Api >>
                                RequestsAsync.Post.DeletePosts(MoreFeedArgs.NewsFeedClass.PostId.ToString()).ConfigureAwait(false); 
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e); 
                            }
                        }); 
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
                else
                {
                    if (p1 == DialogAction.Positive)
                    {
                    }
                    else if (p1 == DialogAction.Negative)
                    {
                        p0.Dismiss();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //DeletePost
        public void OnMenuDeletePostOnClick(MoreFeedClickEventArgs feed)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "DeletePost";
                    MoreFeedArgs = feed;

                    var dialog = new MaterialDialog.Builder(MainContext).Theme(AppSettings.SetTabDarkTheme ? Theme.Dark : Theme.Light);
                    dialog.Title(MainContext.GetText(Resource.String.Lbl_DeletePost));
                    dialog.Content(MainContext.GetText(Resource.String.Lbl_AreYouSureDeletePost));
                    dialog.PositiveText(MainContext.GetText(Resource.String.Lbl_Yes)).OnPositive(this);
                    dialog.NegativeText(MainContext.GetText(Resource.String.Lbl_No)).OnNegative(this);
                    dialog.AlwaysCallSingleChoiceCallback();
                    dialog.ItemsCallback(this).Build().Show(); 
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Edit Post
        public void OnMenuEditPostOnClick(MoreFeedClickEventArgs feed)
        {
            try
            { 
                Intent intent = new Intent(MainContext, typeof(EditPostActivity));
                intent.PutExtra("IdPost", feed.NewsFeedClass.PostId.ToString());
                intent.PutExtra("TextPost", feed.NewsFeedClass.Description);
                MainContext.StartActivityForResult(intent,2250);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //ReportPost
        public void OnMenuReportPostOnClick(MoreFeedClickEventArgs feed)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    //Sent Api >>
                    RequestsAsync.Post.ReportPosts(feed.NewsFeedClass.PostId.ToString()).ConfigureAwait(false);
                    Toast.MakeText(MainContext,MainContext.GetText(Resource.String.Lbl_YourReportPost), ToastLength.Short).Show();
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Go to Post
        public void OnMenuGoPostOnClick(MoreFeedClickEventArgs feed)
        {
            try
            {
                ((HomeActivity) feed.View.Context).OpenNewsFeedItem(feed.NewsFeedClass);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Copy
        public void OnMenuCopyOnClick(PostsObject feed)
        {
            try
            {
                ClipboardManager clipboard = (ClipboardManager)MainContext.GetSystemService(Context.ClipboardService);
                ClipData clip = ClipData.NewPlainText("clipboard", feed.Link);
                clipboard.PrimaryClip = clip;

                Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_Text_copied), ToastLength.Short).Show(); 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //BoostPost 
        private async void BoostPostEvent(PostsObject item)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    item.Boosted = "1";
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_postSuccessfullyBoosted), ToastLength.Short).Show();
                    //Sent Api >>
                    var (apiStatus, respond) = await RequestsAsync.Post.BoostPost(item.PostId.ToString()).ConfigureAwait(false);
                    if (apiStatus == 200)
                    {
                        if (respond is AddBoostPostObject actionsObject)
                        {
                            item.Boosted = actionsObject.Code;
                        }
                    }
                }
                else
                {
                    Toast.MakeText(MainContext, MainContext.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void OnPlayYoutubeButtonClicked(YoutubeVideoClickEventArgs e)
        {
            try
            {
                if (AppSettings.StartYoutubeAsIntent)
                {
                    Intent intent = YouTubeStandalonePlayer.CreateVideoIntent((Activity)MainContext, AppSettings.YoutubeKey, e.NewsFeedClass.Youtube);  
                    ((Activity)MainContext).StartActivity(intent);
                }
                else
                {
                    ((HomeActivity)MainContext).OpenNewsFeedItem(e.NewsFeedClass);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void CommentReplyPostClick(CommentReplyClickEventArgs e)
        {
            try
            {
                Bundle bundle = new Bundle();
                bundle.PutString("CommentId", e.CommentObject.Id.ToString());
                bundle.PutString("CommentObject", JsonConvert.SerializeObject(e.CommentObject));
                var mFragment = new ReplyCommentFragment
                {
                    Arguments = bundle
                };

                ((HomeActivity)e.View.Context).OpenFragment(mFragment); 
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }
        }


        public bool SetLike(TextView likeButton)
        {
            try
            {
                CommentsAdapter.MyBounceInterpolator interpolator = new CommentsAdapter.MyBounceInterpolator(0.2, 20);

                var animationScale = AnimationUtils.LoadAnimation(MainContext, Resource.Animation.scale);

                animationScale.Interpolator = interpolator;
               
                if (likeButton.Tag.ToString() == "Liked")
                {
                    likeButton.StartAnimation(animationScale);
                    likeButton.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, likeButton, IonIconsFonts.IosHeartOutline);
                    likeButton.Tag = "Like";
                   
                    return false;
                }
                else
                {
                    likeButton.StartAnimation(animationScale);
                    likeButton.SetTextColor(Color.ParseColor("#ed4856"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, likeButton, IonIconsFonts.IosHeart);
                    likeButton.Tag = "Liked";
                    likeButton.StartAnimation(animationScale);
                    return true;
                }
               
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        }

        public bool SetFav(TextView favButton)
        {
            try
            {
                CommentsAdapter.MyBounceInterpolator interpolator = new CommentsAdapter.MyBounceInterpolator(0.2, 20);

                var animationScale = AnimationUtils.LoadAnimation(MainContext, Resource.Animation.scale);

                animationScale.Interpolator = interpolator;

                if (favButton.Tag.ToString() == "Added")
                {
                    favButton.StartAnimation(animationScale);
                    favButton.SetTextColor(AppSettings.SetTabDarkTheme ? Color.ParseColor("#888888") : Color.ParseColor("#444444"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, favButton, IonIconsFonts.IosStarOutline);
                    favButton.Tag = "Add";
                    return false;
                }
                else
                {
                    favButton.StartAnimation(animationScale);
                    favButton.SetTextColor(Color.ParseColor("#FFCE00"));
                    FontUtils.SetTextViewIcon(FontsIconFrameWork.IonIcons, favButton, IonIconsFonts.AndroidStar);
                    favButton.Tag = "Added";
                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return false;
            }
        } 
    }

    public class MoreFeedClickEventArgs
    {
        public View View { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public bool IsOwner { get; set; }
    }

    public class ShareFeedClickEventArgs
    {
        public View View { get; set; }
        public PostsObject NewsFeedClass { get; set; }
    }

    public class PlayVideoClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public Holders.VideoAdapterViewHolder Holder { get; set; }
    }

    public class YoutubeVideoClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public Holders.YoutubeAdapterViewHolder Holder { get; set; }
    }

    public class AvatarFeedClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public ImageView Image { get; set; }
    }

    public class FavNewsFeedClickEventArgs : EventArgs
    {
        public PostsObject NewsFeedClass { get; set; }
        public TextView FavButton { get; set; }
    }

    public class CommentFeedClickEventArgs
    {
        public View View { get; set; }
        public PostsObject NewsFeedClass { get; set; }
    }

    public class CommentReplyClickEventArgs
    {
        public View View { get; set; }
        public  int Position { get; set; }
        public CommentObject CommentObject { get; set; }
    }

    public class LikeNewsFeedClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public PostsObject NewsFeedClass { get; set; }
        public TextView LikeButton { get; set; }
    }
     
}