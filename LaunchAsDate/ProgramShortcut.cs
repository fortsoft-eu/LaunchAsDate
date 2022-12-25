/**
 * This library is open source software licensed under terms of the MIT License.
 *
 * Copyright (c) 2019-2022 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
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

using IWshRuntimeLibrary;

namespace FostSoft.Tools {

    /// <summary>
    /// Tool for creating Windows shortcuts (*.lnk).
    /// </summary>
    public class ProgramShortcut {

        /// <summary>
        /// Field
        /// </summary>
        private WshShell wshShell;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramShortcut"/>
        /// class.
        /// </summary>
        public ProgramShortcut() {
            wshShell = new WshShell();
        }

        /// <summary>
        /// Shortcut file path.
        /// </summary>
        public string ShortcutFilePath { get; set; }

        /// <summary>
        /// Target path.
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Working directory.
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// Arguments.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Icon location.
        /// </summary>
        public string IconLocation { get; set; }

        /// <summary>
        /// Creates Windows shortcut.
        /// </summary>
        public void Create() {
            IWshShortcut shortcut = (IWshShortcut)wshShell.CreateShortcut(ShortcutFilePath);
            shortcut.TargetPath = TargetPath;
            shortcut.WorkingDirectory = WorkingDirectory;
            shortcut.Arguments = Arguments;
            shortcut.IconLocation = IconLocation;
            shortcut.Save();
        }
    }
}
