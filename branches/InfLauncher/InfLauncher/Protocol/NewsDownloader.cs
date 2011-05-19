using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using InfLauncher.Models;

namespace InfLauncher.Protocol
{
    public class NewsDownloader
    {
        /// <summary>
        /// The persistent http downloading object.
        /// </summary>
        private WebClient webClient;

        /// <summary>
        /// 
        /// </summary>
        private string baseDirectoryUrl;

        /// <summary>
        /// Creates a new NewsDownloader object given the location of the file list that references the news
        /// to be downloaded.
        /// </summary>
        /// <param name="baseDirectoryUrl">XML file list</param>
        public NewsDownloader(string baseDirectoryUrl)
        {
            if (baseDirectoryUrl == null)
            {
                throw new ArgumentNullException("baseDirectoryUrl");
            }

            DownloadNewsFile(baseDirectoryUrl);

            this.baseDirectoryUrl = baseDirectoryUrl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalPercentageComplete"></param>
        public delegate void NewsFileProgressChanged(int totalPercentageComplete);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetList"></param>
        public delegate void NewsFileDownloadCompleted(List<News> NewsList);

        /// <summary>
        /// 
        /// </summary>
        public NewsFileProgressChanged OnNewsFileDownloadProgressChanged;

        /// <summary>
        /// 
        /// </summary>
        public NewsFileDownloadCompleted OnNewsFileDownloadCompleted;

        /// <summary>
        /// Downloads the XML file that contains our news.
        /// </summary>
        /// <param name="newsFileName"></param>
        public void DownloadNewsFile(string newsFileName)
        {
            if (newsFileName == null)
            {
                throw new ArgumentNullException("newsFileName");
            }

            webClient = new WebClient();

            // Hook in the async delegates
            webClient.DownloadProgressChanged += NewsListProgressChanged;
            webClient.DownloadDataCompleted += NewsListDownloadCompleted;

            webClient.DownloadDataAsync(new Uri(newsFileName));
        }


        private void NewsListProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            OnNewsFileDownloadProgressChanged(e.ProgressPercentage);
        }

        private void NewsListDownloadCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            var parser = new XmlNewsFileParser(Encoding.UTF8.GetString(e.Result));
            OnNewsFileDownloadCompleted(parser.NewsList);
        }

        /// <summary>
        /// 
        /// </summary>
        private class XmlNewsFileParser
        {
            public List<News> NewsList { get; private set; }

            public XmlNewsFileParser(string fileData)
            {
                NewsList = new List<News>();

                try
                {
                    XDocument doc = XDocument.Parse(fileData);
                    foreach (XElement post in doc.Descendants("news"))
                    {
                        NewsList.Add(new News(post.Element("title").Value,
                            post.Element("url").Value, 
                            post.Element("description").Value
                            ));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
    }
}
