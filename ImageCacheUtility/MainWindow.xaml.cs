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
        private bool zeroKBFound, oldFilesFound;
        private int oldFilesCount;
        private long oldFilesSize;
        private string sizeLabel;
        Actions action = new Actions(); //initialize action class
        
        DebugCacheGenerator debugCacheGenerator;

        public MainWindow()
        {
            InitializeComponent();

            #if DEBUG
            enableDebugTab();
            #else
            DebugTab.Visibility = Visibility.Hidden;
            #endif

        }
        
        [System.Diagnostics.Conditional("DEBUG")]
        private void enableDebugTab() {
            DebugTab.Visibility = Visibility.Visible;
            DebugTab.IsEnabled = true;
            debugCacheGenerator = new DebugCacheGenerator();
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (!_CheckCacheExistsWithPrompt())
                return;

            _0kbFixNestedCache.IsEnabled = false;
            zeroKBFound = false;
            InaccessibleFilesTab.Visibility = Visibility.Hidden; //hide inaccessible files tab
            InaccessibleFiles.Items.Clear(); //clear inaccessible files tab list view
            action.ClearInaccessibleFiles(); //clear inaccessible files list
            action.ClearLists();       //clear lists of files in action
            Zero_KB_Files.Items.Clear();//clear listview
            action.SetCachePath(ImageCachePathBox.Text);    //seth cache path with path provided
            action.FindFiles();

            for (int i = 0; i < action.ReturnFileInfo().Count; i++)
            {
                if (action.ReturnFileInfo()[i].Length == 0)
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
                Fix.IsEnabled = true;
                if (action.ReturnNestedCaches().Count > 0)
                {
                    _0kbFixNestedCache.IsEnabled = true;
                    action.ReturnNestedCacheInfo();
                }
            }
            else
            {
                MessageBox.Show("No empty files found.", "No Empty Files");
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
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            if (!_CheckCacheExistsWithPrompt())
                return;
           
            Fix.IsEnabled = false;
            action.FixEmptyFiles();
            if(_0kbFixNestedCache.IsChecked == true)
            {
                action.CleanUpNestedCaches();
                _0kbFixNestedCache.IsChecked = false;
            }
            _0kbFixNestedCache.IsEnabled = false;
            Zero_KB_Files.Items.Clear();//clear the list after deleting them, makes it look like the app actually did something.
            action.ClearLists(); //clear list of files in action
        }

        private void Find_Delete_Click(object sender, RoutedEventArgs e)
        {
            //they must have a cache path or it will not run
            if (!_CheckCacheExistsWithPrompt())
                return;

            DeleteFixNestedCache.IsEnabled = false;
            oldFilesSize = 0;
            oldFilesCount = 0;
            InaccessibleFilesTab.Visibility = Visibility.Hidden; //hide inaccessible files tab
            InaccessibleFiles.Items.Clear(); //clear inaccessible files tab list view
            action.ClearInaccessibleFiles(); //clear inaccessible files list
            Results_Old_Files.Items.Clear();//clear listview
            action.SetCachePath(ImageCachePathBox_Delete.Text); //set path with path provided in Delete Old Items tab

            //check for date in past or it will not run todays date will delete entire cache probably not what they want
            if (DesiredRemovalDate.DisplayDate.Date >= DateTime.Now.Date)
            {
                MessageBox.Show("The delete date is today's date or a future date. Please select a date in the past.", "Removal Date Is Not In Past");
            }
            else
            {
                oldFilesFound = false;
                action.ClearLists(); // clear list of files in action
                action.SetDate(DesiredRemovalDate.DisplayDate); //set delete date based off of date picker
                action.FindFiles();

                for (int i = 0; i < action.ReturnFileInfo().Count; i++)
                {
                   if (action.ReturnFileInfo()[i].LastWriteTime < DesiredRemovalDate.DisplayDate)
                   { 
                        Results_Old_Files.Items.Add(new MyItemOldFile { FilePath = action.ReturnFileInfo()[i].ToString(), LastModifiedDate = action.ReturnFileInfo()[i].LastWriteTime.ToString() });
                        oldFilesCount++;
                        oldFilesSize += action.ReturnFileInfo()[i].Length;
                        oldFilesFound = true;
                   }
                }
                if(oldFilesFound == true)
                {
                    Delete.IsEnabled = true;
                    //enable checkbox if there are nested caches
                    if (action.ReturnNestedCaches().Count > 0)
                    {
                        DeleteFixNestedCache.IsEnabled = true;
                        action.ReturnNestedCacheInfo();
                    }
                }
                else
                {
                    Delete.IsEnabled = false;
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
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //date picker listener listens for the date picker to be changed and updates the display date to match
            Delete.IsEnabled = false;
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
                Delete.IsEnabled = false;//disables delete button
                action.DeleteOldFiles(); //delete old files
                if (DeleteFixNestedCache.IsChecked == true)
                {
                    action.CleanUpNestedCaches();
                    DeleteFixNestedCache.IsChecked = false;
                }
                DeleteFixNestedCache.IsEnabled = false;
                Results_Old_Files.Items.Clear();//clear the list after deleting them, makes it look like the app actually did something.
                action.ClearLists(); // clear list of files in action
                FileSizeValue.Content = ""; //resets file size value label
                CountValue.Content = ""; //resets count label
            }
        }


        private async void DebugGenerateCache_Click(object sender, RoutedEventArgs e) {
            if (!checkCacheExistsWithPrompt())
                return;
            int randSeed =  String.IsNullOrEmpty(DebugCacheGeneratorSeed.Text) ? 0 : 
                            Convert.ToInt32(DebugCacheGeneratorSeed.Text);

            await debugCacheGenerator.generateCache(action.CachePath,
                Cache0KBCheckBox.IsChecked ?? false,
                CacheNestedCheckBox.IsChecked ?? false,
                CacheSizeComboBox.SelectedIndex,
                CachePermsCheckBox.IsChecked ?? false,
                Results_Debug,
                randSeed);
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
            ImageCachePathBox_Delete.Text = action.CachePath;
            ImageCachePathBox_Debug.Text = action.CachePath;
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
            Fix.IsEnabled = false;
            Delete.IsEnabled = false;
            _0kbFixNestedCache.IsEnabled = false;
            DeleteFixNestedCache.IsEnabled = false;
        }
    }
}


