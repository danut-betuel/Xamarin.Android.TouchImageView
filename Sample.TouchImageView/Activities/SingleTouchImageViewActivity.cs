using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Android.TouchImageView;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SingleTouchImageViewActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_single_touchimageview);

            var touchImageView = FindViewById<TouchImageView>(Resource.Id.imageSingle);
            var scrollPositionTextView = FindViewById<TextView>(Resource.Id.scroll_position);
            var zoomedRectTextView = FindViewById<TextView>(Resource.Id.zoomed_rect);
            var currentZoomTextView = FindViewById<TextView>(Resource.Id.current_zoom);

            touchImageView.TouchMoveImageViewAction = () =>
            {
                var point = touchImageView.ScrollPosition;
                var rect = touchImageView.ZoomedRect;
                var currentZoom = touchImageView.CurrentZoom;
                var isZoomed = touchImageView.IsZoomed;
                scrollPositionTextView.Text = $"x: {point.X:#.##} y: {point.Y:#.##}";
                zoomedRectTextView.Text = $"left: {rect.Left:#.##} top: {rect.Top:#.##} \nright: {rect.Right:#.##} + bottom: {rect.Bottom:#.##}";
                currentZoomTextView.Text = $"getCurrentZoom(): {currentZoom} isZoomed(): {isZoomed}";
            };
        }
    }
}