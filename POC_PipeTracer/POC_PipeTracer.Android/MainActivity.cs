using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using POC_PipeTracer.Droid.Api;
using Android.Content;
using Xamarin.Forms;
using POC_PipeTracer.Droid;
[assembly: Dependency(typeof(MainActivity))]

namespace POC_PipeTracer.Droid
{
    [Activity(MainLauncher = true ,ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden
     | Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    //[Activity(Label = "POC_PipeTracer", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity ,INeoReader
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        private bool _validNeoReaderLicense = false;
        private void CheckNeoReaderLicense()
        {
            _validNeoReaderLicense = NeoReaderLicense.LicenseIsValid(this);
        }


        private void ReadDM()
        {
            CheckNeoReaderLicense();
            if (true)
            {
                ReadWithNeoReader();
            }
        }

        private void ReadWithNeoReader()
        {
            // Shared.Model.Tally.Totals totals = _headerFragment.GetData();

            try
            {
            Intent intent = new Intent(this, typeof(NeoReaderViewFinder));
            intent.SetFlags(ActivityFlags.NoHistory);
            intent.PutExtra("txtRnLength", "test");
            intent.PutExtra("txtOvLength", "test");
            intent.PutExtra("txtWeight", "test");
            intent.PutExtra("txtCount", "test");

            StartActivityForResult(intent, 1);

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

         void INeoReader.ReadDM()
        {
            ReadDM();
           
        }
    }
}