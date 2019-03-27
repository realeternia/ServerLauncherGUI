using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using DGServerControllerGUI.Configuration;
using DGServerControllerGUI.Tools;

namespace DGServerControllerGUI
{
    public delegate void AppendTextCallback(int index, string text, LogShowMode mode);
    public delegate void ResetTextCallback(List<LogTextManager.LogItem> items);
    public delegate void RemoveTextCallback(int index);

    public partial class Form1 : Form
    {
        private DateTime lastFocusTime; //增加刷新间隔，降低延迟
        private void AppendText(int index, string text, LogShowMode mode)
        {

            if (this.listView1.InvokeRequired)//如果调用控件的线程和创建创建控件的线程不是同一个则为True
            {
                AppendTextCallback d = new AppendTextCallback(AppendText);
                this.listView1.Invoke(d, new object[] { index, text, mode });
            }
            else
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = text;
                lvi.Tag = index;
                switch (mode)
                {
                    case LogShowMode.DEBUG: lvi.ForeColor = ConfigObject.Instance.DebugForeColor; lvi.BackColor = ConfigObject.Instance.DebugBackColor; break;
                    case LogShowMode.NOTIC: lvi.ForeColor = ConfigObject.Instance.NoticForeColor; lvi.BackColor = ConfigObject.Instance.NoticBackColor; break;
                    case LogShowMode.WARN: lvi.ForeColor = ConfigObject.Instance.WarnForeColor; lvi.BackColor = ConfigObject.Instance.WarnBackColor; break;
                    case LogShowMode.ERROR: lvi.ForeColor = ConfigObject.Instance.ErrorForeColor; lvi.BackColor = ConfigObject.Instance.ErrorBackColor; break;
                    case LogShowMode.FATAL: lvi.ForeColor = ConfigObject.Instance.FatalForeColor; lvi.BackColor = ConfigObject.Instance.FatalBackColor; break;

                }

                if (text.Contains("[Tmp]"))
                {
                    lvi.Text = lvi.Text.Replace("[Tmp]", "-------------TMP DEBUG LOG------------");
                    lvi.Font = new Font("Arial", 11, FontStyle.Underline | FontStyle.Bold);
                }

                this.listView1.Items.Add(lvi);
                if (lastFocusTime == null || (DateTime.Now - lastFocusTime).TotalSeconds >= 0.2)
                {
                    this.listView1.EnsureVisible(this.listView1.Items.Count - 1);
                    this.listView1.Items[this.listView1.Items.Count - 1].Checked = true;    
                    lastFocusTime = DateTime.Now;
                }
            }
        }

