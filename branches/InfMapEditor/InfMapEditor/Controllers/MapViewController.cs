using System;
using System.Drawing;
using System.Windows.Forms;
using InfMapEditor.DataStructures;
using InfMapEditor.Rendering;
using InfMapEditor.Rendering.Spatial;
using InfMapEditor.Views.Main.Partials;

namespace InfMapEditor.Controllers
{
    public class MapViewController
    {
        #region Delegates

        public event MouseDownDelegate MouseDown;

        public event MouseUpDelegate MouseUp;

        public event SizeChangedDelegate SizeChanged;

        public delegate void SizeChangedDelegate(Rectangle newSize);

        public delegate void MouseMoved(int x, int y);

        public delegate void MouseDownDelegate(MouseEventArgs mouse);

        public delegate void MouseUpDelegate(MouseEventArgs mouse);

        public delegate BlobImage SelectedTileDelegate();

        public MouseMoved OnMouseMoved;

        #endregion

        public MapViewController(MapControl map)
        {
            this.map = map;
            InitMapEventHandling();

            Rectangle viewport = new Rectangle(0, 0, map.Width, map.Height);
            renderer = new Renderer(map.Handle, viewport);
        }

        public void Refresh()
        {
            renderer.Render();
        }

        public void SetSelectedTileDelegate(SelectedTileDelegate del)
        {
            selectedTile = del;
        }

        private void InitMapEventHandling()
        {
            map.SizeChanged += Map_OnSizeChanged;
            map.MouseMove += Map_OnMouseMove;
            map.MouseDown += Map_OnMouseDown;
            map.MouseUp += Map_OnMouseUp;
            map.ScrollChanged += Map_OnScrollChanged;
        }

        #region MapControl Event Handling

        private void Map_OnSizeChanged(object sender, EventArgs e)
        {
            Rectangle newSize = new Rectangle(0, 0, map.Width, map.Height);

            renderer.Viewport = newSize;
            
            if(SizeChanged != null)
            {
                SizeChanged(newSize);
            }
        }

        private void Map_OnMouseMove(object sender, MouseEventArgs e)
        {
            if(selectionStarted)
            {
                renderer.UpdateSelection(new Point(e.X, e.Y));
                map.Cursor = Cursors.Cross;
            }
        }

        private void Map_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                selectionStarted = true;
                renderer.StartSelection(new Point(e.X, e.Y));
            }
            else
            {
                BlobImage img = selectedTile();

                if (img == null)
                    return;

                CellData.FloorData floor = new CellData.FloorData();
                floor.Image = img;

                renderer.SetFloorAt(floor, e.X, e.Y);
            }

            if(MouseDown != null)
            {
                MouseDown(e);
            }
        }

        private void Map_OnMouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                selectionStarted = false;
                map.Cursor = Cursors.Default;
            }

            if(MouseUp != null)
            {
                MouseUp(e);
            }
        }

        private void Map_OnScrollChanged(int x, int y)
        {
            renderer.Offset = new Size(x, y);
            renderer.Render();
        }

        #endregion

        private MapControl map;
        private Renderer renderer;
        private SelectedTileDelegate selectedTile;
        private bool selectionStarted;
    }
}
