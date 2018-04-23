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
            //they must have a cache path or it will not run
            if ((ImageCachePathBox.Text != null) && (ImageCachePathBox.Text != ""))
            {

                Results.Items.Clear();  //clear list
                action.SetCachePath(ImageCachePathBox.Text);    //seth cache path with path provided
                action.FindEmptyFiles();    
                //action.Debug_FullPath();
                
                //add the returned empty files (names or full path) to the list element 
                for(int i =0; i < action.FindEmptyFiles().Length; i++)
                {
                    Results.Items.Add(action.FindEmptyFiles()[i]);
                }
            }
            else
            {
                MessageBox.Show("Please insert a cache path.", "Cache Path Empty");
            }
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            //they must have a cache path or it will not run
            if ((ImageCachePathBox.Text != null) && (ImageCachePathBox.Text != ""))
            {
                action.FixEmptyFiles();
                Results.Items.Clear(); //clear the list after deleting them, makes it look like the app actually did something.
            }
            else
            {
                MessageBox.Show("Please insert a cache path.", "Cache Path Empty");
            }
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            action.SetReturnFullPathToggle();
        }

        private void Find_Delete_Click(object sender, RoutedEventArgs e)
        {
            //they must have a cache path or it will not run
            if ((ImageCachePathBox_Delete.Text != null) && (ImageCachePathBox_Delete.Text != ""))
            {

                Results_Delete.Items.Clear(); //clear list
                //Console.WriteLine("Cleared");
                action.SetCachePath(ImageCachePathBox_Delete.Text); //set path with path provided in Delete Old Items tab
               // Console.WriteLine(DesiredRemovalDate.DisplayDate);
                //Console.WriteLine(DesiredRemovalDate.DisplayDateStart);

                //check for date in past or it will not run todays date will delete entire cache probably not what they want
                if (DesiredRemovalDate.DisplayDate.Date  >= System.DateTime.Now.Date)
                {
                    MessageBox.Show("The delete date is todays date or a future date. Please select a date in the past.", "Removal Date Is Not In Past");
                }
                else
                {
                    //Console.WriteLine("set date");
                    action.SetDate(DesiredRemovalDate.DisplayDate); //set delete date based off of date picker
                    action.FindOldFiles();
                   // Console.WriteLine("Generate List");

                    //add old files full paths to the list along with last modified date
                    for (int i = 0; i < action.ReturnOldFilesFullPath().Length; i++)
                    {
                        Results_Delete.Items.Add(action.ReturnOldFilesFullPath()[i] + "  ||  Last Modified Date: " + action.ReturnOldFilesModifyDate()[i]);
                    }
                    //Console.WriteLine("List Complete");
                    CountValue.Content = action.ReturnOldFilesFullPath().Length;
                }
            }
            else
            {
                MessageBox.Show("Please insert a cache path.", "Cache Path Empty");
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            //date picker listener listens for the date picker to be changed and updates the display date to match
            var picker = sender as DatePicker;
            DateTime? date = picker.SelectedDate;
            DesiredRemovalDate.DisplayDate = date.Value;
            
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            action.DeleteOldFiles(); //delete old files
            Results_Delete.Items.Clear(); //clear the list after deleting them, makes it look like the app actually did something.
        }
    }
}
