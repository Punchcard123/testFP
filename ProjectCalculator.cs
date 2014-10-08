using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Collections;

namespace ArdupilotMega
{
    public partial class ProjectCalculator : Form
    {
        const float rad2deg = (float)(180 / Math.PI);
        const float deg2rad = (float)(1.0 / rad2deg);

        Dictionary<string, camerainfo> cameras = new Dictionary<string, camerainfo>();

        public struct surveyinfo
        {
            //public string name;
            //public float focallen;
            public int cameraOrientn;   // 0 or +/-90 or 180
            public float GSD;
            public float maxFPS;
            public float flyHeightAGL;
            public float cruiseSpeed;
            public int fwdOverlap;
            public int sideOverlap;
            public float photoSpacing;
            public float runSpacing;
        }

        public struct camerainfo
        {
            public string name;
            public string brand;
            public string model;
            public float focallen;
            public float sensorwidth;
            public float sensorheight;
            public float imagewidth;
            public float imageheight;
        }

        private static camerainfo camInfo;
        private static surveyinfo survInfo;


        #region     PublicProperties

        [Category("General")]
        [DisplayName("CameraId")]
        [Description("Camera Id")]
        public string CameraId
        {
            get { return (camInfo.name); }
            //set { camInfo.name = value; }
        }

        [Category("General")]
        [DisplayName("Brand")]
        [Description("Brand")]
        public string Brand
        {
            get { return (camInfo.brand); }
            //set { camInfo.brand = value; }
        }

        [Category("General")]
        [DisplayName("Model")]
        [Description("Model")]
        public string Model
        {
            get { return (camInfo.model); }
            //set { camInfo.model = value; }
        }

        //[Category("General")]
        //[DisplayName("SerialNum")]
        //[Description("Serial Number")]
        //public string SerialNum
        //{
        //    get { return (m_SerialNum); }
        //    set { m_SerialNum = value; }
        //}

        [Category("Geometry")]
        [DisplayName("FocalLength")]
        [Description("lens Focal Length")]
        public float FocalLength
        {
            get { return (camInfo.focallen); }
            //set { camInfo.focallen = value; }
        }

        [Category("Geometry")]
        [DisplayName("Image Width")]
        [Description("Image width (pixels)")]
        public float ImageWidth
        {
            get { return (camInfo.imagewidth); }
            //set { camInfo.imagewidth = value; }
        }

        [Category("Geometry")]
        [DisplayName("Image Height")]
        [Description("Image height (pixels)")]
        public float ImageHeight
        {
            get { return (camInfo.imageheight); }
            //set { camInfo.imageheight = value; }
        }

        [Category("Geometry")]
        [DisplayName("FormatX")]
        [Description("Format X dimension")]
        public float FormatX
        {
            get { return (camInfo.sensorwidth); }
            //set { camInfo.sensorwidth = value; }
        }

        [Category("Geometry")]
        [DisplayName("FormatY")]
        [Description("Format Y dimension")]
        public float FormatY
        {
            get { return (camInfo.sensorheight); }
            //set { camInfo.sensorheight = value; }
        }

        //[Category("Geometry")]
        //[DisplayName("SensorX")]
        //[Description("Sensor X dimension")]
        //public float SensorX
        //{
        //    get { return (m_SensorX); }
        //    set { m_SensorX = value; }
        //}

        //[Category("Geometry")]
        //[DisplayName("SensorY")]
        //[Description("Sensor Y dimension")]
        //public float SensorY
        //{
        //    get { return (m_SensorY); }
        //    set { m_SensorY = value; }
        //}

        [Category("Survey")]
        [DisplayName("cameraOrientn")]
        [Description("cameraOrientn deg")]
        public int cameraOrientn
        {
            get { return (survInfo.cameraOrientn); }
            //set { survInfo.cameraOrientn = value; }
        }

        [Category("Survey")]
        [DisplayName("GSD")]
        [Description("GSD cm/pixel")]
        public float GSD
        {
            get { return (survInfo.GSD); }
            //set { survInfo.GSD = value; }
        }

        [Category("General")]
        [DisplayName("imgFormat")]
        [Description("Image Format")]
        public string imgFormat
        {
            get 
            {
                if (radioButRaw.Checked)
                    return ("Raw");
                else
                    return ("JPeg"); 
            }
            //set { camInfo.name = value; }
        }

        [Category("Survey")]
        [DisplayName("maxFPS")]
        [Description("max FPS")]
        public float maxFPS
        {
            get { return (survInfo.maxFPS); }
            //set { survInfo.maxFPS = value; }
        }
        
