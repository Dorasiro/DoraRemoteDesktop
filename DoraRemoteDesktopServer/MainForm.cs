using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoraRemoteDesktopServer
{
    public partial class MainForm : Form
    {
        private SubForm s;

        public MainForm()
        {
            InitializeComponent();

            s = new SubForm(pictureBox1);
            s.Show();
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            s.DisposeBitmap();
            s.Dispose();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            
        }
    }
}
