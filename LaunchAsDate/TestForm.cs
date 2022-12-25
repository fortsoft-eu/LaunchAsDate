using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LaunchAsDate {
    public partial class TestForm : Form {
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        [DllImport("user32.dll", EntryPoint = "mouse_event", SetLastError = true)]
        private static extern void MouseEvent(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        private int textBoxClicks;
        private Point location;
        private Timer textBoxClicksTimer, timer;

        public TestForm(string[] args) {
            Icon = Properties.Resources.Icon;
            Text = Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Text;

            textBoxClicksTimer = new Timer();
            textBoxClicksTimer.Interval = SystemInformation.DoubleClickTime;
            textBoxClicksTimer.Tick += new EventHandler((sender, e) => {
                textBoxClicksTimer.Stop();
                textBoxClicks = 0;
            });

            timer = new Timer();
            timer.Interval = 1000;
            timer.Tick += new EventHandler((sender, e) => dateTimePickerCurrentDate.Value = DateTime.Now);

            InitializeComponent();

            textBoxArguments.Text = string.Join(Constants.Space.ToString(), args);
            timer.Start();
        }

        private void Close(object sender, EventArgs e) {
            Close();
        }

        private void TextBoxMouseDown(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                textBoxClicks = 0;
                return;
            }
            TextBox textBox = (TextBox)sender;
            textBoxClicksTimer.Stop();
            if (textBox.SelectionLength > 0) {
                textBoxClicks = 2;
            } else if (textBoxClicks == 0 || Math.Abs(e.X - location.X) < 2 && Math.Abs(e.Y - location.Y) < 2) {
                textBoxClicks++;
            } else {
                textBoxClicks = 0;
            }
            location = e.Location;
            if (textBoxClicks == 3) {
                textBoxClicks = 0;
                MouseEvent(MOUSEEVENTF_LEFTUP, Convert.ToUInt32(Cursor.Position.X), Convert.ToUInt32(Cursor.Position.Y), 0, 0);
                if (textBox.Multiline) {
                    int selectionEnd = Math.Min(textBox.Text.IndexOf(Constants.CarriageReturn, textBox.SelectionStart), textBox.Text.IndexOf(Constants.LineFeed, textBox.SelectionStart));
                    if (selectionEnd < 0) {
                        selectionEnd = textBox.TextLength;
                    }
                    selectionEnd = Math.Max(textBox.SelectionStart + textBox.SelectionLength, selectionEnd);
                    int selectionStart = Math.Min(textBox.SelectionStart, selectionEnd);
                    while (--selectionStart > 0 && textBox.Text[selectionStart] != Constants.LineFeed && textBox.Text[selectionStart] != Constants.CarriageReturn) { }
                    textBox.Select(selectionStart, selectionEnd - selectionStart);
                } else {
                    textBox.SelectAll();
                }
                textBox.Focus();
            } else {
                textBoxClicksTimer.Start();
            }
        }

        private void TextBoxKeyDown(object sender, KeyEventArgs e) {
            if (e.Control && e.KeyCode == Keys.A) {
                e.SuppressKeyPress = true;
                ((TextBox)sender).SelectAll();
            }
        }

        private void OpenHelp(object sender, HelpEventArgs e) {
            try {
                Process.Start(Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant() + Constants.Slash + Application.ProductName.ToLowerInvariant() + Constants.Slash);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }
    }
}
