using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using InfMapEditor.DataStructures;
using InfMapEditor.Helpers;
using InfMapEditor.Views.Main;

namespace InfMapEditor.Controllers
{
    public class MainController : ApplicationContext 
    {
        public MainController()
        {
            mainForm = new MainForm();
            mainForm.FloorSelected += OnFloorSelected;
            mainForm.GuideChanged += OnGuideChanged;

            mapViewController = new MapViewController(mainForm.MapControl);
            mapViewController.OnMouseMoved += OnMouseMoved;

            LoadBlobs();

            mainForm.SetFloorImagesDelegate(() => blobImages);
            mapViewController.SetSelectedTileDelegate(() => selectedBlobImage);

            // Show form
            MainForm = mainForm;
        }

        public void PostWindowsLoop()
        {
            mapViewController.Refresh();
        }

        private void LoadBlobs()
        {
            var directory = new DirectoryInfo("assets");
            FileInfo[] floorFiles = directory.GetFiles("f_*.blo");

            blobImages = BlobLoader.GetBlobsFrom(floorFiles);
        }

        private void OnMouseMoved(int x, int y)
        {
            mainForm.UpdatePositionLabel(x, y);
        }

        private void OnFloorSelected(BlobImage blob)
        {
            selectedBlobImage = blob;
        }

        private void OnGuideChanged(bool enabled, int colInterval, int rowInterval, Color color)
        {
            
        }

        private List<BlobImage> blobImages;
        private readonly MapViewController mapViewController;
        private readonly MainForm mainForm;
        private BlobImage selectedBlobImage;
    }
}
