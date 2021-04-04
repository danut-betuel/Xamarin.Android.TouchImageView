using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager2.Widget;
using Sample.Adapters;
using Sample.Constants;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ViewPager2ExampleActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_viewpager2_example);

            var viewPager2 = FindViewById<ViewPager2>(Resource.Id.view_pager2);
            viewPager2.Adapter = new AdapterImages(ImagesConstants.Images);
        }
    }
}