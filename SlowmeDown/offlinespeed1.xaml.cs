using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Devices.Geolocation;
using System.Device.Location;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using System.Windows.Media;
using Windows.Phone.Speech.Synthesis;
using Telerik.Windows.Controls;
using System.Text.RegularExpressions;
using Microsoft.Phone.Tasks;
using System;
using System.Net;
using Microsoft.Phone.Maps.Services;

namespace SlowmeDown
{
    public partial class offlinespeed1 : PhoneApplicationPage
    {

        Boolean onlinemode = false;
        double greenspeed, yellowspeed, redspeed;
        string unit;
        Geolocator geolocator = null;
        int thresholdVal = 1;

        double maxspeed = 0;
        public double lat, longi;
        public offlinespeed1()
        {
            InitializeComponent();
            geolocator = new Geolocator();

            geolocator.DesiredAccuracy = PositionAccuracy.High;
            geolocator.MovementThreshold = thresholdVal;


            geolocator.StatusChanged += geolocator_StatusChanged;
            geolocator.PositionChanged += geolocator_PositionChanged;
        }
          void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            string status = "";

            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    // the application does not have the right capability or the location master switch is off
                    status = "Location is disabled in phone settings. Please turn it on";
                    MessageBox.Show(status);
                    break;
                case PositionStatus.Initializing:
                    // the geolocator started the tracking operation
                    status = "1";
                    
                    break;
                case PositionStatus.NoData:
                    // the location service was not able to acquire the location
                    status = "no data";
                    break;
                case PositionStatus.Ready:
                    // the location service is generating geopositions as specified by the tracking parameters
                    status = "2";
                    
                    break;
                case PositionStatus.NotAvailable:
                    status = "not available";
                    // not used in WindowsPhone, Windows desktop uses this value to signal that there is no hardware capable to acquire location information
                    break;
                case PositionStatus.NotInitialized:
                    // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state

