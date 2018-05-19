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
            if (!checkCacheExistsWithPrompt())
                return;

            action.ClearLists();       //clear lists of files in action
            Results.Items.Clear();  //clear listbox
            action.SetCachePath(ImageCachePathBox.Text);    //seth cache path with path provided
            action.FindEmptyFiles();
            //action.Debug_FullPath();
            if (action.ReturnEmptyFiles().Count == 0)
            {
                MessageBox.Show("No empty files found.", "No Empty Files");
            }
            else { 
                //add the returned empty files (names or full path) to the list element 
                for (int i = 0; i < action.ReturnEmptyFiles().Count; i++)
                {
                    Results.Items.Add(action.ReturnEmptyFiles()[i]);
                }
                Fix.IsEnabled = true;
            }
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            if (!checkCacheExistsWithPrompt())
                return;

            Fix.IsEnabled = false;
            action.FixEmptyFiles();
            Results.Items.Clear(); //clear the list after deleting them, makes it look like the app actually did something.
            action.ClearLists(); //clear list of files in action
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            action.SetReturnFullPathToggle();
        }

        private void Find_Delete_Click(object sender, RoutedEventArgs e)
        {
            //they must have a cache path or it will not run
            if (!checkCacheExistsWithPrompt())               
                return;
            

            Results_Delete.Items.Clear(); //clear list
            //Console.WriteLine("Cleared");
            action.SetCachePath(ImageCachePathBox_Delete.Text); //set path with path provided in Delete Old Items tab
            // Console.WriteLine(DesiredRemovalDate.DisplayDate);
            //Console.WriteLine(DesiredRemovalDate.DisplayDateStart);

            //check for date in past or it will not run todays date will delete entire cache probably not what they want
            if (DesiredRemovalDate.DisplayDate.Date >= System.DateTime.Now.Date)
            {
                MessageBox.Show("The delete date is today's date or a future date. Please select a date in the past.", "Removal Date Is Not In Past");
            }
            else
            {
                action.ClearLists(); // clear list of files in action
                //Console.WriteLine("set date");
                action.SetDate(DesiredRemovalDate.DisplayDate); //set delete date based off of date picker
                action.FindOldFiles();
                // Console.WriteLine("Generate List");

                if (action.ReturnOldFilesFullPath().Count == 0)
                {
                    Delete.IsEnabled = false;
                    MessageBox.Show("No old files found.", "No Old Files");
                }
                else { 
                    //add old files full paths to the list along with last modified date
                    for (int i = 0; i < action.ReturnOldFilesFullPath().Count; i++)
                    {
                        Results_Delete.Items.Add(action.ReturnOldFilesFullPath()[i] + "  ||  Last Modified Date: " + action.ReturnOldFilesModifyDate()[i]);
                    }
                    Delete.IsEnabled = true;
                    //Console.WriteLine("List Complete");                   
                }
                CountValue.Content = action.ReturnOldFilesFullPath().Count;
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
            Delete.IsEnabled = false;
            action.DeleteOldFiles(); //delete old files
            Results_Delete.Items.Clear(); //clear the list after deleting them, makes it look like the app actually did something.
            action.ClearLists(); // clear list of files in action
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
        private void cachePathTextChanged(object sender, TextChangedEventArgs e)
        {
            Fix.IsEnabled = false;
            Delete.IsEnabled = false;
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
        private bool checkCacheExists()
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
        private bool checkCacheExistsWithPrompt()
        {
            if (String.IsNullOrEmpty(action.CachePath))
            {
                Fix.IsEnabled = false;
                Delete.IsEnabled = false;
                MessageBox.Show("Please insert a cache path.", "Cache Path Empty");
                return false;
            }

            if (!System.IO.Directory.Exists(action.CachePath))
            {
                Fix.IsEnabled = false;
                Delete.IsEnabled = false;
                MessageBox.Show("The cache could not be found,\nor the user does not have read access.", "Cache Inaccessible");
                return false;
            }

            return true;
        }
    }
}

