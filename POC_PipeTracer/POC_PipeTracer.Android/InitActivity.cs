using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using POC_PipeTracer.Droid.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace POC_PipeTracer.Droid
{
    //[Activity(Label = "InitActivity")]
    [Activity(Label = "POC", MainLauncher =true,
    ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden
     | Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class InitActivity : BaseActivity
    {


        static InitActivity()
        {
            //load library liblavalib.so
            Java.Lang.JavaSystem.LoadLibrary("lavalib");
        }

        protected override async void OnCreate(Bundle bundle)
        {
            //if (Intent.GetBooleanExtra("ReloadConfig", false))
            //{
            //    Settings.Init(new SharedPreferences());
            //}

            //ManageTheme();

            base.OnCreate(bundle);

            // SetContentView(Resource.Layout.InitActivity);
            ThreadPool.QueueUserWorkItem(o => {
                NeoReaderLicense.GetLicense(this);
                ReadWithNeoReader();
            });

            var permissionsManager = new PermissionsManager(this);
            var isPermissionGranted = await permissionsManager.IsStoragePermissionGranted();
            if (!isPermissionGranted)
            {
                Finish();
                ContinueToMainActivity();
                return;
            }
            await permissionsManager.IsCameraPermissionGranted();
            // await permissionsManager.IsLocationPermissionGranted();

            // InitApp();
            ContinueToMainActivity();
        }

        private void ContinueToMainActivity()
        {
            //HideViewLocked();

            Intent intent = new Intent(this, typeof(MainActivity));
            string action = Intent.Action;
            string scheme = Intent.Scheme;
            if (Intent.ActionView.Equals(action) && ("file".Equals(scheme) || "content".Equals(scheme)))
            {
                intent.PutExtra("file_to_import", Intent.DataString);
            }
            StartActivity(intent);
            Finish();
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
    }
}