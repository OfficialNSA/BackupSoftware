// Backup Software, creates incremental Backups on a drive of your choice
// Copyright (C) 2022  Josua Gunzenhauser
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.


// Contact the creator via chromsuport@gmail.com

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BackupSoftware
{
    //Edit the folders in the source code (It is called bad backup, duh)
    //Strategie: 5 inkrementelle anhand von der ältesten vollständigen, dann die vollständige mit dem ersten inkrementellen vereinigen

    class Program
    {

        // source
        static string root1 = @"A:\";
        // The full backup where the increment should be created from
        static string root2 = @"E:\PC\full_backup\";
        // Destination of the increment
        static string destination = @"E:\PC\" + DateTime.Today.Year.ToString("D4") + "-" + DateTime.Today.Month.ToString("D2") + "-" + DateTime.Today.Day.ToString("D2") + @"\";

        // What shouldn't be included (I don't know if it works, probably not)
        static string[] ignorables = { ".git", "node_modules", "build" };

        static void Main(string[] args)
        {

            // License and warranty hint in the running program
            Console.WriteLine("Financial Analysis  Copyright (C) 2022  Josua Gunzenhauser\nThis program comes with ABSOLUTELY NO WARRANTY; for details see the COPYING file or <https://www.gnu.org/licenses/>\nThis is free software, and you are welcome to redistribute it under certain conditions; see the COPYING file or <https://www.gnu.org/licenses/> for details.");
            Console.WriteLine("\n\nTHIS SOFTWARE IS ABLE TO BREAK AND WILL BREAK YOUR COMPUTER IF YOU DON'T USE IT WITH CARE.");


            // Time the process
            Stopwatch sw = new Stopwatch();

            sw.Start();

            // List of all files that should be copied to the incremental Backup. "" means it is the relative root folder
            try
            {
                var changedFiles = getDifferenceBetweenFolders("");

                foreach (var item in changedFiles)
                {

                    Console.WriteLine("Copying " + item);

                    new FileInfo(destination + item).Directory.Create();
                    try
                    {
                        File.Copy(root1 + item, destination + item);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(item + e.ToString());
                        continue;
                    }

                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.ToString());
            }


            sw.Stop();

            Console.WriteLine(sw.ElapsedMilliseconds + "ms");
            Console.Read();

        }

        static List<string> getDifferenceBetweenFolders(string relativePath)
        {

            // Empty return value to be filled throughout the function
            List<string> differentFiles = new List<string>();

            // Don't even try if the current folder is a ignorable (probably doesn't work, idc)
            foreach (var item in ignorables)
            {
                if (relativePath.Contains(item))
                {
                    return differentFiles;
                }
            }

            // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/how-to-compare-the-contents-of-two-folders-linq
            // Get top level folders
            IEnumerable<DirectoryInfo> childrenDirectories1 = new DirectoryInfo(root1 + relativePath).GetDirectories("*", SearchOption.TopDirectoryOnly);
            IEnumerable<DirectoryInfo> childrenDirectories2 = new DirectoryInfo(root2 + relativePath).GetDirectories("*", SearchOption.TopDirectoryOnly);
            DirectoryCompare myDirectoryCompare = new DirectoryCompare();

            // Get all new / renamed folders
            var queryList1 = (from file in childrenDirectories1 select file).Except(childrenDirectories2, myDirectoryCompare);

            // All files in the new/renamed directories should be copied, no need for recursion
            foreach (var v in queryList1)
            {
                try
                {
                    foreach (var item in v.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        bool ignored = false;
                        foreach (string ignore in ignorables)
                        {
                            if (item.FullName.Contains(ignore))
                            {
                                ignored = true;
                                break;
                            }
                        }
                        if (!ignored)
                        {
                            differentFiles.Add(item.FullName.Replace(root1, ""));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

            // Look recursively in unchanged folders for changed files / changed subfolders
            foreach (var v in childrenDirectories1.Except(queryList1, myDirectoryCompare))
            {
                differentFiles.AddRange(getDifferenceBetweenFolders(v.FullName.Replace(root1, "") + @"\"));
            }


            // Search for new / edited files in the current folder
            IEnumerable<FileInfo> fileList1 = new DirectoryInfo(root1 + relativePath).GetFiles("*", SearchOption.TopDirectoryOnly);
            IEnumerable<FileInfo> fileList2 = new DirectoryInfo(root2 + relativePath).GetFiles("*", SearchOption.TopDirectoryOnly);
            FileCompare myFileCompare = new FileCompare();

            if (!fileList1.SequenceEqual(fileList2, myFileCompare))
            {
                // Find the set difference between the two folders.
                var queryList1Only = (from file in fileList1 select file).Except(fileList2, myFileCompare);

                foreach (var v in queryList1Only)
                {
                    differentFiles.Add(v.FullName.Replace(root1, ""));
                }

            }

            Console.WriteLine("Done scanning " + relativePath);

            return differentFiles;
        }

    }

    class DirectoryCompare : IEqualityComparer<DirectoryInfo>
    {
        public bool Equals(DirectoryInfo x, DirectoryInfo y)
        {
            return x.Name == y.Name;
        }

        public int GetHashCode(DirectoryInfo obj)
        {
            string s = $"{obj.Name}";
            return s.GetHashCode();
        }
    }

    // This implementation defines a very simple comparison  
    // between two FileInfo objects. It only compares the name  
    // of the files being compared and their length in bytes.  
    class FileCompare : IEqualityComparer<FileInfo>
    {
        public FileCompare() { }

        public bool Equals(FileInfo f1, FileInfo f2)
        {
            return f1.Name == f2.Name && f1.Length == f2.Length && f1.LastWriteTime == f2.LastWriteTime;
        }

        // Return a hash that reflects the comparison criteria. According to the
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must  
        // also be equal. Because equality as defined here is a simple value equality, not  
        // reference identity, it is possible that two or more objects will produce the same  
        // hash code.  
        public int GetHashCode(FileInfo fi)
        {
            string s = $"{fi.Name}{fi.Length}";
            return s.GetHashCode();
        }
    }
}
