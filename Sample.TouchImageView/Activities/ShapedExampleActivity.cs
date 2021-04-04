using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using Xamarin.Android.TouchImageView;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ShapedExampleActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_shaped_example);

            var touchImageView = FindViewById<TouchImageView>(Resource.Id.image_view);

            touchImageView.OutlineProvider = new OutlineProvider();
            touchImageView.ClipToOutline = true;
        }
    }

    public class OutlineProvider : ViewOutlineProvider
    {
        public override void GetOutline(View view, Outline outline)
        {
            outline.SetRoundRect(
                           0,
                           0,
                           view.Width,
                           view.Height,
                           view.Resources.GetDimension(Resource.Dimension.grid_2));
        }
    }
}