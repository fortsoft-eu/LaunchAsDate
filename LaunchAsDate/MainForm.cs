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

using FostSoft.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LaunchAsDate {
    public partial class MainForm : Form {
        private Form dialog;
        private int textBoxClicks;
        private Point location;
        private Process process;
        private Settings settings;
        private string workingDirectoryTemp, shortcutNameTemp, administratorRegPath;
        private Timer textBoxClicksTimer;

        public MainForm(Settings settings) {
            Text = Program.GetTitle();
            Icon = Properties.Resources.Icon;

            textBoxClicksTimer = new Timer();
            textBoxClicksTimer.Interval = SystemInformation.DoubleClickTime;
            textBoxClicksTimer.Tick += new EventHandler((sender, e) => {
                textBoxClicksTimer.Stop();
                textBoxClicks = 0;
            });

            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;

            openFileDialog.DefaultExt = Constants.ExtensionExe;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            this.settings = settings;
            textBox1.Text = settings.ApplicationFilePath;
            comboBox1.SelectedIndex = settings.DateIndex < 0 || settings.DateIndex > 1 ? 0 : settings.DateIndex;
            if (settings.DateTime >= dateTimePicker.MinDate && settings.DateTime <= dateTimePicker.MaxDate) {
                dateTimePicker.Value = settings.DateTime;
            }
            numericUpDown1.Minimum = Constants.SpanMinimum;
            numericUpDown1.Maximum = Constants.SpanMaximum;
            if (settings.SpanValue < Constants.SpanMinimum || settings.SpanValue > Constants.SpanMaximum
                    || settings.SpanValue.Equals(0)) {

                numericUpDown1.Value = Constants.SpanDefault;
            } else {
                numericUpDown1.Value = settings.SpanValue;
            }
            numericUpDown1.Select(0, numericUpDown1.Text.Length);
            comboBox2.SelectedIndex = settings.SpanIndex < 0 || settings.SpanIndex > 2 ? 0 : settings.SpanIndex;
            textBox2.Text = settings.Arguments;
            textBox3.Text = settings.WorkingDirectory;
            numericUpDown2.Minimum = Constants.IntervalMinimum;
            numericUpDown2.Maximum = Constants.IntervalMaximum;
            if (settings.Interval < Constants.IntervalMinimum || settings.Interval > Constants.IntervalMaximum) {
                numericUpDown2.Value = Constants.IntervalDefault;
            } else {
                numericUpDown2.Value = settings.Interval;
            }
            numericUpDown2.Select(0, numericUpDown2.Text.Length);
            textBox4.Text = settings.ShortcutName;
            checkBox.Checked = settings.OneInstance;
        }

        private List<string> BuildArguments() {
            bool setTestArgument = false;
            List<string> arguments = new List<string>();
            string applicationFilePath = textBox1.Text;
            if (!string.IsNullOrWhiteSpace(applicationFilePath) && !File.Exists(applicationFilePath)) {
                throw new ApplicationException(Properties.Resources.MessageApplicationNotFound);
            }
            if (!string.IsNullOrWhiteSpace(applicationFilePath)) {
                arguments.Add(Constants.CommandLineSwitchWI);
                arguments.Add(StaticMethods.EscapeArgument(applicationFilePath));
            } else if (Program.IsDebugging) {
                arguments.Add(Constants.CommandLineSwitchWI);
                arguments.Add(StaticMethods.EscapeArgument(Application.ExecutablePath));
                setTestArgument = true;
            } else {
                throw new ApplicationException(Properties.Resources.MessageApplicationNotSet);
            }
            if (comboBox1.SelectedIndex > 0) {
                string[] spanUnit = new string[] {
                    Constants.EnglishDay,
                    Constants.EnglishMonth,
                    Constants.EnglishYear
                };
                if (numericUpDown1.Value > 0) {
                    arguments.Add(Constants.CommandLineSwitchWR);
                    arguments.Add(new StringBuilder()
                        .Append(Constants.Plus)
                        .Append(Math.Abs(numericUpDown1.Value).ToString(Constants.NumberSign.ToString()))
                        .Append(spanUnit[comboBox2.SelectedIndex])
                        .ToString());
                } else if (numericUpDown1.Value < 0) {
                    arguments.Add(Constants.CommandLineSwitchWR);
                    arguments.Add(new StringBuilder()
                        .Append(Constants.Hyphen)
                        .Append(Math.Abs(numericUpDown1.Value).ToString(Constants.NumberSign.ToString()))
                        .Append(spanUnit[comboBox2.SelectedIndex])
                        .ToString());
                } else {
                    throw new ApplicationException(Properties.Resources.ExceptionMessageZ);
                }
            } else {
                arguments.Add(Constants.CommandLineSwitchWD);
                arguments.Add(dateTimePicker.Value.ToString(Constants.ISO8601DateFormat));
            }
            if (!string.IsNullOrWhiteSpace(textBox2.Text)) {
                arguments.Add(Constants.CommandLineSwitchWA);
                arguments.Add(StaticMethods.EscapeArgument(textBox2.Text));
            } else if (setTestArgument) {
                arguments.Add(Constants.CommandLineSwitchWA);
                arguments.Add(new StringBuilder()
                    .Append(Constants.QuotationMark)
                    .Append(Constants.CommandLineSwitchWT)
                    .Append(Constants.QuotationMark)
                    .ToString());
            }
            if (Directory.Exists(textBox3.Text)) {
                arguments.Add(Constants.CommandLineSwitchWW);
                arguments.Add(StaticMethods.EscapeArgument(textBox3.Text));
            }
            if (checkBox.Checked) {
                arguments.Add(Constants.CommandLineSwitchWO);
            }
            arguments.Add(Constants.CommandLineSwitchWS);
            arguments.Add(numericUpDown2.Value.ToString());
            return arguments;
        }

        private void Close(object sender, EventArgs e) => Close();

        private void CreateShortcut(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(textBox4.Text)) {
                dialog = new MessageForm(this, Properties.Resources.MessageShortcutNameNotSet, null, MessageForm.Buttons.OK,
                    MessageForm.BoxIcon.Exclamation);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog(this);
                textBox4.Focus();
                textBox4.SelectAll();
                return;
            }
            try {
                string shortcutFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), textBox4.Text);
                if (!shortcutFilePath.EndsWith(Constants.ExtensionLnk, StringComparison.OrdinalIgnoreCase)) {
                    shortcutFilePath += Constants.ExtensionLnk;
                }
                if (File.Exists(shortcutFilePath)) {
                    dialog = new MessageForm(this, Properties.Resources.MessageShortcutAlreadyExists, null, MessageForm.Buttons.YesNo,
                        MessageForm.BoxIcon.Warning, MessageForm.DefaultButton.Button2);
                    dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                    if (!dialog.ShowDialog(this).Equals(DialogResult.Yes)) {
                        textBox4.Focus();
                        textBox4.SelectAll();
                        return;
                    }
                }
                List<string> arguments = BuildArguments();
                ProgramShortcut programShortcut = new ProgramShortcut() {
                    ShortcutFilePath = shortcutFilePath,
                    TargetPath = Application.ExecutablePath,
                    WorkingDirectory = Application.StartupPath,
                    Arguments = string.Join(Constants.Space.ToString(), arguments),
                    IconLocation = textBox1.Text
                };
                programShortcut.Create();
            } catch (ApplicationException exception) {
                ShowApplicationException(exception);
            } catch (Exception exception) {
                ShowException(exception);
            }
        }

        private void Launch(object sender, EventArgs e) {
            try {
                administratorRegPath = Application.CommonAppDataRegistry.Name;
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
                dialog = new MessageForm(this, Properties.Resources.MessageRunAsAdministrator, null, MessageForm.Buttons.OK,
                    MessageForm.BoxIcon.Shield);
                dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                dialog.ShowDialog(this);
                return;
            }
            try {
                List<string> arguments = BuildArguments();

                if (!settings.WarningOk) {
                    StringBuilder stringBuilder = new StringBuilder()
                        .AppendLine(Properties.Resources.MessageLaunchWarning1)
                        .AppendLine()
                        .AppendLine(Properties.Resources.MessageLaunchWarning2)
                        .AppendLine()
                        .AppendLine(Properties.Resources.MessageLaunchWarning3);
                    dialog = new MessageForm(this, stringBuilder.ToString(), null, MessageForm.Buttons.YesNo,
                        MessageForm.BoxIcon.Warning, MessageForm.DefaultButton.Button2);
                    dialog.HelpRequested += new HelpEventHandler(OpenHelp);
                    if (dialog.ShowDialog(this).Equals(DialogResult.Yes)) {
                        settings.WarningOk = true;
                        SaveSettings();
                    }
                }
                if (settings.WarningOk) {
                    process = new Process();
                    process.StartInfo.FileName = Application.ExecutablePath;
                    process.StartInfo.Arguments = string.Join(Constants.Space.ToString(), arguments);
                    process.StartInfo.WorkingDirectory = Application.StartupPath;
                    process.Start();
                    SaveSettings();
                }
            } catch (ApplicationException exception) {
                ShowApplicationException(exception);
            } catch (Exception exception) {
                ShowException(exception);
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e) {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop, false) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e) {
            try {
                textBox1.Text = ((string[])e.Data.GetData(DataFormats.FileDrop, false))[0];
                if (string.IsNullOrWhiteSpace(textBox3.Text) || !Directory.Exists(textBox3.Text)
                        || textBox3.Text.Equals(workingDirectoryTemp)) {

                    textBox3.Text = Path.GetDirectoryName(textBox1.Text);
                    textBox3.SelectAll();
                    workingDirectoryTemp = textBox3.Text;
                }
                if (string.IsNullOrWhiteSpace(textBox4.Text) || textBox4.Text.Equals(shortcutNameTemp)) {
                    textBox4.Text = Program.GetTitle() + Constants.Space + Path.GetFileNameWithoutExtension(textBox1.Text);
                    textBox4.SelectAll();
                    shortcutNameTemp = textBox4.Text;
                }
                textBox1.Focus();
                textBox1.SelectAll();
            } catch (Exception exception) {
                ShowException(exception);
            }
        }

        private void OnFormActivated(object sender, EventArgs e) {
            if (dialog != null) {
                dialog.Activate();
            }
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e) => SaveSettings();

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.Control && e.KeyCode.Equals(Keys.A)) {
                e.SuppressKeyPress = true;
                if (sender is TextBox) {
                    ((TextBox)sender).SelectAll();
                } else if (sender is NumericUpDown) {
                    NumericUpDown numericUpDown = (NumericUpDown)sender;
                    numericUpDown.Select(0, numericUpDown.Text.Length);
                }
            }
        }

        private void OnKeyPress(object sender, KeyPressEventArgs e) {
            if (sender is TextBox) {
                TextBox textBox = (TextBox)sender;
                if (IsKeyLocked(Keys.Insert)
                        && !char.IsControl(e.KeyChar)
                        && !textBox.ReadOnly
                        && textBox.SelectionLength.Equals(0)
                        && textBox.SelectionStart < textBox.TextLength) {

                    int selectionStart = textBox.SelectionStart;
                    StringBuilder stringBuilder = new StringBuilder(textBox.Text);
                    stringBuilder[textBox.SelectionStart] = e.KeyChar;
                    e.Handled = true;
                    textBox.Text = stringBuilder.ToString();
                    textBox.SelectionStart = selectionStart + 1;
                }
            } else if (sender is NumericUpDown) {
                NumericUpDown numericUpDown = (NumericUpDown)sender;
                if (IsKeyLocked(Keys.Insert) && char.IsDigit(e.KeyChar) && !numericUpDown.ReadOnly) {
                    FieldInfo fieldInfo = numericUpDown.GetType().GetField(Constants.NumericUpDownEdit,
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    TextBox textBox = (TextBox)fieldInfo.GetValue(numericUpDown);
                    if (textBox.SelectionLength.Equals(0) && textBox.SelectionStart < textBox.TextLength) {
                        int selectionStart = textBox.SelectionStart;
                        StringBuilder stringBuilder = new StringBuilder(numericUpDown.Text);
                        stringBuilder[textBox.SelectionStart] = e.KeyChar;
                        e.Handled = true;
                        textBox.Text = stringBuilder.ToString();
                        textBox.SelectionStart = selectionStart + 1;
                    }
                }
            }
        }

        private void OnMouseDown(object sender, MouseEventArgs e) {
            if (!e.Button.Equals(MouseButtons.Left)) {
                textBoxClicks = 0;
                return;
            }
            TextBox textBox = (TextBox)sender;
            textBoxClicksTimer.Stop();
            if (textBox.SelectionLength > 0) {
                textBoxClicks = 2;
            } else if (textBoxClicks.Equals(0) || Math.Abs(e.X - location.X) < 2 && Math.Abs(e.Y - location.Y) < 2) {
                textBoxClicks++;
            } else {
                textBoxClicks = 0;
            }
            location = e.Location;
            if (textBoxClicks.Equals(3)) {
                textBoxClicks = 0;
                NativeMethods.MouseEvent(
                    Constants.MOUSEEVENTF_LEFTUP,
                    Convert.ToUInt32(Cursor.Position.X),
                    Convert.ToUInt32(Cursor.Position.Y),
                    0,
                    0);
                Application.DoEvents();
                if (textBox.Multiline) {
                    char[] chars = textBox.Text.ToCharArray();
                    int selectionEnd = Math.Min(
                        Array.IndexOf(chars, Constants.CarriageReturn, textBox.SelectionStart),
                        Array.IndexOf(chars, Constants.LineFeed, textBox.SelectionStart));
                    if (selectionEnd < 0) {
                        selectionEnd = textBox.TextLength;
                    }
                    selectionEnd = Math.Max(textBox.SelectionStart + textBox.SelectionLength, selectionEnd);
                    int selectionStart = Math.Min(textBox.SelectionStart, selectionEnd);
                    while (--selectionStart > 0
                        && !chars[selectionStart].Equals(Constants.LineFeed)
                        && !chars[selectionStart].Equals(Constants.CarriageReturn)) { }
                    textBox.Select(selectionStart, selectionEnd - selectionStart);
                } else {
                    textBox.SelectAll();
                }
                textBox.Focus();
            } else {
                textBoxClicksTimer.Start();
            }
        }

        private void OnSelectedIndexChanged(object sender, EventArgs e) {
            if (comboBox1.SelectedIndex > 0) {
                dateTimePicker.Enabled = false;
                numericUpDown1.Enabled = true;
                comboBox2.Enabled = true;
            } else {
                dateTimePicker.Enabled = true;
                numericUpDown1.Enabled = false;
                comboBox2.Enabled = false;
            }
        }

        private void OnValueChanged(object sender, EventArgs e) {
            label6.Text = numericUpDown2.Value > 1 ? Properties.Resources.CaptionSeconds : Properties.Resources.CaptionSecond;
        }

        private void OpenHelp(object sender, EventArgs e) {
            if (InvokeRequired) {
                Invoke(new EventHandler(OpenHelp), sender, e);
            } else {
                try {
                    StringBuilder url = new StringBuilder()
                        .Append(Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant())
                        .Append(Constants.Slash)
                        .Append(Application.ProductName.ToLowerInvariant())
                        .Append(Constants.Slash);
                    Process.Start(url.ToString());
                } catch (Exception exception) {
                    ShowException(exception);
                }
            }
        }

        private void SaveSettings() {
            settings.ApplicationFilePath = textBox1.Text;
            settings.DateIndex = comboBox1.SelectedIndex;
            settings.DateTime = dateTimePicker.Value;
            settings.SpanValue = (int)numericUpDown1.Value;
            settings.SpanIndex = comboBox2.SelectedIndex;
            settings.Arguments = textBox2.Text;
            settings.WorkingDirectory = textBox3.Text;
            settings.Interval = (int)numericUpDown2.Value;
            settings.ShortcutName = textBox4.Text;
            settings.OneInstance = checkBox.Checked;
            settings.Save();
        }

        private void SelectApplication(object sender, EventArgs e) {
            try {
                if (!string.IsNullOrEmpty(textBox1.Text)) {
                    string directoryPath = Path.GetDirectoryName(textBox1.Text);
                    if (Directory.Exists(directoryPath)) {
                        openFileDialog.InitialDirectory = directoryPath;
                    }
                    if (File.Exists(textBox1.Text)) {
                        openFileDialog.FileName = Path.GetFileName(textBox1.Text);
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
            try {
                if (openFileDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    textBox1.Text = openFileDialog.FileName;
                    if (string.IsNullOrWhiteSpace(textBox3.Text) || !Directory.Exists(textBox3.Text)
                            || textBox3.Text.Equals(workingDirectoryTemp)) {

                        textBox3.Text = Path.GetDirectoryName(textBox1.Text);
                        textBox3.SelectAll();
                        workingDirectoryTemp = textBox3.Text;
                    }
                    if (string.IsNullOrWhiteSpace(textBox4.Text) || textBox4.Text.Equals(shortcutNameTemp)) {
                        textBox4.Text = new StringBuilder()
                            .Append(Program.GetTitle())
                            .Append(Constants.Space)
                            .Append(Path.GetFileNameWithoutExtension(textBox1.Text))
                            .ToString();
                        textBox4.SelectAll();
                        shortcutNameTemp = textBox4.Text;
                    }
                }
            } catch (Exception exception) {
                ShowException(exception);
            } finally {
                textBox1.Focus();
                textBox1.SelectAll();
            }
        }

        private void SelectFolder(object sender, EventArgs e) {
            try {
                if (!string.IsNullOrEmpty(textBox3.Text)) {
                    if (Directory.Exists(textBox3.Text)) {
                        folderBrowserDialog.SelectedPath = textBox3.Text;
                    }
                }
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
            try {
                if (folderBrowserDialog.ShowDialog(this).Equals(DialogResult.OK)) {
                    if (!textBox3.Text.Equals(folderBrowserDialog.SelectedPath)) {
                        textBox3.Text = folderBrowserDialog.SelectedPath;
                        workingDirectoryTemp = textBox3.Text;
                    }
                }
            } catch (Exception exception) {
                ShowException(exception);
            } finally {
                textBox3.Focus();
                textBox3.SelectAll();
            }
        }

        private void ShowAbout(object sender, EventArgs e) {
            dialog = new AboutForm();
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            dialog.ShowDialog(this);
        }

        private void ShowApplicationException(ApplicationException exception) {
            Debug.WriteLine(exception);
            ErrorLog.WriteLine(exception);
            dialog = new MessageForm(this, exception.Message, null, MessageForm.Buttons.OK, MessageForm.BoxIcon.Exclamation);
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            dialog.ShowDialog(this);
        }

        private void ShowException(Exception exception) => ShowException(exception, null);

        private void ShowException(Exception exception, string statusMessage) {
            Debug.WriteLine(exception);
            ErrorLog.WriteLine(exception);
            StringBuilder title = new StringBuilder()
                .Append(Program.GetTitle())
                .Append(Constants.Space)
                .Append(Constants.EnDash)
                .Append(Constants.Space)
                .Append(Properties.Resources.CaptionError);
            dialog = new MessageForm(this, exception.Message, title.ToString(), MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
            dialog.HelpRequested += new HelpEventHandler(OpenHelp);
            dialog.ShowDialog(this);
        }
    }
}
