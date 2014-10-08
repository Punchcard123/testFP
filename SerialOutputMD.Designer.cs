namespace ArdupilotMega
{
    partial class SerialOutputMD
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SerialOutputMD));
            this.CMB_serialport = new System.Windows.Forms.ComboBox();
            this.BUT_connect = new ArdupilotMega.Controls.MyButton();
            this.CMB_baudrate = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // CMB_serialport
            // 
            this.CMB_serialport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_serialport.FormattingEnabled = true;
            this.CMB_serialport.Location = new System.Drawing.Point(17, 16);
            this.CMB_serialport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CMB_serialport.Name = "CMB_serialport";
            this.CMB_serialport.Size = new System.Drawing.Size(160, 24);
            this.CMB_serialport.TabIndex = 0;
            // 
            // BUT_connect
            // 
            this.BUT_connect.Location = new System.Drawing.Point(187, 16);
            this.BUT_connect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_connect.Name = "BUT_connect";
            this.BUT_connect.Size = new System.Drawing.Size(100, 28);
            this.BUT_connect.TabIndex = 1;
            this.BUT_connect.Text = "Connect";
            this.BUT_connect.UseVisualStyleBackColor = true;
            this.BUT_connect.Click += new System.EventHandler(this.BUT_connect_Click);
            // 
            // CMB_baudrate
            // 
            this.CMB_baudrate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CMB_baudrate.FormattingEnabled = true;
            this.CMB_baudrate.Items.AddRange(new object[] {
            "4800",
            "9600",
            "14400",
            "19200",
            "28800",
            "38400",
            "57600",
            "115200"});
            this.CMB_baudrate.Location = new System.Drawing.Point(17, 49);
            this.CMB_baudrate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CMB_baudrate.Name = "CMB_baudrate";
            this.CMB_baudrate.Size = new System.Drawing.Size(160, 24);
            this.CMB_baudrate.TabIndex = 2;
            // 
            // SerialOutputMD
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 92);
            this.Controls.Add(this.CMB_baudrate);
            this.Controls.Add(this.BUT_connect);
            this.Controls.Add(this.CMB_serialport);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SerialOutputMD";
            this.Text = "SerialOutput - Microdrone";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SerialOutput_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox CMB_serialport;
        private ArdupilotMega.Controls.MyButton BUT_connect;
        private System.Windows.Forms.ComboBox CMB_baudrate;
    }
}