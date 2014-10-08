using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using ArdupilotMega.Controls.BackstageView;
using ArdupilotMega.Controls;
using ArdupilotMega.Utilities;
using GMap.NET;
//using GMap.NET.WindowsForms;
//using GMap.NET.WindowsForms.Markers;
using System.Threading;
using System.IO;
using System.Xml;
using System.Collections;


namespace ArdupilotMega.GCSViews	//.ConfigurationView
{
    public partial class ConfigProject : MyUserControl, IActivate
    {
        private bool startup = false;
        //private List<CultureInfo> _languages;

        const float rad2deg = (float)(180 / Math.PI);
        const float deg2rad = (float)(1.0 / rad2deg);

        public static ProjectCalculator camera;               // Make a public camera for access by other forms

        public class GCSBitmapInfo
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public long Fps { get; set; }
            public string Standard { get; set; }
            public AMMediaType Media { get; set; }

            public GCSBitmapInfo(int width, int height, long fps, string standard, AMMediaType media)
            {
                Width = width;
                Height = height;
                Fps = fps;
                Standard = standard;
                Media = media;
            }

            public override string ToString()
            {
                return Width.ToString() + " x " + Height.ToString() + String.Format(" {0:0.00} fps ", 10000000.0 / Fps) + Standard;
            }
        }

        Dictionary<string, staffinfo> personnel = new Dictionary<string, staffinfo>();

        public struct staffinfo
        {
            public string name;
            public string title;
        }


        public ConfigProject()
        {
            InitializeComponent();

            camera = new ProjectCalculator();
            //Camera cameraForm = new Camera();
        }


        //private void BUT_videostart_Click(object sender, EventArgs e)
        //{
        //    if (MainV2.MONO)
        //        return;

        //    // stop first
        //    BUT_videostop_Click(sender, e);

        //    var bmp = (GCSBitmapInfo)CMB_videoresolutions.SelectedItem;

        //    try
        //    {
        //        MainV2.cam = new WebCamService.Capture(CMB_videosources.SelectedIndex, bmp.Media);

        //        MainV2.cam.Start();

        //        MainV2.config["video_options"] = CMB_videoresolutions.SelectedIndex;

        //        BUT_videostart.Enabled = false;
        //    }
        //    catch (Exception ex) { CustomMessageBox.Show("Camera Fail: " + ex.Message); }

        //}

        //private void BUT_videostop_Click(object sender, EventArgs e)
        //{
        //    BUT_videostart.Enabled = true;
        //    if (MainV2.cam != null)
        //    {
        //        MainV2.cam.Dispose();
        //        MainV2.cam = null;
        //    }
        //}

        //private void CMB_videosources_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (MainV2.MONO)
        //        return;

        //    // the reason why i dont populate this list is because on linux/mac this call will fail.
        //    WebCamService.Capture capt = new WebCamService.Capture();

        //    List<string> devices = WebCamService.Capture.getDevices();

        //    CMB_videosources.DataSource = devices;

        //    capt.Dispose();
        //}

        //private void CMB_videosources_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (MainV2.MONO)
        //        return;

        //    int hr;
        //    int count;
        //    int size;
        //    object o;
        //    IBaseFilter capFilter = null;
        //    ICaptureGraphBuilder2 capGraph = null;
        //    AMMediaType media = null;
        //    VideoInfoHeader v;
        //    VideoStreamConfigCaps c;
        //    List<GCSBitmapInfo> modes = new List<GCSBitmapInfo>();

        //    // Get the ICaptureGraphBuilder2
        //    capGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();
        //    IFilterGraph2 m_FilterGraph = (IFilterGraph2)new FilterGraph();

        //    DsDevice[] capDevices;
        //    capDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

        //    // Add the video device
        //    hr = m_FilterGraph.AddSourceFilterForMoniker(capDevices[CMB_videosources.SelectedIndex].Mon, null, "Video input", out capFilter);
        //    try
        //    {
        //        DsError.ThrowExceptionForHR(hr);
        //    }
        //    catch (Exception ex)
        //    {
        //        CustomMessageBox.Show("Can not add video source\n" + ex.ToString());
        //        return;
        //    }

