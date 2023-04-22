/**
 * This is open-source software licensed under the terms of the MIT License.
 *
 * Copyright (c) 2023 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
 * Copyright (c) 2012 Hamid Sadeghian
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
 * Version 1.1.0.0
 */

using System;
using System.Runtime.InteropServices;

namespace FortSoft.Controls {

    /// <summary>
    /// Implements custom LinkLabel with proper system hand cursor.
    /// </summary>
    public class LinkLabel : System.Windows.Forms.LinkLabel {

        /// <summary>
        /// Constant value found in the WinUser.h header file.
        /// </summary>
        public const int IDC_HAND = 0x7F89;

        /// <summary>
        /// Import
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        /// <summary>
        /// Proper system hand cursor.
        /// </summary>
        private static readonly System.Windows.Forms.Cursor SystemHandCursor =
            new System.Windows.Forms.Cursor(LoadCursor(IntPtr.Zero, IDC_HAND));

        /// <summary>
        /// Overriding OnMouseMove method. If the base class decided to show the
        /// ugly hand cursor show the system hand cursor instead.
        /// </summary>
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);

            if (OverrideCursor == System.Windows.Forms.Cursors.Hand) {
                OverrideCursor = SystemHandCursor;
            }
        }
    }
}
