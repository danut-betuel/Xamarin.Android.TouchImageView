using Android.Views;
using static Android.Views.View;

namespace Sample.Listeners
{
    public class TouchListener : Java.Lang.Object, IOnTouchListener
    {
        private readonly TouchListenerDelegate mTouchListenerDelegate;

        public TouchListener(TouchListenerDelegate touchListenerDelegate)
        {
            mTouchListenerDelegate = touchListenerDelegate;
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            if(mTouchListenerDelegate != null)
            {
                return mTouchListenerDelegate.Invoke(v, e);
            }

            return true;
        }
    }

    public delegate bool TouchListenerDelegate(View v, MotionEvent e);
}