using Android.Views;
using static Android.Views.ScaleGestureDetector;

namespace Xamarin.Android.TouchImageView.Listeners
{
    public class ScaleListener : SimpleOnScaleGestureListener
    {
        private TouchImageView mTouchImageView;

        public ScaleListener(TouchImageView touchImageView)
        {
            mTouchImageView = touchImageView;
        }

        public override bool OnScaleBegin(ScaleGestureDetector detector)
        {
            mTouchImageView.State = ImageActionState.Zoom;
            return true;
        }

        public override bool OnScale(ScaleGestureDetector detector)
        {
            mTouchImageView.ScaleImage(detector.ScaleFactor, detector.FocusX, detector.FocusY, true);
            mTouchImageView.TouchMoveImageViewAction?.Invoke();

            return true;
        }

        public override void OnScaleEnd(ScaleGestureDetector detector)
        {
            base.OnScaleEnd(detector);
            mTouchImageView.State = ImageActionState.None;
            var animateToZoomBoundary = false;
            var targetZoom = mTouchImageView.CurrentZoom;
            if (mTouchImageView.CurrentZoom > mTouchImageView.MaxScale)
            {
                targetZoom = mTouchImageView.MaxScale;
                animateToZoomBoundary = true;
            }
            else if (mTouchImageView.CurrentZoom < mTouchImageView.MinScale)
            {
                targetZoom = mTouchImageView.MinScale;
                animateToZoomBoundary = true;
            }
            if (animateToZoomBoundary)
            {
                var doubleTap = new DoubleTapZoom(mTouchImageView, targetZoom, mTouchImageView.ViewWidth / 2, mTouchImageView.ViewHeight / 2, true);
                mTouchImageView.CompatPostOnAnimation(doubleTap);
            }
        }
    }
}