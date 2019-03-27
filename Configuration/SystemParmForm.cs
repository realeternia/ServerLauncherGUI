using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DGServerControllerGUI.Tools;

namespace DGServerControllerGUI.Configuration
{
    public partial class SystemParmForm : Form
    {
        public SystemParmForm()
        {
            InitializeComponent();
        }

        private void SystemParmForm_Load(object sender, EventArgs e)
        {
            var results = ProcessHelper.StartProcessResult("DGServerController.exe", " -ip").Replace("\r","").Split('\n');
            foreach (var result in results)
            {
                if (result.Length <= 0 || result[0] == '[')
                    continue;

                var dataDetails = result.Split('-');
                UCSystemParm parm = new UCSystemParm();
                parm.Set(dataDetails[0], dataDetails[1], dataDetails[2], dataDetails[3]);
                flowLayoutPanel1.Controls.Add(parm);
            }
            
        }
    }
}
