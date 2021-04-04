using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Java.Lang;
using System;
using Xamarin.Android.TouchImageView.Listeners;
using static Android.Views.GestureDetector;

namespace Xamarin.Android.TouchImageView
{
    [Register("xamarin.android.touchimageview.TouchImageView")]
    public class TouchImageView : AppCompatImageView
    {
        #region private fields

        private Context mContext;

        private Matrix mPrevMatrix;
        private bool mOrientationJustChanged;

        private float mUserSpecifiedMinScale = 0f;
        private bool mMaxScaleIsSetByMultiplier;
        private float mMaxScaleMultiplier = 0f;
        private float mSuperMinScale = 0f;
        private float mSuperMaxScale = 0f;

        private global::Android.Content.Res.Orientation mOrientation;
        private ScaleType mTouchScaleType;
        private bool mImageRenderedAtLeastOnce;
        private bool mOnDrawReady;
        private ZoomVariables mDelayedZoomVariables;

        // Size of view and previous view size (ie before rotation)

        private int mPrevViewWidth = 0;
        private int mPrevViewHeight = 0;

        // Size of image when it is stretched to fit view. Before and After rotation.
        private float mMatchViewWidth = 0f;
        private float mMatchViewHeight = 0f;
        private float mPrevMatchViewWidth = 0f;
        private float mPrevMatchViewHeight = 0f;

        #endregion

        #region public fields

        public TouchImageState State { get; set; }
        public float CurrentZoom { get; set; }
        public bool IsZoomEnabled { get; set; }
        public bool IsRotateImageToFitScreen { get; set; }
        public float ImageWidth => mMatchViewWidth * CurrentZoom;
        public float ImageHeight => mMatchViewHeight * CurrentZoom;
        public bool IsZoomed => CurrentZoom != 1f;
        public int ViewWidth { get; set; }
        public int ViewHeight { get; set; }
        public FixedPixel OrientationChange { get; set; } = FixedPixel.Center;
        public FixedPixel ViewSizeChange { get; set; } = FixedPixel.Center;
        public float DoubleTapScale { get; set; } = 0f;
        public float MaxScale { get; set; }
        public float MinScale { get; set; }
        public Action TouchMoveImageViewAction { get; set; }
        public IOnDoubleTapListener DoubleTapListener { get; set; }
        public IOnTouchListener UserTouchListener { get; set; }
        public Matrix TouchMatrix { get; set; }
        public float[] FloatMatrix { get; set; }
        public TouchImageFling Fling { get; set; }
        public ScaleGestureDetector ScaleDetector { get; set; }
        public GestureDetector GestureDetector { get; set; }

        public float MinZoom
        {
            get => MinScale;
            set
            {
                mUserSpecifiedMinScale = value;
                if (value == TouchImageConstants.AUTOMATIC_MIN_ZOOM)
                {
                    if (mTouchScaleType == ScaleType.Center || mTouchScaleType == ScaleType.CenterCrop)
                    {
                        var drawable = Drawable;
                        var drawableWidth = GetDrawableWidth(drawable);
                        var drawableHeight = GetDrawableHeight(drawable);
                        if (drawable != null && drawableWidth > 0 && drawableHeight > 0)
                        {
                            var widthRatio = (float)ViewWidth / drawableWidth;
                            var heightRatio = (float)ViewHeight / drawableHeight;
                            MinScale = mTouchScaleType == ScaleType.Center ? System.Math.Min(widthRatio, heightRatio) :
                                                                             System.Math.Min(widthRatio, heightRatio) / System.Math.Max(widthRatio, heightRatio);
                        }
                    }
                    else
                    {
                        MinScale = 1.0f;
                    }
                }
                else
                {
                    MinScale = mUserSpecifiedMinScale;
                }
                if (mMaxScaleIsSetByMultiplier)
                {
                    SetMaxZoomRatio(mMaxScaleMultiplier);
                }
                mSuperMinScale = TouchImageConstants.SUPER_MIN_MULTIPLIER * MinScale;
            }
        }

        public PointF ScrollPosition
        {
            get
            {
                if (Drawable == null)
                {
                    return new PointF(.5f, .5f);
                }

                var drawableWidth = GetDrawableWidth(Drawable);
                var drawableHeight = GetDrawableHeight(Drawable);
                var point = TransformCoordTouchToBitmap(ViewWidth / 2f, ViewHeight / 2f, true);
                point.X /= drawableWidth;
                point.Y /= drawableHeight;
                return point;
            }
        }

