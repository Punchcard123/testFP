﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Collections;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Globalization;
using System.Threading;
using System.Net.Sockets;
using ArdupilotMega.Utilities;
using IronPython.Hosting;
using log4net;
using ArdupilotMega.Controls;
using System.Security.Cryptography;
using ArdupilotMega.Comms;
using ArdupilotMega.Arduino;
using System.IO.Ports;
using Transitions;
using System.Web.Script.Serialization;

namespace ArdupilotMega
{
    public partial class MainV2 : Form
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // used to hide/show console window
        [DllImport("user32.dll")]
        public static extern int FindWindow(string szClass, string szTitle);
        [DllImport("user32.dll")]
        public static extern int ShowWindow(int Handle, int showState);

        const int SW_SHOWNORMAL = 1;
        const int SW_HIDE = 0;

        /// <summary>
        /// Active Comport interface
        /// </summary>
        public static MAVLink comPort = new MAVLink();
        /// <summary>
        /// passive comports
        /// </summary>
        public static List<MAVLink> Comports = new List<MAVLink>();

        /// <summary>
        /// Comport name
        /// </summary>
        public static string comPortName = "";
        /// <summary>
        /// use to store all internal config
        /// </summary>
        public static Hashtable config = new Hashtable();
        /// <summary>
        /// mono detection
        /// </summary>
        public static bool MONO = false;
        /// <summary>
        /// speech engine enable
        /// </summary>
        public static bool speechEnable = false;
        /// <summary>
        /// spech engine static class
        /// </summary>
        public static Speech speechEngine = null;
        /// <summary>
        /// joystick static class
        /// </summary>
        public static Joystick joystick = null;
        /// <summary>
        /// track last joystick packet sent. used to control rate
        /// </summary>
        DateTime lastjoystick = DateTime.Now;
        /// <summary>
        /// hud background image grabber from a video stream - not realy that efficent. ie no hardware overlays etc.
        /// </summary>
        public static WebCamService.Capture cam = null;
        /// <summary>
        /// controls the main serial reader thread
        /// </summary>
        bool serialThread = false;
        /// <summary>
        /// used for mini http server for websockets/mjpeg video stream, and network link kmls
        /// </summary>
        private TcpListener listener;
        /// <summary>
        /// track the last heartbeat sent
        /// </summary>
        private DateTime heatbeatSend = DateTime.Now;
        /// <summary>
        /// used to call anything as needed.
        /// </summary>
        public static MainV2 instance = null;

        public static string LogDir { 
            get { 
                if (config["logdirectory"] == null) 
                    return _logdir; 
                return config["logdirectory"].ToString(); 
            } 
            set
            { 
                _logdir = value;
                config["logdirectory"] = value; 
            } 
        }
        static string _logdir =  Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"logs";

        public static MainSwitcher View;

        /// <summary>
        /// used to feed in a network link kml to the http server
        /// </summary>
        public string georefkml = "";
        public string mavelous_web = Application.StartupPath + Path.DirectorySeparatorChar + @"mavelous_web\";
        public string georefimagepath = "";

        /// <summary>
        /// store the time we first connect
        /// </summary>
        DateTime connecttime = DateTime.Now;

        /// <summary>
        /// enum of firmwares
        /// </summary>
        public enum Firmwares
        {
            ArduPlane,
            ArduCopter2,
            ArduRover,
            Ateryx
        }

        DateTime connectButtonUpdate = DateTime.Now;
        /// <summary>
        /// declared here if i want a "single" instance of the form
        /// ie configuration gets reloaded on every click
        /// </summary>
        GCSViews.FlightData FlightData;
        GCSViews.FlightPlanner FlightPlanner;

        //GCSViews.ConfigurationView.Setup Configuration;
        GCSViews.Simulation Simulation;
        //GCSViews.Firmware Firmware;
        //GCSViews.Terminal Terminal;

        private Form connectionStatsForm;
        private ConnectionStats _connectionStats;

        /// <summary>
        /// This 'Control' is the toolstrip control that holds the comport combo, baudrate combo etc
        /// Otiginally seperate controls, each hosted in a toolstip sqaure, combined into this custom
        /// control for layout reasons.
        /// </summary>
        static internal ConnectionControl _connectionControl;

        //static camerainfo camer;  ConfigProject;


        public MainV2()
        {
            log.Info("Mainv2 ctor");

            Form splash = Program.Splash;

            string strVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            strVersion = "mav " + MAVLink.MAVLINK_WIRE_PROTOCOL_VERSION;

            splash.Text = "DMX Mission Planner " + Application.ProductVersion;  // +" " + strVersion;

            splash.Refresh();

            Application.DoEvents();

            instance = this;

            InitializeComponent();

            AdvButnChkBox.Checked = true;
            AdvButnChkBox.Checked = false;  // Do it again to ensure graphics updated

            View = MyView;

            _connectionControl = toolStripConnectionControl.ConnectionControl;
            _connectionControl.CMB_baudrate.TextChanged += this.CMB_baudrate_TextChanged;
            _connectionControl.CMB_baudrate.SelectedIndexChanged += this.CMB_baudrate_SelectedIndexChanged;
            _connectionControl.CMB_serialport.SelectedIndexChanged += this.CMB_serialport_SelectedIndexChanged;
            _connectionControl.CMB_serialport.Enter += this.CMB_serialport_Enter;
            _connectionControl.CMB_serialport.Click += this.CMB_serialport_Click;
            _connectionControl.TOOL_APMFirmware.SelectedIndexChanged += this.TOOL_APMFirmware_SelectedIndexChanged;

            _connectionControl.ShowLinkStats += (sender, e) => ShowConnectionStatsForm();
            srtm.datadirectory = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "srtm";

            var t = Type.GetType("Mono.Runtime");
            MONO = (t != null);

            speechEngine = new Speech();

            // proxy loader - dll load now instead of on config form load
            new Transition(new TransitionType_EaseInEaseOut(2000));

            MyRenderer.currentpressed = MenuFlightData;

            MainMenu.Renderer = new MyRenderer();

            foreach (object obj in Enum.GetValues(typeof(Firmwares)))
            {
                _connectionControl.TOOL_APMFirmware.Items.Add(obj);
            }

            if (_connectionControl.TOOL_APMFirmware.Items.Count > 0)
                _connectionControl.TOOL_APMFirmware.SelectedIndex = 0;

            this.Text = splash.Text;

            comPort.BaseStream.BaudRate = 115200;

            _connectionControl.CMB_serialport.Items.Add("AUTO");
            _connectionControl.CMB_serialport.Items.AddRange(ArdupilotMega.Comms.SerialPort.GetPortNames());
            _connectionControl.CMB_serialport.Items.Add("TCP");
            _connectionControl.CMB_serialport.Items.Add("UDP");
            if (_connectionControl.CMB_serialport.Items.Count > 0)
            {
                _connectionControl.CMB_baudrate.SelectedIndex = 7;
                _connectionControl.CMB_serialport.SelectedIndex = 0;
            }

            splash.Refresh();
            Application.DoEvents();

            // set this before we reset it
            MainV2.config["NUM_tracklength"] = "200";

            xmlconfig(false);

            if (config.ContainsKey("language") && !string.IsNullOrEmpty((string)config["language"]))
                changelanguage(CultureInfoEx.GetCultureInfo((string)config["language"]));

            if (!MONO) // windows only
            {
                if (MainV2.config["showconsole"] != null && MainV2.config["showconsole"].ToString() == "True")
                {
                }
                else
                {
                    int win = FindWindow("ConsoleWindowClass", null);
                    ShowWindow(win, SW_HIDE); // hide window
                }
            }

            ChangeUnits();

            if (config["theme"] != null)
            {
                ThemeManager.SetTheme((ThemeManager.Themes)Enum.Parse(typeof(ThemeManager.Themes), MainV2.config["theme"].ToString()));

                if (ThemeManager.CurrentTheme == ThemeManager.Themes.Custom)
                {
                    try
                    {
                        ThemeManager.BGColor = Color.FromArgb(int.Parse(MainV2.config["theme_bg"].ToString()));
                        ThemeManager.ControlBGColor = Color.FromArgb(int.Parse(MainV2.config["theme_ctlbg"].ToString()));
                        ThemeManager.TextColor = Color.FromArgb(int.Parse(MainV2.config["theme_text"].ToString()));
                        ThemeManager.ButBG = Color.FromArgb(int.Parse(MainV2.config["theme_butbg"].ToString()));
                        ThemeManager.ButBorder = Color.FromArgb(int.Parse(MainV2.config["theme_butbord"].ToString()));
                    }
                    catch { log.Error("Bad Custom theme - reset to standard"); ThemeManager.SetTheme(ThemeManager.Themes.BurntKermit); }
                }
            }

            try
            {
                FlightData = new GCSViews.FlightData();
                FlightPlanner = new GCSViews.FlightPlanner();
                //Configuration = new GCSViews.ConfigurationView.Setup();
                Simulation = new GCSViews.Simulation();
                //Firmware = new GCSViews.Firmware();
                //Terminal = new GCSViews.Terminal();

                // preload
                Python.CreateEngine();
            }
            catch (ArgumentException e)
            {
                //http://www.microsoft.com/en-us/download/details.aspx?id=16083
                //System.ArgumentException: Font 'Arial' does not support style 'Regular'.

                log.Fatal(e);
                CustomMessageBox.Show(e.ToString() + "\n\n Please install this http://www.microsoft.com/en-us/download/details.aspx?id=16083");
                this.Close();
            }
            catch (Exception e) { log.Fatal(e); CustomMessageBox.Show("A Major error has occured : " + e.ToString()); this.Close(); }

            if (MainV2.config["CHK_GDIPlus"] != null)
                GCSViews.FlightData.myhud.UseOpenGL = !bool.Parse(MainV2.config["CHK_GDIPlus"].ToString());

            try
            {
                if (config["MainLocX"] != null && config["MainLocY"] != null)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    Point startpos = new Point(int.Parse(config["MainLocX"].ToString()), int.Parse(config["MainLocY"].ToString()));
                    this.Location = startpos;
                }

                if (config["MainMaximised"] != null)
                {
                    this.WindowState = (FormWindowState)Enum.Parse(typeof(FormWindowState), config["MainMaximised"].ToString());
                    // dont allow minimised start state
                    if (this.WindowState == FormWindowState.Minimized)
                    {
                        this.WindowState = FormWindowState.Normal;
                        this.Location = new Point(100, 100);
                    }
                }

                if (config["MainHeight"] != null)
                    this.Height = int.Parse(config["MainHeight"].ToString());
                if (config["MainWidth"] != null)
                    this.Width = int.Parse(config["MainWidth"].ToString());

                if (config["CMB_rateattitude"] != null)
                    MainV2.comPort.MAV.cs.rateattitude = byte.Parse(config["CMB_rateattitude"].ToString());
                if (config["CMB_rateposition"] != null)
                    MainV2.comPort.MAV.cs.rateposition = byte.Parse(config["CMB_rateposition"].ToString());
                if (config["CMB_ratestatus"] != null)
                    MainV2.comPort.MAV.cs.ratestatus = byte.Parse(config["CMB_ratestatus"].ToString());
                if (config["CMB_raterc"] != null)
                    MainV2.comPort.MAV.cs.raterc = byte.Parse(config["CMB_raterc"].ToString());
                if (config["CMB_ratesensors"] != null)
                    MainV2.comPort.MAV.cs.ratesensors = byte.Parse(config["CMB_ratesensors"].ToString());

                if (config["speechenable"] != null)
                    MainV2.speechEnable = bool.Parse(config["speechenable"].ToString());

                //int fixme;
                /*
                MainV2.comPort.MAV.cs.rateattitude = 50;
                MainV2.comPort.MAV.cs.rateposition = 50;
                MainV2.comPort.MAV.cs.ratestatus = 50;
                MainV2.comPort.MAV.cs.raterc = 50;
                MainV2.comPort.MAV.cs.ratesensors = 50;
                */
                try
                {
                    if (config["TXT_homelat"] != null)
                        MainV2.comPort.MAV.cs.HomeLocation.Lat = double.Parse(config["TXT_homelat"].ToString());

                    if (config["TXT_homelng"] != null)
                        MainV2.comPort.MAV.cs.HomeLocation.Lng = double.Parse(config["TXT_homelng"].ToString());

                    if (config["TXT_homealt"] != null)
                        MainV2.comPort.MAV.cs.HomeLocation.Alt = double.Parse(config["TXT_homealt"].ToString());
                }
                catch { }
            }
            catch { }

