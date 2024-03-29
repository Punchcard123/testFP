﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArdupilotMega.Controls.BackstageView;
using ArdupilotMega.Controls;
using System.Diagnostics;

namespace ArdupilotMega.GCSViews.ConfigurationView
{
    public partial class ConfigFailSafe : UserControl, IActivate, IDeactivate
    {
        Timer timer = new Timer();
        //

        enum thr_fs_types
        {
            Disable = 0,
            Enabled_always_RTL = 1,
            Enabled_continue_in_auto_mode = 2,
        }


        public ConfigFailSafe()
        {
            InitializeComponent();

            // setup rc update
            timer.Tick += new EventHandler(timer_Tick);
        }

        public void Deactivate()
        {
            timer.Stop();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            // update all linked controls - 10hz
            try
            {
                MainV2.comPort.MAV.cs.UpdateCurrentSettings(currentStateBindingSource);
            }
            catch { }
        }

        public void Activate()
        {
            // arducopter
            mavlinkCheckBoxfs_batt_enable.setup(1, 0, "FS_BATT_ENABLE", MainV2.comPort.MAV.param);
            mavlinkComboBox_fs_thr_enable.setup(typeof(thr_fs_types), "FS_THR_ENABLE", MainV2.comPort.MAV.param);
            mavlinkNumericUpDownfs_thr_value.setup(800, 1200, 1, 1, "FS_THR_VALUE", MainV2.comPort.MAV.param);
            mavlinkNumericUpDownlow_voltage.setup(6, 99, 1, 0.1f, "LOW_VOLT", MainV2.comPort.MAV.param, PNL_low_bat);

            // plane
            mavlinkCheckBoxthr_fs.setup(1, 0, "THR_FAILSAFE", MainV2.comPort.MAV.param, mavlinkNumericUpDownthr_fs_value);
            mavlinkNumericUpDownthr_fs_value.setup(800, 1200, 1, 1, "THR_FS_VALUE", MainV2.comPort.MAV.param);
            mavlinkCheckBoxthr_fs_action.setup(1, 0, "THR_FS_ACTION",MainV2.comPort.MAV.param);
            mavlinkCheckBoxgcs_fs.setup(1, 0, "FS_GCS_ENABL", MainV2.comPort.MAV.param);
            mavlinkCheckBoxshort_fs.setup(1, 0, "FS_SHORT_ACTN", MainV2.comPort.MAV.param);
            mavlinkCheckBoxlong_fs.setup(1, 0, "FS_LONG_ACTN", MainV2.comPort.MAV.param);
            // 20140804 DWR... Batt Failsafe Dialogue for planes
            mavlinkNumericUpDownLowVoltage.setup(6, 99, 1, 0.1f, "FS_BATT_VOLTAGE", MainV2.comPort.MAV.param, PNL_low_bat);
            mavlinkNumericUpDownLowAmp.setup(6, 9999, 1, 10.0f, "FS_BATT_MAH", MainV2.comPort.MAV.param, PNL_low_bat);

            timer.Enabled = true;
            timer.Interval = 100;
            timer.Start();

            CustomMessageBox.Show("Ensure your props are not on the Plane/Quad","FailSafe",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
        }

        private void LNK_wiki_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (MainV2.comPort.MAV.cs.firmware == MainV2.Firmwares.ArduCopter2)
            {
                Process.Start(new ProcessStartInfo("http://code.google.com/p/arducopter/wiki/AC2_Failsafe"));
            }
            else
            {
                Process.Start(new ProcessStartInfo("http://code.google.com/p/ardupilot-mega/wiki/APM2xFailsafe"));
            }
        }

        private void lbl_armed_Paint(object sender, PaintEventArgs e)
        {
            if (lbl_armed.Text == "True")
            {
                lbl_armed.Text = "Armed";
            }
            else if (lbl_armed.Text == "False")
            {
                lbl_armed.Text = "Disarmed";
            }
        }

        private void lbl_gpslock_Paint(object sender, PaintEventArgs e)
        {
            int _gpsfix = 0;
            try
            {
                _gpsfix = int.Parse(lbl_gpslock.Text);
            }
            catch { return; }
            string gps = "";

            if (_gpsfix == 0)
            {
                gps = ("GPS: No GPS");
            }
            else if (_gpsfix == 1)
            {
                gps = ("GPS: No Fix");
            }
            else if (_gpsfix == 2)
            {
                gps = ("GPS: 3D Fix");
            }
            else if (_gpsfix == 3)
            {
                gps = ("GPS: 3D Fix");
            }

            lbl_gpslock.Text = gps;
        }

        private void lbl_currentmode_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (MainV2.comPort.MAV.cs.ch3in < (float)MainV2.comPort.MAV.param["FS_THR_VALUE"])
                {
                    lbl_currentmode.ForeColor = Color.Red;
                }
                else
                {
                    lbl_currentmode.ForeColor = Color.White;
                }
            }
            catch { }
        }
    }
}
