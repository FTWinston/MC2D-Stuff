﻿namespace Terrain_generator
{
    partial class SetupForm
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
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.trackBarGroundAmplitude = new System.Windows.Forms.TrackBar();
            this.label3 = new System.Windows.Forms.Label();
            this.trackBarGroundBumpiness = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.chkLockSeed = new System.Windows.Forms.CheckBox();
            this.trackBarCaveComplexity = new System.Windows.Forms.TrackBar();
            this.label5 = new System.Windows.Forms.Label();
            this.txtSerialized = new System.Windows.Forms.TextBox();
            this.chkGroundAmplitude = new System.Windows.Forms.CheckBox();
            this.chkGroundBumpiness = new System.Windows.Forms.CheckBox();
            this.chkCaveComplexity = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGroundAmplitude)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGroundBumpiness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarCaveComplexity)).BeginInit();
            this.SuspendLayout();
            // 
            // txtWidth
            // 
            this.txtWidth.Location = new System.Drawing.Point(53, 12);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(39, 20);
            this.txtWidth.TabIndex = 0;
            this.txtWidth.Text = "1024";
            this.txtWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Width";
            // 
            // txtHeight
            // 
            this.txtHeight.Location = new System.Drawing.Point(165, 12);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(39, 20);
            this.txtHeight.TabIndex = 1;
            this.txtHeight.Text = "768";
            this.txtHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(121, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Height";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(150, 212);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 100;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // trackBarGroundAmplitude
            // 
            this.trackBarGroundAmplitude.Enabled = false;
            this.trackBarGroundAmplitude.LargeChange = 100;
            this.trackBarGroundAmplitude.Location = new System.Drawing.Point(67, 38);
            this.trackBarGroundAmplitude.Maximum = 1000;
            this.trackBarGroundAmplitude.Name = "trackBarGroundAmplitude";
            this.trackBarGroundAmplitude.Size = new System.Drawing.Size(137, 45);
            this.trackBarGroundAmplitude.SmallChange = 10;
            this.trackBarGroundAmplitude.TabIndex = 2;
            this.trackBarGroundAmplitude.TickFrequency = 50;
            this.trackBarGroundAmplitude.Value = 500;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 38);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(47, 39);
            this.label3.TabIndex = 2;
            this.label3.Text = "Ground\r\nheight\r\nvariation";
            // 
            // trackBarGroundBumpiness
            // 
            this.trackBarGroundBumpiness.Enabled = false;
            this.trackBarGroundBumpiness.LargeChange = 100;
            this.trackBarGroundBumpiness.Location = new System.Drawing.Point(67, 89);
            this.trackBarGroundBumpiness.Maximum = 1000;
            this.trackBarGroundBumpiness.Name = "trackBarGroundBumpiness";
            this.trackBarGroundBumpiness.Size = new System.Drawing.Size(137, 45);
            this.trackBarGroundBumpiness.SmallChange = 10;
            this.trackBarGroundBumpiness.TabIndex = 3;
            this.trackBarGroundBumpiness.TickFrequency = 50;
            this.trackBarGroundBumpiness.Value = 400;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(57, 26);
            this.label4.TabIndex = 3;
            this.label4.Text = "Ground\r\nbumpiness";
            // 
            // chkLockSeed
            // 
            this.chkLockSeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkLockSeed.AutoSize = true;
            this.chkLockSeed.Location = new System.Drawing.Point(12, 216);
            this.chkLockSeed.Name = "chkLockSeed";
            this.chkLockSeed.Size = new System.Drawing.Size(76, 17);
            this.chkLockSeed.TabIndex = 99;
            this.chkLockSeed.Text = "Lock seed";
            this.chkLockSeed.UseVisualStyleBackColor = true;
            // 
            // trackBarCaveComplexity
            // 
            this.trackBarCaveComplexity.Enabled = false;
            this.trackBarCaveComplexity.LargeChange = 2;
            this.trackBarCaveComplexity.Location = new System.Drawing.Point(67, 140);
            this.trackBarCaveComplexity.Maximum = 8;
            this.trackBarCaveComplexity.Minimum = 1;
            this.trackBarCaveComplexity.Name = "trackBarCaveComplexity";
            this.trackBarCaveComplexity.Size = new System.Drawing.Size(137, 45);
            this.trackBarCaveComplexity.TabIndex = 3;
            this.trackBarCaveComplexity.Value = 5;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 140);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 26);
            this.label5.TabIndex = 3;
            this.label5.Text = "Cave\r\ncomplexity";
            // 
            // txtSerialized
            // 
            this.txtSerialized.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSerialized.Location = new System.Drawing.Point(12, 190);
            this.txtSerialized.Name = "txtSerialized";
            this.txtSerialized.Size = new System.Drawing.Size(213, 20);
            this.txtSerialized.TabIndex = 101;
            // 
            // chkGroundAmplitude
            // 
            this.chkGroundAmplitude.AutoSize = true;
            this.chkGroundAmplitude.Location = new System.Drawing.Point(210, 38);
            this.chkGroundAmplitude.Name = "chkGroundAmplitude";
            this.chkGroundAmplitude.Size = new System.Drawing.Size(15, 14);
            this.chkGroundAmplitude.TabIndex = 102;
            this.chkGroundAmplitude.UseVisualStyleBackColor = true;
            this.chkGroundAmplitude.CheckedChanged += new System.EventHandler(this.chkGroundHeightVariation_CheckedChanged);
            // 
            // chkGroundBumpiness
            // 
            this.chkGroundBumpiness.AutoSize = true;
            this.chkGroundBumpiness.Location = new System.Drawing.Point(210, 89);
            this.chkGroundBumpiness.Name = "chkGroundBumpiness";
            this.chkGroundBumpiness.Size = new System.Drawing.Size(15, 14);
            this.chkGroundBumpiness.TabIndex = 102;
            this.chkGroundBumpiness.UseVisualStyleBackColor = true;
            this.chkGroundBumpiness.CheckedChanged += new System.EventHandler(this.chkGroundBumpiness_CheckedChanged);
            // 
            // chkCaveComplexity
            // 
            this.chkCaveComplexity.AutoSize = true;
            this.chkCaveComplexity.Location = new System.Drawing.Point(210, 140);
            this.chkCaveComplexity.Name = "chkCaveComplexity";
            this.chkCaveComplexity.Size = new System.Drawing.Size(15, 14);
            this.chkCaveComplexity.TabIndex = 102;
            this.chkCaveComplexity.UseVisualStyleBackColor = true;
            this.chkCaveComplexity.CheckedChanged += new System.EventHandler(this.chkCaveComplexity_CheckedChanged);
            // 
            // SetupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(237, 247);
            this.Controls.Add(this.chkCaveComplexity);
            this.Controls.Add(this.chkGroundBumpiness);
            this.Controls.Add(this.chkGroundAmplitude);
            this.Controls.Add(this.trackBarCaveComplexity);
            this.Controls.Add(this.txtSerialized);
            this.Controls.Add(this.chkLockSeed);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.trackBarGroundBumpiness);
            this.Controls.Add(this.trackBarGroundAmplitude);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtHeight);
            this.Controls.Add(this.txtWidth);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "SetupForm";
            this.Text = "Terrain generator";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGroundAmplitude)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarGroundBumpiness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarCaveComplexity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TrackBar trackBarGroundAmplitude;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBarGroundBumpiness;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkLockSeed;
        private System.Windows.Forms.TrackBar trackBarCaveComplexity;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtSerialized;
        private System.Windows.Forms.CheckBox chkGroundAmplitude;
        private System.Windows.Forms.CheckBox chkGroundBumpiness;
        private System.Windows.Forms.CheckBox chkCaveComplexity;
    }
}