        [Category("Survey")]
        [DisplayName("flyHeightAGL")]
        [Description("flyHeightAGL m")]
        public float flyHeightAGL
        {
            get { return (survInfo.flyHeightAGL); }
            //set { survInfo.flyHeightAGL = value; }
        }

        [Category("Survey")]
        [DisplayName("cruiseSpeed")]
        [Description("cruiseSpeed m/s")]
        public float cruiseSpeed
        {
            get { return (survInfo.cruiseSpeed); }
            //set { survInfo.cruiseSpeed = value; }
        }

        [Category("Survey")]
        [DisplayName("fwdOverlap")]
        [Description("fwdOverlap m")]
        public int fwdOverlap
        {
            get { return (survInfo.fwdOverlap); }
            //set { survInfo.fwdOverlap = value; }
        }

        [Category("Survey")]
        [DisplayName("sideOverlap")]
        [Description("sideOverlap m")]
        public int sideOverlap
        {
            get { return (survInfo.sideOverlap); }
            //set { survInfo.sideOverlap = value; }
        }

        [Category("Survey")]
        [DisplayName("photoSpacing")]
        [Description("photoSpacing m")]
        public float photoSpacing
        {
            get { return (survInfo.photoSpacing); }
            //set { survInfo.photoSpacing = value; }
        }

        [Category("Survey")]
        [DisplayName("runSpacing")]
        [Description("runSpacing m")]
        public float runSpacing
        {
            get { return (survInfo.runSpacing); }
            //set { survInfo.runSpacing = value; }
        }

        #endregion


        public ProjectCalculator()
        {
            InitializeComponent();

            // Need to load a default values as required

            // camerainfo
            try
            {
                camInfo.name = CMB_camera.Text.Length > 0 ? CMB_camera.Text : "Unknown";
                //brand;
                //model;
                camInfo.focallen = (float)num_focallength.Value;
                camInfo.imagewidth = int.Parse(TXT_imgwidth.Text);
                camInfo.imageheight = int.Parse(TXT_imgheight.Text);
                camInfo.sensorwidth = float.Parse(TXT_senswidth.Text);
                camInfo.sensorheight = float.Parse(TXT_sensheight.Text);
            }
            catch
            {
                CMB_camera.SelectedText = "Default";
                //brand;
                //model;
                num_focallength.Value = 35.0M;
                TXT_imgheight.Text = "3000";
                TXT_imgwidth.Text = "4000";
                TXT_sensheight.Text = "24.0";
                TXT_senswidth.Text = "36.0";  
            }


            // surveyinfo
            try
            {
                //survInfo.cameraOrientn;   // 0 or +/-90 or +/-180
                survInfo.cameraOrientn = CHK_camdirection.Checked ? 0 : 90;
                survInfo.flyHeightAGL = (float)num_agl.Value;
                survInfo.cruiseSpeed = (float)num_cruiseSpeed.Value;
                survInfo.fwdOverlap = (int)num_fwdOverlap.Value;
                survInfo.sideOverlap = (int)num_sideOverlap.Value;
                survInfo.maxFPS = float.Parse(TXT_maxFPS.Text);
                //textBoxImgFormat.Text = survInfo.imgFormat;
                //textBoxPhotoSpacing.Text = survInfo.photoSpacing.ToString();
                //textBoxGSD.Text = survInfo.GSD.ToString();
            }
            catch
            {
                //survInfo.cameraOrientn;   // 0 or +/-90 or +/-180
                CHK_camdirection.Checked = true;
                num_agl.Value = 120;
                num_cruiseSpeed.Value = 65;
                num_fwdOverlap.Value = 70;
                num_sideOverlap.Value = 60;
                survInfo.maxFPS = 1.6F;
                radioButRaw.Checked = true;
            }

            doCalc();
        }

