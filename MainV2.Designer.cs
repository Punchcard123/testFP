namespace ArdupilotMega
{
    partial class MainV2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainV2));
            this.MyView = new ArdupilotMega.Controls.MainSwitcher();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.CTX_mainmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.autoHideToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuProjectSettings = new System.Windows.Forms.ToolStripButton();
            this.MenuFlightPlanner = new System.Windows.Forms.ToolStripButton();
            this.MenuPreFlight = new System.Windows.Forms.ToolStripButton();
            this.MenuFlightData = new System.Windows.Forms.ToolStripButton();
            this.MenuSimulation = new System.Windows.Forms.ToolStripButton();
            this.MenuFlightRecorder = new System.Windows.Forms.ToolStripButton();
            this.MenuConfiguration = new System.Windows.Forms.ToolStripButton();
            this.MenuHelp = new System.Windows.Forms.ToolStripButton();
            this.MenuConnect = new System.Windows.Forms.ToolStripButton();
            this.toolStripConnectionControl = new ArdupilotMega.Controls.ToolStripConnectionControl();
            this.menu = new ArdupilotMega.Controls.MyButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.AdvButnChkBox = new System.Windows.Forms.CheckBox();
            this.MainMenu.SuspendLayout();
            this.CTX_mainmenu.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MyView
            // 
            this.MyView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MyView.Location = new System.Drawing.Point(0, 28);
            this.MyView.Margin = new System.Windows.Forms.Padding(0);
            this.MyView.Name = "MyView";
            this.MyView.Size = new System.Drawing.Size(1344, 633);
            this.MyView.TabIndex = 3;
            // 
            // MainMenu
            // 
            this.MainMenu.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MainMenu.BackgroundImage")));
            this.MainMenu.ContextMenuStrip = this.CTX_mainmenu;
            this.MainMenu.GripMargin = new System.Windows.Forms.Padding(0);
            this.MainMenu.ImageScalingSize = new System.Drawing.Size(76, 76);
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuProjectSettings,
            this.MenuFlightPlanner,
            this.MenuPreFlight,
            this.MenuFlightData,
            this.MenuSimulation,
            this.MenuFlightRecorder,
            this.MenuConfiguration,
            this.MenuHelp,
            this.MenuConnect,
            this.toolStripConnectionControl});
            this.MainMenu.Location = new System.Drawing.Point(0, 0);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Padding = new System.Windows.Forms.Padding(0);
            this.MainMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.MainMenu.Size = new System.Drawing.Size(1200, 79);
            this.MainMenu.Stretch = false;
            this.MainMenu.TabIndex = 5;
            this.MainMenu.Text = "menuStrip1";
            this.MainMenu.MouseLeave += new System.EventHandler(this.MainMenu_MouseLeave);
            // 
            // CTX_mainmenu
            // 
            this.CTX_mainmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoHideToolStripMenuItem});
            this.CTX_mainmenu.Name = "CTX_mainmenu";
            this.CTX_mainmenu.Size = new System.Drawing.Size(143, 28);
            // 
            // autoHideToolStripMenuItem
            // 
            this.autoHideToolStripMenuItem.Checked = true;
            this.autoHideToolStripMenuItem.CheckOnClick = true;
            this.autoHideToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoHideToolStripMenuItem.Name = "autoHideToolStripMenuItem";
            this.autoHideToolStripMenuItem.Size = new System.Drawing.Size(142, 24);
            this.autoHideToolStripMenuItem.Text = "AutoHide";
            this.autoHideToolStripMenuItem.Click += new System.EventHandler(this.autoHideToolStripMenuItem_Click);
            // 
            // MenuProjectSettings
            // 
            this.MenuProjectSettings.AutoSize = false;
            this.MenuProjectSettings.BackgroundImage = global::ArdupilotMega.Properties.Resources.ProjectSettings_button_1;
            this.MenuProjectSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MenuProjectSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuProjectSettings.Name = "MenuProjectSettings";
            this.MenuProjectSettings.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuProjectSettings.Size = new System.Drawing.Size(76, 76);
            this.MenuProjectSettings.Text = "Project Settings";
            this.MenuProjectSettings.ToolTipText = "Project Settings";
            this.MenuProjectSettings.Click += new System.EventHandler(this.MenuProjectSettings_Click);
            // 
            // MenuFlightPlanner
            // 
            this.MenuFlightPlanner.AutoSize = false;
            this.MenuFlightPlanner.BackgroundImage = global::ArdupilotMega.Properties.Resources.FlightPlan_button_1;
            this.MenuFlightPlanner.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MenuFlightPlanner.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuFlightPlanner.Margin = new System.Windows.Forms.Padding(0);
            this.MenuFlightPlanner.Name = "MenuFlightPlanner";
            this.MenuFlightPlanner.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuFlightPlanner.Size = new System.Drawing.Size(76, 76);
            this.MenuFlightPlanner.Text = "Flight Plan";
            this.MenuFlightPlanner.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuFlightPlanner.ToolTipText = "Flight Planner";
            this.MenuFlightPlanner.Click += new System.EventHandler(this.MenuFlightPlanner_Click);
            // 
            // MenuPreFlight
            // 
            this.MenuPreFlight.AutoSize = false;
            this.MenuPreFlight.BackgroundImage = global::ArdupilotMega.Properties.Resources.PreFlight_button_1;
            this.MenuPreFlight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MenuPreFlight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuPreFlight.Margin = new System.Windows.Forms.Padding(0);
            this.MenuPreFlight.Name = "MenuPreFlight";
            this.MenuPreFlight.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuPreFlight.Size = new System.Drawing.Size(76, 76);
            this.MenuPreFlight.Text = "Pre-Flight";
            this.MenuPreFlight.ToolTipText = "Pre-Flight Setup / Checks";
            this.MenuPreFlight.Click += new System.EventHandler(this.MenuPreFlight_Click);
            // 
            // MenuFlightData
            // 
            this.MenuFlightData.AutoSize = false;
            this.MenuFlightData.BackgroundImage = global::ArdupilotMega.Properties.Resources.FlightControl_button_1;
            this.MenuFlightData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MenuFlightData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuFlightData.Margin = new System.Windows.Forms.Padding(0);
            this.MenuFlightData.Name = "MenuFlightData";
            this.MenuFlightData.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuFlightData.Size = new System.Drawing.Size(76, 76);
            this.MenuFlightData.Click += new System.EventHandler(this.MenuFlightData_Click);
            // 
            // MenuSimulation
            // 
            this.MenuSimulation.AutoSize = false;
            this.MenuSimulation.BackgroundImage = global::ArdupilotMega.Properties.Resources.FlightSimulator_button;
            this.MenuSimulation.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MenuSimulation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuSimulation.Margin = new System.Windows.Forms.Padding(0);
            this.MenuSimulation.Name = "MenuSimulation";
            this.MenuSimulation.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuSimulation.Size = new System.Drawing.Size(76, 76);
            this.MenuSimulation.Text = "Simulate";
            this.MenuSimulation.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuSimulation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuSimulation.ToolTipText = "Flight Simulation";
            this.MenuSimulation.Click += new System.EventHandler(this.MenuSimulation_Click);
            // 
            // MenuFlightRecorder
            // 
            this.MenuFlightRecorder.AutoSize = false;
            this.MenuFlightRecorder.BackgroundImage = global::ArdupilotMega.Properties.Resources.FlightRecorder_button_1;
            this.MenuFlightRecorder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MenuFlightRecorder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuFlightRecorder.Margin = new System.Windows.Forms.Padding(0);
            this.MenuFlightRecorder.Name = "MenuFlightRecorder";
            this.MenuFlightRecorder.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuFlightRecorder.Size = new System.Drawing.Size(76, 76);
            this.MenuFlightRecorder.Text = "Flight Recorder";
            this.MenuFlightRecorder.Click += new System.EventHandler(this.MenuFlightRecorder_Click);
            // 
            // MenuConfiguration
            // 
            this.MenuConfiguration.AutoSize = false;
            this.MenuConfiguration.BackgroundImage = global::ArdupilotMega.Properties.Resources.Factory_button_1;
            this.MenuConfiguration.Font = new System.Drawing.Font("Segoe UI", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MenuConfiguration.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuConfiguration.Margin = new System.Windows.Forms.Padding(0);
            this.MenuConfiguration.Name = "MenuConfiguration";
            this.MenuConfiguration.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuConfiguration.Size = new System.Drawing.Size(76, 76);
            this.MenuConfiguration.Text = "Configure";
            this.MenuConfiguration.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuConfiguration.TextImageRelation = System.Windows.Forms.TextImageRelation.TextAboveImage;
            this.MenuConfiguration.ToolTipText = "Configuration";
            this.MenuConfiguration.Click += new System.EventHandler(this.MenuConfiguration_Click);
            // 
            // MenuHelp
            // 
            this.MenuHelp.AutoSize = false;
            this.MenuHelp.BackgroundImage = global::ArdupilotMega.Properties.Resources.Help_button_1;
            this.MenuHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.MenuHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuHelp.Margin = new System.Windows.Forms.Padding(0);
            this.MenuHelp.Name = "MenuHelp";
            this.MenuHelp.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuHelp.Size = new System.Drawing.Size(76, 76);
            this.MenuHelp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuHelp.ToolTipText = "Help";
            this.MenuHelp.Click += new System.EventHandler(this.MenuHelp_Click);
            // 
            // MenuConnect
            // 
            this.MenuConnect.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.MenuConnect.AutoSize = false;
            this.MenuConnect.BackgroundImage = global::ArdupilotMega.Properties.Resources.Red_button;
            this.MenuConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuConnect.Margin = new System.Windows.Forms.Padding(0);
            this.MenuConnect.Name = "MenuConnect";
            this.MenuConnect.Padding = new System.Windows.Forms.Padding(0, 0, 72, 72);
            this.MenuConnect.Size = new System.Drawing.Size(76, 76);
            this.MenuConnect.Text = "Connect";
            this.MenuConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuConnect.ToolTipText = "AutoPilot connection status";
            this.MenuConnect.Click += new System.EventHandler(this.MenuConnect_Click);
            // 
            // toolStripConnectionControl
            // 
            this.toolStripConnectionControl.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripConnectionControl.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripConnectionControl.BackgroundImage = global::ArdupilotMega.Properties.Resources.bg;
            this.toolStripConnectionControl.Margin = new System.Windows.Forms.Padding(0);
            this.toolStripConnectionControl.Name = "toolStripConnectionControl";
            this.toolStripConnectionControl.Padding = new System.Windows.Forms.Padding(0, 0, 30, 0);
            this.toolStripConnectionControl.Size = new System.Drawing.Size(299, 79);
            this.toolStripConnectionControl.MouseLeave += new System.EventHandler(this.MainMenu_MouseLeave);
            // 
            // menu
            // 
            this.menu.Dock = System.Windows.Forms.DockStyle.Top;
            this.menu.Location = new System.Drawing.Point(0, 0);
            this.menu.Margin = new System.Windows.Forms.Padding(4);
            this.menu.Name = "menu";
            this.menu.Size = new System.Drawing.Size(1344, 28);
            this.menu.TabIndex = 6;
            this.menu.Text = "Menu";
            this.menu.UseVisualStyleBackColor = true;
            this.menu.MouseEnter += new System.EventHandler(this.menu_MouseEnter);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.AdvButnChkBox);
            this.panel1.Controls.Add(this.MainMenu);
            this.panel1.Location = new System.Drawing.Point(57, 57);
            this.panel1.Margin = new System.Windows.Forms.Padding(4);
            this.panel1.MaximumSize = new System.Drawing.Size(133332, 94);
            this.panel1.MinimumSize = new System.Drawing.Size(1200, 94);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1200, 94);
            this.panel1.TabIndex = 7;
            this.panel1.Visible = false;
            this.panel1.MouseLeave += new System.EventHandler(this.MainMenu_MouseLeave);
            // 
            // AdvButnChkBox
            // 
            this.AdvButnChkBox.AutoSize = true;
            this.AdvButnChkBox.Location = new System.Drawing.Point(676, 53);
            this.AdvButnChkBox.Name = "AdvButnChkBox";
            this.AdvButnChkBox.Size = new System.Drawing.Size(156, 21);
            this.AdvButnChkBox.TabIndex = 6;
            this.AdvButnChkBox.Text = "Advanced buttons...";
            this.AdvButnChkBox.UseVisualStyleBackColor = true;
            this.AdvButnChkBox.CheckedChanged += new System.EventHandler(this.AdvButnChkBox_CheckedChanged);
            // 
            // MainV2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1344, 661);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.MyView);
            this.Controls.Add(this.menu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.MainMenu;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1359, 697);
            this.Name = "MainV2";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DMX Mission Planner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainV2_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainV2_FormClosed);
            this.Load += new System.EventHandler(this.MainV2_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainV2_KeyDown);
            this.Resize += new System.EventHandler(this.MainV2_Resize);
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.CTX_mainmenu.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion


        private ArdupilotMega.Controls.MainSwitcher MyView;
        private System.Windows.Forms.MenuStrip MainMenu;
        private System.Windows.Forms.ToolStripButton MenuFlightData;
        private System.Windows.Forms.ToolStripButton MenuFlightPlanner;
        private System.Windows.Forms.ToolStripButton MenuConfiguration;
        private System.Windows.Forms.ToolStripButton MenuSimulation;
        private System.Windows.Forms.ToolStripButton MenuConnect;

        private System.Windows.Forms.ToolStripButton MenuHelp;
        private Controls.ToolStripConnectionControl toolStripConnectionControl;
        private Controls.MyButton menu;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ContextMenuStrip CTX_mainmenu;
        private System.Windows.Forms.ToolStripMenuItem autoHideToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton MenuPreFlight;
        private System.Windows.Forms.ToolStripButton MenuFlightRecorder;
        private System.Windows.Forms.ToolStripButton MenuProjectSettings;
        private System.Windows.Forms.CheckBox AdvButnChkBox;
    }
}