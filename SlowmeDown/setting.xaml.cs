using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;

namespace SlowmeDown
{
    public partial class setting : PhoneApplicationPage
    {
        public setting()
        {
            InitializeComponent();
        }

 

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            save();
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.RelativeOrAbsolute));
        }

        void save()
        {
            if (radiokilometer.IsChecked == true)
                IsolatedStorageSettings.ApplicationSettings["speed_unit"] = "km";
            else
                IsolatedStorageSettings.ApplicationSettings["speed_unit"] = "ml";

            if (txtboxSpeedLimit.Text == "")
            {
                txtboxSpeedLimit.Text = "80";
                IsolatedStorageSettings.ApplicationSettings["speed_limit"] = txtboxSpeedLimit.Text;
            }
            else
                IsolatedStorageSettings.ApplicationSettings["speed_limit"] = txtboxSpeedLimit.Text;

            if (toggleLocation.IsChecked == true)
                IsolatedStorageSettings.ApplicationSettings["location_allow"] = "allowed";
            else
                IsolatedStorageSettings.ApplicationSettings["location_allow"] = "notallowed";

            IsolatedStorageSettings.ApplicationSettings["settings"] = "done";
            IsolatedStorageSettings.ApplicationSettings.Save();

        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            save();
            e.Cancel = false;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if(IsolatedStorageSettings.ApplicationSettings.Contains("speed_unit"))
            {
                if (IsolatedStorageSettings.ApplicationSettings["speed_unit"].ToString() == "km")
                    radiokilometer.IsChecked = true;
                else
                    radiomile.IsChecked = true;
            }

            if (IsolatedStorageSettings.ApplicationSettings.Contains("speed_limit"))
                txtboxSpeedLimit.Text = IsolatedStorageSettings.ApplicationSettings["speed_limit"].ToString();

            if (IsolatedStorageSettings.ApplicationSettings.Contains("location_allow"))
            {
                if (IsolatedStorageSettings.ApplicationSettings["location_allow"].ToString() == "allowed")
                    toggleLocation.IsChecked = true;
                else
                    toggleLocation.IsChecked = false;
            }

            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settings"))
            {
                MessageBox.Show("You need to specify your preferences before proceeding further", "Your Preferences", MessageBoxButton.OK);
            }
        
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/privacy.xaml", UriKind.Relative));
        }
        
    }
}