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
using System.Threading;

[assembly: Dependency(typeof(MainActivity))]
namespace POC_PipeTracer.Droid
{
    [Activity(Label="POC Pipe Tracer", MainLauncher = true, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden
     | Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : BaseActivity, INeoReader
    {
        public MainActivity()
        {
            Java.Lang.JavaSystem.LoadLibrary("lavalib");
        }
        internal static MainActivity Instance { get; private set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            ValidatePermissions();
            Instance = this;            
            LoadApplication(new App());
        }
        private async void ValidatePermissions()
        {
            var permissionsManager = new PermissionsManager(Instance);
            await permissionsManager.IsCameraPermissionGranted();
            var isPermissionGranted = await permissionsManager.IsStoragePermissionGranted();
            if (!isPermissionGranted)
            {
                Finish();
                return;
            }
            NeoReaderLicense.LicenseIsValid(Instance);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        void INeoReader.ReadDM()
        {
            ReadDM();
        }
        private void ReadDM()
        {            
            if (NeoReaderLicense.LicenseIsValid(Instance)) { ReadWithNeoReader(); }
        }                
        private void ReadWithNeoReader()
        {
            try
            {
                Intent intent = new Intent(Instance, typeof(NeoReaderViewFinder));
                intent.SetFlags(ActivityFlags.NoHistory);
                intent.PutExtra("txtRnLength", "test");
                intent.PutExtra("txtOvLength", "test");
                intent.PutExtra("txtWeight", "test");
                intent.PutExtra("txtCount", "test");
                Instance.StartActivityForResult(intent,1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}