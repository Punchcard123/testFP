namespace ArdupilotMega.GCSViews	//.ConfigurationView
{
    partial class ConfigProject
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigProject));
            this.label98 = new System.Windows.Forms.Label();
            this.label97 = new System.Windows.Forms.Label();
            this.CMB_speedunits = new System.Windows.Forms.ComboBox();
            this.CMB_distunits = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_log_dir = new System.Windows.Forms.TextBox();
            this.BUT_logdirbrowse = new ArdupilotMega.Controls.MyButton();
            this.CHK_autodec = new System.Windows.Forms.CheckBox();
            this.label27 = new System.Windows.Forms.Label();
            this.linkLabelmagdec = new System.Windows.Forms.LinkLabel();
            this.label100 = new System.Windows.Forms.Label();
            this.TXT_declination = new System.Windows.Forms.TextBox();
            this.But_searchAddress = new ArdupilotMega.Controls.MyButton();
            this.txtBox_locAddress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxCamera = new System.Windows.Forms.TextBox();
            this.textBoxGSD = new System.Windows.Forms.TextBox();
            this.textBoxImgFormat = new System.Windows.Forms.TextBox();
            this.But_cameraSetup = new ArdupilotMega.Controls.MyButton();
            this.label9 = new System.Windows.Forms.Label();
            this.lineSeparator1 = new ArdupilotMega.Controls.LineSeparator();
            this.lineSeparator2 = new ArdupilotMega.Controls.LineSeparator();
            this.textBoxFwdOverlap = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.textBoxSideOverlap = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.LabelLense = new System.Windows.Forms.Label();
            this.textBoxFocal = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.textBoxCruiseSpeed = new System.Windows.Forms.TextBox();
            this.label_CameraOrientn = new System.Windows.Forms.Label();
            this.textBoxMaxFrameRate = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.textBoxAGL = new System.Windows.Forms.TextBox();
            this.textBoxPhotoSpacing = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.CMBox_aircraft = new System.Windows.Forms.ComboBox();
            this.label103 = new System.Windows.Forms.Label();
            this.numUpDown_airframeNr = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.lineSeparator3 = new ArdupilotMega.Controls.LineSeparator();
            this.CMBox_pilot = new System.Windows.Forms.ComboBox();
            this.CMBox_GSoperator = new System.Windows.Forms.ComboBox();
            this.CMBox_observer = new System.Windows.Forms.ComboBox();
            this.BUT_save = new ArdupilotMega.Controls.MyButton();
            this.But_People = new System.Windows.Forms.Button();
            this.numUpDown_Battery = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.dataGridView_People = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDown_airframeNr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDown_Battery)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_People)).BeginInit();
            this.SuspendLayout();
            // 
            // label98
            // 
            resources.ApplyResources(this.label98, "label98");
            this.label98.Name = "label98";
            // 
            // label97
            // 
            resources.ApplyResources(this.label97, "label97");
            this.label97.Name = "label97";
            // 
            // CMB_speedunits
            // 
            this.CMB_speedunits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_speedunits.FormattingEnabled = true;
            resources.ApplyResources(this.CMB_speedunits, "CMB_speedunits");
            this.CMB_speedunits.Name = "CMB_speedunits";
            this.CMB_speedunits.SelectedIndexChanged += new System.EventHandler(this.CMB_speedunits_SelectedIndexChanged);
            // 
            // CMB_distunits
            // 
            this.CMB_distunits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_distunits.FormattingEnabled = true;
            resources.ApplyResources(this.CMB_distunits, "CMB_distunits");
            this.CMB_distunits.Name = "CMB_distunits";
            this.CMB_distunits.SelectedIndexChanged += new System.EventHandler(this.CMB_distunits_SelectedIndexChanged);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // txt_log_dir
            // 
            resources.ApplyResources(this.txt_log_dir, "txt_log_dir");
            this.txt_log_dir.Name = "txt_log_dir";
            // 
            // BUT_logdirbrowse
            // 
            resources.ApplyResources(this.BUT_logdirbrowse, "BUT_logdirbrowse");
            this.BUT_logdirbrowse.Name = "BUT_logdirbrowse";
            this.BUT_logdirbrowse.UseVisualStyleBackColor = true;
            this.BUT_logdirbrowse.Click += new System.EventHandler(this.BUT_logdirbrowse_Click);
            // 
            // CHK_autodec
            // 
            resources.ApplyResources(this.CHK_autodec, "CHK_autodec");
            this.CHK_autodec.Name = "CHK_autodec";
            this.CHK_autodec.UseVisualStyleBackColor = true;
            this.CHK_autodec.CheckedChanged += new System.EventHandler(this.CHK_autodec_CheckedChanged);
            // 
            // label27
            // 
            resources.ApplyResources(this.label27, "label27");
            this.label27.Name = "label27";
            // 
            // linkLabelmagdec
            // 
            resources.ApplyResources(this.linkLabelmagdec, "linkLabelmagdec");
            this.linkLabelmagdec.Name = "linkLabelmagdec";
            this.linkLabelmagdec.TabStop = true;
            this.linkLabelmagdec.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelmagdec_LinkClicked);
            // 
            // label100
            // 
            resources.ApplyResources(this.label100, "label100");
            this.label100.Name = "label100";
            // 
            // TXT_declination
            // 
            resources.ApplyResources(this.TXT_declination, "TXT_declination");
            this.TXT_declination.Name = "TXT_declination";
            this.TXT_declination.Validated += new System.EventHandler(this.TXT_declination_Validated);
            // 
            // But_searchAddress
            // 
            resources.ApplyResources(this.But_searchAddress, "But_searchAddress");
            this.But_searchAddress.Name = "But_searchAddress";
            this.But_searchAddress.UseVisualStyleBackColor = true;
            this.But_searchAddress.Click += new System.EventHandler(this.But_searchAddress_Click);
            // 
            // txtBox_locAddress
            // 
            resources.ApplyResources(this.txtBox_locAddress, "txtBox_locAddress");
            this.txtBox_locAddress.Name = "txtBox_locAddress";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // textBoxCamera
            // 
            resources.ApplyResources(this.textBoxCamera, "textBoxCamera");
            this.textBoxCamera.Name = "textBoxCamera";
            // 
            // textBoxGSD
            // 
            resources.ApplyResources(this.textBoxGSD, "textBoxGSD");
            this.textBoxGSD.Name = "textBoxGSD";
            // 
            // textBoxImgFormat
            // 
            resources.ApplyResources(this.textBoxImgFormat, "textBoxImgFormat");
            this.textBoxImgFormat.Name = "textBoxImgFormat";
            // 
            // But_cameraSetup
            // 
            resources.ApplyResources(this.But_cameraSetup, "But_cameraSetup");
            this.But_cameraSetup.Name = "But_cameraSetup";
            this.But_cameraSetup.UseVisualStyleBackColor = true;
            this.But_cameraSetup.Click += new System.EventHandler(this.But_cameraSetup_Click);
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // lineSeparator1
            // 
            resources.ApplyResources(this.lineSeparator1, "lineSeparator1");
            this.lineSeparator1.MaximumSize = new System.Drawing.Size(2000, 2);
            this.lineSeparator1.MinimumSize = new System.Drawing.Size(0, 2);
            this.lineSeparator1.Name = "lineSeparator1";
            // 
            // lineSeparator2
            // 
            resources.ApplyResources(this.lineSeparator2, "lineSeparator2");
            this.lineSeparator2.MaximumSize = new System.Drawing.Size(2000, 2);
            this.lineSeparator2.MinimumSize = new System.Drawing.Size(0, 2);
            this.lineSeparator2.Name = "lineSeparator2";
            // 
            // textBoxFwdOverlap
            // 
            resources.ApplyResources(this.textBoxFwdOverlap, "textBoxFwdOverlap");
            this.textBoxFwdOverlap.Name = "textBoxFwdOverlap";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // textBoxSideOverlap
            // 
            resources.ApplyResources(this.textBoxSideOverlap, "textBoxSideOverlap");
            this.textBoxSideOverlap.Name = "textBoxSideOverlap";
            // 
            // label11
            // 
            resources.ApplyResources(this.label11, "label11");
            this.label11.Name = "label11";
            // 
            // LabelLense
            // 
            resources.ApplyResources(this.LabelLense, "LabelLense");
            this.LabelLense.Name = "LabelLense";
            // 
            // textBoxFocal
            // 
            resources.ApplyResources(this.textBoxFocal, "textBoxFocal");
            this.textBoxFocal.Name = "textBoxFocal";
            // 
            // label20
            // 
            resources.ApplyResources(this.label20, "label20");
            this.label20.Name = "label20";
            // 
            // textBoxCruiseSpeed
            // 
            resources.ApplyResources(this.textBoxCruiseSpeed, "textBoxCruiseSpeed");
            this.textBoxCruiseSpeed.Name = "textBoxCruiseSpeed";
            // 
            // label_CameraOrientn
            // 
            resources.ApplyResources(this.label_CameraOrientn, "label_CameraOrientn");
            this.label_CameraOrientn.Name = "label_CameraOrientn";
            // 
            // textBoxMaxFrameRate
            // 
            resources.ApplyResources(this.textBoxMaxFrameRate, "textBoxMaxFrameRate");
            this.textBoxMaxFrameRate.Name = "textBoxMaxFrameRate";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // textBoxAGL
            // 
            resources.ApplyResources(this.textBoxAGL, "textBoxAGL");
            this.textBoxAGL.Name = "textBoxAGL";
            // 
            // textBoxPhotoSpacing
            // 
            resources.ApplyResources(this.textBoxPhotoSpacing, "textBoxPhotoSpacing");
            this.textBoxPhotoSpacing.Name = "textBoxPhotoSpacing";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // CMBox_aircraft
            // 
            this.CMBox_aircraft.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMBox_aircraft.FormattingEnabled = true;
            this.CMBox_aircraft.Items.AddRange(new object[] {
            resources.GetString("CMBox_aircraft.Items"),
            resources.GetString("CMBox_aircraft.Items1"),
            resources.GetString("CMBox_aircraft.Items2")});
            resources.ApplyResources(this.CMBox_aircraft, "CMBox_aircraft");
            this.CMBox_aircraft.Name = "CMBox_aircraft";
            // 
            // label103
            // 
            resources.ApplyResources(this.label103, "label103");
            this.label103.Name = "label103";
            // 
            // numUpDown_airframeNr
            // 
            resources.ApplyResources(this.numUpDown_airframeNr, "numUpDown_airframeNr");
            this.numUpDown_airframeNr.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numUpDown_airframeNr.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numUpDown_airframeNr.Name = "numUpDown_airframeNr";
            this.numUpDown_airframeNr.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label12
            // 
            resources.ApplyResources(this.label12, "label12");
            this.label12.Name = "label12";
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // lineSeparator3
            // 
            resources.ApplyResources(this.lineSeparator3, "lineSeparator3");
            this.lineSeparator3.MaximumSize = new System.Drawing.Size(2000, 2);
            this.lineSeparator3.MinimumSize = new System.Drawing.Size(0, 2);
            this.lineSeparator3.Name = "lineSeparator3";
            // 
            // CMBox_pilot
            // 
            this.CMBox_pilot.FormattingEnabled = true;
            resources.ApplyResources(this.CMBox_pilot, "CMBox_pilot");
            this.CMBox_pilot.Name = "CMBox_pilot";
            // 
            // CMBox_GSoperator
            // 
            this.CMBox_GSoperator.FormattingEnabled = true;
            resources.ApplyResources(this.CMBox_GSoperator, "CMBox_GSoperator");
            this.CMBox_GSoperator.Name = "CMBox_GSoperator";
            // 
            // CMBox_observer
            // 
            this.CMBox_observer.FormattingEnabled = true;
            resources.ApplyResources(this.CMBox_observer, "CMBox_observer");
            this.CMBox_observer.Name = "CMBox_observer";
            // 
            // BUT_save
            // 
            resources.ApplyResources(this.BUT_save, "BUT_save");
            this.BUT_save.Name = "BUT_save";
            this.BUT_save.UseVisualStyleBackColor = true;
            this.BUT_save.Click += new System.EventHandler(this.BUT_save_Click);
            // 
            // But_People
            // 
            this.But_People.BackgroundImage = global::ArdupilotMega.Properties.Resources.three_peopleBW;
            resources.ApplyResources(this.But_People, "But_People");
            this.But_People.ForeColor = System.Drawing.SystemColors.ControlText;
            this.But_People.Name = "But_People";
            this.But_People.UseVisualStyleBackColor = true;
            this.But_People.Click += new System.EventHandler(this.But_People_Click);
            // 
            // numUpDown_Battery
            // 
            resources.ApplyResources(this.numUpDown_Battery, "numUpDown_Battery");
            this.numUpDown_Battery.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numUpDown_Battery.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numUpDown_Battery.Name = "numUpDown_Battery";
            this.numUpDown_Battery.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // dataGridView_People
            // 
            this.dataGridView_People.BackgroundColor = System.Drawing.SystemColors.Highlight;
            this.dataGridView_People.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dataGridView_People, "dataGridView_People");
            this.dataGridView_People.Name = "dataGridView_People";
            this.dataGridView_People.RowTemplate.Height = 24;
            // 
            // ConfigProject
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView_People);
            this.Controls.Add(this.numUpDown_Battery);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.But_People);
            this.Controls.Add(this.BUT_save);
            this.Controls.Add(this.CMBox_observer);
            this.Controls.Add(this.CMBox_GSoperator);
            this.Controls.Add(this.CMBox_pilot);
            this.Controls.Add(this.lineSeparator3);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.numUpDown_airframeNr);
            this.Controls.Add(this.label103);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CMBox_aircraft);
            this.Controls.Add(this.textBoxPhotoSpacing);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBoxAGL);
            this.Controls.Add(this.textBoxMaxFrameRate);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label_CameraOrientn);
            this.Controls.Add(this.textBoxCruiseSpeed);
            this.Controls.Add(this.label20);
            this.Controls.Add(this.LabelLense);
            this.Controls.Add(this.textBoxFocal);
            this.Controls.Add(this.textBoxSideOverlap);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.textBoxFwdOverlap);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lineSeparator2);
            this.Controls.Add(this.lineSeparator1);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.But_cameraSetup);
            this.Controls.Add(this.textBoxImgFormat);
            this.Controls.Add(this.textBoxGSD);
            this.Controls.Add(this.textBoxCamera);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.But_searchAddress);
            this.Controls.Add(this.txtBox_locAddress);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.CHK_autodec);
            this.Controls.Add(this.label27);
            this.Controls.Add(this.linkLabelmagdec);
            this.Controls.Add(this.label100);
            this.Controls.Add(this.TXT_declination);
            this.Controls.Add(this.BUT_logdirbrowse);
            this.Controls.Add(this.txt_log_dir);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label98);
            this.Controls.Add(this.label97);
            this.Controls.Add(this.CMB_speedunits);
            this.Controls.Add(this.CMB_distunits);
            this.Name = "ConfigProject";
            this.Load += new System.EventHandler(this.ConfigProject_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numUpDown_airframeNr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numUpDown_Battery)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_People)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label98;
        private System.Windows.Forms.Label label97;
        private System.Windows.Forms.ComboBox CMB_speedunits;
        private System.Windows.Forms.ComboBox CMB_distunits;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_log_dir;
        private Controls.MyButton BUT_logdirbrowse;
        private System.Windows.Forms.CheckBox CHK_autodec;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.LinkLabel linkLabelmagdec;
        private System.Windows.Forms.Label label100;
        private System.Windows.Forms.TextBox TXT_declination;
        private Controls.MyButton But_searchAddress;
        private System.Windows.Forms.TextBox txtBox_locAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxCamera;
        private System.Windows.Forms.TextBox textBoxGSD;
        private System.Windows.Forms.TextBox textBoxImgFormat;
        private Controls.MyButton But_cameraSetup;
        private System.Windows.Forms.Label label9;
        private Controls.LineSeparator lineSeparator1;
        private Controls.LineSeparator lineSeparator2;
        private System.Windows.Forms.TextBox textBoxFwdOverlap;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox textBoxSideOverlap;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label LabelLense;
        private System.Windows.Forms.TextBox textBoxFocal;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.TextBox textBoxCruiseSpeed;
        private System.Windows.Forms.Label label_CameraOrientn;
        private System.Windows.Forms.TextBox textBoxMaxFrameRate;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBoxAGL;
        private System.Windows.Forms.TextBox textBoxPhotoSpacing;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox CMBox_aircraft;
        private System.Windows.Forms.Label label103;
        private System.Windows.Forms.NumericUpDown numUpDown_airframeNr;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label14;
        private Controls.LineSeparator lineSeparator3;
        private System.Windows.Forms.ComboBox CMBox_pilot;
        private System.Windows.Forms.ComboBox CMBox_GSoperator;
        private System.Windows.Forms.ComboBox CMBox_observer;
        private Controls.MyButton BUT_save;
        private System.Windows.Forms.Button But_People;
        private System.Windows.Forms.NumericUpDown numUpDown_Battery;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.DataGridView dataGridView_People;
    }
}
