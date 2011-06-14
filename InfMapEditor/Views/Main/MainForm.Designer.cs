namespace InfMapEditor.Views.Main
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusLabelMousePosition = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.menuItemFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.editMenuItemShowGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItemObjects = new System.Windows.Forms.ToolStripMenuItem();
            this.doorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hidesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nestedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.parallaxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.portalsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.switchesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.warpsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainMenuItemWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.windowMenuItemNewTerrainPalette = new System.Windows.Forms.ToolStripMenuItem();
            this.toolBar = new System.Windows.Forms.ToolStrip();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.minimap = new InfMapEditor.Views.Main.Partials.MinimapControl();
            this.mapControl = new InfMapEditor.Views.Main.Partials.MapControl();
            this.layersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.floorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar.SuspendLayout();
            this.menuBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelMousePosition});
            this.statusBar.Location = new System.Drawing.Point(0, 606);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(970, 24);
            this.statusBar.TabIndex = 0;
            this.statusBar.Text = "statusStrip1";
            // 
            // statusLabelMousePosition
            // 
            this.statusLabelMousePosition.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statusLabelMousePosition.Name = "statusLabelMousePosition";
            this.statusLabelMousePosition.Size = new System.Drawing.Size(86, 19);
            this.statusLabelMousePosition.Text = "Map Pos (0, 0)";
            // 
            // menuBar
            // 
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemFile,
            this.menuItemEdit,
            this.layersToolStripMenuItem,
            this.menuItemObjects,
            this.mainMenuItemWindow});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(970, 24);
            this.menuBar.TabIndex = 1;
            this.menuBar.Text = "menuStrip1";
            // 
            // menuItemFile
            // 
            this.menuItemFile.Name = "menuItemFile";
            this.menuItemFile.Size = new System.Drawing.Size(37, 20);
            this.menuItemFile.Text = "File";
            // 
            // menuItemEdit
            // 
            this.menuItemEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editMenuItemShowGrid});
            this.menuItemEdit.Name = "menuItemEdit";
            this.menuItemEdit.Size = new System.Drawing.Size(39, 20);
            this.menuItemEdit.Text = "Edit";
            // 
            // editMenuItemShowGrid
            // 
            this.editMenuItemShowGrid.Name = "editMenuItemShowGrid";
            this.editMenuItemShowGrid.Size = new System.Drawing.Size(105, 22);
            this.editMenuItemShowGrid.Text = "Grid...";
            this.editMenuItemShowGrid.Click += new System.EventHandler(this.editMenuItemShowGrid_Click);
            // 
            // menuItemObjects
            // 
            this.menuItemObjects.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.doorsToolStripMenuItem,
            this.flagsToolStripMenuItem,
            this.hidesToolStripMenuItem,
            this.nestedToolStripMenuItem,
            this.parallaxToolStripMenuItem,
            this.portalsToolStripMenuItem,
            this.soundsToolStripMenuItem,
            this.switchesToolStripMenuItem,
            this.textToolStripMenuItem,
            this.warpsToolStripMenuItem});
            this.menuItemObjects.Name = "menuItemObjects";
            this.menuItemObjects.Size = new System.Drawing.Size(59, 20);
            this.menuItemObjects.Text = "Objects";
            // 
            // doorsToolStripMenuItem
            // 
            this.doorsToolStripMenuItem.Name = "doorsToolStripMenuItem";
            this.doorsToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.doorsToolStripMenuItem.Text = "Doors...";
            // 
            // flagsToolStripMenuItem
            // 
            this.flagsToolStripMenuItem.Name = "flagsToolStripMenuItem";
            this.flagsToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.flagsToolStripMenuItem.Text = "Flags...";
            // 
            // hidesToolStripMenuItem
            // 
            this.hidesToolStripMenuItem.Name = "hidesToolStripMenuItem";
            this.hidesToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.hidesToolStripMenuItem.Text = "Hides...";
            // 
            // nestedToolStripMenuItem
            // 
            this.nestedToolStripMenuItem.Name = "nestedToolStripMenuItem";
            this.nestedToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.nestedToolStripMenuItem.Text = "Nested...";
            // 
            // parallaxToolStripMenuItem
            // 
            this.parallaxToolStripMenuItem.Name = "parallaxToolStripMenuItem";
            this.parallaxToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.parallaxToolStripMenuItem.Text = "Parallax...";
            // 
            // portalsToolStripMenuItem
            // 
            this.portalsToolStripMenuItem.Name = "portalsToolStripMenuItem";
            this.portalsToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.portalsToolStripMenuItem.Text = "Portals...";
            // 
            // soundsToolStripMenuItem
            // 
            this.soundsToolStripMenuItem.Name = "soundsToolStripMenuItem";
            this.soundsToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.soundsToolStripMenuItem.Text = "Sounds...";
            // 
            // switchesToolStripMenuItem
            // 
            this.switchesToolStripMenuItem.Name = "switchesToolStripMenuItem";
            this.switchesToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.switchesToolStripMenuItem.Text = "Switches...";
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.textToolStripMenuItem.Text = "Text...";
            // 
            // warpsToolStripMenuItem
            // 
            this.warpsToolStripMenuItem.Name = "warpsToolStripMenuItem";
            this.warpsToolStripMenuItem.Size = new System.Drawing.Size(129, 22);
            this.warpsToolStripMenuItem.Text = "Warps...";
            // 
            // mainMenuItemWindow
            // 
            this.mainMenuItemWindow.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.windowMenuItemNewTerrainPalette});
            this.mainMenuItemWindow.Name = "mainMenuItemWindow";
            this.mainMenuItemWindow.Size = new System.Drawing.Size(63, 20);
            this.mainMenuItemWindow.Text = "Window";
            // 
            // windowMenuItemNewTerrainPalette
            // 
            this.windowMenuItemNewTerrainPalette.Name = "windowMenuItemNewTerrainPalette";
            this.windowMenuItemNewTerrainPalette.Size = new System.Drawing.Size(177, 22);
            this.windowMenuItemNewTerrainPalette.Text = "New Terrain Palette";
            this.windowMenuItemNewTerrainPalette.Click += new System.EventHandler(this.windowMenuItemNewTerrainPalette_Click);
            // 
            // toolBar
            // 
            this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolBar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolBar.Location = new System.Drawing.Point(0, 24);
            this.toolBar.Name = "toolBar";
            this.toolBar.Size = new System.Drawing.Size(970, 25);
            this.toolBar.TabIndex = 2;
            this.toolBar.Text = "toolStrip1";
            // 
            // splitContainer
            // 
            this.splitContainer.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.IsSplitterFixed = true;
            this.splitContainer.Location = new System.Drawing.Point(0, 49);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.minimap);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.mapControl);
            this.splitContainer.Size = new System.Drawing.Size(970, 557);
            this.splitContainer.SplitterDistance = 264;
            this.splitContainer.TabIndex = 3;
            // 
            // minimap
            // 
            this.minimap.BackColor = System.Drawing.SystemColors.Control;
            this.minimap.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.minimap.Image = ((System.Drawing.Bitmap)(resources.GetObject("minimap.Image")));
            this.minimap.Location = new System.Drawing.Point(3, 3);
            this.minimap.Name = "minimap";
            this.minimap.Size = new System.Drawing.Size(256, 256);
            this.minimap.TabIndex = 0;
            // 
            // mapControl
            // 
            this.mapControl.AutoSize = true;
            this.mapControl.BackColor = System.Drawing.SystemColors.Control;
            this.mapControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mapControl.Location = new System.Drawing.Point(0, 0);
            this.mapControl.Name = "mapControl";
            this.mapControl.Size = new System.Drawing.Size(698, 553);
            this.mapControl.TabIndex = 0;
            // 
            // layersToolStripMenuItem
            // 
            this.layersToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.floorToolStripMenuItem,
            this.objectsToolStripMenuItem,
            this.physicsToolStripMenuItem,
            this.visionToolStripMenuItem});
            this.layersToolStripMenuItem.Name = "layersToolStripMenuItem";
            this.layersToolStripMenuItem.Size = new System.Drawing.Size(52, 20);
            this.layersToolStripMenuItem.Text = "Layers";
            // 
            // floorToolStripMenuItem
            // 
            this.floorToolStripMenuItem.Checked = true;
            this.floorToolStripMenuItem.CheckOnClick = true;
            this.floorToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.floorToolStripMenuItem.Name = "floorToolStripMenuItem";
            this.floorToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.floorToolStripMenuItem.Text = "Floors";
            // 
            // objectsToolStripMenuItem
            // 
            this.objectsToolStripMenuItem.Checked = true;
            this.objectsToolStripMenuItem.CheckOnClick = true;
            this.objectsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.objectsToolStripMenuItem.Name = "objectsToolStripMenuItem";
            this.objectsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.objectsToolStripMenuItem.Text = "Objects";
            // 
            // physicsToolStripMenuItem
            // 
            this.physicsToolStripMenuItem.Checked = true;
            this.physicsToolStripMenuItem.CheckOnClick = true;
            this.physicsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.physicsToolStripMenuItem.Name = "physicsToolStripMenuItem";
            this.physicsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.physicsToolStripMenuItem.Text = "Physics";
            // 
            // visionToolStripMenuItem
            // 
            this.visionToolStripMenuItem.Checked = true;
            this.visionToolStripMenuItem.CheckOnClick = true;
            this.visionToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.visionToolStripMenuItem.Name = "visionToolStripMenuItem";
            this.visionToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.visionToolStripMenuItem.Text = "Vision";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 630);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.toolBar);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.menuBar);
            this.MainMenuStrip = this.menuBar;
            this.Name = "MainForm";
            this.Text = "Infantry Map Editor";
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStrip toolBar;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.ToolStripMenuItem menuItemFile;
        private System.Windows.Forms.ToolStripMenuItem menuItemObjects;
        private Partials.MinimapControl minimap;
        private System.Windows.Forms.ToolStripMenuItem menuItemEdit;
        private System.Windows.Forms.ToolStripMenuItem mainMenuItemWindow;
        private System.Windows.Forms.ToolStripMenuItem windowMenuItemNewTerrainPalette;
        private System.Windows.Forms.ToolStripMenuItem doorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem flagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hidesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nestedToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem parallaxToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem portalsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem soundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem switchesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem warpsToolStripMenuItem;
        private Partials.MapControl mapControl;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelMousePosition;
        private System.Windows.Forms.ToolStripMenuItem editMenuItemShowGrid;
        private System.Windows.Forms.ToolStripMenuItem layersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem floorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem objectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem physicsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem visionToolStripMenuItem;
    }
}

