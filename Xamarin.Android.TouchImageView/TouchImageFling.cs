using Android.Graphics;
using Java.Lang;

namespace Xamarin.Android.TouchImageView
{
    public class TouchImageFling : Java.Lang.Object, IRunnable
    {
        private TouchImageView mTouchImageView;

        public CompatScroller Scroller { get; set; }
        public int CurrX { get; set; }
        public int CurrY { get; set; }

        public TouchImageFling(TouchImageView tiv, int velocityX, int velocityY)
        {
            mTouchImageView = tiv;
            mTouchImageView.State = ImageActionState.Fling;
            Scroller = new CompatScroller(tiv.Context);
            mTouchImageView.TouchMatrix.GetValues(mTouchImageView.FloatMatrix);

            var startX = (int)mTouchImageView.FloatMatrix[Matrix.MtransX];
            var startY = (int)mTouchImageView.FloatMatrix[Matrix.MtransY];
            int minX;
            int maxX;
            int minY;
            int maxY;
            if (mTouchImageView.IsRotateImageToFitScreen && mTouchImageView.OrientationMismatch(mTouchImageView.Drawable))
            {
                startX -= (int)mTouchImageView.ImageWidth;
            }
            if (mTouchImageView.ImageWidth > mTouchImageView.ViewWidth)
            {
                minX = mTouchImageView.ViewWidth - (int)mTouchImageView.ImageWidth;
                maxX = 0;
            }
            else
            {
                maxX = startX;
                minX = maxX;
            }
            if (mTouchImageView.ImageHeight > mTouchImageView.ViewHeight)
            {
                minY = mTouchImageView.ViewHeight - (int)mTouchImageView.ImageHeight;
                maxY = 0;
            }
            else
            {
                maxY = startY;
                minY = maxY;
            }
            Scroller.Fling(startX, startY, velocityX, velocityY, minX, maxX, minY, maxY);
            CurrX = startX;
            CurrY = startY;
        }

        public void CancelFling()
        {
            if (Scroller == null)
            {
                return;
            }

            mTouchImageView.State = ImageActionState.None;
            Scroller.ForceFinished(true);
        }

        public void Run()
        {
            // OnTouchImageViewListener is set: TouchImageView listener has been flung by user.
            // Listener runnable updated with each frame of fling animation.
            mTouchImageView.TouchMoveImageViewAction?.Invoke();

            if (Scroller.IsFinished)
            {
                Scroller = null;
                return;
            }
            if (Scroller.ComputeScrollOffset())
            {
                var newX = Scroller.CurrX;
                var newY = Scroller.CurrY;
                var transX = newX - CurrX;
                var transY = newY - CurrY;
                CurrX = newX;
                CurrY = newY;
                mTouchImageView.TouchMatrix.PostTranslate(transX, transY);
                mTouchImageView.FixTrans();
                mTouchImageView.ImageMatrix = mTouchImageView.TouchMatrix;
                mTouchImageView.CompatPostOnAnimation(this);
            }
        }
    }
}