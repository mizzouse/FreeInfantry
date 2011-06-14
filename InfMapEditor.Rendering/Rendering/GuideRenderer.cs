using System.Collections.Generic;
using System.Drawing;
using InfMapEditor.Rendering.Helpers;
using SlimDX;
using SlimDX.Direct3D9;

namespace InfMapEditor.Rendering.Rendering
{
    internal class GuideRenderer
    {
        internal static float RenderingZOffset = 0.8f;

        public int ColumnSpan
        {
            get { return columnSpan; }
            set { columnSpan = value; ResetGrid(); }
        }

        public int RowSpan
        {
            get { return rowSpan; }
            set { rowSpan = value; ResetGrid(); }
        }

        public int Transparency
        {
            get; set;
        }

        public Color LineColor
        {
            get { return color; }
            set { color = value; ResetGrid(); }
        }

        public Rectangle Viewport
        {
            get { return viewport; }
            set { viewport = value; ResetGrid(); }
        }

        public GuideRenderer(Device device, Rectangle viewport, Color color, int colInterval, int rowInterval, int transparency)
        {
            this.device = device;
            this.viewport = viewport;
            this.color = color;
            this.columnSpan = colInterval;
            this.rowSpan = rowInterval;
            this.Transparency = transparency;

            ResetGrid();
        }

        public void Render()
        {
            device.SetTexture(0, null);

            vertexBuffer.Render();
        }

        private void ResetGrid()
        {
            if(vertexBuffer != null)
            {
                vertexBuffer.Dispose();
            }

            vertexBuffer = new ExpandableVertexBuffer<ColorVertex>(device, PrimitiveType.LineList, Usage.None,
                                                                   VertexFormat.None, Pool.Managed);

            int iX = viewport.X%columnSpan;
            int iY = viewport.Y%rowSpan;

            int fX = viewport.Width;
            int fY = viewport.Height;

            int numOfColumns = viewport.Width/columnSpan;
            int numOfRows = viewport.Height/rowSpan;

            var colVertices = new List<ColorVertex>(numOfColumns * 2);
            var rowVertices = new List<ColorVertex>(numOfRows * 2);

            for(int i = iX; i < fX; i += columnSpan)
            {
                var v0 = new ColorVertex();
                var v1 = new ColorVertex();

                v0.Position = new Vector4(i, 0, 0.5f, 1.0f);
                v0.Color = color.ToArgb();

                v1.Position = new Vector4(i, viewport.Height, 0.5f, 1.0f);
                v1.Color = color.ToArgb();

                colVertices.Add(v0);
                colVertices.Add(v1);
            }

            for(int i = iY; i < fY; i += rowSpan)
            {
                var v0 = new ColorVertex();
                var v1 = new ColorVertex();

                v0.Position = new Vector4(0, i, 0.5f, 1.0f);
                v0.Color = color.ToArgb();

                v1.Position = new Vector4(viewport.Width, i, 0.5f, 1.0f);
                v1.Color = color.ToArgb();

                rowVertices.Add(v0);
                rowVertices.Add(v1);
            }

            vertexBuffer.AddVertices(colVertices);
            vertexBuffer.AddVertices(rowVertices);
        }

        private int columnSpan;
        private int rowSpan;
        private Color color;
        private Rectangle viewport;
        private readonly Device device;
        private ExpandableVertexBuffer<ColorVertex> vertexBuffer;
    }
}
