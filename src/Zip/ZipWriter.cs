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
using System.Runtime.Serialization;

namespace OrganicBit.Zip {

    /// <summary>Provides support for writing files in the ZIP file format. Includes support for both compressed and uncompressed entries.</summary>
    /// <example>This example shows how to create a ZIP file.
    /// <code>
    /// public static void Add(string zipFileName, string[] entryPatterns) {
    ///     string currentDirectory = Directory.GetCurrentDirectory();
    ///     Console.WriteLine("Creating {0}", zipFileName);
    /// 
    ///     ZipWriter writer = new ZipWriter(zipFileName);
    /// 
    ///     // buffer to hold temp bytes
    ///     byte[] buffer = new byte[4096];
    ///     int byteCount;
    /// 
    ///     // add files to archive
    ///     foreach (string pattern in entryPatterns) {
    ///         foreach (string path in Directory.GetFiles(currentDirectory, pattern)) {
    ///             string fileName = Path.GetFileName(path);
    ///             Console.Write("Adding {0}", fileName);
    /// 
    ///             ZipEntry entry = new ZipEntry(fileName);
    ///             entry.ModifiedTime = File.GetLastWriteTime(fileName);
    ///             entry.Comment = "local file comment";
    /// 
    ///             writer.AddEntry(entry);
    /// 
    ///             FileStream reader = File.OpenRead(entry.Name);
    ///             while ((byteCount = reader.Read(buffer, 0, buffer.Length)) > 0) {
    ///                 Console.Write(".");
    ///                 writer.Write(buffer, 0, byteCount);
    ///             }
    ///             reader.Close();
    ///             Console.WriteLine();
    ///         }
    ///     }
    /// 
    ///     writer.Close();
    /// }
    /// </code>
    /// </example>
    public class ZipWriter : IDisposable {

        /// <summary>Name of the zip file.</summary>
        string _fileName;

        /// <summary>Zip file global comment.</summary>
        string _comment = "";

        /// <summary>True if currently writing a new zip file entry.</summary>
        bool _entryOpen = false;

        /// <summary>Zip file handle.</summary>
        IntPtr _handle = IntPtr.Zero;

        /// <summary>Initializes a new instance fo the <see cref="ZipWriter"/> class with a specified file name.  Any Existing file will be overwritten.</summary>
        /// <param name="fileName">The name of the zip file to create.</param>
        public ZipWriter(string fileName) {
            _fileName = fileName;

            _handle = ZipLib.zipOpen(fileName, 0);
            if (_handle == IntPtr.Zero) {
                string msg = String.Format("Could not open zip file '{0}' for writing.", fileName);
                throw new ZipException(msg);
            }
        }

        /// <summary>Cleans up the resources used by this zip file.</summary>
        ~ZipWriter() {
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

        /// <summary>Gets the name of the zip file.</summary>
        public string Name {
            get {
                return _fileName;
            }
        }

        /// <summary>Gets and sets the zip file comment.</summary>
        public string Comment {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>Creates a new zip entry in the directory and positions the stream to the start of the entry data.</summary>
        /// <param name="entry">The zip entry to be written.</param>
        /// <remarks>Closes the current entry if still active.</remarks>
        public void AddEntry(ZipEntry entry) {
            ZipFileEntryInfo info;
            info.DateTime = entry.ModifiedTime;

            int result;
            unsafe {
                byte[] extra = null;
                uint extraLength = 0;
                if (entry.ExtraField != null) {
                    extra = entry.ExtraField;
                    extraLength = (uint) entry.ExtraField.Length;
                }

                result = ZipLib.zipOpenNewFileInZip(
                    _handle,
                    entry.Name,
                    &info,
                    extra, 
                    extraLength,
                    null, 0,
                    entry.Comment,
                    (int) entry.Method,
                    entry.Level);
            }
            _entryOpen = true;
        }

        /// <summary>Compress a block of bytes from the given buffer and writes them into the current zip entry.</summary>
        /// <param name="buffer">The array to read data from.</param>
        /// <param name="index">The byte offset in <paramref name="buffer"/> at which to begin reading.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        public void Write(byte[] buffer, int index, int count) {
            int result = ZipLib.zipWriteInFileInZip(_handle, buffer, (uint) count);
        }

        private void CloseEntry() {
            if (_entryOpen) {
                int result = ZipLib.zipCloseFileInZip(_handle);
                _entryOpen = false;
            }
        }

        void CloseFile() {
            if (_handle != IntPtr.Zero) {
                CloseEntry();
                int result = ZipLib.zipClose(_handle, _comment);
                if (result < 0) {
                    throw new ZipException("Could not close zip file.");
                }
                _handle = IntPtr.Zero;
            }
        }
    }
}
