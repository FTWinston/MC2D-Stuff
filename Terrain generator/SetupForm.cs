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
    public partial class SetupForm : Form
    {
        public SetupForm()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            int width, height;

            if (!int.TryParse(txtWidth.Text, out width)
                || !int.TryParse(txtHeight.Text, out height)
                )
                return;

            Random r = new Random();
            TerrainInfo ti = new TerrainInfo() {
                Width = width,
                Height = height,
                Seed = r.Next()
            };

            new TerrainForm(ti).Show();
        }
    }
}
