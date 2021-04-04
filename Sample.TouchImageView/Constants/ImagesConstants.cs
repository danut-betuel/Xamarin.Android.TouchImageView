using static Android.Widget.ImageView;

namespace Sample.Constants
{
    public class ImagesConstants
    {

        public readonly static ScaleType[] ScaleTypes = new[] { ScaleType.Center, ScaleType.CenterCrop, ScaleType.FitStart, // stand-in for CENTER with initial zoom that looks like FIT_CENTER
                                                                  ScaleType.FitEnd, // stand-in for CENTER with initial zoom that looks like CENTER_CROP
                                                                  ScaleType.CenterInside, ScaleType.FitXy, ScaleType.FitCenter };

        public readonly static int[] Images = new[] { Resource.Drawable.nature_1, Resource.Drawable.nature_2, Resource.Drawable.nature_3, Resource.Drawable.nature_4,
                                                        Resource.Drawable.nature_5, Resource.Drawable.nature_6, Resource.Drawable.nature_7, Resource.Drawable.nature_8 };
    }
}