        void doCalc()
        {
            try
            {
                // entered values
                //camInfo.name = CMB_camera.Text;
                //camera.brand = comboBoxBrand.Text;
                //camera.model = textBoxModel.Text;
                camInfo.focallen = (float)num_focallength.Value;
                camInfo.imagewidth = int.Parse(TXT_imgwidth.Text);
                camInfo.imageheight = int.Parse(TXT_imgheight.Text);
                camInfo.sensorwidth = float.Parse(TXT_senswidth.Text);
                camInfo.sensorheight = float.Parse(TXT_sensheight.Text);

                survInfo.flyHeightAGL = (float)num_agl.Value;
                survInfo.cruiseSpeed = (float)num_cruiseSpeed.Value;
                survInfo.fwdOverlap = (int)num_fwdOverlap.Value;
                survInfo.sideOverlap = (int)num_sideOverlap.Value;
                survInfo.maxFPS = float.Parse(TXT_maxFPS.Text);

                TXT_fovAH.Text = (Math.Atan(camInfo.sensorwidth / (2.0 * camInfo.focallen)) * rad2deg * 2.0).ToString();
                TXT_fovAV.Text = (Math.Atan(camInfo.sensorheight / (2.0 * camInfo.focallen)) * rad2deg * 2.0).ToString();
 
                // scale
                float flscale = 1000 * survInfo.flyHeightAGL / camInfo.focallen;
                float viewwidth = (camInfo.sensorwidth * flscale / 1000);
                float viewheight = (camInfo.sensorheight * flscale / 1000);

                TXT_fovH.Text = (viewwidth).ToString();
                TXT_fovV.Text = (viewheight).ToString();
                survInfo.GSD = (viewheight / camInfo.imageheight) * 100;
                TXT_cmpixel.Text = survInfo.GSD.ToString("0.00");   // cm");

                //survInfo.cameraOrientn;   // 0 or +/-90 or +/-180
                if (CHK_camdirection.Checked)
                {
                    survInfo.cameraOrientn = 0;
                    survInfo.photoSpacing = (1 - (survInfo.fwdOverlap / 100.0f)) * viewheight;
                    survInfo.runSpacing = (1 - (survInfo.sideOverlap / 100.0f)) * viewwidth;
                }
                else
                {
                    survInfo.cameraOrientn = 90;
                    survInfo.photoSpacing = (1 - (survInfo.fwdOverlap / 100.0f)) * viewwidth;
                    survInfo.runSpacing = (1 - (survInfo.sideOverlap / 100.0f)) * viewheight;
                }
                float ftmp = (survInfo.cruiseSpeed / 3.6F) / survInfo.maxFPS;
                if (ftmp > survInfo.photoSpacing)       // Modify Fwd photo spacing to prevent camera buffer overload
                {
                    survInfo.photoSpacing = ftmp;
                    if (CHK_camdirection.Checked)
                    {
                        survInfo.fwdOverlap = (int)(100.0f * (1.0f - survInfo.photoSpacing / viewheight) +0.5);
                        survInfo.photoSpacing = (1 - (survInfo.fwdOverlap / 100.0f)) * viewheight;
                    }
                    else
                    {
                        survInfo.fwdOverlap = (int)(100.0f * (1.0f - survInfo.photoSpacing / viewwidth) + 0.5);
                        survInfo.photoSpacing = (1 - (survInfo.fwdOverlap / 100.0f)) * viewwidth;
                    }
                    num_fwdOverlap.Value = survInfo.fwdOverlap;
                }

                TXT_distflphotos.Text = survInfo.photoSpacing.ToString();
                TXT_distacflphotos.Text = survInfo.runSpacing.ToString();
            }

            catch { return; }
        }

        private void num_agl_ValueChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void num_focallength_ValueChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void num_overlap_ValueChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void num_sidelap_ValueChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void CHK_camdirection_CheckedChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void TXT_imgwidth_TextChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void TXT_imgheight_TextChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void TXT_senswidth_TextChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void TXT_sensheight_TextChanged(object sender, EventArgs e)
        {
            doCalc();
        }

        private void CMB_camera_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cameras.ContainsKey(CMB_camera.Text))
            {
                camerainfo camera = cameras[CMB_camera.Text];
                //comboBoxBrand.Text = camera.brand;
                //textBoxModel.Text = camera.model;

                num_focallength.Value = (decimal)camera.focallen;
                TXT_imgheight.Text = camera.imageheight.ToString();
                TXT_imgwidth.Text = camera.imagewidth.ToString();
                TXT_sensheight.Text = camera.sensorheight.ToString();
                TXT_senswidth.Text = camera.sensorwidth.ToString();
                //// maxFPS may change ///
            }

