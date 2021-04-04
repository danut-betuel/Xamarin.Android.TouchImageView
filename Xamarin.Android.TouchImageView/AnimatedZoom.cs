using Android.Graphics;
using Android.Views.Animations;
using Java.Lang;
using System;

namespace Xamarin.Android.TouchImageView
{
    public class AnimatedZoom : Java.Lang.Object, IRunnable
    {
        #region pirvate fields

        private int mZoomTimeMillis;
        private long mStartTime;
        private float mStartZoom;
        private float mTargetZoom;
        private PointF mStartFocus;
        private PointF mTargetFocus;
        private LinearInterpolator mInterpolator = new LinearInterpolator();

        private TouchImageView mTouchImageView;

        #endregion

        public Action OnZoomFinishedAction { get; set; }
        

        public AnimatedZoom(TouchImageView touchImageView, float targetZoom, PointF focus, int zoomTimeMillis)
        {
            mTouchImageView = touchImageView;
            touchImageView.State = TouchImageState.AnimateZoom;
            mStartTime = JavaSystem.CurrentTimeMillis();
            mStartZoom = touchImageView.CurrentZoom;
            mTargetZoom = targetZoom;
            mZoomTimeMillis = zoomTimeMillis;

            // Used for translating image during zooming
            mStartFocus = touchImageView.ScrollPosition;
            mTargetFocus = focus;
        }

        public void Run()
        {
            var t = Interpolate();

            // Calculate the next focus and zoom based on the progress of the interpolation
            var nextZoom = mStartZoom + (mTargetZoom - mStartZoom) * t;
            var nextX = mStartFocus.X + (mTargetFocus.X - mStartFocus.X) * t;
            var nextY = mStartFocus.Y + (mTargetFocus.Y - mStartFocus.Y) * t;
            mTouchImageView.SetZoom(nextZoom, nextX, nextY);
            if (t < 1f)
            {
                // We haven't finished zooming
                mTouchImageView.CompatPostOnAnimation(this);
            }
            else
            {
                // Finished zooming
                mTouchImageView.State = TouchImageState.None;
                OnZoomFinishedAction?.Invoke();
            }
        }

        /**
        * Use interpolator to get t
        *
        * @return progress of the interpolation
        */
        private float Interpolate()
        {
            var elapsed = (JavaSystem.CurrentTimeMillis() - mStartTime) / (float)mZoomTimeMillis;
            elapsed = System.Math.Min(1f, elapsed);
            return mInterpolator.GetInterpolation(elapsed);
        }
    }
}