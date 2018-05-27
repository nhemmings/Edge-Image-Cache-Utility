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
        public string CachePath { get; set; }
        private DateTime oldDate;
        private List<string> inaccessibleFiles; 

        private List<FileInfo> accessibleFilesInfo;

        public void FindFiles()
        {
            _GetAccessibleFiles(CachePath);
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
                }
                catch(Exception ex) { Trace.TraceError(ex.ToString()); inaccessibleFiles.Add(subDir); }
            }

        }

        public void ClearLists() //cleasrs all lists besides inaccessible files as they are cleared at different times
        {
            accessibleFilesInfo.Clear();
        }

        public void ClearInaccessibleFiles() //clears inaccessible files
        {
            inaccessibleFiles.Clear();
        }


        public Actions() //Constructor for Lists
        {
            inaccessibleFiles = new List<string>();
            accessibleFilesInfo = new List<FileInfo>();
        }
    }
}

