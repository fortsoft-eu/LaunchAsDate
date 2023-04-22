/**
 * This is open-source software licensed under the terms of the MIT License.
 *
 * Copyright (c) 2009-2023 Petr Červinka - FortSoft <cervinka@fortsoft.eu>
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
 * Version 2.0.1.0
 */

using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows.Forms;

namespace FortSoft.Tools {

    /// <summary>
    /// Implements saving the values of the software application in the Windows
    /// registry for later use. Also implements reading previously saved values
    /// from the Windows registry.
    /// Supported datatypes are: sbyte (System.SByte), byte (System.Byte), short
    /// (System.Int16), ushort (System.UInt16), int (System.Int32), uint
    /// (System.UInt32), long (System.Int64), ulong (System.UInt64), char
    /// (System.Char), float (System.Single), double (System.Double), bool
    /// (System.Boolean), decimal (System.Decimal), string (System.String),
    /// DateTime (System.DateTime), TimeSpan (System.TimeSpan), Color
    /// (System.Drawing.Color).
    /// Datatypes not listed above may work, but the class is not ready for them.
    /// </summary>
    public sealed class PersistentSettings : Component {

        /// <summary>
        /// Constants
        /// </summary>
        private const string Software = "Software";

        /// <summary>
        /// Fields
        /// </summary>
        private string registryPath;
        private RegistryKey registryKeyReadOnly, registryKeyWritable;

        /// <summary>
        /// Events
        /// </summary>
        public event EventHandler<PersistentSettingsEventArgs> Error;
        public event EventHandler<PersistentSettingsEventArgs> Loaded;
        public event EventHandler<PersistentSettingsEventArgs> Saved;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistentSettings"/>
        /// class.
        /// </summary>
        public PersistentSettings() {
            registryPath = Path.Combine(Software, Application.CompanyName, Application.ProductName);
        }

        /// <summary>
        /// Gets Windows registry subtree path.
        /// </summary>
        public string RegistryPath { get; private set; }

