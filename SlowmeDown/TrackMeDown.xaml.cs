using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Maps.Services;
using System.IO.IsolatedStorage;
using Windows.Devices.Geolocation;
using System.Device.Location;
using Microsoft.Phone.Maps.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.ComponentModel;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Tasks;

namespace SlowmeDown
{
    public partial class TrackMeDown : PhoneApplicationPage
    {
        double lat, longi, speed, speed2;
        double maxspeed = 0, greenspeed, yellowspeed;
        string unit;
        public string rusername, ruserid;
        public string access1;
        bool tracking = false;
        Geolocator geolocator = null;
        public string addstreet, addcity, addstate, addcountry, addpost;

        //for storing the login settings
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        // My current location
        private GeoCoordinate MyCoordinate = null;

        // Reverse geocode query
        private ReverseGeocodeQuery MyReverseGeocodeQuery = null;

        BackgroundWorker worker = new BackgroundWorker();
        /// Accuracy of my current location in meters;

        private double _accuracy = 0.0;
        string access, fname, addr;

        public TrackMeDown()
        {
            InitializeComponent();
            worker.DoWork += new DoWorkEventHandler(worker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
        }

        private void PhoneApplicationPage_Loaded_1(object sender, RoutedEventArgs e)
        {
            access = NavigationContext.QueryString["accesscode"];
            fname = NavigationContext.QueryString["username"];

            if (!worker.IsBusy)
            {
                worker.RunWorkerAsync();
            }
        }
        
        //private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        //{
            
        
        //}
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            System.Threading.Thread.Sleep(500);
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WebClient webclient = new WebClient();
            webclient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(webclient_DownloadStringCompleted);
            webclient.DownloadStringAsync(new System.Uri("http://yourwebisteurl?access=" + access + "&fname=" + fname + "&junk=" + DateTime.Now));
            worker.RunWorkerAsync();
        }

        void webclient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string reply = e.Result.ToString();
                if (reply != "")
                {
                    char[] seps = { ',' };
                    string[] chk = reply.Split(seps);
                    
                    lat = Convert.ToDouble(chk[0]);
                    longi = Convert.ToDouble(chk[1]);
                    speed = Convert.ToDouble(chk[2]);
                    GetCurrentCoordinate();
                    MyCoordinate = new GeoCoordinate(lat, longi);
                    //trackmedownmap.SetView(new GeoCoordinate(lat, longi), 14D);

                    trackmedownmap.Center = new GeoCoordinate(lat, longi);
                    trackmedownmap.Pitch = 45;
                    trackmedownmap.ZoomLevel = 16;
                    trackmedownmap.PedestrianFeaturesEnabled = true;
                    trackmedownmap.LandmarksEnabled = true;
                    SpeedoMeter();
                    DrawMapMarkers();
                }
            }
        }

        private void SpeedoMeter()
        {
            if (IsolatedStorageSettings.ApplicationSettings["speed_unit"].ToString() == "km")
            {
                speed2 = Math.Round((double)(speed * 3.6), 0);
                unit = "Km/Hr";
                trackometer.Text = speed + " " + unit;
            }
            else
            {

                speed2 = Math.Round((double)(speed * 2.23694), 0);
                unit = "Miles/Hr";
                

            }
            //speed2 = speed;
            if (speed2 >= maxspeed)
            {
                maxspeed = speed2;

            }
            greenspeed = Convert.ToDouble(IsolatedStorageSettings.ApplicationSettings["speed_limit"].ToString()) - 20;
            yellowspeed = Convert.ToDouble(IsolatedStorageSettings.ApplicationSettings["speed_limit"].ToString());


            if (speed2 >= 0 && speed2 <= greenspeed)
            {

                trackometer.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (speed2 > greenspeed && speed2 <= yellowspeed)
            {
                trackometer.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else if (speed2 > yellowspeed)
            {


                trackometer.Foreground = new SolidColorBrush(Colors.Red);

            }
            maximumspeed2.Text = maxspeed.ToString();
            maximumSpeedUnit.Text = unit;
            trackometer.Text = speed2.ToString();
            trackometerUnit.Text = unit;
        }


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


        private void ReverseGeocodeQuery_QueryCompleted(object sender, QueryCompletedEventArgs<IList<MapLocation>> e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 0)
                {
                    MapAddress address = e.Result[0].Information.Address;

                    addcity = address.City;
                    addstate = address.State;
                    addcountry = address.Country;
                    addpost = address.PostalCode;
                    addstreet = address.Street;

                    }
            }
        }

        private void DrawMapMarkers()
        {
            trackmedownmap.Layers.Clear();
            MapLayer mapLayer = new MapLayer();

            // Draw marker for current position
            if (MyCoordinate != null)
            {
                //DrawAccuracyRadius(mapLayer);
                DrawMapMarker(MyCoordinate, Colors.Red, mapLayer);
            }


            trackmedownmap.Layers.Add(mapLayer);
        }

        private void DrawMapMarker(GeoCoordinate coordinate, Color color, MapLayer mapLayer)
        {
            Image img = new Image();
            BitmapImage bi3 = new BitmapImage();
            bi3.UriSource = new Uri("icons/MapMarker.png", UriKind.Relative);
            img.Stretch = Stretch.Fill;
            img.Source = bi3;

            
            img.Tag = new GeoCoordinate(lat, longi);
            img.MouseLeftButtonUp += new MouseButtonEventHandler(Marker_Click);

            // Create a MapOverlay and add marker.
            MapOverlay overlay = new MapOverlay();
            overlay.Content = img;
            overlay.GeoCoordinate = new GeoCoordinate(lat, longi);
            overlay.PositionOrigin = new Point(0.0, 1.0);
            mapLayer.Add(overlay);
        }

        private void Marker_Click(object sender, EventArgs e)
        {
            //Polygon p = (Polygon)sender;
            Image im = (Image)sender;
            GeoCoordinate geoCoordinate = (GeoCoordinate)im.Tag;
            addr = "";
            if (addstreet != "" && addstreet != null)
                addr += addstreet + ",";
            if (addcity != "" && addcity != null)
                addr += addcity + ",";
            if (addstate != "" && addstate != null)
                addr += addstate;
            if (addpost != "" && addpost != null)
                addr += "-" + addpost + ",";
            if (addcountry != "" && addcountry != null)
                addr += addcountry;
            if (addr == "")
                addr = "Address Initialsing...Press Ok and try again.";
            MessageBox.Show(addr);

         

        }
       

        

       
        private void trackmedownmap_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "c0aec027-1b32-4e21-94ac-25f603b9f796";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "dPCQtnMEtCzsYbCv7YuC2w";
        }

        private void calling_eventhandler(object sender, RoutedEventArgs e)
        {
            InputPrompt input = new InputPrompt();
            input.Completed += new EventHandler<PopUpEventArgs<string, PopUpResult>>(phone_input_Completed);
            input.Title = "Phone Number";
            input.Message = "Enter the phone no of your friend";
            input.InputScope = new InputScope { Names = { new InputScopeName() { NameValue = InputScopeNameValue.TelephoneNumber } } };
            input.Show();
        }
        string phonenum;
        private void phone_input_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    PhoneCallTask phoneCallTask = new PhoneCallTask();
                    phonenum = e.Result.ToString();
                    phoneCallTask.PhoneNumber = phonenum;
                    phoneCallTask.DisplayName = "Friend";
                    phoneCallTask.Show();
                }
            }
        }

        private void emegencycontact_Click(object sender, RoutedEventArgs e)
        {

        }

        

    }
}