using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace InfLauncher.Views
{
    public partial class NewsForm : Form
    {
        public NewsForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="title"></param>
        public void SetTitle(string title)
        {
            lblTitle.Text = title;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public void SetURL(string url)
        {
            urlPostAddress.Text = url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        public void SetDescription(string description)
        {
            lblDescription.Text = description;
        }

        private void lblDescription_Click(object sender, EventArgs e)
        {

        }

        private void urlPostAddress_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(urlPostAddress.Text);
        }
    }
}
