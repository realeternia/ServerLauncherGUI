using System;
using System.Windows.Forms;
using DGServerControllerGUI.Tools;

namespace DGServerControllerGUI.Configuration
{
    public partial class UCSystemParm : UserControl
    {
        private string section;
        private string key;
        private string value;
        private bool canWrite;

        private bool finishSet;
        private string valType;

        public UCSystemParm()
        {
            InitializeComponent();
        }

        public void Set(string sect, string key, string val, string canwrite)
        {
            section = sect;
            this.key = key;
            this.value = val;
            this.canWrite = (canwrite.ToLower() == "true");
            label1.Text = string.Format("{0}/{1}", section, key);
            textBox1.Text = val;
            if (!canWrite)
                textBox1.Enabled = false;

            valType = "string";
            int testVal;
            if (int.TryParse(val, out testVal))
                valType = "int";
            bool testValb;
            if (bool.TryParse(val, out testValb))
                valType = "bool";
            finishSet = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!finishSet)
                return;

            int testVal;
            if (valType == "int" && !int.TryParse(textBox1.Text, out testVal))
                return;
            bool testValb;
            if (valType == "bool" && !bool.TryParse(textBox1.Text, out testValb))
                return;

            value = textBox1.Text;
            ProcessHelper.StartProcessResult("DGServerController.exe", string.Format(" -im {0} {1} {2}", section, key, value));
        }
    }
}
