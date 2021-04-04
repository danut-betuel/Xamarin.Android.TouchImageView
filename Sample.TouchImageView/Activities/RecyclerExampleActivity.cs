using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;
using AndroidX.RecyclerView.Widget;
using Sample.Adapters;
using Sample.Constants;

namespace Sample.Activities
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait)]
    public class RecyclerExampleActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_recyclerview);

            var recyclerVeiw = FindViewById<RecyclerView>(Resource.Id.recycler);
            recyclerVeiw.SetAdapter(new AdapterImages(ImagesConstants.Images));
            new PagerSnapHelper().AttachToRecyclerView(recyclerVeiw);
        }
    }
}