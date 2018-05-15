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

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            if (!_CheckCacheExistsWithPrompt())
                return;

            InaccessibleFilesTab.Visibility = Visibility.Hidden; //hide inaccessible files tab
            InaccessibleFiles.Items.Clear(); //clear inaccessible files tab list view
            action.ClearInaccessibleFiles(); //clear inaccessible files list
            action.ClearLists();       //clear lists of files in action
            Zero_KB_Files.Items.Clear();//clear listview
            action.SetCachePath(ImageCachePathBox.Text);    //seth cache path with path provided
            action.FindEmptyFiles();

            if (action.ReturnEmptyFiles().Count == 0)
            {
                MessageBox.Show("No empty files found.", "No Empty Files");
            }
            else {
                //add the returned empty files (names or full path) to the list element 
                for (int i = 0; i < action.ReturnEmptyFiles().Count; i++)
                {
                    Zero_KB_Files.Items.Add(new MyItem0KB { ZeroKBFilePath = action.ReturnEmptyFiles()[i], ZeroKBSize = "0 KB" });
                }
                Fix.IsEnabled = true;

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
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            if (!_CheckCacheExistsWithPrompt())
                return;

            Fix.IsEnabled = false;
            action.FixEmptyFiles();
            Zero_KB_Files.Items.Clear();//clear the list after deleting them, makes it look like the app actually did something.
            action.ClearLists(); //clear list of files in action
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            action.SetReturnFullPathToggle();
        }

        private void Find_Delete_Click(object sender, RoutedEventArgs e)
        {
            //they must have a cache path or it will not run
            if (!_CheckCacheExistsWithPrompt())
                return;

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
                action.ClearLists(); // clear list of files in action
                action.SetDate(DesiredRemovalDate.DisplayDate); //set delete date based off of date picker
                action.FindOldFiles();

                if (action.ReturnOldFilesFullPath().Count == 0)
                {
                    Delete.IsEnabled = false;
                    MessageBox.Show("No old files found.", "No Old Files");
                }
                else {
                    //add old files full paths to the list along with last modified date
                    for (int i = 0; i < action.ReturnOldFilesFullPath().Count; i++)
                    {
                        Results_Old_Files.Items.Add(new MyItemOldFile { FilePath = action.ReturnOldFilesFullPath()[i], LastModifiedDate = action.ReturnOldFilesModifyDate()[i] });
                    }
                    Delete.IsEnabled = true;
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
                CountValue.Content = action.ReturnOldFilesFullPath().Count;
                FileSizeValue.Content = action.ReturnTotalBytes() + " " + action.ReturnFileSizeLabel();
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
                    "You are about to delete " + action.ReturnTotalBytes() + " " + action.ReturnFileSizeLabel() +
                    "\n would you like to proceed?", "Delete Prompt", MessageBoxButton.YesNo);

            //If they chose to proceed with the delete goes through delete process and clears lists
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Delete.IsEnabled = false;//disables delete button
                action.DeleteOldFiles(); //delete old files
                Results_Old_Files.Items.Clear();//clear the list after deleting them, makes it look like the app actually did something.
                action.ClearLists(); // clear list of files in action
                FileSizeValue.Content = ""; //resets file size value label
                CountValue.Content = ""; //resets count label
            }
        }

        /**
         * TextBox TextChanged listener for ImageCachePath text boxes.
         * Synchronizes CachePath property and all ImageCachePath TextBoxes any time the user modifies a path field
         */
        private void _CachePathTextChanged(object sender, TextChangedEventArgs e)
        {
            Fix.IsEnabled = false;
            Delete.IsEnabled = false;
            TextBox textBox = sender as TextBox;
            action.CachePath = textBox.Text;
            ImageCachePathBox.Text = action.CachePath;
            ImageCachePathBox_Delete.Text = action.CachePath;
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

        //private classes to add items to list views for each tab TODO is there a better way to do this?
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
    }
}