        public RectF ZoomedRect
        {
            get
            {
                if (mTouchScaleType == ScaleType.FitXy)
                {
                    throw new UnsupportedOperationException("getZoomedRect() not supported with FIT_XY");
                }
                var topLeft = TransformCoordTouchToBitmap(0f, 0f, true);
                var bottomRight = TransformCoordTouchToBitmap(ViewWidth, ViewHeight, true);
                var w = (float)GetDrawableWidth(Drawable);
                var h = (float)GetDrawableHeight(Drawable);
                return new RectF(topLeft.X / w, topLeft.Y / h, bottomRight.X / w, bottomRight.Y / h);
            }
        }

        #endregion

        #region ctors

        public TouchImageView(Context context) : base(context)
        {
            Init(context, null, 0);
        }

        public TouchImageView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context, attrs, 0);
        }

        public TouchImageView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init(context, attrs, defStyleAttr);
        }

        protected TouchImageView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        #endregion

        #region public methods

        // Reset zoom and translation to initial state.
        public void ResetZoom()
        {
            CurrentZoom = 1f;
            FitImageToView();
        }

        public void ResetZoomAnimated()
        {
            SetZoomAnimated(1f, 0.5f, 0.5f);
        }

        // Set zoom to the specified scale. Image will be centered by default.
        public void SetZoom(float scale)
        {
            SetZoom(scale, 0.5f, 0.5f);
        }

        /**
         * Set zoom to the specified scale. Image will be centered around the point
         * (focusX, focusY). These floats range from 0 to 1 and denote the focus point
         * as a fraction from the left and top of the view. For example, the top left
         * corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
         */
        public void SetZoom(float scale, float focusX, float focusY)
        {
            SetZoom(scale, focusX, focusY, mTouchScaleType);
        }

        /**
         * Set zoom to the specified scale. Image will be centered around the point
         * (focusX, focusY). These floats range from 0 to 1 and denote the focus point
         * as a fraction from the left and top of the view. For example, the top left
         * corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
         */
        public void SetZoom(float scale, float focusX, float focusY, ScaleType scaleType)
        {

            // setZoom can be called before the image is on the screen, but at this point,
            // image and view sizes have not yet been calculated in onMeasure. Thus, we should
            // delay calling setZoom until the view has been measured.
            if (!mOnDrawReady)
            {
                mDelayedZoomVariables = new ZoomVariables(scale, focusX, focusY, scaleType);
                return;
            }
            if (mUserSpecifiedMinScale == TouchImageConstants.AUTOMATIC_MIN_ZOOM)
            {
                MinZoom = TouchImageConstants.AUTOMATIC_MIN_ZOOM;
                if (CurrentZoom < MinScale)
                {
                    CurrentZoom = MinScale;
                }
            }
            if (scaleType != mTouchScaleType)
            {
                SetScaleType(scaleType);
            }
            ResetZoom();
            ScaleImage(scale, ViewWidth / 2f, ViewHeight / 2f, true);
            TouchMatrix.GetValues(FloatMatrix);
            FloatMatrix[Matrix.MtransX] = (ViewWidth - mMatchViewWidth) / 2 - focusX * (scale - 1) * mMatchViewWidth;
            FloatMatrix[Matrix.MtransY] = (ViewHeight - mMatchViewHeight) / 2 - focusY * (scale - 1) * mMatchViewHeight;
            TouchMatrix.SetValues(FloatMatrix);
            FixTrans();
            SavePreviousImageValues();
            ImageMatrix = TouchMatrix;
        }

        /**
         * Set zoom parameters equal to another TouchImageView. Including scale, position,
         * and ScaleType.
         */
        public void SetZoom(TouchImageView img)
        {
            var center = img.ScrollPosition;
            SetZoom(img.CurrentZoom, center.X, center.Y, img.GetScaleType());
        }

        public void ScaleImage(double deltaScale, float focusX, float focusY, bool stretchImageToSuper)
        {
            var deltaScaleLocal = deltaScale;
            float lowerScale;
            float upperScale;
            if (stretchImageToSuper)
            {
                lowerScale = mSuperMinScale;
                upperScale = mSuperMaxScale;
            }
            else
            {
                lowerScale = MinScale;
                upperScale = MaxScale;
            }
            var origScale = CurrentZoom;
            CurrentZoom *= (float)deltaScaleLocal;
            if (CurrentZoom > upperScale)
            {
                CurrentZoom = upperScale;
                deltaScaleLocal = upperScale / (double)origScale;
            }
            else if (CurrentZoom < lowerScale)
            {
                CurrentZoom = lowerScale;
                deltaScaleLocal = lowerScale / (double)origScale;
            }
            TouchMatrix.PostScale((float)deltaScaleLocal, (float)deltaScaleLocal, focusX, focusY);
            FixScaleTrans();
        }

        /**
        * This function will transform the coordinates in the touch event to the coordinate
        * system of the drawable that the imageview contain
        *
        * @param x            x-coordinate of touch event
        * @param y            y-coordinate of touch event
        * @param clipToBitmap Touch event may occur within view, but outside image content. True, to clip return value
        * to the bounds of the bitmap size.
        * @return Coordinates of the point touched, in the coordinate system of the original drawable.
        */
        public PointF TransformCoordTouchToBitmap(float x, float y, bool clipToBitmap)
        {
            TouchMatrix.GetValues(FloatMatrix);
            var origW = (float)Drawable.IntrinsicWidth;
            var origH = (float)Drawable.IntrinsicHeight;
            var transX = FloatMatrix[Matrix.MtransX];
            var transY = FloatMatrix[Matrix.MtransY];
            var finalX = (x - transX) * origW / ImageWidth;
            var finalY = (y - transY) * origH / ImageHeight;
            if (clipToBitmap)
            {
                finalX = System.Math.Min(System.Math.Max(finalX, 0f), origW);
                finalY = System.Math.Min(System.Math.Max(finalY, 0f), origH);
            }
            return new PointF(finalX, finalY);
        }

        /**
         * Inverse of transformCoordTouchToBitmap. This function will transform the coordinates in the
         * drawable's coordinate system to the view's coordinate system.
         *
         * @param bx x-coordinate in original bitmap coordinate system
         * @param by y-coordinate in original bitmap coordinate system
         * @return Coordinates of the point in the view's coordinate system.
         */
        public PointF TransformCoordBitmapToTouch(float bx, float by)
        {
            TouchMatrix.GetValues(FloatMatrix);
            var origW = (float)Drawable.IntrinsicWidth;
            var origH = (float)Drawable.IntrinsicHeight;
            var px = bx / origW;
            var py = by / origH;
            var finalX = FloatMatrix[Matrix.MtransX] + ImageWidth * px;
            var finalY = FloatMatrix[Matrix.MtransY] + ImageHeight * py;
            return new PointF(finalX, finalY);
        }

        /**
        * When transitioning from zooming from focus to zoom from center (or vice versa)
        * the image can become unaligned within the view. This is apparent when zooming
        * quickly. When the content size is less than the view size, the content will often
        * be centered incorrectly within the view. fixScaleTrans first calls fixTrans() and
        * then makes sure the image is centered correctly within the view.
        */
        public void FixScaleTrans()
        {
            FixTrans();
            TouchMatrix.GetValues(FloatMatrix);
            if (ImageWidth < ViewWidth)
            {
                var xOffset = (ViewWidth - ImageWidth) / 2;
                if (IsRotateImageToFitScreen && OrientationMismatch(Drawable))
                {
                    xOffset += ImageWidth;
                }
                FloatMatrix[Matrix.MtransX] = xOffset;
            }
            if (ImageHeight < ViewHeight)
            {
                FloatMatrix[Matrix.MtransY] = (ViewHeight - ImageHeight) / 2;
            }
            TouchMatrix.SetValues(FloatMatrix);
        }
        /**
         * Performs boundary checking and fixes the image matrix if it
         * is out of bounds.
         */
        public void FixTrans()
        {
            TouchMatrix.GetValues(FloatMatrix);
            var transX = FloatMatrix[Matrix.MtransX];
            var transY = FloatMatrix[Matrix.MtransY];
            var offset = 0f;
            if (IsRotateImageToFitScreen && OrientationMismatch(Drawable))
            {
                offset = ImageWidth;
            }
            var fixTransX = GetFixTrans(transX, ViewWidth, ImageWidth, offset);
            var fixTransY = GetFixTrans(transY, ViewHeight, ImageHeight, 0f);
            TouchMatrix.PostTranslate(fixTransX, fixTransY);
        }

        public void CompatPostOnAnimation(IRunnable runnable)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
            {
                PostOnAnimation(runnable);
            }
            else
            {
                PostDelayed(runnable, 1000L / 60L);
            }
        }

        public float GetFixDragTrans(float delta, float viewSize, float contentSize)
        {
            return contentSize <= viewSize ? 0f : delta;
        }

        public bool OrientationMismatch(Drawable drawable)
        {
            if (drawable == null)
            {
                return ViewWidth > ViewHeight != false;
            }

            return ViewWidth > ViewHeight != drawable.IntrinsicWidth > drawable.IntrinsicHeight;
        }

        /**
        * Save the current matrix and view dimensions
        * in the prevMatrix and prevView variables.
        */
        public void SavePreviousImageValues()
        {
            if (TouchMatrix != null && ViewHeight != 0 && ViewWidth != 0)
            {
                TouchMatrix.GetValues(FloatMatrix);
                mPrevMatrix.SetValues(FloatMatrix);
                mPrevMatchViewHeight = mMatchViewHeight;
                mPrevMatchViewWidth = mMatchViewWidth;
                mPrevViewHeight = ViewHeight;
                mPrevViewWidth = ViewWidth;
            }
        }

        /**
        * Set the max zoom multiplier as a multiple of minZoom, whatever minZoom may change to. By
        * default, this is not done, and maxZoom has a fixed value of 3.
        *
        * @param max max zoom multiplier, as a multiple of minZoom
        */
        public void SetMaxZoomRatio(float max)
        {
            mMaxScaleMultiplier = max;
            MaxScale = MinScale * mMaxScaleMultiplier;
            mSuperMaxScale = TouchImageConstants.SUPER_MAX_MULTIPLIER * MaxScale;
            mMaxScaleIsSetByMultiplier = true;
        }

        /**
        * Set zoom to the specified scale with a linearly interpolated animation. Image will be
        * centered around the point (focusX, focusY). These floats range from 0 to 1 and denote the
        * focus point as a fraction from the left and top of the view. For example, the top left
        * corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
        */
        public void SetZoomAnimated(float scale, float focusX, float focusY)
        {
            SetZoomAnimated(scale, focusX, focusY, TouchImageConstants.DEFAULT_ZOOM_TIME);
        }

        public void SetZoomAnimated(float scale, float focusX, float focusY, int zoomTimeMs)
        {
            var animation = new AnimatedZoom(this, scale, new PointF(focusX, focusY), zoomTimeMs);
            CompatPostOnAnimation(animation);
        }

        /**
         * Set zoom to the specified scale with a linearly interpolated animation. Image will be
         * centered around the point (focusX, focusY). These floats range from 0 to 1 and denote the
         * focus point as a fraction from the left and top of the view. For example, the top left
         * corner of the image would be (0, 0). And the bottom right corner would be (1, 1).
         *
         * @param listener the listener, which will be notified, once the animation ended
         */
        public void SetZoomAnimated(float scale, float focusX, float focusY, int zoomTimeMs, Action onZoomFinishedAction)
        {
            var animation = new AnimatedZoom(this, scale, new PointF(focusX, focusY), zoomTimeMs);
            animation.OnZoomFinishedAction = onZoomFinishedAction;
            CompatPostOnAnimation(animation);
        }

        public void SetZoomAnimated(float scale, float focusX, float focusY, Action onZoomFinishedAction)
        {
            var animation = new AnimatedZoom(this, scale, new PointF(focusX, focusY), TouchImageConstants.DEFAULT_ZOOM_TIME);
            animation.OnZoomFinishedAction = onZoomFinishedAction;
            CompatPostOnAnimation(animation);
        }

        #endregion

        #region overridenn methods

        public override void SetOnTouchListener(IOnTouchListener l)
        {
            UserTouchListener = l;
        }

        public override void SetImageResource(int resId)
        {
            mImageRenderedAtLeastOnce = false;
            base.SetImageResource(resId);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            mImageRenderedAtLeastOnce = false;
            base.SetImageBitmap(bm);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageDrawable(Drawable drawable)
        {
            mImageRenderedAtLeastOnce = false;
            base.SetImageDrawable(drawable);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetImageURI(global::Android.Net.Uri uri)
        {
            mImageRenderedAtLeastOnce = false;
            base.SetImageURI(uri);
            SavePreviousImageValues();
            FitImageToView();
        }

        public override void SetScaleType(ScaleType scaleType)
        {
            if (scaleType == ScaleType.Matrix)
            {
                base.SetScaleType(ScaleType.Matrix);
            }
            else
            {
                mTouchScaleType = scaleType;
                if (mOnDrawReady)
                {
                    // If the image is already rendered, scaleType has been called programmatically
                    // and the TouchImageView should be updated with the new scaleType.
                    SetZoom(this);
                }
            }
        }

        public override ScaleType GetScaleType()
        {
            return mTouchScaleType;
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var bundle = new Bundle();
            bundle.PutParcelable("instanceState", base.OnSaveInstanceState());
            bundle.PutInt("orientation", (int)mOrientation);
            bundle.PutFloat("saveScale", CurrentZoom);
            bundle.PutFloat("matchViewHeight", mMatchViewHeight);
            bundle.PutFloat("matchViewWidth", mMatchViewWidth);
            bundle.PutInt("viewWidth", ViewWidth);
            bundle.PutInt("viewHeight", ViewHeight);
            TouchMatrix.GetValues(FloatMatrix);
            bundle.PutFloatArray("matrix", FloatMatrix);
            bundle.PutBoolean("imageRendered", mImageRenderedAtLeastOnce);
            bundle.PutInt("viewSizeChangeFixedPixel", (int)ViewSizeChange);
            bundle.PutInt("orientationChangeFixedPixel", (int)OrientationChange);
            return bundle;
        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            if (state is Bundle bundle)
            {
                CurrentZoom = bundle.GetFloat("saveScale");
                FloatMatrix = bundle.GetFloatArray("matrix");
                mPrevMatrix.SetValues(FloatMatrix);
                mPrevMatchViewHeight = bundle.GetFloat("matchViewHeight");
                mPrevMatchViewWidth = bundle.GetFloat("matchViewWidth");
                mPrevViewHeight = bundle.GetInt("viewHeight");
                mPrevViewWidth = bundle.GetInt("viewWidth");
                mImageRenderedAtLeastOnce = bundle.GetBoolean("imageRendered");
                ViewSizeChange = (FixedPixel)bundle.GetInt("viewSizeChangeFixedPixel");
                OrientationChange = (FixedPixel)bundle.GetInt("orientationChangeFixedPixel");
                var oldOrientation = bundle.GetInt("orientation");
                if ((int)mOrientation != oldOrientation)
                {
                    mOrientationJustChanged = true;
                }
                base.OnRestoreInstanceState(bundle.GetParcelable("instanceState") as IParcelable);
                return;
            }
            base.OnRestoreInstanceState(state);
        }

        protected override void OnDraw(Canvas canvas)
        {
            mOnDrawReady = true;
            mImageRenderedAtLeastOnce = true;
            if (mDelayedZoomVariables != null)
            {
                SetZoom(mDelayedZoomVariables.Scale, mDelayedZoomVariables.FocusX, mDelayedZoomVariables.FocusY, mDelayedZoomVariables.ScaleType);
                mDelayedZoomVariables = null;
            }
            base.OnDraw(canvas);
        }

        protected override void OnConfigurationChanged(global::Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            var newOrientation = Resources.Configuration.Orientation;
            if (newOrientation != mOrientation)
            {
                mOrientationJustChanged = true;
                mOrientation = newOrientation;
            }
            SavePreviousImageValues();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var drawable = Drawable;
            if (drawable == null || drawable.IntrinsicWidth == 0 || drawable.IntrinsicHeight == 0)
            {
                SetMeasuredDimension(0, 0);
                return;
            }
            var drawableWidth = GetDrawableWidth(drawable);
            var drawableHeight = GetDrawableHeight(drawable);
            var widthSize = MeasureSpec.GetSize(widthMeasureSpec);
            var widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            var heightSize = MeasureSpec.GetSize(heightMeasureSpec);
            var heightMode = MeasureSpec.GetMode(heightMeasureSpec);
            var totalViewWidth = SetViewSize(widthMode, widthSize, drawableWidth);
            var totalViewHeight = SetViewSize(heightMode, heightSize, drawableHeight);
            if (!mOrientationJustChanged)
            {
                SavePreviousImageValues();
            }

            // Image view width, height must consider padding
            var width = totalViewWidth - PaddingLeft - PaddingRight;
            var height = totalViewHeight - PaddingTop - PaddingBottom;

            // Set view dimensions
            SetMeasuredDimension(width, height);
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);

            // Fit content within view.
            //
            // onMeasure may be called multiple times for each layout change, including orientation
            // changes. For example, if the TouchImageView is inside a ConstraintLayout, onMeasure may
            // be called with:
            // widthMeasureSpec == "AT_MOST 2556" and then immediately with
            // widthMeasureSpec == "EXACTLY 1404", then back and forth multiple times in quick
            // succession, as the ConstraintLayout tries to solve its constraints.
            //
            // onSizeChanged is called once after the final onMeasure is called. So we make all changes
            // to class members, such as fitting the image into the new shape of the TouchImageView,
            // here, after the final size has been determined. This helps us avoid both
            // repeated computations, and making irreversible changes (e.g. making the View temporarily too
            // big or too small, thus making the current zoom fall outside of an automatically-changing
            // minZoom and maxZoom).
            ViewWidth = w;
            ViewHeight = h;
            FitImageToView();
        }

        public override bool CanScrollHorizontally(int direction)
        {
            TouchMatrix.GetValues(FloatMatrix);
            var x = FloatMatrix[Matrix.MtransX];
            if (ImageWidth < ViewWidth)
            {
                return false;
            }
            else if (x >= -1 && direction < 0)
            {
                return false;
            }
            else
            {
                return System.Math.Abs(x) + ViewWidth + 1 < ImageWidth || direction <= 0;
            }
        }

        public override bool CanScrollVertically(int direction)
        {
            TouchMatrix.GetValues(FloatMatrix);
            var y = FloatMatrix[Matrix.MtransY];
            if (ImageHeight < ViewHeight)
            {
                return false;
            }
            else if (y >= -1 && direction < 0)
            {
                return false;
            }
            else
            {
                return System.Math.Abs(y) + ViewHeight + 1 < ImageHeight || direction <= 0;
            }
        }

        #endregion

        #region private methods

        private void Init(Context context, IAttributeSet attrs, int defStyleAttr)
        {
            mContext = context;

            Clickable = true;
            mOrientation = Resources.Configuration.Orientation;
            ScaleDetector = new ScaleGestureDetector(mContext, new ScaleListener(this));
            GestureDetector = new GestureDetector(mContext, new GestureListener(this));
            TouchMatrix = new Matrix();
            mPrevMatrix = new Matrix();
            FloatMatrix = new float[9];
            CurrentZoom = 1f;

            if (mTouchScaleType == null)
            {
                mTouchScaleType = ScaleType.FitCenter;
            }
            MinScale = 1f;
            MaxScale = 3f;
            mSuperMinScale = TouchImageConstants.SUPER_MIN_MULTIPLIER * MinScale;
            mSuperMaxScale = TouchImageConstants.SUPER_MAX_MULTIPLIER * MaxScale;
            ImageMatrix = TouchMatrix;
            SetScaleType(ScaleType.Matrix);
            State = TouchImageState.None;

            mOnDrawReady = false;
            base.SetOnTouchListener(new PrivateOnTouchListener(this));

            var attributes = mContext.Theme.ObtainStyledAttributes(attrs, Resource.Styleable.TouchImageView, defStyleAttr, 0);
            try
            {
                if (!IsInEditMode)
                {
                    IsZoomEnabled = attributes.GetBoolean(Resource.Styleable.TouchImageView_zoom_enabled, true);
                }
            }
            finally
            {
                // release the TypedArray so that it can be reused.
                attributes.Recycle();
            }
        }

        private float GetFixTrans(float trans, float viewSize, float contentSize, float offset)
        {
            float minTrans;
            float maxTrans;
            if (contentSize <= viewSize)
            {
                minTrans = offset;
                maxTrans = offset + viewSize - contentSize;
            }
            else
            {
                minTrans = offset + viewSize - contentSize;
                maxTrans = offset;
            }
            if (trans < minTrans) return -trans + minTrans;
            return (trans > maxTrans) ? -trans + maxTrans : 0f;
        }

        /**
        * This function can be called:
        * 1. When the TouchImageView is first loaded (onMeasure).
        * 2. When a new image is loaded (setImageResource|Bitmap|Drawable|URI).
        * 3. On rotation (onSaveInstanceState, then onRestoreInstanceState, then onMeasure).
        * 4. When the view is resized (onMeasure).
        * 5. When the zoom is reset (resetZoom).
        *
        * In cases 2, 3 and 4, we try to maintain the zoom state and position as directed by
        * orientationChangeFixedPixel or viewSizeChangeFixedPixel (if there is an existing zoom state
        * and position, which there might not be in case 2).
        *
        *
        * If the normalizedScale is equal to 1, then the image is made to fit the View. Otherwise, we
        * maintain zoom level and attempt to roughly put the same part of the image in the View as was
        * there before, paying attention to orientationChangeFixedPixel or viewSizeChangeFixedPixel.
        */
        private void FitImageToView()
        {
            var fixedPixel = mOrientationJustChanged ? OrientationChange : ViewSizeChange;
            mOrientationJustChanged = false;
            if (Drawable == null || Drawable.IntrinsicWidth == 0 || Drawable.IntrinsicHeight == 0)
            {
                return;
            }
            if (TouchMatrix == null || mPrevMatrix == null)
            {
                return;
            }
            if (mUserSpecifiedMinScale == TouchImageConstants.AUTOMATIC_MIN_ZOOM)
            {
                MinZoom = TouchImageConstants.AUTOMATIC_MIN_ZOOM;
                if (CurrentZoom < MinScale)
                {
                    CurrentZoom = MinScale;
                }
            }
            var drawableWidth = GetDrawableWidth(Drawable);
            var drawableHeight = GetDrawableHeight(Drawable);

            // Scale image for view
            var scaleX = (float)ViewWidth / drawableWidth;
            var scaleY = (float)ViewHeight / drawableHeight;
            if (mTouchScaleType == ScaleType.Center)
            {
                scaleY = 1f;
                scaleX = scaleY;
            }
            else if (mTouchScaleType == ScaleType.CenterCrop)
            {
                scaleY = System.Math.Max(scaleX, scaleY);
                scaleX = scaleY;
            }
            else if (mTouchScaleType == ScaleType.CenterInside)
            {
                scaleY = System.Math.Min(1f, System.Math.Min(scaleX, scaleY));
                scaleX = scaleY;
                scaleY = System.Math.Min(scaleX, scaleY);
                scaleX = scaleY;
            }
            else if (mTouchScaleType == ScaleType.FitCenter || mTouchScaleType == ScaleType.FitStart || mTouchScaleType == ScaleType.FitEnd)
            {
                scaleY = System.Math.Min(scaleX, scaleY);
                scaleX = scaleY;
            }

            // Put the image's center in the right place.
            var redundantXSpace = ViewWidth - scaleX * drawableWidth;
            var redundantYSpace = ViewHeight - scaleY * drawableHeight;
            mMatchViewWidth = ViewWidth - redundantXSpace;
            mMatchViewHeight = ViewHeight - redundantYSpace;
            if (!IsZoomed && !mImageRenderedAtLeastOnce)
            {

                // Stretch and center image to fit view
                if (IsRotateImageToFitScreen && OrientationMismatch(Drawable))
                {
                    TouchMatrix.SetRotate(90f);
                    TouchMatrix.PostTranslate(drawableWidth, 0f);
                    TouchMatrix.PostScale(scaleX, scaleY);
                }
                else
                {
                    TouchMatrix.SetScale(scaleX, scaleY);
                }

                if (mTouchScaleType == ScaleType.FitStart)
                {
                    TouchMatrix.PostTranslate(0f, 0f);
                }
                else if (mTouchScaleType == ScaleType.FitEnd)
                {
                    TouchMatrix.PostTranslate(redundantXSpace, redundantYSpace);
                }
                else
                {
                    TouchMatrix.PostTranslate(redundantXSpace / 2, redundantYSpace / 2);
                }

                CurrentZoom = 1f;
            }
            else
            {
                // These values should never be 0 or we will set viewWidth and viewHeight
                // to NaN in newTranslationAfterChange. To avoid this, call savePreviousImageValues
                // to set them equal to the current values.
                if (mPrevMatchViewWidth == 0f || mPrevMatchViewHeight == 0f)
                {
                    SavePreviousImageValues();
                }

                // Use the previous matrix as our starting point for the new matrix.
                mPrevMatrix.GetValues(FloatMatrix);

                // Rescale Matrix if appropriate
                FloatMatrix[Matrix.MscaleX] = mMatchViewWidth / drawableWidth * CurrentZoom;
                FloatMatrix[Matrix.MscaleY] = mMatchViewHeight / drawableHeight * CurrentZoom;

                // TransX and TransY from previous matrix
                var transX = FloatMatrix[Matrix.MtransX];
                var transY = FloatMatrix[Matrix.MtransY];

                // X position
                var prevActualWidth = mPrevMatchViewWidth * CurrentZoom;
                var actualWidth = ImageWidth;
                FloatMatrix[Matrix.MtransX] = NewTranslationAfterChange(transX, prevActualWidth, actualWidth, mPrevViewWidth, ViewWidth, drawableWidth, fixedPixel);

                // Y position
                var prevActualHeight = mPrevMatchViewHeight * CurrentZoom;
                var actualHeight = ImageHeight;
                FloatMatrix[Matrix.MtransY] = NewTranslationAfterChange(transY, prevActualHeight, actualHeight, mPrevViewHeight, ViewHeight, drawableHeight, fixedPixel);

                // Set the matrix to the adjusted scale and translation values.
                TouchMatrix.SetValues(FloatMatrix);
            }
            FixTrans();
            ImageMatrix = TouchMatrix;
        }

        private int GetDrawableWidth(Drawable drawable)
        {
            if (drawable == null)
            {
                return 0;
            }
            return OrientationMismatch(drawable) && IsRotateImageToFitScreen ? drawable.IntrinsicHeight :
                                                                               drawable.IntrinsicWidth;
        }

        private int GetDrawableHeight(Drawable drawable)
        {
            if (drawable == null)
            {
                return 0;
            }
            return OrientationMismatch(drawable) && IsRotateImageToFitScreen ? drawable.IntrinsicWidth :
                                                                               drawable.IntrinsicHeight;
        }

        /**
        * After any change described in the comments for fitImageToView, the matrix needs to be
        * translated. This function translates the image so that the fixed pixel in the image
        * stays in the same place in the View.
        *
        * @param trans                the value of trans in that axis before the rotation
        * @param prevImageSize        the width/height of the image before the rotation
        * @param imageSize            width/height of the image after rotation
        * @param prevViewSize         width/height of view before rotation
        * @param viewSize             width/height of view after rotation
        * @param drawableSize         width/height of drawable
        * @param sizeChangeFixedPixel how we should choose the fixed pixel
        */
        private float NewTranslationAfterChange(float trans, float prevImageSize, float imageSize, int prevViewSize, int viewSize, int drawableSize, FixedPixel sizeChangeFixedPixel)
        {
            if (imageSize < viewSize)
            {
                // The width/height of image is less than the view's width/height. Center it.
                return (viewSize - drawableSize * FloatMatrix[Matrix.MscaleX]) * 0.5f;
            }
            else if (trans > 0)
            {
                // The image is larger than the view, but was not before the view changed. Center it.
                return -((imageSize - viewSize) * 0.5f);
            }
            else
            {
                // Where is the pixel in the View that we are keeping stable, as a fraction of the
                // width/height of the View?
                var fixedPixelPositionInView = 0.5f; // CENTER
                if (sizeChangeFixedPixel == FixedPixel.BottomRight)
                {
                    fixedPixelPositionInView = 1.0f;
                }
                else if (sizeChangeFixedPixel == FixedPixel.TopLeft)
                {
                    fixedPixelPositionInView = 0.0f;
                }
                // Where is the pixel in the Image that we are keeping stable, as a fraction of the
                // width/height of the Image?
                var fixedPixelPositionInImage = (-trans + fixedPixelPositionInView * prevViewSize) / prevImageSize;

                // Here's what the new translation should be so that, after whatever change triggered
                // this function to be called, the pixel at fixedPixelPositionInView of the View is
                // still the pixel at fixedPixelPositionInImage of the image.
                return -(fixedPixelPositionInImage * imageSize - viewSize * fixedPixelPositionInView);
            }
        }

        // Set view dimensions based on layout params
        private int SetViewSize(MeasureSpecMode mode, int size, int drawableWidth)
        {
            switch (mode)
            {
                case MeasureSpecMode.Exactly:
                    return size;
                case MeasureSpecMode.AtMost:
                    return System.Math.Min(drawableWidth, size);
                case MeasureSpecMode.Unspecified:
                    return drawableWidth;
                default:
                    return size;
            }
        }

        #endregion
    }
}