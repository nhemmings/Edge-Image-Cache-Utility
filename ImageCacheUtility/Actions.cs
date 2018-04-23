using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImageCacheUtility
{
    class Actions
    {
        private string[] fullPath, oldFilesFullPath;
        private string cachePath;
        private bool returnFullPath = false;
        private DateTime oldDate;
        private string[] oldFilesDate;

        
        public dynamic[] FindEmptyFiles()
        {
            var di = new DirectoryInfo(cachePath); //create instance of directory information based on cachepath
            FileInfo[] zeroSizeFiles = di.GetFiles("*.*", SearchOption.AllDirectories).Where(fi => fi.Length == 0).ToArray();// create a file system array and add files that are 0kbs to it
            fullPath = zeroSizeFiles.Select(file => file.FullName).ToArray(); //add the 0kb file paths to string array
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
                MessageBox.Show("Please Find Empty Files First","Find Must Be Run");
            }
            else
            { 
                //passes the full path of each item in the 0kb full path array to the delete command
                for (int i = 0; i < fullPath.Length; i++)
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
            var di = new DirectoryInfo(cachePath); //create instance of directory information based on cachepath
            FileInfo[] filesCreatedOnOrBeforeDate = di.GetFiles("*.*", SearchOption.AllDirectories).Where(fi => fi.LastWriteTime.Date <= oldDate).ToArray(); //create a file system array and add files with a modified date of the delete date or older 
            oldFilesFullPath = filesCreatedOnOrBeforeDate.Select(file => file.FullName).ToArray();//add the old file paths to string array

            oldFilesDate = filesCreatedOnOrBeforeDate.Select(file => file.LastWriteTime.ToShortDateString()).ToArray(); //add the last modified date to a string array
          
        }

        public void SetDate(DateTime date)
        {
            oldDate = date;
            //Console.WriteLine("old date" + oldDate);
        }


        public string[] ReturnOldFilesFullPath()
        {
            return oldFilesFullPath;
        }

        public string[] ReturnOldFilesModifyDate()
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
                for (int i = 0; i < oldFilesFullPath.Length; i++)
                {
                    File.Delete(oldFilesFullPath[i]);
                }
                MessageBox.Show("Old Files Deleted", "Delete Old Files Complete");
            }
        }




    }
}
