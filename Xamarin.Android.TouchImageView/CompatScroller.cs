using Android.Content;
using Android.Widget;

namespace Xamarin.Android.TouchImageView
{
    public class CompatScroller
    {
        public OverScroller OverScroller { get; set; }

        public CompatScroller(Context context)
        {
            OverScroller = new OverScroller(context);
        }

        public void Fling(int startX, int startY, int velocityX, int velocityY, int minX, int maxX, int minY, int maxY)
        {
            OverScroller.Fling(startX, startY, velocityX, velocityY, minX, maxX, minY, maxY);
        }

        public void ForceFinished(bool finished)
        {
            OverScroller.ForceFinished(finished);
        }

        public bool IsFinished => OverScroller.IsFinished;

        public bool ComputeScrollOffset()
        {
            return OverScroller.ComputeScrollOffset();
        }

        public int CurrX => OverScroller.CurrX;
        public int CurrY => OverScroller.CurrY;
    }
}