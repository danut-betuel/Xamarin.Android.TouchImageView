using Android.Animation;
using System;
using static Android.Animation.ValueAnimator;

namespace Sample.Listeners
{
    public class AnimatorUpdateListener : Java.Lang.Object, IAnimatorUpdateListener
    {
        public Action<ValueAnimator> OnAnimationUpdateAction { get; set; }

        public AnimatorUpdateListener(Action<ValueAnimator> onAnimationUpdateAction)
        {
            OnAnimationUpdateAction = onAnimationUpdateAction;
        }

        public void OnAnimationUpdate(ValueAnimator animation)
        {
            OnAnimationUpdateAction?.Invoke(animation);
        }
    }
}