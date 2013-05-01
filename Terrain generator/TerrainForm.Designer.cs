namespace Terrain_generator
{
    partial class TerrainForm
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.lnkLeft = new System.Windows.Forms.LinkLabel();
            this.lnkRight = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(78, 72);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Location = new System.Drawing.Point(100, 0);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(78, 72);
            this.pictureBox2.TabIndex = 0;
            this.pictureBox2.TabStop = false;
            // 
            // lnkLeft
            // 
            this.lnkLeft.AutoSize = true;
            this.lnkLeft.BackColor = System.Drawing.Color.White;
            this.lnkLeft.Location = new System.Drawing.Point(12, 9);
            this.lnkLeft.Name = "lnkLeft";
            this.lnkLeft.Size = new System.Drawing.Size(37, 13);
            this.lnkLeft.TabIndex = 1;
            this.lnkLeft.TabStop = true;
            this.lnkLeft.Text = "<- Left";
            this.lnkLeft.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkLeft_LinkClicked);
            // 
            // lnkRight
            // 
            this.lnkRight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkRight.AutoSize = true;
            this.lnkRight.BackColor = System.Drawing.Color.White;
            this.lnkRight.Location = new System.Drawing.Point(129, 9);
            this.lnkRight.Name = "lnkRight";
            this.lnkRight.Size = new System.Drawing.Size(44, 13);
            this.lnkRight.TabIndex = 1;
            this.lnkRight.TabStop = true;
            this.lnkRight.Text = "Right ->";
            this.lnkRight.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRight_LinkClicked);
            // 
            // TerrainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(178, 72);
            this.Controls.Add(this.lnkRight);
            this.Controls.Add(this.lnkLeft);
            this.Controls.Add(this.pictureBox2);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "TerrainForm";
            this.Text = "Generated terrain";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.LinkLabel lnkLeft;
        private System.Windows.Forms.LinkLabel lnkRight;
    }
}