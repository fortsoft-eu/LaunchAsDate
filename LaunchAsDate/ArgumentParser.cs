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
 * Version 1.5.1.1
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LaunchAsDate {
    internal class ArgumentParser {
        private bool argumentsSet;
        private bool dateSet;
        private bool expectingArguments;
        private bool expectingDate;
        private bool expectingFilePath;
        private bool expectingFolderPath;
        private bool expectingInterval;
        private bool expectingSpan;
        private bool filePathSet;
        private bool folderPathSet;
        private bool hasArguments;
        private bool helpSet;
        private bool intervalSet;
        private bool oneInstanceSet;
        private bool spanSet;
        private bool testSet;
        private bool thisTestSet;
        private DateTime? dateTime;
        private int interval;
        private List<string> arguments;
        private Regex spanRegex;
        private string applicationArguments;
        private string applicationFilePath;
        private string argumentString;
        private string workingDirectory;

        internal ArgumentParser() {
            StringBuilder spanPattern = new StringBuilder()
                .Append(Constants.SpanPatternStart)
                .Append(Constants.OpeningParenthesis)
                .Append(Constants.EnglishDay)
                .Append(Constants.VerticalBar)
                .Append(Constants.EnglishDays)
                .Append(Constants.VerticalBar)
                .Append(Constants.EnglishMonth)
                .Append(Constants.VerticalBar)
                .Append(Constants.EnglishMonths)
                .Append(Constants.VerticalBar)
                .Append(Constants.EnglishYear)
                .Append(Constants.VerticalBar)
                .Append(Constants.EnglishYears)
                .Append(Constants.ClosingParenthesis)
                .Append(Constants.DollarSign);
            spanRegex = new Regex(spanPattern.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Reset();
        }

        internal bool HasArguments => hasArguments;

        internal bool IsHelp => helpSet;

        internal bool IsTest => testSet;

        internal bool IsThisTest => thisTestSet;

        internal bool OneInstance => oneInstanceSet;

        internal DateTime? DateTime => dateTime;

        internal int Interval => interval;

        internal string ApplicationArguments => applicationArguments;

        internal string ApplicationFilePath => applicationFilePath;

        internal string ArgumentString {
            get {
                if (string.IsNullOrEmpty(argumentString) && arguments.Count > 0) {
                    return string.Join(Constants.Space.ToString(), arguments);
                }
                return argumentString;
            }
            set {
                Reset();
                argumentString = value;
                arguments = Parse(argumentString);
                try {
                    Evaluate();
                } catch (Exception exception) {
                    Reset();
                    throw exception;
                }
            }
        }

        internal string WorkingDirectory => workingDirectory;

        internal string[] Arguments {
            get {
                return arguments.ToArray();
            }
            set {
                Reset();
                arguments = new List<string>(value.Length);
                arguments.AddRange(value);
                try {
                    Evaluate();
                } catch (Exception exception) {
                    Reset();
                    throw exception;
                }
            }
        }

        private void Evaluate() {
            DateTime systemDateTime = StaticMethods.GetSystemTime();
            foreach (string arg in arguments) {
                string argument = arg;
                hasArguments = true;

                // Input file path: Application to launch.
                if (argument.Equals(Constants.CommandLineSwitchUI) || argument.Equals(Constants.CommandLineSwitchWI)) {
                    if (filePathSet || expectingFilePath) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageI);
                    }
                    if (expectingDate || expectingSpan || expectingArguments || expectingInterval || expectingFolderPath || testSet
                        || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingFilePath = true;

                    // Absolute date in format yyyy-mm-dd.
                } else if (argument.Equals(Constants.CommandLineSwitchUD) || argument.Equals(Constants.CommandLineSwitchWD)) {
                    if (dateSet || expectingDate) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageD);
                    }
                    if (dateTime.HasValue) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageT);
                    }
                    if (expectingFilePath || expectingSpan || expectingArguments || expectingInterval || expectingFolderPath || testSet
                            || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingDate = true;

                    // Relative time span in format for example -9day, -1year, +2month.
                } else if (argument.Equals(Constants.CommandLineSwitchUR) || argument.Equals(Constants.CommandLineSwitchWR)) {
                    if (spanSet || expectingSpan) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageQ);
                    }
                    if (dateTime.HasValue) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageT);
                    }
                    if (expectingFilePath || expectingDate || expectingArguments || expectingInterval || expectingFolderPath || testSet
                            || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingSpan = true;

                    // Arguments passed to the launched application.
                } else if (argument.Equals(Constants.CommandLineSwitchUA) || argument.Equals(Constants.CommandLineSwitchWA)) {
                    if (argumentsSet || expectingArguments) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageA);
                    }
                    if (expectingFilePath || expectingDate || expectingSpan || expectingInterval || expectingFolderPath || testSet
                            || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingArguments = true;

                    // Interval in seconds to return to the current date.
                } else if (argument.Equals(Constants.CommandLineSwitchUS) || argument.Equals(Constants.CommandLineSwitchWS)) {
                    if (intervalSet || expectingInterval) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageS);
                    }
                    if (expectingFilePath || expectingDate || expectingSpan || expectingArguments || expectingFolderPath || testSet
                            || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingInterval = true;

                    // Working folder path.
                } else if (argument.Equals(Constants.CommandLineSwitchUW) || argument.Equals(Constants.CommandLineSwitchWW)) {
                    if (folderPathSet || expectingFolderPath) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageW);
                    }
                    if (expectingFilePath || expectingDate || expectingSpan || expectingArguments || expectingInterval || testSet
                            || helpSet || thisTestSet) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    expectingFolderPath = true;

                    // Allows only one instance.
                } else if (argument.Equals(Constants.CommandLineSwitchUO) || argument.Equals(Constants.CommandLineSwitchWO)) {
                    if (oneInstanceSet || testSet || helpSet || thisTestSet || expectingFilePath || expectingDate || expectingSpan
                            || expectingArguments || expectingInterval || expectingFolderPath) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    oneInstanceSet = true;

                    // Test mode (will show form with the date launched).
                } else if (argument.Equals(Constants.CommandLineSwitchUT) || argument.Equals(Constants.CommandLineSwitchWT)) {
                    if (filePathSet || dateSet || spanSet || argumentsSet || intervalSet || oneInstanceSet || testSet || helpSet
                            || thisTestSet || expectingFilePath || expectingDate || expectingSpan || expectingArguments
                            || expectingInterval || expectingFolderPath) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    testSet = true;

                    // Will show help.
                } else if (argument.Equals(Constants.CommandLineSwitchUH) || argument.Equals(Constants.CommandLineSwitchWH)
                        || argument.Equals(Constants.CommandLineSwitchUQ) || argument.Equals(Constants.CommandLineSwitchWQ)) {

                    if (filePathSet || dateSet || spanSet || argumentsSet || intervalSet || oneInstanceSet || testSet || helpSet
                            || thisTestSet || expectingFilePath || expectingDate || expectingSpan || expectingArguments
                            || expectingInterval || expectingFolderPath) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    helpSet = true;

                    // Test mode (ArgumentParser test).
                } else if (argument.Equals(Constants.CommandLineSwitchUU) || argument.Equals(Constants.CommandLineSwitchWU)) {
                    if (filePathSet || dateSet || spanSet || argumentsSet || intervalSet || oneInstanceSet || testSet || helpSet
                            || thisTestSet || expectingFilePath || expectingDate || expectingSpan || expectingArguments
                            || expectingInterval || expectingFolderPath) {

                        throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                    }
                    thisTestSet = true;
                } else if (expectingFilePath) {
                    applicationFilePath = argument;
                    expectingFilePath = false;
                    filePathSet = true;
                } else if (expectingDate) {
                    if (dateTime.HasValue) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageT);
                    }
                    dateTime = System.DateTime.Parse(argument).Add(systemDateTime.TimeOfDay);
                    expectingDate = false;
                    dateSet = true;
                } else if (expectingSpan) {
                    if (dateTime.HasValue) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageT);
                    }
                    if (!spanRegex.IsMatch(argument)) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageF);
                    }
                    string[] span = spanRegex.Split(argument);
                    if (int.Parse(span[2]).Equals(0)) {
                        throw new ApplicationException(Properties.Resources.ExceptionMessageZ);
                    }
                    if (span[3].Equals(Constants.EnglishYear, StringComparison.OrdinalIgnoreCase)
                            || span[3].Equals(Constants.EnglishYears, StringComparison.OrdinalIgnoreCase)) {

                        dateTime = systemDateTime.AddYears(span[1].Equals(Constants.Hyphen.ToString())
                            ? 0 - int.Parse(span[2])
                            : int.Parse(span[2]));
                    } else if (span[3].Equals(Constants.EnglishMonth, StringComparison.OrdinalIgnoreCase)
                            || span[3].Equals(Constants.EnglishMonth, StringComparison.OrdinalIgnoreCase)) {

                        dateTime = systemDateTime.AddMonths(span[1].Equals(Constants.Hyphen.ToString())
                            ? 0 - int.Parse(span[2])
                            : int.Parse(span[2]));
                    } else {
                        dateTime = systemDateTime.AddDays(span[1].Equals(Constants.Hyphen.ToString())
                            ? 0 - double.Parse(span[2])
                            : double.Parse(span[2]));
                    }
                    expectingSpan = false;
                    spanSet = true;
                } else if (expectingArguments) {
                    applicationArguments = argument;
                    expectingArguments = false;
                    argumentsSet = true;
                } else if (expectingFolderPath) {
                    workingDirectory = argument;
                    expectingFolderPath = false;
                    folderPathSet = true;
                } else if (expectingInterval) {
                    interval = int.Parse(argument);
                    if (interval < Constants.IntervalMinimum || interval > Constants.IntervalMaximum) {
                        throw new ApplicationException(string.Format(Properties.Resources.ExceptionMessageN, Constants.IntervalMaximum));
                    }
                    expectingInterval = false;
                    intervalSet = true;
                } else if (argument.StartsWith(Constants.Hyphen.ToString()) || argument.StartsWith(Constants.Slash.ToString())) {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageU);
                } else {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageM);
                }
            }
            if (expectingFilePath || expectingDate || expectingSpan || expectingArguments || expectingInterval || expectingFolderPath) {
                throw new ApplicationException(Properties.Resources.ExceptionMessageM);
            }
            if (hasArguments && !testSet && !helpSet && !thisTestSet) {
                if (!dateTime.HasValue && interval.Equals(0)) {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageJ);
                }
                if (!dateTime.HasValue) {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageK);
                }
                if (interval.Equals(0)) {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageL);
                }
            }
        }

        private void Reset() {
            applicationArguments = string.Empty;
            applicationFilePath = string.Empty;
            argumentsSet = false;
            dateSet = false;
            dateTime = null;
            expectingArguments = false;
            expectingDate = false;
            expectingFilePath = false;
            expectingFolderPath = false;
            expectingInterval = false;
            expectingSpan = false;
            filePathSet = false;
            folderPathSet = false;
            hasArguments = false;
            helpSet = false;
            interval = 0;
            intervalSet = false;
            oneInstanceSet = false;
            spanSet = false;
            testSet = false;
            thisTestSet = false;
            workingDirectory = string.Empty;
        }

        private static List<string> Parse(string str) {
            char[] c = str.ToCharArray();
            List<string> arguments = new List<string>();
            StringBuilder stringBuilder = new StringBuilder();
            bool e = false, d = false, s = false;
            for (int i = 0; i < c.Length; i++) {
                if (!s) {
                    if (c[i].Equals(Constants.Space)) {
                        continue;
                    }
                    d = c[i].Equals(Constants.QuotationMark);
                    s = true;
                    e = false;
                    if (d) {
                        continue;
                    }
                }
                if (d) {
                    if (c[i].Equals(Constants.BackSlash)) {
                        if (i + 1 < c.Length && c[i + 1].Equals(Constants.QuotationMark)) {
                            stringBuilder.Append(c[++i]);
                        } else {
                            stringBuilder.Append(c[i]);
                        }
                    } else if (c[i].Equals(Constants.QuotationMark)) {
                        if (i + 1 < c.Length && c[i + 1].Equals(Constants.QuotationMark)) {
                            stringBuilder.Append(c[++i]);
                        } else {
                            d = false;
                            e = true;
                        }
                    } else {
                        stringBuilder.Append(c[i]);
                    }
                } else if (s) {
                    if (c[i].Equals(Constants.Space)) {
                        s = false;
                        arguments.Add(e ? stringBuilder.ToString() : stringBuilder.ToString().TrimEnd(Constants.Space));
                        stringBuilder = new StringBuilder();
                    } else if (!e) {
                        stringBuilder.Append(c[i]);
                    }
                }
            }
            if (stringBuilder.Length > 0) {
                arguments.Add(stringBuilder.ToString());
            }
            return arguments;
        }
    }
}
