using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace DGServerControllerGUI.Tools
{
    public class ProcessHelper
    {
        public class ProcessInfoData
        {
            public Process Process;
            public int ProcessId;
            public LogShowDevice Dev;
            public Rectangle Rect;
            public ServerRunState State = ServerRunState.Start;
        }

        private static List<ProcessInfoData> processList = new List<ProcessInfoData>();
        public static void StartProcess(string pname, LogShowDevice dev, string parm = "")
        {
            Process process = new Process();
            process.StartInfo.FileName = pname;
            if (!string.IsNullOrEmpty(parm))
            {
                process.StartInfo.Arguments = parm;
            }
            process.StartInfo.UseShellExecute = false; // 是否使用外壳程序 
            //process.StartInfo.CreateNoWindow = true; //是否在新窗口中启动该进程的值 
            process.StartInfo.RedirectStandardError = true; // 重定向输入流 
            process.StartInfo.RedirectStandardOutput = true; // 重定向输入流 
            process.ErrorDataReceived += (s, _e) => LogTextManager.AddLog(_e.Data);
            process.OutputDataReceived += (s, _e) => LogTextManager.AddLog(_e.Data);
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            processList.Add(new ProcessInfoData { Process = process, ProcessId = process.Id, Dev = dev, Rect = new Rectangle(25, 60 + 95 * processList.Count, 70, 70) });
        }

        public static string StartProcessResult(string pname, string parm = "")
        {
            Process process = new Process();
            process.StartInfo.FileName = pname;
            if (!string.IsNullOrEmpty(parm))
            {
                process.StartInfo.Arguments = parm;
            }
            process.StartInfo.UseShellExecute = false; // 是否使用外壳程序 
            process.StartInfo.CreateNoWindow = true; //是否在新窗口中启动该进程的值 
            process.StartInfo.RedirectStandardOutput = true; // 重定向输入流 
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }

        public static void ClearProcess()
        {
            foreach (var process in processList)
            {
                try
                {
                    process.Process.Kill();
                }
                catch (Exception)
                {
                }
            }
            processList.Clear();
        }

        public static List<ProcessInfoData> GetAll()
        {
            return processList;
        }

        public static void SetProcessState(LogShowDevice dev, ServerRunState state)
        {
            foreach (var process in processList)
            {
                process.State = state;
            }
        }
    }
}