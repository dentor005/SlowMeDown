using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using SlowmeDown.Resources;
using System.IO.IsolatedStorage;
using Telerik.Windows.Controls;

namespace SlowmeDown
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        string username, accesscode;
        bool pop_is_open = false;
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("settings"))
            {
                NavigationService.Navigate(new Uri("/setting.xaml", UriKind.Relative));
            }
            else if (!(IsolatedStorageSettings.ApplicationSettings["location_allow"].ToString() == "allowed"))
            {
                MessageBoxResult m = MessageBox.Show("You have not allowed us to access your location. Please allow us.", "Access Location", MessageBoxButton.OKCancel);
                if (m == MessageBoxResult.OK)
                    NavigationService.Navigate(new Uri("/setting.xaml", UriKind.Relative));
                else
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            }
            else
            {
                NavigationService.Navigate(new Uri("/offlineSpeed1.xaml", UriKind.RelativeOrAbsolute));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (isonline() == 0)
            {
                MessageBox.Show("This features requires internet connection", "Internet Required", MessageBoxButton.OK);
            }
            else
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains("settings"))
                {
                    NavigationService.Navigate(new Uri("/setting.xaml", UriKind.Relative));
                }
                else
                {
                    Style usernameStyle = new System.Windows.Style(typeof(RadTextBox));
                    usernameStyle.Setters.Add(new Setter(RadTextBox.WatermarkProperty, "UserName"));
                    usernameStyle.Setters.Add(new Setter(RadTextBox.HeaderProperty, "UserName"));
                    Style accessCode = new System.Windows.Style(typeof(RadTextBox));
                    accessCode.Setters.Add(new Setter(RadTextBox.HeaderProperty, "Access Code"));
                    accessCode.Setters.Add(new Setter(RadTextBox.WatermarkProperty, "Access Code"));

                    InputPromptSettings settings = new InputPromptSettings();
                    settings.Field1Mode = InputMode.Text;
                    settings.Field1Style = usernameStyle;
                    settings.Field3Mode = InputMode.Text;
                    settings.Field3Style = accessCode;

                    pop_is_open = true;
                    RadInputPrompt.Show(settings, "Enter Information", closedHandler: (args) =>
                    {

                        this.username = args.Text;
                        this.accesscode = args.Text3;
                        pop_is_open = false;

                        if (username != null || accesscode != null)
                            ServerValidate1();
                    });


                }
            }
            
            //NavigationService.Navigate(new Uri("/TrackMeDown.xaml", UriKind.RelativeOrAbsolute));
        }

        private int isonline()
        {
            if (!(Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType !=
        Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None))
                return 0;
            else
                return 1;
        }

        public void ServerValidate1()
        {
            if (username == "Username" || username == "" || username == null || accesscode == null || accesscode == "Access Code" || accesscode == "")
            {
                MessageBox.Show("Please enter a valid username and Accesscode", "Error", MessageBoxButton.OK);
                
            }
            else
            {
                WebClient webclient = new WebClient();
                webclient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webclient_DownloadStringCompleted1);
                webclient.DownloadStringAsync(new System.Uri("http://www.bharatghimire.co.nf/SlowmeDown/friendlogin.php?username=" + username + "&accesscode=" + accesscode + "&junk=" + DateTime.Now));
                
            }
                
           
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            if (pop_is_open)
            {
                e.Cancel = false;
                pop_is_open = false;
            }
            else
            {
                e.Cancel = true;
                App.Current.Terminate();
            }
        }

        private void webclient_DownloadStringCompleted1(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null && e.Result != "")
                {
                    string reply = e.Result.ToString();

                    if (reply == "1")
                    {

                        NavigationService.Navigate(new Uri("/TrackMeDown.xaml?username="+username+"&accesscode="+accesscode, UriKind.RelativeOrAbsolute));
                        //if User name and password match navigate to given uri 
                        //NavigationService.Navigate(new Uri("/slowmedownonline.xamlusername=" + user + "&userid=" + pass + "&access=" + access1, UriKind.Relative));
                        //NavigationService.Navigate(new Uri("/speedPermission.xaml?username=" + user + "&userid=" + pass + "&access=" + access1, UriKind.Relative));
                    }
                    else if (reply == "0")
                    {
                        MessageBox.Show("Username and Accesscode do not match. Please enter again");
                    }
                }
            }
        }

        private void appbarSettings_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/setting.xaml", UriKind.Relative));
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains("signup_shown"))
            {
                IsolatedStorageSettings.ApplicationSettings["signup_shown"] = "shown";
                IsolatedStorageSettings.ApplicationSettings.Save();
                NavigationService.Navigate(new Uri("/signUp.xaml", UriKind.Relative));
            }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/signUp.xaml", UriKind.Relative));
        }
    }
}