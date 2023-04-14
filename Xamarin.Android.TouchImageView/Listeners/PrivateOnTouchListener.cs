using Android.Graphics;
using Android.Views;
using static Android.Views.View;

namespace Xamarin.Android.TouchImageView.Listeners
{
    public class PrivateOnTouchListener : Java.Lang.Object, IOnTouchListener
    {
        private readonly TouchImageView mTouchImageView;
        private PointF mLast = new PointF();

        public PrivateOnTouchListener(TouchImageView touchImageView)
        {
            this.mTouchImageView = touchImageView;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if (mTouchImageView.Drawable == null)
            {
                mTouchImageView.State = ImageActionState.None;
                return false;
            }
            if (mTouchImageView.IsZoomEnabled)
            {
                mTouchImageView.ScaleDetector.OnTouchEvent(e);
            }
            mTouchImageView.GestureDetector.OnTouchEvent(e);
            var curr = new PointF(e.GetX(), e.GetY());
            if (mTouchImageView.State == ImageActionState.None || mTouchImageView.State == ImageActionState.Drag || mTouchImageView.State == ImageActionState.Fling)
            {
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        mLast.Set(curr);
                        if (mTouchImageView.Fling != null) mTouchImageView.Fling.CancelFling();
                        mTouchImageView.State = ImageActionState.Drag;
                        break;
                    case MotionEventActions.Move:
                        if (mTouchImageView.State == ImageActionState.Drag)
                        {
                            var deltaX = curr.X - mLast.X;
                            var deltaY = curr.Y - mLast.Y;
                            var fixTransX = mTouchImageView.GetFixDragTrans(deltaX, mTouchImageView.ViewWidth, mTouchImageView.ImageWidth);
                            var fixTransY = mTouchImageView.GetFixDragTrans(deltaY, mTouchImageView.ViewHeight, mTouchImageView.ImageHeight);
                            mTouchImageView.TouchMatrix.PostTranslate(fixTransX, fixTransY);
                            mTouchImageView.FixTrans();
                            mLast.Set(curr.X, curr.Y);//TO BE VERIFIED
                        }
                        break;
                    case MotionEventActions.Up:
                    case MotionEventActions.PointerUp:
                        mTouchImageView.State = ImageActionState.None;
                        break;

                }

            }
            mTouchImageView.OnTouchCoordinatesAction?.Invoke(v, e, mTouchImageView.TransformCoordTouchToBitmap(e.GetX(), e.GetY(), true));

            mTouchImageView.ImageMatrix = mTouchImageView.TouchMatrix;

            //
            // User-defined OnTouchListener
            //
            if (mTouchImageView.UserTouchListener != null)
            {
                mTouchImageView.UserTouchListener.OnTouch(v, e);
            }

            //
            // OnTouchImageViewListener is set: TouchImageView dragged by user.
            //
            mTouchImageView.TouchMoveImageViewAction?.Invoke();

            //
            // indicate event was handled
            //
            return true;
        }
    }
}