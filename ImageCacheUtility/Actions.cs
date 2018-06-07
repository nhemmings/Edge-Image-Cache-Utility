﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;


namespace ImageCacheUtility
{
    class Actions
    {
        public string CachePath { get; set; }
        private DateTime oldDate;
        private List<string> inaccessibleFiles, nestedCaches, messageBoxContent; 
        private List<FileInfo> accessibleFilesInfo;
        private int guidLength = 36;
        private string subDirTemp;


        public void FindFiles()
        {
            _GetAccessibleFiles(CachePath);
            nestedCaches = nestedCaches.Distinct().ToList();

        }

        public void FixEmptyFiles()
        {
            //They must provide a path and find the empty files before deleting them
            if (accessibleFilesInfo is null || accessibleFilesInfo[0].Name == "")
            {
                MessageBox.Show("Please Find Empty Files First", "Find Must Be Run");
            }
            else
            {
                //passes the full path of each item in the 0kb full path array to the delete command
                for (int i = 0; i < accessibleFilesInfo.Count; i++)
                {
                    if (accessibleFilesInfo[i].Length == 0)
                    {
                        File.Delete(accessibleFilesInfo[i].ToString());
                    }
                }
                MessageBox.Show("Empty Files Deleted", "Delete Empty Files Complete");
            }
        }

        public void SetCachePath(string path)
        {
            CachePath = path;

        }

        public void SetDate(DateTime date)
        {
            oldDate = date;
        }

        public List<string> ReturnInaccessibleFiles()
        {
            return inaccessibleFiles;
        }

        public List<FileInfo> ReturnFileInfo()
        {
            return accessibleFilesInfo;
        }

        public void DeleteOldFiles()
        {
            //they must find the old files before we delete them
            if (accessibleFilesInfo is null || accessibleFilesInfo[0].Name == "")
            {
                MessageBox.Show("Please Find Old Files First", "Find Must Be Run");
            }
            else
            {
                //passes the full path of each item in the old files full path  array to the delete command
                for (int i = 0; i < accessibleFilesInfo.Count; i++)
                {
                    if (accessibleFilesInfo[i].LastWriteTime < oldDate) { 
                        File.Delete(accessibleFilesInfo[i].ToString());
                    }
                }
                MessageBox.Show("Old Files Deleted", "Delete Old Files Complete");
            }
        }

        //recursively goes through sub directories and files within the passed in file path. It attempts accessing them to build accessible files and inaccessible files/folders
        private void _GetAccessibleFiles(string folder)
        {
            foreach (string file in Directory.GetFiles(folder))
            {
                try
                {
                    File.Open(file, FileMode.Open).Close();
                    accessibleFilesInfo.Add(new FileInfo(file));
                }
                catch(Exception ex) { Trace.TraceError(ex.ToString()); inaccessibleFiles.Add(file);}
            }
            foreach (string subDir in Directory.GetDirectories(folder))
            {
                try
                {
                    _GetAccessibleFiles(subDir);
                    subDirTemp = subDir;
                    //checks to see if the path after the cache path is longer than 2 GUIDS, magic numbers are to account for / in path
                    if (subDir.Remove(0, CachePath.Length + 1).Length > (guidLength * 2))
                    {
                        if (subDir.Remove(0, CachePath.Length + guidLength + 2).Length > guidLength)
                        {
                            subDirTemp = subDirTemp.Remove(0, CachePath.Length +1);
                            subDirTemp = subDirTemp.Remove((guidLength*2)+1);
                            nestedCaches.Add(subDirTemp);
                        }
                    }
                }
                catch(Exception ex) { Trace.TraceError(ex.ToString()); inaccessibleFiles.Add(subDir); }
            }

        }

        public void ClearLists() //cleasrs all lists besides inaccessible files as they are cleared at different times
        {
            accessibleFilesInfo.Clear();
            nestedCaches.Clear();
            messageBoxContent.Clear();
        }

        public void ClearInaccessibleFiles() //clears inaccessible files
        {
            inaccessibleFiles.Clear();
        }

        /*for each nested cache it recursively goes through the nested cache and sees if the folder exists in the parent folder if not it creates it.
        then it copies the files that do not exist to the parent folder and then deletes the nested folder*/
        public void CleanUpNestedCaches() 
        {
            for (int i = 0; i < nestedCaches.Count; i++)
            {
                string sourcePath = Path.Combine(CachePath, nestedCaches[i]);
                try
                { 
                    foreach (string directory in Directory.GetDirectories(sourcePath))
                    {
                        string destinationPath = Path.Combine(CachePath, directory.Substring(CachePath.Length + guidLength + 2));

                        if (Directory.Exists(destinationPath))
                        {
                            foreach (string file in Directory.GetFiles(directory))
                            {
                                string fileName = Path.GetFileName(file);
                                string destFile = Path.Combine(destinationPath, fileName);
                                if (!File.Exists(destFile))
                                {
                                    File.Copy(file, destFile, false);
                                }
                            }
                        }
                        else
                        {
                            Directory.CreateDirectory(destinationPath);
                            foreach (string file in Directory.GetFiles(directory))
                            {
                                string fileName = Path.GetFileName(file);
                                string destFile = Path.Combine(destinationPath, fileName);
                                File.Copy(file, destFile, false);
                            }
                        }
                    }
                    Directory.Delete(sourcePath, true);
                }
                catch(Exception ex) { Trace.TraceError(ex.ToString());}
            }
        }


        public Actions() //Constructor for Lists
        {
            inaccessibleFiles = new List<string>();
            accessibleFilesInfo = new List<FileInfo>();
            nestedCaches = new List<string>();
            messageBoxContent = new List<string>();
        }

        private void _NestedCacheOutput() //creates the string for the message box and then displays a message box to the end user of what nested caches were detected
        {
            if (nestedCaches.Count > 0)
            {
                for (int i = 0; i < nestedCaches.Count; i++)
                {
                    messageBoxContent.Add("Parent folder: " + nestedCaches[i].Substring(0, guidLength) + "\nNested cache: " + nestedCaches[i].Substring(guidLength + 1) + "\n");
                }
                StringBuilder nestedCachesString = new StringBuilder();
                for (int i = 0; i < messageBoxContent.Count; i++)
                {
                    nestedCachesString.AppendLine(messageBoxContent[i]);
                }
                MessageBox.Show(nestedCachesString.ToString() + "Check all machines cache paths to make sure they are correct.", "Nested Cache Detected",MessageBoxButton.OK);
            }
        }

        public List<string> ReturnNestedCaches()
        {
            return nestedCaches;
        }

        public void ReturnNestedCacheInfo()
        {
            _NestedCacheOutput();
        }
    }
}

