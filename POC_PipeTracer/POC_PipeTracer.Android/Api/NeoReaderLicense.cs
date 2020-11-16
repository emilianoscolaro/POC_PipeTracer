using Android.Content;
using DE.Neom.Neoreadersdk;
using Java.Text;
using System;

namespace POC_PipeTracer.Droid.Api
{
    public class NeoReaderLicense
    {
        // your application identifier
        private static string _appID = "6038";

        // your customer key
        private static byte[] _customerKey =
        {
            (byte) 0x96, (byte) 0x2b, (byte) 0x9e, (byte) 0xc0, (byte) 0x56, (byte) 0x7f, (byte) 0xe0, (byte) 0x4a, (byte) 0x25, (byte) 0xbf, (byte) 0x72, (byte) 0x1e, (byte) 0xa4, (byte) 0x2d, (byte) 0xe7, (byte) 0xdb, (byte) 0x34, (byte) 0xa8, (byte) 0xc0, (byte) 0x0d, (byte) 0xaf, (byte) 0x47, (byte) 0xae, (byte) 0xc6, (byte) 0x1f, (byte) 0xc2, (byte) 0x8a, (byte) 0x88, (byte) 0x16, (byte) 0xff, (byte) 0x58, (byte) 0xbc
        };

        public static bool LicenseIsValid(Context context)
        {
            return GetLicense(context) != null;
        }

        public static License GetLicense(Context context)
        {
            License license = GetSavedLicense(context);

            if (license == null || !license.IsValid)
            {
                GetLicenseFromServerAsync(context);
            }

            return license;
        }

        private static void GetLicenseFromServerAsync(Context context)
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                var license = DonwloadLicense(context);
                if (license != null)
                {
                    SaveLicense(license, context);
                }
            });
        }

        private static byte[] DonwloadLicense(Context context)
        {
            LicenseServerResponse response = new LicenseServerResponse();
            try
            {
                byte[] licenseData = License.Download(context, _appID, _customerKey,
                    /*request1D = */true, /*requestDM = */true, /*requestQR = */true,
                    /*requestAztec = */false, /*requestPDF417 = */false,
                    /*requestViewfinder = */true, /*requestCodeParser = */true,
                    /*requestBrandingOff = */true, response);

                if (response.ResponseCode != 0)
                {
                    //Logger.Info("GetLicense", "LicenseServer returned " + response.ResponseCode + " " +
                    //  ((response.ResponseMessage != null) ? response.ResponseMessage : ""));
                }

                License license = new License(context, _appID, licenseData, _customerKey);

                // check the expiration date of this license
                long expire = license.ExpirationMilis;
                long now = Java.Lang.JavaSystem.CurrentTimeMillis();
                if (now > expire)
                {
                    LogLicenseExpiration(expire, now);
                    throw new InsufficientLicenseException("- License expired. -");
                }

                return licenseData;
            }
            catch (Exception ex)
            {
               // Logger.Error("GetLicense", "Error downloading and creating license: " + ex.Message);
                return null;
            }
        }

        private static void LogLicenseExpiration(long expire, long now)
        {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ssZ");
            sdf.TimeZone = Java.Util.TimeZone.GetTimeZone("UTC");
         //  Logger.Info("NeoReader license", "now: " + sdf.Format(new Java.Util.Date(now)));
         //  Logger.Info("NeoReader license", "expire: " + sdf.Format(new Java.Util.Date(expire)));
        }

        private static License GetSavedLicense(Context context)
        {
            try
            {
                byte[] licenseData = null;
                var filePath = System.IO.Path.Combine(context.FilesDir.AbsolutePath, "temp");
                if (System.IO.File.Exists(filePath))
                {
                    licenseData = System.IO.File.ReadAllBytes(filePath);
                }

                if (licenseData != null)
                {
                    try
                    {
                        License license = new License(context, _appID, licenseData, _customerKey);
                        return license;
                    }
                    catch (Exception ex)
                    {
                        //Logger.Error("GetSavedLicense", "Error creating license: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Error("GetSavedLicense", ex.Message);
            }
            return null;
        }

        private static void SaveLicense(byte[] license, Context context)
        {
            System.IO.Stream fos = null;
            try
            {
                fos = context.OpenFileOutput("temp", FileCreationMode.Private);
                fos.Write(license, 0, license.Length);
            }
            catch (Exception ex)
            {
                //Logger.Error("SaveLicense", ex.Message);
            }
            finally
            {
                try
                {
                    if (fos != null)
                    {
                        fos.Close();
                    }
                }
                catch (System.IO.IOException) { }
            }
        }
    }
}