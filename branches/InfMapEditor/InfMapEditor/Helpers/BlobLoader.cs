using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using InfMapEditor.DataStructures;
using InfMapEditor.FileFormats.Infantry;

namespace InfMapEditor.Helpers
{
    internal static class BlobLoader
    {
        public static List<BlobImage> GetBlobsFrom(FileInfo[] files)
        {
            List<BlobImage> result = new List<BlobImage>(files.Length);

            foreach (FileInfo file in files)
            {
                BlobFile blob = new BlobFile();

                using (Stream s = file.Open(FileMode.Open, FileAccess.Read))
                {
                    blob.Deserialize(s);

                    foreach (var entry in blob.Entries)
                    {
                        BlobImage image = new BlobImage();
                        image.BlobReference = new BlobReference();

                        image.BlobReference.Id = entry.Name;
                        image.BlobReference.FileName = file.Name;

                        s.Seek(entry.Offset, SeekOrigin.Begin);
                        byte[] data = new byte[entry.Size];
                        s.Read(data, 0, data.Length);
                        image.Image = new Bitmap(new MemoryStream(data));

                        result.Add(image);
                    }
                }
            }

            return result;
        }
    }
}
