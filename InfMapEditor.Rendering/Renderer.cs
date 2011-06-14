using System;
using System.Collections.Generic;
using System.Drawing;
using InfMapEditor.Rendering.Rendering;
using InfMapEditor.Rendering.Spatial;
using SlimDX.Direct3D9;

namespace InfMapEditor.Rendering
{
    public class Renderer
    {
        public static Color DefaultGuideColor = Color.White;
        public static int DefaultGuideColumnInterval = 8;
        public static int DefaultGuideRowInterval = 8;
        public static int DefaultGuideTransparency = 0;

        public enum Layers
        {
            Floor,
            Guide,
            Object,
            Physics,
            Vision,
            Selection,
        }

        public Rectangle Viewport
        {
            get { return viewport; }
            set 
            {
                viewport = value;
                ResetDevice();
                guides.Viewport = viewport;
            }
        }

        public Renderer(IntPtr mapHwnd, Rectangle initialViewport)
        {
            layerStates = new Dictionary<Layers, bool>
                              {
                                  {Layers.Floor, true},
                                  {Layers.Guide, false},
                                  {Layers.Object, true},
                                  {Layers.Physics, true},
                                  {Layers.Vision, true},
                                  {Layers.Selection, false},
                              };

            var presentParams = new PresentParameters
                                         {
                                             BackBufferWidth = initialViewport.Width,
                                             BackBufferHeight = initialViewport.Height,
                                             Windowed = true,
                                         };

            device = new Device(new Direct3D(), 0, DeviceType.Hardware, mapHwnd, CreateFlags.HardwareVertexProcessing,
                                presentParams);

            viewport = initialViewport;
            floors = new FloorRenderer(device);
            guides = new GuideRenderer(device, initialViewport, DefaultGuideColor, DefaultGuideColumnInterval,
                                       DefaultGuideRowInterval, DefaultGuideTransparency);

            grid = new Grid(2048, 2048);
        }

        public void SetLayerEnabled(Layers layer, bool enabled)
        {
            layerStates[layer] = enabled;
        }

        public void SetFloorAt(CellData.FloorData floor, int pixelX, int pixelY)
        {
            int[] coords = grid.PixelsToGridCoordinates(pixelX + viewport.X, pixelY + viewport.Y);
            CellData data = grid.Get(coords[0], coords[1]);

            if(data == null)
            {
                data = new CellData();
            }
            data.Floor = floor;
            grid.Insert(data, coords[0], coords[1]);
        }

        public void Render()
        {
            Grid.GridRange visibleRange = grid.GetRange(viewport.X, viewport.Y, viewport.X + viewport.Width,
                                                        viewport.Y + viewport.Height);

            device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Black, 1.0f, 0);
            device.BeginScene();

            if(layerStates[Layers.Floor])
            {
                floors.Render(visibleRange, viewport);
            }
            if(layerStates[Layers.Guide])
            {
                guides.Render();
            }

            device.EndScene();
            device.Present();
        }

        private void ResetDevice()
        {
            var presentParams = new PresentParameters
            {
                BackBufferWidth = viewport.Width,
                BackBufferHeight = viewport.Height,
                Windowed = true,
            };

            device.Reset(presentParams);
        }

        private Dictionary<Layers, bool> layerStates;
        private FloorRenderer floors;
        private GuideRenderer guides;
        private Grid grid;
        private Device device;
        private Rectangle viewport;
    }
}
