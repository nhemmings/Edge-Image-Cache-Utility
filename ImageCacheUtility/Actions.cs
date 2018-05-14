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
        //private string[] fullPath;
        public string CachePath { get; set; }
        private bool returnFullPath = false;
        private DateTime oldDate;
        private List<string> accessibleFiles, oldFilesFullPath, oldFilesDate, fullPath, zeroSizeFiles, inaccessibleFiles;
        private long totalFileSize;
        private string fileSizeLabel;

        public void FindEmptyFiles()
        {
            getAccessibleFiles(CachePath, processFile);

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
            CachePath = path;

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
            getAccessibleFiles(CachePath, processFile);

            for (int i = 0; i < accessibleFiles.Count; i++)
            {
                var fi = new FileInfo(accessibleFiles[i].ToString());
                if (fi.LastWriteTime < oldDate)
                {
                    totalFileSize += fi.Length;
                    //Console.WriteLine(di.FullName +" "+ di.LastWriteTime);
                    oldFilesFullPath.Add(accessibleFiles.ElementAt(i));
                    oldFilesDate.Add(fi.LastWriteTime.ToString());
                }
            }
            convertBytesLabel();
            convertBytes();
            
        }

        public void SetDate(DateTime date)
        {
            oldDate = date;
            //Console.WriteLine("old date" + oldDate);
        }


        public List<string> ReturnOldFilesFullPath()
        {
            return oldFilesFullPath;
        }

        public List<string> ReturnOldFilesModifyDate()
        {
            return oldFilesDate;
        }

        public List<string> ReturnInaccessibleFiles()
        {
            return inaccessibleFiles;
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
            foreach (string file in Directory.GetFiles(folder))
            {
                try
                {
                    File.Open(file, FileMode.Open).Close();
                    fileAction(file);
                    //Console.WriteLine(file);
                    accessibleFiles.Add(file);
                }
                catch(Exception ex) { Trace.TraceError(ex.ToString()); inaccessibleFiles.Add(file);}
            }
            foreach (string subDir in Directory.GetDirectories(folder))
            {
               // try
               // {
                    getAccessibleFiles(subDir, fileAction);

               // }
              //  catch (Exception ex) { Trace.TraceError(ex.ToString()); }
            }

        }

        static void processFile(string path)
        {
 
        }

        public void ClearLists()
        {
            oldFilesFullPath.Clear();
            accessibleFiles.Clear();
            oldFilesDate.Clear();
            fullPath.Clear();
            zeroSizeFiles.Clear();
        }

        public void ClearInaccessibleFiles()
        {
            inaccessibleFiles.Clear();
        }




        
        public Actions() //Constructor for Lists
        {
            accessibleFiles = new List<string>();
            oldFilesDate = new List<string>();
            oldFilesFullPath = new List<string>();
            fullPath = new List<string>();
            zeroSizeFiles = new List<string>();
            inaccessibleFiles = new List<string>();
        }


        private void convertBytesLabel() //Sets the file size label 
        {
            if (totalFileSize < 1000)
            {
                fileSizeLabel = "bytes";
            }
            else if (totalFileSize > 1000 && totalFileSize < 1000000)
            {
                fileSizeLabel = "KB";
            }
            else if(totalFileSize > 1000000 && totalFileSize < 1000000000)
            {
                fileSizeLabel = "MB";
            }
            else if(totalFileSize > 1000000000)
            {
                fileSizeLabel = "GB";
            }
        }

        private void convertBytes() //converts the byte number to match the updated label
        {
            switch (fileSizeLabel)
            {
                case "bytes":
                    return;
                case "KB":
                    totalFileSize = (totalFileSize / 1000);
                    return;
                case "MB":
                    totalFileSize = (totalFileSize / 1000000);
                    return;
                case "GB":
                    totalFileSize = (totalFileSize / 1000000000);
                    return;
            }
        }

        public string ReturnTotalBytes()
        {
            return totalFileSize.ToString();
        }

        public string ReturnFileSizeLabel()
        {
            return fileSizeLabel;
        }
    }
}

