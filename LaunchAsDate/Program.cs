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
 * Version 1.5.2.0
 */

using FortSoft.Tools;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LaunchAsDate {
    internal static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        internal static void Main(string[] args) {
            if (!Environment.OSVersion.Platform.Equals(PlatformID.Win32NT)) {
                MessageBox.Show(Properties.Resources.MessageApplicationCannotRun, GetTitle(Properties.Resources.CaptionError),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Settings settings = new Settings();
            if (!settings.DisableThemes) {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                settings.RenderWithVisualStyles = Application.RenderWithVisualStyles;
            }
            ArgumentParser argumentParser = new ArgumentParser();
            try {
                argumentParser.Arguments = args;
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (argumentParser.HasArguments) {
                if (argumentParser.IsHelp) {
                    StringBuilder help = new StringBuilder()
                        .AppendLine(Properties.Resources.HelpLine1)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine2)
                        .AppendLine(Properties.Resources.HelpLine3)
                        .AppendLine(Properties.Resources.HelpLine4)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine5)
                        .AppendLine(Properties.Resources.HelpLine6)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine7)
                        .AppendLine(Properties.Resources.HelpLine8)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine9)
                        .AppendLine(Properties.Resources.HelpLine10)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine11)
                        .AppendLine(Properties.Resources.HelpLine12)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine13)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine14)
                        .AppendLine(Properties.Resources.HelpLine15)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine16)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine17)
                        .AppendLine()
                        .AppendLine(Properties.Resources.HelpLine18);
                    StringBuilder replace = new StringBuilder()
                        .Append(Constants.BackSlash)
                        .Append(Constants.LowerCaseT);
                    MessageBox.Show(help.ToString().Replace(replace.ToString(), Constants.VerticalTab.ToString()),
                        GetTitle(Properties.Resources.CaptionHelp), MessageBoxButtons.OK, MessageBoxIcon.Question);
                } else if (argumentParser.IsTest) {
                    try {
                        Application.Run(new TestForm(args));
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                        MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        MessageBox.Show(Properties.Resources.MessageApplicationError, GetTitle(), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                } else if (argumentParser.IsThisTest) {
                    try {
                        Application.Run(new ArgumentParserForm());
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                        MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        MessageBox.Show(Properties.Resources.MessageApplicationError, GetTitle(), MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                } else {
                    LauncherAsDate launcherAsDate = new LauncherAsDate() {
                        ApplicationFilePath = argumentParser.ApplicationFilePath,
                        DateTime = argumentParser.DateTime.Value,
                        Arguments = argumentParser.ApplicationArguments,
                        WorkingDirectory = argumentParser.WorkingDirectory,
                        OneInstance = argumentParser.OneInstance,
                        Interval = argumentParser.Interval,
                        DisableTimeCorrection = settings.DisableTimeCorrection,
                        ForceTimeCorrection = settings.ForceTimeCorrection
                    };
                    try {
                        launcherAsDate.Launch();
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                        MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    } finally {
                        launcherAsDate.Dispose();
                    }
                }
            } else {
                try {
                    SingleInstance.Run(new MainForm(settings), GetTitle());
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    MessageBox.Show(exception.Message, GetTitle(Properties.Resources.CaptionError), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    MessageBox.Show(Properties.Resources.MessageApplicationError, GetTitle(), MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        internal static string GetTitle() {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            string title = null;
            if (attributes.Length > 0) {
                AssemblyTitleAttribute assemblyTitleAttribute = (AssemblyTitleAttribute)attributes[0];
                title = assemblyTitleAttribute.Title;
            }
            return string.IsNullOrEmpty(title) ? Application.ProductName : title;
        }

        internal static string GetTitle(string title) {
            return new StringBuilder()
                .Append(GetTitle())
                .Append(Constants.Space)
                .Append(Constants.EnDash)
                .Append(Constants.Space)
                .Append(title)
                .ToString();
        }

        internal static bool IsDebugging {
            get {
                bool isDebugging = false;
                Debugging(ref isDebugging);
                return isDebugging;
            }
        }

        [Conditional("DEBUG")]
        private static void Debugging(ref bool isDebugging) => isDebugging = true;
    }
}
