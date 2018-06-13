using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace ImageCacheUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool zeroKBFound, oldFilesFound, pathChange = true;
        private int oldFilesCount;
        private long oldFilesSize;
        private string sizeLabel;
        private DateTime startTime, endTime;
        Actions action = new Actions(); //initialize action class

        public MainWindow()
        {
            InitializeComponent();
            unhideDebugTab();

        }
        
        [System.Diagnostics.Conditional("DEBUG")]
        private void unhideDebugTab() {
            DebugTab.Visibility = Visibility.Visible;
        }

        private async void Analyze_Click(object sender, RoutedEventArgs e)
        {
            if (!_CheckCacheExistsWithPrompt())
                return;

            zeroKBFound = false;
            Zero_KB_Files.Items.Clear();//clear listview


            if (pathChange == true)
            {
                AnalysisResults.Items.Clear();
                FixZeroKB.IsEnabled = false;
                AnalyzeProgressBar.IsIndeterminate = true;
                Fix_Nested_Cache.IsEnabled = false;
                InaccessibleFilesTab.Visibility = Visibility.Hidden; //hide inaccessible files tab
                InaccessibleFiles.Items.Clear(); //clear inaccessible files tab list view
                action.ClearInaccessibleFiles(); //clear inaccessible files list
                action.ClearLists();       //clear lists of files in action
                action.SetCachePath(ImageCachePathBox.Text);    //seth cache path with path provided
                MessageBox.Show("Finding files this process can take a long time", "", MessageBoxButton.OK);
                startTime = DateTime.Now;
                await Task.Run(() =>
                {
                    action.FindFiles();
                });
                endTime = DateTime.Now;
                AnalysisResults.Items.Add("Scanning cache took " + (endTime - startTime).Minutes + " minutes and " + (endTime-startTime).Seconds + " seconds.");
            }
            if (action.ReturnFileInfo().Count > 0)
            {
                for (int i = 0; i < action.ReturnFileInfo().Count; i++)
                {
                    if (action.ReturnFileInfo()[i] is null)
                    {
                        continue;
                    }
                    else if (action.ReturnFileInfo()[i].Length == 0)
                    {
                        if (Convert.ToBoolean(ReturnFullPathCheckBox.IsChecked))
                        {
                            Zero_KB_Files.Items.Add(new MyItem0KB
                            {
                                ZeroKBFilePath = action.ReturnFileInfo()[i].ToString(),
                                ZeroKBSize = action.ReturnFileInfo()[i].Length.ToString() + "KB"
                            });
                        }
                        else
                        {
                            Zero_KB_Files.Items.Add(new MyItem0KB
                            {
                                ZeroKBFilePath = action.ReturnFileInfo()[i].Name,
                                ZeroKBSize = action.ReturnFileInfo()[i].Length.ToString() + "KB"
                            });
                        }
                        zeroKBFound = true;
                    }
                }

                if (zeroKBFound == true)
                {
                    AnalysisResults.Items.Add("Found " + Zero_KB_Files.Items.Count + " 0KB files.");
                    FixZeroKB.IsEnabled = true;

                }
                else
                {
                    MessageBox.Show("No empty files found.", "No Empty Files");
                }

                if (action.ReturnNestedCaches().Count > 0)
                {
                    Fix_Nested_Cache.IsEnabled = true;
                    action.ReturnNestedCacheInfo();
                }

                if (action.ReturnInaccessibleFiles().Count > 0) //if there are inaccessible files make the inaccessible files tab visible and add the file names to the list view
                {
                    InaccessibleFilesTab.Visibility = Visibility.Visible;
                    for (int i = 0; i < action.ReturnInaccessibleFiles().Count; i++)
                    {
                        InaccessibleFiles.Items.Add(new MyItemInaccessibleFiles { InaccessibleFileList = action.ReturnInaccessibleFiles()[i] });
                    }
                    InaccessibleFilesCount.Content = action.ReturnInaccessibleFiles().Count;
                    AnalysisResults.Items.Add("Found " + action.ReturnInaccessibleFiles().Count + " inaccessible files.");
                }
                FindOldFiles.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("No Files Found", "No Files");
                FindOldFiles.IsEnabled = false;
            }
            AnalyzeProgressBar.IsIndeterminate = false;
            pathChange = false;
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            if (!_CheckCacheExistsWithPrompt())
                return;

            FixZeroKB.IsEnabled = false;
            action.FixEmptyFiles();
            Zero_KB_Files.Items.Clear();//clear the list after deleting them, makes it look like the app actually did something.
        }

        private void Find_Old_Files_Click(object sender, RoutedEventArgs e)
        {
            //they must have a cache path or it will not run
            if (!_CheckCacheExistsWithPrompt())
                return;

            //check for date in past or it will not run todays date will delete entire cache probably not what they want
            if (DesiredRemovalDate.DisplayDate.Date > DateTime.Now.Date)
            {
                MessageBox.Show("The delete date is a future date. Please select today or a date in the past.", "Removal Date Is In The Future");
            }
            else
            {

                oldFilesSize = 0;
                oldFilesCount = 0;
                Results_Old_Files.Items.Clear();//clear listview
                oldFilesFound = false;
                action.SetDate(DesiredRemovalDate.DisplayDate); //set delete date based off of date picker

                for (int i = 0; i < action.ReturnFileInfo().Count; i++)
                {
                    if (action.ReturnFileInfo()[i] is null)
                    {
                        continue;
                    }
                    else if (action.ReturnFileInfo()[i].LastWriteTime.Date <= DesiredRemovalDate.DisplayDate)
                    {
                        Results_Old_Files.Items.Add(new MyItemOldFile { FilePath = action.ReturnFileInfo()[i].ToString(), LastModifiedDate = action.ReturnFileInfo()[i].LastWriteTime.ToString() });
                        oldFilesCount++;
                        oldFilesSize += action.ReturnFileInfo()[i].Length;
                        oldFilesFound = true;
                    }
                }
                if (oldFilesFound == true)
                {
                    DeleteOldFiles.IsEnabled = true;
                    AnalysisResults.Items.Add("Found " + Results_Old_Files.Items.Count + " old files.");

                }
                else
                {
                    DeleteOldFiles.IsEnabled = false;
                    MessageBox.Show("No old files found.", "No Old Files");
                }

                if (action.ReturnInaccessibleFiles().Count > 0) //if there are inaccessible files make the inaccessible files tab visible and add the file names to the list view
                {
                    InaccessibleFilesTab.Visibility = Visibility.Visible;
                    for (int i = 0; i < action.ReturnInaccessibleFiles().Count; i++)
                    {
                        InaccessibleFiles.Items.Add(new MyItemInaccessibleFiles { InaccessibleFileList = action.ReturnInaccessibleFiles()[i] });
                    }
                    InaccessibleFilesCount.Content = action.ReturnInaccessibleFiles().Count;
                }
                _ConvertBytes();
                CountValue.Content = oldFilesCount;
                FileSizeValue.Content = oldFilesSize + " " + sizeLabel;
                pathChange = false;
            }
        }

        public void FixNestedCacheClick(object sender, RoutedEventArgs e)
        {
            action.CleanUpNestedCaches();
            Fix_Nested_Cache.IsEnabled = false;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //date picker listener listens for the date picker to be changed and updates the display date to match
            DeleteOldFiles.IsEnabled = false;
            var picker = sender as DatePicker;
            DateTime? date = picker.SelectedDate;
            DesiredRemovalDate.DisplayDate = date.Value;

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            //Message box to display size of files being deleted and allows the operator to back out before the delete occurs
            MessageBoxResult messageBoxResult = MessageBox.Show(
                    "You are about to delete " + oldFilesSize + " " + sizeLabel +
                    "\n would you like to proceed?", "Delete Prompt", MessageBoxButton.YesNo);

            //If they chose to proceed with the delete goes through delete process and clears lists
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                DeleteOldFiles.IsEnabled = false;//disables delete button
                action.DeleteOldFiles(); //delete old files
                Results_Old_Files.Items.Clear();//clear the list after deleting them, makes it look like the app actually did something.
                FileSizeValue.Content = ""; //resets file size value label
                CountValue.Content = ""; //resets count label
            }
        }


        private void DebugGenerateCache_Click(object sender, RoutedEventArgs e) {
            if (!_CheckCacheExistsWithPrompt())
                return;
            int randSeed =  String.IsNullOrEmpty(DebugCacheGeneratorSeed.Text) ? 0 : 
                            Convert.ToInt32(DebugCacheGeneratorSeed.Text);

            DebugCacheGenerator debugCacheGenerator = new DebugCacheGenerator(action.CachePath,
                Cache0KBCheckBox.IsChecked ?? false,
                CacheNestedCheckBox.IsChecked ?? false,
                CachePermsCheckBox.IsChecked ?? false,
                CacheSizeComboBox.SelectedIndex,
                randSeed);

            debugCacheGenerator.generateCache();
        }


        /**
         * TextBox TextChanged listener for ImageCachePath text boxes.
         * Synchronizes CachePath property and all ImageCachePath TextBoxes any time the user modifies a path field
         */
        private void _CachePathTextChanged(object sender, TextChangedEventArgs e)
        {
            _DisableItemsReset();
            TextBox textBox = sender as TextBox;
            action.CachePath = textBox.Text;
            ImageCachePathBox.Text = action.CachePath;
            ImageCachePathBox_Debug.Text = action.CachePath;
            pathChange = true;
        }

        private void DebugCacheGeneratorSeed_TextChanged(object sender, TextChangedEventArgs e) {
            var textBox = sender as TextBox;
            var changes = e.Changes;
            foreach (var change in changes) {
                if (change.RemovedLength > 0)
                    continue;
;
                if (!char.IsDigit(textBox.Text[change.Offset])) {
                    textBox.Text = textBox.Text.Remove(change.Offset, 1);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        /**
         * Check if current cache path is a valid, accessible directory.
         */
        private bool _CheckCacheExists()
        {
            if (String.IsNullOrEmpty(action.CachePath))
                return false;

            return System.IO.Directory.Exists(action.CachePath);
        }

        /**
         * Check if current cache path is a valid, accessible directory.
         * Displays a dialog box prompting the user to enter a cache path if the path property is null or empty.
         * Displays a dialog box informing the user that the provided path is not accessible.
         */
        private bool _CheckCacheExistsWithPrompt()
        {
            if (String.IsNullOrEmpty(action.CachePath))
            {
                _DisableItemsReset();
                MessageBox.Show("Please insert a cache path.", "Cache Path Empty");
                return false;
            }

            if (!System.IO.Directory.Exists(action.CachePath))
            {
                _DisableItemsReset();
                MessageBox.Show("The cache could not be found,\nor the user does not have read access.", "Cache Inaccessible");
                return false;
            }

            return true;
        }
        //TODO is there a better way to do this?
        //private classes to add items to list views for each tab
        private class MyItemOldFile
        {
            public string FilePath { get; set; }

            public string LastModifiedDate { get; set; }
        }

        private class MyItem0KB
        {
            public string ZeroKBFilePath { get; set; }

            public string ZeroKBSize { get; set; }
        }

        private class MyItemInaccessibleFiles
        {
            public string InaccessibleFileList { get; set;}
        }

        private void _ConvertBytes()
        {
            if (oldFilesSize < 1000)
            {
                sizeLabel = "bytes";
            }
            else if (oldFilesSize > 1000 && oldFilesSize < 1000000)
            {
                oldFilesSize = oldFilesSize / 1000;
                sizeLabel = "KB";
            }
            else if (oldFilesSize > 1000000 && oldFilesSize < 1000000000)
            {
                oldFilesSize = oldFilesSize / 1000000;
                sizeLabel = "MB";
            }
            else if (oldFilesSize > 1000000000 && oldFilesSize < 1000000000000)
            {
                oldFilesSize = oldFilesSize / 1000000000;
                sizeLabel = "GB";
            }
            else
            {
                oldFilesSize = oldFilesSize / 1000000000000;
                sizeLabel = "TB";
            }
        }
        private void _DisableItemsReset() //Resets items back to their disabled state
        {
            FixZeroKB.IsEnabled = false;
            DeleteOldFiles.IsEnabled = false;
            Fix_Nested_Cache.IsEnabled = false;
            FindOldFiles.IsEnabled = false;
        }
 
    }
}


