using Android.Views;
using Android.Widget;
using AndroidX.ViewPager.Widget;
using Xamarin.Android.TouchImageView;

namespace Sample.Adapters
{
    public class TouchImageViewPagerAdapter : PagerAdapter
    {
        private readonly int[] mImages;

        public TouchImageViewPagerAdapter(int[] images)
        {
            this.mImages = images;
        }

        public override int Count => mImages == null ? 0 : mImages.Length;

        public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
        {
            var touchImageview = new TouchImageView(container.Context);
            touchImageview.SetImageResource(mImages[position]);
            container.AddView(touchImageview, LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);

            return touchImageview;
        }

        public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object @object)
        {
            container.RemoveView(@object as View);
        }

        public override bool IsViewFromObject(View view, Java.Lang.Object @object)
        {
            return view == @object;
        }
    }
}