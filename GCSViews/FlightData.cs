﻿using System;
using System.Collections.Generic; // Lists
using System.Text; // stringbuilder
using System.Drawing; // pens etc
using System.IO; // file io
using System.IO.Ports; // serial
using System.Windows.Forms; // Forms
using System.Collections; // hashs
using System.Text.RegularExpressions; // regex
using System.Xml; // GE xml alt reader
using System.Net; // dns, ip address
using System.Net.Sockets; // tcplistner
using GMap.NET;
using GMap.NET.WindowsForms;
using System.Globalization; // language
using GMap.NET.WindowsForms.Markers;
using ZedGraph; // Graphs
using System.Drawing.Drawing2D;
using ArdupilotMega.Controls;
using ArdupilotMega.Utilities;
using ArdupilotMega.Controls.BackstageView;
//using Crom.Controls.Docking;
using log4net;
using System.Reflection;

// written by michael oborne
namespace ArdupilotMega.GCSViews
{
    partial class FlightData : MyUserControl, IActivate, IDeactivate
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static int threadrun = 0;
        int tickStart = 0;
        RollingPointPairList list1 = new RollingPointPairList(1200);
        RollingPointPairList list2 = new RollingPointPairList(1200);
        RollingPointPairList list3 = new RollingPointPairList(1200);
        RollingPointPairList list4 = new RollingPointPairList(1200);
        RollingPointPairList list5 = new RollingPointPairList(1200);
        RollingPointPairList list6 = new RollingPointPairList(1200);
        RollingPointPairList list7 = new RollingPointPairList(1200);
        RollingPointPairList list8 = new RollingPointPairList(1200);
        RollingPointPairList list9 = new RollingPointPairList(1200);
        RollingPointPairList list10 = new RollingPointPairList(1200);

        System.Reflection.PropertyInfo list1item = null;
        System.Reflection.PropertyInfo list2item = null;
        System.Reflection.PropertyInfo list3item = null;
        System.Reflection.PropertyInfo list4item = null;
        System.Reflection.PropertyInfo list5item = null;
        System.Reflection.PropertyInfo list6item = null;
        System.Reflection.PropertyInfo list7item = null;
        System.Reflection.PropertyInfo list8item = null;
        System.Reflection.PropertyInfo list9item = null;
        System.Reflection.PropertyInfo list10item = null;

        CurveItem list1curve;
        CurveItem list2curve;
        CurveItem list3curve;
        CurveItem list4curve;
        CurveItem list5curve;
        CurveItem list6curve;
        CurveItem list7curve;
        CurveItem list8curve;
        CurveItem list9curve;
        CurveItem list10curve;

        internal static GMapOverlay kmlpolygons;
        internal static GMapOverlay geofence;
        internal static GMapOverlay photos;             // DWR 03/09/2013
        long lastphotocnt = 0L;

        Dictionary<Guid, Form> formguids = new Dictionary<Guid, Form>();

        bool huddropout = false;
        bool huddropoutresize = false;

        //      private DockStateSerializer _serializer = null;

        List<PointLatLng> trackPoints = new List<PointLatLng>();

        const float rad2deg = (float)(180 / Math.PI);

        const float deg2rad = (float)(1.0 / rad2deg);

        public static ArdupilotMega.Controls.HUD myhud = null;
        public static GMapControl mymap = null;

        bool playingLog = false;
        double LogPlayBackSpeed = 1.0;

        GMapMarker marker;

        AviWriter aviwriter;

        public SplitContainer MainHcopy = null;

        public static FlightData instance;


        protected override void Dispose(bool disposing)
        {
            threadrun = 0;
            MainV2.comPort.logreadmode = false;
            try
            {
                MainV2.config["FlightSplitter"] = hud1.Width;
            }
            catch { }
            System.Threading.Thread.Sleep(100);
            base.Dispose(disposing);
        }

        public FlightData()
        {
            InitializeComponent();

            instance = this;
            //    _serializer = new DockStateSerializer(dockContainer1);
            //    _serializer.SavePath = Application.StartupPath + Path.DirectorySeparatorChar + "FDscreen.xml";
            //    dockContainer1.PreviewRenderer = new PreviewRenderer();
            //
            mymap = gMapControl1;
            myhud = hud1;
            MainHcopy = MainH;
//////////
            // get map type
            mymap.MapType = MapType.GoogleSatellite;
            //comboBoxMapType.DataSource = Enum.GetValues(typeof(MapType));
            //comboBoxMapType.SelectedItem = mymap.MapType;      // Want this to be the same as MainMap.MapType;

            // map events
            //mymap.OnMapTypeChanged += new MapTypeChanged(myMap_OnMapTypeChanged);
//////////
            // setup default tuning graph
            if (MainV2.config["Tuning_Graph_Selected"] != null)
            {
                string line = MainV2.config["Tuning_Graph_Selected"].ToString();
                string[] lines = line.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string option in lines)
                {
                    chk_box_CheckedChanged((object)(new CheckBox() { Name = option, Checked = true }), new EventArgs());
                }
            }
            else
            {
                chk_box_CheckedChanged((object)(new CheckBox() { Name = "roll", Checked = true }), new EventArgs());
                chk_box_CheckedChanged((object)(new CheckBox() { Name = "pitch", Checked = true }), new EventArgs());
                chk_box_CheckedChanged((object)(new CheckBox() { Name = "nav_roll", Checked = true }), new EventArgs());
                chk_box_CheckedChanged((object)(new CheckBox() { Name = "nav_pitch", Checked = true }), new EventArgs());
            }

            foreach (string item in MainV2.config.Keys)
            {
                if (item.StartsWith("hud1_useritem_"))
                {
                    string selection = item.Replace("hud1_useritem_", "");

                    CheckBox chk = new CheckBox();
                    chk.Name = selection;
                    chk.Checked = true;

                    HUD.Custom cust = new HUD.Custom();
                    cust.Header = MainV2.config[item].ToString();

                    addHudUserItem(ref cust, chk);
                }
            }


            List<string> list = new List<string>();

            {
                list.Add("LOITER_UNLIM");
                list.Add("RETURN_TO_LAUNCH");
                list.Add("PREFLIGHT_CALIBRATION");
                list.Add("MISSION_START");
                list.Add("PREFLIGHT_REBOOT_SHUTDOWN");
                //DO_SET_SERVO
                //DO_REPEAT_SERVO
                list.Add("DO_SET_PRELAUNCH_THROTTLE");      // 20140805 DWR
            }

            CMB_action.DataSource = list;

            CMB_modes.DataSource = Common.getModesList();
            CMB_modes.ValueMember = "Key";
            CMB_modes.DisplayMember = "Value";

            CMB_setwp.SelectedIndex = 0;

            zg1.Visible = true;

            CreateChart(zg1);
//////////
            // config map
            //gMapControl1.MapType = MapType.GoogleSatellite;
            gMapControl1.MapType = mymap.MapType;   // 12/11/2013 test
            //comboBoxMapType.SelectedItem = mymap.MapType;   // 12/11/2013 test..works GSat can b diff to flightPlan
//////////
            gMapControl1.MinZoom = 1;
            gMapControl1.CacheLocation = Path.GetDirectoryName(Application.ExecutablePath) + "/gmapcache/";

            gMapControl1.OnMapZoomChanged += new MapZoomChanged(gMapControl1_OnMapZoomChanged);

            gMapControl1.Zoom = 3;

            gMapControl1.RoutesEnabled = true;
            gMapControl1.PolygonsEnabled = true;

            kmlpolygons = new GMapOverlay(gMapControl1, "kmlpolygons");
            gMapControl1.Overlays.Add(kmlpolygons);

            geofence = new GMapOverlay(gMapControl1, "geofence");
            gMapControl1.Overlays.Add(geofence);

            polygons = new GMapOverlay(gMapControl1, "polygons");
            gMapControl1.Overlays.Add(polygons);

            routes = new GMapOverlay(gMapControl1, "routes");
            gMapControl1.Overlays.Add(routes);

            // Photo position Markers
            photos = new GMapOverlay(gMapControl1, "photos");
            gMapControl1.Overlays.Add(photos);

            try
            {
                if (MainV2.getConfig("GspeedMAX") != "")
                {
                    Gspeed.MaxValue = float.Parse(MainV2.getConfig("GspeedMAX"));
                }
            }
            catch { }

