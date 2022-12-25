/**
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
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Windows.Forms;

namespace LaunchAsDate {
    public partial class AboutForm : Form {
        private const int defaultWidth = 420;

        private StringBuilder stringBuilder;
        private Form dialog;

        public AboutForm() {
            InitializeComponent();

            Text = Properties.Resources.CaptionAbout + Constants.Space + Program.GetTitle();
            pictureBox.Image = Properties.Resources.Icon.ToBitmap();

            panelProductInfo.ContextMenu = new ContextMenu();
            panelProductInfo.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyAbout, new EventHandler(CopyAbout)));
            panelWebsite.ContextMenu = new ContextMenu();
            panelWebsite.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyAbout, new EventHandler(CopyAbout)));

            stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(Program.GetTitle());
            stringBuilder.AppendLine(WordWrap(Properties.Resources.Description, labelProductInfo.Font, defaultWidth - 70));
            stringBuilder.Append(Properties.Resources.LabelVersion);
            stringBuilder.Append(Constants.Space);
            stringBuilder.AppendLine(Application.ProductVersion);
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length > 0) {
                AssemblyCopyrightAttribute assemblyCopyrightAttribute = (AssemblyCopyrightAttribute)attributes[0];
                stringBuilder.AppendLine(assemblyCopyrightAttribute.Copyright);
            }
            attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(TargetFrameworkAttribute), false);
            if (attributes.Length > 0) {
                TargetFrameworkAttribute assemblyCopyrightAttribute = (TargetFrameworkAttribute)attributes[0];
                stringBuilder.Append(Properties.Resources.LabelTargetFramework);
                stringBuilder.Append(Constants.Space);
                stringBuilder.AppendLine(assemblyCopyrightAttribute.FrameworkDisplayName);
            }
            labelProductInfo.Text = stringBuilder.ToString();
            labelWebsite.Text = Properties.Resources.LabelWebsite;

            linkLabel.ContextMenu = new ContextMenu();
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyLink, new EventHandler(CopyLink)));
            linkLabel.ContextMenu.MenuItems.Add(new MenuItem(Properties.Resources.MenuItemCopyAbout, new EventHandler(CopyAbout)));
            linkLabel.Text = Properties.Resources.Website.TrimEnd(Constants.Slash).ToLowerInvariant() + Constants.Slash + Application.ProductName.ToLowerInvariant() + Constants.Slash;
            toolTip.SetToolTip(linkLabel, Properties.Resources.ToolTipVisit);
            button.Text = Properties.Resources.ButtonClose;
            stringBuilder.AppendLine();
            stringBuilder.Append(labelWebsite.Text);
            stringBuilder.Append(Constants.Space);
            stringBuilder.Append(linkLabel.Text);
        }

        private void CopyAbout(object sender, EventArgs e) {
            try {
                Clipboard.SetText(stringBuilder.ToString());
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void CopyLink(object sender, EventArgs e) {
            try {
                Clipboard.SetText(((LinkLabel)((MenuItem)sender).GetContextMenu().SourceControl).Text);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
                ErrorLog.WriteLine(exception);
            }
        }

        private void OnLinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                try {
                    Process.Start(((LinkLabel)sender).Text);
                    linkLabel.LinkVisited = true;
                } catch (Exception exception) {
                    Debug.WriteLine(exception);
                    ErrorLog.WriteLine(exception);
                    dialog = new MessageForm(this, exception.Message, Program.GetTitle() + Constants.Space + Constants.EnDash + Constants.Space + Properties.Resources.CaptionError, MessageForm.Buttons.OK, MessageForm.BoxIcon.Error);
                    dialog.ShowDialog();
                }
            }
        }

        private void OnFormActivated(object sender, EventArgs e) {
            if (dialog != null) {
                dialog.Activate();
            }
        }

        private void OnFormLoad(object sender, EventArgs e) {
            linkLabel.Location = new Point(linkLabel.Location.X + labelWebsite.Width + 10, linkLabel.Location.Y);
            button.Select();
            button.Focus();
        }

        private static string WordWrap(string text, Font font, int width) {
            StringBuilder stringBuilder = new StringBuilder();
            StringReader stringReader = new StringReader(text);
            for (string line; (line = stringReader.ReadLine()) != null;) {
                string[] words = line.Split(Constants.Space);
                StringBuilder builder = new StringBuilder();
                foreach (string word in words) {
                    if (builder.Length == 0) {
                        builder.Append(word);
                    } else if (TextRenderer.MeasureText(builder.ToString() + Constants.Space + word, font).Width <= width) {
                        builder.Append(Constants.Space);
                        builder.Append(word);
                    } else {
                        stringBuilder.AppendLine(builder.ToString());
                        builder = new StringBuilder();
                        builder.Append(word);
                    }
                }
                stringBuilder.AppendLine(builder.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
