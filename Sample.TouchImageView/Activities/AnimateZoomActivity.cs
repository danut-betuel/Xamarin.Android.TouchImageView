using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Android.TouchImageView;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AnimateZoomActivity : AppCompatActivity
    {
        #region private fields

        private TextView mCurrentZoom;
        private TextView mScrollPosition;
        private TouchImageView mImageSingle;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_single_touchimageview);
            InitViews();

            mCurrentZoom.Click += delegate 
            {
                if (mImageSingle.IsZoomed)
                {
                    mImageSingle.ResetZoomAnimated();
                }
                else
                {
                    mImageSingle.SetZoomAnimated(3f, 0.75f, 0.75f, OnZoomFinished);
                }
            };

            mImageSingle.SetZoom(1.1f, 0f, 0f);
        }

        private void InitViews()
        {
            mCurrentZoom = FindViewById<TextView>(Resource.Id.current_zoom);
            mScrollPosition = FindViewById<TextView>(Resource.Id.scroll_position);
            mImageSingle = FindViewById<TouchImageView>(Resource.Id.imageSingle);
        }

        public void OnZoomFinished()
        {
            mScrollPosition.Text = "Zoom done";
        }
    }
}