using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace LaunchAsDate {
    public class LauncherAsDate {
        [DllImport("kernel32.dll", EntryPoint = "GetSystemTime", SetLastError = true)]
        private static extern void GetSystemTime(ref SystemTime sysTime);

        [DllImport("kernel32.dll", EntryPoint = "SetSystemTime", SetLastError = true)]
        private static extern bool SetSystemTime(ref SystemTime sysTime);

        private string mutexId;
        private Mutex mutex;
        private Process process;
        private DateTime currentDateTime;

        public LauncherAsDate() {
            mutexId = Path.Combine(Constants.MutexLocal, Application.CompanyName + Constants.Underscore + Application.ProductName);
            process = new Process();
        }

        public bool DisableTimeCorrection { get; set; }

        public bool ForceTimeCorrection { get; set; }

        public bool OneInstance { get; set; }

        public DateTime DateTime { get; set; }

        public int Interval { get; set; }

        public string ApplicationFilePath { get; set; }

        public string Arguments { get; set; }

        public string WorkingDirectory { get; set; }

        private bool IsDynamicDaylightTimeDisabled() {
            RegistryKey registryKey = null;
            int value = 0;
            try {
                registryKey = Registry.LocalMachine.OpenSubKey(Constants.RegPathTimeZoneInformationKey);
                value = (int)registryKey.GetValue(Constants.RegValDynamicDaylightTimeDisabled, 0);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
            return value != 0;
        }

        private bool IsRealTimeUniversal() {
            RegistryKey registryKey = null;
            int value = 0;
            try {
                registryKey = Registry.LocalMachine.OpenSubKey(Constants.RegPathTimeZoneInformationKey);
                value = (int)registryKey.GetValue(Constants.RegValRealTimeIsUniversal, 0);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
            return value != 0;
        }

        public void Launch() {
            if (OneInstance) {
                if (SingleInstance.FocusRunning(ApplicationFilePath)) {
                    return;
                }
            }
            bool createdNew = false;
            mutex = new Mutex(true, mutexId, out createdNew);
            if (!createdNew) {
                return;
            }
            process.StartInfo.FileName = ApplicationFilePath;
            process.StartInfo.Arguments = Arguments;
            process.StartInfo.WorkingDirectory = WorkingDirectory;
            currentDateTime = GetSystemTime().AddSeconds(Interval);
            if (!DisableTimeCorrection) {
                if (ForceTimeCorrection || !IsRealTimeUniversal() && !IsDynamicDaylightTimeDisabled()) {
                    DateTime = DateTime.Add(TimeZone.CurrentTimeZone.GetUtcOffset(currentDateTime)).Subtract(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime));
                }
            }
            if (currentDateTime.Year != DateTime.Year || currentDateTime.Month != DateTime.Month || currentDateTime.Day != DateTime.Day) {
                SetSystemTime(DateTime);
                process.Start();
                Thread.Sleep(Interval * 1000);
                SetSystemTime(currentDateTime);
            } else {
                process.Start();
            }
        }

        public static DateTime GetSystemTime() {
            SystemTime systemTime = new SystemTime();
            GetSystemTime(ref systemTime);
            return new DateTime(systemTime.Year, systemTime.Month, systemTime.Day, systemTime.Hour, systemTime.Minute, systemTime.Second);
        }

        private static void SetSystemTime(DateTime dateTime) {
            SystemTime systemTime = new SystemTime() {
                Year = (ushort)dateTime.Year,
                Month = (ushort)dateTime.Month,
                Day = (ushort)dateTime.Day,
                Hour = (ushort)dateTime.Hour,
                Minute = (ushort)dateTime.Minute,
                Second = (ushort)dateTime.Second
            };
            SetSystemTime(ref systemTime);
        }

        private struct SystemTime {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Millisecond;
        }
    }
}
