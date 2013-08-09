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
using OrganicBit.Zip;

namespace OrganicBit.Zip.Examples {

    public class MiniZip {

        public static void ShowHelp() {
            Console.WriteLine(
@"MiniZip - A simple zip file manipulator
http://www.organicbit.com/zip/

Usage: minizip [-v|-e|-a] <filename.zip> [files to add]

Examples:
  minizip                           # displays this message
  minizip -v <filename.zip>         # view contents
  minizip -e <filename.zip>         # extract files
  minizip -a <filename.zip> [*.txt] # add files
");
        }

        public static void View(string zipFileName) {
            ZipReader reader = new ZipReader(zipFileName);

            Console.WriteLine("Archive: {0} ({1} files)", zipFileName, reader.Entries.Count);
            Console.WriteLine(reader.Comment);

            string format = "{0,8} {1,8} {2,5} {3,10} {4,5} {5}";
            Console.WriteLine(format, " Length ", "  Size  ", "Ratio", "   Date   ", "Time ", "Name");
            Console.WriteLine(format, "--------", "--------", "-----", "----------", "-----", "----");

            foreach (ZipEntry entry in reader.Entries) {
                if (!entry.IsDirectory) {
                    Console.WriteLine(format,
                        entry.Length,
                        entry.CompressedLength,
                        entry.Ratio.ToString("P0"),
                        entry.ModifiedTime.ToString("yyyy-MM-dd"),
                        entry.ModifiedTime.ToString("hh:mm"),
                        entry.Name);
                }
            }
            reader.Close();
        }

        public static void Extract(string zipFileName) {
            ZipReader reader = new ZipReader(zipFileName);
            Console.WriteLine("Archive: {0}", zipFileName);
            Console.WriteLine(reader.Comment);

            // buffer to hold temp bytes
            byte[] buffer = new byte[4096];
            int byteCount;

            // Get the zipped entries
            while (reader.MoveNext()) {
                ZipEntry entry = reader.Current;

                if (entry.IsDirectory) {
                    Directory.CreateDirectory(entry.Name);
                } else {
                    Console.Write("  {0}", entry.Name);

                    // create output stream
                    FileStream writer = File.Open(entry.Name, FileMode.Create);

                    // write uncompressed data
                    while ((byteCount = reader.Read(buffer, 0, buffer.Length)) > 0) {
                        Console.Write(".");
                        writer.Write(buffer, 0, byteCount);
                    }
                    writer.Close();
                    Console.WriteLine();
                }
            }
            reader.Close();
        }

        public static void Add(string zipFileName, string[] entryPatterns) {
            string currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine("Creating {0}", zipFileName);

            ZipWriter writer = new ZipWriter(zipFileName);

            // buffer to hold temp bytes
            byte[] buffer = new byte[4096];
            int byteCount;

            // add files to archive
            foreach (string pattern in entryPatterns) {
                foreach (string path in Directory.GetFiles(currentDirectory, pattern)) {
                    string fileName = Path.GetFileName(path);
                    Console.Write("Adding {0}", fileName);

                    ZipEntry entry = new ZipEntry(fileName);
                    entry.ModifiedTime = File.GetLastWriteTime(fileName);
                    entry.Comment = "local file comment";

                    writer.AddEntry(entry);

                    FileStream reader = File.OpenRead(entry.Name);
                    while ((byteCount = reader.Read(buffer, 0, buffer.Length)) > 0) {
                        Console.Write(".");
                        writer.Write(buffer, 0, byteCount);
                    }
                    reader.Close();
                    Console.WriteLine();
                }
            }

            writer.Close();
        }

        enum Action {
            Nothing,
            ShowHelp,
            View,
            Extract,
            Add
        }

        static Action ParseCommandLine(string[] args) {
            Action action = Action.ShowHelp;
            if (args.Length >= 2) {
                if (args[0][0] == '-') {
                    switch (args[0][1]) {
                    case 'v':
                        action = Action.View;
                        break;

                    case 'e':
                        action = Action.Extract;
                        break;

                    case 'a':
                        action = Action.Add;
                        break;
                    }
                }
            }
            return action;
        }

        public static int Main(string[] args) {
            try {
                Action action = ParseCommandLine(args);
                switch (action) {
                case Action.ShowHelp:
                    ShowHelp();
                    break;

                case Action.View:
                    View(args[1]);
                    break;

                case Action.Extract:
                    Extract(args[1]);
                    break;

                case Action.Add:
                    string[] patterns = new string[args.Length - 2];
                    Array.Copy(args, 2, patterns, 0, args.Length - 2);
                    Add(args[1], patterns);
                    break;
                }
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            return 0;
        }
    }
}
