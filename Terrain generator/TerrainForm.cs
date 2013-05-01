﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Terrain_generator
{
    public partial class TerrainForm : Form
    {
        public TerrainForm(TerrainInfo ti)
        {
            InitializeComponent();

            Bitmap bmp = ti.Generate();
            pictureBox1.Width = bmp.Width;
            pictureBox1.Height = bmp.Height;
            ClientSize = new Size(pictureBox1.Width, pictureBox1.Height);            
            
            pictureBox2.Width = pictureBox1.Width;
            pictureBox2.Height = pictureBox1.Height;
            pictureBox2.Left = pictureBox1.Width;
            
            pictureBox2.Image = pictureBox1.Image = bmp;
        }

        const int scroll = 128;
        private void lnkLeft_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pictureBox1.Left += scroll;
            pictureBox2.Left += scroll;

            if (pictureBox1.Left > 0)
            {
                pictureBox1.Left -= pictureBox1.Width;
                pictureBox2.Left -= pictureBox2.Width;
            }
        }

        private void lnkRight_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            pictureBox1.Left -= scroll;
            pictureBox2.Left -= scroll;

            if (pictureBox2.Left < 0)
            {
                pictureBox1.Left += pictureBox1.Width;
                pictureBox2.Left += pictureBox2.Width;
            }
        }
    }
}
