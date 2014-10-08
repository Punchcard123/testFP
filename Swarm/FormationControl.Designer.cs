namespace ArdupilotMega.Swarm
{
    partial class FormationControl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormationControl));
            this.CMB_mavs = new System.Windows.Forms.ComboBox();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.grid1 = new ArdupilotMega.Swarm.Grid();
            this.BUT_connect = new ArdupilotMega.Controls.MyButton();
            this.BUT_Start = new ArdupilotMega.Controls.MyButton();
            this.BUT_leader = new ArdupilotMega.Controls.MyButton();
            this.BUT_Land = new ArdupilotMega.Controls.MyButton();
            this.BUT_Takeoff = new ArdupilotMega.Controls.MyButton();
            this.BUT_Disarm = new ArdupilotMega.Controls.MyButton();
            this.BUT_Arm = new ArdupilotMega.Controls.MyButton();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // CMB_mavs
            // 
            this.CMB_mavs.DataSource = this.bindingSource1;
            this.CMB_mavs.FormattingEnabled = true;
            this.CMB_mavs.Location = new System.Drawing.Point(448, 15);
            this.CMB_mavs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.CMB_mavs.Name = "CMB_mavs";
            this.CMB_mavs.Size = new System.Drawing.Size(160, 24);
            this.CMB_mavs.TabIndex = 4;
            this.CMB_mavs.SelectedIndexChanged += new System.EventHandler(this.CMB_mavs_SelectedIndexChanged);
            // 
            // grid1
            // 
            this.grid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grid1.Location = new System.Drawing.Point(16, 84);
            this.grid1.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.grid1.Name = "grid1";
            this.grid1.Size = new System.Drawing.Size(921, 481);
            this.grid1.TabIndex = 8;
            this.grid1.Vertical = false;
            this.grid1.UpdateOffsets += new ArdupilotMega.Swarm.Grid.UpdateOffsetsEvent(this.grid1_UpdateOffsets);
            // 
            // BUT_connect
            // 
            this.BUT_connect.Location = new System.Drawing.Point(617, 15);
            this.BUT_connect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_connect.Name = "BUT_connect";
            this.BUT_connect.Size = new System.Drawing.Size(100, 28);
            this.BUT_connect.TabIndex = 7;
            this.BUT_connect.Text = "Connect MAVs";
            this.BUT_connect.UseVisualStyleBackColor = true;
            this.BUT_connect.Click += new System.EventHandler(this.BUT_connect_Click);
            // 
            // BUT_Start
            // 
            this.BUT_Start.Location = new System.Drawing.Point(833, 15);
            this.BUT_Start.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_Start.Name = "BUT_Start";
            this.BUT_Start.Size = new System.Drawing.Size(100, 28);
            this.BUT_Start.TabIndex = 6;
            this.BUT_Start.Text = "Start";
            this.BUT_Start.UseVisualStyleBackColor = true;
            this.BUT_Start.Click += new System.EventHandler(this.BUT_Start_Click);
            // 
            // BUT_leader
            // 
            this.BUT_leader.Location = new System.Drawing.Point(725, 15);
            this.BUT_leader.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_leader.Name = "BUT_leader";
            this.BUT_leader.Size = new System.Drawing.Size(100, 28);
            this.BUT_leader.TabIndex = 5;
            this.BUT_leader.Text = "Set Leader";
            this.BUT_leader.UseVisualStyleBackColor = true;
            this.BUT_leader.Click += new System.EventHandler(this.BUT_leader_Click);
            // 
            // BUT_Land
            // 
            this.BUT_Land.Location = new System.Drawing.Point(340, 15);
            this.BUT_Land.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_Land.Name = "BUT_Land";
            this.BUT_Land.Size = new System.Drawing.Size(100, 28);
            this.BUT_Land.TabIndex = 3;
            this.BUT_Land.Text = "Land";
            this.BUT_Land.UseVisualStyleBackColor = true;
            this.BUT_Land.Click += new System.EventHandler(this.BUT_Land_Click);
            // 
            // BUT_Takeoff
            // 
            this.BUT_Takeoff.Enabled = false;
            this.BUT_Takeoff.Location = new System.Drawing.Point(232, 15);
            this.BUT_Takeoff.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_Takeoff.Name = "BUT_Takeoff";
            this.BUT_Takeoff.Size = new System.Drawing.Size(100, 28);
            this.BUT_Takeoff.TabIndex = 2;
            this.BUT_Takeoff.Text = "Takeoff";
            this.BUT_Takeoff.UseVisualStyleBackColor = true;
            this.BUT_Takeoff.Click += new System.EventHandler(this.BUT_Takeoff_Click);
            // 
            // BUT_Disarm
            // 
            this.BUT_Disarm.Location = new System.Drawing.Point(124, 15);
            this.BUT_Disarm.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_Disarm.Name = "BUT_Disarm";
            this.BUT_Disarm.Size = new System.Drawing.Size(100, 28);
            this.BUT_Disarm.TabIndex = 1;
            this.BUT_Disarm.Text = "Disarm";
            this.BUT_Disarm.UseVisualStyleBackColor = true;
            this.BUT_Disarm.Click += new System.EventHandler(this.BUT_Disarm_Click);
            // 
            // BUT_Arm
            // 
            this.BUT_Arm.Location = new System.Drawing.Point(16, 15);
            this.BUT_Arm.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BUT_Arm.Name = "BUT_Arm";
            this.BUT_Arm.Size = new System.Drawing.Size(100, 28);
            this.BUT_Arm.TabIndex = 0;
            this.BUT_Arm.Text = "Arm";
            this.BUT_Arm.UseVisualStyleBackColor = true;
            this.BUT_Arm.Click += new System.EventHandler(this.BUT_Arm_Click);
            // 
            // FormationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(953, 580);
            this.Controls.Add(this.grid1);
            this.Controls.Add(this.BUT_connect);
            this.Controls.Add(this.BUT_Start);
            this.Controls.Add(this.BUT_leader);
            this.Controls.Add(this.CMB_mavs);
            this.Controls.Add(this.BUT_Land);
            this.Controls.Add(this.BUT_Takeoff);
            this.Controls.Add(this.BUT_Disarm);
            this.Controls.Add(this.BUT_Arm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormationControl";
            this.Text = "Control";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Control_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.MyButton BUT_Arm;
        private Controls.MyButton BUT_Disarm;
        private Controls.MyButton BUT_Takeoff;
        private Controls.MyButton BUT_Land;
        private System.Windows.Forms.ComboBox CMB_mavs;
        private Controls.MyButton BUT_leader;
        private Controls.MyButton BUT_Start;
        private Controls.MyButton BUT_connect;
        private Grid grid1;
        private System.Windows.Forms.BindingSource bindingSource1;
    }
}