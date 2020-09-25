namespace PushbackHelper
{
    partial class FrmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.btnConnect = new System.Windows.Forms.Button();
            this.grpPushBack = new System.Windows.Forms.GroupBox();
            this.lblHeadingValue = new System.Windows.Forms.Label();
            this.lblHeading = new System.Windows.Forms.Label();
            this.btnStraight = new System.Windows.Forms.Button();
            this.btnRight = new System.Windows.Forms.Button();
            this.btnLeft = new System.Windows.Forms.Button();
            this.btnTogglePushback = new System.Windows.Forms.Button();
            this.lblSimStatus = new System.Windows.Forms.Label();
            this.lblSimStatusValue = new System.Windows.Forms.Label();
            this.grpPushBack.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(231, 7);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(107, 27);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect to Sim";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // grpPushBack
            // 
            this.grpPushBack.Controls.Add(this.lblHeadingValue);
            this.grpPushBack.Controls.Add(this.lblHeading);
            this.grpPushBack.Controls.Add(this.btnStraight);
            this.grpPushBack.Controls.Add(this.btnRight);
            this.grpPushBack.Controls.Add(this.btnLeft);
            this.grpPushBack.Controls.Add(this.btnTogglePushback);
            this.grpPushBack.Location = new System.Drawing.Point(15, 45);
            this.grpPushBack.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpPushBack.Name = "grpPushBack";
            this.grpPushBack.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.grpPushBack.Size = new System.Drawing.Size(323, 146);
            this.grpPushBack.TabIndex = 1;
            this.grpPushBack.TabStop = false;
            // 
            // lblHeadingValue
            // 
            this.lblHeadingValue.AutoSize = true;
            this.lblHeadingValue.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Bold);
            this.lblHeadingValue.Location = new System.Drawing.Point(41, 19);
            this.lblHeadingValue.Name = "lblHeadingValue";
            this.lblHeadingValue.Size = new System.Drawing.Size(11, 14);
            this.lblHeadingValue.TabIndex = 5;
            this.lblHeadingValue.Text = "-";
            // 
            // lblHeading
            // 
            this.lblHeading.AutoSize = true;
            this.lblHeading.Font = new System.Drawing.Font("Calibri", 9F);
            this.lblHeading.Location = new System.Drawing.Point(6, 19);
            this.lblHeading.Name = "lblHeading";
            this.lblHeading.Size = new System.Drawing.Size(34, 14);
            this.lblHeading.TabIndex = 4;
            this.lblHeading.Text = "HDG:";
            // 
            // btnStraight
            // 
            this.btnStraight.Location = new System.Drawing.Point(124, 91);
            this.btnStraight.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnStraight.Name = "btnStraight";
            this.btnStraight.Size = new System.Drawing.Size(70, 42);
            this.btnStraight.TabIndex = 3;
            this.btnStraight.Text = "Straight";
            this.btnStraight.UseVisualStyleBackColor = true;
            this.btnStraight.Click += new System.EventHandler(this.btnStraight_Click);
            // 
            // btnRight
            // 
            this.btnRight.Location = new System.Drawing.Point(204, 57);
            this.btnRight.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnRight.Name = "btnRight";
            this.btnRight.Size = new System.Drawing.Size(70, 42);
            this.btnRight.TabIndex = 2;
            this.btnRight.Text = "To Right";
            this.btnRight.UseVisualStyleBackColor = true;
            this.btnRight.Click += new System.EventHandler(this.btnRight_Click);
            // 
            // btnLeft
            // 
            this.btnLeft.Location = new System.Drawing.Point(44, 57);
            this.btnLeft.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnLeft.Name = "btnLeft";
            this.btnLeft.Size = new System.Drawing.Size(70, 42);
            this.btnLeft.TabIndex = 1;
            this.btnLeft.Text = "To Left";
            this.btnLeft.UseVisualStyleBackColor = true;
            this.btnLeft.Click += new System.EventHandler(this.btnLeft_Click);
            // 
            // btnTogglePushback
            // 
            this.btnTogglePushback.Location = new System.Drawing.Point(124, 19);
            this.btnTogglePushback.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnTogglePushback.Name = "btnTogglePushback";
            this.btnTogglePushback.Size = new System.Drawing.Size(70, 42);
            this.btnTogglePushback.TabIndex = 0;
            this.btnTogglePushback.Text = "Toggle PushBack";
            this.btnTogglePushback.UseVisualStyleBackColor = true;
            this.btnTogglePushback.Click += new System.EventHandler(this.btnTogglePushback_Click);
            // 
            // lblSimStatus
            // 
            this.lblSimStatus.AutoSize = true;
            this.lblSimStatus.Location = new System.Drawing.Point(12, 13);
            this.lblSimStatus.Name = "lblSimStatus";
            this.lblSimStatus.Size = new System.Drawing.Size(73, 17);
            this.lblSimStatus.TabIndex = 1;
            this.lblSimStatus.Text = "Sim Status :";
            // 
            // lblSimStatusValue
            // 
            this.lblSimStatusValue.AutoSize = true;
            this.lblSimStatusValue.Font = new System.Drawing.Font("Calibri", 10F, System.Drawing.FontStyle.Bold);
            this.lblSimStatusValue.Location = new System.Drawing.Point(91, 13);
            this.lblSimStatusValue.Name = "lblSimStatusValue";
            this.lblSimStatusValue.Size = new System.Drawing.Size(107, 17);
            this.lblSimStatusValue.TabIndex = 2;
            this.lblSimStatusValue.Text = "NOT CONNECTED";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 202);
            this.Controls.Add(this.grpPushBack);
            this.Controls.Add(this.lblSimStatusValue);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblSimStatus);
            this.Font = new System.Drawing.Font("Calibri", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pushback Helper v1.0";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.grpPushBack.ResumeLayout(false);
            this.grpPushBack.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.GroupBox grpPushBack;
        private System.Windows.Forms.Button btnStraight;
        private System.Windows.Forms.Button btnRight;
        private System.Windows.Forms.Button btnLeft;
        private System.Windows.Forms.Button btnTogglePushback;
        private System.Windows.Forms.Label lblSimStatus;
        private System.Windows.Forms.Label lblSimStatusValue;
        private System.Windows.Forms.Label lblHeadingValue;
        private System.Windows.Forms.Label lblHeading;
    }
}