            MainV2.comPort.ParamListChanged += FlightData_ParentChanged;
        }
   
        void tabStatus_Resize(object sender, EventArgs e)
        {
            // localise it
            //Control tabStatus = sender as Control;

            //  tabStatus.SuspendLayout();

            //foreach (Control temp in tabStatus.Controls)
            {
                //  temp.DataBindings.Clear();
                //temp.Dispose();
            }
            //tabStatus.Controls.Clear();

            int x = 10;
            int y = 10;

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            PropertyInfo[] props = test.GetProperties();

            //int fixmesort;
            //props

            foreach (var field in props)
            {
                // field.Name has the field's name.
                object fieldValue;
                try
                {
                    fieldValue = field.GetValue(thisBoxed, null); // Get value
                }
                catch { continue; }

                // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                TypeCode typeCode = Type.GetTypeCode(fieldValue.GetType());

                bool add = true;

                MyLabel lbl1 = new MyLabel();
                MyLabel lbl2 = new MyLabel();
                try
                {
                    lbl1 = (MyLabel)tabStatus.Controls.Find(field.Name, false)[0];

                    lbl2 = (MyLabel)tabStatus.Controls.Find(field.Name + "value", false)[0];

                    add = false;
                }
                catch { }

                if (add)
                {

                    lbl1.Location = new Point(x, y);
                    lbl1.Size = new System.Drawing.Size(75, 13);
                    lbl1.Text = field.Name;
                    lbl1.Name = field.Name;
                    lbl1.Visible = true;
                    lbl2.AutoSize = false;

                    lbl2.Location = new Point(lbl1.Right + 5, y);
                    lbl2.Size = new System.Drawing.Size(50, 13);
                    //if (lbl2.Name == "")
                    lbl2.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSource1, field.Name, false, System.Windows.Forms.DataSourceUpdateMode.OnValidation, "0"));
                    lbl2.Name = field.Name + "value";
                    lbl2.Visible = true;
                    //lbl2.Text = fieldValue.ToString();


                    tabStatus.Controls.Add(lbl1);
                    tabStatus.Controls.Add(lbl2);
                }
                else
                {
                    lbl1.Location = new Point(x, y);
                    lbl2.Location = new Point(lbl1.Right + 5, y);
                }

                //Application.DoEvents();

                x += 0;
                y += 15;

                if (y > tabStatus.Height - 30)
                {
                    x += 140;
                    y = 10;
                }
            }

            tabStatus.Width = x;

            ThemeManager.ApplyThemeTo(tabStatus);

            //   tabStatus.ResumeLayout();
        }

        public void Activate()
        {
            if (CB_tuning.Checked)
                ZedGraphTimer.Start();

            if (MainV2.MONO)
            {
                if (!hud1.Visible)
                    hud1.Visible = true;
                if (!hud1.Enabled)
                    hud1.Enabled = true;

                hud1.Dock = DockStyle.Fill;
            }

            for (int f = 1; f < 10; f++)
            {
                // load settings
                if (MainV2.config["quickView" + f] != null)
                {
                    Control[] ctls = this.Controls.Find("quickView" + f, true);
                    if (ctls.Length > 0)
                    {
                        // set description
                        ((QuickView)ctls[0]).desc = MainV2.comPort.MAV.cs.GetNameandUnit(MainV2.config["quickView" + f].ToString());

                        // set databinding for value
                        ((QuickView)ctls[0]).DataBindings.Clear();
                        try
                        {
                            ((QuickView)ctls[0]).DataBindings.Add(new System.Windows.Forms.Binding("number", this.bindingSource1, MainV2.config["quickView" + f].ToString(), true));
                        }
                        catch { }
                    }
                }
                else
                {
                    // if no config, update description on predefined
                    try
                    {
                          Control[] ctls = this.Controls.Find("quickView" + f, true);
                          if (ctls.Length > 0)
                          {
                              ((QuickView)ctls[0]).desc = MainV2.comPort.MAV.cs.GetNameandUnit(((QuickView)ctls[0]).desc);
                          }
                    }
                    catch { }
                }
            }

            // ensure battery display is on - also set in hud if current is updated
            if (MainV2.comPort.MAV.param.ContainsKey("BATT_MONITOR") && (float)MainV2.comPort.MAV.param["BATT_MONITOR"] != 0)
            {
                hud1.batteryon = true;
            }
            else
            {
                hud1.batteryon = false;
            }

            foreach (Control ctl in splitContainer1.Panel2.Controls)
            {
                ctl.Visible = true;
            }
        }

        public void Deactivate()
        {
            if (MainV2.MONO)
            {
                hud1.Dock = DockStyle.None;
                hud1.Size = new System.Drawing.Size(5, 5);
                hud1.Enabled = false;
                hud1.Visible = false;
            }
            //     hud1.Location = new Point(-1000,-1000);

            ZedGraphTimer.Stop();
        }

        void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            //e.DrawBackground();
            // Define the default color of the brush as black.
            Brush myBrush = Brushes.Black;

            LinearGradientBrush linear = new LinearGradientBrush(e.Bounds, Color.FromArgb(0x94, 0xc1, 0x1f), Color.FromArgb(0xcd, 0xe2, 0x96), LinearGradientMode.Vertical);

            e.Graphics.FillRectangle(linear, e.Bounds);

            // Draw the current item text based on the current Font 
            // and the custom brush settings.
            e.Graphics.DrawString(((TabControl)sender).TabPages[e.Index].Text.ToString(),
                e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();

        }

        void gMapControl1_OnMapZoomChanged()
        {
            TRK_zoom.Value = gMapControl1.Zoom;
            Zoomlevel.Value = Convert.ToDecimal(gMapControl1.Zoom);
        }

        private void FlightData_Load(object sender, EventArgs e)
        {
            System.Threading.Thread t11 = new System.Threading.Thread(new System.Threading.ThreadStart(mainloop))
            {
                IsBackground = true,
                Name = "FlightData updater"
            };

            t11.Start();
            //MainH.threads.Add(t11);

            TRK_zoom.Minimum = gMapControl1.MinZoom;
            TRK_zoom.Maximum = gMapControl1.MaxZoom + 1;
            TRK_zoom.Value = gMapControl1.Zoom;

            Zoomlevel.Minimum = gMapControl1.MinZoom;
            Zoomlevel.Maximum = gMapControl1.MaxZoom + 1;
            Zoomlevel.Value = Convert.ToDecimal(gMapControl1.Zoom);

            if (MainV2.config["CHK_autopan"] != null)
                CHK_autopan.Checked = bool.Parse(MainV2.config["CHK_autopan"].ToString());

            if (MainV2.config.Contains("FlightSplitter"))
            {
                //  hud1.Width = int.Parse(MainV2.config["FlightSplitter"].ToString());
                //  MainH.PerformLayout();
                MainH.SplitterDistance = int.Parse(MainV2.config["FlightSplitter"].ToString());
            }
        }

        private void mainloop()
        {
            //System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
            //System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            threadrun = 1;
            EndPoint Remote = (EndPoint)(new IPEndPoint(IPAddress.Any, 0));

            DateTime lastdata = DateTime.MinValue;

            DateTime tracklast = DateTime.Now.AddSeconds(0);

            DateTime tunning = DateTime.Now.AddSeconds(0);

            DateTime mapupdate = DateTime.Now.AddSeconds(0);

            DateTime vidrec = DateTime.Now.AddSeconds(0);

            DateTime waypoints = DateTime.Now.AddSeconds(0);

            DateTime updatescreen = DateTime.Now;

            DateTime tsreal = DateTime.Now;
            double taketime = 0;
            double timeerror = 0;

            //comPort.stopall(true);

            while (threadrun == 1)
            {
                if (threadrun == 0) { return; }

                if (MainV2.comPort.giveComport == true)
                {
                    System.Threading.Thread.Sleep(50);
                    continue;
                }
                if (!MainV2.comPort.BaseStream.IsOpen)
                    lastdata = DateTime.Now;
                // re-request servo data
                if (!(lastdata.AddSeconds(8) > DateTime.Now) && MainV2.comPort.BaseStream.IsOpen)
                {
                    //Console.WriteLine("REQ streams - flightdata");
                    try
                    {
                        //System.Threading.Thread.Sleep(1000);

                        //comPort.requestDatastream((byte)ArdupilotMega.MAVLink09.MAV_DATA_STREAM.RAW_CONTROLLER, 0); // request servoout
                        MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTENDED_STATUS, MainV2.comPort.MAV.cs.ratestatus); // mode
                        MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.POSITION, MainV2.comPort.MAV.cs.rateposition); // request gps
                        MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTRA1, MainV2.comPort.MAV.cs.rateattitude); // request attitude
                        MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTRA2, MainV2.comPort.MAV.cs.rateattitude); // request vfr
                        MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.EXTRA3, MainV2.comPort.MAV.cs.ratesensors); // request extra stuff - tridge
                        MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.RAW_SENSORS, MainV2.comPort.MAV.cs.ratesensors); // request raw sensor
                        MainV2.comPort.requestDatastream(ArdupilotMega.MAVLink.MAV_DATA_STREAM.RC_CHANNELS, MainV2.comPort.MAV.cs.raterc); // request rc info
                    }
                    catch { log.Error("Failed to request rates"); }
                    lastdata = DateTime.Now.AddSeconds(120); // prevent flooding
                }

                if (!MainV2.comPort.logreadmode)
                    System.Threading.Thread.Sleep(100); // max is only ever 10 hz

                try
                {
                    if (aviwriter != null && vidrec.AddMilliseconds(100) <= DateTime.Now)
                    {
                        vidrec = DateTime.Now;

                        hud1.streamjpgenable = true;

                        //aviwriter.avi_start("test.avi");
                        // add a frame
                        aviwriter.avi_add(hud1.streamjpg.ToArray(), (uint)hud1.streamjpg.Length);
                        // write header - so even partial files will play
                        aviwriter.avi_end(hud1.Width, hud1.Height, 10);
                    }
                }
                catch { log.Error("Failed to write avi"); }

                // log playback
                if (MainV2.comPort.logreadmode && MainV2.comPort.logplaybackfile != null)
                {
                    if (threadrun == 0) { return; }

                    if (MainV2.comPort.BaseStream.IsOpen)
                    {
                        MainV2.comPort.logreadmode = false;
                        try
                        {
                            MainV2.comPort.logplaybackfile.Close();
                        }
                        catch { log.Error("Failed to close logfile"); }
                        MainV2.comPort.logplaybackfile = null;
                    }


                    //Console.WriteLine(DateTime.Now.Millisecond);

                    if (updatescreen.AddMilliseconds(300) < DateTime.Now)
                    {
                        try
                        {
                            updatePlayPauseButton(true);
                            updateLogPlayPosition();
                        }
                        catch { log.Error("Failed to update log playback pos"); }
                        updatescreen = DateTime.Now;
                    }

                    //Console.WriteLine(DateTime.Now.Millisecond + " done ");

                    DateTime logplayback = MainV2.comPort.lastlogread;
                    try
                    {
                        MainV2.comPort.readPacket();
                    }
                    catch { log.Error("Failed to read log packet"); }

                    double act = (MainV2.comPort.lastlogread - logplayback).TotalMilliseconds;

                    if (act > 9999 || act < 0)
                        act = 0;

                    double ts = 0;
                    if (LogPlayBackSpeed == 0)
                        LogPlayBackSpeed = 0.01;
                    try
                    {
                        ts = Math.Min((act / LogPlayBackSpeed), 1000);
                    }
                    catch { }

                    double timetook = (DateTime.Now - tsreal).TotalMilliseconds;
                    if (timetook != 0)
                    {
                        //Console.WriteLine("took: " + timetook + "=" + taketime + " " + (taketime - timetook) + " " + ts);
                        //Console.WriteLine(MainV2.comPort.lastlogread.Second + " " + DateTime.Now.Second + " " + (MainV2.comPort.lastlogread.Second - DateTime.Now.Second));
                        //if ((taketime - timetook) < 0)
                        {
                            timeerror += (taketime - timetook);
                            if (ts != 0)
                            {
                                ts += timeerror;
                                timeerror = 0;
                            }
                        }
                        if (ts > 1000)
                            ts = 1000;
                    }

                    taketime = ts;
                    tsreal = DateTime.Now;

                    if (ts > 0 && ts < 1000)
                        System.Threading.Thread.Sleep((int)ts);



                    if (threadrun == 0) { return; }

                    tracklast = tracklast.AddMilliseconds(ts - act);
                    tunning = tunning.AddMilliseconds(ts - act);

                    if (tracklast.Month != DateTime.Now.Month)
                    {
                        tracklast = DateTime.Now;
                        tunning = DateTime.Now;
                    }

                    try
                    {
                        if (MainV2.comPort.logplaybackfile != null && MainV2.comPort.logplaybackfile.BaseStream.Position == MainV2.comPort.logplaybackfile.BaseStream.Length)
                        {
                            MainV2.comPort.logreadmode = false;
                        }
                    }
                    catch { MainV2.comPort.logreadmode = false; }
                }
                else
                {
                    // ensure we know to stop
                    if (MainV2.comPort.logreadmode)
                        MainV2.comPort.logreadmode = false;
                    updatePlayPauseButton(false);

                    if (!playingLog && MainV2.comPort.logplaybackfile != null)
                    {
                        continue;
                    }
                }



                try
                {
                     //Console.WriteLine(DateTime.Now.Millisecond);
                    updateBindingSource();
                    // Console.WriteLine(DateTime.Now.Millisecond + " done ");

                    // battery warning.
                    float warnvolt = 0;
                    float.TryParse(MainV2.getConfig("speechbatteryvolt"), out warnvolt);
                    float warnpercent = 0;
                    float.TryParse(MainV2.getConfig("speechbatterypercent"), out warnpercent);

                    if (MainV2.comPort.MAV.cs.battery_voltage <= warnvolt)
                    {
                        hud1.lowvoltagealert = true;
                    }
                    else if ((MainV2.comPort.MAV.cs.battery_remaining * 100) < warnpercent)
                    {
                        hud1.lowvoltagealert = true;
                    }
                    else
                    {
                        hud1.lowvoltagealert = false;
                    }
                    
                    // update opengltest
                    if (ArdupilotMega.Controls.OpenGLtest.instance != null)
                    {
                        ArdupilotMega.Controls.OpenGLtest.instance.rpy = new OpenTK.Vector3(MainV2.comPort.MAV.cs.roll, MainV2.comPort.MAV.cs.pitch, MainV2.comPort.MAV.cs.yaw);
                        ArdupilotMega.Controls.OpenGLtest.instance.LocationCenter = new PointLatLngAlt(MainV2.comPort.MAV.cs.lat, MainV2.comPort.MAV.cs.lng, MainV2.comPort.MAV.cs.alt, "here");
                    }

                    // udpate tunning tab
                    if (tunning.AddMilliseconds(50) < DateTime.Now && CB_tuning.Checked == true)
                    {

                        double time = (Environment.TickCount - tickStart) / 1000.0;
                        if (list1item != null)
                            list1.Add(time, (float)list1item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list2item != null)
                            list2.Add(time, (float)list2item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list3item != null)
                            list3.Add(time, (float)list3item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list4item != null)
                            list4.Add(time, (float)list4item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list5item != null)
                            list5.Add(time, (float)list5item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list6item != null)
                            list6.Add(time, (float)list6item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list7item != null)
                            list7.Add(time, (float)list7item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list8item != null)
                            list8.Add(time, (float)list8item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list9item != null)
                            list9.Add(time, (float)list9item.GetValue((object)MainV2.comPort.MAV.cs, null));
                        if (list10item != null)
                            list10.Add(time, (float)list10item.GetValue((object)MainV2.comPort.MAV.cs, null));
                    }

                    // update map
                    if (tracklast.AddSeconds(1) < DateTime.Now)
                    {
                        if (MainV2.config["CHK_maprotation"] != null && MainV2.config["CHK_maprotation"].ToString() == "True")
                        {
                            // dont holdinvalidation here
                            setMapBearing();
                        }

                        if (route == null)
                        {
                            route = new GMapRoute(trackPoints, "track");
                            routes.Routes.Add(route);
                        }

                        PointLatLng currentloc = new PointLatLng(MainV2.comPort.MAV.cs.lat, MainV2.comPort.MAV.cs.lng);

                        gMapControl1.HoldInvalidation = true;

                        int cnt = 0;

                        while (gMapControl1.inOnPaint == true)
                        {
                            System.Threading.Thread.Sleep(1);
                            cnt++;
                        }

                        // maintain route history length
                        if (route.Points.Count > int.Parse(MainV2.config["NUM_tracklength"].ToString()))
                        {
                            //  trackPoints.RemoveRange(0, trackPoints.Count - int.Parse(MainV2.config["NUM_tracklength"].ToString()));
                            route.Points.RemoveRange(0, route.Points.Count - int.Parse(MainV2.config["NUM_tracklength"].ToString()));
                        }
                        // add new route point
                        if (MainV2.comPort.MAV.cs.lat != 0)
                        {
                            // trackPoints.Add(currentloc);
                            route.Points.Add(currentloc);

                            // 03/09/2013 Add photo marker
                            if (MainV2.comPort.MAV.cs.photocnt > lastphotocnt)
                            {
                                lastphotocnt = MainV2.comPort.MAV.cs.photocnt;
                                addpolygonmarkerred(lastphotocnt.ToString(), MainV2.comPort.MAV.cs.photolng, MainV2.comPort.MAV.cs.photolat, (int)MainV2.comPort.MAV.cs.photoalt, Color.Purple, photos);
                            }
                        }

                        {

                            while (gMapControl1.inOnPaint == true)
                            {
                                System.Threading.Thread.Sleep(1);
                            }

                            //route = new GMapRoute(route.Points, "track");
                            //track.Stroke = Pens.Red;
                            //route.Stroke = new Pen(Color.FromArgb(144, Color.Red));
                            //route.Stroke.Width = 5;
                            //route.Tag = "track";

                            //updateClearRoutes();

                            gMapControl1.UpdateRouteLocalPosition(route);

                            // update programed wp course
                            if (waypoints.AddSeconds(5) < DateTime.Now)
                            {
                                //Console.WriteLine("Doing FD WP's");
                                updateMissionRouteMarkers();

                                foreach (MAVLink.mavlink_mission_item_t plla in MainV2.comPort.MAV.wps.Values)
                                {
                                    if (plla.x == 0 || plla.y == 0)
                                        continue;

                                    if (plla.command == (byte)MAVLink.MAV_CMD.ROI)
                                    {
                                        addpolygonmarkerred(plla.seq.ToString(), plla.y, plla.x, (int)plla.z, Color.Red, routes);
                                        continue;
                                    }

                                    string tag = plla.seq.ToString();
                                    if (plla.seq == 0 && plla.current != 2)
                                    {
                                        tag = "Home";
                                    }
                                    if (plla.current == 2)
                                    {
                                        continue;
                                    }

                                    addpolygonmarker(tag, plla.y, plla.x, (int)plla.z, Color.White, polygons);
                                }

                                RegeneratePolygon();

                                waypoints = DateTime.Now;
                            }

                            //routes.Polygons.Add(poly);    

                            if (route.Points.Count > 0)
                            {
                                // add primary route icon
                                if (routes.Markers.Count != 1)
                                {
                                    routes.Markers.Clear();
                                    routes.Markers.Add(new GMapMarkerCross(currentloc));
                                }

                                if (MainV2.comPort.MAV.cs.mode.ToLower() == "guided" && MainV2.comPort.MAV.GuidedMode.x != 0)
                                {
                                    addpolygonmarker("Guided Mode", MainV2.comPort.MAV.GuidedMode.y, MainV2.comPort.MAV.GuidedMode.x, (int)MainV2.comPort.MAV.GuidedMode.z, Color.Blue, routes);
                                }

                                if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane || MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.Ateryx)
                                {
                                    routes.Markers[0] = (new GMapMarkerPlane(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.groundcourse, MainV2.comPort.MAV.cs.nav_bearing, MainV2.comPort.MAV.cs.target_bearing, gMapControl1) { ToolTipText = MainV2.comPort.MAV.cs.alt.ToString("0"), ToolTipMode = MarkerTooltipMode.Always });
                                }
                                else if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduRover)
                                {
                                    routes.Markers[0] = (new GMapMarkerRover(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.groundcourse, MainV2.comPort.MAV.cs.nav_bearing, MainV2.comPort.MAV.cs.target_bearing, gMapControl1));
                                }
                                else
                                {
                                    routes.Markers[0] = (new GMapMarkerQuad(currentloc, MainV2.comPort.MAV.cs.yaw, MainV2.comPort.MAV.cs.groundcourse, MainV2.comPort.MAV.cs.nav_bearing));
                                }

                                // add extra mavs
                                int a = 1;
                                foreach (var port in MainV2.Comports)
                                {
                                    if (port == MainV2.comPort)
                                        continue;

                                    PointLatLng portlocation = new PointLatLng(port.MAV.cs.lat, port.MAV.cs.lng);

                                    while (routes.Markers.Count < (a+1))
                                        routes.Markers.Add(new GMapMarkerCross(portlocation));

                                    if (port.MAV.cs.firmware == MainV2.Firmwares.ArduPlane || MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.Ateryx)
                                    {
                                        routes.Markers[a] = (new GMapMarkerPlane(portlocation, port.MAV.cs.yaw, port.MAV.cs.groundcourse, port.MAV.cs.nav_bearing, port.MAV.cs.target_bearing, gMapControl1) { ToolTipText = "MAV: " + a + " " +port.MAV.cs.alt.ToString("0"), ToolTipMode = MarkerTooltipMode.Always });
                                    }
                                    else if (port.MAV.cs.firmware == MainV2.Firmwares.ArduRover)
                                    {
                                        routes.Markers[a] = (new GMapMarkerRover(portlocation, port.MAV.cs.yaw, port.MAV.cs.groundcourse, port.MAV.cs.nav_bearing, port.MAV.cs.target_bearing, gMapControl1));
                                    }
                                    else
                                    {
                                        routes.Markers[a] = (new GMapMarkerQuad(portlocation, port.MAV.cs.yaw, port.MAV.cs.groundcourse, port.MAV.cs.nav_bearing));
                                    }
                                    a++;
                                }

                                if (route.Points[route.Points.Count - 1].Lat != 0 && (mapupdate.AddSeconds(3) < DateTime.Now) && CHK_autopan.Checked)
                                {
                                    updateMapPosition(currentloc);
                                    mapupdate = DateTime.Now;
                                }

                                if (route.Points.Count == 1 && gMapControl1.Zoom == 3) // 3 is the default load zoom
                                {
                                    updateMapPosition(currentloc);
                                    updateMapZoom(17);
                                    //gMapControl1.ZoomAndCenterMarkers("routes");// ZoomAndCenterRoutes("routes");
                                }
                            }

                            gMapControl1.HoldInvalidation = false;

                            gMapControl1.Invalidate();
                        }

                        tracklast = DateTime.Now;
                    }
                }
                catch (Exception ex) { Console.WriteLine("FD Main loop exception " + ex.ToString()); }
            }
            Console.WriteLine("FD Main loop exit");
        }

        private void setMapBearing()
        {
            this.Invoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                gMapControl1.Bearing = (int)MainV2.comPort.MAV.cs.yaw;
            });
        }


        // to prevent cross thread calls while in a draw and exception
        private void updateClearRoutes()
        {
            // not async
            this.Invoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                routes.Routes.Clear();
                routes.Routes.Add(route);
            });
        }

        // to prevent cross thread calls while in a draw and exception
        private void updateMissionRouteMarkers()
        {
            // not async
            this.Invoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                polygons.Markers.Clear();
                routes.Markers.Clear();
            });
        }


        private void updatePlayPauseButton(bool playing)
        {
            if (playing)
            {
                if (BUT_playlog.Text == "Pause")
                    return;

                this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                {
                    try
                    {
                        BUT_playlog.Text = "Pause";
                    }
                    catch { }
                });
            }
            else
            {
                if (BUT_playlog.Text == "Play")
                    return;

                this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
                {
                    try
                    {
                        BUT_playlog.Text = "Play";
                    }
                    catch { }
                });
            }
        }

        private void updateBindingSource()
        {
            this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate()
            {
                try
                {
                    MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource1);
                }
                catch { }
            });
        }

        private void updateMapPosition(PointLatLng currentloc)
        {
            this.BeginInvoke((MethodInvoker)delegate()
            {
                try
                {
                    gMapControl1.Position = currentloc;
                    //hud1.Refresh();
                }
                catch { }
            });
        }

        private void updateMapZoom(int zoom)
        {
            this.BeginInvoke((MethodInvoker)delegate()
            {
                try
                {
                    gMapControl1.Zoom = zoom;
                }
                catch { }
            });
        }

        private void updateLogPlayPosition()
        {
            this.BeginInvoke((MethodInvoker)delegate()
            {
                try
                {
                    if (tracklog.Visible)
                        tracklog.Value = (int)(MainV2.comPort.logplaybackfile.BaseStream.Position / (double)MainV2.comPort.logplaybackfile.BaseStream.Length * 100);
                    if (lbl_logpercent.Visible)
                        lbl_logpercent.Text = (MainV2.comPort.logplaybackfile.BaseStream.Position / (double)MainV2.comPort.logplaybackfile.BaseStream.Length).ToString("0.00%");
                }
                catch { }
            });
        }

        private void addpolygonmarker(string tag, double lng, double lat, int alt, Color? color, GMapOverlay overlay)
        {
            try
            {
                PointLatLng point = new PointLatLng(lat, lng);
                GMapMarkerGoogleGreen m = new GMapMarkerGoogleGreen(point);
                m.ToolTipMode = MarkerTooltipMode.Always;
                m.ToolTipText = tag;
                m.Tag = tag;

                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                    try
                    {
                        mBorders.wprad = (int)(float.Parse(ArdupilotMega.MainV2.config["TXT_WPRad"].ToString()) / MainV2.comPort.MAV.cs.multiplierdist);
                    }
                    catch { }
                    mBorders.MainMap = gMapControl1;
                    if (color.HasValue)
                    {
                        mBorders.Color = color.Value;
                    }
                }

                overlay.Markers.Add(m);
                overlay.Markers.Add(mBorders);
            }
            catch (Exception) { }
        }

        private void addpolygonmarkerred(string tag, double lng, double lat, int alt, Color? color, GMapOverlay overlay)
        {
            try
            {
                PointLatLng point = new PointLatLng(lat, lng);
                //GMapMarkerGoogleRed m = new GMapMarkerGoogleRed(point);
                GMapMarkerGoogleCameraSpot m = new GMapMarkerGoogleCameraSpot(point);
                m.ToolTipMode = MarkerTooltipMode.Always;
                m.ToolTipText = tag;
                m.Tag = tag;

                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                }

                overlay.Markers.Add(m);
                overlay.Markers.Add(mBorders);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// used to redraw the polygon
        /// </summary>
        void RegeneratePolygon()
        {
            List<PointLatLng> polygonPoints = new List<PointLatLng>();

            if (routes == null)
                return;

            foreach (GMapMarker m in polygons.Markers)
            {
                if (m is GMapMarkerRect)
                {
                    m.Tag = polygonPoints.Count;
                    polygonPoints.Add(m.Position);
                }
            }

            if (polygon == null)
            {
                polygon = new GMapPolygon(polygonPoints, "polygon test");
                polygons.Polygons.Add(polygon);
            }
            else
            {
                polygon.Points.Clear();
                polygon.Points.AddRange(polygonPoints);

                polygon.Stroke = new Pen(Color.Yellow, 4);

                if (polygons.Polygons.Count == 0)
                {
                    polygons.Polygons.Add(polygon);
                }
                else
                {
                    gMapControl1.UpdatePolygonLocalPosition(polygon);
                }
            }
        }

        GMapPolygon polygon;
        GMapOverlay polygons;
        GMapOverlay routes;
        GMapRoute route;

        public void CreateChart(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;

            // Set the titles and axis labels
            myPane.Title.Text = "Tuning";
            myPane.XAxis.Title.Text = "Time (s)";
            myPane.YAxis.Title.Text = "Unit";

            // Show the x axis grid
            myPane.XAxis.MajorGrid.IsVisible = true;

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 5;

            // Make the Y axis scale red
            myPane.YAxis.Scale.FontSpec.FontColor = Color.White;
            myPane.YAxis.Title.FontSpec.FontColor = Color.White;
            // turn off the opposite tics so the Y tics don't show up on the Y2 axis
            myPane.YAxis.MajorTic.IsOpposite = false;
            myPane.YAxis.MinorTic.IsOpposite = false;
            // Don't display the Y zero line
            myPane.YAxis.MajorGrid.IsZeroLine = true;
            // Align the Y axis labels so they are flush to the axis
            myPane.YAxis.Scale.Align = AlignP.Inside;
            // Manually set the axis range
            //myPane.YAxis.Scale.Min = -1;
            //myPane.YAxis.Scale.Max = 1;

            // Fill the axis background with a gradient
            //myPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);

            // Sample at 50ms intervals
            ZedGraphTimer.Interval = 100;
            //timer1.Enabled = true;
            //timer1.Start();


            // Calculate the Axis Scale Ranges
            zgc.AxisChange();

            tickStart = Environment.TickCount;


        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                // Make sure that the curvelist has at least one curve
                if (zg1.GraphPane.CurveList.Count <= 0)
                    return;

                // Get the first CurveItem in the graph
                LineItem curve = zg1.GraphPane.CurveList[0] as LineItem;
                if (curve == null)
                    return;

                // Get the PointPairList
                IPointListEdit list = curve.Points as IPointListEdit;
                // If this is null, it means the reference at curve.Points does not
                // support IPointListEdit, so we won't be able to modify it
                if (list == null)
                    return;

                // Time is measured in seconds
                double time = (Environment.TickCount - tickStart) / 1000.0;

                // Keep the X scale at a rolling 30 second interval, with one
                // major step between the max X value and the end of the axis
                Scale xScale = zg1.GraphPane.XAxis.Scale;
                if (time > xScale.Max - xScale.MajorStep)
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = xScale.Max - 10.0;
                }

                // Make sure the Y axis is rescaled to accommodate actual data
                zg1.AxisChange();

                // Force a redraw

                zg1.Invalidate();
            }
            catch { }

        }

        private void FlightData_FormClosing(object sender, FormClosingEventArgs e)
        {
            ZedGraphTimer.Stop();
            threadrun = 0;
            try
            {
                if (MainV2.comPort.BaseStream.IsOpen)
                {
                    MainV2.comPort.Close();
                }
            }
            catch { }
        }

        private void BUT_clear_track_Click(object sender, EventArgs e)
        {
            if (route != null)
                route.Points.Clear();
        }

        private void BUTactiondo_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
