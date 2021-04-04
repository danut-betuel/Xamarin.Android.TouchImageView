using static Android.Widget.ImageView;

namespace Xamarin.Android.TouchImageView
{
    public class ZoomVariables
    {
        public ZoomVariables(float scale, float focusX, float focusY, ScaleType scaleType)
        {
            Scale = scale;
            FocusX = focusX;
            FocusY = focusY;
            ScaleType = scaleType;
        }

        public float Scale { get; set; }
        public float FocusX { get; set; }
        public float FocusY { get; set; }
        public ScaleType ScaleType { get; set; }
    }
}