        /// <summary>
        /// Clears the application subtree in the Windows registry.
        /// </summary>
        public void Clear() {
            try {
                Registry.CurrentUser.DeleteSubKeyTree(registryPath);
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed;
        /// otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (registryKeyReadOnly != null) {
                    registryKeyReadOnly.Dispose();
                }
                if (registryKeyWritable != null) {
                    registryKeyWritable.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Loads the value from the Windows registry. Supported datatypes are:
        /// sbyte (System.SByte), byte (System.Byte), short (System.Int16),
        /// ushort (System.UInt16), int (System.Int32), uint (System.UInt32),
        /// long (System.Int64), ulong (System.UInt64), char (System.Char),
        /// float (System.Single), double (System.Double), bool (System.Boolean),
        /// decimal (System.Decimal), string (System.String), DateTime
        /// (System.DateTime), TimeSpan (System.TimeSpan), Color
        /// (System.Drawing.Color). Datatypes not listed here may work, but their
        /// support is not implemented.
        /// <param name="valueName">Name of the value.</param>
        /// </summary>
        public T Load<T>(string valueName) {
            try {
                if (registryKeyReadOnly == null) {
                    registryKeyReadOnly = Registry.CurrentUser.OpenSubKey(registryPath);
                }
                if (registryKeyReadOnly != null) {
                    object value = registryKeyReadOnly.GetValue(valueName, null);
                    Loaded?.Invoke(this, new PersistentSettingsEventArgs(registryKeyReadOnly));
                    if (typeof(T) == typeof(bool)) {
                        return (T)Convert.ChangeType((int)value > 0, typeof(T));
                    }
                    if (typeof(T) == typeof(byte)) {
                        return (T)Convert.ChangeType((byte)unchecked((int)value), typeof(T));
                    }
                    if (typeof(T) == typeof(Color)) {
                        return (T)Convert.ChangeType(Color.FromArgb((int)value), typeof(T));
                    }
                    if (typeof(T) == typeof(DateTime)) {
                        return (T)Convert.ChangeType(DateTime.FromBinary((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(decimal)) {
                        return (T)Convert.ChangeType(decimal.Parse((string)value, CultureInfo.InvariantCulture), typeof(T));
                    }
                    if (typeof(T) == typeof(double)) {
                        return (T)Convert.ChangeType(BitConverter.Int64BitsToDouble((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(float)) {
                        return (T)Convert.ChangeType(BitConverter.ToSingle(BitConverter.GetBytes((int)value), 0), typeof(T));
                    }
                    if (typeof(T) == typeof(TimeSpan)) {
                        return (T)Convert.ChangeType(TimeSpan.FromTicks((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(uint)) {
                        return (T)Convert.ChangeType((uint)unchecked((int)value), typeof(T));
                    }
                    if (typeof(T) == typeof(ulong)) {
                        return (T)Convert.ChangeType((ulong)unchecked((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(ushort)) {
                        return (T)Convert.ChangeType((ushort)unchecked((int)value), typeof(T));
                    }
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            } catch (IOException exception) {
                Debug.WriteLine(exception);
                Error?.Invoke(this, new PersistentSettingsEventArgs(registryKeyReadOnly, exception));
            } catch (SecurityException exception) {
                Debug.WriteLine(exception);
                Error?.Invoke(this, new PersistentSettingsEventArgs(registryKeyReadOnly, exception));
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
            return default(T);
        }

        /// <summary>
        /// Loads the value from the Windows registry. If the value is not found
        /// in the Windows registry, returns the default value. Supported
        /// datatypes are: sbyte (System.SByte), byte (System.Byte), short
        /// (System.Int16), ushort (System.UInt16), int (System.Int32), uint
        /// (System.UInt32), long (System.Int64), ulong (System.UInt64), char
        /// (System.Char), float (System.Single), double (System.Double), bool
        /// (System.Boolean), decimal (System.Decimal), string (System.String),
        /// DateTime (System.DateTime), TimeSpan (System.TimeSpan), Color
        /// (System.Drawing.Color). Datatypes not listed here may work, but their
        /// support is not implemented.
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">Default value.</param>
        /// </summary>
        public T Load<T>(string valueName, T defaultValue) {
            try {
                if (registryKeyReadOnly == null) {
                    registryKeyReadOnly = Registry.CurrentUser.OpenSubKey(registryPath);
                }
                if (registryKeyReadOnly != null) {
                    object defaultVal;
                    if (typeof(T) == typeof(bool)) {
                        defaultVal = (bool)Convert.ChangeType(defaultValue, typeof(T)) ? 1 : 0;
                    } else if (typeof(T) == typeof(byte)) {
                        defaultVal = (int)checked((byte)Convert.ChangeType(defaultValue, typeof(T)));
                    } else if (typeof(T) == typeof(char)) {
                        defaultVal = (int)(char)Convert.ChangeType(defaultValue, typeof(T));
                    } else if (typeof(T) == typeof(Color)) {
                        defaultVal = ((Color)Convert.ChangeType(defaultValue, typeof(T))).ToArgb();
                    } else if (typeof(T) == typeof(DateTime)) {
                        defaultVal = ((DateTime)Convert.ChangeType(defaultValue, typeof(T))).ToBinary();
                    } else if (typeof(T) == typeof(decimal)) {
                        defaultVal = ((decimal)Convert.ChangeType(defaultValue, typeof(T))).ToString(CultureInfo.InvariantCulture);
                    } else if (typeof(T) == typeof(double)) {
                        defaultVal = BitConverter.DoubleToInt64Bits((double)Convert.ChangeType(defaultValue, typeof(T)));
                    } else if (typeof(T) == typeof(float)) {
                        defaultVal = BitConverter.ToInt32(BitConverter.GetBytes((float)Convert.ChangeType(defaultValue, typeof(T))), 0);
                    } else if (typeof(T) == typeof(int)) {
                        defaultVal = (int)Convert.ChangeType(defaultValue, typeof(T));
                    } else if (typeof(T) == typeof(long)) {
                        defaultVal = (long)Convert.ChangeType(defaultValue, typeof(T));
                    } else if (typeof(T) == typeof(sbyte)) {
                        defaultVal = (int)(sbyte)Convert.ChangeType(defaultValue, typeof(T));
                    } else if (typeof(T) == typeof(short)) {
                        defaultVal = (int)(short)Convert.ChangeType(defaultValue, typeof(T));
                    } else if (typeof(T) == typeof(TimeSpan)) {
                        defaultVal = ((TimeSpan)Convert.ChangeType(defaultValue, typeof(T))).Ticks;
                    } else if (typeof(T) == typeof(uint)) {
                        defaultVal = (int)checked((uint)Convert.ChangeType(defaultValue, typeof(T)));
                    } else if (typeof(T) == typeof(ulong)) {
                        defaultVal = (long)checked((ulong)Convert.ChangeType(defaultValue, typeof(T)));
                    } else if (typeof(T) == typeof(ushort)) {
                        defaultVal = (int)checked((ushort)Convert.ChangeType(defaultValue, typeof(T)));
                    } else {
                        defaultVal = defaultValue;
                    }
                    object value = registryKeyReadOnly.GetValue(valueName, defaultVal);
                    Loaded?.Invoke(this, new PersistentSettingsEventArgs(registryKeyReadOnly));
                    if (typeof(T) == typeof(bool)) {
                        return (T)Convert.ChangeType((int)value > 0, typeof(T));
                    }
                    if (typeof(T) == typeof(byte)) {
                        return (T)Convert.ChangeType((byte)unchecked((int)value), typeof(T));
                    }
                    if (typeof(T) == typeof(Color)) {
                        return (T)Convert.ChangeType(Color.FromArgb((int)value), typeof(T));
                    }
                    if (typeof(T) == typeof(DateTime)) {
                        return (T)Convert.ChangeType(DateTime.FromBinary((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(decimal)) {
                        return (T)Convert.ChangeType(decimal.Parse((string)value, CultureInfo.InvariantCulture), typeof(T));
                    }
                    if (typeof(T) == typeof(double)) {
                        return (T)Convert.ChangeType(BitConverter.Int64BitsToDouble((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(float)) {
                        return (T)Convert.ChangeType(BitConverter.ToSingle(BitConverter.GetBytes((int)value), 0), typeof(T));
                    }
                    if (typeof(T) == typeof(TimeSpan)) {
                        return (T)Convert.ChangeType(TimeSpan.FromTicks((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(uint)) {
                        return (T)Convert.ChangeType((uint)unchecked((int)value), typeof(T));
                    }
                    if (typeof(T) == typeof(ulong)) {
                        return (T)Convert.ChangeType((ulong)unchecked((long)value), typeof(T));
                    }
                    if (typeof(T) == typeof(ushort)) {
                        return (T)Convert.ChangeType((ushort)unchecked((int)value), typeof(T));
                    }
                    return (T)Convert.ChangeType(value, typeof(T));
                }
            } catch (IOException exception) {
                Debug.WriteLine(exception);
                Error?.Invoke(this, new PersistentSettingsEventArgs(registryKeyReadOnly, exception));
            } catch (SecurityException exception) {
                Debug.WriteLine(exception);
                Error?.Invoke(this, new PersistentSettingsEventArgs(registryKeyReadOnly, exception));
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
            return defaultValue;
        }

        /// <summary>
        /// Saves the value into the Windows registry. Supported datatypes are:
        /// sbyte (System.SByte), byte (System.Byte), short (System.Int16),
        /// ushort (System.UInt16), int (System.Int32), uint (System.UInt32),
        /// long (System.Int64), ulong (System.UInt64), char (System.Char),
        /// float (System.Single), double (System.Double), bool (System.Boolean),
        /// decimal (System.Decimal), string (System.String), DateTime
        /// (System.DateTime), TimeSpan (System.TimeSpan), Color
        /// (System.Drawing.Color). Datatypes not listed here may work, but their
        /// support is not implemented.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="value">Value to save.</param>
        public void Save(string valueName, object value) {
            try {
                if (registryKeyWritable == null) {
                    registryKeyWritable = Registry.CurrentUser.CreateSubKey(registryPath);
                }
                Type type = value.GetType();
                if (type == typeof(bool)) {
                    registryKeyWritable.SetValue(valueName,
                        (bool)value ? 1 : 0, RegistryValueKind.DWord);
                } else if (type == typeof(byte)) {
                    registryKeyWritable.SetValue(valueName,
                        (int)checked((byte)value), RegistryValueKind.DWord);
                } else if (type == typeof(char)) {
                    registryKeyWritable.SetValue(valueName,
                        (int)(char)value, RegistryValueKind.DWord);
                } else if (type == typeof(Color)) {
                    registryKeyWritable.SetValue(valueName,
                        ((Color)value).ToArgb(), RegistryValueKind.DWord);
                } else if (type == typeof(DateTime)) {
                    registryKeyWritable.SetValue(valueName,
                        ((DateTime)value).ToBinary(), RegistryValueKind.QWord);
                } else if (type == typeof(decimal)) {
                    registryKeyWritable.SetValue(valueName,
                        ((decimal)value).ToString(CultureInfo.InvariantCulture), RegistryValueKind.String);
                } else if (type == typeof(double)) {
                    registryKeyWritable.SetValue(valueName,
                        BitConverter.DoubleToInt64Bits((double)value), RegistryValueKind.QWord);
                } else if (type == typeof(float)) {
                    registryKeyWritable.SetValue(valueName,
                        BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0), RegistryValueKind.DWord);
                } else if (type == typeof(int)) {
                    registryKeyWritable.SetValue(valueName,
                        (int)value, RegistryValueKind.DWord);
                } else if (type == typeof(long)) {
                    registryKeyWritable.SetValue(valueName,
                        (long)value, RegistryValueKind.QWord);
                } else if (type == typeof(sbyte)) {
                    registryKeyWritable.SetValue(valueName,
                        (int)(sbyte)value, RegistryValueKind.DWord);
                } else if (type == typeof(short)) {
                    registryKeyWritable.SetValue(valueName,
                        (int)(short)value, RegistryValueKind.DWord);
                } else if (type == typeof(TimeSpan)) {
                    registryKeyWritable.SetValue(valueName,
                        ((TimeSpan)value).Ticks, RegistryValueKind.QWord);
                } else if (type == typeof(uint)) {
                    registryKeyWritable.SetValue(valueName,
                        (int)checked((uint)value), RegistryValueKind.DWord);
                } else if (type == typeof(ulong)) {
                    registryKeyWritable.SetValue(valueName,
                        (long)checked((ulong)value), RegistryValueKind.QWord);
                } else if (type == typeof(ushort)) {
                    registryKeyWritable.SetValue(valueName,
                        (int)checked((ushort)value), RegistryValueKind.DWord);
                } else {
                    registryKeyWritable.SetValue(valueName,
                        value);
                }
                Saved?.Invoke(this, new PersistentSettingsEventArgs(registryKeyWritable));
            } catch (IOException exception) {
                Debug.WriteLine(exception);
                Error?.Invoke(this, new PersistentSettingsEventArgs(registryKeyWritable, exception));
            } catch (SecurityException exception) {
                Debug.WriteLine(exception);
                Error?.Invoke(this, new PersistentSettingsEventArgs(registryKeyWritable, exception));
            } catch (UnauthorizedAccessException exception) {
                Debug.WriteLine(exception);
                Error?.Invoke(this, new PersistentSettingsEventArgs(registryKeyWritable, exception));
            } catch (Exception exception) {
                Debug.WriteLine(exception);
            }
        }
    }

    /// <summary>
    /// Implements custom event args used by PersistentSettings class.
    /// </summary>
    public sealed class PersistentSettingsEventArgs : EventArgs {

        /// <summary>
        /// The Exception property.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// The RegistryKey property.
        /// </summary>
        public RegistryKey RegistryKey { get; private set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="PersistWindowStateEventArgs"/> class.
        /// </summary>
        public PersistentSettingsEventArgs(RegistryKey registryKey, Exception exception = null) {
            Exception = exception;
            RegistryKey = registryKey;
        }
    }
}