#if MAVLINK10
                MainV2.comPort.doCommand((MAVLink.MAV_CMD)Enum.Parse(typeof(MAVLink.MAV_CMD), CMB_action.Text), 1, 0, 1, 0, 0, 0, 0);
#else
                comPort.doAction((MAVLink.MAV_ACTION)Enum.Parse(typeof(MAVLink.MAV_ACTION), "MAV_ACTION_" + CMB_action.Text));
#endif
            }
            catch { CustomMessageBox.Show("The Command failed to execute"); }
            ((Button)sender).Enabled = true;
        }

        private void BUTrestartmission_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                //MainV2.comPort.setWPCurrent(1); // set nav to
                MainV2.comPort.setWPCurrent(0); // set nav to
            }
            //catch { CustomMessageBox.Show("The command failed to execute"); }
            catch { CustomMessageBox.Show("The command failed to execute", "Error"); }
            ((Button)sender).Enabled = true;
        }

        private void FlightData_Resize(object sender, EventArgs e)
        {
            //Gspeed;
            //Galt;
            //Gheading;
            //attitudeIndicatorInstrumentControl1;
        }

        private void CB_tuning_CheckedChanged(object sender, EventArgs e)
        {
            if (CB_tuning.Checked)
            {
                splitContainer1.Panel1Collapsed = false;
                ZedGraphTimer.Enabled = true;
                ZedGraphTimer.Start();
                zg1.Visible = true;
                zg1.Refresh();
            }
            else
            {
                splitContainer1.Panel1Collapsed = true;
                ZedGraphTimer.Enabled = false;
                ZedGraphTimer.Stop();
                zg1.Visible = false;
            }
        }

        private void BUT_RAWSensor_Click(object sender, EventArgs e)
        {
            Form temp = new RAW_Sensor();
            ThemeManager.ApplyThemeTo(temp);
            temp.Show();
        }

        private void gMapControl1_Click(object sender, EventArgs e)
        {

        }

        PointLatLng gotolocation = new PointLatLng();

        private void gMapControl1_MouseDown(object sender, MouseEventArgs e)
        {
            gotolocation = gMapControl1.FromLocalToLatLng(e.X, e.Y);
        }

        private void goHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
            {
                CustomMessageBox.Show("Please Connect First");
                return;
            }

            if (MainV2.comPort.MAV.GuidedMode.z == 0)
            {
                flyToHereAltToolStripMenuItem_Click(null, null);

                if (MainV2.comPort.MAV.GuidedMode.z == 0)
                    return;
            }

            if (gotolocation.Lat == 0 || gotolocation.Lng == 0)
            {
                CustomMessageBox.Show("Bad Lat/Long");
                return;
            }

            Locationwp gotohere = new Locationwp();

            gotohere.id = (byte)MAVLink.MAV_CMD.WAYPOINT;
            gotohere.alt = (float)(MainV2.comPort.MAV.GuidedMode.z); // back to m
            gotohere.lat = (float)(gotolocation.Lat);
            gotohere.lng = (float)(gotolocation.Lng);

            try
            {
                MainV2.comPort.setGuidedModeWP(gotohere);
            }
            catch (Exception ex) { MainV2.comPort.giveComport = false; CustomMessageBox.Show("Error sending command : " + ex.Message); }

        }

        private void Zoomlevel_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (gMapControl1.MaxZoom + 1 == (double)Zoomlevel.Value)
                {
                    gMapControl1.Zoom = (double)Zoomlevel.Value - .1;
                }
                else
                {
                    gMapControl1.Zoom = (double)Zoomlevel.Value;
                }
            }
            catch { }
        }

        private void gMapControl1_MouseMove(object sender, MouseEventArgs e)
        {
            PointLatLng point = gMapControl1.FromLocalToLatLng(e.X, e.Y);

            if (e.Button == MouseButtons.Left)
            {
                double latdif = gotolocation.Lat - point.Lat;
                double lngdif = gotolocation.Lng - point.Lng;

                try
                {
                    gMapControl1.Position = new PointLatLng(gMapControl1.Position.Lat + latdif, gMapControl1.Position.Lng + lngdif);
                }
                catch { }
            }
            else
            {
                // setup a ballon with home distance
                if (marker != null)
                {
                    if (routes.Markers.Contains(marker))
                        routes.Markers.Remove(marker);
                }

                if (MainV2.getConfig("CHK_disttohomeflightdata") != false.ToString())
                {
                    marker = new GMapMarkerRect(point);
                    marker.ToolTip = new GMapToolTip(marker);
                    marker.ToolTipMode = MarkerTooltipMode.Always;
                    marker.ToolTipText = "Dist to Home: " + ((gMapControl1.Manager.GetDistance(point, MainV2.comPort.MAV.cs.HomeLocation.Point()) * 1000) * MainV2.comPort.MAV.cs.multiplierdist).ToString("0");

                    routes.Markers.Add(marker);
                }
            }
        }

        private void FlightData_ParentChanged(object sender, EventArgs e)
        {
            if (MainV2.cam != null)
            {
                MainV2.cam.camimage += new WebCamService.CamImage(cam_camimage);
            }

            // QUAD
            if (MainV2.comPort.MAV.param.ContainsKey("WP_SPEED_MAX"))
            {
                modifyandSetSpeed.Value = (decimal)((float)MainV2.comPort.MAV.param["WP_SPEED_MAX"] / 100.0);
            } // plane with airspeed
            else if (MainV2.comPort.MAV.param.ContainsKey("TRIM_ARSPD_CM") && MainV2.comPort.MAV.param.ContainsKey("ARSPD_ENABLE")
                && MainV2.comPort.MAV.param.ContainsKey("ARSPD_USE") && (float)MainV2.comPort.MAV.param["ARSPD_ENABLE"] == 1
                && (float)MainV2.comPort.MAV.param["ARSPD_USE"] == 1)
            {
                modifyandSetSpeed.Value = (decimal)((float)MainV2.comPort.MAV.param["TRIM_ARSPD_CM"] / 100.0);
            } // plane without airspeed
            else if (MainV2.comPort.MAV.param.ContainsKey("TRIM_THROTTLE") && MainV2.comPort.MAV.param.ContainsKey("ARSPD_USE")
                && (float)MainV2.comPort.MAV.param["ARSPD_USE"] == 0)
            {
                modifyandSetSpeed.Value = (decimal)(float)MainV2.comPort.MAV.param["TRIM_THROTTLE"]; // percent
            }
        }

        void cam_camimage(Image camimage)
        {
            hud1.bgimage = camimage;
        }

        private void BUT_Homealt_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.MAV.cs.altoffsethome != 0)
            {
                MainV2.comPort.MAV.cs.altoffsethome = 0;
            }
            else
            {
                MainV2.comPort.MAV.cs.altoffsethome = MainV2.comPort.MAV.cs.alt / MainV2.comPort.MAV.cs.multiplierdist;
            }
        }

        private void gMapControl1_Resize(object sender, EventArgs e)
        {
            gMapControl1.Zoom = gMapControl1.Zoom + 0.01;
        }

        private void BUT_loadtelem_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.logplaybackfile != null)
            {
                try
                {
                    MainV2.comPort.logplaybackfile.Close();
                    MainV2.comPort.logplaybackfile = null;
                }
                catch { }
            }

            OpenFileDialog fd = new OpenFileDialog();
            fd.AddExtension = true;
            fd.Filter = "Ardupilot Telemtry log (*.tlog)|*.tlog|Mavlink Log (*.mavlog)|*.mavlog";
            fd.InitialDirectory = MainV2.LogDir;
            fd.DefaultExt = ".tlog";
            DialogResult result = fd.ShowDialog();
            string file = fd.FileName;
            if (file != "")
            {
                try
                {
                    BUT_clear_track_Click(sender, e);

                    MainV2.comPort.logreadmode = false;
                    MainV2.comPort.logplaybackfile = new BinaryReader(File.OpenRead(file));
                    MainV2.comPort.lastlogread = DateTime.MinValue;

                    tracklog.Value = 0;
                    tracklog.Minimum = 0;
                    tracklog.Maximum = 100;
                }
                catch { CustomMessageBox.Show("Error: Failed to write log file"); }
            }
        }

        private void BUT_playlog_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.logreadmode)
            {
                MainV2.comPort.logreadmode = false;
                ZedGraphTimer.Stop();
                playingLog = false;
            }
            else
            {
                // BUT_clear_track_Click(sender, e);
                MainV2.comPort.logreadmode = true;
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
                list5.Clear();
                list6.Clear();
                list7.Clear();
                list8.Clear();
                list9.Clear();
                list10.Clear();
                tickStart = Environment.TickCount;

                zg1.GraphPane.XAxis.Scale.Min = 0;
                zg1.GraphPane.XAxis.Scale.Max = 1;
                ZedGraphTimer.Start();
                playingLog = true;
            }
        }

        private void tracklog_Scroll(object sender, EventArgs e)
        {
            try
            {
                BUT_clear_track_Click(sender, e);

                MainV2.comPort.lastlogread = DateTime.MinValue;

                if (MainV2.comPort.logplaybackfile != null)
                    MainV2.comPort.logplaybackfile.BaseStream.Position = (long)(MainV2.comPort.logplaybackfile.BaseStream.Length * (tracklog.Value / 100.0));

                updateLogPlayPosition();
            }
            catch { } // ignore any invalid 
        }

        private void tabPage1_Resize(object sender, EventArgs e)
        {
            int mywidth, myheight;

            // localize it
            Control tabGauges = sender as Control;

            float scale = tabGauges.Width / (float)tabGauges.Height;

            if (scale > 0.5 && scale < 1.9)
            {// square
                Gvspeed.Visible = true;

                if (tabGauges.Height < tabGauges.Width)
                    myheight = tabGauges.Height / 2;
                else
                    myheight = tabGauges.Width / 2;

                Gvspeed.Height = myheight;
                Gspeed.Height = myheight;
                Galt.Height = myheight;
                Gheading.Height = myheight;

                Gvspeed.Location = new Point(0, 0);
                Gspeed.Location = new Point(Gvspeed.Right, 0);


                Galt.Location = new Point(0, Gspeed.Bottom);
                Gheading.Location = new Point(Galt.Right, Gspeed.Bottom);

                return;
            }

            if (tabGauges.Width < 500)
            {
                Gvspeed.Visible = false;
                mywidth = tabGauges.Width / 3;

                Gspeed.Height = mywidth;
                Galt.Height = mywidth;
                Gheading.Height = mywidth;

                Gspeed.Location = new Point(0, 0);
            }
            else
            {
                Gvspeed.Visible = true;
                mywidth = tabGauges.Width / 4;

                Gvspeed.Height = mywidth;
                Gspeed.Height = mywidth;
                Galt.Height = mywidth;
                Gheading.Height = mywidth;

                Gvspeed.Location = new Point(0, 0);
                Gspeed.Location = new Point(Gvspeed.Right, 0);
            }

            Galt.Location = new Point(Gspeed.Right, 0);
            Gheading.Location = new Point(Galt.Right, 0);

        }

        private void BUT_setmode_Click(object sender, EventArgs e)
        {
            MainV2.comPort.setMode(CMB_modes.Text);
        }

        private void BUT_setwp_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                MainV2.comPort.setWPCurrent((ushort)CMB_setwp.SelectedIndex); // set nav to
            }
            catch { CustomMessageBox.Show("The command failed to execute"); }
            ((Button)sender).Enabled = true;
        }

        private void CMB_setwp_Click(object sender, EventArgs e)
        {
            CMB_setwp.Items.Clear();

            CMB_setwp.Items.Add("0 (Home)");

            if (MainV2.comPort.MAV.param["CMD_TOTAL"] != null)
            {
                int wps = int.Parse(MainV2.comPort.MAV.param["CMD_TOTAL"].ToString());
                for (int z = 1; z <= wps; z++)
                {
                    CMB_setwp.Items.Add(z.ToString());
                }
            }

            if (MainV2.comPort.MAV.param["WP_TOTAL"] != null)
            {
                int wps = int.Parse(MainV2.comPort.MAV.param["WP_TOTAL"].ToString());
                for (int z = 1; z <= wps; z++)
                {
                    CMB_setwp.Items.Add(z.ToString());
                }
            }
        }

        private void BUT_quickauto_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                MainV2.comPort.setMode("Auto");
            }
            //catch { CustomMessageBox.Show("The Command failed to execute"); }
            catch { CustomMessageBox.Show("The Command failed to execute", "Error"); }
            ((Button)sender).Enabled = true;
        }

        private void BUT_quickrtl_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                MainV2.comPort.setMode("RTL");
            }
            catch { CustomMessageBox.Show("The Command failed to execute"); }
            ((Button)sender).Enabled = true;
        }

        private void BUT_quickmanual_Click(object sender, EventArgs e)
        {
            try
            {
                ((Button)sender).Enabled = false;
                if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduPlane || MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.Ateryx)
                    MainV2.comPort.setMode("Manual");
                if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduCopter2)
                    MainV2.comPort.setMode("Stabilize");

            }
            catch { CustomMessageBox.Show("The Command failed to execute"); }
            ((Button)sender).Enabled = true;
        }

        private void BUT_log2kml_Click(object sender, EventArgs e)
        {
            Form frm = new MavlinkLog();
            ThemeManager.ApplyThemeTo(frm);
            frm.ShowDialog();
        }

        private void BUT_joystick_Click(object sender, EventArgs e)
        {
            Form joy = new JoystickSetup();
            ThemeManager.ApplyThemeTo(joy);
            joy.Show();
        }

        private void CMB_modes_Click(object sender, EventArgs e)
        {
            CMB_modes.DataSource = Common.getModesList();
            CMB_modes.ValueMember = "Key";
            CMB_modes.DisplayMember = "Value";
        }

        private void hud1_DoubleClick(object sender, EventArgs e)
        {
            if (huddropout)
                return;

            SubMainLeft.Panel1Collapsed = true;
            Form dropout = new Form();
            dropout.Size = new System.Drawing.Size(hud1.Width, hud1.Height + 20);
            SubMainLeft.Panel1.Controls.Remove(hud1);
            dropout.Controls.Add(hud1);
            dropout.Resize += new EventHandler(dropout_Resize);
            dropout.FormClosed += new FormClosedEventHandler(dropout_FormClosed);
            dropout.Show();
            huddropout = true;
        }

        void dropout_FormClosed(object sender, FormClosedEventArgs e)
        {
            //GetFormFromGuid(GetOrCreateGuid("fd_hud_guid")).Controls.Add(hud1);
            SubMainLeft.Panel1.Controls.Add(hud1);
            SubMainLeft.Panel1Collapsed = false;
            huddropout = false;
        }

        void dropout_Resize(object sender, EventArgs e)
        {
            if (huddropoutresize)
                return;

            huddropoutresize = true;

            int hudw = hud1.Width;
            int hudh = hud1.Height;

            int formh = ((Form)sender).Height - 30;
            int formw = ((Form)sender).Width;

            if (((Form)sender).Height < hudh)
            {
                if (((Form)sender).WindowState == FormWindowState.Maximized)
                {
                    Point tl = ((Form)sender).DesktopLocation;
                    ((Form)sender).WindowState = FormWindowState.Normal;
                    ((Form)sender).Location = tl;
                }
                ((Form)sender).Width = (int)(formh * 1.333f);
                ((Form)sender).Height = (int)(formh) + 20;
            }
            hud1.Refresh();
            huddropoutresize = false;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {



            if (tabControl1.SelectedTab == tabStatus)
            {
                tabStatus_Resize(sender, e);
            }
            else
            {
                foreach (Control temp in tabStatus.Controls)
                {
                    //   temp.DataBindings.Clear();
                    //  temp.Dispose();
                    //  tabStatus.Controls.Remove(temp);
                }

                if (tabControl1.SelectedTab == tabQuick)
                {

                }
            }

        }

        private void Gspeed_DoubleClick(object sender, EventArgs e)
        {
            string max = "60";
            if (DialogResult.OK == Common.InputBox("Enter Max", "Enter Max Speed", ref max))
            {
                Gspeed.MaxValue = float.Parse(max);
                MainV2.config["GspeedMAX"] = Gspeed.MaxValue.ToString();
            }
        }

        private void recordHudToAVIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopRecordToolStripMenuItem_Click(sender, e);

            CustomMessageBox.Show("Output avi will be saved to the log folder");

            aviwriter = new AviWriter();
            Directory.CreateDirectory(MainV2.LogDir);
            aviwriter.avi_start(MainV2.LogDir + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".avi");

            recordHudToAVIToolStripMenuItem.Text = "Recording";
        }

        private void stopRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            recordHudToAVIToolStripMenuItem.Text = "Start Recording";

            if (aviwriter != null)
                aviwriter.avi_close();

            aviwriter = null;
        }

        bool setupPropertyInfo(ref System.Reflection.PropertyInfo input, string name, object source)
        {
            Type test = source.GetType();

            foreach (var field in test.GetProperties())
            {
                if (field.Name == name)
                {
                    input = field;
                    return true;
                }
            }

            return false;
        }

        private void zg1_DoubleClick(object sender, EventArgs e)
        {
            Form selectform = new Form()
            {
                Name = "select",
                Width = 50,
                Height = 250,
                Text = "Graph This"
            };

            int x = 10;
            int y = 10;

            {
                CheckBox chk_box = new CheckBox();
                chk_box.Text = "Logarithmic";
                chk_box.Name = "Logarithmic";
                chk_box.Location = new Point(x, y);
                chk_box.Size = new System.Drawing.Size(100, 20);
                chk_box.CheckedChanged += new EventHandler(chk_log_CheckedChanged);

                selectform.Controls.Add(chk_box);
            }

            y += 20;

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            foreach (var field in test.GetProperties())
            {
                // field.Name has the field's name.
                object fieldValue;
                try
                {
                    fieldValue = field.GetValue(thisBoxed, null); // Get value
                }
                catch { continue; }

                // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                TypeCode typeCode = Type.GetTypeCode(fieldValue.GetType());

                if (!(typeCode == TypeCode.Single))
                    continue;

                CheckBox chk_box = new CheckBox();

                if (list1item != null && list1item.Name == field.Name)
                    chk_box.Checked = true;
                if (list2item != null && list2item.Name == field.Name)
                    chk_box.Checked = true;
                if (list3item != null && list3item.Name == field.Name)
                    chk_box.Checked = true;
                if (list4item != null && list4item.Name == field.Name)
                    chk_box.Checked = true;
                if (list5item != null && list5item.Name == field.Name)
                    chk_box.Checked = true;
                if (list6item != null && list6item.Name == field.Name)
                    chk_box.Checked = true;
                if (list7item != null && list7item.Name == field.Name)
                    chk_box.Checked = true;
                if (list8item != null && list8item.Name == field.Name)
                    chk_box.Checked = true;
                if (list9item != null && list9item.Name == field.Name)
                    chk_box.Checked = true;
                if (list10item != null && list10item.Name == field.Name)
                    chk_box.Checked = true;

                chk_box.Text = field.Name;
                chk_box.Name = field.Name;
                chk_box.Location = new Point(x, y);
                chk_box.Size = new System.Drawing.Size(100, 20);
                chk_box.CheckedChanged += new EventHandler(chk_box_CheckedChanged);

                selectform.Controls.Add(chk_box);

                Application.DoEvents();

                x += 0;
                y += 20;

                if (y > selectform.Height - 50)
                {
                    x += 100;
                    y = 10;

                    selectform.Width = x + 100;
                }
            }
            ThemeManager.ApplyThemeTo(selectform);
            selectform.Show();
        }

        private void hud_UserItem(object sender, EventArgs e)
        {

            Form selectform = new Form()
            {
                Name = "select",
                Width = 50,
                Height = 250,
                Text = "Display This"
            };

            int x = 10;
            int y = 10;

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            foreach (var field in test.GetProperties())
            {
                // field.Name has the field's name.
                object fieldValue;
                try
                {
                    fieldValue = field.GetValue(thisBoxed, null); // Get value
                }
                catch { continue; }

                // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                TypeCode typeCode = Type.GetTypeCode(fieldValue.GetType());

                if (!(typeCode == TypeCode.Single))
                    continue;

                CheckBox chk_box = new CheckBox();

                chk_box.Text = field.Name;
                chk_box.Name = field.Name;
                chk_box.Tag = (sender);
                chk_box.Location = new Point(x, y);
                chk_box.Size = new System.Drawing.Size(100, 20);
                if (hud1.CustomItems.ContainsKey(field.Name))
                {
                    chk_box.Checked = true;
                }

                chk_box.CheckedChanged += chk_box_hud_UserItem_CheckedChanged;

                selectform.Controls.Add(chk_box);

                Application.DoEvents();

                x += 0;
                y += 20;

                if (y > selectform.Height - 50)
                {
                    x += 100;
                    y = 10;

                    selectform.Width = x + 100;
                }
            }
            ThemeManager.ApplyThemeTo(selectform);
            selectform.Show();
        }

        void addHudUserItem(ref HUD.Custom cust, CheckBox sender)
        {
            setupPropertyInfo(ref cust.Item, (sender).Name, MainV2.comPort.MAV.cs);

            hud1.CustomItems.Add((sender).Name, cust);

            hud1.Invalidate();
        }

        void chk_box_hud_UserItem_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                HUD.Custom cust = new HUD.Custom();

                string prefix = ((CheckBox)sender).Name + ": ";
                if (MainV2.config["hud1_useritem_" + ((CheckBox)sender).Name] != null)
                    prefix = (string)MainV2.config["hud1_useritem_" + ((CheckBox)sender).Name];

                Common.InputBox("Header", "Please enter your item prefix", ref prefix);

                MainV2.config["hud1_useritem_" + ((CheckBox)sender).Name] = prefix;

                cust.Header = prefix;

                addHudUserItem(ref cust, (CheckBox)sender);
            }
            else
            {
                if (hud1.CustomItems.ContainsKey(((CheckBox)sender).Name))
                {
                    hud1.CustomItems.Remove(((CheckBox)sender).Name);
                }
                MainV2.config.Remove("hud1_useritem_" + ((CheckBox)sender).Name);
                hud1.Invalidate();
            }
        }

        void chk_log_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                zg1.GraphPane.YAxis.Type = AxisType.Log;
            }
            else
            {
                zg1.GraphPane.YAxis.Type = AxisType.Linear;
            }
        }

        void chk_box_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                if (list1item == null)
                {
                    if (setupPropertyInfo(ref list1item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list1.Clear();
                        list1curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list1, Color.Red, SymbolType.None);
                    }
                }
                else if (list2item == null)
                {
                    if (setupPropertyInfo(ref list2item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list2.Clear();
                        list2curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list2, Color.Blue, SymbolType.None);
                    }
                }
                else if (list3item == null)
                {
                    if (setupPropertyInfo(ref list3item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list3.Clear();
                        list3curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list3, Color.Green, SymbolType.None);
                    }
                }
                else if (list4item == null)
                {
                    if (setupPropertyInfo(ref list4item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list4.Clear();
                        list4curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list4, Color.Orange, SymbolType.None);
                    }
                }
                else if (list5item == null)
                {
                    if (setupPropertyInfo(ref list5item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list5.Clear();
                        list5curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list5, Color.Yellow, SymbolType.None);
                    }
                }
                else if (list6item == null)
                {
                    if (setupPropertyInfo(ref list6item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list6.Clear();
                        list6curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list6, Color.Magenta, SymbolType.None);
                    }
                }
                else if (list7item == null)
                {
                    if (setupPropertyInfo(ref list7item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list7.Clear();
                        list7curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list7, Color.Purple, SymbolType.None);
                    }
                }
                else if (list8item == null)
                {
                    if (setupPropertyInfo(ref list8item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list8.Clear();
                        list8curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list8, Color.LimeGreen, SymbolType.None);
                    }
                }
                else if (list9item == null)
                {
                    if (setupPropertyInfo(ref list9item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list9.Clear();
                        list9curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list9, Color.Cyan, SymbolType.None);
                    }
                }
                else if (list10item == null)
                {
                    if (setupPropertyInfo(ref list10item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list10.Clear();
                        list10curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list10, Color.Violet, SymbolType.None);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Max 10 at a time.");
                    ((CheckBox)sender).Checked = false;
                }
                ThemeManager.ApplyThemeTo(this);

                string selected = "";
                try
                {
                    selected = selected + zg1.GraphPane.CurveList[0].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[1].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[2].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[3].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[4].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[5].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[6].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[7].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[8].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[9].Label.Text + "|";
                    selected = selected + zg1.GraphPane.CurveList[10].Label.Text + "|";
                }
                catch { }
                MainV2.config["Tuning_Graph_Selected"] = selected;
            }
            else
            {
                // reset old stuff
                if (list1item != null && list1item.Name == ((CheckBox)sender).Name)
                {
                    list1item = null;
                    zg1.GraphPane.CurveList.Remove(list1curve);
                }
                if (list2item != null && list2item.Name == ((CheckBox)sender).Name)
                {
                    list2item = null;
                    zg1.GraphPane.CurveList.Remove(list2curve);
                }
                if (list3item != null && list3item.Name == ((CheckBox)sender).Name)
                {
                    list3item = null;
                    zg1.GraphPane.CurveList.Remove(list3curve);
                }
                if (list4item != null && list4item.Name == ((CheckBox)sender).Name)
                {
                    list4item = null;
                    zg1.GraphPane.CurveList.Remove(list4curve);
                }
                if (list5item != null && list5item.Name == ((CheckBox)sender).Name)
                {
                    list5item = null;
                    zg1.GraphPane.CurveList.Remove(list5curve);
                }
                if (list6item != null && list6item.Name == ((CheckBox)sender).Name)
                {
                    list6item = null;
                    zg1.GraphPane.CurveList.Remove(list6curve);
                }
                if (list7item != null && list7item.Name == ((CheckBox)sender).Name)
                {
                    list7item = null;
                    zg1.GraphPane.CurveList.Remove(list7curve);
                }
                if (list8item != null && list8item.Name == ((CheckBox)sender).Name)
                {
                    list8item = null;
                    zg1.GraphPane.CurveList.Remove(list8curve);
                }
                if (list9item != null && list9item.Name == ((CheckBox)sender).Name)
                {
                    list9item = null;
                    zg1.GraphPane.CurveList.Remove(list9curve);
                }
                if (list10item != null && list10item.Name == ((CheckBox)sender).Name)
                {
                    list10item = null;
                    zg1.GraphPane.CurveList.Remove(list10curve);
                }
            }
        }

        private void pointCameraHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
            {
                CustomMessageBox.Show("Please Connect First");
                return;
            }

            string alt = (100 * MainV2.comPort.MAV.cs.multiplierdist).ToString("0");
            Common.InputBox("Enter Alt", "Enter Target Alt (absolute)", ref alt);

            int intalt = (int)(100 * MainV2.comPort.MAV.cs.multiplierdist);
            if (!int.TryParse(alt, out intalt))
            {
                CustomMessageBox.Show("Bad Alt");
                return;
            }

            if (gotolocation.Lat == 0 || gotolocation.Lng == 0)
            {
                CustomMessageBox.Show("Bad Lat/Long");
                return;
            }

            MainV2.comPort.setMountConfigure(MAVLink.MAV_MOUNT_MODE.GPS_POINT, true, true, true);
            MainV2.comPort.setMountControl(gotolocation.Lat, gotolocation.Lng, (int)(intalt / MainV2.comPort.MAV.cs.multiplierdist), true);

        }

        private void BUT_script_Click(object sender, EventArgs e)
        {

            System.Threading.Thread t11 = new System.Threading.Thread(new System.Threading.ThreadStart(ScriptStart))
            {
                IsBackground = true,
                Name = "Script Thread"
            };
            t11.Start();
        }

        void ScriptStart()
        {
            string myscript = @"
# cs.???? = currentstate, any variable on the status tab in the planner can be used.
# Script = options are 
# Script.Sleep(ms)
# Script.ChangeParam(name,value)
# Script.GetParam(name)
# Script.ChangeMode(mode) - same as displayed in mode setup screen 'AUTO'
# Script.WaitFor(string,timeout)
# Script.SendRC(channel,pwm,sendnow)
# 

print 'Start Script'
for chan in range(1,9):
    Script.SendRC(chan,1500,False)
Script.SendRC(3,Script.GetParam('RC3_MIN'),True)

Script.Sleep(5000)
while cs.lat == 0:
    print 'Waiting for GPS'
    Script.Sleep(1000)
print 'Got GPS'
jo = 10 * 13
print jo
Script.SendRC(3,1000,False)
Script.SendRC(4,2000,True)
cs.messages.Clear()
Script.WaitFor('ARMING MOTORS',30000)
Script.SendRC(4,1500,True)
print 'Motors Armed!'

Script.SendRC(3,1700,True)
while cs.alt < 50:
    Script.Sleep(50)

Script.SendRC(5,2000,True) # acro

Script.SendRC(1,2000,False) # roll
Script.SendRC(3,1370,True) # throttle
while cs.roll > -45: # top hald 0 - 180
    Script.Sleep(5)
while cs.roll < -45: # -180 - -45
    Script.Sleep(5)

Script.SendRC(5,1500,False) # stabalise
Script.SendRC(1,1500,True) # level roll
Script.Sleep(2000) # 2 sec to stabalise
Script.SendRC(3,1300,True) # throttle back to land

thro = 1350 # will decend

while cs.alt > 0.1:
    Script.Sleep(300)

Script.SendRC(3,1000,False)
Script.SendRC(4,1000,True)
Script.WaitFor('DISARMING MOTORS',30000)
Script.SendRC(4,1500,True)

print 'Roll complete'

";

            //  CustomMessageBox.Show("This is Very ALPHA");

            Form scriptedit = new Form();

            scriptedit.Size = new System.Drawing.Size(500, 500);

            TextBox tb = new TextBox();

            tb.Dock = DockStyle.Fill;

            tb.ScrollBars = ScrollBars.Both;
            tb.Multiline = true;

            tb.Location = new Point(0, 0);
            tb.Size = new System.Drawing.Size(scriptedit.Size.Width - 30, scriptedit.Size.Height - 30);

            scriptedit.Controls.Add(tb);

            tb.Text = myscript;

            scriptedit.ShowDialog();

            if (DialogResult.Yes == CustomMessageBox.Show("Run Script", "Run this script?", MessageBoxButtons.YesNo))
            {

                Script scr = new Script();

                scr.runScript(tb.Text);
            }
        }

        private void CHK_autopan_CheckedChanged(object sender, EventArgs e)
        {
            MainV2.config["CHK_autopan"] = CHK_autopan.Checked.ToString();
        }

        private void NUM_playbackspeed_Scroll(object sender, EventArgs e)
        {
            LogPlayBackSpeed = NUM_playbackspeed.Value;
            lbl_playbackspeed.Text = "x " + LogPlayBackSpeed;
        }

        private void setMJPEGSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string url = MainV2.config["mjpeg_url"] != null ? MainV2.config["mjpeg_url"].ToString() : @"http://127.0.0.1:56781/map.jpg";

            if (DialogResult.OK == Common.InputBox("Mjpeg url", "Enter the url to the mjpeg source url", ref url))
            {

                MainV2.config["mjpeg_url"] = url;

                Utilities.CaptureMJPEG.Stop();

                Utilities.CaptureMJPEG.URL = url;

                Utilities.CaptureMJPEG.OnNewImage += new EventHandler(CaptureMJPEG_OnNewImage);

                Utilities.CaptureMJPEG.runAsync();
            }
            else
            {
                Utilities.CaptureMJPEG.Stop();
            }
        }

        void CaptureMJPEG_OnNewImage(object sender, EventArgs e)
        {
            GCSViews.FlightData.myhud.bgimage = (Image)sender;
        }

        private void setAspectRatioToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hud1.SixteenXNine = !hud1.SixteenXNine;
            hud1.doResize();
        }

        private void quickView_DoubleClick(object sender, EventArgs e)
        {

            Form selectform = new Form()
            {
                Name = "select",
                Width = 50,
                Height = 250,
                Text = "Display This"
            };

            int x = 10;
            int y = 10;

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            foreach (var field in test.GetProperties())
            {
                // field.Name has the field's name.
                object fieldValue;
                try
                {
                    fieldValue = field.GetValue(thisBoxed, null); // Get value
                }
                catch { continue; }

                // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                TypeCode typeCode = Type.GetTypeCode(fieldValue.GetType());

                if (!(typeCode == TypeCode.Single))
                    continue;

                CheckBox chk_box = new CheckBox();

                // dont change to ToString() = null exception
                if (((QuickView)sender).Tag != null && ((QuickView)sender).Tag.ToString() == field.Name)
                    chk_box.Checked = true;

                chk_box.Text = field.Name;
                chk_box.Name = field.Name;
                chk_box.Tag = ((QuickView)sender);
                chk_box.Location = new Point(x, y);
                chk_box.Size = new System.Drawing.Size(100, 20);
                chk_box.CheckedChanged += new EventHandler(chk_box_quickview_CheckedChanged);

                selectform.Controls.Add(chk_box);

                Application.DoEvents();

                x += 0;
                y += 20;

                if (y > selectform.Height - 50)
                {
                    x += 100;
                    y = 10;

                    selectform.Width = x + 100;
                }
            }
            ThemeManager.ApplyThemeTo(selectform);
            selectform.Show();
        }

        void chk_box_quickview_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                // save settings
                MainV2.config[((QuickView)((CheckBox)sender).Tag).Name] = ((CheckBox)sender).Name;

                // set description
                string desc = ((CheckBox)sender).Name;
                ((QuickView)((CheckBox)sender).Tag).Tag = desc;

                desc = MainV2.comPort.MAV.cs.GetNameandUnit(desc);

                ((QuickView)((CheckBox)sender).Tag).desc = desc;

                // set databinding for value
                ((QuickView)((CheckBox)sender).Tag).DataBindings.Clear();
                ((QuickView)((CheckBox)sender).Tag).DataBindings.Add(new System.Windows.Forms.Binding("number", this.bindingSource1, ((CheckBox)sender).Name, true));

                // close selection form
                ((Form)((CheckBox)sender).Parent).Close();
            }
        }

        private void flyToHereAltToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string alt = "100";

            if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduCopter2)
            {
                alt = (10 * MainV2.comPort.MAV.cs.multiplierdist).ToString("0");
            }
            else
            {
                alt = (100 * MainV2.comPort.MAV.cs.multiplierdist).ToString("0");
            }

            if (MainV2.config.ContainsKey("guided_alt"))
                alt = MainV2.config["guided_alt"].ToString();

            if (DialogResult.Cancel == Common.InputBox("Enter Alt", "Enter Guided Mode Alt", ref alt))
                return;

            MainV2.config["guided_alt"] = alt;

            int intalt = (int)(100 * MainV2.comPort.MAV.cs.multiplierdist);
            if (!int.TryParse(alt, out intalt))
            {
                CustomMessageBox.Show("Bad Alt");
                return;
            }

            MainV2.comPort.MAV.GuidedMode.z = intalt / MainV2.comPort.MAV.cs.multiplierdist;

            if (MainV2.comPort.MAV.cs.mode == "Guided")
            {
                MainV2.comPort.setGuidedModeWP(new Locationwp() { alt = (float)MainV2.comPort.MAV.GuidedMode.z, lat = (float)MainV2.comPort.MAV.GuidedMode.x, lng = (float)MainV2.comPort.MAV.GuidedMode.y });
            }
        }

        private void flightPlannerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Control ctl in splitContainer1.Panel2.Controls)
            {
                ctl.Visible = false;
            }

            foreach (MainSwitcher.Screen sc in MainV2.View.screens)
            {
                if (sc.Name == "FlightPlanner")
                {
                    MyButton but = new MyButton() { Location = new Point(splitContainer1.Panel2.Width / 2, 0), Text = "Close" };
                    but.Click += new EventHandler(but_Click);

                    splitContainer1.Panel2.Controls.Add(but);
                    splitContainer1.Panel2.Controls.Add(sc.Control);
                    ThemeManager.ApplyThemeTo(sc.Control);

                    sc.Control.Dock = DockStyle.Fill;
                    sc.Control.Visible = true;

                    if (sc.Control is IActivate)
                    {
                        ((IActivate)(sc.Control)).Activate();
                    }

                    but.BringToFront();
                    break;
                }
            }
        }

        void but_Click(object sender, EventArgs e)
        {
            foreach (MainSwitcher.Screen sc in MainV2.View.screens)
            {
                if (sc.Name == "FlightPlanner")
                {
                    splitContainer1.Panel2.Controls.Remove(sc.Control);
                    splitContainer1.Panel2.Controls.Remove((Control)sender);
                    sc.Control.Visible = false;

                    if (sc.Control is IDeactivate)
                    {
                        ((IDeactivate)(sc.Control)).Deactivate();
                    }

                    break;
                }
            }

            foreach (Control ctl in splitContainer1.Panel2.Controls)
            {
                ctl.Visible = true;
            }
        }

        private void tabQuick_Resize(object sender, EventArgs e)
        {
            int height = ((Control)sender).Height / 6;
            quickView1.Size = new System.Drawing.Size(tabQuick.Width, height);
            quickView2.Size = new System.Drawing.Size(tabQuick.Width, height);
            quickView3.Size = new System.Drawing.Size(tabQuick.Width, height);
            quickView4.Size = new System.Drawing.Size(tabQuick.Width, height);
            quickView5.Size = new System.Drawing.Size(tabQuick.Width, height);
            quickView6.Size = new System.Drawing.Size(tabQuick.Width, height);
        }

        private void hud1_Resize(object sender, EventArgs e)
        {
            Console.WriteLine("HUD resize " + hud1.Width + " " + hud1.Height);

            SubMainLeft.SplitterDistance = hud1.Height;

            try
            {
                //  dockContainer1.SetHeight(dockhud, hud1.Height);
            }
            catch { }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //dockContainer1.Clear();
            //SetupDocking();
            this.Refresh();
        }

        private void BUT_ARM_Click(object sender, EventArgs e)
        {
            if (!MainV2.comPort.BaseStream.IsOpen)
                return;

            // arm the MAV
            try
            {
                bool ans = MainV2.comPort.doARM(!MainV2.comPort.MAV.cs.armed);
                if (ans == false)
                    CustomMessageBox.Show("Error: Arm message rejected by MAV");
            }
            catch { CustomMessageBox.Show("Error: No response from MAV"); }

        }

        private void modifyandSetAlt_Click(object sender, EventArgs e)
        {
            int newalt = (int)modifyandSetAlt.Value;
            try
            {
                MainV2.comPort.setNewWPAlt(new Locationwp() { alt = newalt / MainV2.comPort.MAV.cs.multiplierdist });
            }
            catch { CustomMessageBox.Show("Error sending new Alt"); }
            //MainV2.comPort.setNextWPTargetAlt((ushort)MainV2.comPort.MAV.cs.wpno, newalt);
        }

        private void gMapControl1_MouseLeave(object sender, EventArgs e)
        {
            if (marker != null)
            {
                try
                {
                    if (routes.Markers.Contains(marker))
                        routes.Markers.Remove(marker);
                }
                catch { }
            }
        }

        private void modifyandSetSpeed_Click(object sender, EventArgs e)
        {
            // QUAD
            if (MainV2.comPort.MAV.param.ContainsKey("WP_SPEED_MAX"))
            {
                try
                {
                    MainV2.comPort.setParam("WP_SPEED_MAX", ((float)modifyandSetSpeed.Value * 100.0f));
                }
                catch { CustomMessageBox.Show("Error sending WP_SPEED_MAX command"); }
            } // plane with airspeed
            else if (MainV2.comPort.MAV.param.ContainsKey("TRIM_ARSPD_CM") && MainV2.comPort.MAV.param.ContainsKey("ARSPD_ENABLE")
                && MainV2.comPort.MAV.param.ContainsKey("ARSPD_USE") && (float)MainV2.comPort.MAV.param["ARSPD_ENABLE"] == 1
                && (float)MainV2.comPort.MAV.param["ARSPD_USE"] == 1)
            {
                try
                {
                    MainV2.comPort.setParam("TRIM_ARSPD_CM", ((float)modifyandSetSpeed.Value * 100.0f));
                }
                catch { CustomMessageBox.Show("Error sending TRIM_ARSPD_CM command"); }
            } // plane without airspeed
            else if (MainV2.comPort.MAV.param.ContainsKey("TRIM_THROTTLE") && MainV2.comPort.MAV.param.ContainsKey("ARSPD_USE")
                && (float)MainV2.comPort.MAV.param["ARSPD_USE"] == 0)
            {
                try
                {
                    MainV2.comPort.setParam("TRIM_THROTTLE", (float)modifyandSetSpeed.Value);
                }
                catch { CustomMessageBox.Show("Error sending TRIM_THROTTLE command"); }
            }
        }

        private void modifyandSetSpeed_ParentChanged(object sender, EventArgs e)
        {
        }

        private void triggerCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MainV2.comPort.setDigicamControl(true);
            }
            catch { CustomMessageBox.Show("Error sending command"); }
        }

        private void TRK_zoom_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (gMapControl1.MaxZoom + 1 == (double)TRK_zoom.Value)
                {
                    gMapControl1.Zoom = (double)TRK_zoom.Value - .1;
                }
                else
                {
                    gMapControl1.Zoom = (double)TRK_zoom.Value;
                }
            }
            catch { }
        }


        //void myMap_OnMapTypeChanged(MapType type)
        //{
        //    comboBoxMapType.SelectedItem = mymap.MapType;

        //    TRK_zoom.Minimum = mymap.MinZoom;
        //    TRK_zoom.Maximum = mymap.MaxZoom + 0.99;

        //    if (type == MapType.CustomWMS)
        //    {
        //        string url = "";
        //        if (MainV2.config["WMSserver"] != null)
        //            url = MainV2.config["WMSserver"].ToString();
        //        Common.InputBox("WMS Server", "Enter the WMS server URL", ref url);
        //        MainV2.config["WMSserver"] = url;
        //        mymap.Manager.CustomWMSURL = url;
        //    }
        //}

        //private void comboBoxMapType_SelectedValueChanged(object sender, EventArgs e)
        //{
        //    // This routine causes startup Map to always be MapType.None ? So won't use it for now
        //    try
        //    {
        //        mymap.MapType = (MapType)comboBoxMapType.SelectedItem;
        //        //FlightPlanner.MainMap.MapType = (MapType)comboBoxMapType.SelectedItem;
        //        MainV2.config["MapType"] = comboBoxMapType.Text;
        //    }
        //    catch { CustomMessageBox.Show("Map change failed. try zooming out first."); }
        //}

        private void CB_ShowPhotos_CheckedChanged(object sender, EventArgs e)
        {
            if(CB_ShowPhotos.Checked)
                gMapControl1.Overlays.Add(photos);
            else
                gMapControl1.Overlays.Remove(photos);
            //mymap
        }

        // 20140805 DWR
        private void modifyandSetPreLaunchThrottle_Click(object sender, EventArgs e)
        {
            String CMDtext = "DO_SET_PRELAUNCH_THROTTLE";

            try
            {
                ((Button)sender).Enabled = false;
#if MAVLINK10
                MainV2.comPort.doCommand((MAVLink.MAV_CMD)Enum.Parse(typeof(MAVLink.MAV_CMD), CMDtext), (int)modifyandSetPreLaunchThrottle.Value, 0, 1, 0, 0, 0, 0);
#else
                comPort.doAction((MAVLink.MAV_ACTION)Enum.Parse(typeof(MAVLink.MAV_ACTION), "MAV_ACTION_" + CMDtext));
#endif
            }
            catch { CustomMessageBox.Show("Setting the Pre Launch Throttle PWM Command failed to execute"); }
            ((Button)sender).Enabled = true;

        }

        //private void lbl_ch4in_TextChanged(object sender, EventArgs e)
        //{
        //    //if (textBoxFlash.Enabled)
        //    //{
        //    //    textBoxFlash.ForeColor = Color.Green;
        //    //    textBoxFlash.Enabled = false;
        //    //}
        //    //else
        //    //{
        //        //photocnt++;
        //        //textBoxFlash.Enabled = true;
        //        //textBoxFlash.Text = photocnt.ToString();
        //        textBoxFlash.ForeColor = Color.Red;
        //    //}
        //}

    }
}
