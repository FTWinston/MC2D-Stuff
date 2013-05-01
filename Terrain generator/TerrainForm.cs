using System;
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

            Bitmap bmp = new Bitmap(ti.Width, ti.Height);
            Width = pictureBox1.Width = ti.Width;
            Height = pictureBox1.Height = ti.Height;

            using (Graphics g = Graphics.FromImage(bmp))
                ti.Generate(g);
            
            pictureBox1.Image = bmp;
        }
    }
}
