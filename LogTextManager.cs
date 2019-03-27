using System;
using System.Collections.Generic;
using System.Diagnostics;
using DGServerControllerGUI.Configuration;
using DGServerControllerGUI.Tools;

namespace DGServerControllerGUI
{
    public static class LogTextManager
    {
        public class LogItem
        {
            public int Id; //日志的编号
            public LogShowMode Type;
            public LogShowDevice Device;
            public string Text;
        }

        static readonly LinkedList<LogItem> items = new LinkedList<LogItem>();

        private static int logIndex = 10000;

        private static LogShowMode filterMode = LogShowMode.All;
        private static LogShowDevice filterDevice = LogShowDevice.All;
        private static string filterWords;

        public static AppendTextCallback AppendTextHandle;
        public static ResetTextCallback ResetTextHandle;
        public static RemoveTextCallback RemoveTextHandle;

        public static int LogCount { get { return items.Count; } }
        public static bool ShowLog { get; set; } = true;

        public static void SetMode(LogShowMode md)
        {
            filterMode = md;
            RefreshItems();
        }
        public static void SetSevice(LogShowDevice md)
        {
            filterDevice = md;
            RefreshItems();
        }

        private static void RefreshItems()
        {
            var newItems = new List<LogItem>(items);
            newItems.RemoveAll(lt => (lt.Type & filterMode) == 0 || (lt.Device & filterDevice) == 0);
            if (!string.IsNullOrEmpty(filterWords))
                newItems.RemoveAll(lt => !lt.Text.Contains(filterWords));
            
            ResetTextHandle(newItems);
        }

        public static void SetWords(string str)
        {
            filterWords = str;
            RefreshItems();
        }

        public static void AddLog(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            if (text[0] != '[' || text.Contains("[print_frame]"))
                return;

            LogShowMode type = LogShowMode.DEBUG;
            LogShowDevice dev = LogShowDevice.ROL;

            var datas = text.Replace("][", "]").Split(']');
            if (datas.Length > 2)
            {
                switch (datas[2])
                {
                    case "loginSrv": dev = LogShowDevice.LGI; break;
                    case "dbSrv": dev = LogShowDevice.DBS; break;
                }
            }
            if (datas.Length > 6)
            {
                switch (datas[6])
                {
                    case "WARNING": type = LogShowMode.WARN; break;
                    case "ERROR": type = LogShowMode.ERROR; break;
                    case "FATAL": type = LogShowMode.FATAL; break;
                    case "CRITICAL": type = LogShowMode.FATAL; break;
                }
            }

            var newLog = new LogItem {Id = logIndex++, Type = type, Text = text, Device = dev};
            items.AddLast(newLog);
            if (LogCount >= ConfigObject.Instance.MaxLogCount)
            {
                var firstId = items.First.Value.Id;
                items.RemoveFirst();
                RemoveTextHandle(firstId);
            }

            if (newLog.Text.Contains("DGServerNode.AllServerRunning"))
                ProcessHelper.SetProcessState(newLog.Device, ServerRunState.Running);
            else if (newLog.Text.Contains("DGServerNode.DoShutdown"))
                ProcessHelper.SetProcessState(newLog.Device, ServerRunState.Shutdown);

            if (!ShowLog)
                return;

            //if ((type & filterMode) == 0 || (dev & filterDevice) == 0)
            //    return;
            //if (!string.IsNullOrEmpty(filterWords) && !text.Contains(filterWords))
            //    return;
            AppendTextHandle(newLog.Id, text, type);
        }
    }
}