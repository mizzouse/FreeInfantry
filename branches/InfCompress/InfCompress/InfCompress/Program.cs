using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Xml;

// File Name="Alien2.blo" CRC="-1797125014" Compression=".gz" DownloadSize="108653" MD5="98A59217A5EC1258382BE16B7CDE127D" Size="188860" />

namespace InfCompress
{
    /// <summary>
    /// XML file model.
    /// </summary>
    class AssetFile
    {
        public string Name;
        public const int Crc = -1;
        public const string Compression = ".gz";
        public long DownloadSize;
        public string Md5;
        public long Size;

        public override string ToString()
        {
            return String.Format("Name: {0}, Size: {1}, Md5: {2}", Name, Size, Md5);
        }

        public void WriteElement(XmlWriter writer)
        {
            writer.WriteStartElement("File");
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("CRC", Crc.ToString());
            writer.WriteAttributeString("Compression", Compression);
            writer.WriteAttributeString("DownloadSize", DownloadSize.ToString());
            writer.WriteAttributeString("MD5", Md5);
            writer.WriteAttributeString("Size", Size.ToString());
            writer.WriteEndElement();
        }
    }

    class Program
    {
        public static string ManifestFile = @"manifest.xml";
        public static string ProgramFile = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName;

        static void Main(string[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            FileInfo[] files = directory.GetFiles();

            List<AssetFile> assets = new List<AssetFile>();

            // 1. Filter out the files
            foreach(var file in files)
            {
                if (file.Name == ManifestFile || file.Name == ProgramFile || file.Extension == AssetFile.Compression) continue;

                // Extract the first bit of data out of this.
                var asset = new AssetFile();

                asset.Name = file.Name;
                asset.Size = file.Length;
                asset.Md5 = GetMD5HashFromFile(file.Name);

                assets.Add(asset);
            }

            // 2. Compress files
            foreach(var asset in assets)
            {
                var fi = new FileInfo(asset.Name);

                using (FileStream inFile = fi.OpenRead())
                {
                    using (FileStream outFile = File.Create(fi.FullName + ".gz"))
                    {
                        using (GZipStream Compress = new GZipStream(outFile, CompressionMode.Compress))
                        {
                            inFile.CopyTo(Compress);

                            FileInfo fout = new FileInfo(fi.FullName + ".gz");
                            asset.DownloadSize = fout.Length;

                        }
                    }
                }
            }

            
            // 3. Generate manifest
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter writer = XmlWriter.Create("manifest.xml", settings);

            writer.WriteStartDocument();
            writer.WriteStartElement("FIPatcher");
            foreach(var asset in assets)
            {
                asset.WriteElement(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Flush();
            writer.Close();

            Console.WriteLine("Done, press any key to exit.");
            Console.Read();
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
