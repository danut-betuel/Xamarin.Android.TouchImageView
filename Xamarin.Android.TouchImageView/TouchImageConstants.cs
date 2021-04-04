namespace Xamarin.Android.TouchImageView
{
    public class TouchImageConstants
    {
        // before animating back to the min/max zoom boundary.
        public const float SUPER_MIN_MULTIPLIER = .75f;
        public const float SUPER_MAX_MULTIPLIER = 1.25f;
        public const int DEFAULT_ZOOM_TIME = 500;

        // If setMinZoom(AUTOMATIC_MIN_ZOOM), then we'll set the min scale to include the whole image.
        public const float AUTOMATIC_MIN_ZOOM = -1.0f;
    }
}