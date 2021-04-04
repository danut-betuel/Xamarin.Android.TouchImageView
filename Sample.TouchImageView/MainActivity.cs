using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;
using Sample.Activities;

namespace Sample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region private fields

        private Button mSingleTouchImageViewButton;
        private Button mViewpagerExampleButton;
        private Button mViewpager2ExampleButton;
        private Button mMirrorTouchimageviewButton;
        private Button mSwitchImageButton;
        private Button mSwitchScaletypeButton;
        private Button mResizeButton;
        private Button mRecyclerButton;
        private Button mAnimateButton;
        private Button mGlideButton;
        private Button mShapedImageButton;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            InitViews();

            mSingleTouchImageViewButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(SingleTouchImageViewActivity))); };
            mViewpagerExampleButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(ViewPagerExampleActivity))); };
            mViewpager2ExampleButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(ViewPager2ExampleActivity))); };
            mMirrorTouchimageviewButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(MirroringExampleActivity))); };
            mSwitchImageButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(SwitchImageExampleActivity))); };
            mSwitchScaletypeButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(SwitchScaleTypeExampleActivity))); };
            mResizeButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(ChangeSizeExampleActivity))); };
            mRecyclerButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(RecyclerExampleActivity))); };
            mAnimateButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(AnimateZoomActivity))); };
            mGlideButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(GlideExampleActivity))); };
            mShapedImageButton.Click += delegate { StartActivity(new Android.Content.Intent(this, typeof(ShapedExampleActivity))); };
        }

        private void InitViews()
        {
            mSingleTouchImageViewButton = FindViewById<Button>(Resource.Id.single_touchimageview_button);
            mViewpagerExampleButton = FindViewById<Button>(Resource.Id.viewpager_example_button);
            mViewpager2ExampleButton = FindViewById<Button>(Resource.Id.viewpager2_example_button);
            mMirrorTouchimageviewButton = FindViewById<Button>(Resource.Id.mirror_touchimageview_button);
            mSwitchImageButton = FindViewById<Button>(Resource.Id.switch_image_button);
            mSwitchScaletypeButton = FindViewById<Button>(Resource.Id.switch_scaletype_button);
            mResizeButton = FindViewById<Button>(Resource.Id.resize_button);
            mRecyclerButton = FindViewById<Button>(Resource.Id.recycler_button);
            mAnimateButton = FindViewById<Button>(Resource.Id.animate_button);
            mGlideButton = FindViewById<Button>(Resource.Id.glide_button);
            mShapedImageButton = FindViewById<Button>(Resource.Id.shaped_image_button);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}