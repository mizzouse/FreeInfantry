namespace InfLauncher.Views
{
    partial class UpdaterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdaterForm));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lblPleaseWait = new System.Windows.Forms.Label();
            this.lblCurrentFilename = new System.Windows.Forms.Label();
            this.lblFileCount = new System.Windows.Forms.Label();
            this.lblTask = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 102);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(427, 23);
            this.progressBar.TabIndex = 0;
            // 
            // lblPleaseWait
            // 
            this.lblPleaseWait.AutoSize = true;
            this.lblPleaseWait.Location = new System.Drawing.Point(9, 9);
            this.lblPleaseWait.Name = "lblPleaseWait";
            this.lblPleaseWait.Size = new System.Drawing.Size(179, 13);
            this.lblPleaseWait.TabIndex = 1;
            this.lblPleaseWait.Text = "Please wait while the game updates.";
            // 
            // lblCurrentFilename
            // 
            this.lblCurrentFilename.AutoSize = true;
            this.lblCurrentFilename.Location = new System.Drawing.Point(12, 64);
            this.lblCurrentFilename.Name = "lblCurrentFilename";
            this.lblCurrentFilename.Size = new System.Drawing.Size(0, 13);
            this.lblCurrentFilename.TabIndex = 3;
            // 
            // lblFileCount
            // 
            this.lblFileCount.AutoSize = true;
            this.lblFileCount.Location = new System.Drawing.Point(12, 85);
            this.lblFileCount.Name = "lblFileCount";
            this.lblFileCount.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblFileCount.Size = new System.Drawing.Size(0, 13);
            this.lblFileCount.TabIndex = 4;
            // 
            // lblTask
            // 
            this.lblTask.AutoSize = true;
            this.lblTask.Location = new System.Drawing.Point(12, 35);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(0, 13);
            this.lblTask.TabIndex = 5;
            // 
            // UpdaterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 137);
            this.Controls.Add(this.lblTask);
            this.Controls.Add(this.lblFileCount);
            this.Controls.Add(this.lblCurrentFilename);
            this.Controls.Add(this.lblPleaseWait);
            this.Controls.Add(this.progressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "UpdaterForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Infantry Updater";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label lblPleaseWait;
        private System.Windows.Forms.Label lblCurrentFilename;
        private System.Windows.Forms.Label lblFileCount;
        private System.Windows.Forms.Label lblTask;
    }
}