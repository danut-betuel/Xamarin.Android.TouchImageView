using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using Bumptech.Glide;
using Bumptech.Glide.Request.Target;
using Xamarin.Android.TouchImageView;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class GlideExampleActivity : AppCompatActivity
    {
        private const string GLIDE_IMAGE_URL = "https://raw.githubusercontent.com/bumptech/glide/master/static/glide_logo.png";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_glide);

            var touchImageView = FindViewById<TouchImageView>(Resource.Id.imageGlide);
            Glide.With(this).Load(GLIDE_IMAGE_URL).Placeholder(Resource.Drawable.nature_1).Into(new DrawableImageViewTarget(touchImageView));
        }
    }
}