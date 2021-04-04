using Android.Animation;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Sample.Constants;
using Sample.Listeners;
using System;
using Xamarin.Android.TouchImageView;
using static Android.Views.View;
using static Android.Widget.ImageView;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme")]
    public class ChangeSizeExampleActivity : AppCompatActivity
    {
        #region private fields

        private FrameLayout mImageContainer;
        private TouchImageView mImageChangeSize;
        private Button mLeft;
        private Button mRight;
        private Button mUp;
        private Button mDown;
        private Button mResize;
        private Button mRotate;
        private Button mSwitchScaleType;
        private Button mSwitchImageButton;

        private ValueAnimator xSizeAnimator = new ValueAnimator();
        private ValueAnimator ySizeAnimator = new ValueAnimator();
        private int xSizeAdjustment = 0;
        private int ySizeAdjustment = 0;
        private int scaleTypeIndex = 0;
        private int imageIndex = 0;

        private SizeBehaviorAdjuster resizeAdjuster;
        private SizeBehaviorAdjuster rotateAdjuster;

        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_change_size);
            InitViews();

            mImageChangeSize.SetBackgroundColor(Color.LightGray);
            mImageChangeSize.MinZoom = TouchImageConstants.AUTOMATIC_MIN_ZOOM;
            mImageChangeSize.SetMaxZoomRatio(6.0f);

            mLeft.Click += delegate { AdjustImageSize(-1, 0); };
            mRight.Click += delegate { AdjustImageSize(1, 0); };
            mUp.Click += delegate { AdjustImageSize(0, -1); };
            mDown.Click += delegate { AdjustImageSize(0, 1); };

            resizeAdjuster = new SizeBehaviorAdjuster(mImageChangeSize, false, "resize: ");
            rotateAdjuster = new SizeBehaviorAdjuster(mImageChangeSize, true, "rotate: ");

            mResize.SetOnClickListener(resizeAdjuster);
            mRotate.SetOnClickListener(rotateAdjuster);

            mSwitchScaleType.Click += delegate
            {
                scaleTypeIndex = (scaleTypeIndex + 1) % ImagesConstants.ScaleTypes.Length;
                ProcessScaleType(ImagesConstants.ScaleTypes[scaleTypeIndex], true);
            };

            mSwitchImageButton.Click += delegate
            {
                imageIndex = (imageIndex + 1) % ImagesConstants.Images.Length;
                mImageChangeSize.SetImageResource(ImagesConstants.Images[imageIndex]);
            };

            if (savedInstanceState != null)
            {
                scaleTypeIndex = savedInstanceState.GetInt("scaleTypeIndex");
                resizeAdjuster.SetIndex(mResize, savedInstanceState.GetInt("resizeAdjusterIndex"));
                rotateAdjuster.SetIndex(mRotate, savedInstanceState.GetInt("rotateAdjusterIndex"));
                imageIndex = savedInstanceState.GetInt("imageIndex");
                mImageChangeSize.SetImageResource(ImagesConstants.Images[imageIndex]);
            }

            mImageChangeSize.Post(() => ProcessScaleType(ImagesConstants.ScaleTypes[scaleTypeIndex], false));
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutInt("scaleTypeIndex", scaleTypeIndex);
            outState.PutInt("resizeAdjusterIndex", resizeAdjuster.Index);
            outState.PutInt("rotateAdjusterIndex", rotateAdjuster.Index);
            outState.PutInt("imageIndex", imageIndex);
        }

        private void InitViews()
        {
            mImageContainer = FindViewById<FrameLayout>(Resource.Id.image_container);
            mImageChangeSize = FindViewById<TouchImageView>(Resource.Id.imageChangeSize);
            mLeft = FindViewById<Button>(Resource.Id.left);
            mRight = FindViewById<Button>(Resource.Id.right);
            mUp = FindViewById<Button>(Resource.Id.up);
            mDown = FindViewById<Button>(Resource.Id.down);
            mResize = FindViewById<Button>(Resource.Id.resize);
            mRotate = FindViewById<Button>(Resource.Id.rotate);
            mSwitchScaleType = FindViewById<Button>(Resource.Id.switch_scaletype_button);
            mSwitchImageButton = FindViewById<Button>(Resource.Id.switch_image_button);
        }

        private void AdjustImageSize(int dx, int dy)
        {
            var newXScale = Math.Min(0, xSizeAdjustment + dx);
            var newYScale = Math.Min(0, ySizeAdjustment + dy);
            if (newXScale == xSizeAdjustment && newYScale == ySizeAdjustment)
            {
                return;
            }
            xSizeAdjustment = newXScale;
            ySizeAdjustment = newYScale;

            var width = mImageContainer.MeasuredWidth * Math.Pow(1.1, xSizeAdjustment);
            var height = mImageContainer.MeasuredHeight * Math.Pow(1.1, ySizeAdjustment);
            xSizeAnimator.Cancel();
            ySizeAnimator.Cancel();
            xSizeAnimator = ValueAnimator.OfInt(mImageChangeSize.Width, (int)width);
            ySizeAnimator = ValueAnimator.OfInt(mImageChangeSize.Height, (int)height);
            xSizeAnimator.AddUpdateListener(new AnimatorUpdateListener((animation) =>
            {
                var lp = mImageChangeSize.LayoutParameters;
                lp.Width = (int)animation.AnimatedValue;
                mImageChangeSize.LayoutParameters = lp;
            }));

            ySizeAnimator.AddUpdateListener(new AnimatorUpdateListener((animation) =>
            {
                var lp = mImageChangeSize.LayoutParameters;
                lp.Height = (int)animation.AnimatedValue;
                mImageChangeSize.LayoutParameters = lp;
            }));

            xSizeAnimator.SetDuration(200);
            ySizeAnimator.SetDuration(200);
            xSizeAnimator.Start();
            ySizeAnimator.Start();
        }

        private void ProcessScaleType(ScaleType scaleType, bool resetZoom)
        {
            if (scaleType == ScaleType.FitEnd)
            {
                mSwitchScaleType.Text = ScaleType.Center.Name() + " (with " + ScaleType.CenterCrop.Name() + " zoom)";
                mImageChangeSize.SetScaleType(ScaleType.Center);
                if (resetZoom)
                {
                    var widthRatio = (float)mImageChangeSize.MeasuredWidth / mImageChangeSize.Drawable.IntrinsicWidth;
                    var heightRatio = (float)mImageChangeSize.MeasuredHeight / mImageChangeSize.Drawable.IntrinsicHeight;
                    mImageChangeSize.SetZoom(Math.Max(widthRatio, heightRatio));
                }
            }
            else if (scaleType == ScaleType.FitStart)
            {
                mSwitchScaleType.Text = ScaleType.Center.Name() + " (with " + ScaleType.FitCenter.Name() + " zoom)";
                mImageChangeSize.SetScaleType(ScaleType.Center);
                if (resetZoom)
                {
                    var widthRatio = (float)mImageChangeSize.MeasuredWidth / mImageChangeSize.Drawable.IntrinsicWidth;
                    var heightRatio = (float)mImageChangeSize.MeasuredHeight / mImageChangeSize.Drawable.IntrinsicHeight;
                    mImageChangeSize.SetZoom(Math.Min(widthRatio, heightRatio));
                }
            }
            else
            {
                mSwitchScaleType.Text = scaleType.Name();
                mImageChangeSize.SetScaleType(scaleType);
                if (resetZoom)
                {
                    mImageChangeSize.ResetZoom();
                }
            }
        }
    }

    public class SizeBehaviorAdjuster : Java.Lang.Object, IOnClickListener
    {
        private string[] mFixedPixelEnumNames = Enum.GetNames(typeof(FixedPixel));
        private readonly bool mForOrientationChanges;
        private readonly string mButtonPrefix;
        private readonly TouchImageView mTouchImageView;

        public int Index { get; private set; }

        public SizeBehaviorAdjuster(TouchImageView touchImageView, bool forOrientationChanges, string buttonPrefix)
        {
            mTouchImageView = touchImageView;
            mForOrientationChanges = forOrientationChanges;
            mButtonPrefix = buttonPrefix;
        }


        public void OnClick(View v)
        {
            if (v is Button button)
            {
                SetIndex(button, (Index + 1) % mFixedPixelEnumNames.Length);
            }
        }

        public void SetIndex(Button b, int index)
        {
            Index = index;
            if (mForOrientationChanges)
            {
                mTouchImageView.OrientationChange = (FixedPixel)index;
            }
            else
            {
                mTouchImageView.ViewSizeChange = (FixedPixel)index;
            }
            b.Text = mButtonPrefix + mFixedPixelEnumNames[index];
        }
    }
}