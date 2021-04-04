using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using Sample.Constants;
using Xamarin.Android.TouchImageView;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SwitchScaleTypeExampleActivity : AppCompatActivity
    {
        private int index = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_switch_scaletype_example);

            var imageScale = FindViewById<TouchImageView>(Resource.Id.imageScale);
            var imageScaleButton = FindViewById<TextView>(Resource.Id.switch_scaletype);

            imageScaleButton.Click += delegate
            {
                index = ++index % ImagesConstants.ScaleTypes.Length;
                var currScaleType = ImagesConstants.ScaleTypes[index];
                imageScale.SetScaleType(currScaleType);
                Toast.MakeText(this, $"ScaleType: {currScaleType}", ToastLength.Short).Show();
            };
        }
    }
}