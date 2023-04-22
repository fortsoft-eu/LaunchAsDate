﻿/**
 * This is open-source software licensed under the terms of the MIT License.
 *
 * Copyright (c) 2009-2023 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
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
 * Version 1.1.1.1
 */

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace FortSoft.Tools {

    /// <summary>
    /// Implements a set of methods to ensure that only one instance of the
    /// application runs, or only one instance of a particular form runs, or to
    /// find the main window of the desired application and give it focus.
    /// </summary>
    public static class SingleInstance {

        /// <summary>
        /// Windows API constant.
        /// </summary>
        private const int SW_RESTORE = 9;

        /// <summary>
        /// Constants.
        /// </summary>
        private const string Local = "Local";
        private const string Underscore = "_";

        /// <summary>
        /// Field.
        /// </summary>
        private static Mutex mutex;

        /// <summary>
        /// Imports.
        /// </summary>
        [DllImport("user32.dll")]
        private static extern int ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int IsIconic(IntPtr hWnd);

        /// <summary>
        /// Restore application window if minimized. Do not restore if already in
        /// normal or maximized window state, since we don't want to change the
        /// current state of the window. Then focus the window.
        /// </summary>
        /// <returns>True if succeeded.</returns>
        public static bool FocusRunning() => FocusRunning(null);

        /// <summary>
        /// Restore application window if minimized. Do not restore if already in
        /// normal or maximized window state, since we don't want to change the
        /// current state of the window. Then focus the window.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <returns>True if succeeded.</returns>
        public static bool FocusRunning(string filePath) => FocusRunning(filePath, regex: null);

        /// <summary>
        /// Restore application window if minimized. Do not restore if already in
        /// normal or maximized window state, since we don't want to change the
        /// current state of the window. Then focus the window.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="regex">Regex object to perform comparison.</param>
        /// <returns>True if succeeded.</returns>
        public static bool FocusRunning(string filePath, Regex regex) {
            IntPtr hWnd = GetWindowHandle(filePath, regex);
            if (!hWnd.Equals(IntPtr.Zero)) {
                if (!IsIconic(hWnd).Equals(0)) {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                }
                SetForegroundWindow(hWnd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restore application window if minimized. Do not restore if already in
        /// normal or maximized window state, since we don't want to change the
        /// current state of the window. Then focus the window.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <returns>True if succeeded.</returns>
        public static bool FocusRunning(string filePath, string mainWindowTitle) {
            return FocusRunning(filePath, mainWindowTitle, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Restore application window if minimized. Do not restore if already in
        /// normal or maximized window state, since we don't want to change the
        /// current state of the window. Then focus the window.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="comparisonType">Substring comparison type.</param>
        /// <returns>True if succeeded.</returns>
        public static bool FocusRunning(string filePath, string mainWindowTitle, StringComparison comparisonType) {
            IntPtr hWnd = GetWindowHandle(filePath, mainWindowTitle, comparisonType);
            if (!hWnd.Equals(IntPtr.Zero)) {
                if (!IsIconic(hWnd).Equals(0)) {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                }
                SetForegroundWindow(hWnd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restore application window if minimized. Do not restore if already in
        /// normal or maximized window state, since we don't want to change the
        /// current state of the window. Then focus the window.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="culture">Culture for string comparison.</param>
        /// <returns>True if succeeded.</returns>
        public static bool FocusRunning(string filePath, string mainWindowTitle, CultureInfo culture) {
            return FocusRunning(filePath, mainWindowTitle, culture, CompareOptions.None);
        }

        /// <summary>
        /// Restore application window if minimized. Do not restore if already in
        /// normal or maximized window state, since we don't want to change the
        /// current state of the window. Then focus the window.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="culture">Culture for string comparison.</param>
        /// <param name="options">Compare options.</param>
        /// <returns>True if succeeded.</returns>
        public static bool FocusRunning(string filePath, string mainWindowTitle, CultureInfo culture, CompareOptions options) {
            IntPtr hWnd = GetWindowHandle(filePath, mainWindowTitle, culture, options);
            if (!hWnd.Equals(IntPtr.Zero)) {
                if (!IsIconic(hWnd).Equals(0)) {
                    ShowWindowAsync(hWnd, SW_RESTORE);
                }
                SetForegroundWindow(hWnd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the first instance that is not this instance, has the same
        /// process name and was started from the same file name and location.
        /// Also check that the process has a valid window handle in this session
        /// to filter out other user's processes.
        /// </summary>
        /// <returns>Window handle.</returns>
        public static IntPtr GetWindowHandle() => GetWindowHandle(null);

        /// <summary>
        /// Gets the window handle of the application according to the specified
        /// path to the executable file if another instance of the application is
        /// already running. If null is given, it will get the window handle of
        /// the instance of the current application, if any.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <returns>Window handle.</returns>
        public static IntPtr GetWindowHandle(string filePath) => GetWindowHandle(filePath, regex: null);

        /// <summary>
        /// Gets the window handle of the application according to the specified
        /// path to the executable file if another instance of the application is
        /// already running. If null is given, it will get the window handle of
        /// the instance of the current application, if any.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="regex">Regex object to perform comparison.</param>
        /// <returns>Window handle.</returns>
        public static IntPtr GetWindowHandle(string filePath, Regex regex) {
            IntPtr hWnd = IntPtr.Zero;
            try {
                Process process = Process.GetCurrentProcess();
                if (filePath == null) {
                    FileSystemInfo processFileInfo = new FileInfo(process.MainModule.FileName);
                    foreach (Process p in Process.GetProcessesByName(process.ProcessName)
                            .Where(new Func<Process, bool>(p => p.SessionId.Equals(process.SessionId)))
                            .ToArray()) {

                        bool eq = !p.Id.Equals(process.Id)
                            && !p.MainWindowHandle.Equals(IntPtr.Zero)
                            && processFileInfo.Name.Equals(new FileInfo(p.MainModule.FileName).Name);
                        if (eq && regex == null || eq && regex.IsMatch(p.MainWindowTitle)) {
                            hWnd = p.MainWindowHandle;
                            break;
                        }
                    }
                } else {
                    foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(filePath))
                            .Where(new Func<Process, bool>(p => p.SessionId.Equals(process.SessionId)))
                            .ToArray()) {

                        bool eq = string.Compare(p.MainModule.FileName, filePath, StringComparison.OrdinalIgnoreCase).Equals(0)
                            && !p.MainWindowHandle.Equals(IntPtr.Zero);
                        if (eq && regex == null || eq && regex.IsMatch(p.MainWindowTitle)) {
                            hWnd = p.MainWindowHandle;
                            break;
                        }
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
            return hWnd;
        }

        /// <summary>
        /// Gets the window handle of the application according to the specified
        /// path to the executable file if another instance of the application is
        /// already running. If null is given, it will get the window handle of
        /// the instance of the current application, if any.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <returns>Window handle.</returns>
        public static IntPtr GetWindowHandle(string filePath, string mainWindowTitle) {
            return GetWindowHandle(filePath, mainWindowTitle, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the window handle of the application according to the specified
        /// path to the executable file if another instance of the application is
        /// already running. If null is given, it will get the window handle of
        /// the instance of the current application, if any.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="comparisonType">Substring comparison type.</param>
        /// <returns>Window handle.</returns>
        public static IntPtr GetWindowHandle(string filePath, string mainWindowTitle, StringComparison comparisonType) {
            IntPtr hWnd = IntPtr.Zero;
            try {
                Process process = Process.GetCurrentProcess();
                if (filePath == null) {
                    FileSystemInfo processFileInfo = new FileInfo(process.MainModule.FileName);
                    foreach (Process p in Process.GetProcessesByName(process.ProcessName)
                            .Where(new Func<Process, bool>(p => p.SessionId.Equals(process.SessionId)))
                            .ToArray()) {

                        bool eq = !p.Id.Equals(process.Id)
                            && !p.MainWindowHandle.Equals(IntPtr.Zero)
                            && processFileInfo.Name.Equals(new FileInfo(p.MainModule.FileName).Name);
                        if (eq && string.IsNullOrEmpty(mainWindowTitle)
                                || eq && p.MainWindowTitle.IndexOf(mainWindowTitle, comparisonType) >= 0) {

                            hWnd = p.MainWindowHandle;
                            break;
                        }
                    }
                } else {
                    foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(filePath))
                            .Where(new Func<Process, bool>(p => p.SessionId.Equals(process.SessionId)))
                            .ToArray()) {

                        bool eq = string.Compare(p.MainModule.FileName, filePath, StringComparison.OrdinalIgnoreCase).Equals(0)
                            && !p.MainWindowHandle.Equals(IntPtr.Zero);
                        if (eq && string.IsNullOrEmpty(mainWindowTitle)
                                || eq && p.MainWindowTitle.IndexOf(mainWindowTitle, comparisonType) >= 0) {

                            hWnd = p.MainWindowHandle;
                            break;
                        }
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
            return hWnd;
        }

        /// <summary>
        /// Gets the window handle of the application according to the specified
        /// path to the executable file if another instance of the application is
        /// already running. If null is given, it will get the window handle of
        /// the instance of the current application, if any.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="culture">Culture for string comparison.</param>
        /// <returns>Window handle.</returns>
        public static IntPtr GetWindowHandle(string filePath, string mainWindowTitle, CultureInfo culture) {
            return GetWindowHandle(filePath, mainWindowTitle, culture, CompareOptions.None);
        }

        /// <summary>
        /// Gets the window handle of the application according to the specified
        /// path to the executable file if another instance of the application is
        /// already running. If null is given, it will get the window handle of
        /// the instance of the current application, if any.
        /// </summary>
        /// <param name="filePath">Application executable path.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="culture">Culture for string comparison.</param>
        /// <param name="options">Compare options.</param>
        /// <returns>Window handle.</returns>
        public static IntPtr GetWindowHandle(string filePath, string mainWindowTitle, CultureInfo culture, CompareOptions options) {
            IntPtr hWnd = IntPtr.Zero;
            try {
                Process process = Process.GetCurrentProcess();
                if (filePath == null) {
                    FileSystemInfo processFileInfo = new FileInfo(process.MainModule.FileName);
                    foreach (Process p in Process.GetProcessesByName(process.ProcessName)
                            .Where(new Func<Process, bool>(p => p.SessionId.Equals(process.SessionId)))
                            .ToArray()) {

                        bool eq = !p.Id.Equals(process.Id)
                            && !p.MainWindowHandle.Equals(IntPtr.Zero)
                            && processFileInfo.Name.Equals(new FileInfo(p.MainModule.FileName).Name);
                        if (eq && string.IsNullOrEmpty(mainWindowTitle)
                                || eq && string.Compare(p.MainWindowTitle, mainWindowTitle, culture, options).Equals(0)) {

                            hWnd = p.MainWindowHandle;
                            break;
                        }
                    }
                } else {
                    foreach (Process p in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(filePath))
                            .Where(new Func<Process, bool>(p => p.SessionId.Equals(process.SessionId)))
                            .ToArray()) {

                        bool eq = string.Compare(p.MainModule.FileName, filePath, StringComparison.OrdinalIgnoreCase).Equals(0)
                            && !p.MainWindowHandle.Equals(IntPtr.Zero);
                        if (eq && string.IsNullOrEmpty(mainWindowTitle)
                                || eq && string.Compare(p.MainWindowTitle, mainWindowTitle, culture, options).Equals(0)) {

                            hWnd = p.MainWindowHandle;
                            break;
                        }
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
            return hWnd;
        }

        /// <summary>
        /// Checks if at least one instance of the current application is already
        /// running or not according to mutex.
        /// </summary>
        /// <returns>
        /// Returns true if the current application is already running or on
        /// error to prevent multiple instances; otherwise, false.
        /// </returns>
        public static bool IsRunning() => IsRunning(null);

        /// <summary>
        /// Checks if at least one instance of the current application or given
        /// Windows Form of the current application is already running or not
        /// according to mutex. If no Windows Form given, tries to find out if
        /// the current application is running regardless of open forms.
        /// </summary>
        /// <param name="form">A Windows Form to check.</param>
        /// <returns>
        /// Returns true if the current application is already running or on
        /// error to prevent multiple instances; otherwise, false.
        /// </returns>
        public static bool IsRunning(Form form) {
            bool createdNew = false;
            try {
                string mutexName = Application.CompanyName + Underscore + Application.ProductName;
                if (form != null) {
                    mutexName += Underscore + form.Name;
                }
                mutex = new Mutex(true, Path.Combine(Local, mutexName), out createdNew);
                if (createdNew) {
                    mutex.ReleaseMutex();
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
            return !createdNew;
        }

        /// <summary>
        /// Begins running a standard application message loop on the current
        /// thread, and makes the specified form visible. If the current
        /// application is already running, tries to give focus to the form of
        /// the running instance.
        /// </summary>
        /// <param name="form">A Windows Form to run.</param>
        public static void Run(Form form) => Run(form, regex: null);

        /// <summary>
        /// Begins running a standard application message loop on the current
        /// thread, and makes the specified form visible. If the current
        /// application is already running, tries to give focus to the form of
        /// the running instance.
        /// </summary>
        /// <param name="form">A Windows Form to run.</param>
        /// <param name="regex">Regex object to perform comparison.</param>
        public static void Run(Form form, Regex regex) {
            if (IsRunning(regex == null ? null : form)) {
                FocusRunning(null, regex);
            } else {
                Application.Run(form);
            }
        }

        /// <summary>
        /// Begins running a standard application message loop on the current
        /// thread, and makes the specified form visible. If the current
        /// application is already running, tries to give focus to the form of
        /// the running instance.
        /// </summary>
        /// <param name="form">A Windows Form to run.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        public static void Run(Form form, string mainWindowTitle) => Run(form, mainWindowTitle, CultureInfo.CurrentCulture);

        /// <summary>
        /// Begins running a standard application message loop on the current
        /// thread, and makes the specified form visible. If the current
        /// application is already running, tries to give focus to the form of
        /// the running instance.
        /// </summary>
        /// <param name="form">A Windows Form to run.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="comparisonType">Substring comparison type.</param>
        public static void Run(Form form, string mainWindowTitle, StringComparison comparisonType) {
            if (IsRunning(string.IsNullOrEmpty(mainWindowTitle) ? null : form)) {
                FocusRunning(null, mainWindowTitle, comparisonType);
            } else {
                Application.Run(form);
            }
        }

        /// <summary>
        /// Begins running a standard application message loop on the current
        /// thread, and makes the specified form visible. If the current
        /// application is already running, tries to give focus to the form of
        /// the running instance.
        /// </summary>
        /// <param name="form">A Windows Form to run.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="culture">Culture for string comparison.</param>
        public static void Run(Form form, string mainWindowTitle, CultureInfo culture) {
            Run(form, mainWindowTitle, culture, CompareOptions.None);
        }

        /// <summary>
        /// Begins running a standard application message loop on the current
        /// thread, and makes the specified form visible. If the current
        /// application is already running, tries to give focus to the form of
        /// the running instance.
        /// </summary>
        /// <param name="form">A Windows Form to run.</param>
        /// <param name="mainWindowTitle">Main window title.</param>
        /// <param name="culture">Culture for string comparison.</param>
        /// <param name="options">String comparison options.</param>
        public static void Run(Form form, string mainWindowTitle, CultureInfo culture, CompareOptions options) {
            if (IsRunning(string.IsNullOrEmpty(mainWindowTitle) ? null : form)) {
                FocusRunning(null, mainWindowTitle, culture, options);
            } else {
                Application.Run(form);
            }
        }
    }
}
