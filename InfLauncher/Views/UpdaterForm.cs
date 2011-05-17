using System.Windows.Forms;

namespace InfLauncher.Views
{
    public partial class UpdaterForm : Form
    {
        public UpdaterForm()
        {
            InitializeComponent();
        }

        public void SetCurrentTask(string task)
        {
            lblTask.Text = task;
        }

        public void SetFileCounts(int finished, int total)
        {
            lblFileCount.Text = string.Format("{0} / {1}", finished, total);
        }

        public void SetFilename(string filename)
        {
            lblCurrentFilename.Text = filename;
        }

        public void SetProgress(int progress)
        {
            progressBar.Value = progress;
        }
    }
}
