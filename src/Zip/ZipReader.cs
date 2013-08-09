// Copyright (C) 2001 Gerry Shaw
//
// This software is provided 'as-is', without any express or implied
// warranty.  In no event will the authors be held liable for any damages
// arising from the use of this software.
//
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it
// freely, subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not
//    claim that you wrote the original software. If you use this software
//    in a product, an acknowledgment in the product documentation would be
//    appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be
//    misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.
//
// Gerry Shaw (gerry_shaw@yahoo.com)

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace OrganicBit.Zip {

    /// <summary>Provides support for reading files in the ZIP file format. Includes support for both compressed and uncompressed entries.</summary>
    /// <example>This example shows how to view the entries in a ZIP file.
    /// <code>
    /// public static void View(string zipFileName) {
    ///     ZipReader reader = new ZipReader(zipFileName);
    /// 
    ///     Console.WriteLine("Archive: {0} ({1} files)", zipFileName, reader.Entries.Count);
    ///     Console.WriteLine(reader.Comment);
    /// 
    ///     string format = "{0,8} {1,8} {2,5} {3,10} {4,5} {5}";
    ///     Console.WriteLine(format, " Length ", "  Size  ", "Ratio", "   Date   ", "Time ", "Name");
    ///     Console.WriteLine(format, "--------", "--------", "-----", "----------", "-----", "----");
    /// 
    ///     foreach (ZipEntry entry in reader.Entries) {
    ///         if (!entry.IsDirectory) {
    ///             Console.WriteLine(format,
    ///                 entry.Length,
    ///                 entry.CompressedLength,
    ///                 entry.Ratio.ToString("P0"),
    ///                 entry.ModifiedTime.ToString("yyyy-MM-dd"),
    ///                 entry.ModifiedTime.ToString("hh:mm"),
    ///                 entry.Name);
    ///         }
    ///     }
    ///     reader.Close();
    /// }
    /// </code>
    /// </example>
    /// <example>This example shows how to extract files from a ZIP file.
    /// <code>
    /// public static void Extract(string zipFileName) {
    ///     ZipReader reader = new ZipReader(zipFileName);
    ///     Console.WriteLine("Archive: {0}", zipFileName);
    ///     Console.WriteLine(reader.Comment);
    /// 
    ///     // buffer to hold temp bytes
    ///     byte[] buffer = new byte[4096];
    ///     int byteCount;
    /// 
    ///     // Get the zipped entries
    ///     while (reader.MoveNext()) {
    ///         ZipEntry entry = reader.Current;
    /// 
    ///         if (entry.IsDirectory) {
    ///             Directory.CreateDirectory(entry.Name);
    ///         } else {
    ///             Console.Write("  {0}", entry.Name);
    /// 
    ///             // create output stream
    ///             FileStream writer = File.Open(entry.Name, FileMode.Create);
    /// 
    ///             // write uncompressed data
    ///             while ((byteCount = reader.Read(buffer, 0, buffer.Length)) > 0) {
    ///                 Console.Write(".");
    ///                 writer.Write(buffer, 0, byteCount);
    ///             }
    ///             writer.Close();
    ///             Console.WriteLine();
    ///         }
    ///     }
    ///     reader.Close();
    /// }
    /// </code>
    /// </example>
    public class ZipReader : IEnumerator, IDisposable {

        /// <summary>ZipFile handle to read data from.</summary>
        IntPtr _handle = IntPtr.Zero;

        /// <summary>Name of zip file.</summary>
        string _fileName = null;

        /// <summary>Contents of zip file directory.</summary>
        ZipEntryCollection _entries = null;

        /// <summary>Global zip file comment.</summary>
        string _comment = null;

        /// <summary>True if an entry is open for reading.</summary>
        bool _entryOpen = false;

        /// <summary>Current zip entry open for reading.</summary>
        ZipEntry _current = null;

        /// <summary>Initializes a instance of the <see cref="ZipReader"/> class for reading the zip file with the given name.</summary>
        /// <param name="fileName">The name of zip file that will be read.</param>
        public ZipReader(string fileName) {
            _fileName = fileName;
            _handle = ZipLib.unzOpen(fileName);
            if (_handle == IntPtr.Zero) {
                string msg = String.Format("Could not open zip file '{0}'.", fileName);
                throw new ZipException(msg);
            }
        }

        /// <summary>Cleans up the resources used by this zip file.</summary>
        ~ZipReader() {
            CloseFile(); 
        }

        /// <remarks>Dispose is synonym for Close.</remarks>
        void IDisposable.Dispose() {
            Close();
        }

        /// <summary>Closes the zip file and releases any resources.</summary>
        public void Close() {
            // Free unmanaged resources.
            CloseFile();

            // If base type implements IDisposable we would call it here.

            // Request the system not call the finalizer method for this object.
            GC.SuppressFinalize(this);
        }

        /// <summary>Gets the name of the zip file that was passed to the constructor.</summary>
        public string Name {
            get { return _fileName; }
        }

        /// <summary>Gets the global comment for the zip file.</summary>
        public string Comment {
            get {
                if (_comment == null) {
                    ZipFileInfo info;
                    int result = 0;
                    unsafe {
                        result = ZipLib.unzGetGlobalInfo(_handle, &info);
                    }
                    if (result < 0) {
                        string msg = String.Format("Could not read comment from zip file '{0}'.", Name);
                        throw new ZipException(msg);
                    }

                    sbyte[] buffer = new sbyte[info.CommentLength];
                    result = ZipLib.unzGetGlobalComment(_handle, buffer, (uint) buffer.Length);
                    if (result < 0) {
                        string msg = String.Format("Could not read comment from zip file '{0}'.", Name);
                        throw new ZipException(msg);
                    }
                    _comment = ZipLib.AnsiToString(buffer);
                }
                return _comment;
            }
        }

        /// <summary>Gets a <see cref="ZipEntryCollection"/> object that contains all the entries in the zip file directory.</summary>
        public ZipEntryCollection Entries {
            get {
                if (_entries == null) {
                    _entries = new ZipEntryCollection();

                    int result = ZipLib.unzGoToFirstFile(_handle);
                    while (result == 0) {
                        ZipEntry entry = new ZipEntry(_handle);
                        _entries.Add(entry);
                        result = ZipLib.unzGoToNextFile(_handle);
                    }
                }
                return _entries;
            }
        }

        object IEnumerator.Current {
            get {
                return _current;
            }
        }

        /// <summary>Gets the current entry in the zip file..</summary>
        public ZipEntry Current {
            get {
                return _current;
            }
        }

        /// <summary>Advances the enumerator to the next element of the collection.</summary>
        /// <summary>Sets <see cref="Current"/> to the next zip entry.</summary>
        /// <returns><c>true</c> if the next entry is not <c>null</c>; otherwise <c>false</c>.</returns>
        public bool MoveNext() {
            // close any open entry
            CloseEntry();

            int result;
            if (_current == null) {
                result = ZipLib.unzGoToFirstFile(_handle);
            } else {
                result = ZipLib.unzGoToNextFile(_handle);
            }
            if (result < 0) {
                // last entry found - not an exceptional case
                _current = null;
            } else {
                // entry found
                OpenEntry();
            }

            return (_current != null);
        }

        /// <summary>Move to just before the first entry in the zip directory.</summary>
        public void Reset() {
            CloseEntry();
            _current = null;
        }

        /// <summary>Seek to the specified entry.</summary>
        /// <param name="entryName">The name of the entry to seek to.</param>
        public void Seek(string entryName) {
            CloseEntry();
            int result = ZipLib.unzLocateFile(_handle, entryName, 0);
            if (result < 0) {
                string msg = String.Format("Could not locate entry named '{0}'.", entryName);
                throw new ZipException(msg);
            }
            OpenEntry();
        }

        private void OpenEntry() {
            _current = new ZipEntry(_handle);
            int result = ZipLib.unzOpenCurrentFile(_handle);
            if (result < 0) {
                _current = null;
                throw new ZipException("Could not open entry for reading.");
            }
            _entryOpen = true;
        }

        /// <summary>Uncompress a block of bytes from the current zip entry and writes the data in a given buffer.</summary>
        /// <param name="buffer">The array to write data into.</param>
        /// <param name="index">The byte offset in <paramref name="buffer"/> at which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        public int Read(byte[] buffer, int index, int count) {
            if (index != 0) {
                throw new ArgumentException("index", "Only index values of zero currently supported.");
            }
            int bytesRead = ZipLib.unzReadCurrentFile(_handle, buffer, (uint) count);
            if (bytesRead < 0) {
                throw new ZipException("Error reading zip entry.");
            }
            return bytesRead;
        }

        private void CloseEntry() {
            if (_entryOpen) {
                int result = ZipLib.unzCloseCurrentFile(_handle);
                if (result < 0) {
                    switch ((ErrorCode) result) {
                    case ErrorCode.CrcError:
                        throw new ZipException("All the file was read but the CRC did not match.");

                    default:
                        throw new ZipException("Could not close zip entry.");
                    }
                }
                _entryOpen = false;
            }
        }

        private void CloseFile() {
            if (_handle != IntPtr.Zero) {
                CloseEntry();
                int result = ZipLib.unzClose(_handle);
                if (result < 0) {
                    throw new ZipException("Could not close zip file.");
                }
                _handle = IntPtr.Zero;
            }
        }
    }
}