        private void ResetText(List<LogTextManager.LogItem> items)
        {
            if (this.listView1.InvokeRequired)//如果调用控件的线程和创建创建控件的线程不是同一个则为True
            {
                ResetTextCallback d = new ResetTextCallback(ResetText);
                this.listView1.Invoke(d, new object[] { items });
            }
            else
            {
                listView1.Items.Clear();
                foreach (var logItem in items)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = logItem.Text;
                    lvi.Tag = logItem.Id;
                    switch (logItem.Type)
                    {
                        case LogShowMode.DEBUG: lvi.ForeColor = Color.White; lvi.BackColor = Color.Black; break;
                        case LogShowMode.NOTIC: lvi.ForeColor = Color.White; lvi.BackColor = Color.Green; break;
                        case LogShowMode.WARN: lvi.ForeColor = Color.Orange; lvi.BackColor = Color.Black; break;
                        case LogShowMode.ERROR: lvi.ForeColor = Color.Red; lvi.BackColor = Color.White; break;
                        case LogShowMode.FATAL: lvi.ForeColor = Color.White; lvi.BackColor = Color.Red; break;
                    }
                    this.listView1.Items.Add(lvi);
                }

                if (listView1.Items.Count > 0)
                {
                    this.listView1.EnsureVisible(this.listView1.Items.Count - 1);
                    this.listView1.Items[this.listView1.Items.Count - 1].Checked = true;
                }
            }
        }

        private void RemoveText(int index)
        {
            if (this.listView1.InvokeRequired)//如果调用控件的线程和创建创建控件的线程不是同一个则为True
            {
                RemoveTextCallback d = new RemoveTextCallback(RemoveText);
                this.listView1.Invoke(d, new object[] { index });
            }
            else
            {
                if(listView1.Items.Count > 0 && int.Parse(listView1.Items[0].Tag.ToString()) == index)
                    listView1.Items.RemoveAt(0);
            }
        }

        private int fastRefreshTickCount;
        private Thread startupThread;

        public Form1()
        {
            InitializeComponent();
        }

        private bool isOpen = true;
        private void button1_Click(object sender, EventArgs e)
        {
            if (isOpen)
            {
                button1.Text = "关闭";
                isOpen = false;

                LogTextManager.AppendTextHandle = AppendText;
                LogTextManager.ResetTextHandle = ResetText;
                LogTextManager.RemoveTextHandle = RemoveText;

                startupThread = new Thread(WorkStart); //为了不阻塞主线程，启动一个线程来帮助
                startupThread.IsBackground = true;
                startupThread.Start();
            }
            else
            {
                button1.Text = "开启";
                isOpen = true;

                ProcessHelper.ClearProcess();
            }
            fastRefreshTickCount = 100;//5秒的快速刷新时间
        }

        private void WorkStart()
        {
            ProcessHelper.ClearProcess();
            if (ConfigObject.Instance.ClearLogOnReboot)
            {
                ResetText(new List<LogTextManager.LogItem>());//清除所有log
            }

            var nowPath = System.IO.Directory.GetCurrentDirectory();
            System.IO.Directory.SetCurrentDirectory(@"C:\Work\development\install");
            Environment.SetEnvironmentVariable("PYTHONPATH", @"C:\Work\development\server");
            var oldPath = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", oldPath + @";C:\Work\development\server_build_run");

            ProcessHelper.StartProcess("python", LogShowDevice.CET, " -m switcher.switcherSrv 60000 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m room.rmSrv 60700 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m db.dbSrv 60200 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m login.loginSrv 60300 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m gateway.gatewaySrv 60400 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m logic.logicSrv 60500 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m team.teamSrv 60600 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m sync.syncSrv 60800 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m gmserver.gmSrv 60900 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m iplocation.ipLocationSrv 61000 &");
            ProcessHelper.StartProcess("python", LogShowDevice.ROL, " -m monitor.monitorSrv  60100 &");

            System.IO.Directory.SetCurrentDirectory(nowPath);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ProcessHelper.ClearProcess();
        }

        private void doubleBufferedPanel1_Paint(object sender, PaintEventArgs e)
        {
            var font1 = new Font("宋体", 10);
            foreach (var processInfoData in ProcessHelper.GetAll().ToArray())
            {
                var pData = Array.Find(Process.GetProcesses(), p=>p.Id == processInfoData.ProcessId);
                if (pData != null)
                {
                    var img = ImageLoader.Load(GetIconByDev(processInfoData.Dev));
                    if (img != null)
                        e.Graphics.DrawImage(img, processInfoData.Rect);

                    Brush b = Brushes.White;
                    string info = "";
                    switch (processInfoData.State)
                    {
                        case ServerRunState.Start: info = "启动中"; break;
                        case ServerRunState.Running: info = "运行中"; b = Brushes.Lime; break;
                        case ServerRunState.Shutdown: info = "关闭中"; b = Brushes.DarkRed; break;
                    }
                    e.Graphics.DrawString(info, font1, b, processInfoData.Rect.X, processInfoData.Rect.Y + processInfoData.Rect.Height+5);
                }
            }

            e.Graphics.DrawString(string.Format("日志数 {0}/{1}", LogTextManager.LogCount, ConfigObject.Instance.MaxLogCount), font1, Brushes.Lime, 10, doubleBufferedPanel1.Height -20);
            font1.Dispose();
        }

        private Thread workThread;
        private void Form1_Load(object sender, EventArgs e)
        {
            workThread = new Thread(Work);
            workThread.IsBackground = true;
            workThread.Start();

            Text = string.Format("DGServerControllerGUI v{0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private void Work()
        {
            while (true)
            {
                doubleBufferedPanel1.Invalidate();
                if (fastRefreshTickCount > 0)
                {
                    fastRefreshTickCount --;
                    Thread.Sleep(50);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private uint nowTag = (int)LogShowMode.All;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            var control = sender as CheckBox;
            if (control.Checked)
            {
                nowTag |= uint.Parse(control.Tag.ToString());
            }
            else
            {
                nowTag &= ~uint.Parse(control.Tag.ToString());
            }
            LogTextManager.SetMode((LogShowMode)nowTag);
        }

        private uint nowTag2 = (int)LogShowDevice.All;
        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            var control = sender as CheckBox;
            if (control.Checked)
            {
                nowTag2 |= uint.Parse(control.Tag.ToString());
            }
            else
            {
                nowTag2 &= ~uint.Parse(control.Tag.ToString());
            }
            LogTextManager.SetSevice((LogShowDevice)nowTag2);
        }

        private void textBoxKey_TextChanged(object sender, EventArgs e)
        {
            LogTextManager.SetWords(textBoxKey.Text);
        }

        private void cfgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfigForm cf = new ConfigForm();
            cf.ShowDialog(this);
        }

        private void systemparmToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SystemParmForm spf = new SystemParmForm();
            spf.ShowDialog(this);
        }

        private void timeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TimeForm spf = new TimeForm();
            spf.ShowDialog(this);
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            LogTextManager.ShowLog = false;
        }

        private void buttonContinue_Click(object sender, EventArgs e)
        {
            LogTextManager.ShowLog = true;
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                StreamWriter sw = new StreamWriter("./logclip.txt");
                foreach (var selectedItem in listView1.SelectedItems)
                {
                    sw.WriteLine(( selectedItem as ListViewItem).Text);
                }
                sw.Close();
                
                System.Diagnostics.Process.Start("notepad.exe", "./logclip.txt");
            }
        }

        private void buttonClean_Click(object sender, EventArgs e)
        {
            ResetText(new List<LogTextManager.LogItem>());
            listView1.Items.Clear();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private string GetIconByDev(LogShowDevice dev)
        {
            switch (dev)
            {
                case LogShowDevice.CET: return "Cet";
                case LogShowDevice.MAY: return "May";
                case LogShowDevice.DBS: return "Db";
                case LogShowDevice.LGI: return "Lgi";
                case LogShowDevice.GAT: return "Gat";
                case LogShowDevice.ROL: return "Rol";
            }
            return null;
        }

        private void editorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo("python", "starteditor.py");
            info.WorkingDirectory = "../../";
            Process.Start(info);
        }

        private void simulatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProcessStartInfo info = new ProcessStartInfo("python", "startsimulator.py");
            info.WorkingDirectory = "../../";
            Process.Start(info);
        }
    }
}
