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
    public partial class signUp : PhoneApplicationPage
    {
        public signUp()
        {
            InitializeComponent();
        }

        String user, pass, num;

        public int validate()
        {
            if (signup_username.Text == "Username" || signup_username.Text == "" || signup_pass.Password == "Password" || signup_pass.Password == "")
            {
                MessageBox.Show("Please enter a valid username and password", "Error", MessageBoxButton.OK);
                return 0;
            }
            else if (signup_username.Text.Length < 5 || signup_username.Text.Length > 15 || signup_pass.Password.Length < 5 || signup_pass.Password.Length > 15)
            {
                MessageBox.Show("Please enter a valid username and password of length 5-15 characters", "Error", MessageBoxButton.OK);
                return 0;
            }
            else if (signup_num.Text == "Phone Number" || signup_num.Text == "")
            {
                MessageBox.Show("Please enter a valid phone number", "Error", MessageBoxButton.OK);
                return 0;
            }
            else if (isonline() == 0)
            {
                MessageBox.Show("Network Connection Error. Please connect to the internet.", "Connectivity Error", MessageBoxButton.OK);
                return 0;
            }
            return 1;
        }



        public int isonline()
        {
            if (!(Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType !=
            Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None))
                return 0;
            else
                return 1;
        }

        private void signup_username_GotFocus(object sender, RoutedEventArgs e)
        {
            if (signup_username.Text == "Username")
            {
                signup_username.Text = "";
            }
        }

        private void signup_num_GotFocus(object sender, RoutedEventArgs e)
        {
            if (signup_num.Text == "Phone Number")
            {
                signup_num.Text = "";
            }
        }

        private void signup_submit_Click(object sender, RoutedEventArgs e)
        {
            int chk;
            chk = validate();
            if (chk == 1)
            {
                user = signup_username.Text;
                pass = signup_pass.Password;
                num = signup_num.Text;
                WebClient webclient = new WebClient();
                webclient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webclient_DownloadStringCompleted);
                webclient.DownloadStringAsync(new System.Uri("http://www.bharatghimire.co.nf/SlowmeDown/signup.php?username=" + user + "&password=" + pass + "&number=" + num + "&junk=" + DateTime.Now));
            }
        }

        private void webclient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string reply = e.Result.ToString();

                if (reply == "1")
                {
                    Error_msg.Text = "The Username already exists.";
                }
                else if (reply == "0")
                {
                    IsolatedStorageSettings.ApplicationSettings["signup"] = "done";
                    IsolatedStorageSettings.ApplicationSettings["signup_username"] = signup_username.Text;
                    IsolatedStorageSettings.ApplicationSettings["signup_pass"] = signup_pass.Password;
                    IsolatedStorageSettings.ApplicationSettings.Save();
                    NavigationService.Navigate(new Uri("/MainPage.xaml?mainmsg=registered", UriKind.RelativeOrAbsolute));
                }
            }
        }


        private void signup_pass_GotFocus_1(object sender, RoutedEventArgs e)
        {
            if (signup_pass.Password == "Password")
            {
                signup_pass.Password = "";
            }
        }

        private void signup_username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (signup_username.Text == "")
                signup_username.Text = "Username";
        }

        private void signup_pass_LostFocus(object sender, RoutedEventArgs e)
        {
            if (signup_pass.Password == "")
                signup_pass.Password = "Password";
        }

        private void signup_num_LostFocus(object sender, RoutedEventArgs e)
        {
            if (signup_num.Text == "")
                signup_num.Text = "Email Address";
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (isonline() == 0)
            {
                MessageBox.Show("Network Connection Error. Please connect to the internet.", "Connectivity Error", MessageBoxButton.OK);
            }
        }

    }
}