            doCalc();
        }

        private void xmlcamera(bool write)
        {
            string filename = "cameras.xml";

            if (write || !File.Exists(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + filename))
            {
                try
                {
                    XmlTextWriter xmlwriter = new XmlTextWriter(Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + filename, Encoding.ASCII);
                    xmlwriter.Formatting = Formatting.Indented;

                    xmlwriter.WriteStartDocument();

                    xmlwriter.WriteStartElement("Cameras");

                    foreach (string key in cameras.Keys)
                    {
                        try
                        {
                            if (key == "")
                                continue;
                            xmlwriter.WriteStartElement("Camera");
                            xmlwriter.WriteElementString("name", cameras[key].name);
                            //xmlwriter.WriteElementString("brand", cameras[key].brand);
                            //xmlwriter.WriteElementString("model", cameras[key].model);
                            xmlwriter.WriteElementString("flen", cameras[key].focallen.ToString(new System.Globalization.CultureInfo("en-US")));
                            xmlwriter.WriteElementString("imgh", cameras[key].imageheight.ToString(new System.Globalization.CultureInfo("en-US")));
                            xmlwriter.WriteElementString("imgw", cameras[key].imagewidth.ToString(new System.Globalization.CultureInfo("en-US")));
                            xmlwriter.WriteElementString("senh", cameras[key].sensorheight.ToString(new System.Globalization.CultureInfo("en-US")));
                            xmlwriter.WriteElementString("senw", cameras[key].sensorwidth.ToString(new System.Globalization.CultureInfo("en-US")));
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
                                    case "Camera":
                                        {
                                            camerainfo camera = new camerainfo();

                                            while (xmlreader.Read())
                                            {
                                                bool dobreak = false;
                                                xmlreader.MoveToElement();
                                                switch (xmlreader.Name)
                                                {
                                                    case "name":
                                                        camera.name = xmlreader.ReadString();
                                                        break;
                                                    //case "brand":
                                                    //    camera.brand = xmlreader.ReadString();
                                                    //    break;
                                                    //case "model":
                                                    //    camera.model = xmlreader.ReadString();
                                                    //    break;
                                                    case "imgw":
                                                        camera.imagewidth = float.Parse(xmlreader.ReadString(), new System.Globalization.CultureInfo("en-US"));
                                                        break;
                                                    case "imgh":
                                                        camera.imageheight = float.Parse(xmlreader.ReadString(), new System.Globalization.CultureInfo("en-US"));
                                                        break;
                                                    case "senw":
                                                        camera.sensorwidth = float.Parse(xmlreader.ReadString(), new System.Globalization.CultureInfo("en-US"));
                                                        break;
                                                    case "senh":
                                                        camera.sensorheight = float.Parse(xmlreader.ReadString(), new System.Globalization.CultureInfo("en-US"));
                                                        break;
                                                    case "flen":
                                                        camera.focallen= float.Parse(xmlreader.ReadString(), new System.Globalization.CultureInfo("en-US"));
                                                        break;
                                                    case "Camera":
                                                        cameras.Add(camera.name,camera);
                                                        CMB_camera.Items.Add(camera.name);
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
                                        //config[xmlreader.Name] = xmlreader.ReadString();
                                        break;
                                }
                            }
                            catch (Exception ee) { Console.WriteLine(ee.Message); } // silent fail on bad entry
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Bad Camera File: " + ex.ToString()); } // bad config file
            }
        }

        private void BUT_save_Click(object sender, EventArgs e)
        {
            camerainfo camera = new camerainfo();

            // check if camera exists alreay
            if (cameras.ContainsKey(CMB_camera.Text))
            {
                camera = cameras[CMB_camera.Text];
            }
            else
            {
                cameras.Add(CMB_camera.Text, camera);
            }

            try
            {
                //camera.name = CMB_camera.Tag.ToString();
                camera.name = CMB_camera.Text;
                //camera.brand = comboBoxBrand.Text;
                //camera.model = textBoxModel.Text;
            }
            catch { CustomMessageBox.Show("Camera name is not valid"); return; }

            try
            {
                camera.focallen = (float)num_focallength.Value;
                camera.imageheight = float.Parse(TXT_imgheight.Text);
                camera.imagewidth = float.Parse(TXT_imgwidth.Text);
                camera.sensorheight = float.Parse(TXT_sensheight.Text);
                camera.sensorwidth = float.Parse(TXT_senswidth.Text);

                /////////////// ToDo add  survInfo parsing ///////////////
            }
            catch { CustomMessageBox.Show("One of your entries is not a valid number"); return; }

            cameras[CMB_camera.Text] = camera;

            xmlcamera(true);
        }

        private void Camera_Load(object sender, EventArgs e)
        {
            xmlcamera(false);
        }

        private void radioButRaw_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButRaw.Checked)
                TXT_maxFPS.Text = "1.6";
            else
                TXT_maxFPS.Text = "0.8";
            doCalc();
        }

        private void num_cruiseSpeed_ValueChanged(object sender, EventArgs e)
        {
            doCalc();        
        }

        private void TXT_cmpixel_TextChanged(object sender, EventArgs e)
        {
            try
            {
                //survInfo.GSD = float.Parse(TXT_cmpixel.Text,"0.00 cm");   // Syntax incorrect
                survInfo.GSD = float.Parse(TXT_cmpixel.Text);
                float sensorRes_mm = camInfo.sensorheight / camInfo.imageheight;
                num_agl.Value = decimal.Parse(((camInfo.focallen * survInfo.GSD / 100.0f) / sensorRes_mm).ToString());
            }
            catch { return; }

            doCalc();        
        }
    }
}