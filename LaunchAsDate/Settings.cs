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
using System;
using System.Collections;
using System.Text;

namespace LaunchAsDate {

    /// <summary>
    /// This is an implementation of using the PersistentSettings class for
    /// LaunchAsDate.
    /// </summary>
    public class Settings : IDisposable {

        /// <summary>
        /// Field.
        /// </summary>
        private PersistentSettings persistentSettings;

        /// <summary>
        /// Occurs on successful saving all application settings into the Windows
        /// registry.
        /// </summary>
        public event EventHandler Saved;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        public Settings() {
            persistentSettings = new PersistentSettings();
            Load();
        }

        /// <summary>
        /// Represents the setting if the application should check for updates.
        /// The default value is true.
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;

        /// <summary>
        /// Represents whether visual styles will be used when rendering
        /// application windows. The default value is false.
        /// </summary>
        public bool DisableThemes { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool DisableTimeCorrection { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool ForceTimeCorrection { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool OneInstance { get; set; }

        /// <summary>
        /// Represents the printing setting, whether to use soft margins (larger)
        /// or hard margins (smaller). This setting does not apply to the
        /// embedded Chromium browser. The default value is true.
        /// </summary>
        public bool PrintSoftMargins { get; set; } = true;

        /// <summary>
        /// Represents the setting if the application should inform the user
        /// about available updates in the status bar only. If not, a pop-up
        /// window will appear. The default value is false.
        /// </summary>
        public bool StatusBarNotifOnly { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public bool WarningOk { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int DateIndex { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int SpanIndex { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public int SpanValue { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public string ApplicationFilePath { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public string ShortcutName { get; set; }

        /// <summary>
        /// An example of software application setting that will be stored in the
        /// Windows registry.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Loads the software application settings from the Windows registry.
        /// </summary>
        private void Load() {
            IntToBitSettings(persistentSettings.Load("BitSettings", BitSettingsToInt()));
            ApplicationFilePath = persistentSettings.Load("Path", ApplicationFilePath);
            DateIndex = persistentSettings.Load("DateIndex", DateIndex);
            DateTime dateTime = DateTime.Now;
            DateTime.TryParse(persistentSettings.Load("DateTime", dateTime.ToString(Constants.ISO8601DateFormat)), out dateTime);
            DateTime = dateTime;
            SpanValue = persistentSettings.Load("Span", SpanValue);
            SpanIndex = persistentSettings.Load("SpanIndex", SpanIndex);
            Arguments = persistentSettings.Load("Arguments", Arguments);
            WorkingDirectory = persistentSettings.Load("Folder", WorkingDirectory);
            Interval = persistentSettings.Load("Interval", Interval);
            ShortcutName = persistentSettings.Load("Shortcut", ShortcutName);
        }

        /// <summary>
        /// Saves the software application settings into the Windows registry.
        /// </summary>
        public void Save() {
            persistentSettings.Save("BitSettings", BitSettingsToInt());
            persistentSettings.Save("Path", ApplicationFilePath);
            persistentSettings.Save("DateIndex", DateIndex);
            persistentSettings.Save("DateTime", IsToday(DateTime)
                ? Constants.ISO8601EmptyDate
                : DateTime.ToString(Constants.ISO8601DateFormat));
            persistentSettings.Save("Span", SpanValue);
            persistentSettings.Save("SpanIndex", SpanIndex);
            persistentSettings.Save("Arguments", Arguments);
            persistentSettings.Save("Folder", WorkingDirectory);
            persistentSettings.Save("Interval", Interval);
            persistentSettings.Save("Shortcut", ShortcutName);
            Saved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Expands an integer value into some boolean settings.
        /// </summary>
        private void IntToBitSettings(int i) {
            BitArray bitArray = new BitArray(new int[] { i });
            bool[] bitSettings = new bool[bitArray.Count];
            bitArray.CopyTo(bitSettings, 0);
            i = bitSettings.Length - 24;

            WarningOk = bitSettings[--i];
            DisableTimeCorrection = bitSettings[--i];
            ForceTimeCorrection = bitSettings[--i];
            OneInstance = bitSettings[--i];
            DisableThemes = bitSettings[--i];
            PrintSoftMargins = bitSettings[--i];
            StatusBarNotifOnly = bitSettings[--i];
            CheckForUpdates = bitSettings[--i];
        }

        /// <summary>
        /// Compacts some boolean settings into an integer value.
        /// </summary>
        private int BitSettingsToInt() {
            StringBuilder stringBuilder = new StringBuilder(string.Empty.PadRight(24, Constants.Zero))
                .Append(WarningOk ? 1 : 0)
                .Append(DisableTimeCorrection ? 1 : 0)
                .Append(ForceTimeCorrection ? 1 : 0)
                .Append(OneInstance ? 1 : 0)
                .Append(DisableThemes ? 1 : 0)
                .Append(PrintSoftMargins ? 1 : 0)
                .Append(StatusBarNotifOnly ? 1 : 0)
                .Append(CheckForUpdates ? 1 : 0);
            return Convert.ToInt32(stringBuilder.ToString(), 2);
        }

        /// <summary>
        /// This setting will not be directly stored in the Windows registry.
        /// </summary>
        public bool RenderWithVisualStyles { get; set; }

        /// <summary>
        /// Clears the software application values from the Windows registry.
        /// </summary>
        public void Clear() => persistentSettings.Clear();

        /// <summary>Clean up any resources being used.</summary>
        public void Dispose() => persistentSettings.Dispose();

        /// <summary>Check if the provided date is today.</summary>
        /// <param name="dateTime">A DateTime to check.</param>
        /// <returns>True if provided date is today; otherwise false.</returns>
        private static bool IsToday(DateTime dateTime) {
            return dateTime.Day.Equals(DateTime.Now.Day)
                && dateTime.Month.Equals(DateTime.Now.Month)
                && dateTime.Year.Equals(DateTime.Now.Year);
        }
    }
}
