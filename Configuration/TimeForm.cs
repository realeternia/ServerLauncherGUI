using System;
using System.Windows.Forms;
using DGServerControllerGUI.Tools;

namespace DGServerControllerGUI.Configuration
{
    public partial class TimeForm : Form
    {
        private bool canSync = false;

        public TimeForm()
        {
            InitializeComponent();
        }

        private void TimeForm_Load(object sender, EventArgs e)
        {
            var results = ProcessHelper.StartProcessResult("DGServerController.exe", " -time").Replace("\r", "").Split('\n');
            foreach (var result in results)
            {
                if (result.Length <= 0 || result[0] == '[')
                    continue;

                dateTimePicker1.Value = U2D(uint.Parse(result));
            }
            canSync = true;
        }

        private static uint D2U(DateTime dt)
        {
            var start = new DateTime(1970, 1, 1).ToLocalTime();
            return (uint)((dt - start).TotalSeconds);
        }
        private static DateTime U2D(uint dt)
        {
            return new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(dt);
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
        }

        private void dateTimePicker1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!canSync)
            {
                return;
            }

            ProcessHelper.StartProcessResult("DGServerController.exe", string.Format(" -timem {0}", D2U(dateTimePicker1.Value)));

        }
    }
}
