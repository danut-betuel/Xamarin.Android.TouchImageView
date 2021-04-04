using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Xamarin.Android.TouchImageView;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MirroringExampleActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_mirroring_example);

            var topImage = FindViewById<TouchImageView>(Resource.Id.topImage);
            var bottomImage = FindViewById<TouchImageView>(Resource.Id.bottomImage);

            topImage.TouchMoveImageViewAction = () => bottomImage.SetZoom(topImage);
            bottomImage.TouchMoveImageViewAction = () => topImage.SetZoom(bottomImage);
        }
    }
}