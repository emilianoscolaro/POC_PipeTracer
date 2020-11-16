using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Plugin.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace POC_PipeTracer.Droid
{
    [Activity(Label = "")]
    public class BaseActivity : AppCompatActivity
    {
        private Dialog progressDialog = null;

        protected override void OnCreate(Bundle bundle)
        {
            //SetTheme(Settings.GetBool(ConfigItems.APP_STYLE_DARK) ?
            //    Resource.Style.AppThemeDark : Resource.Style.AppThemeLight);

            base.OnCreate(bundle);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #region Progress Dialog
        public void ShowLoading(string message = "", string title = "")
        {
            if (!IsFinishing)
            {
                RunOnUiThread(() =>
                {
                    HideLoading();

                    //if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(title))
                    //{
                    //    this.progressDialog = CProgressDialog.Show(this);
                    //}
                    //else
                    //{
                    //    this.progressDialog = CProgressDialog.Show(this, title, message);
                    //}
                });
            }

        }

        public void HideLoading()
        {
            if (this.progressDialog == null) { return; }

            RunOnUiThread(() =>
            {
                this.progressDialog.Dismiss();
            });
        }

        protected bool IsShowingLoading()
        {
            if (this.progressDialog == null) { return false; }

            return this.progressDialog.IsShowing;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
        }

        #endregion
    }
}