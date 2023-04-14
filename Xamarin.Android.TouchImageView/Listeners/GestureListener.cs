using Android.Views;
using static Android.Views.GestureDetector;

namespace Xamarin.Android.TouchImageView.Listeners
{
    public class GestureListener : SimpleOnGestureListener
    {
        private readonly TouchImageView mTouchImageView;

        public GestureListener(TouchImageView touchImageView)
        {
            mTouchImageView = touchImageView;
        }

        public override bool OnSingleTapConfirmed(MotionEvent e)
        {
            if (mTouchImageView.DoubleTapListener != null)
            {
                return mTouchImageView.DoubleTapListener.OnSingleTapConfirmed(e);
            }

            return mTouchImageView.PerformClick();
        }

        public override void OnLongPress(MotionEvent e)
        {
            mTouchImageView.PerformLongClick();
        }

        public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            mTouchImageView.Fling?.CancelFling();
            mTouchImageView.Fling = new TouchImageFling(mTouchImageView, (int)velocityX, (int)velocityY);
            mTouchImageView.CompatPostOnAnimation(mTouchImageView.Fling);

            return base.OnFling(e1, e2, velocityX, velocityY);
        }

        public override bool OnDoubleTap(MotionEvent e)
        {
            var consumed = false;
            if (mTouchImageView.IsZoomEnabled)
            {
                if (mTouchImageView.DoubleTapListener != null)
                {
                    consumed = mTouchImageView.DoubleTapListener.OnDoubleTap(e);
                }

                if (mTouchImageView.State == ImageActionState.None)
                {
                    var maxZoomScale = mTouchImageView.DoubleTapScale == 0f ? mTouchImageView.MaxScale : mTouchImageView.DoubleTapScale;
                    var targetZoom = mTouchImageView.CurrentZoom == mTouchImageView.MinScale ? maxZoomScale : mTouchImageView.MinScale;
                    var doubleTap = new DoubleTapZoom(mTouchImageView, targetZoom, e.GetX(), e.GetY(), false);
                    mTouchImageView.CompatPostOnAnimation(doubleTap);
                    consumed = true;
                }
            }
            return consumed;
        }

        public override bool OnDoubleTapEvent(MotionEvent e)
        {
            if (mTouchImageView.DoubleTapListener != null)
            {
                return mTouchImageView.DoubleTapListener.OnDoubleTapEvent(e);
            }

            return false;
        }
    }
}