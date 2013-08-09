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
using System.IO;

namespace OrganicBit.Zip {

    /// <summary>Specifies how the the zip entry should be compressed.</summary>
    public enum CompressionMethod {
        /// <summary>No compression.</summary>
        Stored = 0,

        /// <summary>Default and only supported compression method.</summary>
        Deflated = 8
    }

    /// <summary>Specifies the amount of compression to apply to compressed zip entires.</summary>
    public enum CompressionLevel : int {
        /// <summary>Default compression level.  A good choice for speed and size.</summary>
        Default = -1,

        /// <summary>Do not perfrom compression.</summary>
        None = 0,

        /// <summary>Compress the entry as fast as possible size trading size for time.</summary>
        Fastest = 1,

        /// <summary>Compress the entry using a balance of size and time.</summary>
        Average = 5,

        /// <summary>Compress the entry to smallest possible size trading time for size.</summary>
        Smallest = 9
    }

    /// <summary>Represents a entry in a zip file.</summary>
    public class ZipEntry {

        string   _name = String.Empty;
        uint     _crc = 0;
        long     _compressedLength = -1;
        long     _uncompressedLength = -1;
        byte[]   _extraField = null;
        string   _comment = String.Empty;
        DateTime _modifiedTime = DateTime.Now;

        CompressionMethod _method = CompressionMethod.Deflated;
        int               _level  = (int) CompressionLevel.Default;

        /// <summary>Initializes a instance of the <see cref="ZipEntry"/> class with the given name.</summary>
        /// <param name="name">The name of entry that will be stored in the directory of the zip file.</param>
        public ZipEntry(string name) {
            Name = name;
        }

        /// <summary>Creates a new Zip file entry reading values from a zip file.</summary>
        internal ZipEntry(IntPtr handle) {
            ZipEntryInfo entryInfo;
            int result = 0;
            unsafe {
                result = ZipLib.unzGetCurrentFileInfo(handle, &entryInfo, null, 0, null, 0, null, 0);
            }
            if (result != 0) {
                throw new ZipException("Could not read entries from zip file " + Name);
            }

            ExtraField = new byte[entryInfo.ExtraFieldLength];
            sbyte[] entryNameBuffer = new sbyte[entryInfo.FileNameLength];
            sbyte[] commentBuffer   = new sbyte[entryInfo.CommentLength];

            unsafe {
                result = ZipLib.unzGetCurrentFileInfo(handle, &entryInfo,
                    entryNameBuffer, (uint) entryNameBuffer.Length,
                    ExtraField,      (uint) ExtraField.Length,
                    commentBuffer,   (uint) commentBuffer.Length);
            }
            if (result != 0) {
                throw new ZipException("Could not read entries from zip file " + Name);
            }

            _name = ZipLib.AnsiToString(entryNameBuffer);
            _comment = ZipLib.AnsiToString(commentBuffer);

            _crc = entryInfo.Crc;
            _compressedLength = entryInfo.CompressedSize;
            _uncompressedLength = entryInfo.UncompressedSize;
            _method = (CompressionMethod) entryInfo.CompressionMethod;
            _modifiedTime = new DateTime(
                (int) entryInfo.DateTime.Year,
                (int) entryInfo.DateTime.Month + 1,
                (int) entryInfo.DateTime.Day,
                (int) entryInfo.DateTime.Hours,
                (int) entryInfo.DateTime.Minutes,
                (int) entryInfo.DateTime.Seconds);
        }

        /// <summary>Gets and sets the local file comment for the entry.</summary>
        /// <remarks>
        ///   <para>Currently only Ascii 8 bit characters are supported in comments.</para>
        ///   <para>A comment cannot exceed 65535 bytes.</para>
        /// </remarks>
        public string Comment {
            get { return _comment; }
            set {
                // null comments are valid
                if (value != null) {
                    if (value.Length > 0xffff) {
                        throw new ArgumentOutOfRangeException("Comment cannot not exceed 65535 characters.");
                    }
                    if (!IsAscii(value)) {
                        throw new ArgumentException("Name can only contain Ascii 8 bit characters.");
                    }
                }

                // TODO: check for ASCII only characters
                _comment = value;
            }
        }

        /// <summary>Gets the compressed size of the entry data in bytes, or -1 if not known.</summary>
        public long CompressedLength {
            get { return _compressedLength; }
        }

        /// <summary>Gets the CRC-32 checksum of the uncompressed entry data.</summary>
        public uint Crc {
            get { return _crc; }
        }

        /// <summary>Gets and sets the optional extra field data for the entry.</summary>
        /// <remarks>ExtraField data cannot exceed 65535 bytes.</remarks>
        public byte[] ExtraField {
            get {
                return _extraField;
            }
            set {
                if (value.Length > 0xffff) {
                    throw new ArgumentOutOfRangeException("ExtraField cannot not exceed 65535 bytes.");
                }
                _extraField = value;
            }
        }

        /// <summary>Gets and sets the default compresion method for zip file entries.  See <see cref="CompressionMethod"/> for a list of possible values.</summary>
        public CompressionMethod Method {
            get { return _method; }
            set { _method = value; }
        }

        /// <summary>Gets and sets the default compresion level for zip file entries.  See <see cref="CompressionMethod"/> for a partial list of values.</summary>
        public int Level {
            get { return _level; }
            set {
                if (value < -1 || value > 9) {
                    throw new ArgumentOutOfRangeException("Level", value, "Level value must be between -1 and 9.");
                }
                _level = value;
            }
        }

        /// <summary>Gets the size of the uncompressed entry data in in bytes.</summary>
        public long Length {
            get { return _uncompressedLength; }
        }

        /// <summary>Gets and sets the modification time of the entry.</summary>
        public DateTime ModifiedTime {
            get { return _modifiedTime; }
            set { _modifiedTime = value; }
        }

        /// <summary>Gets and sets the name of the entry.</summary>
        /// <remarks>
        ///   <para>Currently only Ascii 8 bit characters are supported in comments.</para>
        ///   <para>A comment cannot exceed 65535 bytes.</para>
        /// </remarks>
        public string Name {
            get { return _name; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("Name cannot be null.");
                }
                if (value.Length > 0xffff) {
                    throw new ArgumentOutOfRangeException("Name cannot not exceed 65535 characters.");
                }
                if (!IsAscii(value)) {
                    throw new ArgumentException("Name can only contain Ascii 8 bit characters.");
                }
                // TODO: check for ASCII only characters
                _name = value;
            }
        }

        /// <summary>Flag that indicates if this entry is a directory or a file.</summary>
        public bool IsDirectory {
            get {
                return (Length == 0 && CompressedLength == 0);
            }
        }

        /// <summary>Gets the compression ratio as a percentage.</summary>
        /// <remarks>Returns -1.0 if unknown.</remarks>
        public float Ratio {
            get {
                float ratio = -1.0f;
                if (Length > 0) {
                    ratio = Convert.ToSingle(Length - CompressedLength) / Length;
                }
                return ratio;
            }
        }

        /// <summary>Returns a string representation of the Zip entry.</summary>
        public override string ToString() {
            return String.Format("{0} {1}", Name, base.ToString());
        }

        /// <summary>Check if <paramref name="str"/> only contains Ascii 8 bit characters.</summary>
        static bool IsAscii(string str) {
            foreach (char ch in str) {
                if (ch > 0xff) {
                    return false;
                }
            }
            return true;
        }
    }
}