        //    // Find the stream config interface
        //    hr = capGraph.FindInterface(PinCategory.Capture, MediaType.Video, capFilter, typeof(IAMStreamConfig).GUID, out o);
        //    DsError.ThrowExceptionForHR(hr);

        //    IAMStreamConfig videoStreamConfig = o as IAMStreamConfig;
        //    if (videoStreamConfig == null)
        //    {
        //        throw new Exception("Failed to get IAMStreamConfig");
        //    }

        //    hr = videoStreamConfig.GetNumberOfCapabilities(out count, out size);
        //    DsError.ThrowExceptionForHR(hr);
        //    IntPtr TaskMemPointer = Marshal.AllocCoTaskMem(size);
        //    for (int i = 0; i < count; i++)
        //    {
        //        IntPtr ptr = IntPtr.Zero;

        //        hr = videoStreamConfig.GetStreamCaps(i, out media, TaskMemPointer);
        //        v = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
        //        c = (VideoStreamConfigCaps)Marshal.PtrToStructure(TaskMemPointer, typeof(VideoStreamConfigCaps));
        //        modes.Add(new GCSBitmapInfo(v.BmiHeader.Width, v.BmiHeader.Height, c.MaxFrameInterval, c.VideoStandard.ToString(), media));
        //    }
        //    Marshal.FreeCoTaskMem(TaskMemPointer);
        //    DsUtils.FreeAMMediaType(media);

        //    CMB_videoresolutions.DataSource = modes;

        //    if (MainV2.getConfig("video_options") != "" && CMB_videosources.Text != "")
        //    {
        //        try
        //        {
        //            CMB_videoresolutions.SelectedIndex = int.Parse(MainV2.getConfig("video_options"));
        //        }
        //        catch { } // ignore bad entries
        //    }
        //}

        //private void CHK_hudshow_CheckedChanged(object sender, EventArgs e)
        //{
        //    GCSViews.FlightData.myhud.hudon = CHK_hudshow.Checked;
        //}

        //private void CHK_enablespeech_CheckedChanged(object sender, EventArgs e)
        //{
        //    MainV2.speechEnable = CHK_enablespeech.Checked;
        //    MainV2.config["speechenable"] = CHK_enablespeech.Checked;
        //    if (MainV2.speechEngine != null)
        //        MainV2.speechEngine.SpeakAsyncCancelAll();

        //    if (CHK_enablespeech.Checked)
        //    {
        //        CHK_speechwaypoint.Visible = true;
        //        CHK_speechaltwarning.Visible = true;
        //        CHK_speechbattery.Visible = true;
        //        CHK_speechcustom.Visible = true;
        //        CHK_speechmode.Visible = true;
        //        CHK_speecharmdisarm.Visible = true;
        //    }
        //    else
        //    {
        //        CHK_speechwaypoint.Visible = false;
        //        CHK_speechaltwarning.Visible = false;
        //        CHK_speechbattery.Visible = false;
        //        CHK_speechcustom.Visible = false;
        //        CHK_speechmode.Visible = false;
        //        CHK_speecharmdisarm.Visible = false;
        //    }
        //}

//        private void CMB_language_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            if (startup)
//                return;
//            MainV2.instance.changelanguage((CultureInfo)CMB_language.SelectedItem);

//#if !DEBUG
//                MessageBox.Show("Please Restart the Planner");

//                Application.Exit();
//#endif
//        }

        //private void CMB_osdcolor_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    if (CMB_osdcolor.Text != "")
        //    {
        //        MainV2.config["hudcolor"] = CMB_osdcolor.Text;
        //        GCSViews.FlightData.myhud.hudcolor = Color.FromKnownColor((KnownColor)Enum.Parse(typeof(KnownColor), CMB_osdcolor.Text));
        //    }
        //}

        //private void CHK_speechwaypoint_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config["speechwaypointenabled"] = ((CheckBox)sender).Checked.ToString();

        //    if (((CheckBox)sender).Checked)
        //    {
        //        string speechstring = "Heading to Waypoint {wpn}";
        //        if (MainV2.config["speechwaypoint"] != null)
        //            speechstring = MainV2.config["speechwaypoint"].ToString();
        //        Common.InputBox("Notification", "What do you want it to say?", ref speechstring);
        //        MainV2.config["speechwaypoint"] = speechstring;
        //    }
        //}

