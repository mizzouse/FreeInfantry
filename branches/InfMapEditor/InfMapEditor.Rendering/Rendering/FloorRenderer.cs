using System.Collections.Generic;
using System.Drawing;
using System.IO;
using InfMapEditor.DataStructures;
using InfMapEditor.Rendering.Helpers;
using InfMapEditor.Rendering.Spatial;
using SlimDX;
using SlimDX.Direct3D9;
using Texture = SlimDX.Direct3D9.Texture;

namespace InfMapEditor.Rendering.Rendering
{
    internal class FloorRenderer
    {
        public FloorRenderer(Device device)
        {
            this.device = device;
            this.buffers = new Dictionary<BlobImage, KeyValuePair<Texture, ExpandableVertexBuffer<TexturedVertex>>>();
        }

        public void Render(Grid.GridRange range, Rectangle viewport)
        {
            // 0. Clear buffers.
            ////
            foreach(var buffer in buffers)
            {
                buffer.Value.Value.ClearVertices();
            }

            // 1. Fill buffers.
            ////
            foreach(Grid.GridCell cell in range)
            {
                if(cell.Data == null)
                    continue;

                BlobImage image = cell.Data.Floor.Image;

                // Loading in a texture for the first time?
                if(!buffers.ContainsKey(image))
                {
                    Bitmap bitmap = image.Image;
                    using(Stream s = new MemoryStream())
                    {
                        bitmap.Save(s, bitmap.RawFormat);
                        s.Seek(0, SeekOrigin.Begin);
                        Texture tex = Texture.FromStream(device, s, bitmap.Width, bitmap.Height, 0, Usage.None,
                                                         Format.Unknown,
                                                         Pool.Managed, Filter.None, Filter.None, 0);

                        var vb = new ExpandableVertexBuffer<TexturedVertex>(device, PrimitiveType.TriangleList,
                                                                                Usage.None, VertexFormat.None,
                                                                                Pool.Managed);

                        var pair = new KeyValuePair<Texture, ExpandableVertexBuffer<TexturedVertex>>(tex, vb);

                        buffers.Add(image, pair);
                    }
                }

                int pixelsPerCell = 8;
                int x = (cell.X * pixelsPerCell) - viewport.X;
                int y = (cell.Y * pixelsPerCell) - viewport.Y;

                int w = 8;
                int h = 8;

                // Calculate the (u,v) we need to use based on the tile coordinates.
                float scaleW = 8.0f/image.Image.Width;
                float scaleH = 8.0f/image.Image.Height;

                float uStart = cell.X * scaleW;
                float vStart = cell.Y * scaleH;

                float uEnd = uStart + scaleW;
                float vEnd = vStart + scaleH;

                // Clockwise winding
                // TODO: Index these vertices! argh
                var v0 = new TexturedVertex(new Vector4(x, y, 0.5f, 1.0f), new Vector2(uStart, vStart));
                var v1 = new TexturedVertex(new Vector4(x + w, y, 0.5f, 1.0f), new Vector2(uEnd, vStart));
                var v2 = new TexturedVertex(new Vector4(x + w, y + h, 0.5f, 1.0f),
                                            new Vector2(uEnd, vEnd));

                var v3 = new TexturedVertex(new Vector4(x, y, 0.5f, 1.0f), new Vector2(uStart, vStart));
                var v4 = new TexturedVertex(new Vector4(x + w, y + h, 0.5f, 1.0f),
                                            new Vector2(uEnd, vEnd));
                var v5 = new TexturedVertex(new Vector4(x, y + h, 0.5f, 1.0f), new Vector2(uStart, vEnd));

                buffers[image].Value.AddVertex(v0);
                buffers[image].Value.AddVertex(v1);
                buffers[image].Value.AddVertex(v2);
                buffers[image].Value.AddVertex(v3);
                buffers[image].Value.AddVertex(v4);
                buffers[image].Value.AddVertex(v5);
            }

            // 2. Draw.
            ////
            foreach(var buffer in buffers.Values)
            {
                device.SetTexture(0, buffer.Key);
                buffer.Value.Render();
            }
        }

        private Device device;
        private Dictionary<BlobImage, KeyValuePair<Texture, ExpandableVertexBuffer<TexturedVertex>>> buffers;
    }
}
