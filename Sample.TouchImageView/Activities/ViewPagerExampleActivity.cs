﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.ViewPager.Widget;
using Sample.Adapters;
using Sample.Constants;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ViewPagerExampleActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_viewpager_example);

            var viewPager = FindViewById<ViewPager>(Resource.Id.view_pager);

            viewPager.Adapter = new TouchImageViewPagerAdapter(ImagesConstants.Images);
        }
    }
}