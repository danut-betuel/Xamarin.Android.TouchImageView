using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.ViewPager.Widget;
using System;
using Xamarin.Android.TouchImageView;

namespace Sample.CustomViews
{
    [Register("sample.touchimageview.customviews.ExtendedViewPager")]
    public class ExtendedViewPager : ViewPager
    {
        #region ctors

        public ExtendedViewPager(Context context) : base(context)
        {
        }

        public ExtendedViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected ExtendedViewPager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        #endregion

        protected override bool CanScroll(View v, bool checkV, int dx, int x, int y)
        {
            if(v is TouchImageView)
            {
                return v.CanScrollHorizontally(-dx);
            }

            return base.CanScroll(v, checkV, dx, x, y);
        }
    }
}