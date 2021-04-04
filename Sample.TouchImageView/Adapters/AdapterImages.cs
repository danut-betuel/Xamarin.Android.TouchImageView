using Android.Views;
using AndroidX.RecyclerView.Widget;
using Sample.Listeners;
using Xamarin.Android.TouchImageView;

namespace Sample.Adapters
{
    public class AdapterImages : RecyclerView.Adapter
    {
        private readonly int[] mImgResources;

        public AdapterImages(int[] imgResources)
        {
            mImgResources = imgResources;
        }

        public override int ItemCount => mImgResources == null ? 0 : mImgResources.Length;

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int GetItemViewType(int position)
        {
            return 0;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ((AdapterImagesViewholder)holder).ImagePlace.SetImageResource(mImgResources[position]);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            return new AdapterImagesViewholder(parent, new TouchImageView(parent.Context));
        }
    }

    public class AdapterImagesViewholder : RecyclerView.ViewHolder
    {
        public TouchImageView ImagePlace { get; }

        public AdapterImagesViewholder(ViewGroup parent, TouchImageView itemView) : base(itemView)
        {
            ImagePlace = itemView;

            ImagePlace.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
            ImagePlace.SetOnTouchListener(new TouchListener((view, e) =>
            {
                var result = true;

                //can scroll horizontally checks if there's still a part of the image
                //that can be scrolled until you reach the edge
                if (e.PointerCount >= 2 || view.CanScrollHorizontally(1) && ImagePlace.CanScrollHorizontally(-1))
                {
                    //multi-touch event
                    switch(e.Action)
                    {
                        case MotionEventActions.Down:
                        case MotionEventActions.Move:
                            parent.RequestDisallowInterceptTouchEvent(true); // Disallow RecyclerView to intercept touch events.
                            result = false; // Disable touch on view
                            break;
                        case MotionEventActions.Up:
                            parent.RequestDisallowInterceptTouchEvent(false); // Allow RecyclerView to intercept touch events..
                            result = false;
                            break;
                    }
                }

                return result;
            }));
        }
    }
}