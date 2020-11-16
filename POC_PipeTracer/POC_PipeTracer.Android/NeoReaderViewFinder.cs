using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DE.Neom.Neoreadersdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Java.Lang;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Util;
using POC_PipeTracer.Droid.Api;

namespace POC_PipeTracer.Droid
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.KeyboardHidden
            | Android.Content.PM.ConfigChanges.ScreenSize, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]

    public class NeoReaderViewFinder : BaseActivity, IRunnable, IViewfinderListener
    {
        private static string TAG = "NeoReaderViewFinder";

        public static string HIDE_TALLY_INFO_CONTROLS = "HideTallyInfoControls";
        public static string ENABLE_QR_CODE_SCAN = "EnableQRCodeScan";
        public static string ENABLE_1D_CODE_SCAN = "Enable1DCodeScan";

        private const int MSG_RESUME = 0x0002;

        private static int ACTIVITY_STARTING = 0x0001;
        private static int ACTIVITY_RUNNING = 0x0002;
        private static int ACTIVITY_FINISHING = 0x0004;
        private static int ACTIVITY_PAUSING = 0x0008;
        private static int ACTIVITY_DIALOG = 0x0010;

        private enum FocusMode { NA, AUTO, MACRO, CONTINUOUS }

        License mLicense;
        FocusMode mFocusMode;
        int mState;
        Handler mHandler;

        ViewfinderView mViewfinder;

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        CancellationToken ct = CancellationToken.None;

        private ProgressDialog _progDialog = null;

        private static bool FlashTurnedOn = false;

        private bool _enableQRCodeScan = false;
        private bool _enable1DCodeScan = false;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Handler object for receiving messages and runnables
            mHandler = new Handler();

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.NeoReaderViewFinder);

            mViewfinder = FindViewById<ViewfinderView>(Resource.Id.viewfinderview);
            // set the activity's lifecycle state
            mViewfinder.OnCreate();
            mViewfinder.Click += mViewfinder_Click;
            CancellationToken ct = tokenSource.Token;
            Task.Factory.StartNew(() => ConfigureViewFinder(), tokenSource.Token);

            ImageButton flashButton;
            //Find the button from our resource layout and wire up the click event
            flashButton = FindViewById<ImageButton>(Resource.Id.buttonZxingFlash);
            flashButton.Click += delegate
            {
                ToggleFlash();
            };

            if (Intent.GetStringExtra(HIDE_TALLY_INFO_CONTROLS) == "true")
            {
                FindViewById<LinearLayout>(Resource.Id.RunningLengthContainer).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.OverallLengthContainer).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.WeightContainer).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.CountContainer).Visibility = ViewStates.Gone;
            }
            else
            {
                FindViewById<TextView>(Resource.Id.txtRnLength).Text = Intent.GetStringExtra("txtRnLength");
                FindViewById<TextView>(Resource.Id.txtOvLength).Text = Intent.GetStringExtra("txtOvLength");
                FindViewById<TextView>(Resource.Id.txtWeight).Text = Intent.GetStringExtra("txtWeight");
                FindViewById<TextView>(Resource.Id.txtCount).Text = Intent.GetStringExtra("txtCount");
            }

            _enableQRCodeScan = Intent.GetStringExtra(ENABLE_QR_CODE_SCAN) == "true";
            _enable1DCodeScan = Intent.GetStringExtra(ENABLE_1D_CODE_SCAN) == "true";
        }

        private void FlashSavedState()
        {
            if (FlashTurnedOn)
            {
                mViewfinder.TurnOnFlash();
            }
        }

        private void ToggleFlash()
        {
            if (FlashTurnedOn)
            {
                FlashTurnedOn = false;
                mViewfinder.TurnOffFlash();
            }
            else
            {
                FlashTurnedOn = true;
                mViewfinder.TurnOnFlash();
            }
        }

        protected override void OnDestroy()
        {
            //Logger.Debug(TAG, "onDestroy");
            base.OnDestroy();
            if (mViewfinder != null)
            {
                mViewfinder.RemoveViewfinderListener(this);
                // set the activity's lifecycle state
                mViewfinder.OnDestroy();
                mViewfinder.Release();
            }
            if (IssetState(ACTIVITY_DIALOG))
            {
                // remove open dialogs to prevent window leaking
                //Log.Verbose(TAG, "onDestroy: leaking dialog detected");
                if (_progDialog != null)
                {
                    _progDialog.Dismiss();
                }
            }
        }

        public void OnError(int error, string message)
        {
            Log.Error(TAG, "error: " + error + " " + message);
            if (error == ViewfinderListener.ErrorCameraNoImage)
            {
                SetState(ACTIVITY_DIALOG);
            }
        }

        public void OnDecodingRectChanged(Rect r)
        {

        }

        void mViewfinder_Click(object sender, EventArgs e)
        {
            if (mState == ACTIVITY_RUNNING)
            {
                mViewfinder.Autofocus();
            }
        }

        private void ConfigureViewFinder()
        {
            mLicense = NeoReaderLicense.GetLicense(this);
            try
            {
                mViewfinder.SetLicense(mLicense);
                int loops = 0;
                while (mViewfinder.Camera == null)
                {
                    //Logger.Debug(TAG, "InitializationTask: camera is not yet available");
                    // camera is not yet available => give it some more time
                    try
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (ct.IsCancellationRequested)
                            return;
                    }
                    catch (InterruptedException)
                    {
                        break;
                    }
                    if (++loops == 3)
                    {
                        break;
                    }
                }
                if (mViewfinder.Camera != null)
                {
                    // set focus mode
                    IList<string> modes = mViewfinder.FocusModes;
                    if (modes != null && modes.Contains("auto"))
                    {
                        mFocusMode = FocusMode.AUTO;
                        mViewfinder.FocusMode = "auto";
                    }
                    else
                    {
                        mFocusMode = FocusMode.NA;
                    }
                    //Logger.Debug(TAG, "Focus mode: " + mFocusMode.ToString());
                }
                if (ct.IsCancellationRequested)
                    return;

                // configure desired decoders
                if (mLicense.IsDataMatrixUnlocked)
                {
                   // ToDo
                   // mViewfinder.UseDMEngine(Settings.GetBool(ConfigItems.dm));
                    mViewfinder.UseDMEngine(true);
                }

                if (_enableQRCodeScan && mLicense.IsQRUnlocked)
                {
                    mViewfinder.UseQREngine(true);
                }

                if (_enable1DCodeScan && mLicense.Is1DUnlocked())
                {
                    mViewfinder.Use1DEngine(true);
                }

                if (ct.IsCancellationRequested)
                    return;
                // set the activity as listener so that we get viewfinder events
                mViewfinder.AddViewfinderListener(this);

                StartLivestreamDecoding();
                // no error:
                return;
            }
            catch (Java.Lang.Exception e)
            {
                //Logger.Error(TAG, e.Message);
                //ShowMessage(GetString(Resource.String.DownloadingLicenceError));
                Finish();
            }
        }

        private void ShowMessage(string message)
        {
            RunOnUiThread(() => Toast.MakeText(this, message, ToastLength.Long).Show());
        }

        private void StartLivestreamDecoding()
        {
            try
            {
                if (mLicense != null)
                {
                    StartAutoFocussingTimer();
                    mViewfinder.StartLivestreamDecoding(mLicense);
                    FlashSavedState();
                }
                else
                {
                    Task.Factory.StartNew(() => ConfigureViewFinder(), tokenSource.Token);
                }
            }
            catch (InsufficientLicenseException e)
            {
                //Logger.Info(TAG, e.Message);
                //RunOnUiThread(() => Toast.MakeText(this, e.Message, ToastLength.Long).Show());
            }
        }

        protected override void OnResume()
        {
            //Logger.Debug(TAG, "onResume");
            base.OnResume();

            // set the activity's lifecycle state
            mViewfinder.OnResume();
            if (IssetState(ACTIVITY_STARTING))
            {
                // app is still starting i.e. InitializationTask is not yet finished
                // => try to resume later in handleMessage
                Message msg = Message.Obtain();
                msg.What = MSG_RESUME;
                mHandler.SendMessageDelayed(msg, 500);
            }
            else
            {
                Resume();
            }
        }

        private void Resume()
        {
            //Logger.Debug(TAG, "resume");
            if (IssetState(ACTIVITY_PAUSING))
            {
                UnsetState(ACTIVITY_PAUSING);
            }
            SetState(ACTIVITY_RUNNING);
            // ensure that the livestream is restarted
            int ret = mViewfinder.StartLivestream();
            if (ret != 0)
            {
                //Logger.Debug(TAG, "startLivestream returned " + ret);
                if (ret == ViewfinderListener.ErrorCameraLost || ret == ViewfinderListener.ErrorCameraNotReady)
                {
                    Message msg = Message.Obtain();
                    msg.What = MSG_RESUME;
                    mHandler.SendMessageDelayed(msg, 500);
                    return;
                }
            }
            if (IssetState(ACTIVITY_DIALOG))
            {
                // application is showing a dialog
            }
            else
            {
                // restart the livestream decoding
                //Logger.Debug(TAG, "resume: startLivestreamDecoding");
                StartLivestreamDecoding();
            }
        }

        public void OnViewfinderInitialized()
        {
        }

        public void OnLivestreamDecodingFailed()
        {
        }
        public void OnSnapshotDecodingSucceeded(Code code)
        {
            //Logger.Debug(TAG, "onSnapshotDecodingSucceeded");
        }

        public void OnSnapshotDecodingFailed()
        {
            //Logger.Debug(TAG, "onSnapshotDecodingFailed");
        }

        public void OnLivestreamDecodingSucceeded(Code code)
        {
            if (IssetState(ACTIVITY_FINISHING))
            {
                // application is finishing => ignore the callback results to prevent window leaking
                return;
            }
            //    if (issetState(ACTIVITY_DIALOG)) {
            //      // due to delayed async mechanisms callbacks can occur while another dialog is
            //      // already being opened
            //      return;
            //    }
            // stop the decoding to prevent further decoding when the dialog is shown
            mViewfinder.StopLivestreamDecoding();
            StopAutoFocussingTimer();
            // or just show the code
            SetState(ACTIVITY_DIALOG);

            //return the result
            Intent returnIntent = new Intent();
            returnIntent.PutExtra("code", code.ToString());
            SetResult(Result.Ok, returnIntent);
            Finish();
        }

        protected override void OnPause()
        {
            //Logger.Debug(TAG, "onPause");
            base.OnPause();
            mHandler.RemoveMessages(MSG_RESUME);
            if (IsFinishing == true)
            {
                //Logger.Debug(TAG, "onPause: isFinishing");
                if (tokenSource != null)
                {
                    tokenSource.Cancel();
                    tokenSource = null;
                }
                UnsetState(ACTIVITY_STARTING | ACTIVITY_RUNNING);
                SetState(ACTIVITY_FINISHING);
            }
            else
            {
                // in rare situations onPause is called during ACTIVITY_STARTING without finishing
                // imho these onPauses are false ones
                // => only reset ACTIVITY_RUNNING if it's set.
                // ACTIVITY_STARTING will be reset in onPostExecute
                if (IssetState(ACTIVITY_RUNNING))
                {
                    UnsetState(ACTIVITY_RUNNING);
                    SetState(ACTIVITY_PAUSING);
                }
            }
            StopAutoFocussingTimer();
            mViewfinder.ReleaseCamera();
            // set the activity's lifecycle state
            mViewfinder.OnPause();
        }

        private void SetState(int state)
        {
            int old = mState;
            mState |= state;
            //Logger.Debug(TAG, "state: " + GetState(old) + " => " + GetState(mState));
        }

        private void UnsetState(int state)
        {
            int old = mState;
            mState &= ~state;
            //Logger.Debug(TAG, "state: " + GetState(old) + " => " + GetState(mState));
        }

        private bool IssetState(int state)
        {
            return ((mState & state) == state);
        }

        private string GetState(int state)
        {
            string s = "";
            if (state == 0)
            {
                return "0";
            }
            if ((state & ACTIVITY_STARTING) == ACTIVITY_STARTING)
            {
                s += "ACTIVITY_STARTING ";
            }
            if ((state & ACTIVITY_RUNNING) == ACTIVITY_RUNNING)
            {
                s += "ACTIVITY_RUNNING ";
            }
            if ((state & ACTIVITY_PAUSING) == ACTIVITY_PAUSING)
            {
                s += "ACTIVITY_PAUSING ";
            }
            if ((state & ACTIVITY_FINISHING) == ACTIVITY_FINISHING)
            {
                s += "ACTIVITY_FINISHING ";
            }
            if ((state & ACTIVITY_DIALOG) == ACTIVITY_DIALOG)
            {
                s += "ACTIVITY_DIALOG ";
            }
            return s;
        }

        private void StartAutoFocussingTimer()
        {
            // to do if(mFocusMode == FocusMode.AUTO && Settings.GetInt(ConfigItems.autofocusPeriod > 0)
            if (mFocusMode == FocusMode.AUTO && 5 > 0)
            {
                //Logger.Debug(TAG, "startAutoFocussingTimer");
                mHandler.RemoveCallbacks(this);
                mHandler.PostDelayed(this, 500);
            }
        }

        private void StopAutoFocussingTimer()
        {
            //Logger.Debug(TAG, "stopAutoFocussingTimer");
            mHandler.RemoveCallbacks(this);
        }

        public bool HandleMessage(Message msg)
        {
            //Logger.Debug(TAG, "handleMessage " + msg.What);
            switch (msg.What)
            {
                case MSG_RESUME:
                    if (IssetState(ACTIVITY_STARTING))
                    {
                        // app is still being initialized => try later to resume
                        Message other = Message.Obtain(msg);
                        mHandler.SendMessageDelayed(other, 500);
                    }
                    else if (IssetState(ACTIVITY_FINISHING) || IssetState(ACTIVITY_PAUSING))
                    {
                        // app is already being finished/paused before it even has been initialized
                        // => do nothing
                    }
                    else
                    {
                        Resume();
                    }
                    break;
            }
            return false;
        }

        public void Run()
        {
            //Logger.Debug(TAG, "autofocus()");
            mViewfinder.Autofocus();
            //To do
            //mHandler.PostDelayed(this, Settings.GetInt(ConfigItems.autofocusPeriod) * 1000);
            mHandler.PostDelayed(this, 5 * 1000);
        }

        public override bool OnKeyDown(Keycode keyCode, KeyEvent e)
        {
            if (keyCode == KeyEvent.KeyCodeFromString("KEYCODE_BACK"))
            {
                Finish();
                return true;
            }
            else if (keyCode == KeyEvent.KeyCodeFromString("KEYCODE_FOCUS"))
            {
                if (mState == ACTIVITY_RUNNING)
                {
                    mViewfinder.Autofocus();
                }
                return true;
            }
            else if (keyCode == KeyEvent.KeyCodeFromString("KEYCODE_CAMERA"))
            {
                return true;
            }
            else
            {
                return base.OnKeyDown(keyCode, e);
            }
        }

        public void OnPermissionNotGranted(int p0)
        {

        }

        //public void OnDecodingRectChanged(Rect p0)
        //{
        //    throw new NotImplementedException();
        //}
    }
}