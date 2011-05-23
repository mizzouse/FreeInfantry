using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using InfLauncher.Controllers;
using InfLauncher.Helpers;
using InfLauncher.Models;

namespace InfLauncher.Views
{
    public partial class MainForm : Form
    {
        private MainController _controller;

        public MainForm()
        {
            InitializeComponent();
        }

        public MainForm(MainController controller)
        {
            InitializeComponent();

            btnPlay.Enabled = false;
            _controller = controller;
        }

        public void SetPlayButtonState(bool enabled)
        {
            btnPlay.Enabled = enabled;
        }

        public void SetNews(News news)
        {
            lblNewsTitle.Text = news.Title;
            lblNewsDescription.Text = news.Description;

            lblNewsLink.Tag = news.URL;
        }

        #region View Handlers

        private void btnLogin_Click(object sender, System.EventArgs e)
        {
            var username = txtboxUsername.Text;
            var password = txtboxPassword.Text;

            _controller.LoginAccount(new Account.AccountLoginRequestModel(username, password));
        }

        private void btnNewAccount_Click(object sender, System.EventArgs e)
        {
            _controller.CreateNewAccountForm();
        }

        private void btnPlay_Click(object sender, System.EventArgs e)
        {
            var infantryProcess = new Process();
            infantryProcess.StartInfo.FileName = Path.Combine(Config.GetConfig().InstallPath, "infantry.exe");
            infantryProcess.StartInfo.Arguments = string.Format("/ticket:{0} /name:{1}", _controller.GetSessionId(), "Jovan");

            infantryProcess.Start();
        }

        private void linkWebsite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.aaerox.com");
        }

        #endregion

        private void lblNewsLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel label = (LinkLabel) sender;
            Process.Start((string) label.Tag);
        }
    }
}
