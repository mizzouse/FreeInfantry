using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using InfMapEditor.DataStructures;
using InfMapEditor.Views.Main.Partials;
using InfMapEditor.Views.Palettes;

namespace InfMapEditor.Views.Main
{
    public partial class MainForm : Form
    {
        public event FloorSelectedDelegate FloorSelected;

        public event GuideChangedDelegate GuideChanged;

        public delegate List<BlobImage> FloorImagesDelegate();

        public delegate void FloorSelectedDelegate(BlobImage floorImage);

        public delegate void GuideChangedDelegate(bool enabled, int colInterval, int rowInterval, Color color);

        public MapControl MapControl { get { return mapControl; } }

        public MinimapControl MinimapControl { get { return minimap; } }

        public MainForm()
        {
            InitializeComponent();
        }

        public void SetFloorImagesDelegate(FloorImagesDelegate del)
        {
            floorImagesDelegate = del;
        }

        public void UpdatePositionLabel(int x, int y)
        {
            statusLabelMousePosition.Text = String.Format("Map Pos ({0}, {1})", x, y);
        }

        private void windowMenuItemNewTerrainPalette_Click(object sender, EventArgs e)
        {
            if (floorImagesDelegate != null)
            {
                List<BlobImage> floorBlobs = floorImagesDelegate();
                FloorPalette floorPalette = new FloorPalette(floorBlobs);

                floorPalette.TerrainSelected +=
                    delegate(BlobImage image) { if (FloorSelected != null) FloorSelected(image); };

                floorPalette.Show();
            }
        }

        private void editMenuItemShowGrid_Click(object sender, EventArgs e)
        {
            GridForm grid = new GridForm();

            grid.ShowDialog();

            if(GuideChanged != null)
            {
                GuideChanged(true, 0, 0, Color.Black);
            }
        }

        private FloorImagesDelegate floorImagesDelegate;

        private void menuFileImportItemLevel_Click(object sender, EventArgs e)
        {

        }
    }
}
