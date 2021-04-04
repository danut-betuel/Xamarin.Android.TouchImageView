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
    public class SwitchImageExampleActivity : AppCompatActivity
    {
        private int index = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_switch_image_example);

            var imageSwitch = FindViewById<TouchImageView>(Resource.Id.imageSwitch);
            var imageSwitchButton = FindViewById<TextView>(Resource.Id.switch_image);

            if(savedInstanceState != null)
            {
                index = savedInstanceState.GetInt("index");
            }
            imageSwitch.SetImageResource(ImagesConstants.Images[index]);

            imageSwitchButton.Click += delegate 
            {
                index = (index + 1) % ImagesConstants.Images.Length;
                imageSwitch.SetImageResource(ImagesConstants.Images[index]);
            };
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt("index", index);
        }
    }
}