using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace StNetease
{
    public partial class FrmMain : Form
    {
        private NeteaseMusicAPI NeteaseMusicAPI = new NeteaseMusicAPI();
        public FrmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            var obj = this.NeteaseMusicAPI.GetLyric(32317208);
            var ss = this.NeteaseMusicAPI.GetSongComments(32317208);
        }
    }
}
