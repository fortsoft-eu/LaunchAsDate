/**
 * This is open-source software licensed under the terms of the MIT License.
 *
 * Copyright (c) 2020-2023 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 **
 * Version 1.5.1.0
 */

using FortSoft.Tools;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace LaunchAsDate {
    internal class LauncherAsDate : IDisposable {
        private DateTime currentDateTime;
        private Mutex mutex;
        private Process process;
        private string mutexId;

        internal LauncherAsDate() {
            StringBuilder mutexPath = new StringBuilder()
                .Append(Application.CompanyName)
                .Append(Constants.Underscore)
                .Append(Application.ProductName);
            mutexId = Path.Combine(Constants.MutexLocal, mutexPath.ToString());
            process = new Process();
        }

        internal bool DisableTimeCorrection { get; set; }

        internal bool ForceTimeCorrection { get; set; }

        internal bool OneInstance { get; set; }

        internal DateTime DateTime { get; set; }

        internal int Interval { get; set; }

        internal string ApplicationFilePath { get; set; }

        internal string Arguments { get; set; }

        internal string WorkingDirectory { get; set; }

        public void Dispose() {
            if (mutex != null) {
                mutex.Dispose();
            }
            process.Dispose();
        }

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
            return !value.Equals(0);
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
            return !value.Equals(0);
        }

        internal void Launch() {
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
            currentDateTime = StaticMethods.GetSystemTime().AddSeconds(Interval);
            if (!DisableTimeCorrection) {
                if (ForceTimeCorrection || !IsRealTimeUniversal() && !IsDynamicDaylightTimeDisabled()) {
                    DateTime = DateTime.Add(TimeZone.CurrentTimeZone.GetUtcOffset(currentDateTime))
                        .Subtract(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime));
                }
            }
            if (!currentDateTime.Year.Equals(DateTime.Year)
                    || !currentDateTime.Month.Equals(DateTime.Month)
                    || !currentDateTime.Day.Equals(DateTime.Day)) {

                StaticMethods.SetSystemTime(DateTime);
                process.Start();
                Thread.Sleep(Interval * 1000);
                StaticMethods.SetSystemTime(currentDateTime);
            } else {
                process.Start();
            }
        }
    }
}
