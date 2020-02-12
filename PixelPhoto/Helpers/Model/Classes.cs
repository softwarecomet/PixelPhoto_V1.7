namespace PixelPhoto.Helpers.Model
{
    public class Classes
    {
        public class ViewPagerStrings
        {
            public string Description;
            public string Header;
        }

        public class PostType
        {
            public int Id { get; set; }
            public string TypeText { get; set; }
            public int Image { get; set; }
            public string ImageColor { get; set; }
        }

        public enum TypePostEnum
        {
            Image,
            Video,
            Mention,
            Camera,
            Gif,
            EmbedVideo
        }
    }
}