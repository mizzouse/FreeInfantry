using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace InfCompress
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("ONE SPEED, GO!");
            // Path to directory of files to compress
            string assets = System.IO.Directory.GetCurrentDirectory();

            DirectoryInfo directory = new DirectoryInfo(assets);

            //loop through the running directory for our files
            for (int i = 0; i < directory.GetFiles().Length; i++)
            {
                FileInfo currentFile = directory.GetFiles().ElementAt(i);

                //Don't try compressing us...
                if (currentFile.Name == "InfCompress.exe")
                    continue;
                
                //Compress it..
                Compress(currentFile);
            }

            Console.WriteLine("Done, press any key to exit.");
            Console.Read();
        }

        public static void Compress(FileInfo fi)
        {
            using (FileStream inFile = fi.OpenRead())
            {
                //Prevent compressing hidden and already compressed files
                if ((File.GetAttributes(fi.FullName)
                    & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fi.Extension != ".gz")
                {
                    //Create the compressed file.
                    using (FileStream outFile =
                                File.Create(fi.FullName + ".gz"))
                    {
                        using (GZipStream Compress =
                            new GZipStream(outFile,
                            CompressionMode.Compress))
                        {
                            //Copy the source file into 
                            //the compression stream.
                            inFile.CopyTo(Compress);

                            Console.WriteLine("Compressed {0} from {1} to {2} bytes.",
                                fi.Name, fi.Length.ToString(), outFile.Length.ToString());
                        }
                    }
                }
            }
        }
    }
}
