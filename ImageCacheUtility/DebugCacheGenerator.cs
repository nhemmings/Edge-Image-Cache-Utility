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

        private readonly int[] cacheSizes = { 100, 1000, 2000 };
        private readonly string[] timePointNames = { "Initial", "Progress", "Final" };

        //int numDirectoriesGenerated, numRegularFilesGenerated, num0KBFilesGenerated, numInitialTimePoints,
        //    numProgressTimePoints, numFinalTimePoints;
        //long totalCacheSizeInBytes;

        private readonly string dirPath;
        private readonly bool generate0KB, generateNested, generateBadPerms;
        private readonly int generateSize;
        private readonly Random rand;
        private Dictionary<int, int> _0KBFileDictionary;

        public DebugCacheGenerator(string dirPath, bool generate0KB, bool generateNested, bool generateBadPerms,
            int generateSize, int seed = 0) {
            this.dirPath = dirPath;
            this.generate0KB = generate0KB;
            this.generateNested = generateNested;
            this.generateBadPerms = generateBadPerms;
            this.generateSize = generateSize;
            rand = seed == 0 ? new Random() : new Random(seed);
            _0KBFileDictionary = new Dictionary<int, int>();
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
        public async Task generateCache() {
            if (generate0KB)
            {
                populate0KBFileDictionary();
            }

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
                    await generatePatientDirectoryContents(currDirName, currDirPath);
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

        private Dictionary<int, int> populate0KBFileDictionary()
        {
            int dirNum;

            // One directory to have nothing but a 0KB timepoint
            dirNum = populate0KBFileDictionaryNextKey();
            _0KBFileDictionary.Add(dirNum, -1);

            // One or two directories with complete 0KB timepoints
            for (int i = 0; i < rand.Next(1, 3); i++)
            {
                dirNum = populate0KBFileDictionaryNextKey();
                _0KBFileDictionary.Add(dirNum, 8);
            }

            // 5% of directories with between 1 and 5 0KB files
            for (int i = 0; i < (0.05 * cacheSizes[generateSize]))
            {
                dirNum = populate0KBFileDictionaryNextKey();
                _0KBFileDictionary.Add(dirNum, rand.Next(1, 6));
            }

            return _0KBFileDictionary;
        }

        private int populate0KBFileDictionaryNextKey()
        {
            int newKey;
            do
            {
                newKey = rand.Next(0, cacheSizes[generateSize]);
            } while (_0KBFileDictionary.ContainsKey(newKey));
            return newKey;
        }

        private async Task generatePatientDirectoryContents(string dirName, string dirPath) {
            
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
            }
        }
    }
}
