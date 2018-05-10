using System;
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
        private string cachePath;
        private bool returnFullPath = false;
        private DateTime oldDate;
        private List<string> accessibleFiles, oldFilesFullPath, oldFilesDate, fullPath, zeroSizeFiles;
        private bool pathAccessible;


        public void FindEmptyFiles()
        {
            getAccessibleFiles(cachePath, processFile);

            for (int i = 0; i < accessibleFiles.Count; i++)
            {
                var fi = new FileInfo(accessibleFiles[i].ToString());
                if (fi.Length == 0)
                {
                    //Console.WriteLine(di.FullName +" "+ di.LastWriteTime);
                    zeroSizeFiles.Add(fi.Name);
                    fullPath.Add(fi.FullName);
                    //Console.WriteLine(fi.FullName);
                    
                }
            }

        }

        public List<string> ReturnEmptyFiles()
        {
            if (returnFullPath) //toggle for full path or file name
            {
                return fullPath; //return full path
            }
            else
            {
                return zeroSizeFiles; // return file name
            }
        }


        public void FixEmptyFiles()
        {
            //They must provide a path and find the empty files before deleting them
            if (fullPath is null || fullPath[0] == "")
            {
                MessageBox.Show("Please Find Empty Files First", "Find Must Be Run");
            }
            else
            {
                //passes the full path of each item in the 0kb full path array to the delete command
                for (int i = 0; i < fullPath.Count; i++)
                {
                    File.Delete(fullPath[i]);
                }
                MessageBox.Show("Empty Files Deleted", "Delete Empty Files Complete");
            }
        }

        public void SetCachePath(string path)
        {
            cachePath = path;

        }

        /*public void Debug_FullPath() //Debug method 
        {
            for (int i = 0; i < fullPath.Length; i++)
            {
                Console.WriteLine(fullPath[i]);
            }
        }*/

        public void SetReturnFullPathToggle() //Method to set toggle of returning full file path or file name 0kb files
        {
            if (returnFullPath)
            {
                returnFullPath = false;
            }
            else
            {
                returnFullPath = true;
            }
        }

        public void FindOldFiles()
        {
            getAccessibleFiles(cachePath, processFile);

            for (int i = 0; i < accessibleFiles.Count; i++)
            {
                var fi = new FileInfo(accessibleFiles[i].ToString());
                if (fi.LastWriteTime < oldDate)
                {
                    //Console.WriteLine(di.FullName +" "+ di.LastWriteTime);
                    oldFilesFullPath.Add(accessibleFiles.ElementAt(i));
                    oldFilesDate.Add(fi.LastWriteTime.ToString());
                }
            }
            

        }

        public void SetDate(DateTime date)
        {
            oldDate = date;
            //Console.WriteLine("old date" + oldDate);
        }


        public List<string> ReturnOldFilesFullPath()
        {
            //return oldFilesFullPath;
            return oldFilesFullPath;
        }

        public List<string> ReturnOldFilesModifyDate()
        {
            return oldFilesDate;
        }

        public void DeleteOldFiles()
        {
            //they must find the old files before we delete them
            if (oldFilesFullPath is null || oldFilesFullPath[0] == "")
            {
                MessageBox.Show("Please Find Old Files First", "Find Must Be Run");
            }
            else
            {
                //passes the full path of each item in the old files full path  array to the delete command
                for (int i = 0; i < oldFilesFullPath.Count; i++)
                {
                    File.Delete(oldFilesFullPath[i]);
                }
                MessageBox.Show("Old Files Deleted", "Delete Old Files Complete");
            }
        }


        //TODO better comment, Black magic occurs
        private void getAccessibleFiles(string folder, Action<string> fileAction)
        {
            try {
                foreach (string file in Directory.GetFiles(folder))
                {
                    fileAction(file);
                    //Console.WriteLine(file);
                    accessibleFiles.Add(file);
                }
            
                foreach (string subDir in Directory.GetDirectories(folder))
                {
                    try
                    {
                        getAccessibleFiles(subDir, fileAction);

                    }
                    catch (Exception ex) { Trace.TraceError(ex.ToString()); }
                }
                pathAccessible = true;
            }
            catch (Exception ex) {
                Trace.TraceError(ex.ToString());
                MessageBox.Show("Could not access cache path " + cachePath, "Path Inaccessible");
                pathAccessible = false;
            }

        }

        static void processFile(string path) { }

        public void ClearLists()
        {
            oldFilesFullPath.Clear();
            accessibleFiles.Clear();
            oldFilesDate.Clear();
            fullPath.Clear();
            zeroSizeFiles.Clear();
        }


        //Constructor for Lists
        public Actions()
        {
            accessibleFiles = new List<string>();
            oldFilesDate = new List<string>();
            oldFilesFullPath = new List<string>();
            fullPath = new List<string>();
            zeroSizeFiles = new List<string>();
            pathAccessible = false;
        }

        public bool PathAccessible()
        {
            return pathAccessible;
        }

    }
}

