﻿/**
 * This library is open source software licensed under terms of the MIT License.
 *
 * Copyright (c) 2020-2022 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
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
 * Version 1.0.0.1
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace LaunchAsDate {

    /// <summary>
    /// Simple error logging class.
    /// </summary>
    public static class ErrorLog {

        /// <summary>
        /// A method to split the stack trace into an array of strings.
        /// </summary>
        /// <param name="stackTrace">Input string.</param>
        /// <returns>Array of strings.</returns>
        private static string[] SplitStackTrace(string stackTrace) {
            StringReader stringReader = new StringReader(stackTrace);
            List<string> lines = new List<string>();
            for (string line; (line = stringReader.ReadLine()) != null;) {
                lines.Add(line);
            }
            return lines.ToArray();
        }

        /// <summary>
        /// A method to log an exception.
        /// </summary>
        /// <param name="exception">An instance of the Exception class.</param>
        public static void WriteLine(Exception exception) {
            try {
                using (StreamWriter streamWriter = File.AppendText(Path.Combine(Application.LocalUserAppDataPath, Constants.ErrorLogFileName))) {
                    StringBuilder stringBuilder = new StringBuilder(DateTime.Now.ToString(Constants.ErrorLogTimeFormat));
                    stringBuilder.Append(Constants.VerticalTab);
                    stringBuilder.Append(exception.TargetSite.Name);
                    stringBuilder.Append(Constants.VerticalTab);
                    stringBuilder.Append(exception.GetType().FullName);
                    stringBuilder.Append(Constants.VerticalTab);
                    stringBuilder.Append(exception.Message);
                    string[] stackTrace = SplitStackTrace(exception.StackTrace);
                    if (stackTrace.Length > 0) {
                        stringBuilder.Append(Constants.VerticalTab);
                        stringBuilder.Append(stackTrace[stackTrace.Length - 1]);
                    }
                    streamWriter.WriteLine(stringBuilder.ToString());
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// A method to log an error message.
        /// </summary>
        /// <param name="errorMessage">Error message.</param>
        public static void WriteLine(string errorMessage) {
            try {
                using (StreamWriter streamWriter = File.AppendText(Path.Combine(Application.LocalUserAppDataPath, Constants.ErrorLogFileName))) {
                    StringBuilder stringBuilder = new StringBuilder(DateTime.Now.ToString(Constants.ErrorLogTimeFormat));
                    stringBuilder.Append(Constants.VerticalTab);
                    stringBuilder.Append(Constants.ErrorLogErrorMessage);
                    stringBuilder.Append(Constants.Colon);
                    stringBuilder.Append(Constants.VerticalTab);
                    if (errorMessage == null) {
                        stringBuilder.Append(Constants.ErrorLogNull);
                    } else if (errorMessage.Equals(string.Empty)) {
                        stringBuilder.Append(Constants.ErrorLogEmptyString);
                    } else if (string.IsNullOrWhiteSpace(errorMessage)) {
                        stringBuilder.Append(Constants.ErrorLogWhiteSpace);
                    } else {
                        stringBuilder.Append(errorMessage);
                    }
                    streamWriter.WriteLine(stringBuilder.ToString());
                }
            } catch (Exception e) {
                Debug.WriteLine(e);
            }
        }
    }
}
