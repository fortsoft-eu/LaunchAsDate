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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LaunchAsDate {
    public partial class ArgumentParserForm : Form {
        private ArgumentParser argumentParser;
        private int textBoxClicks;
        private Point location;
        private StatusBarPanel statusBarPanel;
        private StatusBarPanel statusBarPanelCapsLock;
        private StatusBarPanel statusBarPanelInsert;
        private StatusBarPanel statusBarPanelNumLock;
        private StatusBarPanel statusBarPanelScrollLock;
        private Timer textBoxClicksTimer;

        public ArgumentParserForm() {
            Icon = Properties.Resources.Icon;

            textBoxClicksTimer = new Timer();
            textBoxClicksTimer.Interval = SystemInformation.DoubleClickTime;
            textBoxClicksTimer.Tick += new EventHandler((sender, e) => {
                textBoxClicksTimer.Stop();
                textBoxClicks = 0;
            });

            argumentParser = new ArgumentParser();

            InitializeComponent();

            Text = new StringBuilder()
                .Append(Program.GetTitle())
                .Append(Constants.Space)
                .Append(Constants.EnDash)
                .Append(Constants.Space)
                .Append(Text)
                .ToString();

            statusBarPanel = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                AutoSize = StatusBarPanelAutoSize.Spring,
                Alignment = HorizontalAlignment.Left
            };
            statusBar.Panels.Add(statusBarPanel);

            statusBarPanelCapsLock = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelCapsLock);

            statusBarPanelNumLock = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelNumLock);

            statusBarPanelInsert = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelInsert);

            statusBarPanelScrollLock = new StatusBarPanel() {
                BorderStyle = StatusBarPanelBorderStyle.Sunken,
                Alignment = HorizontalAlignment.Center,
                Width = 42
            };
            statusBar.Panels.Add(statusBarPanelScrollLock);

            statusBar.ContextMenu = new ContextMenu();
            statusBar.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy, new EventHandler((sender, e) => {
                if (!string.IsNullOrEmpty(statusBarPanel.Text)) {
                    try {
                        Clipboard.SetText(statusBarPanel.Text);
                    } catch (Exception exception) {
                        Debug.WriteLine(exception);
                        ErrorLog.WriteLine(exception);
                    }
                }
            })));
            statusBar.ContextMenu.Popup += new EventHandler((sender, e) => {
                ((ContextMenu)sender).MenuItems[0].Visible = !string.IsNullOrEmpty(statusBarPanel.Text);
            });

            SubscribeEvents();

            StringBuilder stringBuilder = new StringBuilder()
                .Append(Constants.CommandLineSwitchWI)
                .Append(Constants.Space)
                .Append(StaticMethods.EscapeArgument(Constants.ExampleApplicationFilePath))
                .Append(Constants.Space)
                .Append(Constants.CommandLineSwitchWD)
                .Append(Constants.Space)
                .Append(Constants.ISO8601TestDate)
                .Append(Constants.Space)
                .Append(Constants.CommandLineSwitchWA)
                .Append(Constants.Space)
                .Append(StaticMethods.EscapeArgument(Constants.ExampleApplicationArguments1 + Constants.ExampleApplicationArguments2))
                .Append(Constants.Space)
                .Append(Constants.CommandLineSwitchWW)
                .Append(Constants.Space)
                .Append(StaticMethods.EscapeArgument(Constants.ExampleWorkingDirectory))
                .Append(Constants.Space)
                .Append(Constants.CommandLineSwitchWO)
                .Append(Constants.Space)
                .Append(Constants.CommandLineSwitchWS)
                .Append(Constants.Space)
                .Append(Constants.Five);
            textBox1.Text = stringBuilder.ToString();

            textBoxInput.Text = Constants.ExampleApplicationArguments1 + Constants.ExampleApplicationArguments2;
        }

        private static ContextMenu BuildLabelAndCheckBoxContextMenu() {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopy, new EventHandler((sender, e) => {
                try {
                    Clipboard.SetText(((MenuItem)sender).GetContextMenu().SourceControl.Text);
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                }
            })));
            return contextMenu;
        }

        private void EscapeArgument() => textBoxOutput.Text = StaticMethods.EscapeArgument(textBoxInput.Text);

        private void OnApplicationIdle(object sender, EventArgs e) {
            statusBarPanelCapsLock.Text = IsKeyLocked(Keys.CapsLock)
                ? Properties.Resources.CaptionCapsLock
                : string.Empty;
            statusBarPanelNumLock.Text = IsKeyLocked(Keys.NumLock)
                ? Properties.Resources.CaptionNumLock
                : string.Empty;
            statusBarPanelInsert.Text = IsKeyLocked(Keys.Insert)
                ? Properties.Resources.CaptionOverWrite
                : Properties.Resources.CaptionInsert;
            statusBarPanelScrollLock.Text = IsKeyLocked(Keys.Scroll)
                ? Properties.Resources.CaptionScrollLock
                : string.Empty;
        }

        private void OnArgumentStringChanged(object sender, EventArgs e) {
            try {
                argumentParser.ArgumentString = ((TextBox)sender).Text;
                SetStatusBarPanelText(Properties.Resources.ButtonOK);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                SetStatusBarPanelText(new StringBuilder()
                    .Append(Properties.Resources.CaptionError)
                    .Append(Constants.Colon)
                    .Append(Constants.Space)
                    .Append(exception.Message)
                    .ToString());
            } finally {
                SetValues();
            }
        }

        private void OnCheckedChanged(object sender, EventArgs e) => SetCheckBoxes();

        private void OnFormClosing(object sender, FormClosingEventArgs e) => Application.Idle -= new EventHandler(OnApplicationIdle);

        private void OnFormLoad(object sender, EventArgs e) => Application.Idle += new EventHandler(OnApplicationIdle);

        private void OnInputStringChanged(object sender, EventArgs e) => EscapeArgument();

        private void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode.Equals(Keys.Escape)) {
                Close();
            } else if (e.Control && e.KeyCode.Equals(Keys.A) && sender is TextBox) {
                ((TextBox)sender).SelectAll();
            }
        }

        private static void OnKeyPress(object sender, KeyPressEventArgs e) {
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

        private void OpenHelp(object sender, HelpEventArgs e) {
            try {
                StringBuilder url = new StringBuilder()
                    .Append(Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant())
                    .Append(Constants.Slash)
                    .Append(Application.ProductName.ToLowerInvariant())
                    .Append(Constants.Slash);
                Process.Start(url.ToString());
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void SetCheckBoxes() {
            foreach (Control control in Controls) {
                if (control is CheckBox) {
                    ((CheckBox)control).CheckedChanged -= new EventHandler(OnCheckedChanged);
                }
            }
            checkBox1.Checked = argumentParser.OneInstance;
            checkBox2.Checked = argumentParser.IsTest;
            checkBox3.Checked = argumentParser.IsHelp;
            checkBox4.Checked = argumentParser.IsThisTest;
            checkBox5.Checked = argumentParser.HasArguments;
            foreach (Control control in Controls) {
                if (control is CheckBox) {
                    ((CheckBox)control).CheckedChanged += new EventHandler(OnCheckedChanged);
                }
            }
        }

        private void SetStatusBarPanelText(string text) => statusBarPanel.Text = text;

        private void SetTextBoxes() {
            textBox2.Text = argumentParser.ApplicationFilePath;
            textBox3.Text = argumentParser.DateTime.HasValue
                ? argumentParser.DateTime.Value.ToString(Constants.ISO8601DateTimeFormat)
                : null;
            textBox4.Text = argumentParser.ApplicationArguments;
            textBox5.Text = argumentParser.WorkingDirectory;
            textBox6.Text = argumentParser.Interval.ToString();
        }

        private void SetValues() {
            SetTextBoxes();
            SetCheckBoxes();
        }

        private void SubscribeEvents() {
            foreach (Control control in Controls) {
                if (control is Label) {
                    control.ContextMenu = BuildLabelAndCheckBoxContextMenu();
                } else if (control is TextBox) {
                    TextBox textBox = (TextBox)control;
                    textBox.KeyDown += new KeyEventHandler(OnKeyDown);
                    textBox.KeyPress += new KeyPressEventHandler(OnKeyPress);
                    textBox.MouseDown += new MouseEventHandler(OnMouseDown);
                } else if (control is CheckBox) {
                    CheckBox checkBox = (CheckBox)control;
                    checkBox.ContextMenu = BuildLabelAndCheckBoxContextMenu();
                    checkBox.CheckedChanged += new EventHandler(OnCheckedChanged);
                }
            }
            textBox1.TextChanged += new EventHandler(OnArgumentStringChanged);
            textBoxInput.TextChanged += new EventHandler(OnInputStringChanged);
        }
    }
}