        //private void CHK_speechmode_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config["speechmodeenabled"] = ((CheckBox)sender).Checked.ToString();

        //    if (((CheckBox)sender).Checked)
        //    {
        //        string speechstring = "Mode changed to {mode}";
        //        if (MainV2.config["speechmode"] != null)
        //            speechstring = MainV2.config["speechmode"].ToString();
        //        Common.InputBox("Notification", "What do you want it to say?", ref speechstring);
        //        MainV2.config["speechmode"] = speechstring;
        //    }
        //}

        //private void CHK_speechcustom_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config["speechcustomenabled"] = ((CheckBox)sender).Checked.ToString();

        //    if (((CheckBox)sender).Checked)
        //    {
        //        string speechstring = "Heading to Waypoint {wpn}, altitude is {alt}, Ground speed is {gsp} ";
        //        if (MainV2.config["speechcustom"] != null)
        //            speechstring = MainV2.config["speechcustom"].ToString();
        //        Common.InputBox("Notification", "What do you want it to say?", ref speechstring);
        //        MainV2.config["speechcustom"] = speechstring;
        //    }
        //}

        //private void BUT_rerequestparams_Click(object sender, EventArgs e)
        //{
        //    if (!MainV2.comPort.BaseStream.IsOpen)
        //        return;
        //    ((MyButton)sender).Enabled = false;
        //    try
        //    {

        //        MainV2.comPort.getParamList();


        //    }
        //    catch { CustomMessageBox.Show("Error: getting param list"); }


        //    ((MyButton)sender).Enabled = true;
        //    startup = true;

            

        //    startup = false;
        //}

        //private void CHK_speechbattery_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config["speechbatteryenabled"] = ((CheckBox)sender).Checked.ToString();

        //    if (((CheckBox)sender).Checked)
        //    {
        //        string speechstring = "WARNING, Battery at {batv} Volt, {batp} percent";
        //        if (MainV2.config["speechbattery"] != null)
        //            speechstring = MainV2.config["speechbattery"].ToString();
        //        Common.InputBox("Notification", "What do you want it to say?", ref speechstring);
        //        MainV2.config["speechbattery"] = speechstring;

        //        speechstring = "9.6";
        //        if (MainV2.config["speechbatteryvolt"] != null)
        //            speechstring = MainV2.config["speechbatteryvolt"].ToString();
        //        Common.InputBox("Battery Level", "What Voltage do you want to warn at?", ref speechstring);
        //        MainV2.config["speechbatteryvolt"] = speechstring;

        //        speechstring = "20";
        //        if (MainV2.config["speechbatterypercent"] != null)
        //            speechstring = MainV2.config["speechbatterypercent"].ToString();
        //        Common.InputBox("Battery Level", "What percentage do you want to warn at?", ref speechstring);
        //        MainV2.config["speechbatterypercent"] = speechstring;
        //    }
        //}

        //private void BUT_Joystick_Click(object sender, EventArgs e)
        //{
        //    Form joy = new JoystickSetup();
        //    ThemeManager.ApplyThemeTo(joy);
        //    joy.Show();
        //}

