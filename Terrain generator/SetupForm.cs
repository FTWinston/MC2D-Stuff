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

            TerrainGenerator ti = null;
            if ( txtSerialized.Text != string.Empty )
                try
                {
                    ti = TerrainGenerator.Deserialize(txtSerialized.Text);
                    lastSeed = ti.Seed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to deserialize terrain: " + ex.Message);
                    ti = null;
                }

            if ( ti == null )
                ti = new TerrainGenerator() {
                    Seed = lastSeed,
                    Width = width,
                    Height = height};

            ti.GroundVerticalExtent = chkGroundAmplitude.Checked ? trackBarGroundAmplitude.Value : -1;
            ti.GroundBumpiness = chkGroundBumpiness.Checked ? trackBarGroundBumpiness.Value : -1;
            ti.CaveComplexity = chkCaveComplexity.Checked ? trackBarCaveComplexity.Value : -1;

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

        private void chkGroundHeightVariation_CheckedChanged(object sender, EventArgs e)
        {
            trackBarGroundAmplitude.Enabled = chkGroundAmplitude.Checked;
        }

        private void chkGroundBumpiness_CheckedChanged(object sender, EventArgs e)
        {
            trackBarGroundBumpiness.Enabled = chkGroundBumpiness.Checked;
        }

        private void chkCaveComplexity_CheckedChanged(object sender, EventArgs e)
        {
            trackBarCaveComplexity.Enabled = chkCaveComplexity.Checked;
        }
    }
}
