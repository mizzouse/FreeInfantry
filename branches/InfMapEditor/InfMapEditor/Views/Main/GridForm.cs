using System;
using System.Windows.Forms;

namespace InfMapEditor.Views.Main
{
    public partial class GridForm : Form
    {
        public GridForm()
        {
            InitializeComponent();
        }

        private void checkShowGrid_CheckedChanged(object sender, EventArgs e)
        {
            if(checkShowGrid.Checked)
            {
                boxGridSize.Enabled = true;
            }
            else
            {
                boxGridSize.Enabled = false;
            }
        }

        private void panelColorDisplay_DoubleClick(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            var result = c.ShowDialog();

            if (result == DialogResult.OK)
            {
                panelColorDisplay.BackColor = c.Color;
            }
        }
    }
}