            if (MainV2.comPort.MAV.cs.rateattitude == 0) // initilised to 10, configured above from save
            {
                CustomMessageBox.Show("NOTE: your attitude rate is 0, the hud will not work\nChange in Configuration > Planner > Telemetry Rates");
            }

            // log dir

            if (config["logdirectory"] != null)
                MainV2.LogDir = config["logdirectory"].ToString();

            //System.Threading.Thread.Sleep(2000);

            // make sure new enough .net framework is installed
            if (!MONO)
            {
                Microsoft.Win32.RegistryKey installed_versions = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
                string[] version_names = installed_versions.GetSubKeyNames();
                //version names start with 'v', eg, 'v3.5' which needs to be trimmed off before conversion
                double Framework = Convert.ToDouble(version_names[version_names.Length - 1].Remove(0, 1), CultureInfo.InvariantCulture);
                int SP = Convert.ToInt32(installed_versions.OpenSubKey(version_names[version_names.Length - 1]).GetValue("SP", 0));

                if (Framework < 3.5)
                {
                    CustomMessageBox.Show("This program requires .NET Framework 3.5. You currently have " + Framework);
                }
            }

            Application.DoEvents();

            Comports.Add(comPort);

            splash.Close();
        }


        private void ResetConnectionStats()
        {
            // If the form has been closed, or never shown before, we need do nothing, as 
            // connection stats will be reset when shown
            if (this.connectionStatsForm != null && connectionStatsForm.Visible)
            {
                // else the form is already showing.  reset the stats
                this.connectionStatsForm.Controls.Clear();
                _connectionStats = new ConnectionStats(comPort);
                this.connectionStatsForm.Controls.Add(_connectionStats);
                ThemeManager.ApplyThemeTo(this.connectionStatsForm);
            }
        }

        private void ShowConnectionStatsForm()
        {
            if (this.connectionStatsForm == null || this.connectionStatsForm.IsDisposed)
            {
                // If the form has been closed, or never shown before, we need all new stuff
                this.connectionStatsForm = new Form
                {
                    Width = 430,
                    Height = 180,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    Text = "Link Stats"
                };
                // Change the connection stats control, so that when/if the connection stats form is showing,
                // there will be something to see
                this.connectionStatsForm.Controls.Clear();
                _connectionStats = new ConnectionStats(comPort);
                this.connectionStatsForm.Controls.Add(_connectionStats);
                this.connectionStatsForm.Width = _connectionStats.Width;
            }

            this.connectionStatsForm.Show();
            ThemeManager.ApplyThemeTo(this.connectionStatsForm);
        }

        /// <summary>
        /// used to create planner screenshots - access by control-s
        /// </summary>
        internal void ScreenShot()
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                string name = "ss" + DateTime.Now.ToString("HHmmss") + ".jpg";
                bitmap.Save(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + name, System.Drawing.Imaging.ImageFormat.Jpeg);
                CustomMessageBox.Show("Screenshot saved to " + name);
            }

        }

        private void CMB_serialport_Click(object sender, EventArgs e)
        {
            string oldport = _connectionControl.CMB_serialport.Text;
            _connectionControl.CMB_serialport.Items.Clear();
            _connectionControl.CMB_serialport.Items.Add("AUTO");
            _connectionControl.CMB_serialport.Items.AddRange(ArdupilotMega.Comms.SerialPort.GetPortNames());
            _connectionControl.CMB_serialport.Items.Add("TCP");
            _connectionControl.CMB_serialport.Items.Add("UDP");
            if (_connectionControl.CMB_serialport.Items.Contains(oldport))
                _connectionControl.CMB_serialport.Text = oldport;
        }


        private void MenuFlightData_Click(object sender, EventArgs e)
        {
            MyView.ShowScreen("FlightData");
        }

        private void MenuFlightPlanner_Click(object sender, EventArgs e)
        {
            // refresh ap/ac specific items
            FlightPlanner.updateCMDParams();
            MyView.ShowScreen("FlightPlanner");
        }

        public void MenuConfiguration_Click(object sender, EventArgs e)
        {
            MyView.ShowScreen("Config");
        }

        private void MenuProjectSettings_Click(object sender, EventArgs e)
        {
            MyView.ShowScreen("ProjectSettings");
        }

        private void MenuPreFlight_Click(object sender, EventArgs e)
        {
            MyView.ShowScreen("PreFlight");
        }

        private void MenuFlightRecorder_Click(object sender, EventArgs e)
        {
            // Check if still flying- sanity check
            if (comPort.BaseStream.IsOpen && MainV2.comPort.MAV.cs.groundspeed > 4)
            {
                if (DialogResult.No == CustomMessageBox.Show("Your model is still moving, link to the Flight Recorder will disable live telemetry?", "Continue", MessageBoxButtons.YesNo))
                {
                    return;
                }
            } if (comPort.BaseStream.IsOpen)
            {
                MenuConnect_Click(sender, e);
            }
            MyView.ShowScreen("FlightRecorder");
        }

        private void MenuSimulation_Click(object sender, EventArgs e)
        {
            MyView.ShowScreen("Simulation");
        }

        private void MenuFirmware_Click(object sender, EventArgs e)
        {
            MyView.ShowScreen("Firmware");
        }

        private void MenuTerminal_Click(object sender, EventArgs e)
        {
            if (comPort.BaseStream.IsOpen)
            {
                MenuConnect_Click(sender, e);
            }
            MyView.ShowScreen("Terminal");
        }

        private void MenuConnect_Click(object sender, EventArgs e)
        {
            comPort.giveComport = false;

            // sanity check
            if (comPort.BaseStream.IsOpen && MainV2.comPort.MAV.cs.groundspeed > 4)
            {
                if (DialogResult.No == CustomMessageBox.Show("Your model is still moving are you sure you want to disconnect?", "Disconnect", MessageBoxButtons.YesNo))
                {
                    return;
                }
            }

            // cleanup from any previous sessions
            if (comPort.logfile != null)
                comPort.logfile.Close();

            if (comPort.rawlogfile != null)
                comPort.rawlogfile.Close();

            comPort.logfile = null;
            comPort.rawlogfile = null;

            // decide if this is a connect or disconnect
            if (comPort.BaseStream.IsOpen)
            {
                try
                {
                    if (speechEngine != null) // cancel all pending speech
                        speechEngine.SpeakAsyncCancelAll();

                    comPort.BaseStream.DtrEnable = false;
                    comPort.Close();
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                }

                // now that we have closed the connection, cancel the connection stats
                // so that the 'time connected' etc does not grow, but the user can still
                // look at the now frozen stats on the still open form
                try
                {
                    // if terminal is used, then closed using this button.... exception
                    ((ConnectionStats)this.connectionStatsForm.Controls[0]).StopUpdates();
                }
                catch { }

                this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.Red_button;
                this.MenuConnect.Text = "Connect";
                //this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.connect;
            }
            else
            {
                switch (_connectionControl.CMB_serialport.Text)
                {
                    case "TCP":
                        comPort.BaseStream = new TcpSerial();
                        break;
                    case "UDP":
                        comPort.BaseStream = new UdpSerial();
                        break;
                    case "AUTO":
                    default:
                        comPort.BaseStream = new Comms.SerialPort();
                        break;
                }

                // Tell the connection UI that we are now connected.
                _connectionControl.IsConnected(true);

                // Here we want to reset the connection stats counter etc.
                this.ResetConnectionStats();

                //cleanup any log being played
                comPort.logreadmode = false;
                if (comPort.logplaybackfile != null)
                    comPort.logplaybackfile.Close();
                comPort.logplaybackfile = null;

                try
                {
                    // do autoscan
                    if (_connectionControl.CMB_serialport.Text == "AUTO")
                    {
                        Comms.CommsSerialScan.Scan(false);

                        DateTime deadline = DateTime.Now.AddSeconds(50);

                        while (Comms.CommsSerialScan.foundport == false)
                        {
                            System.Threading.Thread.Sleep(100);

                            if (DateTime.Now > deadline) {
                                CustomMessageBox.Show("Timeout waiting for autoscan/no mavlink device connected");
                                _connectionControl.IsConnected(false);
                                return;
                            }
                        }

                        _connectionControl.CMB_serialport.Text = Comms.CommsSerialScan.portinterface.PortName;
                        _connectionControl.CMB_baudrate.Text = Comms.CommsSerialScan.portinterface.BaudRate.ToString();
                    }

                    // set port, then options
                    comPort.BaseStream.PortName = _connectionControl.CMB_serialport.Text;

                    comPort.BaseStream.DataBits = 8;
                    comPort.BaseStream.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "1");
                    comPort.BaseStream.Parity = (Parity)Enum.Parse(typeof(Parity), "None");
                    try
                    {
                        comPort.BaseStream.BaudRate = int.Parse(_connectionControl.CMB_baudrate.Text);
                    }
                    catch (Exception exp) { log.Error(exp); }

                    // false here
                    comPort.BaseStream.DtrEnable = false;
                    comPort.BaseStream.RtsEnable = false;

                    // prevent serialreader from doing anything
                    comPort.giveComport = true;

                        // reset on connect logic.
                        if (config["CHK_resetapmonconnect"] == null || bool.Parse(config["CHK_resetapmonconnect"].ToString()) == true)
                            comPort.BaseStream.toggleDTR();

                        comPort.giveComport = false;

                    // setup to record new logs
                    try
                    {
                        Directory.CreateDirectory(MainV2.LogDir);
                        comPort.logfile = new BinaryWriter(File.Open(MainV2.LogDir + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".tlog", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None));

                        comPort.rawlogfile = new BinaryWriter(File.Open(MainV2.LogDir + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".rlog", FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None));
                    }
                    catch (Exception exp2) { log.Error(exp2); CustomMessageBox.Show("Failed to create log - wont log this session"); } // soft fail

                    // reset connect time - for timeout functions
                    connecttime = DateTime.Now;

                    // do the connect
                    comPort.Open(true);

                    // detect firmware we are conected to.
                    if (comPort.MAV.param["SYSID_SW_TYPE"] != null)
                    {
                        if (float.Parse(comPort.MAV.param["SYSID_SW_TYPE"].ToString()) == 10)
                        {
                            _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.ArduCopter2);
                        }
                        else if (float.Parse(comPort.MAV.param["SYSID_SW_TYPE"].ToString()) == 7)
                        {
                            _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.Ateryx);
                        }
                        else if (float.Parse(comPort.MAV.param["SYSID_SW_TYPE"].ToString()) == 20)
                        {
                            _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.ArduRover);
                        }
                        else if (float.Parse(comPort.MAV.param["SYSID_SW_TYPE"].ToString()) == 0)
                        {
                            _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.Items.IndexOf(Firmwares.ArduPlane);
                        }
                    }

                    // save the baudrate for this port
                    config[_connectionControl.CMB_serialport.Text + "_BAUD"] = _connectionControl.CMB_baudrate.Text;

                    // refresh config window if needed
                    if (MyView.current != null && MyView.current.Name == "Config")
                    {
                        MyView.ShowScreen("Config");
                    }


                    // load wps on connect option.
                    if (config["loadwpsonconnect"] != null && bool.Parse(config["loadwpsonconnect"].ToString()) == true)
                    {
                        MenuFlightPlanner_Click(null, null);
                        FlightPlanner.BUT_read_Click(null, null);
                    }

                    // set connected icon
                    this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.Green_button;
                    this.MenuConnect.Text = "Disconnect";
                    //this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.disconnect;
                }
                catch (Exception ex)
                {
                    log.Warn(ex);
                    try
                    {
                        _connectionControl.IsConnected(false);
                        UpdateConnectIcon();
                        comPort.Close();
                    }
                    catch { }
                    // detect firmware -> scan eeprom contents -> error if no valid ap param/apvar header detected.
                    try
                    {
                        string version = ArduinoDetect.DetectVersion(comPort.BaseStream.PortName);
                        ArduinoComms port = new ArduinoSTK();
                        if (version == "1280")
                        {
                            port = new ArduinoSTK();
                            port.BaudRate = 57600;
                        }
                        else if (version == "2560")
                        {
                            port = new ArduinoSTKv2();
                            port.BaudRate = 115200;
                        }
                        else { throw new Exception("Can not determine APM board type"); }
                        port.PortName = comPort.BaseStream.PortName;
                        port.DtrEnable = true;
                        port.Open();
                        if (port.connectAP())
                        {
                            byte[] buffer = port.download(20);
                            port.Close();

                            if ((buffer[0] == 'A' || buffer[0] == 'P') && (buffer[1] == 'A' || buffer[1] == 'P')) // this is the apvar header
                            {
                                log.Info("Valid eeprom contents");
                            }
                            else
                            {
                                CustomMessageBox.Show("You dont appear to have uploaded a firmware yet,\n\nPlease goto the firmware page and upload one.");
                                return;
                            }
                        }
                        else
                        {
                            log.Error("Could not download eeprom contents");
                        }
                    }
                    catch (Exception exp3) { log.Error(exp3); }
                    CustomMessageBox.Show("Can not establish a connection\n\n" + ex.Message);
                    return;
                }
            }
        }

        private void CMB_serialport_SelectedIndexChanged(object sender, EventArgs e)
        {
            comPortName = _connectionControl.CMB_serialport.Text;
            if (comPortName == "UDP" || comPortName == "TCP" || comPortName == "AUTO")
            {
                _connectionControl.CMB_baudrate.Enabled = false;
                if (comPortName == "TCP")
                    MainV2.comPort.BaseStream = new TcpSerial();
                if (comPortName == "UDP")
                    MainV2.comPort.BaseStream = new UdpSerial();
                if (comPortName == "AUTO")
                {
                    MainV2.comPort.BaseStream = new ArdupilotMega.Comms.SerialPort();
                    return;
                }
            }
            else
            {
                _connectionControl.CMB_baudrate.Enabled = true;
                MainV2.comPort.BaseStream = new ArdupilotMega.Comms.SerialPort();
            }

            try
            {
                comPort.BaseStream.PortName = _connectionControl.CMB_serialport.Text;

                MainV2.comPort.BaseStream.BaudRate = int.Parse(_connectionControl.CMB_baudrate.Text);

                // check for saved baud rate and restore
                if (config[_connectionControl.CMB_serialport.Text + "_BAUD"] != null)
                {
                    _connectionControl.CMB_baudrate.Text = config[_connectionControl.CMB_serialport.Text + "_BAUD"].ToString();
                }
            }
            catch { }
        }

        private void MainV2_FormClosed(object sender, FormClosedEventArgs e)
        {
            // shutdown threads
            GCSViews.FlightData.threadrun = 0;
            GCSViews.Simulation.threadrun = 0;

            // shutdown local thread
            serialThread = false;

            try
            {
                if (comPort.BaseStream.IsOpen)
                    comPort.Close();
            }
            catch { } // i get alot of these errors, the port is still open, but not valid - user has unpluged usb
            try
            {
                FlightData.Dispose();
            }
            catch { }
            try
            {
                FlightPlanner.Dispose();
            }
            catch { }
            try
            {
                Simulation.Dispose();
            }
            catch { }


            // save config
            xmlconfig(true);
        }


        private void xmlconfig(bool write)
        {
            if (write || !File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"config.xml"))
            {
                try
                {
                    XmlTextWriter xmlwriter = new XmlTextWriter(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"config.xml", Encoding.ASCII);
                    xmlwriter.Formatting = Formatting.Indented;

                    xmlwriter.WriteStartDocument();

                    xmlwriter.WriteStartElement("Config");

                    xmlwriter.WriteElementString("comport", comPortName);

                    xmlwriter.WriteElementString("baudrate", _connectionControl.CMB_baudrate.Text);

                    xmlwriter.WriteElementString("APMFirmware", MainV2.comPort.MAV.cs.firmware.ToString());

                    foreach (string key in config.Keys)
                    {
                        try
                        {
                            if (key == "" || key.Contains("/")) // "/dev/blah"
                                continue;
                            xmlwriter.WriteElementString(key, config[key].ToString());
                        }
                        catch { }
                    }

                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndDocument();
                    xmlwriter.Close();
                }
                catch (Exception ex) { CustomMessageBox.Show(ex.ToString()); }
            }
            else
            {
                try
                {
                    using (XmlTextReader xmlreader = new XmlTextReader(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"config.xml"))
                    {
                        while (xmlreader.Read())
                        {
                            xmlreader.MoveToElement();
                            try
                            {
                                switch (xmlreader.Name)
                                {
                                    case "comport":
                                        string temp = xmlreader.ReadString();

                                        _connectionControl.CMB_serialport.SelectedIndex = _connectionControl.CMB_serialport.FindString(temp);
                                        if (_connectionControl.CMB_serialport.SelectedIndex == -1)
                                        {
                                            _connectionControl.CMB_serialport.Text = temp; // allows ports that dont exist - yet
                                        }
                                        comPort.BaseStream.PortName = temp;
                                        comPortName = temp;
                                        break;
                                    case "baudrate":
                                        string temp2 = xmlreader.ReadString();

                                        _connectionControl.CMB_baudrate.SelectedIndex = _connectionControl.CMB_baudrate.FindString(temp2);
                                        if (_connectionControl.CMB_baudrate.SelectedIndex == -1)
                                        {
                                            _connectionControl.CMB_baudrate.Text = temp2;
                                            //CMB_baudrate.SelectedIndex = CMB_baudrate.FindString("57600"); ; // must exist
                                        }
                                        //bau = int.Parse(CMB_baudrate.Text);
                                        break;
                                    case "APMFirmware":
                                        string temp3 = xmlreader.ReadString();
                                        _connectionControl.TOOL_APMFirmware.SelectedIndex = _connectionControl.TOOL_APMFirmware.FindStringExact(temp3);
                                        if (_connectionControl.TOOL_APMFirmware.SelectedIndex == -1)
                                            _connectionControl.TOOL_APMFirmware.SelectedIndex = 0;
                                        MainV2.comPort.MAV.cs.firmware = (MainV2.Firmwares)Enum.Parse(typeof(MainV2.Firmwares), _connectionControl.TOOL_APMFirmware.Text);
                                        break;
                                    case "Config":
                                        break;
                                    case "xml":
                                        break;
                                    default:
                                        if (xmlreader.Name == "") // line feeds
                                            break;
                                        config[xmlreader.Name] = xmlreader.ReadString();
                                        break;
                                }
                            }
                            // silent fail on bad entry
                            catch (Exception ee)
                            {
                                log.Error(ee);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Bad Config File", ex);
                }
            }
        }

        private void CMB_baudrate_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                comPort.BaseStream.BaudRate = int.Parse(_connectionControl.CMB_baudrate.Text);
            }
            catch
            {
            }
        }

        /// <summary>
        /// thread used to send joystick packets to the MAV
        /// </summary>
        private void joysticksend()
        {

            float rate = 50;
            int count = 0;

            DateTime lastratechange = DateTime.Now;

            while (true)
            {
                try
                {
                    if (MONO)
                    {
                        log.Error("Mono: closing joystick thread");
                        break;
                    }

                    if (!MONO)
                    {
                        //joystick stuff

                        if (joystick != null && joystick.enabled)
                        {
                            MAVLink.mavlink_rc_channels_override_t rc = new MAVLink.mavlink_rc_channels_override_t();

                            rc.target_component = comPort.MAV.compid;
                            rc.target_system = comPort.MAV.sysid;

                            if (joystick.getJoystickAxis(1) != Joystick.joystickaxis.None)
                                rc.chan1_raw = MainV2.comPort.MAV.cs.rcoverridech1;//(ushort)(((int)state.Rz / 65.535) + 1000);
                            if (joystick.getJoystickAxis(2) != Joystick.joystickaxis.None)
                                rc.chan2_raw = MainV2.comPort.MAV.cs.rcoverridech2;//(ushort)(((int)state.Y / 65.535) + 1000);
                            if (joystick.getJoystickAxis(3) != Joystick.joystickaxis.None)
                                rc.chan3_raw = MainV2.comPort.MAV.cs.rcoverridech3;//(ushort)(1000 - ((int)slider[0] / 65.535 ) + 1000);
                            if (joystick.getJoystickAxis(4) != Joystick.joystickaxis.None)
                                rc.chan4_raw = MainV2.comPort.MAV.cs.rcoverridech4;//(ushort)(((int)state.X / 65.535) + 1000);
                            if (joystick.getJoystickAxis(5) != Joystick.joystickaxis.None)
                                rc.chan5_raw = MainV2.comPort.MAV.cs.rcoverridech5;
                            if (joystick.getJoystickAxis(6) != Joystick.joystickaxis.None)
                                rc.chan6_raw = MainV2.comPort.MAV.cs.rcoverridech6;
                            if (joystick.getJoystickAxis(7) != Joystick.joystickaxis.None)
                                rc.chan7_raw = MainV2.comPort.MAV.cs.rcoverridech7;
                            if (joystick.getJoystickAxis(8) != Joystick.joystickaxis.None)
                                rc.chan8_raw = MainV2.comPort.MAV.cs.rcoverridech8;

                            if (lastjoystick.AddMilliseconds(rate) < DateTime.Now)
                            {
                                /*
                                if (MainV2.comPort.MAV.cs.rssi > 0 && MainV2.comPort.MAV.cs.remrssi > 0)
                                {
                                    if (lastratechange.Second != DateTime.Now.Second)
                                    {
                                        if (MainV2.comPort.MAV.cs.txbuffer > 90)
                                        {
                                            if (rate < 20)
                                                rate = 21;
                                            rate--;

                                            if (MainV2.comPort.MAV.cs.linkqualitygcs < 70)
                                                rate = 50;
                                        }
                                        else
                                        {
                                            if (rate > 100)
                                                rate = 100;
                                            rate++;
                                        }

                                        lastratechange = DateTime.Now;
                                    }
                                 
                                }
                                 */
                                //                                Console.WriteLine(DateTime.Now.Millisecond + " {0} {1} {2} {3} {4}", rc.chan1_raw, rc.chan2_raw, rc.chan3_raw, rc.chan4_raw,rate);
                                comPort.sendPacket(rc);
                                count++;
                                lastjoystick = DateTime.Now;
                            }

                        }
                    }
                    Thread.Sleep(20);
                }
                catch
                {

                } // cant fall out
            }
        }

        /// <summary>
        /// Used to fix the icon status for unexpected unplugs etc...
        /// </summary>
        private void UpdateConnectIcon()
        {
            if ((DateTime.Now - connectButtonUpdate).Milliseconds > 500)
            {
                //          Console.WriteLine(DateTime.Now.Millisecond);
                if (comPort.BaseStream.IsOpen)
                {
                    if ((string)this.MenuConnect.BackgroundImage.Tag != "Disconnect")
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.Green_button;
                            this.MenuConnect.Text = "Disconnect";
                            //this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.disconnect;
                            this.MenuConnect.BackgroundImage.Tag = "Disconnect";
                            _connectionControl.IsConnected(true);
                        });
                    }
                }
                else
                {
                    if ((string)this.MenuConnect.BackgroundImage.Tag != "Connect")
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.Red_button;
                            this.MenuConnect.Text = "Connect";
                            //this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.connect;
                            this.MenuConnect.BackgroundImage.Tag = "Connect";
                            _connectionControl.IsConnected(false);
                            if (_connectionStats != null)
                            {
                                _connectionStats.StopUpdates();
                            }
                        });
                    }

                    if (comPort.logreadmode)
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            _connectionControl.IsConnected(true);
                        });
                    }
                }
                connectButtonUpdate = DateTime.Now;
            }
        }

        /// <summary>
        /// main serial reader thread
        /// controls
        /// serial reading
        /// link quality stats
        /// speech voltage - custom - alt warning - data lost
        /// heartbeat packet sending
        /// 
        /// and can't fall out
        /// </summary>
        private void SerialReader()
        {
            if (serialThread == true)
                return;
            serialThread = true;

            int minbytes = 0;

            bool armedstatus = false;

            DateTime speechcustomtime = DateTime.Now;

            DateTime speechbatterytime = DateTime.Now;

            DateTime linkqualitytime = DateTime.Now;

            while (serialThread)
            {
                try
                {
                    //int checkthis;
                    Thread.Sleep(1); // was 5

                    // update connect/disconnect button and info stats
                    UpdateConnectIcon();

                    // 30 seconds interval speech options
                    if (speechEnable && speechEngine != null && (DateTime.Now - speechcustomtime).TotalSeconds > 30 && (MainV2.comPort.logreadmode || comPort.BaseStream.IsOpen))
                    {
                        if (MainV2.getConfig("speechcustomenabled") == "True")
                        {
                            MainV2.speechEngine.SpeakAsync(Common.speechConversion(MainV2.getConfig("speechcustom")));
                        }

                        speechcustomtime = DateTime.Now;
                    }

                    // speech for battery alerts
                    if (speechEnable && speechEngine != null && (DateTime.Now - speechbatterytime).TotalSeconds > 10 && (MainV2.comPort.logreadmode || comPort.BaseStream.IsOpen))
                    {
                        //speechbatteryvolt
                        float warnvolt = 0;
                        float.TryParse(MainV2.getConfig("speechbatteryvolt"), out warnvolt);
                        float warnpercent = 0;
                        float.TryParse(MainV2.getConfig("speechbatterypercent"), out warnpercent);

                        if (MainV2.getConfig("speechbatteryenabled") == "True" && MainV2.comPort.MAV.cs.battery_voltage <= warnvolt && MainV2.comPort.MAV.cs.battery_voltage != 0.0)
                        {
                            MainV2.speechEngine.SpeakAsync(Common.speechConversion(MainV2.getConfig("speechbattery")));
                            speechbatterytime = DateTime.Now;
                        }
                        else if (MainV2.getConfig("speechbatteryenabled") == "True" && (MainV2.comPort.MAV.cs.battery_remaining * 100) < warnpercent && MainV2.comPort.MAV.cs.battery_voltage != 0.0 && MainV2.comPort.MAV.cs.battery_remaining != 0.0)
                        {
                            MainV2.speechEngine.SpeakAsync(Common.speechConversion(MainV2.getConfig("speechbattery")));
                            speechbatterytime = DateTime.Now;
                        }

                        
                    }

                    // speech altitude warning.
                    if (speechEnable && speechEngine != null && (MainV2.comPort.logreadmode || comPort.BaseStream.IsOpen))
                    {
                        float warnalt = float.MaxValue;
                        float.TryParse(MainV2.getConfig("speechaltheight"), out warnalt);
                        try
                        {
                            if (MainV2.getConfig("speechaltenabled") == "True" && MainV2.comPort.MAV.cs.alt  != 0.00 && (MainV2.comPort.MAV.cs.alt - (int)double.Parse(MainV2.getConfig("TXT_homealt"))) <= warnalt)
                            {
                                if (MainV2.speechEngine.State == SynthesizerState.Ready)
                                    MainV2.speechEngine.SpeakAsync(Common.speechConversion(MainV2.getConfig("speechalt")));
                            }
                        }
                        catch { } // silent fail
                    }

                    // if not connected or busy, sleep and loop
                    if (!comPort.BaseStream.IsOpen || comPort.giveComport == true)
                    {
                        if (!comPort.BaseStream.IsOpen)
                        {
                            // check if other ports are still open
                            foreach (var port in Comports)
                            {
                                if (port.BaseStream.IsOpen)
                                {
                                    Console.WriteLine("Main comport shut, swapping to other mav");
                                    comPort = port;
                                    break;
                                }
                            }
                        }

                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    // make sure we attenuate the link quality if we dont see any valid packets
                    if ((DateTime.Now - comPort.lastvalidpacket).TotalSeconds > 10)
                    {
                        MainV2.comPort.MAV.cs.linkqualitygcs = 0;
                    }

                    // attenuate the link qualty over time
                    if ((DateTime.Now - comPort.lastvalidpacket).TotalSeconds >= 1)
                    {
                        if (linkqualitytime.Second != DateTime.Now.Second)
                        {
                            MainV2.comPort.MAV.cs.linkqualitygcs = (ushort)(MainV2.comPort.MAV.cs.linkqualitygcs * 0.8f);
                            linkqualitytime = DateTime.Now;

                            // force redraw is no other packets are being read
                            GCSViews.FlightData.myhud.Invalidate();
                        }
                    }

                    // send a hb every seconds from gcs to ap
                    if (heatbeatSend.Second != DateTime.Now.Second && comPort.giveComport == false)
                    {
                        MAVLink.mavlink_heartbeat_t htb = new MAVLink.mavlink_heartbeat_t()
                        {
                            type = (byte)MAVLink.MAV_TYPE.GCS,
                            autopilot = (byte)MAVLink.MAV_AUTOPILOT.INVALID,
                            mavlink_version = 3,
                        };

                        comPort.sendPacket(htb);

                        foreach (var port in MainV2.Comports)
                        {
                            if (port == MainV2.comPort)
                                continue;
                            try
                            {
                                port.sendPacket(htb);
                            }
                            catch { }
                        }

                        heatbeatSend = DateTime.Now;
                    }

                    // data loss warning - ignore first 30 seconds of connect
                    if ((DateTime.Now - comPort.lastvalidpacket).TotalSeconds > 10 && (DateTime.Now - connecttime).TotalSeconds > 30)
                    {
                        if (speechEnable && speechEngine != null)
                        {
                            if (MainV2.speechEngine.State == SynthesizerState.Ready)
                                MainV2.speechEngine.SpeakAsync("WARNING No Data for " + (int)(DateTime.Now - comPort.lastvalidpacket).TotalSeconds + " Seconds");
                        }
                    }

                    // get home point on armed status change.
                    if (armedstatus != MainV2.comPort.MAV.cs.armed)
                    {
                        armedstatus = MainV2.comPort.MAV.cs.armed;
                        // status just changed to armed
                        if (MainV2.comPort.MAV.cs.armed == true)
                        {
                            try
                            {
                                MainV2.comPort.MAV.cs.HomeLocation = new PointLatLngAlt(MainV2.comPort.getWP(0));
                                if (MyView.current != null && MyView.current.Name == "FlightPlanner")
                                {
                                    // update home if we are on flight data tab
                                    FlightPlanner.updateHome();
                                }
                            }
                            catch { 
                                // dont hang this loop
                                this.BeginInvoke((MethodInvoker)delegate { CustomMessageBox.Show("Failed to update home location"); }); 
                            }
                        }

                        if (speechEnable && speechEngine != null)
                        {
                            if (MainV2.getConfig("speecharmenabled") == "True")
                            {
                                if (armedstatus)
                                    MainV2.speechEngine.SpeakAsync(Common.speechConversion(MainV2.getConfig("speecharm")));
                                else
                                    MainV2.speechEngine.SpeakAsync(Common.speechConversion(MainV2.getConfig("speechdisarm")));
                            }
                        }
                    }

                    // actualy read the packets
                    while (comPort.BaseStream.BytesToRead > minbytes && comPort.giveComport == false)
                    {
                        try
                        {
                            comPort.readPacket();
                        }
                        catch { }
                    }

                    // read the other interfaces
                    foreach (var port in Comports)
                    {
                        if (!port.BaseStream.IsOpen)
                        {
                            // modify array and drop out
                            Comports.Remove(port);
                            break;
                        }
                        // skip primary interface
                        if (port == comPort)
                            continue;
                        while (port.BaseStream.BytesToRead > minbytes)
                        {
                            try
                            {
                                port.readPacket();
                            }
                            catch { }
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error("Serial Reader fail :" + e.ToString());
                    try
                    {
                        comPort.Close();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Override the stock ToolStripProfessionalRenderer to implement 'highlighting' of the 
        /// currently selected GCS view.
        /// </summary>
        private class MyRenderer : ToolStripProfessionalRenderer
        {
            public static ToolStripItem currentpressed;
            protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
            {
                //BackgroundImage
                if (e.Item.BackgroundImage == null) base.OnRenderButtonBackground(e);
                else
                {
                    Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
                    e.Graphics.DrawImage(e.Item.BackgroundImage, bounds);
                    if (e.Item.Pressed || e.Item == currentpressed)
                    {
                        SolidBrush brush = new SolidBrush(Color.FromArgb(73, 0x2b, 0x3a, 0x03));
                        e.Graphics.FillRectangle(brush, bounds);
                        if (e.Item.Name != "MenuConnect")
                        {
                            //Console.WriteLine("new " + e.Item.Name + " old " + currentpressed.Name );
                            //e.Item.GetCurrentParent().Invalidate();
                            if (currentpressed != e.Item)
                                currentpressed.Invalidate();
                            currentpressed = e.Item;
                        }

                        // Something...
                    }
                    else if (e.Item.Selected) // mouse over
                    {
                        SolidBrush brush = new SolidBrush(Color.FromArgb(73, 0x2b, 0x3a, 0x03));
                        e.Graphics.FillRectangle(brush, bounds);
                        // Something...
                    }
                    using (Pen pen = new Pen(Color.Black))
                    {
                        //e.GraphiMainV2.comPort.MAV.cs.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
                    }
                }
            }

            protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
            {
                //base.OnRenderItemImage(e);
            }
        }

        private void MainV2_Load(object sender, EventArgs e)
        {
            // check if its defined, and force to show it if not known about
            if (config["menu_autohide"] == null)
            {
                config["menu_autohide"] = "false";
               }
            
            try
            {
                AutoHideMenu(bool.Parse(config["menu_autohide"].ToString()));
            }
            catch { }

            MyView.AddScreen(new MainSwitcher.Screen("FlightData", FlightData, true));
            MyView.AddScreen(new MainSwitcher.Screen("FlightPlanner", FlightPlanner, true));
            MyView.AddScreen(new MainSwitcher.Screen("Config", new GCSViews.ConfigurationView.Setup(), false));

            MyView.AddScreen(new MainSwitcher.Screen("ProjectSettings", new GCSViews.ConfigProject(), false));
            MyView.AddScreen(new MainSwitcher.Screen("PreFlight", new GCSViews.PreFlightCheck(), false));
            MyView.AddScreen(new MainSwitcher.Screen("FlightRecorder", new GCSViews.FlightRecorder(), false));
            
            MyView.AddScreen(new MainSwitcher.Screen("Simulation", Simulation, true));
            MyView.AddScreen(new MainSwitcher.Screen("Firmware", new GCSViews.Firmware(), false));
            MyView.AddScreen(new MainSwitcher.Screen("Terminal", new GCSViews.Terminal(), false));
            MyView.AddScreen(new MainSwitcher.Screen("Help", new GCSViews.Help(), false));

            // init button depressed - ensures correct action
            //int fixme;
            MenuFlightData_Click(sender, e);


            // for long running tasks using own threads.
            // for short use threadpool

            // setup http server
            try
            {
                listener = new TcpListener(IPAddress.Any, 56781);
                new Thread(listernforclients)
                {
                    Name = "motion jpg stream-network kml",
                    IsBackground = true
                }.Start();
            }
            catch (Exception ex)
            {
                log.Error("Error starting TCP listener thread: ", ex);
                CustomMessageBox.Show(ex.ToString());
            }

            /// setup joystick packet sender
            new Thread(new ThreadStart(joysticksend))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
                Name = "Main joystick sender"
            }.Start();

            // setup main serial reader
            new Thread(SerialReader)
            {
                IsBackground = true,
                Name = "Main Serial reader",
                Priority = ThreadPriority.AboveNormal
            }.Start();

            try
            {
                if (!System.Diagnostics.Debugger.IsAttached)
                {
                    // single update check per day
                    if (getConfig("update_check") != DateTime.Now.ToShortDateString())
                    {
                        //CheckForUpdate();
                        //config["update_check"] = DateTime.Now.ToShortDateString();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Update check failed", ex);
            }
        }

        public static String ComputeWebSocketHandshakeSecurityHash09(String secWebSocketKey)
        {
            const String MagicKEY = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            String secWebSocketAccept = String.Empty;

            // 1. Combine the request Sec-WebSocket-Key with magic key.
            String ret = secWebSocketKey + MagicKEY;

            // 2. Compute the SHA1 hash
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] sha1Hash = sha.ComputeHash(Encoding.UTF8.GetBytes(ret));

            // 3. Base64 encode the hash
            secWebSocketAccept = Convert.ToBase64String(sha1Hash);

            return secWebSocketAccept;
        }

        void refreshmap()
        {
            MethodInvoker m = delegate()
            {
                GCSViews.FlightData.mymap.Refresh();
            };
            this.Invoke(m);
        }

        public Image ResizeImage(Image image, Size size,
bool preserveAspectRatio = true)
        {
            int newWidth;
            int newHeight;
            if (preserveAspectRatio)
            {
                int originalWidth = image.Width;
                int originalHeight = image.Height;
                float percentWidth = (float)size.Width / (float)originalWidth;
                float percentHeight = (float)size.Height / (float)originalHeight;
                float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
                newWidth = (int)(originalWidth * percent);
                newHeight = (int)(originalHeight * percent);
            }
            else
            {
                newWidth = size.Width;
                newHeight = size.Height;
            }
            Image newImage = new Bitmap(newWidth, newHeight);
            using (Graphics graphicsHandle = Graphics.FromImage(newImage))
            {
                graphicsHandle.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return newImage;
        }

        // Thread signal. 
public static ManualResetEvent tcpClientConnected = 
    new ManualResetEvent(false);


        /// <summary>          
        /// little web server for sending network link kml's          
        /// </summary>          
void listernforclients()
{
    try
    {
        listener.Start();
    }
    catch (Exception e)
    {
        log.Error("Exception starting lister. Possible multiple instances of planner?", e);
        return;
    } // in use
    // Enter the listening loop.               
    while (true)
    {
        // Perform a blocking call to accept requests.           
        // You could also user server.AcceptSocket() here.               
        try
        {
            log.Info("Listening for client");
            //TcpClient client = listener.AcceptTcpClient();

            // Set the event to nonsignaled state.
            tcpClientConnected.Reset();

            listener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), listener);

            // Wait until a connection is made and processed before  
            // continuing.
            tcpClientConnected.WaitOne();

            System.Threading.Thread.Sleep(50);

        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }
}

public void DoAcceptTcpClientCallback(IAsyncResult ar)
{
    // Get the listener that handles the client request.
    TcpListener listener = (TcpListener)ar.AsyncState;

    // End the operation and display the received data on  
    // the console.
    using (
    TcpClient client = listener.EndAcceptTcpClient(ar))
    {

        // Signal the calling thread to continue.
        tcpClientConnected.Set();


        try
        {

            // Get a stream object for reading and writing          
            log.Info("Accepted Client " + client.Client.RemoteEndPoint.ToString());
            //client.SendBufferSize = 100 * 1024; // 100kb
            //client.LingerState.Enabled = true;
            //client.NoDelay = true;

            // makesure we have valid image
            GCSViews.FlightData.mymap.streamjpgenable = true;
            GCSViews.FlightData.myhud.streamjpgenable = true;

            NetworkStream stream = client.GetStream();

            // 3 seconds
            stream.ReadTimeout = 3000;

            again:

            var asciiEncoding = new ASCIIEncoding();

            var request = new byte[1024];

            int len = stream.Read(request, 0, request.Length);
            string head = System.Text.Encoding.ASCII.GetString(request, 0, len);
            log.Info(head);

            int index = head.IndexOf('\n');

            string url = head.Substring(0, index - 1);
            //url = url.Replace("\r", "");
            //url = url.Replace("GET ","");
            //url = url.Replace(" HTTP/1.0", "");
            //url = url.Replace(" HTTP/1.1", "");

            if (url.Contains("websocket"))
            {
                using (var writer = new StreamWriter(stream, Encoding.Default))
                {
                    writer.WriteLine("HTTP/1.1 101 WebSocket Protocol Handshake");
                    writer.WriteLine("Upgrade: WebSocket");
                    writer.WriteLine("Connection: Upgrade");
                    writer.WriteLine("WebSocket-Location: ws://localhost:56781/websocket/server");

                    int start = head.IndexOf("Sec-WebSocket-Key:") + 19;
                    int end = head.IndexOf('\r', start);
                    if (end == -1)
                        end = head.IndexOf('\n', start);
                    string accept = ComputeWebSocketHandshakeSecurityHash09(head.Substring(start, end - start));

                    writer.WriteLine("Sec-WebSocket-Accept: " + accept);

                    writer.WriteLine("Server: APM Planner");

                    writer.WriteLine("");

                    writer.Flush();

                    while (client.Connected)
                    {
                        Thread.Sleep(200);
                        log.Debug(stream.DataAvailable + " " + client.Available);

                        while (client.Available > 0)
                        {
                            Console.Write(stream.ReadByte());
                        }

                        byte[] packet = new byte[256];

                        string sendme = MainV2.comPort.MAV.cs.roll + "," + MainV2.comPort.MAV.cs.pitch + "," + MainV2.comPort.MAV.cs.yaw;

                        packet[0] = 0x81; // fin - binary
                        packet[1] = (byte)sendme.Length;

                        int i = 2;
                        foreach (char ch in sendme)
                        {
                            packet[i++] = (byte)ch;
                        }

                        stream.Write(packet, 0, i);

                        //break;
                    }
                }
            }
            else if (url.Contains("georefnetwork.kml"))
            {
                string header = "HTTP/1.1 200 OK\r\nContent-Type: application/vnd.google-earth.kml+xml\r\nContent-Length: " + georefkml.Length + "\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                byte[] buffer = Encoding.ASCII.GetBytes(georefkml);

                stream.Write(buffer, 0, buffer.Length);

                goto again;

                //stream.Close();
            }
            else if (url.Contains("network.kml"))
            {
  

                SharpKml.Dom.Document kml = new SharpKml.Dom.Document();

                SharpKml.Dom.Placemark pmplane = new SharpKml.Dom.Placemark();
                pmplane.Name = "P/Q ";

                pmplane.Visibility = true;

                SharpKml.Dom.Location loc = new SharpKml.Dom.Location();
                loc.Latitude = MainV2.comPort.MAV.cs.lat;
                loc.Longitude = MainV2.comPort.MAV.cs.lng;
                loc.Altitude = MainV2.comPort.MAV.cs.alt;

                if (loc.Altitude < 0)
                    loc.Altitude = 0.01;

                SharpKml.Dom.Orientation ori = new SharpKml.Dom.Orientation();
                ori.Heading = MainV2.comPort.MAV.cs.yaw;
                ori.Roll = -MainV2.comPort.MAV.cs.roll;
                ori.Tilt = -MainV2.comPort.MAV.cs.pitch;

                SharpKml.Dom.Scale sca = new SharpKml.Dom.Scale();

                sca.X = 2;
                sca.Y = 2;
                sca.Z = 2;

                SharpKml.Dom.Model model = new SharpKml.Dom.Model();
                model.Location = loc;
                model.Orientation = ori;
                model.AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute;
                model.Scale = sca;

                SharpKml.Dom.Link link = new SharpKml.Dom.Link();
                link.Href = new Uri("block_plane_0.dae", UriKind.Relative);

                model.Link = link;

                pmplane.Geometry = model;

                SharpKml.Dom.LookAt la = new SharpKml.Dom.LookAt()
                {
                    Altitude = loc.Altitude.Value,
                    Latitude = loc.Latitude.Value,
                    Longitude = loc.Longitude.Value,
                    Tilt = 80,
                    Heading = MainV2.comPort.MAV.cs.yaw,
                    AltitudeMode = SharpKml.Dom.AltitudeMode.Absolute,
                    Range = 50
                };

                kml.Viewpoint = la;

                kml.AddFeature(pmplane);

                SharpKml.Dom.CoordinateCollection coords = new SharpKml.Dom.CoordinateCollection();

                foreach (var point in MainV2.comPort.MAV.wps.Values)
                {
                    coords.Add(new SharpKml.Base.Vector(point.x, point.y, point.z));
                }

                SharpKml.Dom.LineString ls = new SharpKml.Dom.LineString();
                ls.AltitudeMode = SharpKml.Dom.AltitudeMode.RelativeToGround;
                ls.Coordinates = coords;

                SharpKml.Dom.Placemark pm = new SharpKml.Dom.Placemark() { Geometry = ls };

                kml.AddFeature(pm);

                SharpKml.Base.Serializer serializer = new SharpKml.Base.Serializer();
                serializer.Serialize(kml);

                byte[] buffer = Encoding.ASCII.GetBytes(serializer.Xml);

                string header = "HTTP/1.1 200 OK\r\nContent-Type: application/vnd.google-earth.kml+xml\r\nContent-Length: " + buffer.Length + "\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                stream.Write(buffer, 0, buffer.Length);

                goto again;

                //stream.Close();
            }
            else if (url.Contains("block_plane_0.dae"))
            {
                string header = "HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                BinaryReader file = new BinaryReader(File.Open("block_plane_0.dae", FileMode.Open, FileAccess.Read, FileShare.Read));
                byte[] buffer = new byte[1024];
                while (file.PeekChar() != -1)
                {

                    int leng = file.Read(buffer, 0, buffer.Length);

                    stream.Write(buffer, 0, leng);
                }
                file.Close();
                stream.Close();
            }
            else if (url.Contains("hud.html"))
            {
                string header = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                BinaryReader file = new BinaryReader(File.Open("hud.html", FileMode.Open, FileAccess.Read, FileShare.Read));
                byte[] buffer = new byte[1024];
                while (file.PeekChar() != -1)
                {

                    int leng = file.Read(buffer, 0, buffer.Length);

                    stream.Write(buffer, 0, leng);
                }
                file.Close();
                stream.Close();
            }
            else if (url.ToLower().Contains("hud.jpg") || url.ToLower().Contains("map.jpg") || url.ToLower().Contains("both.jpg"))
            {
                refreshmap();

                string header = "HTTP/1.1 200 OK\r\nContent-Type: multipart/x-mixed-replace;boundary=APMPLANNER\r\n\r\n--APMPLANNER\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                while (client.Connected)
                {
                    System.Threading.Thread.Sleep(200); // 5hz
                    byte[] data = null;

                    if (url.ToLower().Contains("hud"))
                    {
                        GCSViews.FlightData.myhud.streamjpgenable = true;
                        data = GCSViews.FlightData.myhud.streamjpg.ToArray();
                    }
                    else if (url.ToLower().Contains("map"))
                    {
                        GCSViews.FlightData.mymap.streamjpgenable = true;
                        data = GCSViews.FlightData.mymap.streamjpg.ToArray();
                    }
                    else
                    {
                        GCSViews.FlightData.mymap.streamjpgenable = true;
                        GCSViews.FlightData.myhud.streamjpgenable = true;
                        Image img1 = Image.FromStream(GCSViews.FlightData.myhud.streamjpg);
                        Image img2 = Image.FromStream(GCSViews.FlightData.mymap.streamjpg);
                        int bigger = img1.Height > img2.Height ? img1.Height : img2.Height;
                        Image imgout = new Bitmap(img1.Width + img2.Width, bigger);

                        Graphics grap = Graphics.FromImage(imgout);

                        grap.DrawImageUnscaled(img1, 0, 0);
                        grap.DrawImageUnscaled(img2, img1.Width, 0);

                        MemoryStream streamjpg = new MemoryStream();
                        imgout.Save(streamjpg, System.Drawing.Imaging.ImageFormat.Jpeg);
                        data = streamjpg.ToArray();

                    }

                    header = "Content-Type: image/jpeg\r\nContent-Length: " + data.Length + "\r\n\r\n";
                    temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);

                    stream.Write(data, 0, data.Length);

                    header = "\r\n--APMPLANNER\r\n";
                    temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);

                }
                GCSViews.FlightData.mymap.streamjpgenable = false;
                GCSViews.FlightData.myhud.streamjpgenable = false;
                stream.Close();

            }
            else if (url.Contains("/guided?"))
            {
                //http://127.0.0.1:56781/guided?lat=-34&lng=117.8&alt=30

                Regex rex = new Regex(@"lat=([\-\.0-9]+)&lng=([\-\.0-9]+)&alt=([\.0-9]+)", RegexOptions.IgnoreCase);

                Match match = rex.Match(url);

                if (match.Success)
                {
                    Locationwp gwp = new Locationwp()
                    {
                        lat = float.Parse(match.Groups[1].Value),
                        lng = float.Parse(match.Groups[2].Value),
                        alt = float.Parse(match.Groups[3].Value)
                    };
                    try
                    {
                        comPort.setGuidedModeWP(gwp);
                    }
                    catch { }

                    string header = "HTTP/1.1 200 OK\r\n\r\nSent Guide Mode Wp";
                    byte[] temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);
                }
                else
                {
                    string header = "HTTP/1.1 200 OK\r\n\r\nFailed Guide Mode Wp";
                    byte[] temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);
                }
                stream.Close();
            }
            else if (url.ToLower().Contains(".jpg"))
            {
                Regex rex = new Regex(@"([^\s]+)\s(.+)\sHTTP/1", RegexOptions.IgnoreCase);

                Match match = rex.Match(url);

                if (match.Success)
                {
                    string fileurl = match.Groups[2].Value;

                    using (Image orig = Image.FromFile(georefimagepath + fileurl))
                    using (Image resi = ResizeImage(orig, new Size(640, 480)))
                    using (MemoryStream memstream = new MemoryStream())
                    {
                        resi.Save(memstream, System.Drawing.Imaging.ImageFormat.Jpeg);

                        memstream.Position = 0;

                        string header = "HTTP/1.1 200 OK\r\nContent-Type: image/jpg\r\nContent-Length: " + memstream.Length + "\r\n\r\n";
                        byte[] temp = asciiEncoding.GetBytes(header);
                        stream.Write(temp, 0, temp.Length);

                        BinaryReader file = new BinaryReader(memstream);
                        byte[] buffer = new byte[1024];
                        while (file.BaseStream.Position < file.BaseStream.Length)
                        {

                            int leng = file.Read(buffer, 0, buffer.Length);

                            stream.Write(buffer, 0, leng);
                        }
                        file.Close();
                        resi.Dispose();
                        orig.Dispose();
                    }

                    goto again;

                    //stream.Close();
                }
                else
                {
                    string header = "HTTP/1.1 404 not found\r\nContent-Type: image/jpg\r\n\r\n";
                    byte[] temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);
                }
                stream.Close();
            }
            else if (url.ToLower().Contains("post /guide"))
            {
                Regex rex = new Regex(@"lat"":([\-\.0-9]+),""lon"":([\-\.0-9]+),""alt"":([\.0-9]+)", RegexOptions.IgnoreCase);

                Match match = rex.Match(head);

                if (match.Success)
                {
                    Locationwp gwp = new Locationwp()
                    {
                        lat = float.Parse(match.Groups[1].Value),
                        lng = float.Parse(match.Groups[2].Value),
                        alt = float.Parse(match.Groups[3].Value)
                    };
                    try
                    {
                        comPort.setGuidedModeWP(gwp);
                    }
                    catch { }

                    string header = "HTTP/1.1 200 OK\r\n\r\nSent Guide Mode Wp";
                    byte[] temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);
                }
                else
                {
                    string header = "HTTP/1.1 200 OK\r\n\r\nFailed Guide Mode Wp";
                    byte[] temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);
                }
                stream.Close();
            }
            else if (url.ToLower().Contains("/command_long"))
            {
                string header = "HTTP/1.1 404 not found\r\nContent-Type: image/jpg\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                stream.Close();
            }
            else if (url.ToLower().Contains("/rcoverride"))
            {
                string header = "HTTP/1.1 404 not found\r\nContent-Type: image/jpg\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                stream.Close();
            }
            else if (url.ToLower().Contains("/get_mission"))
            {
                string header = "HTTP/1.1 404 not found\r\nContent-Type: image/jpg\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                stream.Close();
            }
            else if (url.ToLower().Contains("/mavlink/"))
            {
                /*
GET /mavlink/ATTITUDE+VFR_HUD+NAV_CONTROLLER_OUTPUT+META_WAYPOINT+GPS_RAW_INT+HEARTBEAT+META_LINKQUALITY+GPS_STATUS+STATUSTEXT+SYS_STATUS?_=1355828718540 HTTP/1.1
Host: ubuntu:9999
Connection: keep-alive
X-Requested-With: XMLHttpRequest
User-Agent: Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.97 Safari/537.11
Accept: 
Referer: http://ubuntu:9999/index.html
Accept-Encoding: gzip,deflate,sdch
Accept-Language: en-GB,en-US;q=0.8,en;q=0.6
Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.3

HTTP/1.1 200 OK
Content-Type: application/json
Content-Length: 2121
Date: Thu, 29 Nov 2012 12:13:38 GMT
Server: ubuntu

{
"VFR_HUD": {"msg": {"throttle": 0, "groundspeed": 0.0, "airspeed": 0.0, "climb": 0.0, "mavpackettype": "VFR_HUD", "alt": -0.47999998927116394, "heading": 108}, "index": 687, "time_usec": 0},
"STATUSTEXT": {"msg": {"mavpackettype": "STATUSTEXT", "severity": 1, "text": "Initialising APM..."}, "index": 2, "time_usec": 0}, 
"SYS_STATUS": {"msg": {"onboard_control_sensors_present": 4294966287, "load": 0, "battery_remaining": -1, "errors_count4": 0, "drop_rate_comm": 0, "errors_count2": 0, "errors_count3": 0, "errors_comm": 0, "current_battery": -1, "errors_count1": 0, "onboard_control_sensors_health": 4294966287, "mavpackettype": "SYS_STATUS", "onboard_control_sensors_enabled": 4294945807, "voltage_battery": 10080}, "index": 693, "time_usec": 0}, 
"META_LINKQUALITY": {"msg": {"master_in": 11110, "mav_loss": 0, "mavpackettype": "META_LINKQUALITY", "master_out": 194, "packet_loss": 0.0}, "index": 194, "time_usec": 0},
"ATTITUDE": {"msg": {"pitchspeed": -0.000976863200776279, "yaw": 1.8878594636917114, "rollspeed": -0.0030046366155147552, "time_boot_ms": 194676, "pitch": -0.09986469894647598, "mavpackettype": "ATTITUDE", "yawspeed": -0.0015030358918011189, "roll": -0.029391441494226456}, "index": 687, "time_usec": 0}, 
"GPS_RAW_INT": {"msg": {"fix_type": 1, "cog": 0, "epv": 65535, "lon": 0, "time_usec": 0, "eph": 9999, "satellites_visible": 0, "lat": 0, "mavpackettype": "GPS_RAW_INT", "alt": 137000, "vel": 0}, "index": 687, "time_usec": 0}, 
"HEARTBEAT": {"msg": {"custom_mode": 0, "system_status": 4, "base_mode": 81, "autopilot": 3, "mavpackettype": "HEARTBEAT", "type": 2, "mavlink_version": 3}, "index": 190, "time_usec": 0},
"GPS_STATUS": {"msg": {"satellite_snr": "", "satellite_azimuth": "", "satellite_prn": "", "satellite_elevation": "", "satellites_visible": 0, "satellite_used": "", "mavpackettype": "GPS_STATUS"}, "index": 2, "time_usec": 0}, 
"NAV_CONTROLLER_OUTPUT": {"msg": {"wp_dist": 0, "nav_pitch": 0.0, "target_bearing": 0, "nav_roll": 0.0, "aspd_error": 0.0, "alt_error": 0.0, "mavpackettype": "NAV_CONTROLLER_OUTPUT", "xtrack_error": 0.0, "nav_bearing": 0}, "index": 687, "time_usec": 0}}
              */

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                object[] data = new object[20];


                Messagejson message = new Messagejson();


                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_ATTITUDE] != null)
                    message.ATTITUDE = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_ATTITUDE], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_ATTITUDE].ByteArrayToStructure<MAVLink.mavlink_attitude_t>(6) };
                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_VFR_HUD] != null)
                    message.VFR_HUD = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_VFR_HUD], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_VFR_HUD].ByteArrayToStructure<MAVLink.mavlink_vfr_hud_t>(6) };
                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_NAV_CONTROLLER_OUTPUT] != null)
                    message.NAV_CONTROLLER_OUTPUT = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_NAV_CONTROLLER_OUTPUT], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_NAV_CONTROLLER_OUTPUT].ByteArrayToStructure<MAVLink.mavlink_nav_controller_output_t>(6) };
                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_GPS_RAW_INT] != null)
                    message.GPS_RAW_INT = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_GPS_RAW_INT], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_GPS_RAW_INT].ByteArrayToStructure<MAVLink.mavlink_gps_raw_int_t>(6) };
                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_HEARTBEAT] != null)
                    message.HEARTBEAT = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_HEARTBEAT], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_HEARTBEAT].ByteArrayToStructure<MAVLink.mavlink_heartbeat_t>(6) };
                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_GPS_STATUS] != null)
                    message.GPS_STATUS = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_GPS_STATUS], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_GPS_STATUS].ByteArrayToStructure<MAVLink.mavlink_gps_status_t>(6) };
                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_STATUSTEXT] != null)
                    message.STATUSTEXT = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_STATUSTEXT], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_STATUSTEXT].ByteArrayToStructure<MAVLink.mavlink_statustext_t>(6) };
                if (MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_SYS_STATUS] != null)
                    message.SYS_STATUS = new Message2() { index = MainV2.comPort.MAV.packetseencount[MAVLink.MAVLINK_MSG_ID_SYS_STATUS], msg = MainV2.comPort.MAV.packets[MAVLink.MAVLINK_MSG_ID_SYS_STATUS].ByteArrayToStructure<MAVLink.mavlink_sys_status_t>(6) };

                message.META_LINKQUALITY = message.SYS_STATUS = new Message2() { index = packetindex, msg = new META_LINKQUALITY() { master_in = (int)MainV2.comPort.packetsnotlost, mavpackettype = "META_LINKQUALITY", master_out = MainV2.comPort.packetcount, packet_loss = 100 - MainV2.comPort.MAV.cs.linkqualitygcs } };

                packetindex++;

                string output = serializer.Serialize(message);

                string header = "HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: " + output.Length + "\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                temp = asciiEncoding.GetBytes(output);
                stream.Write(temp, 0, temp.Length);

                goto again;

                //stream.Close();
            }
            else if (url.ToLower().Contains("/mav/"))
            {
                //C:\Users\hog\Desktop\DIYDrones\mavelous\modules\lib\mavelous_web


                Regex rex = new Regex(@"([^\s]+)\s(.+)\sHTTP/1", RegexOptions.IgnoreCase);

                Match match = rex.Match(url);

                if (match.Success)
                {
                    string fileurl = match.Groups[2].Value;

                    fileurl = fileurl.Replace("/mav/", "");

                    if (fileurl == "" || fileurl == "/")
                        fileurl = "index.html";

                    string header = "HTTP/1.1 200 OK\r\n";
                    if (fileurl.Contains(".html"))
                        header += "Content-Type: text/html\r\n\r\n";
                    else if (fileurl.Contains(".js"))
                        header += "Content-Type: application/x-javascript\r\n\r\n";
                    else if (fileurl.Contains(".css"))
                        header += "Content-Type: text/css\r\n\r\n";
                    else
                        header += "Content-Type: text/plain\r\n\r\n";
                    byte[] temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);


                    BinaryReader file = new BinaryReader(File.Open(mavelous_web + fileurl, FileMode.Open, FileAccess.Read, FileShare.Read));
                    byte[] buffer = new byte[1024];
                    while (file.BaseStream.Position < file.BaseStream.Length)
                    {

                        int leng = file.Read(buffer, 0, buffer.Length);

                        stream.Write(buffer, 0, leng);
                    }
                    file.Close();
                    stream.Close();
                }
                else
                {
                    string header = "HTTP/1.1 404 not found\r\nContent-Type: image/jpg\r\n\r\n";
                    byte[] temp = asciiEncoding.GetBytes(header);
                    stream.Write(temp, 0, temp.Length);

                    stream.Close();
                }


            }
            else
            {
                Console.WriteLine(url);
                string header = "HTTP/1.1 404 not found\r\nContent-Type: image/jpg\r\n\r\n";
                byte[] temp = asciiEncoding.GetBytes(header);
                stream.Write(temp, 0, temp.Length);

                stream.Close();
            }

            stream.Close();
        }
        catch (Exception ee)
        {
            log.Error("Failed mjpg ", ee);
            try
            {
                client.Close();
            }
            catch { }
        }
    }
}


        int packetindex = 0;
        //{"master_in": 11110, "mav_loss": 0, "mavpackettype": "META_LINKQUALITY", "master_out": 194, "packet_loss": 0.0}

        public struct META_LINKQUALITY
        {
            public int master_in;
            public int mav_loss;
            public string mavpackettype;
            public int master_out;
            public double packet_loss;
        }

        public struct Messagejson
        {
            public Message2 VFR_HUD;
            public Message2 STATUSTEXT;
            public Message2 SYS_STATUS;
            public Message2 ATTITUDE;
            public Message2 GPS_RAW_INT;
            public Message2 HEARTBEAT;
            public Message2 GPS_STATUS;
            public Message2 NAV_CONTROLLER_OUTPUT;
            public Message2 META_LINKQUALITY;
        }

        public struct Message2
        {
            public object msg;
            public int index;
            public long time_usec;
        }

        private void TOOL_APMFirmware_SelectedIndexChanged(object sender, EventArgs e)
        {
            MainV2.comPort.MAV.cs.firmware = (MainV2.Firmwares)Enum.Parse(typeof(MainV2.Firmwares), _connectionControl.TOOL_APMFirmware.Text);
        }

        private void MainV2_Resize(object sender, EventArgs e)
        {
            log.Info("myview width " + MyView.Width + " height " + MyView.Height);
            log.Info("this   width " + this.Width + " height " + this.Height);
        }

        private void MenuHelp_Click(object sender, EventArgs e)
        {
            MyView.ShowScreen("Help");
        }


        public static void updateCheckMain(ProgressReporterDialogue frmProgressReporter)
        {
            var baseurl = ConfigurationManager.AppSettings["UpdateLocation"];
            try
            {
                bool update = updateCheck(frmProgressReporter, baseurl, "");
                var process = new Process();
                string exePath = Path.GetDirectoryName(Application.ExecutablePath);
                if (MONO)
                {
                    process.StartInfo.FileName = "mono";
                    process.StartInfo.Arguments = " \"" + exePath + Path.DirectorySeparatorChar + "Updater.exe\"" + "  \"" + Application.ExecutablePath + "\"";
                }
                else
                {
                    process.StartInfo.FileName = exePath + Path.DirectorySeparatorChar + "Updater.exe";
                    process.StartInfo.Arguments = Application.ExecutablePath;
                }

                try
                {
                    foreach (string newupdater in Directory.GetFiles(exePath, "Updater.exe*.new"))
                    {
                        File.Copy(newupdater, newupdater.Remove(newupdater.Length - 4), true);
                        File.Delete(newupdater);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Exception during update", ex);
                }
                if (frmProgressReporter != null)
                    frmProgressReporter.UpdateProgressAndStatus(-1, "Starting Updater");
                log.Info("Starting new process: " + process.StartInfo.FileName + " with " + process.StartInfo.Arguments);
                process.Start();
                log.Info("Quitting existing process");
                try
                {
                    // clean close
                    MainV2.instance.BeginInvoke((MethodInvoker)delegate()
                    {
                        MainV2.instance.Close();
                    });
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                log.Error("Update Failed", ex);
                CustomMessageBox.Show("Update Failed " + ex.Message);
            }
        }

        private static void UpdateLabel(Label loadinglabel, string text)
        {
            MainV2.instance.Invoke((MethodInvoker)delegate
            {
                loadinglabel.Text = text;

                Application.DoEvents();
            });
        }

        private static void CheckForUpdate()
        {
            var baseurl = ConfigurationManager.AppSettings["UpdateLocationVersion"];
            string path = Path.GetDirectoryName(Application.ExecutablePath);

            path = path + Path.DirectorySeparatorChar + "version.txt";

            ServicePointManager.ServerCertificateValidationCallback =
new System.Net.Security.RemoteCertificateValidationCallback((sender, certificate, chain, policyErrors) => { return true; });

            log.Debug(path);

            // Create a request using a URL that can receive a post. 
            string requestUriString = baseurl + Path.GetFileName(path);
            log.Debug("Checking for update at: " + requestUriString);
            var webRequest = WebRequest.Create(requestUriString);
            webRequest.Timeout = 5000;

            // Set the Method property of the request to POST.
            webRequest.Method = "GET";

            ((HttpWebRequest)webRequest).IfModifiedSince = File.GetLastWriteTimeUtc(path);

            // Get the response.
            var response = webRequest.GetResponse();
            // Display the status.
            log.Debug("Response status: " + ((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            //dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.

            bool updateFound = false;

            if (File.Exists(path))
            {
                var fi = new FileInfo(path);

                Version LocalVersion = new Version();
                Version WebVersion = new Version();

                if (File.Exists(path))
                {
                    using (Stream fs = File.OpenRead(path))
                    {
                        using (StreamReader sr = new StreamReader(fs))
                        {
                            LocalVersion = new Version(sr.ReadLine());
                            sr.Close();
                        }
                        fs.Close();
                    }
                }

                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    WebVersion = new Version(sr.ReadLine());

                    sr.Close();
                }



                log.Info("New file Check: local " + LocalVersion + " vs Remote " + WebVersion);

                if (LocalVersion < WebVersion)
                {
                    updateFound = true;
                }
            }
            else
            {
                updateFound = true;
                log.Info("File does not exist: Getting " + path);
                // get it
            }

            response.Close();

            if (updateFound)
            {
                var dr = CustomMessageBox.Show("Update Found\n\nDo you wish to update now?", "Update Now", MessageBoxButtons.YesNo);
                if (dr == DialogResult.Yes)
                {
                    DoUpdate();
                }
                else
                {
                    return;
                }
            }
        }

        public static void DoUpdate()
        {
            ProgressReporterDialogue frmProgressReporter = new ProgressReporterDialogue()
            {
                Text = "Check for Updates",
                StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            };

            ThemeManager.ApplyThemeTo(frmProgressReporter);

            frmProgressReporter.DoWork += new Controls.ProgressReporterDialogue.DoWorkEventHandler(DoUpdateWorker_DoWork);

            frmProgressReporter.UpdateProgressAndStatus(-1, "Checking for Updates");

            frmProgressReporter.RunBackgroundOperationAsync();
        }

        static void DoUpdateWorker_DoWork(object sender, Controls.ProgressWorkerEventArgs e)
        {
            // TODO: Is this the right place?
            #region Fetch Parameter Meta Data

            var progressReporterDialogue = ((ProgressReporterDialogue)sender);
            progressReporterDialogue.UpdateProgressAndStatus(-1, "Getting Updated Parameters");

            try
            {
                
               ParameterMetaDataParser.GetParameterInformation();
            }
            catch (Exception ex) { log.Error(ex.ToString()); CustomMessageBox.Show("Error getting Parameter Information"); }

            #endregion Fetch Parameter Meta Data

            progressReporterDialogue.UpdateProgressAndStatus(-1, "Getting Base URL");
            // check for updates
            if (Debugger.IsAttached)
            {
                log.Info("Skipping update test as it appears we are debugging");
            }
            else
            {
                MainV2.updateCheckMain(progressReporterDialogue);
            }
        }

        private static bool updateCheck(ProgressReporterDialogue frmProgressReporter, string baseurl, string subdir)
        {
            bool update = false;
            List<string> files = new List<string>();

            // Create a request using a URL that can receive a post. 
            log.Info(baseurl);
            WebRequest request = WebRequest.Create(baseurl);
            request.Timeout = 10000;
            // Set the Method property of the request to POST.
            request.Method = "GET";
            // Get the request stream.
            Stream dataStream; //= request.GetRequestStream();
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            log.Info(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();
            // Display the content.
            Regex regex = new Regex("href=\"([^\"]+)\"", RegexOptions.IgnoreCase);

            Uri baseuri = new Uri(baseurl, UriKind.Absolute);

            if (regex.IsMatch(responseFromServer))
            {
                MatchCollection matchs = regex.Matches(responseFromServer);
                for (int i = 0; i < matchs.Count; i++)
                {
                    if (matchs[i].Groups[1].Value.ToString().Contains(".."))
                        continue;
                    if (matchs[i].Groups[1].Value.ToString().Contains("http"))
                        continue;
                    // dirs
                    if (matchs[i].Groups[1].Value.ToString().Contains("tree/master/"))
                    {
                        string url = System.Web.HttpUtility.UrlDecode(matchs[i].Groups[1].Value.ToString()) + "/";
                        Uri newuri = new Uri(baseuri, url);
                        files.Add(baseuri.MakeRelativeUri(newuri).ToString());

                    }
                    // files
                    if (matchs[i].Groups[1].Value.ToString().Contains("blob/master/"))
                    {
                        string url = System.Web.HttpUtility.UrlDecode(matchs[i].Groups[1].Value.ToString());
                        Uri newuri = new Uri(baseuri, url);
                        files.Add(System.Web.HttpUtility.UrlDecode(newuri.Segments[newuri.Segments.Length - 1]));
                    }
                }
            }

            //Console.WriteLine(responseFromServer);
            // Clean up the streams.
            reader.Close();
            dataStream.Close();
            response.Close();

            string dir = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + subdir;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            foreach (string file in files)
            {
                if (frmProgressReporter.doWorkArgs.CancelRequested)
                {
                    frmProgressReporter.doWorkArgs.CancelAcknowledged = true;
                    throw new Exception("Cancel");
                }


                if (file.Equals("/") || file.Equals("") || file.StartsWith("../"))
                {
                    continue;
                }
                if (file.EndsWith("/"))
                {
                    update = updateCheck(frmProgressReporter, baseurl + file, subdir.Replace('/', Path.DirectorySeparatorChar) + file) && update;
                    continue;
                }
                if (frmProgressReporter != null)
                    frmProgressReporter.UpdateProgressAndStatus(-1, "Checking " + file);

                string path = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + subdir + file;

                baseurl = baseurl.Replace("//github.com", "//raw.github.com");
                baseurl = baseurl.Replace("/tree/", "/");

                // Create a request using a URL that can receive a post. 
                request = WebRequest.Create(baseurl + file);
                log.Info(baseurl + file + " ");
                // Set the Method property of the request to POST.
                request.Method = "GET";

                ((HttpWebRequest)request).AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                request.Headers.Add("Accept-Encoding", "gzip,deflate");

                // Get the response.
                response = request.GetResponse();
                // Display the status.
                log.Info(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.

                bool updateThisFile = false;

                if (File.Exists(path))
                {
                    FileInfo fi = new FileInfo(path);

                    //log.Info(response.Headers[HttpResponseHeader.ETag]);
                    string CurrentEtag = "";

                    if (File.Exists(path + ".etag"))
                    {
                        using (Stream fs = File.OpenRead(path + ".etag"))
                        {
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                CurrentEtag = sr.ReadLine();
                                sr.Close();
                            }
                            fs.Close();
                        }
                    }

                    log.Debug("New file Check: " + fi.Length + " vs " + response.ContentLength + " " + response.Headers[HttpResponseHeader.ETag] + " vs " + CurrentEtag);

                    if (fi.Length != response.ContentLength || response.Headers[HttpResponseHeader.ETag] != CurrentEtag)
                    {
                        using (StreamWriter sw = new StreamWriter(path + ".etag.new"))
                        {
                            sw.WriteLine(response.Headers[HttpResponseHeader.ETag]);
                            sw.Close();
                        }
                        updateThisFile = true;
                        log.Info("NEW FILE " + file);
                    }
                }
                else
                {
                    updateThisFile = true;
                    log.Info("NEW FILE " + file);
                    using (StreamWriter sw = new StreamWriter(path + ".etag.new"))
                    {
                        sw.WriteLine(response.Headers[HttpResponseHeader.ETag]);
                        sw.Close();
                    }
                    // get it
                }

                if (updateThisFile)
                {
                    if (!update)
                    {
                        //DialogResult dr = MessageBox.Show("Update Found\n\nDo you wish to update now?", "Update Now", MessageBoxButtons.YesNo);
                        //if (dr == DialogResult.Yes)
                        {
                            update = true;
                        }
                        //else
                        {
                            //    return;
                        }
                    }
                    if (frmProgressReporter != null)
                        frmProgressReporter.UpdateProgressAndStatus(-1, "Getting " + file);

                    // from head
                    long bytes = response.ContentLength;

                    long contlen = bytes;

                    byte[] buf1 = new byte[1024];

                    using (FileStream fs = new FileStream(path + ".new", FileMode.Create))
                    {

                        DateTime dt = DateTime.Now;

                        //dataStream.ReadTimeout = 30000;

                        while (dataStream.CanRead)
                        {
                            try
                            {
                                if (dt.Second != DateTime.Now.Second)
                                {
                                    if (frmProgressReporter != null)
                                        frmProgressReporter.UpdateProgressAndStatus((int)(((double)(contlen - bytes) / (double)contlen) * 100), "Getting " + file + ": " + (((double)(contlen - bytes) / (double)contlen) * 100).ToString("0.0") + "%"); //+ Math.Abs(bytes) + " bytes");
                                    dt = DateTime.Now;
                                }
                            }
                            catch { }
                            //    log.Debug(file + " " + bytes);
                            int len = dataStream.Read(buf1, 0, 4096);
                            if (len == 0)
                                break;
                            bytes -= len;
                            fs.Write(buf1, 0, len);
                        }
                        fs.Close();
                    }
                }


                reader.Close();
                //dataStream.Close();
                response.Close();
            }

            //P.StartInfo.CreateNoWindow = true;
            //P.StartInfo.RedirectStandardOutput = true;
            return update;


        }


        /// <summary>
        /// trying to replicate google code etags....... this doesnt work.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="modifyDate"></param>
        /// <returns></returns>
        private string GetFileETag(string fileName, DateTime modifyDate)
        {

            string FileString;

            System.Text.Encoder StringEncoder;

            byte[] StringBytes;

            MD5CryptoServiceProvider MD5Enc;

            //use file name and modify date as the unique identifier

            FileString = fileName + modifyDate.ToString("d", CultureInfo.InvariantCulture);

            //get string bytes

            StringEncoder = Encoding.UTF8.GetEncoder();

            StringBytes = new byte[StringEncoder.GetByteCount(FileString.ToCharArray(), 0, FileString.Length, true)];

            StringEncoder.GetBytes(FileString.ToCharArray(), 0, FileString.Length, StringBytes, 0, true);

            //hash string using MD5 and return the hex-encoded hash

            MD5Enc = new MD5CryptoServiceProvider();

            byte[] hash = MD5Enc.ComputeHash((Stream)File.OpenRead(fileName));

            return "\"" + BitConverter.ToString(hash).Replace("-", string.Empty) + "\"";

        }

        /// <summary>
        /// keyboard shortcuts override
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F12)
            {
                MenuConnect_Click(null, null);
                return true;
            }

            if (keyData == Keys.F1)
            {
                MenuProjectSettings_Click(null, null);
                return true;
            }

            if (keyData == Keys.F2)
            {
                MenuFlightPlanner_Click(null, null);
                return true;
            }

            if (keyData == Keys.F4)
            {
                MenuFlightData_Click(null, null);
                return true;
            }

            if (AdvButnChkBox.Checked)      // Only enable shortcuts to Advanced button functions if they are visible
            {
                if (keyData == Keys.F3)
                {
                    MenuPreFlight_Click(null, null);
                    return true;
                }

                //if (keyData == Keys.F5)
                //{
                //    MenuSimulation_Click(null, null);
                //    return true;
                //}

                if (keyData == Keys.F6)
                {
                    MenuFlightRecorder_Click(null, null);
                    return true;
                }

                if (keyData == Keys.F7)
                {
                    MenuConfiguration_Click(null, null);
                    return true;
                }
            }

            if (keyData == Keys.F10)
            {
                comPort.getParamList();
                MyView.ShowScreen(MyView.current.Name);
                return true;
            }

            if (keyData == (Keys.Control | Keys.F)) // temp
            {
                Form frm = new temp();
                ThemeManager.ApplyThemeTo(frm);
                frm.Show();
                return true;
            }
            /*if (keyData == (Keys.Control | Keys.S)) // screenshot
            {
                ScreenShot();
                return true;
            }*/
            if (keyData == (Keys.Control | Keys.G)) // nmea out
            {
                Form frm = new SerialOutputNMEA();
                ThemeManager.ApplyThemeTo(frm);
                frm.Show();
                return true;
            }
            if (keyData == (Keys.Control | Keys.L)) // limits
            {
                Form temp = new Form();
                Control frm = new GCSViews.ConfigurationView.ConfigAP_Limits();
                temp.Controls.Add(frm);
                temp.Size = frm.Size;
                frm.Dock = DockStyle.Fill;
                ThemeManager.ApplyThemeTo(temp);
                temp.Show();
                return true;
            }
            if (keyData == (Keys.Control | Keys.W)) // test ac config
            {

                Controls.ConfigPanel cfg = new Controls.ConfigPanel(Application.StartupPath + System.IO.Path.DirectorySeparatorChar + "ArduCopterConfig.xml");

                //cfg.Show();

                return true;
            }
            if (keyData == (Keys.Control | Keys.T)) // for override connect
            {
                try
                {
                    MainV2.comPort.Open(false);
                }
                catch (Exception ex) { CustomMessageBox.Show(ex.ToString()); }
                return true;
            }
            if (keyData == (Keys.Control | Keys.Y)) // for ryan beall
            {
#if MAVLINK10
                // write
                MainV2.comPort.doCommand(MAVLink.MAV_CMD.PREFLIGHT_STORAGE, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
                //read
                ///////MainV2.comPort.doCommand(MAVLink09.MAV_CMD.PREFLIGHT_STORAGE, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
#else
                MainV2.comPort.doAction(MAVLink.MAV_ACTION.MAV_ACTION_STORAGE_WRITE);
#endif
                CustomMessageBox.Show("Done MAV_ACTION_STORAGE_WRITE");
                return true;
            }
            if (keyData == (Keys.Control | Keys.J)) // for jani
            {
                string data = "!!";
                Common.InputBox("inject", "enter data to be written", ref data);
                MainV2.comPort.Write(data + "\r");
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void changelanguage(CultureInfo ci)
        {
            if (ci != null && !Thread.CurrentThread.CurrentUICulture.Equals(ci))
            {
                Thread.CurrentThread.CurrentUICulture = ci;
                config["language"] = ci.Name;
                //System.Threading.Thread.CurrentThread.CurrentCulture = ci;

                HashSet<Control> views = new HashSet<Control> { FlightData, FlightPlanner, Simulation };

                foreach (Control view in MyView.Controls)
                    views.Add(view);

                foreach (Control view in views)
                {
                    if (view != null)
                    {
                        ComponentResourceManager rm = new ComponentResourceManager(view.GetType());
                        foreach (Control ctrl in view.Controls)
                            rm.ApplyResource(ctrl);
                        rm.ApplyResources(view, "$this");
                    }
                }
            }
        }

        private void MainV2_FormClosing(object sender, FormClosingEventArgs e)
        {
            config["MainHeight"] = this.Height;
            config["MainWidth"] = this.Width;
            config["MainMaximised"] = this.WindowState.ToString();

            config["MainLocX"] = this.Location.X.ToString();
            config["MainLocY"] = this.Location.Y.ToString();

            try
            {
                comPort.logreadmode = false;
                if (comPort.logfile != null)
                    comPort.logfile.Close();

                if (comPort.rawlogfile != null)
                    comPort.rawlogfile.Close();

                comPort.logfile = null;
                comPort.rawlogfile = null;
            }
            catch { }

        }

        public static string getConfig(string paramname)
        {
            if (config[paramname] != null)
                return config[paramname].ToString();
            return "";
        }

        public void ChangeUnits()
        {
            try
            {
                // dist
                if (MainV2.config["distunits"] != null)
                {
                    switch ((Common.distances)Enum.Parse(typeof(Common.distances), MainV2.config["distunits"].ToString()))
                    {
                        case Common.distances.Meters:
                            MainV2.comPort.MAV.cs.multiplierdist = 1;
                            MainV2.comPort.MAV.cs.DistanceUnit = "Meters";
                            break;
                        case Common.distances.Feet:
                            MainV2.comPort.MAV.cs.multiplierdist = 3.2808399f;
                            MainV2.comPort.MAV.cs.DistanceUnit = "Feet";
                            break;
                    }
                }
                else
                {
                    MainV2.comPort.MAV.cs.multiplierdist = 1;
                    MainV2.comPort.MAV.cs.DistanceUnit = "Meters";
                }

                // speed
                if (MainV2.config["speedunits"] != null)
                {
                    switch ((Common.speeds)Enum.Parse(typeof(Common.speeds), MainV2.config["speedunits"].ToString()))
                    {
                        case Common.speeds.ms:
                            MainV2.comPort.MAV.cs.multiplierspeed = 1;
                            MainV2.comPort.MAV.cs.SpeedUnit = "m/s";
                            break;
                        case Common.speeds.fps:
                            MainV2.comPort.MAV.cs.multiplierdist = 3.2808399f;
                            MainV2.comPort.MAV.cs.SpeedUnit = "fps";
                            break;
                        case Common.speeds.kph:
                            MainV2.comPort.MAV.cs.multiplierspeed = 3.6f;
                            MainV2.comPort.MAV.cs.SpeedUnit = "kph";
                            break;
                        case Common.speeds.mph:
                            MainV2.comPort.MAV.cs.multiplierspeed = 2.23693629f;
                            MainV2.comPort.MAV.cs.SpeedUnit = "mph";
                            break;
                        case Common.speeds.knots:
                            MainV2.comPort.MAV.cs.multiplierspeed = 1.94384449f;
                            MainV2.comPort.MAV.cs.SpeedUnit = "knots";
                            break;
                    }
                }
                else
                {
                    MainV2.comPort.MAV.cs.multiplierspeed = 1;
                    MainV2.comPort.MAV.cs.SpeedUnit = "m/s";
                }
            }
            catch { }

        }

        private void CMB_baudrate_TextChanged(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            int baud = 0;
            for (int i = 0; i < _connectionControl.CMB_baudrate.Text.Length; i++)
                if (char.IsDigit(_connectionControl.CMB_baudrate.Text[i]))
                {
                    sb.Append(_connectionControl.CMB_baudrate.Text[i]);
                    baud = baud * 10 + _connectionControl.CMB_baudrate.Text[i] - '0';
                }
            if (_connectionControl.CMB_baudrate.Text != sb.ToString())
            {
                _connectionControl.CMB_baudrate.Text = sb.ToString();
            }
            try
            {
                if (baud > 0 && comPort.BaseStream.BaudRate != baud)
                    comPort.BaseStream.BaudRate = baud;
            }
            catch (Exception)
            {
            }
        }

        private void CMB_serialport_Enter(object sender, EventArgs e)
        {
            CMB_serialport_Click(sender, e);
        }

        private void MainMenu_MouseLeave(object sender, EventArgs e)
        {
            if (_connectionControl.PointToClient(Control.MousePosition).Y < MainMenu.Height)
                return;            

            this.SuspendLayout();

            panel1.Visible = false;

            this.ResumeLayout();
        }

        void menu_MouseEnter(object sender, EventArgs e)
        {
            this.SuspendLayout();
            panel1.Location = new Point(0,0);
            panel1.Width = menu.Width;
            panel1.BringToFront();
            panel1.Visible = true;
            this.ResumeLayout();
        }

        private void autoHideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AutoHideMenu(autoHideToolStripMenuItem.Checked);

            config["menu_autohide"] = autoHideToolStripMenuItem.Checked.ToString();
        }

        void AutoHideMenu(bool hide)
        {
            autoHideToolStripMenuItem.Checked = hide;

            if (!hide)
            {
                this.SuspendLayout();
                panel1.Dock = DockStyle.Top;
                panel1.SendToBack();
                panel1.Visible = true;
                menu.Visible = false;
                MainMenu.MouseLeave -= MainMenu_MouseLeave;
                panel1.MouseLeave -= MainMenu_MouseLeave;
                toolStripConnectionControl.MouseLeave -= MainMenu_MouseLeave;
                this.ResumeLayout();
            }
            else
            {
                this.SuspendLayout();
                panel1.Dock = DockStyle.None;
                panel1.Visible = false;
                MainMenu.MouseLeave += MainMenu_MouseLeave;
                panel1.MouseLeave += MainMenu_MouseLeave;
                toolStripConnectionControl.MouseLeave += MainMenu_MouseLeave;
                menu.Visible = true;
                menu.SendToBack();
                this.ResumeLayout();
            }
        }

        private void MainV2_KeyDown(object sender, KeyEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }

        //private void toolStripMenuItem1_Click(object sender, EventArgs e)
        //{
        //   System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=mich146%40hotmail%2ecom&lc=AU&item_name=Michael%20Oborne&no_note=0&currency_code=AUD&bn=PP%2dDonationsBF%3abtn_donate_SM%2egif%3aNonHostedGuest");
        //}

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_DEVICECHANGE:
                    // The WParam value identifies what is occurring.
                    WM_DEVICECHANGE_enum n = (WM_DEVICECHANGE_enum)m.WParam;
                     int l = (int)m.LParam;
                     if (n == WM_DEVICECHANGE_enum.DBT_DEVNODES_CHANGED)
                     {

                     }
                     if (n == WM_DEVICECHANGE_enum.DBT_DEVICEARRIVAL)
                     {
                         string port = Marshal.PtrToStringAuto((IntPtr)((long)m.LParam + 12));
                         Console.WriteLine("Added port {0}",port);
                     }
                     Console.WriteLine("Device Change {0} {1} {2}", m.Msg, (WM_DEVICECHANGE_enum)m.WParam, m.LParam);
                    break;
            }
            base.WndProc(ref m);
        }

        const int WM_DEVICECHANGE = 0x219;

        public enum WM_DEVICECHANGE_enum
        {
            DBT_CONFIGCHANGECANCELED = 0x19,
            DBT_CONFIGCHANGED = 0x18,
            DBT_CUSTOMEVENT = 0x8006,
            DBT_DEVICEARRIVAL = 0x8000,
            DBT_DEVICEQUERYREMOVE = 0x8001,
            DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
            DBT_DEVICEREMOVECOMPLETE = 0x8004,
            DBT_DEVICEREMOVEPENDING = 0x8003,
            DBT_DEVICETYPESPECIFIC = 0x8005,
            DBT_DEVNODES_CHANGED = 0x7,
            DBT_QUERYCHANGECONFIG = 0x17,
            DBT_USERDEFINED = 0xFFFF,
        }

        private void AdvButnChkBox_CheckedChanged(object sender, EventArgs e)
        {
            MenuConfiguration.Visible = AdvButnChkBox.Checked;
            MenuFlightRecorder.Visible= AdvButnChkBox.Checked;
            MenuSimulation.Visible    = AdvButnChkBox.Checked;
            MenuPreFlight.Visible = AdvButnChkBox.Checked;
        }

    }
}