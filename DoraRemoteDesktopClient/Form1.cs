using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoraRemoteDesktopClient
{
    public partial class ClientSubForm : Form
    {
        public ClientSubForm()
        {
            InitializeComponent();
        }

        public void AvailableUpdate(int param)
        {
            label1.Text = param.ToString();
            label1.Update();
        }
    }
}