                    break;
            }
            /*This part write the status to the UI element StatusTextBlock*/
            Dispatcher.BeginInvoke(() =>
            {
                //StatusTextBlock.Text = status;
                if (status == "1")
                    telerikLoading.IsRunning = true;
                if (status == "2")
                    telerikLoading.IsRunning = false;
            });
        }
          double speed = 0;
        double send_speed;
        void  geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {

            lat = args.Position.Coordinate.Latitude;
            longi = args.Position.Coordinate.Longitude;
            double original_speed = (double)args.Position.Coordinate.Speed;
            
           
            if (Double.IsNaN(original_speed))
                original_speed = 0;
            send_speed = Math.Round(original_speed,0);
            
            
            if (IsolatedStorageSettings.ApplicationSettings["speed_unit"].ToString() == "km")
            {
                speed = Math.Round((double)(original_speed * 3.6), 0);
                unit = "Km/Hr";
            }
            else 
            {

                speed = Math.Round((double)(original_speed * 2.23694), 0);
                unit = "Miles/Hr";
            
            }
           
            
            Dispatcher.BeginInvoke(() =>
            {
                SpeechSynthesizer synth = new SpeechSynthesizer();
                greenspeed = Convert.ToDouble(IsolatedStorageSettings.ApplicationSettings["speed_limit"].ToString())-20;
                yellowspeed = Convert.ToDouble(IsolatedStorageSettings.ApplicationSettings["speed_limit"].ToString());
                
                speed =Math.Round( speedslider.Value,0);
                if (onlinemode == true)
                {
                    WebClient webclient = new WebClient();
                    webclient.DownloadStringAsync(new System.Uri("http://yourwebisteurl?=username=" + username + "&userid=" + password + "&latitude=" + lat + "&longitude=" + longi + "&speed=" + speed + "&junk=" + DateTime.Now));

                }
                if (speed >= 0 && speed <= greenspeed)
                    {
                        try
                        {
                            synth.Dispose();
                            
                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine("Error in dispatcher 1 : " + ex.Message);
                        }

                    gaugespeed.BarBrush = new SolidColorBrush(Colors.Green);
                       
                    }
                    else if (speed > greenspeed && speed <= yellowspeed)
                    {
                        try
                        {
                            synth.Dispose();
                        }
                        catch(Exception ex)
                        {
                            Debug.WriteLine("Error in dispatcher 2 : " + ex.Message);
                        }
                        gaugespeed.BarBrush = new SolidColorBrush(Colors.Yellow);
                    }
                    else if (speed > yellowspeed)
                    {
                        
                        synth.SpeakTextAsync("You are exceeding the speed limit. Please Slow Down");
                        gaugespeed.BarBrush = new SolidColorBrush(Colors.Red);

                    }

                if (speed >= maxspeed)
                    maxspeed = speed;

                userMaxSpeedValue.Text = maxspeed + " " + unit;
                arrowspeed.Value = (double)speed;
                gaugespeed.Value = (double)speed;
                userSpeed.Text = speed.ToString();
                userSpeedUnit.Text = unit;
                GetCurrentCoordinate();
                //listboxspeed();
                
            });
        }
        string correctstreet = null;
        double streetspeed = 0;
        double topstreetspeed = 0; 
        int locationindex;
        
        private void listboxspeed()
        {
            Speedclass spcls = new Speedclass();
            if (addstreet != correctstreet)
            {
                correctstreet = addstreet;
                streetspeed = speed;
                
                spcls.location = correctstreet.ToString();
                spcls.maxspeeder = streetspeed.ToString();
                checkeventlistbox.Items.Add(spcls);
                locationindex=checkeventlistbox.Items.IndexOf(spcls);
            }
            else if (addstreet==correctstreet && topstreetspeed < speed)
            {
                topstreetspeed = speed;
                 correctstreet = addstreet;
                //checkeventlistbox.Items.IndexOf(spc)
                spcls.maxspeeder = topstreetspeed.ToString();
                spcls.location = correctstreet.ToString();
                checkeventlistbox.Items.RemoveAt(locationindex);
                checkeventlistbox.Items.Insert(locationindex,spcls);
            }

           
        }
        private GeoCoordinate MyCoordinate = null;
        private ReverseGeocodeQuery MyReverseGeocodeQuery = null;
        string addstreet=null;
        private void GetCurrentCoordinate()
        {
            Geolocator geolocator = new Geolocator();
            geolocator.DesiredAccuracy = PositionAccuracy.High;


            try
            {

                MyCoordinate = new GeoCoordinate(lat, longi);

                if (MyReverseGeocodeQuery == null || !MyReverseGeocodeQuery.IsBusy)
                {
                    MyReverseGeocodeQuery = new ReverseGeocodeQuery();
                    MyReverseGeocodeQuery.GeoCoordinate = new GeoCoordinate(MyCoordinate.Latitude, MyCoordinate.Longitude);
                    MyReverseGeocodeQuery.QueryCompleted += ReverseGeocodeQuery_QueryCompleted;
                    MyReverseGeocodeQuery.QueryAsync();
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private void ReverseGeocodeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<System.Collections.Generic.IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress address = e.Result[0].Information.Address;
                    
                    //addcity = address.City;
                    //addstate = address.State;
                    //addcountry = address.Country;
                    //addpost = address.PostalCode;
                    addstreet = address.Street;
                   
        }

            }
        }



        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

       
        string username,password,accesscode;
        
        private void broadcast_Click(object sender, RoutedEventArgs e)
        {
            if (onlinemode)
            {
                broadcast.Content = "Share my speed";
                buttonSpeedSms.Visibility = System.Windows.Visibility.Collapsed;
                onlinemode = false;
            }
            else
            {
                if (isonline() == 0)
                {
                    MessageBox.Show("This features requires internet connection", "Internet Required", MessageBoxButton.OK);
                }
                else
                {
                    Style usernameStyle = new System.Windows.Style(typeof(RadTextBox));
                    Style passwordStyle = new System.Windows.Style(typeof(RadPasswordBox));
                    string user, pass;

                    try
                    {
                        user = IsolatedStorageSettings.ApplicationSettings["signup_username"].ToString();
                        usernameStyle.Setters.Add(new Setter(RadTextBox.TextProperty, user));
                    }
                    catch
                    {
                        usernameStyle.Setters.Add(new Setter(RadTextBox.WatermarkProperty, "User Name"));   
                    }

                    try
                    {
                        pass = IsolatedStorageSettings.ApplicationSettings["signup_pass"].ToString();
                        passwordStyle.Setters.Add(new Setter(RadPasswordBox.PasswordProperty , pass));
                    }
                    catch
                    {
                        passwordStyle.Setters.Add(new Setter(RadPasswordBox.WatermarkProperty, "Password"));
                    }
                    
                    usernameStyle.Setters.Add(new Setter(RadTextBox.HeaderProperty, "User Name"));
                    
                    passwordStyle.Setters.Add(new Setter(RadPasswordBox.HeaderProperty, "Password"));
                    Style accessCode = new System.Windows.Style(typeof(RadTextBox));
                    accessCode.Setters.Add(new Setter(RadTextBox.HeaderProperty, "Access Code"));
                    accessCode.Setters.Add(new Setter(RadTextBox.WatermarkProperty, "Access Code"));

                    InputPromptSettings settings = new InputPromptSettings();
                    settings.Field1Mode = InputMode.Text;
                    settings.Field1Style = usernameStyle;
                    settings.Field2Mode = InputMode.Password;
                    settings.Field2Style = passwordStyle;
                    settings.Field3Mode = InputMode.Text;
                    settings.Field3Style = accessCode;

                    RadInputPrompt.Show(settings, "Login", closedHandler: (args) =>
                    {

                        this.username = args.Text;
                        this.password = args.Text2;
                        this.accesscode = args.Text3;
                        if (username != null || password != null)
                            ServerValidate();
                    });
                }
            }
        }
        string user, pass;
        private void ServerValidate()
        {
            int chk;
            chk = validate();   //First it validates the username and password on the client side
            if (chk == 1)   //If validate function returns 1 i.e. the username and password are validated
            {
                //Error_message.Text = "";
                user = username;
                pass = password;
                //store();
                WebClient webclient = new WebClient();
                webclient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webclient_DownloadStringCompleted);
                webclient.DownloadStringAsync(new System.Uri("http://www.bharatghimire.co.nf/SlowmeDown/login.php?username=" + user + "&userid=" + pass + "&junk=" + DateTime.Now));   //The username ans password are sent to the server and after verification the user gets options for sharing his location.
            }
        }

        private void webclient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null && e.Result != "")
                {
                    string reply = e.Result.ToString();

                    if (reply == "1") // If the reply from the server is 1 i.e. the username and password are verified then the user is asked to enter a n access code
                    {
                        if (accesscode == "" || accesscode == null)
                            MessageBox.Show("You have not entered a valid access code. Please try again.");
                        else
                        {
                            IsolatedStorageSettings.ApplicationSettings["signup"] = "done";
                            IsolatedStorageSettings.ApplicationSettings["signup_username"] = username;
                            IsolatedStorageSettings.ApplicationSettings["signup_pass"] = password;
                            IsolatedStorageSettings.ApplicationSettings.Save();
                            WebClient webclient = new WebClient();
                            webclient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webclient_DownloadStringCompleted2);
                            webclient.DownloadStringAsync(new System.Uri("http://www.bharatghimire.co.nf/SlowmeDown/accesscode.php?username=" + user + "&userid=" + pass + "&access=" + accesscode+ "&junk=" + DateTime.Now));
                        }
                    }
                    else
                    {
                        MessageBox.Show("The username or password entered is Incorrect");
                    }
                }
            }
        }

        private void webclient_DownloadStringCompleted2(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null && e.Result != "")
                {
                    string reply = e.Result.ToString();

                    if (reply == "1")
                    {
                        MessageBox.Show("Your speed is now being successfully shared!");
                        onlinemode = true;
                        broadcast.Content = "Stop Sharing";
                        buttonSpeedSms.Visibility = System.Windows.Visibility.Visible;
                        //if User name and password match navigate to given uri 
                        //NavigationService.Navigate(new Uri("/slowmedownonline.xamlusername=" + user + "&userid=" + pass + "&access=" + access1, UriKind.Relative));
                        //NavigationService.Navigate(new Uri("/speedPermission.xaml?username=" + user + "&userid=" + pass + "&access=" + access1, UriKind.Relative));
                    }
                    else if (reply == "0")
                    {
                        onlinemode = false;
                        buttonSpeedSms.Visibility = System.Windows.Visibility.Collapsed;
                        MessageBox.Show("There is something wrong. Please enter again.");
                    }
                }
            }
        }

        public static Boolean isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z0-9]*$");
            return rg.IsMatch(strToCheck);
        }
        
            public int validate()   //Function to validate the user credentials on the client side.
        {
            if (username == "Username" || username == "" || username == null || password== "Password" || password == "" || password == null)
            {
                MessageBox.Show("Please enter a valid username and password", "Error", MessageBoxButton.OK);
                return 0;
            }
            else if (username.Length < 5 || username.Length > 15 || password.Length < 5 || password.Length > 15)
            {
                MessageBox.Show("Please enter a valid username and password of length 5-15 characters", "Error", MessageBoxButton.OK);
                return 0;
            }
            else if ((!isAlphaNumeric(username)) || (!isAlphaNumeric(password)))
            {
                MessageBox.Show("Only alphanumeric characters are allowed", "Error", MessageBoxButton.OK);
                return 0;
            }
            else if (isonline() == 0)
            {
                MessageBox.Show("Network Connection Error. Please connect to the internet.", "Connectivity Error", MessageBoxButton.OK);
                return 0;
            }
            else
                return 1;
        
        }

            private int isonline()
            {
                if (!(Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType !=
            Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None))
                    return 0;
                else
                    return 1;
            }

        private void store()
        {
            throw new NotImplementedException();
        }

        private void buttonSpeedSms_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            SmsComposeTask sms = new SmsComposeTask();
            sms.Body = "Monitor my speed using SlowMeDown on windows phone" + Environment.NewLine + "username: " + username + Environment.NewLine + "accesscode: " + accesscode;
            sms.Show();
        }
        
    }
    
}


class Speedclass
{
    public string maxspeeder { get; set; }
    public string  location { get; set; }

}