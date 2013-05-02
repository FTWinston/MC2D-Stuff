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

        int lastSeed = -1;

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            int width, height;

            if (!int.TryParse(txtWidth.Text, out width)
                || !int.TryParse(txtHeight.Text, out height)
                )
                return;

            if (!chkLockSeed.Checked)
            {
                Random r = new Random();
                lastSeed = r.Next();
            }

            TerrainInfo ti = null;
            if ( txtSerialized.Text != string.Empty )
                try
                {
                    ti = TerrainInfo.Deserialize(txtSerialized.Text);
                    lastSeed = ti.Seed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to deserialize terrain: " + ex.Message);
                    ti = null;
                }

            if ( ti == null )
                ti = new TerrainInfo() {
                    Width = width,
                    Height = height,
                    GroundVerticalExtent = trackBarGroundAmplitude.Value,
                    GroundBumpiness = trackBarGroundBumpiness.Value,
                    CaveQuantity = trackBarCaveQuantity.Value,
                    Seed = lastSeed
                };

            if (TerrainForm == null || TerrainForm.IsDisposed)
            {
                TerrainForm = new TerrainForm();
                TerrainForm.Left = this.Right;
                TerrainForm.Top = this.Top;
            }

            TerrainForm.SetTerrain(ti);
            TerrainForm.Show();
        }

        TerrainForm TerrainForm;
    }
}
