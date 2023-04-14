using Android.Graphics;
using Android.Views.Animations;
using Java.Lang;

namespace Xamarin.Android.TouchImageView
{
    public class DoubleTapZoom : Object, IRunnable
    {
        #region private fields

        private readonly TouchImageView mTouchImageView;

        private long mStartTime;
        private float mStartZoom;
        private float mTargetZoom;
        private float mBitmapX;
        private float mBitmapY;
        private bool mStretchImageToSuper;
        private AccelerateDecelerateInterpolator mInterpolator = new AccelerateDecelerateInterpolator();
        private PointF mStartTouch;
        private PointF mEndTouch;

        #endregion

        public DoubleTapZoom(TouchImageView touchImageView, float targetZoom, float focusX, float focusY, bool stretchImageToSuper)
        {
            touchImageView.State = ImageActionState.AnimateZoom;
            mTouchImageView = touchImageView;

            mStartTime = JavaSystem.CurrentTimeMillis();
            mStartZoom = touchImageView.CurrentZoom;
            mTargetZoom = targetZoom;
            mStretchImageToSuper = stretchImageToSuper;
            var bitmapPoint = mTouchImageView.TransformCoordTouchToBitmap(focusX, focusY, false);
            mBitmapX = bitmapPoint.X;
            mBitmapY = bitmapPoint.Y;

            // Used for translating image during scaling
            mStartTouch = mTouchImageView.TransformCoordBitmapToTouch(mBitmapX, mBitmapY);
            mEndTouch = new PointF(touchImageView.ViewWidth / 2, touchImageView.ViewHeight / 2);
        }

        public void Run()
        {
            if (mTouchImageView.Drawable == null)
            {
                mTouchImageView.State = ImageActionState.None;
                return;
            }
            var t = Interpolate();
            var deltaScale = CalculateDeltaScale(t);
            mTouchImageView.ScaleImage(deltaScale, mBitmapX, mBitmapY, mStretchImageToSuper);
            TranslateImageToCenterTouchPosition(t);
            mTouchImageView.FixScaleTrans();
            mTouchImageView.ImageMatrix = mTouchImageView.TouchMatrix;

            // double tap runnable updates listener with every frame.
            mTouchImageView.TouchMoveImageViewAction?.Invoke();

            if (t < 1f)
            {
                // We haven't finished zooming
                mTouchImageView.CompatPostOnAnimation(this);
            }
            else
            {
                // Finished zooming
                mTouchImageView.State = ImageActionState.None;
            }
        }

        /**
         * Interpolate between where the image should start and end in order to translate
         * the image so that the point that is touched is what ends up centered at the end
         * of the zoom.
         */
        private void TranslateImageToCenterTouchPosition(float t)
        {
            var targetX = mStartTouch.X + t * (mEndTouch.X - mStartTouch.X);
            var targetY = mStartTouch.Y + t * (mEndTouch.Y - mStartTouch.Y);
            var curr = mTouchImageView.TransformCoordBitmapToTouch(mBitmapX, mBitmapY);
            mTouchImageView.TouchMatrix.PostTranslate(targetX - curr.X, targetY - curr.Y);
        }

        /**
        * Use interpolator to get t
        */
        public float Interpolate()
        {
            var currTime = JavaSystem.CurrentTimeMillis();
            var elapsed = (currTime - mStartTime) / (float)TouchImageConstants.DEFAULT_ZOOM_TIME;
            elapsed = System.Math.Min(1f, elapsed);
            return mInterpolator.GetInterpolation(elapsed);
        }

        /**
        * Interpolate the current targeted zoom and get the delta
        * from the current zoom.
        */
        private double CalculateDeltaScale(float t)
        {
            var zoom = mStartZoom + t * (double)(mTargetZoom - mStartZoom);
            return zoom / mTouchImageView.CurrentZoom;
        }
    }
}