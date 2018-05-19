using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ImageCacheUtility {
    class DebugCacheGenerator {

        int[] cacheSizes = { 100, 1000, 2000 };
        string[] timePointNames = { "Initial", "Progress", "Final" };

        int numDirectoriesGenerated, numRegularFilesGenerated, num0KBFilesGenerated, numInitialTimePoints,
            numProgressTimePoints, numFinalTimePoints;
        long totalCacheSizeInBytes;

        public DebugCacheGenerator() {
        }


        /*
         * Small cache: 100 patient folders, 
         * Medium cache: 1,000 patient folders
         * Large cache: 2,000 patient folders
         * 
         * Nested cache: In addition to above sizes:
         *               1 layer of nesting
         *               1/4 records copied from parent layer
         *               That many records again unique
         *               
         * Bad Permissions:
         *  Reading: Directories that cannot be accessed
         *  Writing: Files that cannot be deleted, insufficient permissions to create directories or write in directories
         *  
         * Cache Files:
         *  1, 2, or 3 timepoints
         *  timepoint = 24 image files
         *              8 image types (0-7)
         *              3 forms of each image type
         *                  jps 220KB~1,100KB
         *                  jpg 10% less than corresponding jps
         *                  jpe 8KB~22KB
         *                  (generate jps and reuse bytes for jpg and jpe)
         * 
         */ 
        public async Task generateCache(string dirPath, bool generate0KB, bool generateNested,
                                  int generateSize, bool generateBadPerms, ListBox output, Int32 seed = 0) {

            Random rand = (seed == 0) ? new Random() : new Random(seed);

            string GUID = generateGUID(rand);
            string GUIDPath = dirPath + '\\' + GUID;
            try {
                Directory.CreateDirectory(GUIDPath);

                string currDirName, currDirPath;
                for (int i = 0; i < cacheSizes[generateSize]; i++) {
                    do {
                        currDirName = generateDirName(rand);
                        currDirPath = GUIDPath + '\\' + currDirName;
                    } while (Directory.Exists(currDirPath));
                    Directory.CreateDirectory(currDirPath);
                    numDirectoriesGenerated++;
                    await generatePatientDirectoryContents(rand, currDirName, currDirPath);
                }

                if (generate0KB) {
                    // Generate psuedorandom count of 0KB files to generate, scaled by cache size
                    // Replace existing files with 0KB ones
                        // Pick a directory to put a full timepoint of 0KB files in
                        // Pick a few directories to put 2~4 0KB files in
                        // Pick a directory for ONLY a couple 0KB files
                        // Put the remaining 0KB files in directories one at a time
                }

                if (generateNested) {
                    string nestedPath = GUIDPath + '\\' + GUID;
                    Directory.CreateDirectory(nestedPath);
                    // Copy 1/4 of first-level directories to nested directory
                    // Generate and equal number of new directories and populate them
                }
                    
            }
            catch (SystemException e) {
                MessageBox.Show(e.Message, "SystemException");
            }

            output.Items.Add("Cache generation complete.");
            output.Items.Add($"  {numDirectoriesGenerated} Directories");
            output.Items.Add($"  {num0KBFilesGenerated} 0KB files");
            output.Items.Add($"  {numRegularFilesGenerated} fake image files");
            output.Items.Add($"    {numInitialTimePoints} Initial TimePoints");
            output.Items.Add($"    {numProgressTimePoints} Progress TimePoints");
            output.Items.Add($"    {numFinalTimePoints} Final TimePoints");
            output.Items.Add($"  {totalCacheSizeInBytes} total bytes");
        }

        private string generateGUID(Random rand) {
            StringBuilder GUIDBuilder = new StringBuilder(36);
            for (int i = 0; i < GUIDBuilder.Capacity; i++) {
                
                switch (i) {
                    case 8:
                    case 13:
                    case 18:
                    case 23:
                        GUIDBuilder.Append('-');
                        continue;
                }

                if ((rand.Next() % 2) == 0)
                    GUIDBuilder.Append((char)rand.Next(97, 102));
                else
                    GUIDBuilder.Append(rand.Next(0, 9));
            }
            return GUIDBuilder.ToString();
        }

        private string generateDirName(Random rand) {
            char letter1 = (char)rand.Next(65, 90);
            char letter2 = (char)rand.Next(65, 90);
            Int32 num = rand.Next(1, 150);
            return $"{letter1}{letter2}{num}";
        }

        private async Task generatePatientDirectoryContents(Random rand, string dirName, string dirPath) {
            
            int numTimePoints = rand.Next(1, 4);
            for (int i = 0; i < numTimePoints; i++) {
                for (int j = 1; j < 9; j++) {
                    string filePathJPS = $"{dirPath}\\{dirName}-{timePointNames[i]}-{j}.jps";
                    string filePathJPG = $"{dirPath}\\{dirName}-{timePointNames[i]}-{j}.jpg";
                    string filePathJPE = $"{dirPath}\\{dirName}-{timePointNames[i]}-{j}.jpe";

                    int sizeInBytesJPS = rand.Next(220000, 1100000);
                    Byte[] fileBytes = new Byte[sizeInBytesJPS];
                    int sizeInBytesJPG = (int)(sizeInBytesJPS * 0.9);
                    int sizeInBytesJPE = rand.Next(8, 22);
                    rand.NextBytes(fileBytes);
                    
                    using (FileStream jpsStream = File.Create(filePathJPS, sizeInBytesJPS, FileOptions.Asynchronous)) {
                        jpsStream.Seek(0, SeekOrigin.Begin);
                        await jpsStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }

                    using (FileStream jpgStream = File.Create(filePathJPG, sizeInBytesJPG, FileOptions.Asynchronous)) {
                        jpgStream.Seek(0, SeekOrigin.Begin);
                        await jpgStream.WriteAsync(fileBytes, 0, sizeInBytesJPG);
                    }

                    using (FileStream jpeStream = File.Create(filePathJPE, sizeInBytesJPE, FileOptions.Asynchronous)) {
                        jpeStream.Seek(0, SeekOrigin.Begin);
                        await jpeStream.WriteAsync(fileBytes, 0, sizeInBytesJPE);
                    }
                    Console.WriteLine($"In \"generatePatientDirectoryContents\" writing to {dirName} with i = {i} and j = {j}");
                    // Statistic Tracking
                    numRegularFilesGenerated += 3;
                    totalCacheSizeInBytes += sizeInBytesJPS += sizeInBytesJPG += sizeInBytesJPE;
                }
                switch (i) {
                    case 0:
                        numInitialTimePoints++;
                        break;
                    case 1:
                        numProgressTimePoints++;
                        break;
                    case 2:
                        numFinalTimePoints++;
                        break;
                }
            }
        }
    }
}