        private void CMB_distunits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startup)
                return;
            MainV2.config["distunits"] = CMB_distunits.Text;
            MainV2.instance.ChangeUnits();
        }

        private void CMB_speedunits_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (startup)
                return;
            MainV2.config["speedunits"] = CMB_speedunits.Text;
            MainV2.instance.ChangeUnits();
        }

        //private void CMB_rateattitude_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config[((ComboBox)sender).Name] = ((ComboBox)sender).Text;
        //    MainV2.comPort.MAV.cs.rateattitude = byte.Parse(((ComboBox)sender).Text);

        //    MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTRA1, MainV2.comPort.MAV.cs.rateattitude); // request attitude
        //    MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTRA2, MainV2.comPort.MAV.cs.rateattitude); // request vfr
        //}

        //private void CMB_rateposition_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config[((ComboBox)sender).Name] = ((ComboBox)sender).Text;
        //    MainV2.comPort.MAV.cs.rateposition = byte.Parse(((ComboBox)sender).Text);

        //    MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.POSITION, MainV2.comPort.MAV.cs.rateposition); // request gps
        //}

        //private void CMB_ratestatus_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config[((ComboBox)sender).Name] = ((ComboBox)sender).Text;
        //    MainV2.comPort.MAV.cs.ratestatus = byte.Parse(((ComboBox)sender).Text);

        //    MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTENDED_STATUS, MainV2.comPort.MAV.cs.ratestatus); // mode
        //}

        //private void CMB_raterc_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config[((ComboBox)sender).Name] = ((ComboBox)sender).Text;
        //    MainV2.comPort.MAV.cs.raterc = byte.Parse(((ComboBox)sender).Text);

        //    MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.RC_CHANNELS, MainV2.comPort.MAV.cs.raterc); // request rc info 
        //}

        //private void CMB_ratesensors_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config[((ComboBox)sender).Name] = ((ComboBox)sender).Text;
        //    MainV2.comPort.MAV.cs.ratesensors = byte.Parse(((ComboBox)sender).Text);

        //    MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTRA3, MainV2.comPort.MAV.cs.ratesensors); // request extra stuff - tridge
        //    MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.RAW_SENSORS, MainV2.comPort.MAV.cs.ratesensors); // request raw sensor
        //}

        //private void CHK_mavdebug_CheckedChanged(object sender, EventArgs e)
        //{
        //    MainV2.comPort.debugmavlink = CHK_mavdebug.Checked;
        //}

        //private void CHK_resetapmonconnect_CheckedChanged(object sender, EventArgs e)
        //{
        //    MainV2.config[((CheckBox)sender).Name] = ((CheckBox)sender).Checked.ToString();
        //}

        //private void CHK_speechaltwarning_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config["speechaltenabled"] = ((CheckBox)sender).Checked.ToString();

        //    if (((CheckBox)sender).Checked)
        //    {
        //        string speechstring = "WARNING, low altitude {alt}";
        //        if (MainV2.config["speechalt"] != null)
        //            speechstring = MainV2.config["speechalt"].ToString();
        //        Common.InputBox("Notification", "What do you want it to say?", ref speechstring);
        //        MainV2.config["speechalt"] = speechstring;

        //        speechstring = "2";
        //        if (MainV2.config["speechaltheight"] != null)
        //            speechstring = MainV2.config["speechaltheight"].ToString();
        //        Common.InputBox("Min Alt", "What altitude do you want to warn at? (relative to home)", ref speechstring);
        //        MainV2.config["speechaltheight"] = (double.Parse(speechstring) / MainV2.comPort.MAV.cs.multiplierdist).ToString(); // save as m

        //    }
        //}

        //private void NUM_tracklength_ValueChanged(object sender, EventArgs e)
        //{
        //    MainV2.config["NUM_tracklength"] = NUM_tracklength.Value.ToString();

        //}

        //private void CHK_loadwponconnect_CheckedChanged(object sender, EventArgs e)
        //{
        //    MainV2.config["loadwpsonconnect"] = CHK_loadwponconnect.Checked.ToString();
        //}

        //private void CHK_GDIPlus_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    CustomMessageBox.Show("You need to restart the planner for this to take effect");
        //    MainV2.config["CHK_GDIPlus"] = CHK_GDIPlus.Checked.ToString();
        //}

        // This load handler now only contains code that should execute once
        // on start up. See Activate() for the remainder
        private void ConfigProject_Load(object sender, EventArgs e)
        {
            startup = true;
            Personnel_Load();   // 23/10/2013 DWR untested
            startup = false;
        }

        //private void CMB_osdcolor_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    if (e.Index < 0)
        //        return;

        //    Graphics g = e.Graphics;
        //    Rectangle rect = e.Bounds;
        //    Brush brush = null;

        //    if ((e.State & DrawItemState.Selected) == 0)
        //        brush = new SolidBrush(CMB_osdcolor.BackColor);
        //    else
        //        brush = SystemBrushes.Highlight;

        //    g.FillRectangle(brush, rect);

        //    brush = new SolidBrush(Color.FromName((string)CMB_osdcolor.Items[e.Index]));

        //    g.FillRectangle(brush, rect.X + 2, rect.Y + 2, 30, rect.Height - 4);
        //    g.DrawRectangle(Pens.Black, rect.X + 2, rect.Y + 2, 30, rect.Height - 4);

        //    if ((e.State & DrawItemState.Selected) == 0)
        //        brush = new SolidBrush(CMB_osdcolor.ForeColor);
        //    else
        //        brush = SystemBrushes.HighlightText;
        //    g.DrawString(CMB_osdcolor.Items[e.Index].ToString(),
        //        CMB_osdcolor.Font, brush, rect.X + 35, rect.Top + rect.Height - CMB_osdcolor.Font.Height);
        //}

        //private void CMB_videosources_Click(object sender, EventArgs e)
        //{
        //    if (MainV2.MONO)
        //        return;
        //    // the reason why i dont populate this list is because on linux/mac this call will fail.
        //    WebCamService.Capture capt = new WebCamService.Capture();

        //    List<string> devices = WebCamService.Capture.getDevices();

        //    CMB_videosources.DataSource = devices;

        //    capt.Dispose();
        //}

        //private void CHK_maprotation_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config["CHK_maprotation"] = CHK_maprotation.Checked.ToString();
        //}

        // Called every time that this control is made current in the backstage view
        public void Activate()
        {
            startup = true; // flag to ignore changes while we programatically populate controls


            //CMB_osdcolor.DataSource = Enum.GetNames(typeof(KnownColor));

            // set distance/speed unit states
            CMB_distunits.DataSource = Enum.GetNames(typeof(Common.distances));
            CMB_speedunits.DataSource = Enum.GetNames(typeof(Common.speeds));

            //CMB_theme.DataSource = Enum.GetNames(typeof(Utilities.ThemeManager.Themes));

            //CMB_theme.Text = ThemeManager.CurrentTheme.ToString();

            // setup language selection
            //var cultureCodes = new[] { "en-US", "zh-Hans", "zh-TW", "ru-RU", "Fr", "Pl", "it-IT", "es-ES","de-DE" };

            //_languages = cultureCodes
            //    .Select(CultureInfoEx.GetCultureInfo)
            //    .Where(c => c != null)
            //    .ToList();

            //CMB_language.DisplayMember = "DisplayName";
            //CMB_language.DataSource = _languages;
            //var currentUiCulture = Thread.CurrentThread.CurrentUICulture;

            //for (int i = 0; i < _languages.Count; i++)
            //{
            //    if (currentUiCulture.IsChildOf(_languages[i]))
            //    {
            //        try
            //        {
            //            CMB_language.SelectedIndex = i;
            //        }
            //        catch { }
            //        break;
            //    }
            //}

            // setup up camera button states
            //if (MainV2.cam != null)
            //{
            //    BUT_videostart.Enabled = false;
            //    CHK_hudshow.Checked = GCSViews.FlightData.myhud.hudon;
            //}
            //else
            //{
            //    BUT_videostart.Enabled = true;
            //}

            //// setup speech states
            //SetCheckboxFromConfig("speechenable", CHK_enablespeech);
            //SetCheckboxFromConfig("speechwaypointenabled", CHK_speechwaypoint);
            //SetCheckboxFromConfig("speechmodeenabled", CHK_speechmode);
            //SetCheckboxFromConfig("speechcustomenabled", CHK_speechcustom);
            //SetCheckboxFromConfig("speechbatteryenabled", CHK_speechbattery);
            //SetCheckboxFromConfig("speechaltenabled", CHK_speechaltwarning);

            //// this can't fail because it set at startup
            //NUM_tracklength.Value = int.Parse(MainV2.config["NUM_tracklength"].ToString());

            //// get wps on connect
            //SetCheckboxFromConfig("loadwpsonconnect", CHK_loadwponconnect);

            //// setup other config state
            //SetCheckboxFromConfig("CHK_resetapmonconnect", CHK_resetapmonconnect);

            //CMB_rateattitude.Text = MainV2.comPort.MAV.cs.rateattitude.ToString();
            //CMB_rateposition.Text = MainV2.comPort.MAV.cs.rateposition.ToString();
            //CMB_raterc.Text = MainV2.comPort.MAV.cs.raterc.ToString();
            //CMB_ratestatus.Text = MainV2.comPort.MAV.cs.ratestatus.ToString();
            //CMB_ratesensors.Text = MainV2.comPort.MAV.cs.ratesensors.ToString();

            //SetCheckboxFromConfig("CHK_GDIPlus", CHK_GDIPlus);
            //SetCheckboxFromConfig("CHK_maprotation", CHK_maprotation);

            //SetCheckboxFromConfig("CHK_disttohomeflightdata", CHK_disttohomeflightdata);

            ////set hud color state
            //string hudcolor = (string)MainV2.config["hudcolor"];
            //int index = CMB_osdcolor.Items.IndexOf(hudcolor ?? "White");
            //try
            //{
            //    CMB_osdcolor.SelectedIndex = index;
            //}
            //catch { }


            if (MainV2.config["distunits"] != null)
                CMB_distunits.Text = MainV2.config["distunits"].ToString();
            if (MainV2.config["speedunits"] != null)
                CMB_speedunits.Text = MainV2.config["speedunits"].ToString();


            txt_log_dir.Text = MainV2.LogDir;
        }


        
        //private static void SetCheckboxFromConfig(string configKey, CheckBox chk)
        //{
        //    if (MainV2.config[configKey] != null)
        //        chk.Checked = bool.Parse(MainV2.config[configKey].ToString());
        //}

        //private void CHK_disttohomeflightdata_CheckedChanged(object sender, EventArgs e)
        //{
        //    MainV2.config["CHK_disttohomeflightdata"] = CHK_disttohomeflightdata.Checked.ToString();
        //}

        private void BUT_logdirbrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog ofd = new FolderBrowserDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txt_log_dir.Text = ofd.SelectedPath;
                MainV2.LogDir = ofd.SelectedPath;
            }                       
        }

        //private void CMB_theme_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;

        //    MainV2.config["theme"] = CMB_theme.Text;
        //    ThemeManager.SetTheme((ThemeManager.Themes)Enum.Parse(typeof(ThemeManager.Themes), CMB_theme.Text));
        //    ThemeManager.ApplyThemeTo(MainV2.instance);

        //    CustomMessageBox.Show("You may need to restart to see the full effect.");
        //}

        //private void BUT_themecustom_Click(object sender, EventArgs e)
        //{
        //    ThemeManager.CustomColor();
        //    CMB_theme.Text = "Custom";
        //}

        //private void CHK_speecharmdisarm_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (startup)
        //        return;
        //    MainV2.config["speecharmenabled"] = ((CheckBox)sender).Checked.ToString();

        //    if (((CheckBox)sender).Checked)
        //    {
        //        string speechstring = "Armed";
        //        if (MainV2.config["speecharm"] != null)
        //            speechstring = MainV2.config["speecharm"].ToString();
        //        Common.InputBox("Arm", "What do you want it to say?", ref speechstring);
        //        MainV2.config["speecharm"] = speechstring;

        //        speechstring = "Disarmed";
        //        if (MainV2.config["speechdisarm"] != null)
        //            speechstring = MainV2.config["speechdisarm"].ToString();
        //        Common.InputBox("Disarmed", "What do you want it to say?", ref speechstring);
        //        MainV2.config["speechdisarm"] = speechstring;

        //    }
        //}

        private void CHK_autodec_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked == true)
            {
                TXT_declination.Enabled = false;
            }
            else
            {
                TXT_declination.Enabled = true;
            }

            if (startup)
                return;
            try
            {
                if (MainV2.comPort.MAV.param["COMPASS_AUTODEC"] == null)
                {
                    CustomMessageBox.Show("Not Available on " + MainV2.comPort.MAV.cs.firmware.ToString());
                }
                else
                {
                    MainV2.comPort.setParam("COMPASS_AUTODEC", ((CheckBox)sender).Checked == true ? 1 : 0);
                }
            }
            catch { CustomMessageBox.Show("Set COMPASS_AUTODEC Failed"); }
        }

        private void linkLabelmagdec_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.magnetic-declination.com/");
            }
            catch { CustomMessageBox.Show("Webpage open failed... \nhttp://www.magnetic-declination.com/"); }
        }

        private void TXT_declination_Validated(object sender, EventArgs e)
        {
            if (startup)
                return;
            try
            {
                if (MainV2.comPort.MAV.param["COMPASS_DEC"] == null)
                {
                    CustomMessageBox.Show("Not Available");
                }
                else
                {
                    float dec = 0.0f;
                    try
                    {
                        string declination = TXT_declination.Text;
                        float.TryParse(declination, out dec);
                        float deg = (float)((int)dec);
                        float mins = (dec - deg);
                        if (dec > 0)
                        {
                            dec += ((mins) / 60.0f);
                        }
                        else
                        {
                            dec -= ((mins) / 60.0f);
                        }
                    }
                    catch { CustomMessageBox.Show("Invalid input!"); return; }

                    TXT_declination.Text = dec.ToString();

                    MainV2.comPort.setParam("COMPASS_DEC", dec * deg2rad);
                }
            }
            catch { CustomMessageBox.Show("Set COMPASS_DEC Failed"); }
        }

        private void But_searchAddress_Click(object sender, EventArgs e)
        {
            string placename = txtBox_locAddress.Text;
            //GeoCoderStatusCode status = MainV2.f  zoomToToolLocation(ref placename);
            //if (status == GeoCoderStatusCode.G_GEO_SUCCESS)
            //    txtBox_locAddress.Text = placename;

            string place;
            if(placename.Length > 3)
                place = placename;
            else
                place = "Adelaide Airport, Australia";

            if (DialogResult.OK == Common.InputBox("Location", "Enter your location", ref place))
                txtBox_locAddress.Text = place;
        }


        private void But_cameraSetup_Click(object sender, EventArgs e)
        {
            using (ProjectCalculator frm = new ProjectCalculator())
            {

                ThemeManager.ApplyThemeTo(frm);
                switch (frm.ShowDialog())
                {
                    case DialogResult.OK:
                        camera = frm;

                        // camerainfo
                        textBoxCamera.Text = camera.CameraId;
                        // string brand;
                        // string model;
                        textBoxFocal.Text = (camera.FocalLength).ToString();
                        // float sensorwidth;
                        // float sensorheight;
                        // float imagewidth;
                        // float imageheight;

                        // surveyinfo
                        textBoxAGL.Text = camera.flyHeightAGL.ToString();
                        textBoxCruiseSpeed.Text = camera.cruiseSpeed.ToString();
                        textBoxFwdOverlap.Text = camera.fwdOverlap.ToString();
                        textBoxSideOverlap.Text = camera.sideOverlap.ToString();
                        textBoxPhotoSpacing.Text = camera.photoSpacing.ToString();
                        //textBoxRunSpacing.Text = camera.runSpacing.ToString();
                        textBoxGSD.Text = camera.GSD.ToString();
                        textBoxImgFormat.Text = camera.imgFormat;
                        textBoxMaxFrameRate.Text = camera.maxFPS.ToString();

                        switch (camera.cameraOrientn)
                        {
                            case 90:
                                label_CameraOrientn.Text = "Camera orientation - top facing left (port) wing";
                            break;

                            case -90:
                            label_CameraOrientn.Text = "Camera orientation - top facing right (starboard) wing";
                            break;

                            case 180:
                            case -180:
                                label_CameraOrientn.Text = "Camera orientation - top facing back";
                            break;

                            case 0:
                            default:
                                label_CameraOrientn.Text = "Camera orientation - top facing forward";
                            break;
                        }
                                  
                        break;

                    case DialogResult.Cancel:
                        //MessageBox.Show("Cancelled!");
                        break;
                }

                // If connected to AutoPilot upload new photo spacing
                //if (connected)
                if (MainV2.comPort.BaseStream.IsOpen)
                {
                    //try
                    //{
                    //    bool ans = MainV2.comPort.setParam(ParamName, (float)this.Value * _scale);
                    //    if (ans == false)
                    //        CustomMessageBox.Show("Set " + ParamName + " Failed 1!");
                    //}
                    //catch 
                    //{ 
                    //    CustomMessageBox.Show("Set " + ParamName + " Failed 2!"); }
                    //}
                }
                //---------------------------------------------------------------------//
            }
        }


        private void xmlstaff(bool write)
        {
            string filename = "personnel.xml";

            if (write || !File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + filename))
            {
                try
                {
                    XmlTextWriter xmlwriter = new XmlTextWriter(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + filename, Encoding.ASCII);
                    xmlwriter.Formatting = Formatting.Indented;

                    xmlwriter.WriteStartDocument();

                    xmlwriter.WriteStartElement("Personnel");

                    foreach (string key in personnel.Keys)
                    {
                        try
                        {
                            if (key == "")
                                continue;
                            xmlwriter.WriteStartElement("Person");
                            xmlwriter.WriteElementString("name", personnel[key].name);
                            xmlwriter.WriteElementString("title", personnel[key].title);
                            xmlwriter.WriteEndElement();
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
                    using (XmlTextReader xmlreader = new XmlTextReader(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + filename))
                    {
                        while (xmlreader.Read())
                        {
                            xmlreader.MoveToElement();
                            try
                            {
                                switch (xmlreader.Name)
                                {
                                    case "Person":
                                        {
                                            staffinfo person = new staffinfo();

                                            while (xmlreader.Read())
                                            {
                                                bool dobreak = false;
                                                xmlreader.MoveToElement();
                                                switch (xmlreader.Name)
                                                {
                                                    case "name":
                                                        person.name = xmlreader.ReadString();
                                                        break;
                                                    case "title":
                                                        person.title = xmlreader.ReadString();
                                                        break;
                                                    case "Person":
                                                        personnel.Add(person.name, person);
                                                        CMBox_pilot.Items.Add(person.name);
                                                        CMBox_GSoperator.Items.Add(person.name);
                                                        CMBox_observer.Items.Add(person.name);
                                                        dobreak = true;
                                                        break;
                                                }
                                                if (dobreak)
                                                    break;
                                            }
                                            string temp = xmlreader.ReadString();
                                        }
                                        break;
                                    case "Config":
                                        break;
                                    case "xml":
                                        break;
                                    default:
                                        if (xmlreader.Name == "") // line feeds
                                            break;
                                        break;
                                }
                            }
                            catch (Exception ee) { Console.WriteLine(ee.Message); } // silent fail on bad entry
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Bad Personnel File: " + ex.ToString()); } // bad config file
            }


            //// Alternative
            //DataTable dt; 

            //if (write || !File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "testpeople.xml")) //filename))
            //{
            //    DataSet ds = new DataSet();
            //    dt = new DataTable();
            //    dt.Columns.Add(new DataColumn("Name", Type.GetType("System.String")));
            //    dt.Columns.Add(new DataColumn("Title", Type.GetType("System.String")));
            //    foreach (string key in personnel.Keys)
            //    {
            //        try
            //        {
            //            DataRow dr;
            //            dr = dt.NewRow();
            //            dr["Name"] = personnel[key].name;
            //            dr["Title"] = personnel[key].title;
            //            dt.Rows.Add(dr);
            //        }
            //        catch { }
            //    }
            //    ds.Tables.Add(dt);
            //    ds.Tables[0].TableName = "Personnel";
            //    ds.WriteXml(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "testpeople.xml"); //filename);
            //    MessageBox.Show("Write Done");

            //}
            //else
            //{   // Read XML

            //    try
            //    {
            //        XmlReader xmlFile;
            //        xmlFile = XmlReader.Create(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + "testpeople.xml", new XmlReaderSettings());
            //        DataSet ds = new DataSet();
            //        ds.ReadXml(xmlFile);
            //        if (ds.Tables.Count > 0)
            //        {
            //            dataGridView_People.DataSource = ds.Tables[0]; // = ds;
            //            //dataGridView_People.DataBind();
            //        }
                    
            //    }
            //    catch (Exception ex) 
            //    { 
            //        Console.WriteLine("Bad Personnel File: " + ex.ToString()); // bad config file
            //    } 
            //}
        }
        

        private void BUT_save_Click(object sender, EventArgs e)
        {

            // Save Personnel Info
            staffinfo person = new staffinfo();

            // check if person exists already
            if (personnel.ContainsKey(CMBox_pilot.Text))
            {
                person = personnel[CMBox_pilot.Text];
            }
            else
            {
                personnel.Add(CMBox_pilot.Text, person);
            }

            try
            {
                person.name = CMBox_pilot.Text;
                //person.title = comboBoxBrand.Text;
            }
            catch { CustomMessageBox.Show("Person name is not valid"); return; }

            personnel[CMBox_pilot.Text] = person;
            xmlstaff(true);

        }

        private void Personnel_Load() // (object sender, EventArgs e)
        {
            xmlstaff(false);
        }

        private void But_People_Click(object sender, EventArgs e)
        {

        }



    }
}
