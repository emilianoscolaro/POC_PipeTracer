using Android.Content;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Threading.Tasks;

namespace POC_PipeTracer.Droid
{
    public class PermissionsManager
    {
        Context context;

        private PermissionsManager() { }
        public PermissionsManager(Context context)
        {
            this.context = context;
        }
        public async Task<bool> IsStoragePermissionGranted()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage))
                    {
                        //await CustomAlert.Instance.ShowOkAsync(Resource.String.permission_storage_title,
                        //    Resource.String.permission_storage_rationale, this.context);
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Storage);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Storage))
                        status = results[Permission.Storage];
                }

                if (status == PermissionStatus.Granted)
                {
                    return true;
                }
                else if (status != PermissionStatus.Unknown)
                {
                    //await CustomAlert.Instance.ShowOkAsync(Resource.String.permission_storage_title,
                    //    Resource.String.permission_storage_denied, this.context);
                }
            }
            catch (Exception ex)
            {
                //await CustomAlert.Instance.ShowErrorAsync(Resource.String.permission_storage_title,
                //    context.GetString(Resource.String.permission_storage_error) + ". " + ex.Message, this.context);
            }
            return false;
        }

        public async Task<bool> IsCameraPermissionGranted()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                    {
                        //await CustomAlert.Instance.ShowOkAsync(Resource.String.permission_camera_title,
                        //    Resource.String.permission_camera_rationale, this.context);
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera);
                    //Best practice to always check that the key exists
                    if (results.ContainsKey(Permission.Camera))
                        status = results[Permission.Camera];
                }

                if (status == PermissionStatus.Granted)
                {
                    return true;
                }
                else if (status != PermissionStatus.Unknown)
                {
                    //await CustomAlert.Instance.ShowOkAsync(Resource.String.permission_camera_title,
                    //    Resource.String.permission_camera_denied, this.context);
                }
            }
            catch (Exception ex)
            {
                //await CustomAlert.Instance.ShowErrorAsync(Resource.String.permission_camera_title,
                //    context.GetString(Resource.String.permission_camera_error) + ". " + ex.Message, this.context);
            }
            return false;
        }
    }
}