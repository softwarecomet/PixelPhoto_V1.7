using Android.Views;
using PixelPhotoClient.Classes.Messages;

namespace PixelPhoto.Helpers.Model
{
    public interface IOnClickListenerSelected
    {
        void OnItemClick(View view, GetChatsObject.Data obj, int pos);

        void OnItemLongClick(View view, GetChatsObject.Data obj, int pos);
    }
}