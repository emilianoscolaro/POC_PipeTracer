using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POC_PipeTracer.Droid.Api
{
    public class Logger
    {
        private static Logger _instance = null;
        private static readonly object syncLock = new object();

        private Logger()
        {
            Init();
        }

        private static Logger Instance
        {
            get
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new Logger();
                    }
                    return _instance;
                }
            }
        }

        private void Init()
        {
            SecureHttpShared.Logger.LOG_TO_FILE = Settings.GetBool(ConfigItems.LOG_ENABLED);
            SecureHttpShared.Logger.ENCRYPT_LOG = true;
        }

        public static void SetWriteLogToFile(bool logToFile)
        {
            SecureHttpShared.Logger.LOG_TO_FILE = logToFile;
        }

        private void LogError(string function, string message)
        {
            SecureHttpShared.Logger.LogError(function, message);
        }

        private void LogWarning(string function, string message)
        {
            SecureHttpShared.Logger.LogWarning(function, message);
        }

        private void LogDebug(string function, string message)
        {
            SecureHttpShared.Logger.LogDebug(function, message);
        }

        private void LogInfo(string function, string message)
        {
            SecureHttpShared.Logger.LogInfo(function, message);
        }

        // public functions
        public static void Error(string function, string message)
        {
            Instance.LogError(function, message);
        }

        public static void Warning(string function, string message)
        {
            Instance.LogWarning(function, message);
        }

        public static void Debug(string function, string message)
        {
            Instance.LogDebug(function, message);
        }

        public static void Info(string function, string message)
        {
            Instance.LogInfo(function, message);
        }

        public static void RefreshConfig()
        {
            Instance.Init();
        }

        public static string CurrentLogFile
        {
            get
            {
                return SecureHttpShared.Logger.CurrentLogFile;
            }
        }
    }
}