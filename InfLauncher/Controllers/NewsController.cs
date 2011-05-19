using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InfLauncher.Helpers;
using InfLauncher.Models;
using InfLauncher.Protocol;
using InfLauncher.Views;

namespace InfLauncher.Controllers
{
    public class NewsController
    {
        /// <summary>
        /// Form showing the news
        /// </summary>
        private NewsForm form = new NewsForm();

        /// <summary>
        /// News downloader
        /// </summary>
        private NewsDownloader newsDownloader;

        public NewsController(string baseUrlDirectory)
        {
            if(baseUrlDirectory == null)
            {
                throw new ArgumentNullException("baseUrlDirectory");
            }
            form.Show();

            newsDownloader = new NewsDownloader(baseUrlDirectory);

            newsDownloader.OnNewsFileDownloadProgressChanged += OnNewsFileDownloadProgressChanged;
            newsDownloader.OnNewsFileDownloadCompleted += OnNewsFileDownloadCompleted;
        }

        private void OnNewsFileDownloadProgressChanged(int totalPercentage)
        {
            // Not used
        }

        private void OnNewsFileDownloadCompleted(List<News> newsList)
        {
            foreach (News newsPost in newsList)
            {
                form.SetTitle(newsPost.Title);
                form.SetURL(newsPost.URL);
                form.SetDescription(newsPost.Description);
            }
        }
    }
}
